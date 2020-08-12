using System;
using Verse;

namespace RimPlas
{
	// Token: 0x0200000F RID: 15
	public class CompProperties_BuildHeal : CompProperties
	{
		// Token: 0x06000038 RID: 56 RVA: 0x00003226 File Offset: 0x00001426
		public CompProperties_BuildHeal()
		{
			this.compClass = typeof(CompBuildHeal);
		}

		// Token: 0x04000020 RID: 32
		public float HealFactor = 1f;

		// Token: 0x04000021 RID: 33
		public bool PowerNeeded = true;
	}
}
