using Verse;

namespace RimPlas
{
    // Token: 0x0200000C RID: 12
    public class CompProperties_SecurityReact : CompProperties
    {
        // Token: 0x04000019 RID: 25
        public float radius;

        // Token: 0x0400001A RID: 26
        public string SecurityHediff;

        // Token: 0x0400001B RID: 27
        public float SecuritySeverity;

        // Token: 0x0600002E RID: 46 RVA: 0x00002FC0 File Offset: 0x000011C0
        public CompProperties_SecurityReact()
        {
            compClass = typeof(CompSecurityReact);
        }
    }
}