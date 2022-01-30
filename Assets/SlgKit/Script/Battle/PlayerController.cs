using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    int moveRange = 5;
    public bool canTraverseWater = false;
    // Start is called before the first frame update

    Animator animator;


    void Start()
    {
        // StartCoroutine(TestUpdate());

        //  StartCoroutine(TestUpdateAB_path());

        animator = GetComponent<Animator>();

        worldPos2MapPos();
    }

    public Int3 mapPos
    {
        get
        {
            return AstarPath.active.GetNearest((Vector3)this.transform.position).node.position;
        }
    }

    void worldPos2MapPos()
    {

        this.transform.position = (Vector3)this.mapPos;


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
        GetMovePath((Path path) =>
        {
            GridMeshManager.Instance.ShowPath(path.path);
            moveRangePath = path.path;
        }
        );
    }

    public void CancelMove()
    {
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
        
        GridMeshManager.Instance.ShowPathRed(ab_path.path);
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
            var speed = this.moveSpeed *Time.deltaTime;
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
                var t1 = curdistance/speed;
                var t2 = remaining_distance/speed;
                t3 = 1 - t2/t1;
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

    void MoveStop()
    {
        this.animator.CrossFade("idle", 0.2f);
    }
    

    /// <summary>
    /// 获取移动路径
    /// </summary>
    /// <param name="OnPathSerchOkCallBack"></param>
    public void GetMovePath(System.Action<Path> OnPathSerchOkCallBack)
    {
        var moveGScore = this.moveRange * 1000 * 3;

        var SerchPath = MoveRangConStantPath.Construct(this.transform.position, moveGScore, canTraverseWater,
        (Path path) =>
        {
            path.path = (path as MoveRangConStantPath).allNodes;
            OnPathSerchOkCallBack.Invoke(path);

        }

        );
        //异步返回搜索结果
        AstarPath.StartPath(SerchPath, true);
    }

    public Transform target;
    private Vector3 startMovePos;
    internal Int3? goMapPos;
    private List<GraphNode> moveRangePath;
    public float moveSpeed;
    private Coroutine moveCor;

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
        ABPathExt mPath = ABPathExt.ConstructRange(p_start, p_endpos,this.moveRangePath,
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






}
