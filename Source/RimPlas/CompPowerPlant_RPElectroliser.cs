using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x0200000B RID: 11
	[StaticConstructorOnStartup]
	public class CompPowerPlant_RPElectroliser : CompPowerPlant
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000028 RID: 40 RVA: 0x00002E53 File Offset: 0x00001053
		protected override float DesiredPowerOutput
		{
			get
			{
				return base.DesiredPowerOutput * this.CollectorFactor;
			}
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002E64 File Offset: 0x00001064
		public override void CompTick()
		{
			base.CompTick();
			float factor = 0f;
			if (this.parent.Spawned)
			{
				float totalCells = 0f;
				float goodCells = 0f;
				foreach (IntVec3 WaterUseCell in CompPowerPlant_RPElectroliser.WaterUseCells(this.parent.Position, this.parent.Rotation))
				{
					totalCells += 1f;
					if (this.parent.Map.terrainGrid.TerrainAt(WaterUseCell).IsWater)
					{
						if (this.parent.Map.terrainGrid.TerrainAt(WaterUseCell).IsRiver)
						{
							goodCells += 1f;
						}
						else
						{
							goodCells += 0.75f;
						}
					}
				}
				if (totalCells > 0f)
				{
					factor = goodCells / totalCells;
				}
			}
			this.CollectorFactor = factor;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002F54 File Offset: 0x00001154
		public override string CompInspectStringExtra()
		{
			return base.CompInspectStringExtra() + ("\n" + "RimPlas.RPElectroliserOutput".Translate(this.CollectorFactor.ToStringPercent()));
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002F8A File Offset: 0x0000118A
		public static IEnumerable<IntVec3> GroundCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc - rot.FacingCell;
			yield return loc - rot.FacingCell - perpOffset;
			yield return loc - rot.FacingCell + perpOffset;
			yield return loc;
			yield return loc - perpOffset;
			yield return loc + perpOffset;
			yield break;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002FA1 File Offset: 0x000011A1
		public static IEnumerable<IntVec3> WaterUseCells(IntVec3 loc, Rot4 rot)
		{
			IntVec3 perpOffset = rot.Rotated(RotationDirection.Counterclockwise).FacingCell;
			yield return loc + rot.FacingCell;
			yield return loc + rot.FacingCell - perpOffset;
			yield return loc + rot.FacingCell + perpOffset;
			yield break;
		}

		// Token: 0x04000018 RID: 24
		private float CollectorFactor;
	}
}
