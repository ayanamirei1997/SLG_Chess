using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using static GameDefine;

public class GameCtrl : MonoBehaviour
{
    public static GameCtrl instance { get; private set; }

    public PlayerController[] players;
    private Bounds mapBounds;
    PlayerController curSelect;
    public Sect mySect;
    private bool battle;
    private PlayerController attacker;

    private void Awake()
    {
        instance = this;

        EffectCtrl.Init();

        SkillSys.Instance.Init();
    }


    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindObjectsOfType<PlayerController>();

        var gridGraph = (AstarPath.active.graphs[0] as GridGraph);
        var size = gridGraph.nodeSize;
        mapBounds = new Bounds(gridGraph.center, new Vector3(gridGraph.width * size, 10, size * gridGraph.depth));

        UICtrl.instance.Init_HpImage();
    }

    //包围盒范围测试，只在编辑器运行
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(mapBounds.center, mapBounds.size);
    }

    internal void CancelSelect()
    {
        UICtrl.instance.actionPanel.SetActive(false);
        curSelect.CancelMove();
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        curSelect = null;
    }

    public void ReleaseSelect()
    {

        UICtrl.instance.actionPanel.SetActive(false);
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        curSelect = null;
    }

    // Update is called once per frame
    void Update()
    {

        //防止点击待机UI时进行移动

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        //战斗中不执行操作
        if (this.battle) return;



        var hitWorldPoint = MouseRaycast();

        //跑过去攻击 敌人->记录人物状态（这个时候不再执行鼠标点击逻辑）
        if (curSelect != null && curSelect.state == PlayerSate.moveAttack)
        {
            return;
        }

        if (hitWorldPoint != null)
        {
            var hitMapNode = AstarPath.active.GetNearest((Vector3)hitWorldPoint).node;
            var hitMapPos = hitMapNode.position;

            //超出A星图边界，不进行任何处理
            if (!mapBounds.Contains((Vector3)hitWorldPoint))
            {
                if (curSelect != null)
                {
                    this.CancelSelect();
                }
                return;
            }

            //通过地图坐标获取人物
            var hitPlayer = SelectPlayer(hitMapPos);


            //如果当前选择的人物是可控门派，且点击的对象是敌人，且在攻击距离内，则进行攻击
            if (hitPlayer != null && curSelect != null && curSelect.sect == mySect && hitPlayer.sect != curSelect.sect
                && curSelect.state == PlayerSate.idle)
            {
                if (curSelect.InAttackRang(hitPlayer))
                {
                    Debug.Log("在有效范围 直接攻击敌人");
                    SetBattleState();
                    curSelect.Active_Attack(hitPlayer);
                    this.ReleaseSelect();
                    return;
                }//并且在移动范围内，就跑过去攻击
                else if (curSelect.CanMoveAttack(hitPlayer))
                {
                    SetBattleState();
                    Debug.Log("不在有效范围 跑过去攻击");
                    curSelect.MoveAttack(hitPlayer);

                    return;
                }

            }


            //假如点击的玩家为不可控制的门派则仅显示移动范围
            if (hitPlayer != null && hitPlayer.sect != mySect)
            {
                if (curSelect != null)
                {
                    this.CancelSelect();
                }
                hitPlayer.ShowMoveRange();

                return;
            }

            if (curSelect == null)
            {
                curSelect = hitPlayer;
                if (curSelect != null)
                {
                    curSelect.ShowMoveRange();

                    //如果玩家可控则执行 准备指令
                    if (curSelect.sect == mySect&& curSelect.state == PlayerSate.idle)
                    {
                        curSelect.Ready();
                        UICtrl.instance.actionPanel.SetActive(true);
                    }
                }
            }
            else
            {
                //选择对象时候判断是不是当前指定的对象
                //如果不是则把当前人物的行动取消
                //切换人物后再进行范围移动显示
                var otherSelect = hitPlayer;

                if (otherSelect == curSelect || otherSelect == null)
                {
                    if (curSelect.goMapPos != hitMapPos && curSelect.state == PlayerSate.idle)
                        curSelect.Move(hitMapNode);
                }
                else if (otherSelect != curSelect)
                {


                    UpdateSelect(hitPlayer);



                }

            }

        }

    }
  

    void UpdateSelect(PlayerController playerController)
    {

        if (playerController.state == PlayerSate.idle)
        {
            UICtrl.instance.actionPanel.SetActive(true);
            playerController.Ready();
        }
        else
        {
            UICtrl.instance.actionPanel.SetActive(false);
        }

        if (curSelect!=null)
        curSelect.CancelMove();
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        playerController.ShowMoveRange();
        playerController.Ready();
        curSelect = playerController;
    }

    private PlayerController SelectPlayer(Int3 hitMapPos)
    {
        foreach (PlayerController player in players)
        {
            if (player.mapPos == hitMapPos)
            {
                return player;
            }
        }

        return null;
    }





    Vector3? MouseRaycast()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //生成一条从摄像机发出的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //用来存储射线打中物体的信息
            RaycastHit hit;
            //发射射线
            bool result = Physics.Raycast(ray, out hit);
            //如果为true说明打中物体了
            if (result)
            {
                //  Debug.Log(hit.point);

                return hit.point;

            }

        }

        return null;
    }

    public List<PlayerController> GetEnemy(Sect sect)
    {
        List<PlayerController> enemy = new List<PlayerController>();
        foreach (var item in this.players)
        {
            if (item.sect != sect) enemy.Add(item);
        }


        return enemy;
    }

    internal List<GraphNode> GetPlayersMapNode()
    {
        List<GraphNode> t = new List<GraphNode>();
        foreach (var item in this.players)
        {
            t.Add(item.mapNode);
        }


        return t;
    }

    internal void Wait(bool p_AttackRoundEnd=false)
    {
        //throw new NotImplementedException();
        if (!p_AttackRoundEnd)
        {
            this.curSelect.state = PlayerSate.wait;
            this.ReleaseSelect();
        }

        var idlePlayer = IdlePlayer(mySect);

        if (idlePlayer != null)
        {
            UpdateSelect(idlePlayer);
        }
        else
        {
            Debug.Log("对手回合！");
        }
    }

    private PlayerController IdlePlayer(Sect sect)
    {
        foreach (PlayerController item in this.players)
        {
            if (item.sect == sect && item.state == PlayerSate.idle) return item;
        }

        return null;
    }

    //防止战斗中操作
    private void SetBattleState()
    {
        //throw new NotImplementedException();
        this.battle = true;
        attacker = curSelect;

    }

    internal void AttackRoundEnd()
    {
       
        attacker.state = PlayerSate.wait;

        this.battle = false;
        this.Wait(true);
    }
}
