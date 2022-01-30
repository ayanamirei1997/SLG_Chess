using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//拓展方法集
public static class ExtensionMethods
{
    public static List<Vector3> toPos(this List<GraphNode> list)
    {
        var n = new List<Vector3>();

        foreach (var item in list)
        {
            n.Add((Vector3)item.position);
        }

        return n;
    }

}
