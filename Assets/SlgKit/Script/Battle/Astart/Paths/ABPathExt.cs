using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class ABPathExt : ABPath
{
    public bool canTraverseWater = false;
    public List<GraphNode> moveRangePath;
    new public static ABPathExt Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null)
    {
        ABPathExt p = PathPool<ABPathExt>.GetPath();
        p.Setup(start, end, callback);
        return p;
    }

    //约束寻路范围
    public static ABPathExt ConstructRange(Vector3 start, Vector3 end, List<GraphNode> moveRangePath, OnPathDelegate callback = null)
    {
        ABPathExt p = PathPool<ABPathExt>.GetPath();
        p.moveRangePath = moveRangePath;
        p.Setup(start, end, callback);

        return p;
    }




    public override bool CanTraverse(GraphNode node)
    {
        //搜素路径时，假如遇到水节点和路径不可遍历水则返回 节点不可能穿越
        if (node.Tag == (uint)GameDefine.AstartTag.Water && canTraverseWater == false) return false;

        //移动范围不包含该节点则代码不可穿越
        if (moveRangePath != null && !moveRangePath.Contains(node))
        {
            return false;
        }


        return base.CanTraverse(node);
    }
}
