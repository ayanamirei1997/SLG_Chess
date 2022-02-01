using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionNode
{
    public abstract void Execute(PlayerController from, PlayerController to);


    public abstract void complete(PlayerController from, PlayerController to);
}


public class HelloWorldNode : ActionNode
{
    public override void complete(PlayerController from, PlayerController to)
    {
        Debug.Log("HelloWorldNode complete");
    }

    public override void Execute(PlayerController from, PlayerController to)
    {
        Debug.Log("HelloWorldNode Execute");
    }
}


public class Add_Attribute : ActionNode
{
    public enum AddType { addvalue, addPercentage }
    public enum ApplayTarget { owner, other }

    public AttributeKey attribute_id = 0;
    public float value = 0;
    private ApplayTarget applayTarget;

    int tem_value = 0;
    public Add_Attribute(AttributeKey id, AddType addType, ApplayTarget p_target, float p_value)
    {
        attribute_id = id;

        value = p_value;
        applayTarget = p_target;
    }

    public override void complete(PlayerController from, PlayerController to)
    {
        // from.attribute[0] += 1;
        // from.attribute[(int)AttributeKey.maxHp] += 1;

        // throw new NotImplementedException();

        PlayerController target = null;
        if (applayTarget == ApplayTarget.owner)
        {
            target = from;
        }
        else
        {
            target = to;
        }

        var attribute_Value = (int)target.attribute[(int)attribute_id];

        attribute_Value -= tem_value;
        target.attribute[(int)attribute_id] = (uint)attribute_Value;

    }

    public override void Execute(PlayerController from, PlayerController to)
    {
        //throw new NotImplementedException();
        PlayerController target = null;

        if (applayTarget == ApplayTarget.owner)
        {
            target = from;
        }
        else
        {
            target = to;
        }


        this.tem_value = Mathf.FloorToInt((float)target.attribute[(int)attribute_id] * value);
        var attribute_Value = (int)target.attribute[(int)attribute_id];
        attribute_Value += tem_value;
        target.attribute[(int)attribute_id] = (uint)attribute_Value;

    }
}

//先攻节点
public class FastAttack : ActionNode
{

    public int count = 1;
    public override void complete(PlayerController from, PlayerController to)
    {
        from.fastAttack = false;
    }

    public override void Execute(PlayerController from, PlayerController to)
    {
        //throw new NotImplementedException();
        if (count >= 1)
        {
            from.fastAttack = true;
            count -= 1;
        }
    }
}

