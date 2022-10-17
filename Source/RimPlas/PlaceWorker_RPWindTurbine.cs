using System.Linq;
using UnityEngine;
using Verse;

namespace RimPlas;

public class PlaceWorker_RPWindTurbine : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        GenDraw.DrawFieldEdges(RPWindTurbine_Utility.CalculateWindCells(center, rot, def.size).ToList());
    }
}