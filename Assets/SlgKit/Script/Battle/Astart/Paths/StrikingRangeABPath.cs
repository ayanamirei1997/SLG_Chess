using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikingRangeABPath : ABPath
{
    public override bool CanTraverse(GraphNode node)
    {
        if (node.Tag == (uint)GameDefine.AstartTag.Obstacel)
        {
            return false;
        }

        return base.CanTraverse(node);
    }
}
