using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x0200001B RID: 27
	[StaticConstructorOnStartup]
	public class RPCompPowerPlantWind : CompPowerPlant
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00003BE9 File Offset: 0x00001DE9
		protected override float DesiredPowerOutput
		{
			get
			{
				return this.cachedPowerOutput;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600005C RID: 92 RVA: 0x00003BF1 File Offset: 0x00001DF1
		private float PowerPercent
		{
			get
			{
				return base.PowerOutput / ((0f - base.Props.basePowerConsumption) * 1.5f);
			}
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00003C14 File Offset: 0x00001E14
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			RPCompPowerPlantWind.BarSize = new Vector2((float)this.parent.def.size.z - 0.95f, 0.1f);
			this.RecalculateBlockages();
			this.spinPosition = Rand.Range(0f, 15f);
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00003C6E File Offset: 0x00001E6E
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<int>(ref this.ticksSinceWeatherUpdate, "updateCounter", 0, false);
			Scribe_Values.Look<float>(ref this.cachedPowerOutput, "cachedPowerOutput", 0f, false);
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00003CA0 File Offset: 0x00001EA0
		public override void CompTick()
		{
			base.CompTick();
			if (!base.PowerOn)
			{
				this.cachedPowerOutput = 0f;
				return;
			}
			this.ticksSinceWeatherUpdate++;
			if (this.ticksSinceWeatherUpdate >= this.updateWeatherEveryXTicks)
			{
				float num = Mathf.Min(this.parent.Map.windManager.WindSpeed, 1.5f);
				this.ticksSinceWeatherUpdate = 0;
				this.cachedPowerOutput = 0f - base.Props.basePowerConsumption * num;
				this.RecalculateBlockages();
				if (this.windPathBlockedCells.Count > 0)
				{
					float num2 = 0f;
					for (int i = 0; i < this.windPathBlockedCells.Count; i++)
					{
						num2 += this.cachedPowerOutput * 0.2f;
					}
					this.cachedPowerOutput -= num2;
					if (this.cachedPowerOutput < 0f)
					{
						this.cachedPowerOutput = 0f;
					}
				}
			}
			if (this.cachedPowerOutput > 0.01f)
			{
				this.spinPosition += this.PowerPercent * RPCompPowerPlantWind.SpinRateFactor;
			}
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00003DB4 File Offset: 0x00001FB4
		public override void PostDraw()
		{
			base.PostDraw();
			GenDraw.FillableBarRequest r = new GenDraw.FillableBarRequest
			{
				center = this.parent.DrawPos + Vector3.up * 0.1f,
				size = RPCompPowerPlantWind.BarSize,
				fillPercent = this.PowerPercent,
				filledMat = RPCompPowerPlantWind.WindTurbineBarFilledMat,
				unfilledMat = RPCompPowerPlantWind.WindTurbineBarUnfilledMat,
				margin = 0.15f
			};
			Rot4 rotation = this.parent.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
			Vector3 pos = this.parent.TrueCenter();
			pos += this.parent.Rotation.FacingCell.ToVector3() * RPCompPowerPlantWind.VerticalBladeOffset;
			pos += this.parent.Rotation.RighthandCell.ToVector3() * RPCompPowerPlantWind.HorizontalBladeOffset;
			pos.y += 0.0454545468f;
			float num = RPCompPowerPlantWind.BladeWidth * Mathf.Sin(this.spinPosition);
			if (num < 0f)
			{
				num *= -1f;
			}
			bool flag = this.spinPosition % 3.14159274f * 2f < 3.14159274f;
			Vector2 vector = new Vector2(num, 1f);
			Vector3 s = new Vector3(vector.x, 1f, vector.y);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(pos, this.parent.Rotation.AsQuat, s);
			Graphics.DrawMesh(flag ? MeshPool.plane10 : MeshPool.plane10Flip, matrix, RPCompPowerPlantWind.WindTurbineBladesMat, 0);
			pos.y -= 0.09090909f;
			matrix.SetTRS(pos, this.parent.Rotation.AsQuat, s);
			Graphics.DrawMesh(flag ? MeshPool.plane10Flip : MeshPool.plane10, matrix, RPCompPowerPlantWind.WindTurbineBladesMat, 0);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003FC0 File Offset: 0x000021C0
		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompInspectStringExtra());
			if (this.windPathBlockedCells.Count > 0)
			{
				stringBuilder.AppendLine();
				Thing thing = null;
				if (this.windPathBlockedByThings != null)
				{
					thing = this.windPathBlockedByThings[0];
				}
				if (thing != null)
				{
					stringBuilder.Append("WindTurbine_WindPathIsBlockedBy".Translate() + " " + thing.Label);
				}
				else
				{
					stringBuilder.Append("WindTurbine_WindPathIsBlockedByRoof".Translate());
				}
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004058 File Offset: 0x00002258
		private void RecalculateBlockages()
		{
			if (this.windPathCells.Count == 0)
			{
				IEnumerable<IntVec3> collection = RPWindTurbine_Utility.CalculateWindCells(this.parent.Position, this.parent.Rotation, this.parent.def.size);
				this.windPathCells.AddRange(collection);
			}
			this.windPathBlockedCells.Clear();
			this.windPathBlockedByThings.Clear();
			for (int i = 0; i < this.windPathCells.Count; i++)
			{
				IntVec3 intVec = this.windPathCells[i];
				if (this.parent.Map.roofGrid.Roofed(intVec))
				{
					this.windPathBlockedByThings.Add(null);
					this.windPathBlockedCells.Add(intVec);
				}
				else
				{
					List<Thing> list = this.parent.Map.thingGrid.ThingsListAt(intVec);
					for (int j = 0; j < list.Count; j++)
					{
						Thing thing = list[j];
						if (thing.def.blockWind)
						{
							this.windPathBlockedByThings.Add(thing);
							this.windPathBlockedCells.Add(intVec);
							break;
						}
					}
				}
			}
		}

		// Token: 0x04000026 RID: 38
		public int updateWeatherEveryXTicks = 250;

		// Token: 0x04000027 RID: 39
		private int ticksSinceWeatherUpdate;

		// Token: 0x04000028 RID: 40
		private float cachedPowerOutput;

		// Token: 0x04000029 RID: 41
		private List<IntVec3> windPathCells = new List<IntVec3>();

		// Token: 0x0400002A RID: 42
		private List<Thing> windPathBlockedByThings = new List<Thing>();

		// Token: 0x0400002B RID: 43
		private List<IntVec3> windPathBlockedCells = new List<IntVec3>();

		// Token: 0x0400002C RID: 44
		private float spinPosition;

		// Token: 0x0400002D RID: 45
		private const float MaxUsableWindIntensity = 1.5f;

		// Token: 0x0400002E RID: 46
		[TweakValue("Graphics", 0f, 0.1f)]
		private static float SpinRateFactor = 0.035f;

		// Token: 0x0400002F RID: 47
		[TweakValue("Graphics", -1f, 1f)]
		private static float HorizontalBladeOffset = -0.02f;

		// Token: 0x04000030 RID: 48
		[TweakValue("Graphics", 0f, 1f)]
		private static float VerticalBladeOffset = 0.7f;

		// Token: 0x04000031 RID: 49
		[TweakValue("Graphics", 4f, 8f)]
		private static float BladeWidth = 3.7f;

		// Token: 0x04000032 RID: 50
		private const float PowerReductionPercentPerObstacle = 0.2f;

		// Token: 0x04000033 RID: 51
		private const string TranslateWindPathIsBlockedBy = "WindTurbine_WindPathIsBlockedBy";

		// Token: 0x04000034 RID: 52
		private const string TranslateWindPathIsBlockedByRoof = "WindTurbine_WindPathIsBlockedByRoof";

		// Token: 0x04000035 RID: 53
		private static Vector2 BarSize;

		// Token: 0x04000036 RID: 54
		private static readonly Material WindTurbineBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f), false);

		// Token: 0x04000037 RID: 55
		private static readonly Material WindTurbineBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f), false);

		// Token: 0x04000038 RID: 56
		private static readonly Material WindTurbineBladesMat = MaterialPool.MatFrom("Things/Building/Power/RPGrapheneWindTurbine/RPGrapheneWindTurbineBlades");
	}
}
