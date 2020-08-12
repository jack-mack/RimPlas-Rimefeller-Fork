using System;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x02000012 RID: 18
	public class Controller : Mod
	{
		// Token: 0x06000041 RID: 65 RVA: 0x000034BC File Offset: 0x000016BC
		public override string SettingsCategory()
		{
			return "RimPlas.Name".Translate();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x000034CD File Offset: 0x000016CD
		public override void DoSettingsWindowContents(Rect canvas)
		{
			Controller.Settings.DoWindowContents(canvas);
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000034DA File Offset: 0x000016DA
		public Controller(ModContentPack content) : base(content)
		{
			Controller.Settings = base.GetSettings<Settings>();
		}

		// Token: 0x04000022 RID: 34
		public static Settings Settings;
	}
}
