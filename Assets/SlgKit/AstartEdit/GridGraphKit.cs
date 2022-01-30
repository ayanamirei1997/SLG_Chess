using System;
using System.Reflection;
using Pathfinding;
using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// Grid地图的工具控件
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(AstarPath))]
public class GridGraphKit : MonoBehaviour
{

    /// <summary>
    /// 缓存的AStar属性
    /// </summary>
    public AstarPath astarPath
    {
        get
        {
            if (mCachaAstarPath == null) mCachaAstarPath = GetComponent<AstarPath>();
            return mCachaAstarPath;
        }
    }

    public GraphEventSystem eventSystem
    {
        get
        {
            if (mEventSystem == null)
            {
                GameObject _go = new GameObject("_EventSystem");
                _go.transform.parent = astarPath.transform;
                mEventSystem = _go.AddComponent<GraphEventSystem>();
            }
            return mEventSystem;
        }
    }

    /// <summary>
    /// 缓存的AStar域
    /// </summary>
    [SerializeField]
    private AstarPath mCachaAstarPath;
    [SerializeField]
    private GraphEventSystem mEventSystem;


    //public Vector3 CalculateGridNodeNormal(GridNode node)
    //{
    //    Vector3 _normal = Vector3.up;
    //    if (node != null)
    //    {
    //        GridGraph gg = GridNode.GetGridGraph(node.GraphIndex);
    //        int[] neighbourOffsets = gg.neighbourOffsets;
    //        GridNode[] nodes = gg.nodes;
    //        Vector3 _pos = (Vector3)node.position;
    //        Vector3 _vector = Vector3.zero;
    //        bool _isCal = false;
    //        if (neighbourOffsets != null && neighbourOffsets.Length > 0 && nodes != null && nodes.Length > 0)
    //        {
    //            for (int i = 0; i < 8; i++)
    //            {
    //                if (i >= 0 && i < neighbourOffsets.Length)
    //                {
    //                    int _otherIdx = node.NodeInGridIndex + neighbourOffsets[i];
    //                    if (_otherIdx >= 0 && _otherIdx < nodes.Length)
    //                    {
    //                        GridNode other = nodes[_otherIdx];
    //                        if (other != null)
    //                        {
    //                            Vector3 _tpos = (Vector3)other.position;
    //                            Handles.color = Color.blue;
    //                            Handles.Label(_tpos, i + "");
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    DrawGridNode(node);
    //    return _normal;
    //}

    //private void DrawGridNode(GridNode node)
    //{
    //    GridGraph gg = GridNode.GetGridGraph(node.GraphIndex);
    //    int[] neighbourOffsets = gg.neighbourOffsets;
    //    GridNode[] nodes = gg.nodes;
    //    if (neighbourOffsets != null && neighbourOffsets.Length >= 8 && nodes != null && nodes.Length > 0)
    //    {
    //        Vector3 _p0;
    //        if (!GetPosition(nodes, node, neighbourOffsets[4], out _p0)) return;
    //        Vector3 _p1;
    //        if (!GetPosition(nodes, node, neighbourOffsets[5], out _p1)) return;
    //        Vector3 _p2;
    //        if (!GetPosition(nodes, node, neighbourOffsets[6], out _p2)) return;
    //        Vector3 _p3;
    //        if (!GetPosition(nodes, node, neighbourOffsets[7], out _p3)) return;
    //        Handles.SphereCap(6, _p0, Quaternion.identity, 0.1f);
    //        Handles.SphereCap(6, _p1, Quaternion.identity, 0.1f);
    //        Handles.SphereCap(6, _p2, Quaternion.identity, 0.1f);
    //        Handles.SphereCap(6, _p3, Quaternion.identity, 0.1f);
    //    }
    //}

    //private bool GetPosition(GridNode[] nodes, GridNode node, int neighbourOffset, out Vector3 position)
    //{
    //    int nodeIdx = node.NodeInGridIndex + neighbourOffset;
    //    if (nodeIdx >= 0 && nodeIdx < nodes.Length)
    //    {
    //        GridNode other = nodes[nodeIdx];
    //        position = ((Vector3)other.position + (Vector3)node.position) * 0.5f;
    //        return true;
    //    }
    //    position = Vector3.zero;
    //    return false;
    //}


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        //for (int i = 0; i < eventSystem.data.Count; i++)
        //{
        //    GraphEventInfo _gei = eventSystem.data[i];
        //    GridGraph _gg = _gei.gridGraph;
        //    if (_gg != null)
        //    {
        //        for (int j = 0; j < _gei.data.Count; j++)
        //        {
        //            GridNodeEventInfo _gnei = _gei.data[j];
        //            if (_gnei != null && _gg.nodes != null && _gnei.index > 0 && _gnei.index < _gg.nodes.Length)
        //            {
        //                if (_gnei.data.Count > 0)
        //                {
        //                    GridNode _gridNode = _gg.nodes[_gnei.index];
        //                    for (int k = 0; k < _gnei.data.Count; k++)
        //                    {
        //                        EventInfo _ei = _gnei.data[k];
        //                        Gizmos.DrawIcon((Vector3)_gridNode.position
        //                            + new Vector3(0, 0.1f * k, 0),
        //                            "Astar/Event/" + _ei.eventType + ".png", true);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }


    public Action drawNoTag;
    public int drawNodeTag;
    public Color drawNodeTagColor = Color.yellow;

    public void OnDrawGizmos()
    {
        Gizmos.color = drawNodeTagColor;

        if (drawNoTag != null)
        {
            drawNoTag();
        }

    }




#endif

}
