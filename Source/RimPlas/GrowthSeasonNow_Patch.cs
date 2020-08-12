using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x02000016 RID: 22
	[HarmonyPatch(typeof(PlantUtility), "GrowthSeasonNow")]
	public class GrowthSeasonNow_Patch
	{
		// Token: 0x0600004D RID: 77 RVA: 0x000037F0 File Offset: 0x000019F0
		[HarmonyPrefix]
		[HarmonyPriority(800)]
		public static bool PreFix(ref bool __result, IntVec3 c, Map map, bool forSowing = false)
		{
			if (map != null)
			{
				List<Thing> things = c.GetThingList(map);
				if (things.Count > 0)
				{
					foreach (Thing thing in things)
					{
						if (thing is Building_PlantGrower && thing.def.defName == "RPGrapheneGrowBin")
						{
							CompPowerTrader comp = (thing as Building).TryGetComp<CompPowerTrader>();
							if (comp != null && comp.PowerOn && !(thing as Building).IsBrokenDown())
							{
								__result = true;
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
