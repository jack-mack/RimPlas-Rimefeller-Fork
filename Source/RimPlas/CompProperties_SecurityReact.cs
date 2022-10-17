using Verse;

namespace RimPlas;

public class CompProperties_SecurityReact : CompProperties
{
    public float radius;

    public string SecurityHediff;

    public float SecuritySeverity;

    public CompProperties_SecurityReact()
    {
        compClass = typeof(CompSecurityReact);
    }
}