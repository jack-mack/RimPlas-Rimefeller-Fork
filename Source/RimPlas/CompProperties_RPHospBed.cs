using Verse;

namespace RimPlas;

public class CompProperties_RPHospBed : CompProperties
{
    public string PainCtrlHediff = "HED_RPHospBed";

    public float PainCtrlSev = 1f;

    public int PainCtrlTicks = 120;

    public float RecPerHour = 0.08f;

    public CompProperties_RPHospBed()
    {
        compClass = typeof(CompRPHospBed);
    }
}