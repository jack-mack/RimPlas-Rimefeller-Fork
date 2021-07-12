using RimWorld;
using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x0200000A RID: 10
    [StaticConstructorOnStartup]
    public class CompPowerPlantSolarGraphene : CompPowerPlant
    {
        // Token: 0x04000013 RID: 19
        private const float FullSunPower = 2500f;

        // Token: 0x04000014 RID: 20
        private const float NightPower = 0f;

        // Token: 0x04000015 RID: 21
        private static readonly Vector2 BarSize = new Vector2(2.3f, 0.14f);

        // Token: 0x04000016 RID: 22
        private static readonly Material PowerPlantSolarBarFilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.5f, 0.475f, 0.1f));

        // Token: 0x04000017 RID: 23
        private static readonly Material PowerPlantSolarBarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.15f, 0.15f, 0.15f));

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000023 RID: 35 RVA: 0x00002C93 File Offset: 0x00000E93
        protected override float DesiredPowerOutput =>
            Mathf.Lerp(0f, 2500f, parent.Map.skyManager.CurSkyGlow) * RoofedPowerOutputFactor;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000024 RID: 36 RVA: 0x00002CC0 File Offset: 0x00000EC0
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

        // Token: 0x06000025 RID: 37 RVA: 0x00002D44 File Offset: 0x00000F44
        public override void PostDraw()
        {
            base.PostDraw();
            var r = default(GenDraw.FillableBarRequest);
            r.center = parent.DrawPos + (Vector3.up * 0.1f);
            r.size = BarSize;
            r.fillPercent = PowerOutput / 2500f;
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