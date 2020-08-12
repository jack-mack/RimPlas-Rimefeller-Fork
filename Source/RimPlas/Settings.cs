using System;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x02000024 RID: 36
	public class Settings : ModSettings
	{
		// Token: 0x060000AE RID: 174 RVA: 0x00006364 File Offset: 0x00004564
		public void DoWindowContents(Rect canvas)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = canvas.width;
			listing_Standard.Begin(canvas);
			listing_Standard.Gap(12f);
			checked
			{
				listing_Standard.Label("RimPlas.ResPct".Translate() + "  " + (int)this.ResPct, -1f, null);
				this.ResPct = (float)((int)listing_Standard.Slider((float)((int)this.ResPct), 10f, 200f));
				listing_Standard.Gap(12f);
				Text.Font = GameFont.Tiny;
				listing_Standard.Label("          " + "RimPlas.ResWarn".Translate(), -1f, null);
				listing_Standard.Gap(12f);
				listing_Standard.Label("          " + "RimPlas.ResTip".Translate(), -1f, null);
				Text.Font = GameFont.Small;
				listing_Standard.Gap(12f);
				listing_Standard.Label("RimPlas.GVentMin".Translate() + "  " + (int)this.GVentMin, -1f, null);
				this.GVentMin = (float)((int)listing_Standard.Slider(this.GVentMin, -20f, 20f));
				listing_Standard.Gap(12f);
				listing_Standard.Label("RimPlas.GVentMax".Translate() + "  " + (int)this.GVentMax, -1f, null);
				this.GVentMax = (float)((int)listing_Standard.Slider(this.GVentMax, 25f, 50f));
				listing_Standard.Gap(12f);
				listing_Standard.CheckboxLabeled("RimPlas.AllowPainCtrlHB".Translate(), ref this.AllowPainCtrlHB, null);
				listing_Standard.Gap(12f);
				listing_Standard.CheckboxLabeled("RimPlas.AllowRecCtrlHB".Translate(), ref this.AllowRecCtrlHB, null);
				listing_Standard.Gap(12f);
				listing_Standard.CheckboxLabeled("RimPlas.AllowMentalCtrlHB".Translate(), ref this.AllowMentalCtrlHB, null);
				listing_Standard.Gap(12f);
				listing_Standard.End();
			}
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00006598 File Offset: 0x00004798
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.ResPct, "ResPct", 100f, false);
			Scribe_Values.Look<float>(ref this.GVentMin, "GVentMin", 10f, false);
			Scribe_Values.Look<float>(ref this.GVentMax, "GVentMax", 30f, false);
			Scribe_Values.Look<bool>(ref this.AllowPainCtrlHB, "AllowPainCtrlHB", true, false);
			Scribe_Values.Look<bool>(ref this.AllowRecCtrlHB, "AllowRecCtrlHB", true, false);
			Scribe_Values.Look<bool>(ref this.AllowMentalCtrlHB, "AllowMentalCtrlHB", true, false);
		}

		// Token: 0x04000056 RID: 86
		public float ResPct = 100f;

		// Token: 0x04000057 RID: 87
		public float GVentMin = 10f;

		// Token: 0x04000058 RID: 88
		public float GVentMax = 30f;

		// Token: 0x04000059 RID: 89
		public bool AllowPainCtrlHB = true;

		// Token: 0x0400005A RID: 90
		public bool AllowRecCtrlHB = true;

		// Token: 0x0400005B RID: 91
		public bool AllowMentalCtrlHB = true;
	}
}
