using Verse;

namespace RimPlas
{
    // Token: 0x0200000D RID: 13
    public class CompProperties_RPHospBed : CompProperties
    {
        // Token: 0x0400001C RID: 28
        public string PainCtrlHediff = "HED_RPHospBed";

        // Token: 0x0400001D RID: 29
        public float PainCtrlSev = 1f;

        // Token: 0x0400001E RID: 30
        public int PainCtrlTicks = 120;

        // Token: 0x0400001F RID: 31
        public float RecPerHour = 0.08f;

        // Token: 0x0600002F RID: 47 RVA: 0x00002FD8 File Offset: 0x000011D8
        public CompProperties_RPHospBed()
        {
            compClass = typeof(CompRPHospBed);
        }
    }
}