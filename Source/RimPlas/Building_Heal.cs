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
        public CompBuildHeal HealComp => GetComp<CompBuildHeal>();

        // Token: 0x06000009 RID: 9 RVA: 0x00002308 File Offset: 0x00000508
        public int GetHashOffset(Thing t)
        {
            var text = t.GetHashCode().ToString();
            return text[text.Length - 1];
        }

        // Token: 0x0600000A RID: 10 RVA: 0x00002330 File Offset: 0x00000530

        // Token: 0x0600000B RID: 11 RVA: 0x00002338 File Offset: 0x00000538
        public override void Tick()
        {
            base.Tick();
            var offset = GetHashOffset(this);
            if (Find.TickManager.TicksGame % (120 + offset) != 0)
            {
                return;
            }

            var HealFactor = GetComp<CompBuildHeal>().Props.HealFactor;
            var PowerNeeded = GetComp<CompBuildHeal>().Props.PowerNeeded;
            var HP = base.HitPoints;
            var MaxHP = MaxHitPoints;
            if (HP >= MaxHP || this.IsBrokenDown() || this.IsBurning())
            {
                return;
            }

            var Healing = true;
            if (PowerNeeded)
            {
                var power = GetComp<CompPowerTrader>();
                if (power == null || !power.PowerOn)
                {
                    Healing = false;
                }
            }

            if (!Healing)
            {
                return;
            }

            var HealPts = (int) (HealFactor * MaxHP);
            if (MaxHP - HP > HealPts)
            {
                base.HitPoints = HP + HealPts;
            }
            else
            {
                base.HitPoints = MaxHP;
                if (Map != null)
                {
                    Map.listerBuildingsRepairable.Notify_BuildingRepaired(this);
                    ResetRepairers(this);
                }
            }

            if (Map != null)
            {
                FleckMaker.ThrowMetaIcon(Position, Map, FleckDefOf.HealingCross);
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x0000243C File Offset: 0x0000063C
        private void ResetRepairers(Building Wall)
        {
            if (Wall.Map == null)
            {
                return;
            }

            var map = Wall.Map;
            List<Pawn> list;
            if (map == null)
            {
                list = null;
            }
            else
            {
                var mapPawns = map.mapPawns;
                if (mapPawns == null)
                {
                    list = null;
                }
                else
                {
                    var freeColonists = mapPawns.FreeColonists;
                    list = freeColonists?.ToList();
                }
            }

            var PawnList = list;
            if (PawnList == null || PawnList.Count <= 0)
            {
                return;
            }

            var Repairer = new List<Pawn>();
            foreach (var element in PawnList)
            {
                var pawn = element;
                var curjob = pawn?.CurJob;
                if (curjob != null && curjob.def.defName == "Repair" && curjob.targetA.Thing == Wall)
                {
                    Repairer.AddDistinct(element);
                }
            }

            if (Repairer.Count <= 0)
            {
                return;
            }

            foreach (var pawn in Repairer)
            {
                var pawn2 = pawn;
                if (pawn2?.CurJob == null)
                {
                    continue;
                }

                var pawn3 = pawn;

                var jobs = pawn3?.jobs;
                jobs?.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }
}