using Verse;

namespace RimPlas;

public class CompProperties_BuildHeal : CompProperties
{
    public readonly float HealFactor = 1f;

    public readonly bool PowerNeeded = true;

    public CompProperties_BuildHeal()
    {
        compClass = typeof(CompBuildHeal);
    }
}