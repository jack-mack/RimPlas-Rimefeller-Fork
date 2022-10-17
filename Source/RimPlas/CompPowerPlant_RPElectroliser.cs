using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
public class CompPowerPlant_RPElectroliser : CompPowerPlant
{
    private float CollectorFactor;

    protected override float DesiredPowerOutput => base.DesiredPowerOutput * CollectorFactor;

    public override void CompTick()
    {
        base.CompTick();
        var factor = 0f;
        if (parent.Spawned)
        {
            var totalCells = 0f;
            var goodCells = 0f;
            foreach (var WaterUseCell in WaterUseCells(parent.Position, parent.Rotation))
            {
                totalCells += 1f;
                if (!parent.Map.terrainGrid.TerrainAt(WaterUseCell).IsWater)
                {
                    continue;
                }

                if (parent.Map.terrainGrid.TerrainAt(WaterUseCell).IsRiver)
                {
                    goodCells += 1f;
                }
                else
                {
                    goodCells += 0.75f;
                }
            }

            if (totalCells > 0f)
            {
                factor = goodCells / totalCells;
            }
        }

        CollectorFactor = factor;
    }

    public override string CompInspectStringExtra()
    {
        return base.CompInspectStringExtra() +
               ("\n" + "RimPlas.RPElectroliserOutput".Translate(CollectorFactor.ToStringPercent()));
    }

    public static IEnumerable<IntVec3> GroundCells(IntVec3 loc, Rot4 rot)
    {
        var perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
        yield return loc - rot.FacingCell;
        yield return loc - rot.FacingCell - perpOffset;
        yield return loc - rot.FacingCell + perpOffset;
        yield return loc;
        yield return loc - perpOffset;
        yield return loc + perpOffset;
    }

    public static IEnumerable<IntVec3> WaterUseCells(IntVec3 loc, Rot4 rot)
    {
        var perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
        yield return loc + rot.FacingCell;
        yield return loc + rot.FacingCell - perpOffset;
        yield return loc + rot.FacingCell + perpOffset;
    }
}