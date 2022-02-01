using UnityEngine;
using System.Collections;
using Pathfinding;
using System.Collections.Generic;

/// <summary>
/// 节点查找限制规则
/// </summary>
public class NNCPlayerMove : NNConstraint
{
    internal List<GraphNode> moveRangePath;
    internal List<GraphNode> playersMapNode;

    //最合适的节点查询规则
    //在移动范围之内
    public override bool Suitable(GraphNode node)
    {


      

        if ( node.Tag == (uint)GameDefine.AstartTag.Obstacel)
        {
            return false;
        }

        if (moveRangePath!=null)
        {
            //该节点 不在移动范围之内 不符合规则
         
            if (moveRangePath.Contains(node) == false) return false; 
        }

        if (playersMapNode != null)
        {
            //该节点 有玩家 不符合规则
            if (playersMapNode.Contains(node) ) return false;
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