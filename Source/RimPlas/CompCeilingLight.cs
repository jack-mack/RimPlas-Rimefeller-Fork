using RimWorld;
using Verse;
using Verse.Sound;

namespace RimPlas
{
    // Token: 0x02000008 RID: 8
    public class CompCeilingLight : ThingComp
    {
        // Token: 0x17000004 RID: 4
        // (get) Token: 0x0600001F RID: 31 RVA: 0x00002BE0 File Offset: 0x00000DE0
        public CompProperties_CeilingLight Props => (CompProperties_CeilingLight) props;

        // Token: 0x06000020 RID: 32 RVA: 0x00002BF0 File Offset: 0x00000DF0
        public override void CompTickRare()
        {
            if (!parent.Spawned || parent.Position.Roofed(parent.Map))
            {
                return;
            }

            var SInfo = SoundInfo.InMap(parent);
            SoundDefOf.Roof_Collapse.PlayOneShot(SInfo);
            FilthMaker.TryMakeFilth(parent.Position, parent.Map, ThingDefOf.Filth_RubbleBuilding);
            parent.Destroy();
        }
    }
}