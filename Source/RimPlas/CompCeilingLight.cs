using RimWorld;
using Verse;
using Verse.Sound;

namespace RimPlas;

public class CompCeilingLight : ThingComp
{
    public CompProperties_CeilingLight Props => (CompProperties_CeilingLight)props;

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