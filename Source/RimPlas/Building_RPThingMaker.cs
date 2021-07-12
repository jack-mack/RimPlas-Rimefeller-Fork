using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimPlas
{
    // Token: 0x02000020 RID: 32
    public class Building_RPThingMaker : Building
    {
        // Token: 0x0400004B RID: 75
        public static string UITexPath = "Things/Building/Misc/RPThingMaker/UI/";

        // Token: 0x0400004E RID: 78
        [NoTranslate] private readonly string debugTexPath = UITexPath + "RPThingMakerDebug_Icon";

        // Token: 0x04000050 RID: 80
        [NoTranslate] private readonly string EndLimitPath = "Limit_icon";

        // Token: 0x0400004F RID: 79
        [NoTranslate] private readonly string FrontLimitPath = UITexPath + "StockLimits/RPThingMakerStock";

        // Token: 0x0400004C RID: 76
        [NoTranslate] private readonly string produceTexPath = UITexPath + "RPThingMakerProduce_Icon";

        // Token: 0x0400004D RID: 77
        [NoTranslate] private readonly string thingTexPath = UITexPath + "RPThingMaker_ThingIcon";

        // Token: 0x04000049 RID: 73
        private List<IntVec3> cachedAdjCellsCardinal;

        // Token: 0x04000040 RID: 64
        public bool debug;

        // Token: 0x04000048 RID: 72
        public float effeciencyFactor = 0.95f;

        // Token: 0x04000045 RID: 69
        public bool isProducing;

        // Token: 0x04000042 RID: 66
        public ThingDef MakerThingDef;

        // Token: 0x0400004A RID: 74
        public Sustainer makeSustainer;

        // Token: 0x04000046 RID: 70
        public int NumProd;

        // Token: 0x04000041 RID: 65
        public CompPowerTrader powerComp;

        // Token: 0x04000043 RID: 67
        public int ProdWorkTicks;

        // Token: 0x04000047 RID: 71
        public int StockLimit;

        // Token: 0x04000044 RID: 68
        public int TotalProdWorkTicks;

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x0600007E RID: 126 RVA: 0x00004885 File Offset: 0x00002A85
        private List<IntVec3> AdjCellsCardinalInBounds
        {
            get
            {
                if (cachedAdjCellsCardinal == null)
                {
                    cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
                        where c.InBounds(Map)
                        select c).ToList();
                }

                return cachedAdjCellsCardinal;
            }
        }

        // Token: 0x0600007F RID: 127 RVA: 0x000048B8 File Offset: 0x00002AB8
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref MakerThingDef, "MakerThingDef");
            Scribe_Values.Look(ref isProducing, "isProducing");
            Scribe_Values.Look(ref NumProd, "NumProd");
            Scribe_Values.Look(ref ProdWorkTicks, "ProdWorkTicks");
            Scribe_Values.Look(ref TotalProdWorkTicks, "TotalProdWorkTicks");
            Scribe_Values.Look(ref StockLimit, "StockLimit");
        }

        // Token: 0x06000080 RID: 128 RVA: 0x00004935 File Offset: 0x00002B35
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            cachedAdjCellsCardinal = AdjCellsCardinalInBounds;
        }

        // Token: 0x06000081 RID: 129 RVA: 0x00004958 File Offset: 0x00002B58
        public void StartMakeSustainer()
        {
            var info = SoundInfo.InMap(this, MaintenanceType.PerTick);
            makeSustainer = SoundDef.Named("RPThingMaker").TrySpawnSustainer(info);
        }

        // Token: 0x06000082 RID: 130 RVA: 0x00004988 File Offset: 0x00002B88
        public override void Tick()
        {
            base.Tick();
            if (debug && Find.TickManager.TicksGame % 100 == 0)
            {
                var debugMsg = "At Tick: " + Find.TickManager.TicksGame;
                debugMsg = string.Concat(debugMsg, " : (", MakerThingDef != null ? MakerThingDef.defName : "Null",
                    ") : Prod: ", isProducing ? "True" : "false", " : Num: ", NumProd.ToString(), " : PWT: ",
                    ProdWorkTicks.ToString());
                Log.Message(debugMsg);
            }

            if (!IsWorking(this) || MakerThingDef == null ||
                StockLimitReached(this, MakerThingDef, StockLimit, out _))
            {
                return;
            }

            if (ProdWorkTicks > 0 && isProducing)
            {
                ProdWorkTicks--;
                if (makeSustainer == null)
                {
                    StartMakeSustainer();
                    return;
                }

                if (makeSustainer.Ended)
                {
                    StartMakeSustainer();
                    return;
                }

                makeSustainer.Maintain();
            }
            else if (isProducing && NumProd > 0 && MakerThingDef != null)
            {
                if (debug)
                {
                    Log.Message("Production point: " + MakerThingDef.defName + " : " + ProdWorkTicks);
                }

                if (ValidateOutput(MakerThingDef, out var hasSpace, out var candidatesOut) && hasSpace > 0)
                {
                    if (hasSpace >= NumProd)
                    {
                        if (debug)
                        {
                            Log.Message("Ejecting: " + MakerThingDef.defName + " : " + NumProd);
                        }

                        MakerEject(this, MakerThingDef, NumProd, candidatesOut, out var Surplus);
                        NumProd = Surplus;
                    }
                    else
                    {
                        if (debug)
                        {
                            Log.Message("Ejecting: " + MakerThingDef.defName + " : " + hasSpace);
                        }

                        MakerEject(this, MakerThingDef, hasSpace, candidatesOut, out var Surplus2);
                        NumProd -= hasSpace - Surplus2;
                    }
                }

                if (NumProd == 0)
                {
                    TotalProdWorkTicks = 0;
                }
            }
            else if (isProducing && MakerThingDef != null && ValidateRecipe(MakerThingDef, out var UseMax,
                out var RecipeList, out var minProd, out var maxProd, out var ticks))
            {
                if (debug)
                {
                    Log.Message(
                        string.Concat("StartProduction: ", MakerThingDef.defName, " :  RCP Items: ",
                            RecipeList.Count));
                }

                if (RecipeList.Count <= 0)
                {
                    return;
                }

                for (var i = 0; i < RecipeList.Count; i++)
                {
                    var recipeThingDef = RecipeList[i].def;
                    var num = UseMax ? RecipeList[i].Max : RecipeList[i].Min;

                    if (debug)
                    {
                        Log.Message(
                            string.Concat("Removing: ", UseMax ? "Max" : "Min", ": ", num.ToString(), " (",
                                recipeThingDef.defName, ")"));
                    }

                    RemoveRecipeItems(recipeThingDef, num);
                }

                NumProd = minProd;
                if (UseMax)
                {
                    NumProd = maxProd;
                }

                ProdWorkTicks = (int) (ticks * effeciencyFactor * NumProd);
                TotalProdWorkTicks = ProdWorkTicks;
            }
        }

        // Token: 0x06000083 RID: 131 RVA: 0x00004D85 File Offset: 0x00002F85

        // Token: 0x06000084 RID: 132 RVA: 0x00004D90 File Offset: 0x00002F90
        public void MakerEject(Building b, ThingDef t, int numProducts, List<Building> candidatesout, out int remaining)
        {
            remaining = numProducts;
            if (candidatesout.Count <= 0)
            {
                return;
            }

            for (var i = 0; i < candidatesout.Count; i++)
            {
                if (i == 0)
                {
                    var unused = candidatesout[i];
                }

                if (numProducts <= 0)
                {
                    continue;
                }

                var thingList = candidatesout[i].Position.GetThingList(candidatesout[i].Map);
                if (thingList.Count <= 0)
                {
                    continue;
                }

                var founditem = false;
                var blocked = false;
                foreach (var thing in thingList)
                {
                    if (thing.def == t)
                    {
                        founditem = true;
                        var canPlace = thing.def.stackLimit - thing.stackCount;
                        if (canPlace <= 0)
                        {
                            continue;
                        }

                        if (canPlace >= numProducts)
                        {
                            thing.stackCount += numProducts;
                            remaining -= numProducts;
                            numProducts = 0;
                        }
                        else
                        {
                            thing.stackCount += canPlace;
                            numProducts -= canPlace;
                            remaining -= canPlace;
                        }
                    }
                    else if (!(thing is Building))
                    {
                        blocked = true;
                    }
                }

                if (founditem || blocked)
                {
                    continue;
                }

                var tStackLimit = t.stackLimit;
                var newProduct = ThingMaker.MakeThing(t);
                if (!candidatesout[i].Position.IsValidStorageFor(candidatesout[i].Map, newProduct))
                {
                    continue;
                }

                if (tStackLimit >= numProducts)
                {
                    newProduct.stackCount = numProducts;
                    remaining -= numProducts;
                    numProducts = 0;
                }
                else
                {
                    newProduct.stackCount = tStackLimit;
                    numProducts -= tStackLimit;
                    remaining -= tStackLimit;
                }

                GenDrop.TryDropSpawn(newProduct, candidatesout[i].Position, candidatesout[i].Map,
                    ThingPlaceMode.Direct, out _);
            }
        }

        // Token: 0x06000085 RID: 133 RVA: 0x00004F5C File Offset: 0x0000315C
        public void RemoveRecipeItems(ThingDef t, int numToRemove)
        {
            var AdjCells = AdjCellsCardinalInBounds;
            if (AdjCells.Count <= 0)
            {
                return;
            }

            var TotalRemoved = 0;
            for (var i = 0; i < AdjCells.Count; i++)
            {
                if (numToRemove <= 0)
                {
                    continue;
                }

                var isInputCell = false;
                var has = 0;
                var candidates = new List<Thing>();
                var thingList = AdjCells[i].GetThingList(Map);
                if (thingList.Count > 0)
                {
                    foreach (var thing in thingList)
                    {
                        if (thing.def == t)
                        {
                            has += thing.stackCount;
                            candidates.Add(thing);
                        }

                        if (thing.def != null && thing is Building && thing.def.defName == "RPThingMakerInput")
                        {
                            isInputCell = true;
                        }
                    }
                }

                if (!isInputCell || has <= 0 || candidates.Count <= 0)
                {
                    continue;
                }

                foreach (var thing in candidates)
                {
                    if (thing.def != t)
                    {
                        continue;
                    }

                    if (numToRemove - thing.stackCount >= 0)
                    {
                        numToRemove -= thing.stackCount;
                        TotalRemoved += thing.stackCount;
                        thing.Destroy();
                    }
                    else
                    {
                        thing.stackCount -= numToRemove;
                        TotalRemoved += numToRemove;
                        numToRemove = 0;
                    }
                }
            }

            if (debug)
            {
                Log.Message("Total Removed: (" + t.defName + ") = " + TotalRemoved);
            }
        }

        // Token: 0x06000086 RID: 134 RVA: 0x00005114 File Offset: 0x00003314
        public bool ValidateOutput(ThingDef t, out int hasSpace, out List<Building> candidatesOut)
        {
            hasSpace = 0;
            candidatesOut = new List<Building>();
            var AdjCells = AdjCellsCardinalInBounds;
            if (AdjCells.Count > 0)
            {
                for (var i = 0; i < AdjCells.Count; i++)
                {
                    var isOutputCell = false;
                    var has = 0;
                    var thingList = AdjCells[i].GetThingList(Map);
                    if (thingList.Count > 0)
                    {
                        foreach (var thing in thingList)
                        {
                            if (thing.def == t)
                            {
                                has += thing.stackCount;
                            }

                            if (thing is not Building || thing.def.defName != "RPThingMakerOutput")
                            {
                                continue;
                            }

                            isOutputCell = true;
                            hasSpace += t.stackLimit;
                            candidatesOut.Add(thing as Building);
                        }
                    }

                    if (isOutputCell)
                    {
                        hasSpace -= has;
                    }
                }
            }

            if (debug)
            {
                Log.Message(hasSpace + " item space on " + candidatesOut.Count + " points");
            }

            return hasSpace > 0;
        }

        // Token: 0x06000087 RID: 135 RVA: 0x00005260 File Offset: 0x00003460
        public bool ValidateRecipe(ThingDef t, out bool CanUseMax, out List<RCPItemCanUse> FinalList, out int MinProd,
            out int MaxProd, out int Ticks)
        {
            CanUseMax = true;
            FinalList = null;
            MinProd = 0;
            MaxProd = 0;
            Ticks = 0;
            if (debug && Find.TickManager.TicksGame % 100 == 0)
            {
                Log.Message("ValRep: " + t.defName);
            }

            if (!RPThingMakerUtility.RCPProdValues(t, out var ticks, out var minProd, out var maxProd, out var Res))
            {
                return false;
            }

            Ticks = ticks;
            MinProd = minProd;
            MaxProd = maxProd;
            if (debug)
            {
                Log.Message(
                    string.Concat("RCPVals: Ticks: ", ticks.ToString(), " minProd: ", minProd.ToString(), " maxProd: ",
                        maxProd.ToString(), " Res: ", Res));
            }

            if (!ResearchProjectDef.Named(Res).IsFinished || minProd <= 0 || maxProd <= 0 || ticks <= 0)
            {
                if (!ResearchProjectDef.Named(Res).IsFinished)
                {
                    Log.Message("RPThingMaker.ErrorRes".Translate(MakerThingDef.label));
                    isProducing = false;
                    NumProd = 0;
                    ProdWorkTicks = 0;
                    TotalProdWorkTicks = 0;
                }
                else
                {
                    Log.Message(
                        "RPThingMaker.ErrorRCP".Translate(MakerThingDef.label, ticks.ToString(), minProd.ToString(),
                            maxProd.ToString()));
                    isProducing = false;
                    NumProd = 0;
                    ProdWorkTicks = 0;
                    TotalProdWorkTicks = 0;
                }

                return false;
            }

            var listRCP = RPThingMakerUtility.GetRCPList(t);
            if (listRCP.Count <= 0)
            {
                if (debug)
                {
                    Log.Message("RCP is False.");
                }

                return false;
            }

            if (debug)
            {
                Log.Message("RCP Listings: " + listRCP.Count);
            }

            var RCPListPotentials = new List<RCPItemCanUse>();
            var RCPGroups = new List<int>();
            foreach (var rprcpListItem in listRCP)
            {
                var MaterialsMin = 0;
                var MaterialsMax = 0;
                var RCPItem = rprcpListItem;
                var RCPMinNumNeeded = (int) Math.Round(RCPItem.num * minProd * RCPItem.ratio);
                var RCPMaxNumNeeded = (int) Math.Round(RCPItem.num * maxProd * RCPItem.ratio);
                if (HasEnoughMaterialInHoppers(RCPItem.def, RCPMinNumNeeded, true))
                {
                    MaterialsMin = RCPMinNumNeeded;
                }

                if (HasEnoughMaterialInHoppers(RCPItem.def, RCPMaxNumNeeded, false))
                {
                    MaterialsMax = RCPMaxNumNeeded;
                }

                if (MaterialsMin > 0 || MaterialsMax > 0)
                {
                    RCPListPotentials.Add(new RCPItemCanUse
                    {
                        def = RCPItem.def,
                        Min = MaterialsMin,
                        Max = MaterialsMax,
                        Grp = RCPItem.mixgrp
                    });
                }

                if (!RCPGroups.Contains(RCPItem.mixgrp))
                {
                    RCPGroups.Add(RCPItem.mixgrp);
                }
            }

            if (debug)
            {
                Log.Message(
                    "InnerRecipe List: Groups: " + RCPGroups.Count + " , Potentials: " + RCPListPotentials.Count);
            }

            FinalList = new List<RCPItemCanUse>();
            var NotAllGroups = false;
            if (RCPGroups.Count > 0)
            {
                foreach (var grp in RCPGroups)
                {
                    var foundGroup = false;
                    if (RCPListPotentials.Count > 0)
                    {
                        var bestthingsofar = default(RCPItemCanUse);
                        var best = false;
                        var bestmax = false;
                        foreach (var itemchk in RCPListPotentials)
                        {
                            if (itemchk.Grp != grp)
                            {
                                continue;
                            }

                            foundGroup = true;
                            if (itemchk.Min <= 0)
                            {
                                continue;
                            }

                            if (itemchk.Max > 0)
                            {
                                if (bestmax)
                                {
                                    continue;
                                }

                                bestthingsofar.def = itemchk.def;
                                bestthingsofar.Min = itemchk.Min;
                                bestthingsofar.Max = itemchk.Max;
                                bestthingsofar.Grp = itemchk.Grp;
                                best = true;
                                bestmax = true;
                            }
                            else if (!best)
                            {
                                bestthingsofar.def = itemchk.def;
                                bestthingsofar.Min = itemchk.Min;
                                bestthingsofar.Max = itemchk.Max;
                                bestthingsofar.Grp = itemchk.Grp;
                                best = true;
                            }
                        }

                        if (!bestmax)
                        {
                            bestthingsofar.Max = 0;
                        }

                        FinalList.Add(bestthingsofar);
                    }

                    if (foundGroup)
                    {
                        continue;
                    }

                    NotAllGroups = true;
                    DoNotFoundGroupsOverlay(this, t, grp);
                }
            }

            if (FinalList.Count > 0)
            {
                for (var l = 0; l < FinalList.Count; l++)
                {
                    if (FinalList[l].Max == 0)
                    {
                        CanUseMax = false;
                    }
                }
            }

            if (NotAllGroups)
            {
                if (debug)
                {
                    Log.Message("RCP is False. Not all inputs found");
                }

                return false;
            }

            if (debug)
            {
                Log.Message("RCP is True. with (" + FinalList.Count + ") final list items");
            }

            return true;
        }

        // Token: 0x06000088 RID: 136 RVA: 0x00005768 File Offset: 0x00003968
        public static void DoNotFoundGroupsOverlay(Building_RPThingMaker b, ThingDef def, int grp)
        {
            if (Find.CurrentMap == null || Find.CurrentMap != b.Map)
            {
                return;
            }

            var listRCP = RPThingMakerUtility.GetRCPList(def);
            var alerts = new List<ThingDef>();
            if (listRCP.Count > 0)
            {
                foreach (var item in listRCP)
                {
                    if (item.mixgrp == grp)
                    {
                        alerts.AddDistinct(item.def);
                    }
                }
            }

            if (alerts.Count <= 0)
            {
                return;
            }

            var OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
            var i = 0;
            foreach (var alert in alerts)
            {
                if (!alert.defName.StartsWith("Chunk") || alert.defName.StartsWith("Chunk") && i < 1)
                {
                    var mat = MaterialPool.MatFrom(alert.uiIcon, ShaderDatabase.MetaOverlay, Color.white);
                    var BaseAlt = AltitudeLayer.WorldClipper.AltitudeFor();
                    if (mat != null)
                    {
                        var altInd = 21;
                        var plane = MeshPool.plane08;
                        var drawPos = b.TrueCenter();
                        drawPos.y = BaseAlt + (0.046875f * altInd);
                        drawPos.x += i;
                        drawPos.z += grp - 2;
                        var num2 = ((float) Math.Sin(
                            (Time.realtimeSinceStartup + (397f * (b.thingIDNumber % 571))) * 4f) + 1f) * 0.5f;
                        num2 = 0.3f + (num2 * 0.7f);
                        for (var j = 0; j < 2; j++)
                        {
                            var material = FadedMaterialPool.FadedVersionOf(j < 1 ? mat : OutOfFuelMat, num2);

                            if (material != null)
                            {
                                Graphics.DrawMesh(plane, drawPos, Quaternion.identity, material, 0);
                            }
                        }
                    }
                }

                i++;
            }
        }

        // Token: 0x06000089 RID: 137 RVA: 0x000059A4 File Offset: 0x00003BA4
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction != Faction.OfPlayer)
            {
                yield break;
            }

            string SelectDesc = "RPThingMaker.ThingSelectDesc".Translate();
            if (MakerThingDef == null)
            {
                string NoChem = "RPThingMaker.ThingSelect".Translate();
                yield return new Command_Action
                {
                    defaultLabel = NoChem,
                    icon = ContentFinder<Texture2D>.Get(thingTexPath),
                    defaultDesc = SelectDesc,
                    action = RPMakerSelectThing
                };
            }
            else
            {
                var IconToUse = RPThingMakerUtility.GetRPThingIcon(MakerThingDef);
                var LabelDetail = MakerThingDef.label.CapitalizeFirst();
                LabelDetail = string.Concat(LabelDetail, " [", NumProd, "] ");
                if (TotalProdWorkTicks > 0)
                {
                    LabelDetail = LabelDetail + " (" +
                                  (int) ((TotalProdWorkTicks - ProdWorkTicks) / (float) TotalProdWorkTicks * 100f) +
                                  "%)";
                }

                yield return new Command_Action
                {
                    defaultLabel = LabelDetail,
                    icon = IconToUse,
                    defaultDesc = SelectDesc,
                    action = RPMakerSelectThing
                };
            }

            string LabelProduce = "RPThingMaker.Production".Translate();
            string LabelProduceDesc = "RPThingMaker.ProductionDesc".Translate();
            if (isProducing)
            {
                if (MakerThingDef != null)
                {
                    if (RPThingMakerUtility.RCPProdValues(MakerThingDef, out _, out var minProd, out var maxProd,
                        out _))
                    {
                        LabelProduce +=
                            "RPThingMaker.ProdLabelRange".Translate(minProd.ToString(), maxProd.ToString());
                    }
                    else
                    {
                        LabelProduce += "RPThingMaker.ProdLabelERR".Translate();
                    }
                }
                else
                {
                    LabelProduce += "RPThingMaker.ProdNoThing".Translate();
                }
            }
            else
            {
                LabelProduce += "RPThingMaker.ProdStopped".Translate();
            }

            yield return new Command_Toggle
            {
                icon = ContentFinder<Texture2D>.Get(produceTexPath),
                defaultLabel = LabelProduce,
                defaultDesc = LabelProduceDesc,
                isActive = () => isProducing,
                toggleAction = delegate { ToggleProducing(isProducing); }
            };
            var LimitTexPath = FrontLimitPath;
            string LimitLabelDetail;
            if (StockLimit > 0)
            {
                StockLimitReached(this, MakerThingDef, StockLimit, out var ActualStockNum);
                var LimitPct = ActualStockNum * 100 / StockLimit;
                LimitLabelDetail = "RPThingMaker.StockLabel".Translate(StockLimit.ToString(), LimitPct.ToString());
                LimitTexPath += StockLimit.ToString();
            }
            else
            {
                LimitLabelDetail = "RPThingMaker.StockLabelNL".Translate();
                LimitTexPath += "No";
            }

            LimitTexPath += EndLimitPath;
            var LimitIconToUse = ContentFinder<Texture2D>.Get(LimitTexPath);
            string SelectLimit = "RPThingMaker.SelectStockLimit".Translate();
            yield return new Command_Action
            {
                defaultLabel = LimitLabelDetail,
                icon = LimitIconToUse,
                defaultDesc = SelectLimit,
                action = RPMakerSelectLimit
            };
            if (Prefs.DevMode)
            {
                yield return new Command_Toggle
                {
                    icon = ContentFinder<Texture2D>.Get(debugTexPath),
                    defaultLabel = "Debug Mode",
                    defaultDesc = "Send debug messages to Log",
                    isActive = () => debug,
                    toggleAction = delegate { ToggleDebug(debug); }
                };
            }
        }

        // Token: 0x0600008A RID: 138 RVA: 0x000059B4 File Offset: 0x00003BB4
        public void ToggleDebug(bool flag)
        {
            debug = !flag;
        }

        // Token: 0x0600008B RID: 139 RVA: 0x000059C0 File Offset: 0x00003BC0
        public void ToggleProducing(bool flag)
        {
            isProducing = !flag;
        }

        // Token: 0x0600008C RID: 140 RVA: 0x000059CC File Offset: 0x00003BCC
        public void RPMakerSelectLimit()
        {
            var list = new List<FloatMenuOption>();
            var Choices = RPThingMakerUtility.GetMaxStock();
            if (Choices.Count > 0)
            {
                foreach (var i in Choices)
                {
                    string text;
                    if (i > 0)
                    {
                        text = i.ToString();
                    }
                    else
                    {
                        text = "RPThingMaker.StockNoLimit".Translate();
                    }

                    var value = i;
                    list.Add(new FloatMenuOption(text, delegate { SetStockLimits(value); }, MenuOptionPriority.Default,
                        null, null, 29f));
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        // Token: 0x0600008D RID: 141 RVA: 0x00005A7C File Offset: 0x00003C7C
        public void RPMakerSelectThing()
        {
            var list = new List<FloatMenuOption>();
            string text = "RPThingMaker.SelNoThing".Translate();
            list.Add(new FloatMenuOption(text, delegate { SetProdControlValues(null, false, 0, 0); },
                MenuOptionPriority.Default, null, null, 29f));
            var Choices = RPThingMakerUtility.GetMakeList();
            if (Choices.Count > 0)
            {
                foreach (var defName in Choices)
                {
                    var ChoiceDef = DefDatabase<ThingDef>.GetNamed(defName);
                    text = ChoiceDef.label.CapitalizeFirst();
                    if (IsThingAvailable(ChoiceDef))
                    {
                        list.Add(new FloatMenuOption(text, delegate { SetProdControlValues(ChoiceDef, true, 0, 0); },
                            MenuOptionPriority.Default, null, null, 29f,
                            rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f),
                                ChoiceDef)));
                    }
                }
            }

            Find.WindowStack.Add(new FloatMenu(list));
        }

        // Token: 0x0600008E RID: 142 RVA: 0x00005B66 File Offset: 0x00003D66
        public void SetStockLimits(int aStockLim)
        {
            StockLimit = aStockLim;
        }

        // Token: 0x0600008F RID: 143 RVA: 0x00005B70 File Offset: 0x00003D70
        public void SetProdControlValues(ThingDef tdef, bool prod, int num, int ticks)
        {
            if (tdef == null)
            {
                MakerThingDef = null;
                isProducing = false;
                NumProd = 0;
                ProdWorkTicks = 0;
                TotalProdWorkTicks = 0;
                return;
            }

            if (MakerThingDef == tdef)
            {
                return;
            }

            MakerThingDef = tdef;
            NumProd = 0;
            ProdWorkTicks = 0;
            TotalProdWorkTicks = 0;
        }

        // Token: 0x06000090 RID: 144 RVA: 0x00005BC9 File Offset: 0x00003DC9
        public bool IsWorking(Building b)
        {
            return !b.IsBrokenDown() && powerComp.PowerOn;
        }

        // Token: 0x06000091 RID: 145 RVA: 0x00005BE8 File Offset: 0x00003DE8
        public static bool IsThingAvailable(ThingDef chkDef)
        {
            return RPThingMakerUtility.RCPProdValues(chkDef, out _, out _, out _, out var research) &&
                   research != "" && DefDatabase<ResearchProjectDef>.GetNamed(research, false).IsFinished;
        }

        // Token: 0x06000092 RID: 146 RVA: 0x00005C24 File Offset: 0x00003E24
        public static bool StockLimitReached(Building b, ThingDef stockThing, int stockLim, out int ActualStockNum)
        {
            ActualStockNum = 0;
            if (stockLim <= 0 || stockThing == null)
            {
                return false;
            }

            var StockListing = b.Map.listerThings.ThingsOfDef(stockThing);
            if (StockListing.Count > 0)
            {
                foreach (var thing in StockListing)
                {
                    ActualStockNum += thing.stackCount;
                }
            }

            if (ActualStockNum >= stockLim)
            {
                return true;
            }

            return false;
        }

        // Token: 0x06000093 RID: 147 RVA: 0x00005C80 File Offset: 0x00003E80
        public virtual bool HasEnoughMaterialInHoppers(ThingDef NeededThing, int required, bool isMin)
        {
            var num = 0;
            foreach (var c in AdjCellsCardinalInBounds)
            {
                Thing thingNeed = null;
                Thing thingHopper = null;
                var thingList = c.GetThingList(Map);
                foreach (var thing3 in thingList)
                {
                    if (thing3.def == NeededThing)
                    {
                        thingNeed = thing3;
                    }

                    if (thing3.def.defName == "RPThingMakerInput")
                    {
                        thingHopper = thing3;
                    }
                }

                if (thingNeed != null && thingHopper != null)
                {
                    num += thingNeed.stackCount;
                }
            }

            if (debug)
            {
                Log.Message(
                    string.Concat("Enough Materials? (", num >= required ? "Yes" : "No", "): (", NeededThing.defName,
                        ") Found:", num.ToString(), " for ", required.ToString(), " required as ",
                        isMin ? "Min" : "Max"));
            }

            return num >= required;
        }

        // Token: 0x0200002A RID: 42
        public struct RCPItemCanUse
        {
            // Token: 0x04000079 RID: 121
            public ThingDef def;

            // Token: 0x0400007A RID: 122
            public int Min;

            // Token: 0x0400007B RID: 123
            public int Max;

            // Token: 0x0400007C RID: 124
            public int Grp;
        }
    }
}