using HarmonyLib;
using RimWorld;
using Verse;

namespace RimPlas
{
    // Token: 0x02000013 RID: 19
    [HarmonyPatch(typeof(ShortCircuitUtility), "DoShortCircuit")]
    public class DoShortCircuit_Patch
    {
        // Token: 0x06000044 RID: 68 RVA: 0x000034F0 File Offset: 0x000016F0
        [HarmonyPrefix]
        [HarmonyPriority(800)]
        public static bool PreFix(Building culprit)
        {
            return culprit.def.defName != "RPUndGrapheneConduit" &&
                   culprit.def.defName != "RPGraphenePowerConduit_Buried" &&
                   culprit.def.defName != "RPGraphenePowerConduit";
        }
    }
}