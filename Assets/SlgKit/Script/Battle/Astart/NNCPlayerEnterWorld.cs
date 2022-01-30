using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Linq;

/// <summary>
/// 在基本的约束上加上格子不存在人
/// </summary>
public class NNCPlayerEnterWorld : NNConstraint
{

    public override bool Suitable(GraphNode node)
    {




        if (node.Tag==1)
        {
            return false;
        }



       return base.Suitable(node);
    }


}
