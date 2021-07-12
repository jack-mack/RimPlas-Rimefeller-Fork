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
                if (thing == __instance || thing is not Building_PlantGrower ||
                    thing.def.defName != "RPGrapheneGrowBin")
                {
                    continue;
                }

                var comp = (thing as Building).TryGetComp<CompPowerTrader>();
                if (comp == null || !comp.PowerOn || (thing as Building).IsBrokenDown())
                {
                    continue;
                }

                __result = 1f;
                return false;
            }

            return true;
        }
    }
}