using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;

public class NNCPlayerMove : NNConstraint
{


    public override bool Suitable(GraphNode node)
    {


      

        if (node.Tag == (uint)GameDefine.AstartTag.playerTag || node.Tag == (uint)GameDefine.AstartTag.Obstacel)
        {
            return false;
        }



        return base.Suitable(node);
    }

   
}

public class NNCMoveAbPath : NNConstraint
{
    GraphNode p_Startnode;
    public NNCMoveAbPath(GraphNode p_start)
    {
        p_Startnode = p_start;
    }

    public override bool Suitable(GraphNode node)
    {

        if (p_Startnode == node)
        {
            return true;
        }

        if (node.Tag == (uint)GameDefine.AstartTag.playerTag || node.Tag == (uint)GameDefine.AstartTag.Obstacel)
        {
            return false;
        }

        return base.Suitable(node);
    }


}