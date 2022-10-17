using UnityEngine;
using Verse;

namespace RimPlas;

public class Settings : ModSettings
{
    public bool AllowMentalCtrlHB = true;

    public bool AllowPainCtrlHB = true;

    public bool AllowRecCtrlHB = true;

    public float GVentMax = 30f;

    public float GVentMin = 10f;

    public float ResPct = 100f;

    public void DoWindowContents(Rect canvas)
    {
        var listing_Standard = new Listing_Standard { ColumnWidth = canvas.width };
        listing_Standard.Begin(canvas);
        listing_Standard.Gap();
        checked
        {
            listing_Standard.Label("RimPlas.ResPct".Translate() + "  " + (int)ResPct);
            ResPct = (int)listing_Standard.Slider((int)ResPct, 10f, 200f);
            listing_Standard.Gap();
            Text.Font = GameFont.Tiny;
            listing_Standard.Label("          " + "RimPlas.ResWarn".Translate());
            listing_Standard.Gap();
            listing_Standard.Label("          " + "RimPlas.ResTip".Translate());
            Text.Font = GameFont.Small;
            listing_Standard.Gap();
            listing_Standard.Label("RimPlas.GVentMin".Translate() + "  " + (int)GVentMin);
            GVentMin = (int)listing_Standard.Slider(GVentMin, -20f, 20f);
            listing_Standard.Gap();
            listing_Standard.Label("RimPlas.GVentMax".Translate() + "  " + (int)GVentMax);
            GVentMax = (int)listing_Standard.Slider(GVentMax, 25f, 50f);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("RimPlas.AllowPainCtrlHB".Translate(), ref AllowPainCtrlHB);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("RimPlas.AllowRecCtrlHB".Translate(), ref AllowRecCtrlHB);
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("RimPlas.AllowMentalCtrlHB".Translate(), ref AllowMentalCtrlHB);
            if (Controller.currentVersion != null)
            {
                listing_Standard.Gap();
                GUI.contentColor = Color.gray;
                listing_Standard.Label("RimPlas.CurrentModVersion".Translate(Controller.currentVersion));
                GUI.contentColor = Color.white;
            }

            listing_Standard.End();
        }
    }

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