using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas;

[HarmonyPatch(typeof(ShortCircuitUtility), "DoShortCircuit")]
public class DoShortCircuit_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(800)]
    public static bool PreFix(Building culprit)
    {
        return culprit.def.defName != "RPUndGrapheneConduit" &&
               culprit.def.defName != "RPGraphenePowerConduit_Buried" &&
               culprit.def.defName != "RPGraphenePowerConduit";
    }
}