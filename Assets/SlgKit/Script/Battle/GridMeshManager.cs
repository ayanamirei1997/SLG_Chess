using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathfinding;
using PathologicalGames;



public class GridMeshManager : Singleton<GridMeshManager>
{

    SpawnPool m_PoolGreenMesh;
    SpawnPool m_Pool_whiteMesh;
    GameObject mouse_Quad;
    float node_size = 0;


    SpawnPool m_Pool_strRangeMesh;
    public GridMeshManager()
    {
        node_size = (AstarPath.active.graphs[0] as GridGraph).nodeSize;

        var pool_Go_green = Resources.Load<GameObject>("Prefab/PoolItem/Pool_GreenMesh");
        var i_Go_green = GameObject.Instantiate(pool_Go_green);
        i_Go_green.SetActive(true);
        m_PoolGreenMesh = i_Go_green.GetComponent<SpawnPool>();

        var pool_Go_red = Resources.Load<GameObject>("Prefab/PoolItem/Pool_WhiteMesh");
        var i_red_Go = GameObject.Instantiate(pool_Go_red);
        i_red_Go.SetActive(true);
        m_Pool_whiteMesh = i_red_Go.GetComponent<SpawnPool>();

        var sktR_pool_Go_red = Resources.Load<GameObject>("Prefab/PoolItem/Pool_RedMesh");
        var sktR_red_Go = GameObject.Instantiate(sktR_pool_Go_red);
        sktR_red_Go.SetActive(true);
        m_Pool_strRangeMesh = sktR_red_Go.GetComponent<SpawnPool>();

        // mouse_Quad = GameObject.Instantiate(Resources.Load<GameObject>("Prefab/PoolItem/mouse_Quad"));
        //  mouse_Quad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        //  mouse_Quad.transform.localScale = new Vector3(node_size, node_size, 1);
    }

    
    void clearMoveRangeEvent()
    {

        m_PoolGreenMesh.DespawnAll();
    }

    public void ShowPath(List<GraphNode> obj)
    {
     
        m_PoolGreenMesh.DespawnAll();

        DarwPath(this.m_PoolGreenMesh, "green_Quad", obj,0.1f,1f);
    }

    public  void DespawnAllPath()
    {
        m_PoolGreenMesh.DespawnAll();
        m_Pool_whiteMesh.DespawnAll();
        m_Pool_strRangeMesh.DespawnAll();
    }

    public void ShowPathWhite(List<GraphNode> obj)
    {

        m_Pool_whiteMesh.DespawnAll();

        DarwPath(this.m_Pool_whiteMesh, "white_Quad", obj,0.2f,0.8f);
    }

    public void StrRangePath(List<GraphNode> obj)
    {

        m_Pool_strRangeMesh.DespawnAll();

        DarwPath(this.m_Pool_strRangeMesh, "red_Quad", obj, 0.15f, 1f);
    }


    void DarwPath(SpawnPool pool, string poolPrefab, List<GraphNode> path,float offsetY,float scale)
    {

        pool.DespawnAll();

        // 移动范围mesh.PageDraw(obj, 移动范围射程材质, meshOffset);

        foreach (GraphNode item in path)
        {

            var pos = (Vector3)item.position;
            pos.y += offsetY;
            var gridGob = pool.Spawn(poolPrefab, pos, Quaternion.Euler(90, 0, 0));
            gridGob.transform.localScale = new Vector3(node_size* scale, node_size * scale, 1);
        }
    }








}

