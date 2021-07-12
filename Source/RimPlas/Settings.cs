using UnityEngine;
using Verse;

namespace RimPlas
{
    // Token: 0x02000024 RID: 36
    public class Settings : ModSettings
    {
        // Token: 0x0400005B RID: 91
        public bool AllowMentalCtrlHB = true;

        // Token: 0x04000059 RID: 89
        public bool AllowPainCtrlHB = true;

        // Token: 0x0400005A RID: 90
        public bool AllowRecCtrlHB = true;

        // Token: 0x04000058 RID: 88
        public float GVentMax = 30f;

        // Token: 0x04000057 RID: 87
        public float GVentMin = 10f;

        // Token: 0x04000056 RID: 86
        public float ResPct = 100f;

        // Token: 0x060000AE RID: 174 RVA: 0x00006364 File Offset: 0x00004564
        public void DoWindowContents(Rect canvas)
        {
            var listing_Standard = new Listing_Standard {ColumnWidth = canvas.width};
            listing_Standard.Begin(canvas);
            listing_Standard.Gap();
            checked
            {
                listing_Standard.Label("RimPlas.ResPct".Translate() + "  " + (int) ResPct);
                ResPct = (int) listing_Standard.Slider((int) ResPct, 10f, 200f);
                listing_Standard.Gap();
                Text.Font = GameFont.Tiny;
                listing_Standard.Label("          " + "RimPlas.ResWarn".Translate());
                listing_Standard.Gap();
                listing_Standard.Label("          " + "RimPlas.ResTip".Translate());
                Text.Font = GameFont.Small;
                listing_Standard.Gap();
                listing_Standard.Label("RimPlas.GVentMin".Translate() + "  " + (int) GVentMin);
                GVentMin = (int) listing_Standard.Slider(GVentMin, -20f, 20f);
                listing_Standard.Gap();
                listing_Standard.Label("RimPlas.GVentMax".Translate() + "  " + (int) GVentMax);
                GVentMax = (int) listing_Standard.Slider(GVentMax, 25f, 50f);
                listing_Standard.Gap();
                listing_Standard.CheckboxLabeled("RimPlas.AllowPainCtrlHB".Translate(), ref AllowPainCtrlHB);
                listing_Standard.Gap();
                listing_Standard.CheckboxLabeled("RimPlas.AllowRecCtrlHB".Translate(), ref AllowRecCtrlHB);
                listing_Standard.Gap();
                listing_Standard.CheckboxLabeled("RimPlas.AllowMentalCtrlHB".Translate(), ref AllowMentalCtrlHB);
                listing_Standard.Gap();
                listing_Standard.End();
            }
        }

        // Token: 0x060000AF RID: 175 RVA: 0x00006598 File Offset: 0x00004798
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ResPct, "ResPct", 100f);
            Scribe_Values.Look(ref GVentMin, "GVentMin", 10f);
            Scribe_Values.Look(ref GVentMax, "GVentMax", 30f);
            Scribe_Values.Look(ref AllowPainCtrlHB, "AllowPainCtrlHB", true);
            Scribe_Values.Look(ref AllowRecCtrlHB, "AllowRecCtrlHB", true);
            Scribe_Values.Look(ref AllowMentalCtrlHB, "AllowMentalCtrlHB", true);
        }
    }
}