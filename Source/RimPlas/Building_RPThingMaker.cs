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
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600007E RID: 126 RVA: 0x00004885 File Offset: 0x00002A85
		private List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (this.cachedAdjCellsCardinal == null)
				{
					this.cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
					where c.InBounds(base.Map)
					select c).ToList<IntVec3>();
				}
				return this.cachedAdjCellsCardinal;
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000048B8 File Offset: 0x00002AB8
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look<ThingDef>(ref this.MakerThingDef, "MakerThingDef");
			Scribe_Values.Look<bool>(ref this.isProducing, "isProducing", false, false);
			Scribe_Values.Look<int>(ref this.NumProd, "NumProd", 0, false);
			Scribe_Values.Look<int>(ref this.ProdWorkTicks, "ProdWorkTicks", 0, false);
			Scribe_Values.Look<int>(ref this.TotalProdWorkTicks, "TotalProdWorkTicks", 0, false);
			Scribe_Values.Look<int>(ref this.StockLimit, "StockLimit", 0, false);
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00004935 File Offset: 0x00002B35
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader>();
			this.cachedAdjCellsCardinal = this.AdjCellsCardinalInBounds;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00004958 File Offset: 0x00002B58
		public void StartMakeSustainer()
		{
			SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
			this.makeSustainer = SoundDef.Named("RPThingMaker").TrySpawnSustainer(info);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00004988 File Offset: 0x00002B88
		public override void Tick()
		{
			base.Tick();
			if (this.debug && Find.TickManager.TicksGame % 100 == 0)
			{
				string debugMsg = "At Tick: " + Find.TickManager.TicksGame;
				debugMsg = string.Concat(new string[]
				{
					debugMsg,
					" : (",
					(this.MakerThingDef != null) ? this.MakerThingDef.defName : "Null",
					") : Prod: ",
					this.isProducing ? "True" : "false",
					" : Num: ",
					this.NumProd.ToString(),
					" : PWT: ",
					this.ProdWorkTicks.ToString()
				});
				Log.Message(debugMsg, false);
			}
			int StockNumbers;
			if (this.IsWorking(this) && this.MakerThingDef != null && !Building_RPThingMaker.StockLimitReached(this, this.MakerThingDef, this.StockLimit, out StockNumbers))
			{
				bool UseMax;
				List<Building_RPThingMaker.RCPItemCanUse> RecipeList;
				int minProd;
				int maxProd;
				int ticks;
				if (this.ProdWorkTicks > 0 && this.isProducing)
				{
					this.ProdWorkTicks--;
					if (this.makeSustainer == null)
					{
						this.StartMakeSustainer();
						return;
					}
					if (this.makeSustainer.Ended)
					{
						this.StartMakeSustainer();
						return;
					}
					this.makeSustainer.Maintain();
					return;
				}
				else if (this.isProducing && this.NumProd > 0 && this.MakerThingDef != null)
				{
					if (this.debug)
					{
						Log.Message("Production point: " + this.MakerThingDef.defName + " : " + this.ProdWorkTicks.ToString(), false);
					}
					int hasSpace;
					List<Building> candidatesOut;
					if (this.ValidateOutput(this.MakerThingDef, out hasSpace, out candidatesOut) && hasSpace > 0)
					{
						if (hasSpace >= this.NumProd)
						{
							if (this.debug)
							{
								Log.Message("Ejecting: " + this.MakerThingDef.defName + " : " + this.NumProd.ToString(), false);
							}
							int Surplus;
							this.MakerEject(this, this.MakerThingDef, this.NumProd, candidatesOut, out Surplus);
							this.NumProd = Surplus;
						}
						else
						{
							if (this.debug)
							{
								Log.Message("Ejecting: " + this.MakerThingDef.defName + " : " + hasSpace.ToString(), false);
							}
							int Surplus2;
							this.MakerEject(this, this.MakerThingDef, hasSpace, candidatesOut, out Surplus2);
							this.NumProd -= hasSpace - Surplus2;
						}
					}
					if (this.NumProd == 0)
					{
						this.TotalProdWorkTicks = 0;
						return;
					}
				}
				else if (this.isProducing && this.MakerThingDef != null && this.ValidateRecipe(this.MakerThingDef, out UseMax, out RecipeList, out minProd, out maxProd, out ticks))
				{
					if (this.debug)
					{
						Log.Message(string.Concat(new object[]
						{
							"StartProduction: ",
							this.MakerThingDef.defName,
							" :  RCP Items: ",
							RecipeList.Count
						}), false);
					}
					if (RecipeList.Count > 0)
					{
						for (int i = 0; i < RecipeList.Count; i++)
						{
							ThingDef recipeThingDef = RecipeList[i].def;
							int num;
							if (UseMax)
							{
								num = RecipeList[i].Max;
							}
							else
							{
								num = RecipeList[i].Min;
							}
							if (this.debug)
							{
								Log.Message(string.Concat(new string[]
								{
									"Removing: ",
									UseMax ? "Max" : "Min",
									": ",
									num.ToString(),
									" (",
									recipeThingDef.defName,
									")"
								}), false);
							}
							this.RemoveRecipeItems(recipeThingDef, num);
						}
						this.NumProd = minProd;
						if (UseMax)
						{
							this.NumProd = maxProd;
						}
						this.ProdWorkTicks = (int)((float)ticks * this.effeciencyFactor * (float)this.NumProd);
						this.TotalProdWorkTicks = this.ProdWorkTicks;
					}
				}
			}
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00004D85 File Offset: 0x00002F85
		public override void TickRare()
		{
			base.TickRare();
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00004D90 File Offset: 0x00002F90
		public void MakerEject(Building b, ThingDef t, int numProducts, List<Building> candidatesout, out int remaining)
		{
			remaining = numProducts;
			if (candidatesout.Count > 0)
			{
				for (int i = 0; i < candidatesout.Count; i++)
				{
					if (i == 0)
					{
						Building building = candidatesout[i];
					}
					if (numProducts > 0)
					{
						List<Thing> thingList = candidatesout[i].Position.GetThingList(candidatesout[i].Map);
						if (thingList.Count > 0)
						{
							bool founditem = false;
							bool blocked = false;
							for (int j = 0; j < thingList.Count; j++)
							{
								if (thingList[j].def == t)
								{
									founditem = true;
									int canPlace = thingList[j].def.stackLimit - thingList[j].stackCount;
									if (canPlace > 0)
									{
										if (canPlace >= numProducts)
										{
											thingList[j].stackCount += numProducts;
											remaining -= numProducts;
											numProducts = 0;
										}
										else
										{
											thingList[j].stackCount += canPlace;
											numProducts -= canPlace;
											remaining -= canPlace;
										}
									}
								}
								else if (thingList[j] != null && !(thingList[j] is Building))
								{
									blocked = true;
								}
							}
							if (!founditem && !blocked)
							{
								int canPlace = t.stackLimit;
								Thing newProduct = ThingMaker.MakeThing(t, null);
								if (candidatesout[i].Position.IsValidStorageFor(candidatesout[i].Map, newProduct))
								{
									if (canPlace >= numProducts)
									{
										newProduct.stackCount = numProducts;
										remaining -= numProducts;
										numProducts = 0;
									}
									else
									{
										newProduct.stackCount = canPlace;
										numProducts -= canPlace;
										remaining -= canPlace;
									}
									Thing newProductThing;
									GenDrop.TryDropSpawn(newProduct, candidatesout[i].Position, candidatesout[i].Map, ThingPlaceMode.Direct, out newProductThing, null, null);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00004F5C File Offset: 0x0000315C
		public void RemoveRecipeItems(ThingDef t, int numToRemove)
		{
			List<IntVec3> AdjCells = this.AdjCellsCardinalInBounds;
			if (AdjCells.Count > 0)
			{
				int TotalRemoved = 0;
				for (int i = 0; i < AdjCells.Count; i++)
				{
					if (numToRemove > 0)
					{
						bool isInputCell = false;
						int has = 0;
						List<Thing> candidates = new List<Thing>();
						List<Thing> thingList = AdjCells[i].GetThingList(base.Map);
						if (thingList.Count > 0)
						{
							for (int j = 0; j < thingList.Count; j++)
							{
								if (thingList[j].def == t)
								{
									has += thingList[j].stackCount;
									candidates.Add(thingList[j]);
								}
								if (thingList[j] is Building && thingList[j].def.defName == "RPThingMakerInput")
								{
									isInputCell = true;
								}
							}
						}
						if (isInputCell && has > 0 && candidates.Count > 0)
						{
							for (int k = 0; k < candidates.Count; k++)
							{
								if (candidates[k].def == t)
								{
									if (numToRemove - candidates[k].stackCount >= 0)
									{
										numToRemove -= candidates[k].stackCount;
										TotalRemoved += candidates[k].stackCount;
										candidates[k].Destroy(DestroyMode.Vanish);
									}
									else
									{
										candidates[k].stackCount -= numToRemove;
										TotalRemoved += numToRemove;
										numToRemove = 0;
									}
								}
							}
						}
					}
				}
				if (this.debug)
				{
					Log.Message("Total Removed: (" + t.defName + ") = " + TotalRemoved.ToString(), false);
				}
			}
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00005114 File Offset: 0x00003314
		public bool ValidateOutput(ThingDef t, out int hasSpace, out List<Building> candidatesOut)
		{
			hasSpace = 0;
			int numSpaces = 0;
			candidatesOut = new List<Building>();
			int hasProduct = 0;
			List<IntVec3> AdjCells = this.AdjCellsCardinalInBounds;
			if (AdjCells.Count > 0)
			{
				for (int i = 0; i < AdjCells.Count; i++)
				{
					bool isOutputCell = false;
					int has = 0;
					List<Thing> thingList = AdjCells[i].GetThingList(base.Map);
					if (thingList.Count > 0)
					{
						for (int j = 0; j < thingList.Count; j++)
						{
							if (thingList[j].def == t)
							{
								has += thingList[j].stackCount;
							}
							if (thingList[j] is Building && thingList[j].def.defName == "RPThingMakerOutput")
							{
								isOutputCell = true;
								numSpaces++;
								hasSpace += t.stackLimit;
								candidatesOut.Add(thingList[j] as Building);
							}
						}
					}
					if (isOutputCell)
					{
						hasProduct += has;
						hasSpace -= has;
					}
				}
			}
			if (this.debug)
			{
				Log.Message(hasSpace.ToString() + " item space on " + candidatesOut.Count.ToString() + " points", false);
			}
			return hasSpace > 0;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00005260 File Offset: 0x00003460
		public bool ValidateRecipe(ThingDef t, out bool CanUseMax, out List<Building_RPThingMaker.RCPItemCanUse> FinalList, out int MinProd, out int MaxProd, out int Ticks)
		{
			CanUseMax = true;
			FinalList = null;
			MinProd = 0;
			MaxProd = 0;
			Ticks = 0;
			if (this.debug && Find.TickManager.TicksGame % 100 == 0)
			{
				Log.Message("ValRep: " + t.defName, false);
			}
			int ticks;
			int minProd;
			int maxProd;
			string Res;
			if (!RPThingMakerUtility.RCPProdValues(t, out ticks, out minProd, out maxProd, out Res))
			{
				return false;
			}
			Ticks = ticks;
			MinProd = minProd;
			MaxProd = maxProd;
			if (this.debug)
			{
				Log.Message(string.Concat(new string[]
				{
					"RCPVals: Ticks: ",
					ticks.ToString(),
					" minProd: ",
					minProd.ToString(),
					" maxProd: ",
					maxProd.ToString(),
					" Res: ",
					Res
				}), false);
			}
			if (!ResearchProjectDef.Named(Res).IsFinished || minProd <= 0 || maxProd <= 0 || ticks <= 0)
			{
				if (!ResearchProjectDef.Named(Res).IsFinished)
				{
					Log.Message("RPThingMaker.ErrorRes".Translate(this.MakerThingDef.label), false);
					this.isProducing = false;
					this.NumProd = 0;
					this.ProdWorkTicks = 0;
					this.TotalProdWorkTicks = 0;
				}
				else
				{
					Log.Message("RPThingMaker.ErrorRCP".Translate(this.MakerThingDef.label, ticks.ToString(), minProd.ToString(), maxProd.ToString()), false);
					this.isProducing = false;
					this.NumProd = 0;
					this.ProdWorkTicks = 0;
					this.TotalProdWorkTicks = 0;
				}
				return false;
			}
			List<RPThingMakerUtility.RPRCPListItem> listRCP = RPThingMakerUtility.GetRCPList(t);
			if (listRCP.Count <= 0)
			{
				if (this.debug)
				{
					Log.Message("RCP is False.", false);
				}
				return false;
			}
			if (this.debug)
			{
				Log.Message("RCP Listings: " + listRCP.Count.ToString(), false);
			}
			List<Building_RPThingMaker.RCPItemCanUse> RCPListPotentials = new List<Building_RPThingMaker.RCPItemCanUse>();
			List<int> RCPGroups = new List<int>();
			for (int i = 0; i < listRCP.Count; i++)
			{
				int MaterialsMin = 0;
				int MaterialsMax = 0;
				RPThingMakerUtility.RPRCPListItem RCPItem = listRCP[i];
				int RCPMinNumNeeded = (int)Math.Round((double)((float)(RCPItem.num * minProd) * RCPItem.ratio));
				int RCPMaxNumNeeded = (int)Math.Round((double)((float)(RCPItem.num * maxProd) * RCPItem.ratio));
				if (this.HasEnoughMaterialInHoppers(RCPItem.def, RCPMinNumNeeded, true))
				{
					MaterialsMin = RCPMinNumNeeded;
				}
				if (this.HasEnoughMaterialInHoppers(RCPItem.def, RCPMaxNumNeeded, false))
				{
					MaterialsMax = RCPMaxNumNeeded;
				}
				if (MaterialsMin > 0 || MaterialsMax > 0)
				{
					RCPListPotentials.Add(new Building_RPThingMaker.RCPItemCanUse
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
			if (this.debug)
			{
				Log.Message("InnerRecipe List: Groups: " + RCPGroups.Count.ToString() + " , Potentials: " + RCPListPotentials.Count.ToString(), false);
			}
			FinalList = new List<Building_RPThingMaker.RCPItemCanUse>();
			bool NotAllGroups = false;
			if (RCPGroups.Count > 0)
			{
				for (int j = 0; j < RCPGroups.Count; j++)
				{
					bool foundGroup = false;
					if (RCPListPotentials.Count > 0)
					{
						Building_RPThingMaker.RCPItemCanUse bestthingsofar = default(Building_RPThingMaker.RCPItemCanUse);
						bool best = false;
						bool bestmax = false;
						for (int k = 0; k < RCPListPotentials.Count; k++)
						{
							Building_RPThingMaker.RCPItemCanUse itemchk = RCPListPotentials[k];
							if (itemchk.Grp == RCPGroups[j])
							{
								foundGroup = true;
								if (itemchk.Min > 0)
								{
									if (itemchk.Max > 0)
									{
										if (!bestmax)
										{
											bestthingsofar.def = itemchk.def;
											bestthingsofar.Min = itemchk.Min;
											bestthingsofar.Max = itemchk.Max;
											bestthingsofar.Grp = itemchk.Grp;
											best = true;
											bestmax = true;
										}
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
							}
						}
						if (!bestmax)
						{
							bestthingsofar.Max = 0;
						}
						FinalList.Add(bestthingsofar);
					}
					if (!foundGroup)
					{
						NotAllGroups = true;
						Building_RPThingMaker.DoNotFoundGroupsOverlay(this, t, RCPGroups[j]);
					}
				}
			}
			if (FinalList.Count > 0)
			{
				for (int l = 0; l < FinalList.Count; l++)
				{
					if (FinalList[l].Max == 0)
					{
						CanUseMax = false;
					}
				}
			}
			if (NotAllGroups)
			{
				if (this.debug)
				{
					Log.Message("RCP is False. Not all inputs found", false);
				}
				return false;
			}
			if (this.debug)
			{
				Log.Message("RCP is True. with (" + FinalList.Count.ToString() + ") final list items", false);
			}
			return true;
		}

		// Token: 0x06000088 RID: 136 RVA: 0x00005768 File Offset: 0x00003968
		public static void DoNotFoundGroupsOverlay(Building_RPThingMaker b, ThingDef def, int grp)
		{
			if (Find.CurrentMap != null && Find.CurrentMap == b.Map)
			{
				List<RPThingMakerUtility.RPRCPListItem> listRCP = RPThingMakerUtility.GetRCPList(def);
				List<ThingDef> alerts = new List<ThingDef>();
				if (listRCP.Count > 0)
				{
					foreach (RPThingMakerUtility.RPRCPListItem item in listRCP)
					{
						if (item.mixgrp == grp)
						{
							alerts.AddDistinct(item.def);
						}
					}
				}
				if (alerts.Count > 0)
				{
					Material OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
					int i = 0;
					foreach (ThingDef alert in alerts)
					{
						if (!alert.defName.StartsWith("Chunk") || (alert.defName.StartsWith("Chunk") && i < 1))
						{
							Material mat = MaterialPool.MatFrom(alert.uiIcon, ShaderDatabase.MetaOverlay, Color.white);
							float BaseAlt = AltitudeLayer.WorldClipper.AltitudeFor();
							if (mat != null)
							{
								int altInd = 21;
								Mesh plane = MeshPool.plane08;
								Vector3 drawPos = b.TrueCenter();
								drawPos.y = BaseAlt + 0.046875f * (float)altInd;
								drawPos.x += (float)i;
								drawPos.z += (float)(grp - 2);
								float num2 = ((float)Math.Sin((double)((Time.realtimeSinceStartup + 397f * (float)(b.thingIDNumber % 571)) * 4f)) + 1f) * 0.5f;
								num2 = 0.3f + num2 * 0.7f;
								for (int j = 0; j < 2; j++)
								{
									Material material;
									if (j < 1)
									{
										material = FadedMaterialPool.FadedVersionOf(mat, num2);
									}
									else
									{
										material = FadedMaterialPool.FadedVersionOf(OutOfFuelMat, num2);
									}
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
			}
		}

		// Token: 0x06000089 RID: 137 RVA: 0x000059A4 File Offset: 0x00003BA4
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			IEnumerator<Gizmo> enumerator = null;
			if (base.Faction == Faction.OfPlayer)
			{
				string SelectDesc = "RPThingMaker.ThingSelectDesc".Translate();
				if (this.MakerThingDef == null)
				{
					string NoChem = "RPThingMaker.ThingSelect".Translate();
					yield return new Command_Action
					{
						defaultLabel = NoChem,
						icon = ContentFinder<Texture2D>.Get(this.thingTexPath, true),
						defaultDesc = SelectDesc,
						action = delegate()
						{
							this.RPMakerSelectThing();
						}
					};
				}
				else
				{
					Texture2D IconToUse = RPThingMakerUtility.GetRPThingIcon(this.MakerThingDef);
					string LabelDetail = this.MakerThingDef.label.CapitalizeFirst();
					LabelDetail = string.Concat(new object[]
					{
						LabelDetail,
						" [",
						this.NumProd,
						"] "
					});
					if (this.TotalProdWorkTicks > 0)
					{
						LabelDetail = LabelDetail + " (" + ((int)((float)(this.TotalProdWorkTicks - this.ProdWorkTicks) / (float)this.TotalProdWorkTicks * 100f)).ToString() + "%)";
					}
					yield return new Command_Action
					{
						defaultLabel = LabelDetail,
						icon = IconToUse,
						defaultDesc = SelectDesc,
						action = delegate()
						{
							this.RPMakerSelectThing();
						}
					};
				}
				string LabelProduce = "RPThingMaker.Production".Translate();
				string LabelProduceDesc = "RPThingMaker.ProductionDesc".Translate();
				if (this.isProducing)
				{
					if (this.MakerThingDef != null)
					{
						int ticks;
						int minProd;
						int maxProd;
						string research;
						if (RPThingMakerUtility.RCPProdValues(this.MakerThingDef, out ticks, out minProd, out maxProd, out research))
						{
							LabelProduce += "RPThingMaker.ProdLabelRange".Translate(minProd.ToString(), maxProd.ToString());
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
					icon = ContentFinder<Texture2D>.Get(this.produceTexPath, true),
					defaultLabel = LabelProduce,
					defaultDesc = LabelProduceDesc,
					isActive = (() => this.isProducing),
					toggleAction = delegate()
					{
						this.ToggleProducing(this.isProducing);
					}
				};
				string LimitTexPath = this.FrontLimitPath;
				string LimitLabelDetail;
				if (this.StockLimit > 0)
				{
					int ActualStockNum;
					Building_RPThingMaker.StockLimitReached(this, this.MakerThingDef, this.StockLimit, out ActualStockNum);
					int LimitPct = ActualStockNum * 100 / this.StockLimit;
					LimitLabelDetail = "RPThingMaker.StockLabel".Translate(this.StockLimit.ToString(), LimitPct.ToString());
					LimitTexPath += this.StockLimit.ToString();
				}
				else
				{
					LimitLabelDetail = "RPThingMaker.StockLabelNL".Translate();
					LimitTexPath += "No";
				}
				LimitTexPath += this.EndLimitPath;
				Texture2D LimitIconToUse = ContentFinder<Texture2D>.Get(LimitTexPath, true);
				string SelectLimit = "RPThingMaker.SelectStockLimit".Translate();
				yield return new Command_Action
				{
					defaultLabel = LimitLabelDetail,
					icon = LimitIconToUse,
					defaultDesc = SelectLimit,
					action = delegate()
					{
						this.RPMakerSelectLimit();
					}
				};
				if (Prefs.DevMode)
				{
					yield return new Command_Toggle
					{
						icon = ContentFinder<Texture2D>.Get(this.debugTexPath, true),
						defaultLabel = "Debug Mode",
						defaultDesc = "Send debug messages to Log",
						isActive = (() => this.debug),
						toggleAction = delegate()
						{
							this.ToggleDebug(this.debug);
						}
					};
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000059B4 File Offset: 0x00003BB4
		public void ToggleDebug(bool flag)
		{
			this.debug = !flag;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x000059C0 File Offset: 0x00003BC0
		public void ToggleProducing(bool flag)
		{
			this.isProducing = !flag;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000059CC File Offset: 0x00003BCC
		public void RPMakerSelectLimit()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<int> Choices = RPThingMakerUtility.GetMaxStock();
			if (Choices.Count > 0)
			{
				for (int i = 0; i < Choices.Count; i++)
				{
					string text;
					if (Choices[i] > 0)
					{
						text = Choices[i].ToString();
					}
					else
					{
						text = "RPThingMaker.StockNoLimit".Translate();
					}
					int value = Choices[i];
					list.Add(new FloatMenuOption(text, delegate()
					{
						this.SetStockLimits(value);
					}, MenuOptionPriority.Default, null, null, 29f, null, null));
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		// Token: 0x0600008D RID: 141 RVA: 0x00005A7C File Offset: 0x00003C7C
		public void RPMakerSelectThing()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			string text = "RPThingMaker.SelNoThing".Translate();
			list.Add(new FloatMenuOption(text, delegate()
			{
				this.SetProdControlValues(null, false, 0, 0);
			}, MenuOptionPriority.Default, null, null, 29f, null, null));
			List<string> Choices = RPThingMakerUtility.GetMakeList();
			if (Choices.Count > 0)
			{
				for (int i = 0; i < Choices.Count; i++)
				{
					ThingDef ChoiceDef = DefDatabase<ThingDef>.GetNamed(Choices[i], true);
					text = ChoiceDef.label.CapitalizeFirst();
					if (Building_RPThingMaker.IsThingAvailable(ChoiceDef))
					{
						list.Add(new FloatMenuOption(text, delegate()
						{
							this.SetProdControlValues(ChoiceDef, true, 0, 0);
						}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, ChoiceDef), null));
					}
				}
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00005B66 File Offset: 0x00003D66
		public void SetStockLimits(int aStockLim)
		{
			this.StockLimit = aStockLim;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x00005B70 File Offset: 0x00003D70
		public void SetProdControlValues(ThingDef tdef, bool prod, int num, int ticks)
		{
			if (tdef == null)
			{
				this.MakerThingDef = null;
				this.isProducing = false;
				this.NumProd = 0;
				this.ProdWorkTicks = 0;
				this.TotalProdWorkTicks = 0;
				return;
			}
			if (this.MakerThingDef != tdef)
			{
				this.MakerThingDef = tdef;
				this.NumProd = 0;
				this.ProdWorkTicks = 0;
				this.TotalProdWorkTicks = 0;
			}
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00005BC9 File Offset: 0x00003DC9
		public bool IsWorking(Building b)
		{
			return !b.IsBrokenDown() && this.powerComp.PowerOn;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00005BE8 File Offset: 0x00003DE8
		public static bool IsThingAvailable(ThingDef chkDef)
		{
			int ticks;
			int minProd;
			int maxProd;
			string research;
			return RPThingMakerUtility.RCPProdValues(chkDef, out ticks, out minProd, out maxProd, out research) && research != "" && DefDatabase<ResearchProjectDef>.GetNamed(research, false).IsFinished;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00005C24 File Offset: 0x00003E24
		public static bool StockLimitReached(Building b, ThingDef stockThing, int stockLim, out int ActualStockNum)
		{
			ActualStockNum = 0;
			if (stockLim > 0 && stockThing != null)
			{
				List<Thing> StockListing = b.Map.listerThings.ThingsOfDef(stockThing);
				if (StockListing.Count > 0)
				{
					for (int i = 0; i < StockListing.Count; i++)
					{
						ActualStockNum += StockListing[i].stackCount;
					}
				}
				if (ActualStockNum >= stockLim)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00005C80 File Offset: 0x00003E80
		public virtual bool HasEnoughMaterialInHoppers(ThingDef NeededThing, int required, bool isMin)
		{
			int num = 0;
			for (int i = 0; i < this.AdjCellsCardinalInBounds.Count; i++)
			{
				IntVec3 c = this.AdjCellsCardinalInBounds[i];
				Thing thingNeed = null;
				Thing thingHopper = null;
				List<Thing> thingList = c.GetThingList(base.Map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing3 = thingList[j];
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
			if (this.debug)
			{
				Log.Message(string.Concat(new string[]
				{
					"Enough Materials? (",
					(num >= required) ? "Yes" : "No",
					"): (",
					NeededThing.defName,
					") Found:",
					num.ToString(),
					" for ",
					required.ToString(),
					" required as ",
					isMin ? "Min" : "Max"
				}), false);
			}
			return num >= required;
		}

		// Token: 0x04000040 RID: 64
		public bool debug;

		// Token: 0x04000041 RID: 65
		public CompPowerTrader powerComp;

		// Token: 0x04000042 RID: 66
		public ThingDef MakerThingDef;

		// Token: 0x04000043 RID: 67
		public int ProdWorkTicks;

		// Token: 0x04000044 RID: 68
		public int TotalProdWorkTicks;

		// Token: 0x04000045 RID: 69
		public bool isProducing;

		// Token: 0x04000046 RID: 70
		public int NumProd;

		// Token: 0x04000047 RID: 71
		public int StockLimit;

		// Token: 0x04000048 RID: 72
		public float effeciencyFactor = 0.95f;

		// Token: 0x04000049 RID: 73
		private List<IntVec3> cachedAdjCellsCardinal;

		// Token: 0x0400004A RID: 74
		public Sustainer makeSustainer;

		// Token: 0x0400004B RID: 75
		public static string UITexPath = "Things/Building/Misc/RPThingMaker/UI/";

		// Token: 0x0400004C RID: 76
		[NoTranslate]
		private string produceTexPath = Building_RPThingMaker.UITexPath + "RPThingMakerProduce_Icon";

		// Token: 0x0400004D RID: 77
		[NoTranslate]
		private string thingTexPath = Building_RPThingMaker.UITexPath + "RPThingMaker_ThingIcon";

		// Token: 0x0400004E RID: 78
		[NoTranslate]
		private string debugTexPath = Building_RPThingMaker.UITexPath + "RPThingMakerDebug_Icon";

		// Token: 0x0400004F RID: 79
		[NoTranslate]
		private string FrontLimitPath = Building_RPThingMaker.UITexPath + "StockLimits/RPThingMakerStock";

		// Token: 0x04000050 RID: 80
		[NoTranslate]
		private string EndLimitPath = "Limit_icon";

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
