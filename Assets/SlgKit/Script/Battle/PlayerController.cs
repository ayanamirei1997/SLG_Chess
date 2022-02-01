using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameDefine;

public class PlayerController : MonoBehaviour
{
    public int moveRange = 4;
    public bool canTraverseWater = false;
    public Sect sect;
    Animator animator;
    public Attribute attribute;
    public Int3 mapPos
    {
        get
        {
            return AstarPath.active.GetNearest((Vector3)this.transform.position).node.position;
        }
    }
    public GraphNode mapNode
    {
        get
        {
            return AstarPath.active.GetNearest((Vector3)this.transform.position).node;
        }
    }
    public Transform target;
    private Vector3 startMovePos;
    internal Int3? goMapPos;
    private List<GraphNode> moveRangePath;
    private List<GraphNode> strikingRange;
    public float moveSpeed;
    private Coroutine moveCor;
    private List<GraphNode> normalStrikingRangen = new List<GraphNode>();
    public PlayerSate state;
    private PlayerController attackTarget;

    void worldPos2MapPos()
    {

        this.transform.position = (Vector3)this.mapPos;


    }


    void Start()
    {
        // StartCoroutine(TestUpdate());

        //  StartCoroutine(TestUpdateAB_path());

        animator = GetComponent<Animator>();

        worldPos2MapPos();

        EventDispatcher.instance.DispatchEvent<PlayerController>(GameEventType.playerInitSkill, this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator TestUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            ShowMoveRange();
        }
    }

    internal void Ready()
    {
        this.startMovePos = this.transform.position;

        animator.CrossFade("ready", 0.2f);
    }

    IEnumerator TestUpdateAB_path()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            ShowAB_Path();
        }
    }

    [ContextMenu("显示范围")]
    public void ShowMoveRange()
    {
        GetMovePath((Path movePath) =>
        {
            //GridMeshManager.Instance.ShowPath(movePath.path);
            moveRangePath = movePath.path;

            this.strikingRange = new List<GraphNode>();
            var progress = 0;

            var otherPlayers = GameCtrl.instance.GetPlayersMapNode();
            otherPlayers.Remove(this.mapNode);
            foreach (var node in movePath.path)
            {
                //在移动范围预查找攻击范围时，对有人物占位的节点进行忽略
                if (otherPlayers.Contains(node))
                {
                    progress += 1;
                    continue;
                }


                GetStrikingRange(node, attribute.striking_Range_Min, attribute.striking_Range_Max, (Path strPath) =>
                {

                    foreach (var pNode in strPath.path)
                    {
                        if (!strikingRange.Contains(pNode))
                            strikingRange.Add(pNode);
                    }

                    progress += 1;

                    if (progress >= movePath.path.Count)
                    {
                        //计算完成
                        //显示范围=攻击范围 减去 移动范围
                        strikingRange = strikingRange.FindAll(t => !movePath.path.Contains(t));
                        GridMeshManager.Instance.ShowPath(movePath.path);
                        GridMeshManager.Instance.StrRangePath(strikingRange);
                    }


                });
            }

        }
        );

        normalStrikingRangen.Clear();

        GetStrikingRange(this.mapNode, attribute.striking_Range_Min, attribute.striking_Range_Max, (r_path) => {
            normalStrikingRangen.AddRange(r_path.path);
        });
    }

    internal void MoveAttack(PlayerController target)
    {
        // throw new NotImplementedException();

        state = GameDefine.PlayerSate.moveAttack;
        if (this.moveRangePath == null) return;
        attackTarget = target;
        e_moveStop -= MoveAnimaStop2Attack;
        e_moveStop += MoveAnimaStop2Attack;



        StartCoroutine(GetMove2EnemyPath(this.transform.position, target.transform.position, this.attribute.striking_Range_Min, (ab_Path) =>
        {


            //为了能在vs里断点调试该方法
            this.OnAbPathCompelet(ab_Path);
            this.goMapPos = target.mapPos;

        }));
    }

    private void MoveAnimaStop2Attack()
    {
        this.Active_Attack(attackTarget);
    }

    public IEnumerator GetMove2EnemyPath(Vector3 p_start, Vector3 end, uint minRange, System.Action<ABPath> v_path)
    {

        var minLength = ((int)minRange + 1) * 1000 * 3;
        //以敌人为中心生成最小攻击范围
        var unwalkePath = StrikingRangePath.Construct(end, minLength);
        AstarPath.StartPath(unwalkePath, true);

        yield return StartCoroutine(unwalkePath.WaitForPath());

        var otherPlayersMapNode = GameCtrl.instance.GetPlayersMapNode();
        otherPlayersMapNode.Remove(this.mapNode);
        var serchNode = this.moveRangePath.FindAll(s => !unwalkePath.allNodes.Contains(s) && !otherPlayersMapNode.Contains(s));

        //约束规则
        var nnc = new NNCPlayerMove();
        //符合条件的节点必须满足
        //1 在移动范围之内
        // 2 不能有玩家
        nnc.moveRangePath = serchNode;
        nnc.playersMapNode = otherPlayersMapNode;
        //返回的结果是在移动路径上距离敌人的坐标

        var p_Node = AstarPath.active.GetNearest(end, nnc).node;

        Vector3 p_endpos = (Vector3)p_Node.position;

        /* Test
         var testView = new List<GraphNode>();
         testView.Add(p_Node);
         GridMeshManager.Instance.ShowPathWhite(testView);
         return;
         */
        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos, this.moveRangePath,
             (Path path) =>
             {
                 ABPathExt m_path = path as ABPathExt;

                 v_path(m_path);

             }
             );
        mPath.canTraverseWater = this.canTraverseWater;

        AstarPath.StartPath(mPath, true);
    }




    internal bool CanMoveAttack(PlayerController hitPlayer)
    {
        // throw new NotImplementedException();
        return this.strikingRange.Contains(hitPlayer.mapNode);
    }

   

    internal bool InAttackRang(PlayerController hitPlayer)
    {
        // throw new NotImplementedException();
        return normalStrikingRangen.Contains(hitPlayer.mapNode);
    }

    public void CancelMove()
    {
        if (state == PlayerSate.wait) return;

        if (moveCor != null) StopCoroutine(moveCor);

        this.transform.position = this.startMovePos;

        this.animator.CrossFade("idle", 0.2f);

        worldPos2MapPos();

        this.goMapPos = null;
    }


    internal void Move(GraphNode hitNode)
    {
        if (this.moveRangePath == null) return;
        //进行寻路计算
        GetMoveABPathCallback(this.transform.position, (Vector3)hitNode.position, (ab_Path) =>
        {

            if (ab_Path.path.Count == 0)
            {
                this.CancelMove();
                return;
            }
            //为了能在vs里断点调试该方法
            this.OnAbPathCompelet(ab_Path);
            this.goMapPos = hitNode.position;

        });
    }

    void OnAbPathCompelet(ABPath ab_path)
    {

        GridMeshManager.Instance.ShowPathWhite(ab_path.path);
        MoveAnimation(ab_path.path.toPos());
    }

    void MoveAnimation(List<Vector3> pos)
    {

        this.animator.CrossFade("run", 0.2f);

        if (moveCor != null) StopCoroutine(moveCor);

        moveCor = StartCoroutine(this.CMoveUpdate(pos, this.transform));
    }

    //l=路程，s=速度,t3=两点坐标的插值
    //t1 = (l)s
    //t2=(l-s)s
    //t3 = 1 - t1t2
    //插值移动
    IEnumerator CMoveUpdate(List<Vector3> pos, Transform transform)
    {
        var _position = transform.position;
        var index = 0;
        //理解为 计算路径的插值坐标
        //Path.lerp(0)=开始坐标
        //Path.lerp(1)=结束坐标
        while (true)
        {
            //每帧执行
            yield return new WaitForEndOfFrame();
            if (index == pos.Count)
            {

                break;
                //完成
            }

            var girdPos = pos[index];
            var _finalPosition = new Vector3(girdPos.x, girdPos.y, girdPos.z);
            //因为是每帧执行，帧数的高低会由手机的性能决定
            //为了让不同性能的硬件保持移动速度一致，需要速度乘以两帧之间的渲染间隔
            //假如不加的话性能高的机器则很快移动完毕，性能低机器则反之
            var speed = this.moveSpeed * Time.deltaTime;
            var curdistance = Vector3.Distance(_position, _finalPosition);
            var remaining_distance = curdistance - speed;
            var t3 = 1f;
            if (remaining_distance <= 0)
            {
                remaining_distance = 0;
                t3 = 1;
            }
            else
            {
                var t1 = curdistance / speed;
                var t2 = remaining_distance / speed;
                t3 = 1 - t2 / t1;
            }


            if (t3 == 1)
            {

                index += 1;
            }
            var outpos = Vector3.Lerp(_position, _finalPosition, t3);

            _position = outpos;


            var orgQua = transform.rotation;

            transform.LookAt(outpos);

            var newQua = transform.rotation;

            transform.rotation = Quaternion.Lerp(orgQua, newQua, 0.3f);

            transform.position = outpos;


        }

        MoveStop();
    }
    System.Action e_moveStop;
    internal Image hpImage;
    internal Transform hpImageTrs;
    internal int viewHp;

    public uint[] skillcfg = new uint[4];
    public Skill[] skill = new Skill[4];
    public bool fastAttack;

    void MoveStop()
    {
        this.animator.CrossFade("idle", 0.2f);

        //考虑到项目后期 会有很多功能会在这个时机 进行执行
        //比如 停止移动后显示UI
        //停止之后 播放声音
        //停止之后 .........

        //为了进行代码的松耦合，这里我们要使用委托或事件

        if (e_moveStop != null) e_moveStop.Invoke();


    }


    /// <summary>
    /// 获取移动路径
    /// </summary>
    /// <param name="OnPathSerchOkCallBack"></param>
    public void GetMovePath(System.Action<Path> OnPathSerchOkCallBack)
    {
        var moveGScore = this.moveRange * 1000 * 3;

        List<PlayerController> enemy = GameCtrl.instance.GetEnemy(this.sect);

        var enemyNodes = enemy.ToGraphNode();

        //  var SerchPath = MoveRangConStantPath.Construct(this.transform.position, moveGScore, canTraverseWater,

        var SerchPath = MoveRangConStantPath.ConstructEnemy(this.transform.position, moveGScore, canTraverseWater, enemyNodes,

        (Path path) =>
        {
            path.path = (path as MoveRangConStantPath).allNodes;
            OnPathSerchOkCallBack.Invoke(path);

        }

        );
        //异步返回搜索结果
        AstarPath.StartPath(SerchPath, true);
    }









    void ShowAB_Path()
    {
        GetMoveABPathCallback(this.transform.position, target.position, (ABPath path) =>
        {
            GridMeshManager.Instance.ShowPath(path.path);
        });

    }

    /// <summary>
    /// 获取两个点之间行走路径
    /// </summary>
    /// <param name="position"></param>
    public void GetMoveABPathCallback(Vector3 p_start, Vector3 end, System.Action<ABPath> v_path)
    {
        Vector3 p_endpos = (Vector3)AstarPath.active.GetNearest(end, new NNCPlayerMove()).node.position;
        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos, this.moveRangePath,
             (Path path) =>
             {
                 ABPathExt m_path = path as ABPathExt;

                 v_path(m_path);

             }
             );
        mPath.canTraverseWater = this.canTraverseWater;
        var p_startNode = AstarPath.active.GetNearest(p_start).node;
        mPath.nnConstraint = new NNCMoveAbPath(p_startNode);

        AstarPath.StartPath(mPath, true);

    }




    /// <summary>
    /// 获取攻击距离
    /// </summary>
    public void GetStrikingRange(GraphNode node, uint minRange, uint maxRange, System.Action<Path> OnPathSerchOkCallBack)
    {
        var moveGScore = ((int)this.attribute.striking_Range_Max + 1) * 1000 * 3;

        var SerchPath = StrikingRangePath.Construct((Vector3)node.position, moveGScore,


        //搜索的最大攻击范围
        (Path path) =>
        {
            // path.path = (path as StrikingRangePath).allNodes;
            // OnPathSerchOkCallBack.Invoke(path);
            //搜索最小攻击范围
            this._GetStrikingRange_min(path, node, minRange, OnPathSerchOkCallBack);
        }

        );
        //异步返回搜索结果
        AstarPath.StartPath(SerchPath, true);
    }

    private void _GetStrikingRange_min(Path maxPath, GraphNode node, uint minRange, System.Action<Path> OnPathSerchOkCallBack)
    {
        var minLength = ((int)minRange + 1) * 1000 * 3;

        maxPath.path = (maxPath as StrikingRangePath).allNodes;

        var minPath = StrikingRangePath.Construct((Vector3)node.position, minLength,
        (Path p_minPath) =>
        {

            p_minPath.path = (p_minPath as StrikingRangePath).allNodes;
            // OnPathSerchOkCallBack.Invoke(path);
            //大范围减去小范围== 两个集合的差集
            var resulePath = new StrikingRangePath();
            resulePath.path = maxPath.path.FindAll(s => !p_minPath.path.Contains(s));

            OnPathSerchOkCallBack.Invoke(resulePath);
        });

        AstarPath.StartPath(minPath, true);
    }





    [ContextMenu("显示射程")]
    void _TestShowStrikingRange()
    {
        GetStrikingRange(this.mapNode, attribute.striking_Range_Min, attribute.striking_Range_Max, (r_path) =>
        {

            GridMeshManager.Instance.ShowPathWhite(r_path.path);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="targetCanStrikeBack">是否能还击</param>
    internal void AttackByOrder(PlayerController target, bool targetCanStrikeBack)
    {

       // if (targetCanStrikeBack)
       // EventDispatcher.instance.DispatchEvent<PlayerController, PlayerController>(GameEventType.battle_Start,this, target);

        // throw new NotImplementedException();

        this.viewHp = (int)this.attribute.hp;

        //this.lookatTarget(target);

        uint dps = this.attribute.atk - target.attribute.def;
        // target.attribute.hp -= dps;
        //防止数值溢出
        if (target.attribute.hp <= dps)
        {
            target.attribute.hp = 0;
        }
        else
        {
            target.attribute.hp -= dps;
        }



        Debug.Log("攻击了敌人 造成了" + dps + "点伤害");
        GameCtrl.instance.ReleaseSelect();

         float[] hitFrames = new float[2] { 0.12f, 0.55f };
        //动作值，总和为1 动作值越大 显示的伤害越高，
        //比如 平砍 和 跳砍 因为跳砍的力量更大，所以显示的伤害越高,具体怎么分配还要看策划设定
         int[] dpsView = CalculateDps((int)dps, new float[] { 0.8f, 0.2f });

        StartCoroutine(AnimationAttack("attack01", hitFrames, dpsView, targetCanStrikeBack, target));
    }


    //主动攻击
    public void Active_Attack(PlayerController target)
    {

        EventDispatcher.instance.DispatchEvent<PlayerController, PlayerController>(GameEventType.battle_Start, this, target);

        var orderList = CalculateAttackOrder(this, target);

        //为了让叠加加成计算准确,数值的加成效果在此基础上计算
        //this.battleAttribute = this.attribute.Clone();
        //target.battleAttribute = target.attribute.Clone();

        this.lookatTarget(target);
        target.lookatTarget(this);

        if (orderList[0] != this)
        {
            //对手先攻
            UICtrl.instance.ShowFastAttack(orderList[0]);

            StartCoroutine(c_Active_Attack_Step02(orderList[0], orderList[1]));

            // this.AttackByOrder(target, true);


        }
        else
        {
            this.AttackByOrder(target, true);
        }
    }

    IEnumerator c_Active_Attack_Step02(PlayerController from, PlayerController to)
    {
        from.animator.CrossFade("ready", 0.2f);
        //  yield return new (0.2f);
        float length = getAnimationlength("ready");
        yield return new WaitForSeconds(length);

        from.AttackByOrder(to, true);
    }

    //需要注意的是并非状态机，而是动画资源的名字
    float getAnimationlength(string name)
    {
        var clips = this.animator.runtimeAnimatorController.animationClips;

        foreach (var item in clips)
        {
            if (item.name == name) return item.length;
        }

        return -1;
    }



    private PlayerController[] CalculateAttackOrder(PlayerController from, PlayerController to)
    {
        // throw new NotImplementedException();
        PlayerController[] p = new PlayerController[2];
        if (to.fastAttack)
        {
            p[0] = to;
            p[1] = from;
        }
        else
        {
            p[0] = from;
            p[1] = to;
        }


        return p;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stateName"></param>
    /// <param name="frames"></param>
    /// <param name="dpsView"></param>
    /// <param name="targetCanStrikeBack">是否能还击</param>
    /// <param name="StrikeDis">攻击距离</param>
    /// <param name="damageTarget"></param>
    /// <returns></returns>
    IEnumerator AnimationAttack(string stateName, float[] frames, int[] dpsView, bool targetCanStrikeBack, PlayerController damageTarget)
    {



        this.animator.CrossFade(stateName, 0.2f);
        //int curFrame = 0;
        for (int i = 0; i < frames.Length; i++)
        {
            var waitframes = frames[i];
            // for (int j = 0; j < waitframes; j++)
            while (true)
            {
                var info = this.animator.GetCurrentAnimatorStateInfo(0);
                yield return new WaitForEndOfFrame();
                if (info.IsName(stateName) && info.normalizedTime >= waitframes) break;

            }

            //输出动动作伤害数值
            Debug.Log("动画 伤害" + dpsView[i]);
            damageTarget.Damage_Ani();
            //UICtrl.instance.SpwanDamage(dpsView[i], damageTarget.transform.position+Vector3.up*4);
            EventDispatcher.instance.DispatchEvent<int, Vector3>(GameEventType.showHudDamage, dpsView[i], this.transform.position + Vector3.up * 4);
            //血条更新
            damageTarget.viewHp -= dpsView[i];
            UICtrl.instance.UpdateHp(damageTarget);
        }


        while (true)
        {
            var info = this.animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForEndOfFrame();
            if (info.IsName("idle")) break;

        }
        //攻击动画结束
        Debug.Log("攻击结束");
        if (targetCanStrikeBack)
        {
            damageTarget.UnderAttack(this);
        }
        else
        {
            Debug.Log("对阵结束");
            AttackRoundEnd(damageTarget);
        }
    }

    void Damage_Ani()
    {
        this.animator.Play("damage01", 0, 0);

      //  EventDispatcherDemo.instance.showHitEffect.Invoke(this,Vector3.zero);

        EventDispatcher.instance.DispatchEvent<PlayerController,Vector3>(GameEventType.showHitEffect,this,this.transform.position+Vector3.up*2);
        //EffectCtrl.instance.showHitEffect(this,this.transform.position+Vector3.up*2);


    }

    void lookatTarget(PlayerController target)
    {
        this.transform.LookAt(target.transform);
        var localEulerAngles = this.transform.localEulerAngles;

        //只需要Y轴旋转
        localEulerAngles.x = 0;
        localEulerAngles.z = 0;

        this.transform.localEulerAngles = localEulerAngles;
    }

    //受到攻击
    void UnderAttack(PlayerController attacker)
    {
        lookatTarget(attacker);

        if (this.attribute.hp <= 0)
        {
            this.animator.CrossFade("death", 0.2f);
            Debug.Log("阵亡 对阵结束");
            AttackRoundEnd( attacker);
        }
        else
        {
            this.StartCoroutine(C_UnderAttack(attacker));
        }
    }

       IEnumerator C_UnderAttack(PlayerController attacker)
    {
        //计算攻击距离

        var strikeNode = this.mapNode;

        var attackerNode = attacker.mapNode;

        var abpath = StrikingRangeABPath.Construct((Vector3)strikeNode.position, (Vector3)attackerNode.position);
        AstarPath.StartPath(abpath);
        yield return StartCoroutine(abpath.WaitForPath());
        var dis = abpath.path.Count - 2;
        Debug.Log("攻击距离" + dis);
        //贴身距离为0
        //弓手距离为1
        //在有效射程才能反击
        if (this.attribute.striking_Range_Max > dis && this.attribute.striking_Range_Min <= dis)
        {
            this.AttackByOrder(attacker, false);
        }else
        {
            AttackRoundEnd(attacker);
            Debug.Log("无法还击 对阵结束");
        }
    }




    void AttackRoundEnd(PlayerController target)
    {
        // a.state = PlayerSate.idel;
        // b.state = PlayerSate.idel;

        EventDispatcher.instance.DispatchEvent<PlayerController, PlayerController>(GameEventType.battle_End, this, target);

        GameCtrl.instance.AttackRoundEnd();
    }





    //血量分配
    int[] CalculateDps(int dps, float[] range)
    {
        int[] o_dps = new int[range.Length];
        for (int i = 0; i < range.Length - 1; i++)
        {
            o_dps[i] = Mathf.FloorToInt(dps * range[i]);
            dps -= o_dps[i];
        }

        //分配剩余的数值
        o_dps[range.Length - 1] = dps;

        return o_dps;
    }


    [ContextMenu("输出分配测试")]
    void Test_CalculateDps()
    {
        var i = CalculateDps(9, new float[] { 0.3f, 0.7f });
        string resulte = "";
        foreach (var item in i) resulte += item + " 和";
        Debug.Log(9 + "分成" + resulte.Substring(0, resulte.Length - 1));
        //9分成2 和7 

        resulte = ""; i = CalculateDps(4, new float[] { 0.3f, 0.7f });
        foreach (var item in i) resulte += item + "  和";
        Debug.Log(4 + "分成" + resulte.Substring(0, resulte.Length - 1));
        //4分成1 和3

        resulte = ""; i = CalculateDps(56, new float[] { 0.3f, 0.3f, 0.4f });
        foreach (var item in i) resulte += item + "  和";
        Debug.Log(56 + "分成" + resulte.Substring(0, resulte.Length - 1));
        //56分成16  和12  和28
    }
}
