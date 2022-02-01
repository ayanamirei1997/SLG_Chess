using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ExecuteTiming {
    FightingStart,
    FightingEnd
}
public class SkillState
{
    public ExecuteTiming timing;
    public List<ActionNode> actionNodes = new List<ActionNode>();

    internal void ExecuteAll(PlayerController from, PlayerController to)
    {
        //throw new NotImplementedException();
        foreach (ActionNode item in actionNodes)
        {
            item.Execute(from, to);
        }

       
    }

    internal void completeAll(PlayerController from, PlayerController to)
    {
        // throw new NotImplementedException();

        foreach (ActionNode item in actionNodes)
        {
            item.complete(from, to);
        }
    }
}

public class Skill
{
    internal string name;

    List<SkillState> skillAction = new List<SkillState>();
    internal SkillState addState()
    {
        //throw new NotImplementedException();
        var n = new SkillState();
        skillAction.Add(n);
        return n;
    }

    internal List<SkillState> FindNodes(ExecuteTiming executeTiming)
    {
        //throw new NotImplementedException();
        return skillAction.FindAll(s => s.timing == executeTiming);
    }
}




public class OverNode : ActionNode
{

    ActionNode actionNode;
    public OverNode(ActionNode n)
    {
        actionNode = n;
    }
    public override void complete(PlayerController from, PlayerController to)
    {
        actionNode.complete(from, to);
    }

    public override void Execute(PlayerController from, PlayerController to)
    {

    }
}



public class Skill_ID
{
    public readonly static uint xijia=2;
    internal static uint kedixianji=3;
}


public class SkillSys :Singleton<SkillSys>
{
    internal void Init()
    {
        //throw new NotImplementedException();
        EventDispatcher.instance.Regist<PlayerController>(GameEventType.playerInitSkill, this.playerInitSkill);

        EventDispatcher.instance.Regist<PlayerController, PlayerController>(GameEventType.battle_Start, this.battle_Start);

        EventDispatcher.instance.Regist<PlayerController, PlayerController>(GameEventType.battle_End, this.battle_End);

        SetSkillCfg();
    }

    private void battle_End(PlayerController from, PlayerController to)
    {
        //  throw new NotImplementedException();
        //攻击者  执行犀甲 的完毕 功能
        var states1 = GetSkillState(from, ExecuteTiming.FightingEnd);
        foreach (SkillState state in states1)
        {
            state.completeAll(from, to);
        }


        //被攻击者 执行犀甲 的完毕 功能
        var states2 = GetSkillState(to, ExecuteTiming.FightingEnd);
        foreach (SkillState state in states2)
        {
            state.completeAll(to, from);
        }

    }

    private void battle_Start(PlayerController from, PlayerController to)
    {
        //throw new NotImplementedException();

        //战斗前 双方执行 被动技能的 节点逻辑

        //以犀甲为例子
        //主动攻击者 执行犀甲 的功能
        var states1 = GetSkillState(from, ExecuteTiming.FightingStart);
        foreach (SkillState state in states1)
        {
            state.ExecuteAll(from, to);
        }

        //考虑对方也存在被动技能
        var states2 = GetSkillState(to, ExecuteTiming.FightingStart);
        foreach (SkillState state in states2)
        {
            state.ExecuteAll(to, from);
        }




    }

    private List<SkillState> GetSkillState(PlayerController player, ExecuteTiming executeTiming)
    {
        //throw new NotImplementedException();
        //查找开始战斗执行时机的技能状态
        List<SkillState> n = new List<SkillState>();
        foreach (Skill skill in player.skill)
        {
            if (skill != null)
            {
                n.AddRange(skill.FindNodes(executeTiming));
            }
        }
        return n;
    }

    private void playerInitSkill(PlayerController player)
    {
       // throw new NotImplementedException();

           for (int i = 0; i < player.skillcfg.Length; i++)
        {
            var id = player.skillcfg[i];
            player.skill[i]=GetSkillByCfg(id);
         
        }

    }

    private Skill GetSkillByCfg(uint id)
    {
        if (!skillMap.ContainsKey(id)) return null;

        return skillMap[id];
    }

    Dictionary<uint, Skill> skillMap = new Dictionary<uint, Skill>();
    /// <summary>
    /// 生成技能模板
    /// </summary>
    void SetSkillCfg()
    {
        //犀甲 
        //id=2
        //触发时机  对战中 
        //作用 敌人防御减少 7% 自己增加14%的防御

        var skill = new Skill();
        skill.name = "犀甲 ";

        //定义一个战斗开始状态
        SkillState start_state = skill.addState();
        start_state.timing = ExecuteTiming.FightingStart;

        //var todo = new HelloWorldNode();
        //start_state.actionNodes.Add(todo);

        //为技能的拥有者 增加 防御的 百分比 7%
        var add_node = new Add_Attribute(AttributeKey.def, Add_Attribute.AddType.addPercentage, Add_Attribute.ApplayTarget.owner, 0.14f);
        start_state.actionNodes.Add(add_node);

        var sub_node = new Add_Attribute(AttributeKey.def, Add_Attribute.AddType.addPercentage, Add_Attribute.ApplayTarget.other, -0.07f);
        start_state.actionNodes.Add(sub_node);



        //定义一个结束状态
        var over_state = skill.addState();
        over_state.timing = ExecuteTiming.FightingEnd;
        var over = new OverNode(sub_node);
        over_state.actionNodes.Add(over);

        var over2 = new OverNode(add_node);
        over_state.actionNodes.Add(over2);


        skillMap.Add(Skill_ID.xijia, skill);


        //克敌先机
        //遭受近战[攻击] 对战开始时 防御+7%
        //遭受近战[攻击] 发动 [先攻] , 每回合一次

        skill = new Skill();
        skill.name = "克敌先机 ";

        //触发条件  遭受近战

        //定义一个战斗开始状态
        start_state = skill.addState();
        start_state.timing = ExecuteTiming.FightingStart;

        //触发条件->遭受近战[攻击]
        var condition = new AttackedByMelee_Condition();


        //先攻节点
        var fastAttackNode = new FastAttack();

        //成功后执行 执行执行先攻
        condition.successTodoNodes.Add(fastAttackNode);
        condition.successTodoNodes.Add(add_node);

        start_state.actionNodes.Add(condition);
       // start_state.actionNodes.Add(add_node);



        //战斗结束时 移除状态
        var end_state = skill.addState();
        end_state.timing = ExecuteTiming.FightingEnd;

        //执行compelet 去除加防状态
        end_state.actionNodes.Add(condition);

        skillMap.Add(Skill_ID.kedixianji, skill);






    }
}
