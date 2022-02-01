using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikingRangePath : MoveRangConStantPath
{

    public static StrikingRangePath Construct(Vector3 start, int maxGScore, OnPathDelegate callback = null)
    {
        StrikingRangePath p = PathPool<StrikingRangePath>.GetPath();

        p.Setup(start, maxGScore, callback);
        return p;
    }

    public override bool CanTraverse(GraphNode node)
    {
        
       
        //技能可以在水区域释放
        if ( node.Tag == (uint)GameDefine.AstartTag.Water)
         {
            return true;
         }

        //可以在敌人区域释放
        //if (enemy != null)
        //{
        //    if (enemy.Contains(node)) return false;
        //}
        return base.CanTraverse(node);
    }
}
