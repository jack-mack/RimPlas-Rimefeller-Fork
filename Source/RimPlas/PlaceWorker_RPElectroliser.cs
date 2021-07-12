using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x02000019 RID: 25
    public class PlaceWorker_RPElectroliser : PlaceWorker
    {
        // Token: 0x06000054 RID: 84 RVA: 0x00003A50 File Offset: 0x00001C50
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

        // Token: 0x06000055 RID: 85 RVA: 0x00003AF8 File Offset: 0x00001CF8
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

        // Token: 0x06000056 RID: 86 RVA: 0x00003B5C File Offset: 0x00001D5C
        public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            GenDraw.DrawFieldEdges(CompPowerPlant_RPElectroliser.GroundCells(loc, rot).ToList(), Color.white);
            var color = WaterCellsPresent(loc, rot, Find.CurrentMap)
                ? Designator_Place.CanPlaceColor.ToOpaque()
                : Designator_Place.CannotPlaceColor.ToOpaque();
            GenDraw.DrawFieldEdges(CompPowerPlant_RPElectroliser.WaterUseCells(loc, rot).ToList(), color);
        }

        // Token: 0x06000057 RID: 87 RVA: 0x00003BB7 File Offset: 0x00001DB7
        public override IEnumerable<TerrainAffordanceDef> DisplayAffordances()
        {
            yield return TerrainAffordanceDefOf.Medium;
        }
    }
}