using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x02000015 RID: 21
	[HarmonyPatch(typeof(Plant))]
	[HarmonyPatch("GrowthRateFactor_Temperature", MethodType.Getter)]
	public class GrowthRateFactor_Temperature_Patch
	{
		// Token: 0x0600004B RID: 75 RVA: 0x00003710 File Offset: 0x00001910
		[HarmonyPrefix]
		[HarmonyPriority(800)]
		public static bool PreFix(ref Plant __instance, ref float __result)
		{
			Plant plant = __instance;
			if (((plant != null) ? plant.Map : null) != null)
			{
				Map map = __instance.Map;
				List<Thing> things = __instance.Position.GetThingList(map);
				if (things.Count > 0)
				{
					foreach (Thing thing in things)
					{
						if (thing != __instance && thing is Building_PlantGrower && thing.def.defName == "RPGrapheneGrowBin")
						{
							CompPowerTrader comp = (thing as Building).TryGetComp<CompPowerTrader>();
							if (comp != null && comp.PowerOn && !(thing as Building).IsBrokenDown())
							{
								__result = 1f;
								return false;
							}
						}
					}
					return true;
				}
			}
			return true;
		}
	}
}
