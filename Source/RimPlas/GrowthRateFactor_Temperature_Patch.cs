using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas;

[HarmonyPatch(typeof(Plant))]
[HarmonyPatch("GrowthRateFactor_Temperature", MethodType.Getter)]
public class GrowthRateFactor_Temperature_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(800)]
    public static bool PreFix(ref Plant __instance, ref float __result)
    {
        var plant = __instance;
        if (plant?.Map == null)
        {
            return true;
        }

        var map = __instance.Map;
        var things = __instance.Position.GetThingList(map);
        if (things.Count <= 0)
        {
            return true;
        }

        foreach (var thing in things)
        {
            if (thing == __instance || thing is not Building_PlantGrower grower ||
                grower.def.defName != "RPGrapheneGrowBin")
            {
                continue;
            }

            var comp = grower.TryGetComp<CompPowerTrader>();
            if (comp is not { PowerOn: true } || grower.IsBrokenDown())
            {
                continue;
            }

            __result = 1f;
            return false;
        }

        return true;
    }
}