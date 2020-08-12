using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimPlas
{
	// Token: 0x0200001A RID: 26
	public class PlaceWorker_RPWindTurbine : PlaceWorker
	{
		// Token: 0x06000059 RID: 89 RVA: 0x00003BC8 File Offset: 0x00001DC8
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GenDraw.DrawFieldEdges(RPWindTurbine_Utility.CalculateWindCells(center, rot, def.size).ToList<IntVec3>());
		}
	}
}
