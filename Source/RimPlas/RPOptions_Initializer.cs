using System;
using System.Collections.Generic;
using Verse;

namespace RimPlas
{
    // Token: 0x0200001F RID: 31
    [StaticConstructorOnStartup]
    internal static class RPOptions_Initializer
    {
        // Token: 0x0600007B RID: 123 RVA: 0x000046AC File Offset: 0x000028AC
        static RPOptions_Initializer()
        {
            LongEventHandler.QueueLongEvent(Setup, "LibraryStartup", false, null);
        }

        // Token: 0x0600007C RID: 124 RVA: 0x000046C8 File Offset: 0x000028C8
        public static void Setup()
        {
            var allDefs = DefDatabase<ResearchProjectDef>.AllDefsListForReading;
            if (allDefs.Count <= 0)
            {
                return;
            }

            var RPList = RPResearchList();
            foreach (var ResDef in allDefs)
            {
                if (!RPList.Contains(ResDef.defName))
                {
                    continue;
                }

                var Resbase = ResDef.baseCost;
                Resbase = checked((int) Math.Round(Resbase * Controller.Settings.ResPct / 100f));
                ResDef.baseCost = Resbase;
            }
        }

        // Token: 0x0600007D RID: 125 RVA: 0x00004760 File Offset: 0x00002960
        public static List<string> RPResearchList()
        {
            var list = new List<string>();
            list.AddDistinct("RimPlas");
            list.AddDistinct("RimPlas_Bulk");
            list.AddDistinct("RimPlas_SemiSynth");
            list.AddDistinct("RimPlas_InjMld");
            list.AddDistinct("RimPlas_AdvInjMld");
            list.AddDistinct("RimPlas_Synth");
            list.AddDistinct("RimPlas_Synth_Bulk");
            list.AddDistinct("RimPlas_Synth_Plasteel");
            list.AddDistinct("RimPlas_Synth_Plasteel_Bulk");
            list.AddDistinct("RimPlas_CarboSynth");
            list.AddDistinct("RimPlas_Graphene");
            list.AddDistinct("RimPlas_BulkGraphene");
            list.AddDistinct("RimPlas_CarbonComposites");
            list.AddDistinct("RimPlas_CarbonCompositesBulk");
            list.AddDistinct("RimPlas_Components");
            list.AddDistinct("RimPlas_AdvComponents");
            list.AddDistinct("RimPlas_MultiAnalyzer");
            list.AddDistinct("RimThermoPlas");
            list.AddDistinct("RPSecurityDoors");
            list.AddDistinct("RimPlas_AdvIntelMats");
            list.AddDistinct("Rimoprene");
            list.AddDistinct("Rimica");
            list.AddDistinct("FortRimica");
            list.AddDistinct("RPGrapheneElectroliser");
            list.AddDistinct("RPThingMaker");
            return list;
        }
    }
}