using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x02000007 RID: 7
	[HarmonyPatch(typeof(Plant), "CheckTemperatureMakeLeafless")]
	public class CheckTemperatureMakeLeafless_Patch
	{
		// Token: 0x0600001D RID: 29 RVA: 0x00002B08 File Offset: 0x00000D08
		[HarmonyPrefix]
		[HarmonyPriority(800)]
		public static bool PreFix(ref Plant __instance)
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
