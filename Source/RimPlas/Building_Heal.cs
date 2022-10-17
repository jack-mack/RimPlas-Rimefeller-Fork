using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimPlas;

public class Building_Heal : Building
{
    public CompBuildHeal HealComp => GetComp<CompBuildHeal>();

    public int GetHashOffset(Thing t)
    {
        var text = t.GetHashCode().ToString();
        return text[text.Length - 1];
    }


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
            if (power is not { PowerOn: true })
            {
                Healing = false;
            }
        }

        if (!Healing)
        {
            return;
        }

        var HealPts = (int)(HealFactor * MaxHP);
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
        if (PawnList is not { Count: > 0 })
        {
            return;
        }

        var Repairer = new List<Pawn>();
        foreach (var element in PawnList)
        {
            var curjob = element?.CurJob;
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
            if (pawn?.CurJob == null)
            {
                continue;
            }

            var jobs = pawn.jobs;
            jobs?.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}