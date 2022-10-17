using System.Reflection;
using HarmonyLib;
using Verse;

namespace RimPlas;

[StaticConstructorOnStartup]
internal static class HarmonyPatching
{
    static HarmonyPatching()
    {
        new Harmony("com.Pelador.Rimworld.RimPlas").PatchAll(Assembly.GetExecutingAssembly());
    }
}