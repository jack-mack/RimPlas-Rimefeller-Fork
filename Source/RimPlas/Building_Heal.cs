using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimPlas
{
	// Token: 0x02000004 RID: 4
	public class Building_Heal : Building
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002300 File Offset: 0x00000500
		public CompBuildHeal HealComp
		{
			get
			{
				return base.GetComp<CompBuildHeal>();
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002308 File Offset: 0x00000508
		public int GetHashOffset(Thing t)
		{
			string text = t.GetHashCode().ToString();
			return (int)text[text.Length - 1];
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002330 File Offset: 0x00000530
		public override void TickLong()
		{
			base.TickLong();
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002338 File Offset: 0x00000538
		public override void Tick()
		{
			base.Tick();
			int offset = this.GetHashOffset(this);
			if (Find.TickManager.TicksGame % (120 + offset) == 0)
			{
				float HealFactor = base.GetComp<CompBuildHeal>().Props.HealFactor;
				bool PowerNeeded = base.GetComp<CompBuildHeal>().Props.PowerNeeded;
				int HP = base.HitPoints;
				int MaxHP = base.MaxHitPoints;
				if (HP < MaxHP && !this.IsBrokenDown() && !this.IsBurning())
				{
					bool Healing = true;
					if (PowerNeeded)
					{
						CompPowerTrader power = base.GetComp<CompPowerTrader>();
						if (power == null || !power.PowerOn)
						{
							Healing = false;
						}
					}
					if (Healing)
					{
						int HealPts = (int)(HealFactor * (float)MaxHP);
						if (MaxHP - HP > HealPts)
						{
							base.HitPoints = HP + HealPts;
						}
						else
						{
							base.HitPoints = MaxHP;
							if (base.Map != null)
							{
								base.Map.listerBuildingsRepairable.Notify_BuildingRepaired(this);
								this.ResetRepairers(this);
							}
						}
						if (base.Map != null)
						{
							MoteMaker.ThrowMetaIcon(base.Position, base.Map, ThingDefOf.Mote_HealingCross);
						}
					}
				}
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x0000243C File Offset: 0x0000063C
		private void ResetRepairers(Building Wall)
		{
			if (Wall.Map != null)
			{
				Map map = Wall.Map;
				List<Pawn> list;
				if (map == null)
				{
					list = null;
				}
				else
				{
					MapPawns mapPawns = map.mapPawns;
					if (mapPawns == null)
					{
						list = null;
					}
					else
					{
						List<Pawn> freeColonists = mapPawns.FreeColonists;
						list = ((freeColonists != null) ? freeColonists.ToList<Pawn>() : null);
					}
				}
				List<Pawn> PawnList = list;
				if (PawnList.Count > 0)
				{
					List<Pawn> Repairer = new List<Pawn>();
					for (int i = 0; i < PawnList.Count; i++)
					{
						Pawn pawn = PawnList[i];
						Job curjob = (pawn != null) ? pawn.CurJob : null;
						if (curjob.def.defName == "Repair" && ((curjob != null) ? curjob.targetA.Thing : null) == Wall)
						{
							Repairer.AddDistinct(PawnList[i]);
						}
					}
					if (Repairer.Count > 0)
					{
						for (int j = 0; j < Repairer.Count; j++)
						{
							Pawn pawn2 = Repairer[j];
							if (((pawn2 != null) ? pawn2.CurJob : null) != null)
							{
								Pawn pawn3 = Repairer[j];
								if (pawn3 != null)
								{
									Pawn_JobTracker jobs = pawn3.jobs;
									if (jobs != null)
									{
										jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
