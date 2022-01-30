using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using System.Collections;

[System.Serializable]
public class EventInfo
{
    public string name;
    public string eventType;
    public string context;

    public void Dispose()
    {

    }

    public override string ToString()
    {
        return string.Format("[Evnet][Name={0}][Type={1}][Contxt={2}]", name, eventType, context);
    }
}

[System.Serializable]
public class GridNodeEventInfo
{

    public List<EventInfo> data
    {
        get
        {
            if (mData == null) mData = new List<EventInfo>();
            return mData;
        }
    }

    public int index;
    [SerializeField]
    private List<EventInfo> mData;

    public void Dispose()
    {
        if (mData != null)
        {
            for (int i = 0; i < mData.Count; i++)
            {
                mData[i].Dispose();
            }
            mData.Clear();
        }
    }

}

[System.Serializable]
public class GraphEventInfo
{


    public GridGraph gridGraph
    {
        get
        {
            if (AstarPath.active)
            {
                NavGraph[] _graphs = AstarPath.active.graphs;
                for (int i = 0; i < _graphs.Length; i++)
                {
                    GridGraph _gg = _graphs[i] as GridGraph;
                    if (_gg != null && _gg.guid.ToString() == guid)
                    {
                        return _gg;
                    }
                }
            }
            return null;
        }
    }

    public List<GridNodeEventInfo> data
    {
        get
        {
            if (mData == null) mData = new List<GridNodeEventInfo>();
            return mData;
        }
    }

    public string guid;
    [SerializeField]
    private List<GridNodeEventInfo> mData;

    public GridNodeEventInfo Get(GridNode node)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (mData[i].index == node.NodeInGridIndex)
            {
                return mData[i];
            }
        }
        return null;
    }

    public GridNodeEventInfo GetOrCreate(GridNode node)
    {
        GridNodeEventInfo _ei = Get(node);
        if (_ei == null)
        {
            _ei = new GridNodeEventInfo();
            _ei.index = node.NodeInGridIndex;
            mData.Add(_ei);
        }
        return _ei;
    }

    public void Remove(GridNodeEventInfo nodeEventInfo)
    {
        if (data.Contains(nodeEventInfo))
        {
            data.Remove(nodeEventInfo);
        }
    }

    public void Dispose()
    {
        if (mData != null)
        {
            for (int i = 0; i < mData.Count; i++)
            {
                mData[i].Dispose();
            }
            mData.Clear();
        }
    }

}


public class GraphEventSystem : MonoBehaviour
{

    public List<GraphEventInfo> data
    {
        get
        {
            if (mData == null) mData = new List<GraphEventInfo>();
            return mData;
        }
    }

    [SerializeField]
    private AstarPath mAstarPath;
    [SerializeField]
    private List<GraphEventInfo> mData;

   // private List<GizmosIcon> mGizmosIcons;


    public void InitEventSystem(AstarPath astarPath)
    {
        mAstarPath = astarPath;
        NavGraph[] _graphs = astarPath.graphs;
        List<GraphEventInfo> _newGraphEventInfos = new List<GraphEventInfo>();
        for (int i = 0; i < _graphs.Length; i++)
        {
            NavGraph _navGraph = _graphs[i];
            if (_navGraph is GridGraph)
            {
                GridGraph _gg = _navGraph as GridGraph;
                GraphEventInfo _graphEventInfo = Get(_gg.guid.ToString());
                if (_graphEventInfo == null)
                {
                    _graphEventInfo = new GraphEventInfo();
                    _graphEventInfo.guid = _gg.guid.ToString();
                    _newGraphEventInfos.Add(_graphEventInfo);
                }
            }
        }
        mData.Clear();
        mData = _newGraphEventInfos;
    }

    public GraphEventInfo Get(GridNode node)
    {
        GridGraph _gg = GridNode.GetGridGraph(node.GraphIndex);
        return Get(_gg.guid.ToString());
    }

    public GraphEventInfo Get(string guid)
    {
        for (int i = 0; i < data.Count; i++)
        {
            if (mData[i].guid == guid)
            {
                return mData[i];
            }
        }
        return null;
    }

    public GraphEventInfo GetOrCreate(GridNode node)
    {
        GridGraph _gg = GridNode.GetGridGraph(node.GraphIndex);
        GraphEventInfo _info = Get(_gg.guid.ToString());
        if (_info == null)
        {
            _info = new GraphEventInfo();
            _info.guid = _gg.guid.ToString();
            mData.Add(_info);
        }
        return _info;
    }

    public void AddEvent(GridNode node, EventInfo evt)
    {
        GraphEventInfo _eventInfo = GetOrCreate(node);
        GridNodeEventInfo _nodeEventInfo = _eventInfo.GetOrCreate(node);
        _nodeEventInfo.data.Add(evt);
    }


    //public void 画出格子事件图片()
    //{
    //    if (mGizmosIcons == null)
    //    {
    //        mGizmosIcons = new List<GizmosIcon>();
    //    }
    //    for (int i = 0; i < data.Count; i++)
    //    {
    //        GraphEventInfo _gei = data[i];
    //        GridGraph _gg = _gei.gridGraph;
    //        if (_gg != null)
    //        {
    //            for (int j = 0; j < _gei.data.Count; j++)
    //            {
    //                GridNodeEventInfo _gnei = _gei.data[j];
    //                if (_gnei != null && _gg.nodes != null && _gnei.index > 0 && _gnei.index < _gg.nodes.Length)
    //                {
    //                    if (_gnei.data.Count > 0)
    //                    {
    //                        GridNode _gridNode = _gg.nodes[_gnei.index];
    //                        for (int k = 0; k < _gnei.data.Count; k++)
    //                        {
    //                            EventInfo _ei = _gnei.data[k];
    //                            Vector3 _pos = (Vector3)_gridNode.position + new Vector3(0, 0.3f + 0.1f * k, 0);
    //                            Texture2D _icon = Resources.Load<Texture2D>("Gizmos/Astar/Event/" + _ei.eventType);
    //                            Material _mat = Resources.Load<Material>("Gizmos/Astar/Event/Gizmos");
    //                            _mat = Object.Instantiate(_mat) as Material;
    //                            _mat.mainTexture = _icon;
    //                            GameObject _iconGo = Resources.Load<GameObject>("Gizmos/Astar/Event/GizmosIcon");
    //                            _iconGo = Object.Instantiate(_iconGo) as GameObject;
    //                            _iconGo.GetComponent<Renderer>().sharedMaterial = _mat;
    //                            _iconGo.transform.position = _pos;
    //                            mGizmosIcons.Add(_iconGo.GetComponent<GizmosIcon>());
    //                        }
    //                        GameObject _go = AppUtility.LoadResources("GizmosNode");
    //                        _go.transform.position = (Vector3)_gridNode.position + new Vector3(0, 0.15f, 0);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}


    //public void ClearAllEventAreaDebug()
    //{
    //    if (mGizmosIcons != null)
    //    {
    //        for (int i = 0; i < mGizmosIcons.Count; i++)
    //        {
    //            mGizmosIcons[i].Dispose();
    //        }
    //        mGizmosIcons.Clear();
    //        mGizmosIcons = null;
    //    }
    //}


    public void ClearAll()
    {
        if (mData != null)
        {
            for (int i = 0; i < mData.Count; i++)
            {
                mData[i].Dispose();
            }
            mData.Clear();
        }
    }

    public List<EventInfo> GetNodeEvent(Vector3 position)
    {
        GraphNode _graphNode = AstarPath.active.GetNearest(position, NNConstraint.None).node;
        if (_graphNode != null && _graphNode is GridNode)
        {
            GridNode _gn = _graphNode as GridNode;
            GraphEventInfo _gei = Get(_gn);
            if (_gei != null)
            {
                GridNodeEventInfo _gnei = _gei.Get(_gn);
                if (_gnei != null)
                {
                    return _gnei.data;
                }
            }
        }
        return null;
    }

}
