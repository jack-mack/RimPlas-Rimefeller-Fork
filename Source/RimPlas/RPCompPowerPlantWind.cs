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
        // Token: 0x0400002D RID: 45
        private const float MaxUsableWindIntensity = 1.5f;

        // Token: 0x04000032 RID: 50
        private const float PowerReductionPercentPerObstacle = 0.2f;

        // Token: 0x04000033 RID: 51
        private const string TranslateWindPathIsBlockedBy = "WindTurbine_WindPathIsBlockedBy";

        // Token: 0x04000034 RID: 52
        private const string TranslateWindPathIsBlockedByRoof = "WindTurbine_WindPathIsBlockedByRoof";

        // Token: 0x0400002E RID: 46
        [TweakValue("Graphics", 0f, 0.1f)] private static readonly float SpinRateFactor = 0.035f;

        // Token: 0x0400002F RID: 47
        [TweakValue("Graphics", -1f, 1f)] private static readonly float HorizontalBladeOffset = -0.02f;

        // Token: 0x04000030 RID: 48
        [TweakValue("Graphics", 0f, 1f)] private static readonly float VerticalBladeOffset = 0.7f;

        // Token: 0x04000031 RID: 49
        [TweakValue("Graphics", 4f, 8f)] private static readonly float BladeWidth = 3.7f;

        // Token: 0x04000035 RID: 53
        private static Vector2 BarSize;

        // Token: 0x04000036 RID: 54
        private static readonly Material WindTurbineBarFilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

        // Token: 0x04000037 RID: 55
        private static readonly Material WindTurbineBarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        // Token: 0x04000038 RID: 56
        private static readonly Material WindTurbineBladesMat =
            MaterialPool.MatFrom("Things/Building/Power/RPGrapheneWindTurbine/RPGrapheneWindTurbineBlades");

        // Token: 0x0400002A RID: 42
        private readonly List<Thing> windPathBlockedByThings = new List<Thing>();

        // Token: 0x0400002B RID: 43
        private readonly List<IntVec3> windPathBlockedCells = new List<IntVec3>();

        // Token: 0x04000029 RID: 41
        private readonly List<IntVec3> windPathCells = new List<IntVec3>();

        // Token: 0x04000028 RID: 40
        private float cachedPowerOutput;

        // Token: 0x0400002C RID: 44
        private float spinPosition;

        // Token: 0x04000027 RID: 39
        private int ticksSinceWeatherUpdate;

        // Token: 0x04000026 RID: 38
        public int updateWeatherEveryXTicks = 250;

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x0600005B RID: 91 RVA: 0x00003BE9 File Offset: 0x00001DE9
        protected override float DesiredPowerOutput => cachedPowerOutput;

        // Token: 0x1700000C RID: 12
        // (get) Token: 0x0600005C RID: 92 RVA: 0x00003BF1 File Offset: 0x00001DF1
        private float PowerPercent => PowerOutput / ((0f - Props.basePowerConsumption) * 1.5f);

        // Token: 0x0600005D RID: 93 RVA: 0x00003C14 File Offset: 0x00001E14
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            BarSize = new Vector2(parent.def.size.z - 0.95f, 0.1f);
            RecalculateBlockages();
            spinPosition = Rand.Range(0f, 15f);
        }

        // Token: 0x0600005E RID: 94 RVA: 0x00003C6E File Offset: 0x00001E6E
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksSinceWeatherUpdate, "updateCounter");
            Scribe_Values.Look(ref cachedPowerOutput, "cachedPowerOutput");
        }

        // Token: 0x0600005F RID: 95 RVA: 0x00003CA0 File Offset: 0x00001EA0
        public override void CompTick()
        {
            base.CompTick();
            if (!PowerOn)
            {
                cachedPowerOutput = 0f;
                return;
            }

            ticksSinceWeatherUpdate++;
            if (ticksSinceWeatherUpdate >= updateWeatherEveryXTicks)
            {
                var num = Mathf.Min(parent.Map.windManager.WindSpeed, 1.5f);
                ticksSinceWeatherUpdate = 0;
                cachedPowerOutput = 0f - (Props.basePowerConsumption * num);
                RecalculateBlockages();
                if (windPathBlockedCells.Count > 0)
                {
                    var num2 = 0f;
                    for (var i = 0; i < windPathBlockedCells.Count; i++)
                    {
                        num2 += cachedPowerOutput * 0.2f;
                    }

                    cachedPowerOutput -= num2;
                    if (cachedPowerOutput < 0f)
                    {
                        cachedPowerOutput = 0f;
                    }
                }
            }

            if (cachedPowerOutput > 0.01f)
            {
                spinPosition += PowerPercent * SpinRateFactor;
            }
        }

        // Token: 0x06000060 RID: 96 RVA: 0x00003DB4 File Offset: 0x00001FB4
        public override void PostDraw()
        {
            base.PostDraw();
            var r = new GenDraw.FillableBarRequest
            {
                center = parent.DrawPos + (Vector3.up * 0.1f),
                size = BarSize,
                fillPercent = PowerPercent,
                filledMat = WindTurbineBarFilledMat,
                unfilledMat = WindTurbineBarUnfilledMat,
                margin = 0.15f
            };
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
            var pos = parent.TrueCenter();
            pos += parent.Rotation.FacingCell.ToVector3() * VerticalBladeOffset;
            pos += parent.Rotation.RighthandCell.ToVector3() * HorizontalBladeOffset;
            pos.y += 0.0454545468f;
            var num = BladeWidth * Mathf.Sin(spinPosition);
            if (num < 0f)
            {
                num *= -1f;
            }

            var vector = new Vector2(num, 1f);
            var s = new Vector3(vector.x, 1f, vector.y);
            var matrix = default(Matrix4x4);
            matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
            Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10 : MeshPool.plane10Flip,
                matrix, WindTurbineBladesMat, 0);
            pos.y -= 0.09090909f;
            matrix.SetTRS(pos, parent.Rotation.AsQuat, s);
            Graphics.DrawMesh(spinPosition % 3.14159274f * 2f < 3.14159274f ? MeshPool.plane10Flip : MeshPool.plane10,
                matrix, WindTurbineBladesMat, 0);
        }

        // Token: 0x06000061 RID: 97 RVA: 0x00003FC0 File Offset: 0x000021C0
        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            if (windPathBlockedCells.Count <= 0)
            {
                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine();
            Thing thing = null;
            if (windPathBlockedByThings != null)
            {
                thing = windPathBlockedByThings[0];
            }

            if (thing != null)
            {
                stringBuilder.Append("WindTurbine_WindPathIsBlockedBy".Translate() + " " + thing.Label);
            }
            else
            {
                stringBuilder.Append("WindTurbine_WindPathIsBlockedByRoof".Translate());
            }

            return stringBuilder.ToString();
        }

        // Token: 0x06000062 RID: 98 RVA: 0x00004058 File Offset: 0x00002258
        private void RecalculateBlockages()
        {
            if (windPathCells.Count == 0)
            {
                var collection =
                    RPWindTurbine_Utility.CalculateWindCells(parent.Position, parent.Rotation, parent.def.size);
                windPathCells.AddRange(collection);
            }

            windPathBlockedCells.Clear();
            windPathBlockedByThings.Clear();
            foreach (var intVec in windPathCells)
            {
                if (parent.Map.roofGrid.Roofed(intVec))
                {
                    windPathBlockedByThings.Add(null);
                    windPathBlockedCells.Add(intVec);
                }
                else
                {
                    var list = parent.Map.thingGrid.ThingsListAt(intVec);
                    foreach (var thing in list)
                    {
                        if (!thing.def.blockWind)
                        {
                            continue;
                        }

                        windPathBlockedByThings.Add(thing);
                        windPathBlockedCells.Add(intVec);
                        break;
                    }
                }
            }
        }
    }
}