using System;
using Verse;

namespace RimPlas
{
	// Token: 0x0200001C RID: 28
	public class RPGrapheneConduit : Building
	{
		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00004234 File Offset: 0x00002434
		public override Graphic Graphic
		{
			get
			{
				Graphic defaultGraphic;
				if (!this.buried)
				{
					defaultGraphic = base.DefaultGraphic;
				}
				else
				{
					this.buriedGraphic = GraphicDatabase.Get(RPGrapheneConduit.buriedDef.graphicData.graphicClass, RPGrapheneConduit.buriedDef.graphicData.texPath, RPGrapheneConduit.buriedDef.graphicData.shaderType.Shader, RPGrapheneConduit.buriedDef.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
					defaultGraphic = this.buriedGraphic;
				}
				return defaultGraphic;
			}
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000042B5 File Offset: 0x000024B5
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			this.buried = true;
			base.SpawnSetup(map, respawningAfterLoad);
		}

		// Token: 0x04000039 RID: 57
		public bool buried;

		// Token: 0x0400003A RID: 58
		private static ThingDef buriedDef = ThingDef.Named("RPGraphenePowerConduit_Buried");

		// Token: 0x0400003B RID: 59
		private Graphic buriedGraphic;
	}
}
