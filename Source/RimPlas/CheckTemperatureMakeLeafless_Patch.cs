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
                if (comp != null && comp.PowerOn && !(thing as Building).IsBrokenDown())
                {
                    return false;
                }
            }

            return true;
        }
    }
}