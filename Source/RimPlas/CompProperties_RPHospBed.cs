using Verse;

namespace RimPlas;

public class CompProperties_RPHospBed : CompProperties
{
    public readonly string PainCtrlHediff = "HED_RPHospBed";

    public readonly float PainCtrlSev = 1f;

    public readonly int PainCtrlTicks = 120;

    public readonly float RecPerHour = 0.08f;

    public CompProperties_RPHospBed()
    {
        compClass = typeof(CompRPHospBed);
    }
}