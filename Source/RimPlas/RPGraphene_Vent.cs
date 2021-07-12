using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x0200001D RID: 29
    public class RPGraphene_Vent : Building_TempControl
    {
        // Token: 0x0400003F RID: 63
        [NoTranslate]
        private readonly string commandTexture = "Things/Building/Temperature/RPGraphene_Vent/UI/RPGraphene_Vent";

        // Token: 0x0400003C RID: 60
        private CompFlickable flickableComp;

        // Token: 0x0400003D RID: 61
        private bool InRangeATM = true;

        // Token: 0x0400003E RID: 62
        private bool UseFixed = true;

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000069 RID: 105 RVA: 0x000042DF File Offset: 0x000024DF
        public override Graphic Graphic => flickableComp.CurrentGraphic;

        // Token: 0x0600006A RID: 106 RVA: 0x000042EC File Offset: 0x000024EC
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            flickableComp = GetComp<CompFlickable>();
        }

        // Token: 0x0600006B RID: 107 RVA: 0x00004302 File Offset: 0x00002502
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref UseFixed, "UseFixed", true);
        }

        // Token: 0x0600006C RID: 108 RVA: 0x0000431C File Offset: 0x0000251C
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (Faction == Faction.OfPlayer)
            {
                yield return new Command_Toggle
                {
                    icon = ContentFinder<Texture2D>.Get(commandTexture),
                    defaultLabel = "RimPlas.Fixed".Translate(),
                    defaultDesc =
                        "RimPlas.FixedDesc".Translate(Controller.Settings.GVentMin, Controller.Settings.GVentMax),
                    isActive = () => UseFixed,
                    toggleAction = delegate { ToggleUseFixed(UseFixed); }
                };
            }
        }

        // Token: 0x0600006D RID: 109 RVA: 0x0000432C File Offset: 0x0000252C
        public void ToggleUseFixed(bool flag)
        {
            UseFixed = !flag;
        }

        // Token: 0x0600006E RID: 110 RVA: 0x00004338 File Offset: 0x00002538
        public override void TickRare()
        {
            var tempRange = default(FloatRange);
            GetSafeTemps(this, UseFixed, out var tempMin, out var tempMax);
            tempRange.min = tempMin;
            tempRange.max = tempMax;
            if (UseFixed && RPGVentCheckTemps(this, tempRange) || !UseFixed)
            {
                if (!InRangeATM && VentIsWorking(this))
                {
                    InRangeATM = !InRangeATM;
                    if (!flickableComp.SwitchIsOn)
                    {
                        flickableComp.DoFlick();
                    }
                }

                if (!FlickUtility.WantsToBeOn(this))
                {
                    return;
                }

                if (VentIsWorking(this))
                {
                    GenTemperature.EqualizeTemperaturesThroughBuilding(this, 20f, true);
                    return;
                }

                GenTemperature.EqualizeTemperaturesThroughBuilding(this, 14f, true);
            }
            else
            {
                InRangeATM = false;
                if (VentIsWorking(this) && flickableComp.SwitchIsOn)
                {
                    flickableComp.DoFlick();
                }
            }
        }

        // Token: 0x0600006F RID: 111 RVA: 0x00004414 File Offset: 0x00002614
        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (FlickUtility.WantsToBeOn(this))
            {
                return stringBuilder.ToString();
            }

            if (stringBuilder.Length > 0)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("VentClosed".Translate());

            return stringBuilder.ToString();
        }

        // Token: 0x06000070 RID: 112 RVA: 0x00004468 File Offset: 0x00002668
        internal bool VentIsWorking(Building b)
        {
            return !b.IsBrokenDown();
        }

        // Token: 0x06000071 RID: 113 RVA: 0x00004475 File Offset: 0x00002675
        internal void GetSafeTemps(Building b, bool Fixed, out float min, out float max)
        {
            min = -9999f;
            max = 9999f;
            if (!Fixed)
            {
                return;
            }

            min = Controller.Settings.GVentMin;
            max = Controller.Settings.GVentMax;
        }

        // Token: 0x06000072 RID: 114 RVA: 0x000044A4 File Offset: 0x000026A4
        internal bool RPGVentCheckTemps(Building b, FloatRange tempRange)
        {
            for (var i = 0; i < 2; i++)
            {
                var intVec = i != 0 ? b.Position - b.Rotation.FacingCell : b.Position + b.Rotation.FacingCell;
                if (!intVec.InBounds(b.Map))
                {
                    continue;
                }

                var roomGroup = intVec.GetRoomOrAdjacent(b.Map);
                if (roomGroup == null)
                {
                    continue;
                }

                var tempChk = roomGroup.Temperature;
                if (!tempRange.Includes(tempChk))
                {
                    return false;
                }
            }

            return true;
        }
    }
}