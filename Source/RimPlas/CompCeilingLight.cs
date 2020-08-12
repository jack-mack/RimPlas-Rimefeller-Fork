using System;
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
		public CompProperties_CeilingLight Props
		{
			get
			{
				return (CompProperties_CeilingLight)this.props;
			}
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002BF0 File Offset: 0x00000DF0
		public override void CompTickRare()
		{
			if (this.parent.Spawned && !this.parent.Position.Roofed(this.parent.Map))
			{
				SoundInfo SInfo = SoundInfo.InMap(this.parent, MaintenanceType.None);
				SoundDefOf.Roof_Collapse.PlayOneShot(SInfo);
				FilthMaker.TryMakeFilth(this.parent.Position, this.parent.Map, ThingDefOf.Filth_RubbleBuilding, 1, FilthSourceFlags.None);
				this.parent.Destroy(DestroyMode.Vanish);
			}
		}
	}
}
