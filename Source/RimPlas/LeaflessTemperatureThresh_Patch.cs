using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas;

[HarmonyPatch(typeof(Plant), "LeaflessTemperatureThresh", MethodType.Getter)]
public class LeaflessTemperatureThresh_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(800)]
    public static bool PreFix(ref Plant __instance)
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
            if (comp is { PowerOn: true } && !grower.IsBrokenDown())
            {
                return false;
            }
        }

        return true;
    }
}