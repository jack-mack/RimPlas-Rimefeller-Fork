using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    public class CompPowerPlantSmallSolarGraphene : CompPowerPlant
    {
        // Token: 0x04000001 RID: 1
        private const float FullSunPower = 750f;

        // Token: 0x04000002 RID: 2
        private const float NightPower = 0f;

        // Token: 0x04000003 RID: 3
        private static readonly Vector2 BarSize = new Vector2(1.15f, 0.07f);

        // Token: 0x04000004 RID: 4
        private static readonly Material PowerPlantSolarBarFilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

        // Token: 0x04000005 RID: 5
        private static readonly Material PowerPlantSolarBarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        protected override float DesiredPowerOutput =>
            Mathf.Lerp(0f, 750f, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000002 RID: 2 RVA: 0x00002080 File Offset: 0x00000280
        private float RoofedPowerOutputFactor
        {
            get
            {
                var num = 0;
                var num2 = 0;
                foreach (var item in parent.OccupiedRect())
                {
                    num++;
                    if (parent.Map.roofGrid.Roofed(item))
                    {
                        num2++;
                    }
                }

                return (num - num2) / (float) num;
            }
        }

        // Token: 0x06000003 RID: 3 RVA: 0x00002104 File Offset: 0x00000304
        public override void PostDraw()
        {
            base.PostDraw();
            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + (Vector3.up * 0.1f);
            r.size = BarSize;
            r.fillPercent = PowerOutput / 750f;
            r.filledMat = PowerPlantSolarBarFilledMat;
            r.unfilledMat = PowerPlantSolarBarUnfilledMat;
            r.margin = 0.15f;
            var rotation = parent.Rotation;
            rotation.Rotate(RotationDirection.Clockwise);
            r.rotation = rotation;
            GenDraw.DrawFillableBar(r);
        }
    }
}