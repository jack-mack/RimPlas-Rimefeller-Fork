using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace RimPlas
{
    // Token: 0x02000018 RID: 24
    [StaticConstructorOnStartup]
    internal static class MultiplayerSupport
    {
        // Token: 0x04000025 RID: 37
        private static readonly Harmony harmony = new Harmony("rimworld.pelador.rimplas.multiplayersupport");

        // Token: 0x06000050 RID: 80 RVA: 0x000038C0 File Offset: 0x00001AC0
        static MultiplayerSupport()
        {
            if (!MP.enabled)
            {
                return;
            }

            MP.RegisterSyncMethod(typeof(RPGraphene_Vent), "ToggleUseFixed");
            MP.RegisterSyncMethod(typeof(Building_TempRegulator), "ToggleUseBoost");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "RPMakerSelectThing");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "SetProdControlValues");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "ToggleProducing");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "RPMakerSelectLimit");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "SetStockLimits");
            MP.RegisterSyncMethod(typeof(Building_RPThingMaker), "ToggleDebug");
            MethodInfo[] array =
            {
                AccessTools.Method(typeof(Building_RPGrapheneBatterySmall), "Tick"),
                AccessTools.Method(typeof(Building_RPGrapheneBatterySmall), "PostApplyDamage"),
                AccessTools.Method(typeof(Globals), "DoSecSpecialEffects")
            };
            foreach (var methodInfo in array)
            {
                FixRNG(methodInfo);
            }
        }

        // Token: 0x06000051 RID: 81 RVA: 0x000039FC File Offset: 0x00001BFC
        private static void FixRNG(MethodInfo method)
        {
            harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
                new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
        }

        // Token: 0x06000052 RID: 82 RVA: 0x00003A36 File Offset: 0x00001C36
        private static void FixRNGPre()
        {
            Rand.PushState(Find.TickManager.TicksAbs);
        }

        // Token: 0x06000053 RID: 83 RVA: 0x00003A47 File Offset: 0x00001C47
        private static void FixRNGPos()
        {
            Rand.PopState();
        }
    }
}