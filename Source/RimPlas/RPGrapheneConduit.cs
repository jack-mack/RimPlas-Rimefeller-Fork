using Verse;

namespace RimPlas
{
    // Token: 0x0200001C RID: 28
    public class RPGrapheneConduit : Building
    {
        // Token: 0x0400003A RID: 58
        private static readonly ThingDef buriedDef = ThingDef.Named("RPGraphenePowerConduit_Buried");

        // Token: 0x04000039 RID: 57
        public bool buried;

        // Token: 0x0400003B RID: 59
        private Graphic buriedGraphic;

        // Token: 0x1700000D RID: 13
        // (get) Token: 0x06000065 RID: 101 RVA: 0x00004234 File Offset: 0x00002434
        public override Graphic Graphic
        {
            get
            {
                Graphic defaultGraphic;
                if (!buried)
                {
                    defaultGraphic = DefaultGraphic;
                }
                else
                {
                    buriedGraphic = GraphicDatabase.Get(buriedDef.graphicData.graphicClass,
                        buriedDef.graphicData.texPath, buriedDef.graphicData.shaderType.Shader,
                        buriedDef.graphicData.drawSize, DrawColor, DrawColorTwo);
                    defaultGraphic = buriedGraphic;
                }

                return defaultGraphic;
            }
        }

        // Token: 0x06000066 RID: 102 RVA: 0x000042B5 File Offset: 0x000024B5
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            buried = true;
            base.SpawnSetup(map, respawningAfterLoad);
        }
    }
}