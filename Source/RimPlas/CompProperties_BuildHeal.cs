using Verse;

namespace RimPlas;

public class CompProperties_BuildHeal : CompProperties
{
    public float HealFactor = 1f;

    public bool PowerNeeded = true;

    public CompProperties_BuildHeal()
    {
        compClass = typeof(CompBuildHeal);
    }
}