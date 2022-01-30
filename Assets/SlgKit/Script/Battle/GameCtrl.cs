using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class GameCtrl : MonoBehaviour
{
    PlayerController[] players ;
    private Bounds mapBounds;
    PlayerController curSelect;
    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindObjectsOfType<PlayerController>();
       
        var gridGraph = (AstarPath.active.graphs[0] as GridGraph);
        var size = gridGraph.nodeSize;
        mapBounds = new Bounds(gridGraph.center, new Vector3(gridGraph .width* size,10, size* gridGraph.depth));
    }

    //包围盒范围测试，只在编辑器运行
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(mapBounds.center, mapBounds.size);
    }

    internal void CancelSelect()
    {
        curSelect.CancelMove();
        //关闭路径
        GridMeshManager.Instance.DespawnAllPath();

        curSelect = null;
    }
    // Update is called once per frame
    void Update()
    {
        var hitWorldPoint = MouseRaycast();

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
            if (curSelect == null)
            {
                curSelect = hitPlayer;
                if (curSelect != null)
                {
                    curSelect.ShowMoveRange();
                    curSelect.Ready();
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
                    if (curSelect.goMapPos != hitMapPos)
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

}
