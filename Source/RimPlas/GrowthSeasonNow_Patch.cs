using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas;

[HarmonyPatch(typeof(PlantUtility), "GrowthSeasonNow")]
public class GrowthSeasonNow_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(800)]
    public static bool PreFix(ref bool __result, IntVec3 c, Map map, bool forSowing = false)
    {
        if (map == null)
        {
            return true;
        }

        var things = c.GetThingList(map);
        if (things.Count <= 0)
        {
            return true;
        }

        foreach (var thing in things)
        {
            if (thing is not Building_PlantGrower grower || grower.def.defName != "RPGrapheneGrowBin")
            {
                continue;
            }

            var comp = grower.TryGetComp<CompPowerTrader>();
            if (comp is not { PowerOn: true } || grower.IsBrokenDown())
            {
                continue;
            }

            __result = true;
            return false;
        }

        return true;
    }
}