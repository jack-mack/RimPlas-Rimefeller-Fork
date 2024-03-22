using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
internal static class MultiplayerSupport
{
    private static readonly Harmony harmony = new Harmony("rimworld.pelador.rimplas.multiplayersupport");

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
        [
            AccessTools.Method(typeof(Building_RPGrapheneBatterySmall), "Tick"),
            AccessTools.Method(typeof(Building_RPGrapheneBatterySmall), "PostApplyDamage"),
            AccessTools.Method(typeof(Globals), "DoSecSpecialEffects")
        ];
        foreach (var methodInfo in array)
        {
            FixRNG(methodInfo);
        }
    }

    private static void FixRNG(MethodInfo method)
    {
        harmony.Patch(method, new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPre"),
            new HarmonyMethod(typeof(MultiplayerSupport), "FixRNGPos"));
    }

    private static void FixRNGPre()
    {
        Rand.PushState(Find.TickManager.TicksAbs);
    }

    private static void FixRNGPos()
    {
        Rand.PopState();
    }
}