using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x02000022 RID: 34
	public class RPThingMakerUtility
	{
		// Token: 0x060000A4 RID: 164 RVA: 0x00005FE4 File Offset: 0x000041E4
		public static Texture2D GetRPThingIcon(ThingDef t)
		{
			if (t == null)
			{
				return ContentFinder<Texture2D>.Get(RPThingMakerUtility.ThingIconPath, true);
			}
			if (((t != null) ? t.uiIcon : null) != null)
			{
				return t.uiIcon;
			}
			if (t != null)
			{
				GraphicData graphicData = t.graphicData;
				if (((graphicData != null) ? graphicData.texPath : null) != null)
				{
					string texturePath = t.graphicData.texPath;
					if (t.graphicData.graphicClass.Name == "Graphic_StackCount")
					{
						texturePath = texturePath + "/" + t.defName + "_a";
					}
					return ContentFinder<Texture2D>.Get(texturePath, true);
				}
			}
			return ContentFinder<Texture2D>.Get(RPThingMakerUtility.ThingIconPath, true);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x00006088 File Offset: 0x00004288
		public static bool RCPProdValues(ThingDef t, out int ticks, out int minProd, out int maxProd, out string research)
		{
			ticks = 0;
			minProd = 0;
			maxProd = 0;
			research = "";
			if (t.defName == "ComponentIndustrial")
			{
				ticks = 1000;
				minProd = 1;
				maxProd = 1;
				research = "RimPlas_Components";
				return true;
			}
			if (t.defName == "ComponentSpacer")
			{
				ticks = 2000;
				minProd = 1;
				maxProd = 1;
				research = "RimPlas_AdvComponents";
				return true;
			}
			return false;
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000060FC File Offset: 0x000042FC
		public static List<RPThingMakerUtility.RPRCPListItem> GetRCPList(ThingDef thingdef)
		{
			List<RPThingMakerUtility.RPRCPListItem> list = new List<RPThingMakerUtility.RPRCPListItem>();
			list.Clear();
			RPThingMakerUtility.RPRCPListItem item = default(RPThingMakerUtility.RPRCPListItem);
			if (thingdef.defName == "ComponentIndustrial")
			{
				item.def = DefDatabase<ThingDef>.GetNamed("RimPoly", true);
				item.mixgrp = 1;
				item.num = 10;
				item.ratio = 1f;
				list.Add(item);
				item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_ElectroSteel_Wool", true);
				item.mixgrp = 2;
				item.num = 2;
				item.ratio = 1f;
				list.Add(item);
			}
			else if (thingdef.defName == "ComponentSpacer")
			{
				item.def = DefDatabase<ThingDef>.GetNamed("RimPoly", true);
				item.mixgrp = 1;
				item.num = 30;
				item.ratio = 1f;
				list.Add(item);
				item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_ElectroSteel_Wool", true);
				item.mixgrp = 2;
				item.num = 12;
				item.ratio = 1f;
				list.Add(item);
				item.def = DefDatabase<ThingDef>.GetNamed("RimPlas_Gold_Wool", true);
				item.mixgrp = 3;
				item.num = 3;
				item.ratio = 1f;
				list.Add(item);
			}
			return list;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x00006256 File Offset: 0x00004456
		public static List<string> GetMakeList()
		{
			return new List<string>
			{
				"ComponentIndustrial",
				"ComponentSpacer"
			};
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00006274 File Offset: 0x00004474
		public static List<int> GetMaxStock()
		{
			return new List<int>
			{
				25,
				50,
				75,
				100,
				150,
				200,
				250,
				300,
				400,
				500,
				750,
				1000,
				0
			};
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00006308 File Offset: 0x00004508
		public static int StringToInt(string ToConvert)
		{
			int Value;
			if (ToConvert != "No Limit" && int.TryParse(ToConvert, out Value))
			{
				return Value;
			}
			return 0;
		}

		// Token: 0x04000051 RID: 81
		public const string RCP_RPoly = "RimPoly";

		// Token: 0x04000052 RID: 82
		public const string RCP_EWool = "RimPlas_ElectroSteel_Wool";

		// Token: 0x04000053 RID: 83
		public const string RCP_GWool = "RimPlas_Gold_Wool";

		// Token: 0x04000054 RID: 84
		public const string RCP_PWool = "RimPlas_Plasteel_Wool";

		// Token: 0x04000055 RID: 85
		[NoTranslate]
		public static string ThingIconPath = "Things/Building/Misc/RPThingMaker/UI/RPThingMaker_ThingIcon";

		// Token: 0x0200002E RID: 46
		public struct RPRCPListItem
		{
			// Token: 0x04000086 RID: 134
			internal ThingDef def;

			// Token: 0x04000087 RID: 135
			internal bool mixed;

			// Token: 0x04000088 RID: 136
			internal int mixgrp;

			// Token: 0x04000089 RID: 137
			internal int num;

			// Token: 0x0400008A RID: 138
			internal float ratio;
		}
	}
}
