using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas;

public class PlaceWorker_RPElectroliser : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        foreach (var item in CompPowerPlant_RPElectroliser.GroundCells(loc, rot))
        {
            if (!map.terrainGrid.TerrainAt(item).affordances.Contains(TerrainAffordanceDefOf.Medium))
            {
                return new AcceptanceReport(
                    "TerrainCannotSupport_TerrainAffordance".Translate(checkingDef, TerrainAffordanceDefOf.Medium));
            }
        }

        if (!WaterCellsPresent(loc, rot, map))
        {
            return false;
        }

        return true;
    }

    private bool WaterCellsPresent(IntVec3 loc, Rot4 rot, Map map)
    {
        foreach (var item in CompPowerPlant_RPElectroliser.WaterUseCells(loc, rot))
        {
            if (!map.terrainGrid.TerrainAt(item).IsWater)
            {
                return false;
            }
        }

        return true;
    }

    public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        GenDraw.DrawFieldEdges(CompPowerPlant_RPElectroliser.GroundCells(loc, rot).ToList(), Color.white);
        var color = WaterCellsPresent(loc, rot, Find.CurrentMap)
            ? Designator_Place.CanPlaceColor.ToOpaque()
            : Designator_Place.CannotPlaceColor.ToOpaque();
        GenDraw.DrawFieldEdges(CompPowerPlant_RPElectroliser.WaterUseCells(loc, rot).ToList(), color);
    }

    public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
    {
        yield return TerrainAffordanceDefOf.Medium;
    }
}