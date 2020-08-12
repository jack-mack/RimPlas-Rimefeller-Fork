using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RimPlas
{
	// Token: 0x02000021 RID: 33
	public class PlaceWorker_RPThingMakerHopper : PlaceWorker
	{
		// Token: 0x060000A0 RID: 160 RVA: 0x00005E9C File Offset: 0x0000409C
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thingy = null)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c = loc + PlaceWorker_RPThingMakerHopper.NewMethod(i);
				if (c.InBounds(map))
				{
					List<Thing> thingList = c.GetThingList(map);
					for (int j = 0; j < thingList.Count; j++)
					{
						Thing thing = thingList[j];
						ThingDef thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef;
						if (thingDef != null && thingDef.building != null && thingDef.defName == "RPThingMaker" && this.IsCorrectSide(thing, c, rot))
						{
							return true;
						}
					}
				}
			}
			return "MustPlaceNextToHopperAccepter".Translate();
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x00005F4A File Offset: 0x0000414A
		private static IntVec3 NewMethod(int i)
		{
			return GenAdj.CardinalDirections[i];
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00005F58 File Offset: 0x00004158
		public bool IsCorrectSide(Thing t, IntVec3 c, Rot4 rot)
		{
			Rot4 tRot = t.Rotation;
			IntVec3 tPos = t.Position;
			if (tRot.AsInt == 0)
			{
				if (c.x > tPos.x)
				{
					return true;
				}
			}
			else if (tRot.AsInt == 1)
			{
				if (c.z >= tPos.z)
				{
					return true;
				}
			}
			else if (tRot.AsInt == 2)
			{
				if (tPos.x > c.x)
				{
					return true;
				}
			}
			else if (tRot.AsInt == 3 && tPos.z >= c.z)
			{
				return true;
			}
			return false;
		}
	}
}
