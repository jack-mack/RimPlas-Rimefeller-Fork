using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimPlas
{
	// Token: 0x02000017 RID: 23
	[StaticConstructorOnStartup]
	internal static class HarmonyPatching
	{
		// Token: 0x0600004F RID: 79 RVA: 0x000038A8 File Offset: 0x00001AA8
		static HarmonyPatching()
		{
			new Harmony("com.Pelador.Rimworld.RimPlas").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}
