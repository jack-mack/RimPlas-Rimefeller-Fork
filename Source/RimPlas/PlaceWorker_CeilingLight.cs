using System;
using System.Collections.Generic;
using Verse;

namespace RimPlas
{
	// Token: 0x02000003 RID: 3
	public class PlaceWorker_CeilingLight : PlaceWorker
	{
		// Token: 0x06000006 RID: 6 RVA: 0x00002214 File Offset: 0x00000414
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (!loc.InBounds(map))
			{
				return false;
			}
			if (!map.roofGrid.Roofed(loc))
			{
				return false;
			}
			if (loc.Filled(map))
			{
				return false;
			}
			List<Thing> list = loc.GetThingList(map);
			if (list.Count > 0)
			{
				foreach (Thing thingy in list)
				{
					if (thingy is Building)
					{
						ThingDef def = thingy.def;
						if (((def != null) ? def.entityDefToBuild : null) != null && thingy.def.entityDefToBuild == checkingDef)
						{
							return false;
						}
						if (thingy.def.IsDoor)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
	}
}
