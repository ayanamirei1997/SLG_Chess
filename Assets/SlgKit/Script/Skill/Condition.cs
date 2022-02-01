using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ConditionNode : ActionNode
{
    public abstract bool condition(PlayerController from, PlayerController to);
}

//遭受近战攻击
public class AttackedByMelee_Condition : ConditionNode
{
    public List<ActionNode> successTodoNodes=new List<ActionNode>();

    bool success = false;

    public override void Execute(PlayerController from, PlayerController to)
    {
        success = condition(from, to);

        if (success)
            foreach (var item in successTodoNodes)
            {
                item.Execute(from, to);
            }
          
    }

    public override void complete(PlayerController from, PlayerController to)
    {
        if (success)
            foreach (var item in successTodoNodes)
            {
                item.complete(from, to);
            }


        success = false;
    }

    public override bool condition(PlayerController from, PlayerController to)
    {
        //form 表达自己 to 代表对手
        //遭受 近战攻击 = 对手的攻击距离为1
        return to.attribute.striking_Range_Max == 1;

    }


}
