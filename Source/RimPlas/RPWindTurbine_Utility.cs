using System.Collections.Generic;
using Verse;

namespace RimPlas;

public class RPWindTurbine_Utility
{
    internal static IEnumerable<IntVec3> CalculateWindCells(IntVec3 center, Rot4 rot, IntVec2 size)
    {
        var rectA = default(CellRect);
        var rectB = default(CellRect);
        var num = 0;
        int num2;
        int num3;
        if (rot == Rot4.North || rot == Rot4.East)
        {
            num2 = 6;
            num3 = 2;
        }
        else
        {
            num2 = 2;
            num3 = 6;
            num = -1;
        }

        if (rot.IsHorizontal)
        {
            rectA.minX = center.x + 2 + num;
            rectA.maxX = center.x + 2 + num2 + num;
            rectB.minX = center.x - 1 - num3 + num;
            rectB.maxX = center.x - 1 + num;
            rectB.minZ = rectA.minZ = center.z - 2;
            rectB.maxZ = rectA.maxZ = center.z + 2;
        }
        else
        {
            rectA.minZ = center.z + 2 + num;
            rectA.maxZ = center.z + 2 + num2 + num;
            rectB.minZ = center.z - 1 - num3 + num;
            rectB.maxZ = center.z - 1 + num;
            rectB.minX = rectA.minX = center.x - 2;
            rectB.maxX = rectA.maxX = center.x + 2;
        }

        int num4;
        for (var z2 = rectA.minZ; z2 <= rectA.maxZ; z2 = num4 + 1)
        {
            for (var x = rectA.minX; x <= rectA.maxX; x = num4 + 1)
            {
                yield return new IntVec3(x, 0, z2);
                num4 = x;
            }

            num4 = z2;
        }

        for (var z2 = rectB.minZ; z2 <= rectB.maxZ; z2 = num4 + 1)
        {
            for (var x = rectB.minX; x <= rectB.maxX; x = num4 + 1)
            {
                yield return new IntVec3(x, 0, z2);
                num4 = x;
            }

            num4 = z2;
        }
    }
}