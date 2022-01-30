using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Pathfinding;
using Pathfinding.Serialization;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEditor.SceneManagement;

/// <summary>
/// GridGraphKit 的编辑器面板控件
/// </summary>
[CustomEditor(typeof(GridGraphKit))]
public class GridGraphKitInspector : Editor
{
    /// <summary>
    /// 笔刷的类型
    /// </summary>
    private enum BrushType
    {
        //Circle,
        Rectangle
    }


    static Collider selectCollider;

    /// <summary>
    /// 用于指定GUI控制的ID时使用的HashCode
    /// </summary>
    private static int sHashCode = "GridGraphKit".GetHashCode();

    /// <summary>
    /// GridGraphKit对象，控件编辑的对象
    /// </summary>
    private GridGraphKit mGridGraphKit;
    /// <summary>
    /// 笔刷类型
    /// </summary>
    private BrushType mBrushType;
   
    private float mBrushSize = 1;
    /// <summary>
    /// 是否正在放大或者缩小笔刷
    /// </summary>
    private bool mBrushZoom = false;
    /// <summary>
    /// 放大或者缩小笔刷时的滚动值，用于指定是放大还是缩小
    /// </summary>
    private float mBrushZoomValue;
    /// <summary>
    /// 放大或者缩小笔刷的灵敏度
    /// </summary>
    private float mBrushZoomSensitivity = 0.03f;

    /// <summary>
    /// 笔刷的世界坐标
    /// </summary>
    private Vector3 mBrushWorldPosition;
    /// <summary>
    /// 右键菜单使用的笔刷世界坐标，因为选择右键菜单选项时，会移动鼠标，所以要先记录笔刷位置
    /// </summary>
    private Vector3 mContextMenuBrushWorldPosition;

    /// <summary>
    /// 笔刷选中需要绘制的节点
    /// </summary>
    private List<GraphNode> mDrawNodes;

    /// <summary>
    /// 是否正在涂绘地图
    /// </summary>
    private bool mDrawing;
    /// <summary>
    /// 笔刷最靠近的节点，用于测试目标节点的属性
    /// </summary>
    private GraphNode mNearsetNode;

    private GraphNode mEditGridNode;

    static Transform mbuilderParent;
    static Transform builderParent
    {

        get
        {

            if (mbuilderParent == null)
            {
                var p_go = GameObject.Find("CustomMap");
                if (p_go == null)
                {
                    mbuilderParent = new GameObject("CustomMap").transform;
                }
                else
                    mbuilderParent = p_go.transform;
                 
            }

            return mbuilderParent;
        }

    }
    /// <summary>
    /// 当控件激活的时候初始化
    /// </summary>
    private void OnEnable()
    {
        Debug.Log("OnEnable");
        mGridGraphKit = target as GridGraphKit;
        SceneView.duringSceneGui -= OnSceneGuiDelegate;
        SceneView.duringSceneGui += OnSceneGuiDelegate;

        //Scene
    }

    /// <summary>
    /// 当控件激活的时候销毁
    /// </summary>
    private void OnDestroy()
    {
        mGridGraphKit = null;
        SceneView.duringSceneGui -= OnSceneGuiDelegate;
    }


    void  DrawtagNode()
    {
        AstarPath.active.graphs[0].GetNodes(OnDrawtagNode);
    }

    bool OnDrawtagNode(GraphNode p_Node)
    {
		if(mGridGraphKit==null)
		{
			return true;
		}
        if (mGridGraphKit.drawNodeTag == p_Node.Tag)
        {
            Gizmos.DrawCube((Vector3)p_Node.position, Vector3.one * AstarPath.active.unwalkableNodeDebugSize*3);
        }

        return true;
        // return false;
    }



    public TextAsset graphData;
    static int mEditPenalty = 0;
    static int mEditTag = 0;
    static MeshRenderer mbuild;
    /// <summary>
    /// 重载脚本的属性面板的GUI
    /// </summary>
    public override void OnInspectorGUI()
    {
         
        //base.OnInspectorGUI();
        mbuild = (MeshRenderer)EditorGUILayout.ObjectField("Build", mbuild, typeof(MeshRenderer), false);
        graphData = (TextAsset)EditorGUILayout.ObjectField("graphData", graphData, typeof(TextAsset), false);
        //GraphEventSystem eventSystem = mGridGraphKit.eventSystem;
        //if (mEditGridNode != null)
        //{
        //    DrawEventArea(eventSystem, mEditGridNode);
        //    return;
        //}

        GUILayout.Space(10);

        ///________________________________________________________________________________Brush Setting

        //笔刷的设定窗口
        GUILayout.BeginVertical("Brush Setting", GUI.skin.window);
        //笔刷类型
        mBrushType = (BrushType)EditorGUILayout.EnumPopup("Brush Type", mBrushType);
        //笔刷最小Size
        mBrushSize = EditorGUILayout.FloatField("mBrushSize", mBrushSize);
        //笔刷最大Size
      //  mMaxBrushSize = EditorGUILayout.FloatField("Max Brush Size", mMaxBrushSize);
        //笔刷缩放灵敏度
        mBrushZoomSensitivity = EditorGUILayout.Slider("Brush Size Zoom Sensitivity", mBrushZoomSensitivity, 0.001f, 0.03f);
        //笔刷Size
        // mBrushSize = EditorGUILayout.Slider("Brush Size", mBrushSize, mMinBrushSize, mMaxBrushSize);

        mEditPenalty = EditorGUILayout.IntField("行走代价", mEditPenalty);
        mEditTag = EditorGUILayout.Popup("编辑标识", mEditTag, GameDefine.GetTagNames());

        GUILayout.EndVertical();

        GUILayout.Space(10);

        mGridGraphKit.drawNodeTagColor = EditorGUILayout.ColorField(mGridGraphKit.drawNodeTagColor);
        mGridGraphKit.drawNodeTag = EditorGUILayout.Popup("显示标识节点", mGridGraphKit.drawNodeTag, GameDefine.GetTagNames());
        mGridGraphKit.drawNoTag = DrawtagNode;

        ///________________________________________________________________________________Nearest Node Info

        //最接近笔刷的节点信息
        GUILayout.BeginVertical("Nearest Node Info", GUI.skin.window);
        if (mNearsetNode != null && mNearsetNode is GridNode)
        {
            int _nodeIndex = -1;

            GridNode _gridNode = mNearsetNode as GridNode;
            GridGraph _gg = GridNode.GetGridGraph(_gridNode.GraphIndex);

            //获取该节点的在地图的中索引
            _gg.GetNodes(delegate(GraphNode node)
            {
                _nodeIndex++;
                return mNearsetNode != node;
            });

            //节点的索引
            EditorGUILayout.IntField("Node Index", _nodeIndex);

            //节点的所属的地图的索引
            EditorGUILayout.IntField("Graph Index", (int)mNearsetNode.GraphIndex);

            //节点的唯一id
            EditorGUILayout.IntField("Node ID", (int)mNearsetNode.NodeIndex);

            EditorGUILayout.IntField("Node Tag", (int)mNearsetNode.Tag);

            //节点释放可行
            EditorGUILayout.ToggleLeft("Walkable", mNearsetNode.Walkable);

            //节点世界位置
            EditorGUILayout.Vector3Field("Position", (Vector3)mNearsetNode.position);

            mNearsetNode.Penalty = (uint)EditorGUILayout.IntField("Penalty", (int)mNearsetNode.Penalty);

          
            EditorGUILayout.LabelField("当前标识          "+ ((GameDefine.AstartTag)mNearsetNode.Tag).ToString());
          

            //显示节点的连接情况
            if (_gg.neighbours == NumNeighbours.Four)
            {
                Color _c0 = Color.green;
                Color _c1 = Color.gray;
                GUILayoutOption[] _options = new GUILayoutOption[]
                {
                    GUILayout.Width(25),
                    GUILayout.Height(25)
                };
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUI.color = Color.clear;
                GUILayout.Box("", _options);
                GUI.color = _gg.HasNodeConnection(_gridNode, 2) ? _c0 : _c1;
                GUILayout.Box("", _options);
                GUI.color = Color.clear;
                GUILayout.Box("", _options);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUI.color = _gg.HasNodeConnection(_gridNode, 3) ? _c0 : _c1;
                GUILayout.Box("", _options);
                GUI.color = Color.clear;
                GUILayout.Box("", _options);
                GUI.color = _gg.HasNodeConnection(_gridNode, 1) ? _c0 : _c1;
                GUILayout.Box("", _options);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUI.color = Color.clear;
                GUILayout.Box("", _options);
                GUI.color = _gg.HasNodeConnection(_gridNode, 0) ? _c0 : _c1;
                GUILayout.Box("", _options);
                GUI.color = Color.clear;
                GUILayout.Box("", _options);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }
            else
            {

            }
            //for (int i = 0; i < 8; i++)
            //{
            //    EditorGUILayout.LabelField(i + "", _gridNode.GetConnectionInternal(i) ? "TRUE" : "");
            //}
            if (_gridNode.connections != null)
            {
                EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                for (int i = 0; i < _gridNode.connections.Length; i++)
                {
                    GraphNode _other = _gridNode.connections[i];
                    EditorGUILayout.LabelField(i + "", _gridNode.connectionCosts[i] + "");
                }
                GUILayout.EndVertical();
            }
        }

        GUILayout.EndVertical();

        GUILayout.Space(10);

        //笔刷的设定窗口
        //Repaint();
        if (selectCollider!=null)
        {
            GUILayout.Label("当前选中的碰撞体"+selectCollider.name);
            if (GUILayout.Button("可行走"))
            {
               var graph= AstarPath.active.graphs[0] as GridGraph;
                   var nodes= graph.GetNodesInArea(selectCollider.bounds);
                   foreach (GraphNode jo in nodes)
                       DrawGraphNode(jo as GridNode,true);
            }

            if (GUILayout.Button("不可行走"))
            {
                var graph = AstarPath.active.graphs[0] as GridGraph;
                var nodes = graph.GetNodesInArea(selectCollider.bounds);
                foreach (GraphNode jo in nodes)
                    DrawGraphNode(jo as GridNode, false);
            }
        }


    }




    


    /// <summary>
    /// Scene窗口的更新回调
    /// </summary>
    /// <param name="sceneView"></param>
    private void OnSceneGuiDelegate(SceneView sceneView)
    {

        Debug.Log("OnSceneGuiDelegate");

        

        if (mGridGraphKit==null)
        mGridGraphKit = target as GridGraphKit;
        //获取鼠标位置
        Vector3 _mouse = Event.current.mousePosition;
        //反转鼠标
        _mouse.y = sceneView.camera.pixelHeight - _mouse.y;
        //获取射线
        Ray _ray = sceneView.camera.ScreenPointToRay(_mouse);
        //通过几何平面获取平面上的鼠标点击位置
        Plane _plane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        float _enter;
        if (_plane.Raycast(_ray, out _enter))
        {
            //记录笔刷的世界位置
            mBrushWorldPosition = _ray.GetPoint(_enter);
            //刷新所有Scene窗口
            SceneView.RepaintAll();

            EditorUtility.SetDirty(mGridGraphKit);
        }


        //鼠标左键释放时，显示选中的碰撞体
        if (Event.current.type== EventType.MouseUp && Event.current.button == 0)
        {


            //获取射线
             _ray = sceneView.camera.ScreenPointToRay(_mouse);
            RaycastHit hit;
          if(  Physics.Raycast(_ray,out  hit)){

            if (hit.collider!=null)
            {
                selectCollider = hit.collider;
            }
          }

           
        }

        if (AstarPath.active)
        {
            NavGraph[] _graphs = AstarPath.active.graphs;
            for (int i = 0, l = _graphs.Length; i < l; i++)
            {
                if (_graphs[i] is GridGraph)
                {
                    GridGraph _gg = _graphs[i] as GridGraph;
                    RaycastGrid(sceneView, _gg, false);
                }
            }
        }
    }

    /// <summary>
    /// 更新Scene窗口的GUI
    /// </summary>
    private void OnSceneGUI()
    {

        //获取GUI事件
        Event _e = Event.current;
        //指定GUI的ID
        int _id = GUIUtility.GetControlID(sHashCode, FocusType.Passive);
        //获取指定ID的事件类型
        EventType _type = _e.GetTypeForControl(_id);
        switch (_type)
        {
            //鼠标点击的事件
            case EventType.MouseDown:
                //左键点击，开始绘制
                if (_e.button == 0)
                {
                    if (!Event.current.control && !Event.current.alt)
                    {
                        mDrawing = true;
                    }
                }
                //右键点击，指定GUI的事件ID
                else if (_e.button == 1)
                {
                    //记录右键菜单的笔刷位置
                    mContextMenuBrushWorldPosition = mBrushWorldPosition;
                    //指定事件ID
                    GUIUtility.hotControl = GUIUtility.keyboardControl = _id;
                    _e.Use();
                }
                break;
            //鼠标释放的事件
            case EventType.MouseUp:
                //判断事件ID是否当前ID
                if (GUIUtility.hotControl == _id)
                {
                    GUIUtility.hotControl = 0;
                    GUIUtility.keyboardControl = 0;

                    //右键释放时，显示菜单
                    if (_e.button == 1)
                    {
                        ShowGenericMenu();
                        _e.Use();
                    }



                }

                //取消绘制
                if (mDrawing)
                {
                    mDrawing = false;
                }
                break;

            //键盘按下
            case EventType.KeyDown:
                //左括号键，缩小笔刷
                if (Event.current.keyCode == KeyCode.LeftBracket)
                {
                    mBrushZoom = true;
                    mBrushZoomValue = -1;
                    _e.Use();
                }

                //右括号键，缩小笔刷
                if (Event.current.keyCode == KeyCode.RightBracket)
                {
                    mBrushZoom = true;
                    mBrushZoomValue = 1;
                    _e.Use();
                }
                break;

            //键盘按键释放时，停止放大缩小
            case EventType.KeyUp:
                mBrushZoom = false;
                break;

            //无效事件时，取消绘制
            case EventType.Ignore:
                if (mDrawing)
                {
                    mDrawing = false;
                }
                break;
            //屏蔽鼠标点击选中物体
            case EventType.Layout:
                HandleUtility.AddDefaultControl(0);
                break;
        }

        //放大缩小笔刷
        if (mBrushZoom)
        {
            mBrushSize += mBrushZoomValue * mBrushZoomSensitivity;
          //  mBrushSize = Mathf.Clamp(mBrushSize, mMinBrushSize, mMaxBrushSize);
        }

        //在地图上绘制
        if (mDrawing)
        {
            //绘制函数
            DrawGraphNode(!Event.current.shift);
        }

        if (mEditGridNode != null)
        {
            Handles.color = new Color(1, 1, 0, 1);
            Handles.SphereCap(3, (Vector3)mEditGridNode.position, Quaternion.identity, 0.3f);
        }
        DrawNodeEventIcon();
        DrawSceneUI();
    }

    private void DrawSceneUI()
    {
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(5, 5, 200, 100));
        if (GUILayout.Button("Scan"))
        {
            mGridGraphKit.astarPath.Scan();
        }
        if (GUILayout.Button("Save Cached Astar Data"))
        {
            mGridGraphKit.astarPath.astarData.SaveCacheData(SerializeSettings.All);

            var d=mGridGraphKit.astarPath.astarData.SerializeGraphs(SerializeSettings.All);
            
        }

        if (GUILayout.Button("Save To StreamingAssets"))
        {
        

            var bytes = mGridGraphKit.astarPath.astarData.SerializeGraphs(SerializeSettings.All);


            var folder = Application.streamingAssetsPath + "/AstarData/";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var path = folder + EditorSceneManager.GetActiveScene().name;
            File.WriteAllBytes(path, bytes);

            Debug.Log(path);
            AssetDatabase.Refresh();

        }

        if (mGridGraphKit.astarPath.astarData.data_cachedStartup != null)
        {
            if (GUILayout.Button("Load Cached Astar Data"))
            {
                mGridGraphKit.astarPath.astarData.LoadFromCache();
            }
            if (GUILayout.Button("Clear Cached Astar Data"))
            {
                mGridGraphKit.astarPath.astarData.data_cachedStartup = null;
                mGridGraphKit.astarPath.astarData.cacheStartup = false;
            }
        }
       
        GUILayout.EndArea();
        Handles.EndGUI();
    }

    /// <summary>
    /// 在地图上的指定区域绘制
    /// </summary>
    /// <param name="astarPath">寻路控件</param>
    /// <param name="position">笔刷的中心位置</param>
    /// <param name="brushType">笔刷的类型</param>
    /// <param name="brushSize">笔刷的size</param>
    /// <param name="Walkable">是否可行</param>
    private void DrawGraphNode(bool Walkable)
    {
        if (mDrawNodes != null)
        {
            var size = (uint)(mGridGraphKit.astarPath.graphs[0] as GridGraph).nodeSize;
            //Debug.Log("DrawGraphNode"+ Walkable + mDrawNodes.Count);

            for (int i = 0, c = mDrawNodes.Count; i < c; i++)
            {
                mDrawNodes[i].Walkable = Walkable;

                

                if (mbuild != null && mDrawNodes[i].Tag != (uint)GameDefine.AstartTag.Obstacel)
                {
                    var go = (MonoBehaviour.Instantiate(mbuild.gameObject) as GameObject);
                    go.transform.SetParent(builderParent);
                    go.transform.position = (Vector3)mDrawNodes[i].position;
                    mDrawNodes[i].Tag = (uint)mEditTag;
                   
                }


                mDrawNodes[i].Penalty = (uint)mEditPenalty*1000* (uint)size;
                mDrawNodes[i].Tag = (uint)mEditTag;
                if (Walkable)
                {
                   // NodeLink link = new NodeLink();
                    //取得周围4个格子
                    GridGraph.CalculateConnections(mDrawNodes[i] as GridNode);
                            
                   
                }
                else
                {
                    mDrawNodes[i].ClearConnections(true);

                }
                
            }
        }
    }

    private void DrawGraphNode(GridNode node,bool Walkable)
    {

        node.Walkable = Walkable;
                if (Walkable)
                {
                    // NodeLink link = new NodeLink();
                    //取得周围4个格子
                    GridGraph.CalculateConnections(node);


                }
                else
                {
                    node.ClearConnections(true);

                }

            
        
    }

    /// <summary>
    /// 绘制笔刷最接近的节点
    /// </summary>
    /// <param name="world"></param>
    private void DrawNearestNode(Vector3 world)
    {
        GraphNode _node = AstarPath.active.GetNearest(world).node;
        if (_node != null && mNearsetNode != _node)
        {
            mNearsetNode = _node;

            Repaint();
        }
        Handles.color = new Color(0, 0, 1, 0.3f);
        Handles.SphereCap(3, (Vector3)mNearsetNode.position, Quaternion.identity, 0.3f);
    }

    private void DrawNodeEventIcon()
    {

    }

    /// <summary>
    /// 显示右键菜单
    /// </summary>
    private void ShowGenericMenu()
    {
        //创建右键菜单
        GenericMenu _menu = new GenericMenu();

        //绘制可行区域的菜单
        _menu.AddItem(new GUIContent("Walkable Ture"), false, delegate
        {
            DrawGraphNode(true);
        });

        //绘制不可行区域的菜单
        _menu.AddItem(new GUIContent("Walkable False"), false, delegate
        {
            DrawGraphNode(false);
        });


        //GraphEventSystem _eventSystem = mGridGraphKit.eventSystem;
        //if (_eventSystem != null && mNearsetNode != null && mNearsetNode is GridNode)
        //{
        //    //绘制不可行区域的菜单
        //    _menu.AddItem(new GUIContent("Manager Evnet"), false, delegate
        //    {
        //        //NavGraphEventEditor.ShowEditorForNode(_eventSystem, mNearsetNode as GridNode);
        //        //mEditGridNode = mNearsetNode as GridNode;
        //        // Repaint();

        //        GridNodeEventInfoEditor.EditNodeEventInfo(_eventSystem, mNearsetNode as GridNode);
        //    });
        //}

        //显示菜单
        _menu.ShowAsContext();
    }



    private GridNode RaycastGrid(SceneView sceneView, GridGraph graph, bool debug)
    {
        GridNode _node = null;

        //获取鼠标位置
        Vector3 _mouse = Event.current.mousePosition;
        //反转鼠标
        _mouse.y = sceneView.camera.pixelHeight - _mouse.y;
        //获取射线
        Ray _ray = sceneView.camera.ScreenPointToRay(_mouse);

        RaycastHit _hit;
        if (Physics.Raycast(_ray, out _hit, float.MaxValue))
        {
            Handles.color = Color.green;
            Handles.DrawLine(_hit.point, _hit.point + _hit.normal * 1);

            //绘制笔刷的范围UI
            //if (mBrushType == BrushType.Circle)
            //{
            //    Handles.CircleCap(0, _hit.point, Quaternion.LookRotation(_hit.normal), mBrushSize);
            //}
            //else 
            if (mBrushType == BrushType.Rectangle)
            {
                Handles.RectangleCap(0, _hit.point, Quaternion.Euler(90,0,0), mBrushSize);
            }

            mNearsetNode = AstarPath.active.GetNearest(_ray);
            if (mNearsetNode != null)
            {
                Handles.color = new Color(0, 0, 1, 1);
                Handles.SphereCap(3, (Vector3)mNearsetNode.position, Quaternion.identity, 0.3f);
            }

            List<Vector3> _points = new List<Vector3>();
            Quaternion _rotation = Quaternion.LookRotation(_hit.normal);
            Bounds _bounds = new Bounds(_hit.point, new Vector3(mBrushSize*2 , 1000 , mBrushSize * 2));
          //  Matrix4x4 _mat = Matrix4x4.TRS(_hit.point, Quaternion.LookRotation(_hit.normal), -Vector3.one).inverse;
            mDrawNodes = graph.GetNodesInArea(_bounds);

            //Debug.Log(mDrawNodes.Count);
            //if (mDrawNodes != null)
            //{
            //    for (int i = 0; i < mDrawNodes.Count; )
            //    {
            //        Vector3 _wpos = (Vector3)mDrawNodes[i].position;
            //        Vector3 _pos = _wpos;
            //        bool _isSelected = false;
            //        if ((Mathf.Abs(_pos.z) < 0.5f))
            //        {
            //            if ((_pos.x > -mBrushSize && _pos.x < mBrushSize) &&
            //                (_pos.y > -mBrushSize && _pos.y < mBrushSize))
            //            {
            //                _isSelected = true;
            //                if (false && _pos.magnitude > mBrushSize)
            //                {
            //                    _isSelected = false;
            //                }
            //            }
            //        }
            //        if (_isSelected)
            //        {
            //            if (debug)
            //            {
            //                Handles.color = new Color(0, 1, 0, 1);
            //                Handles.SphereCap(4, _wpos, Quaternion.identity, 0.5f);
            //            }
            //        }
            //        else
            //        {
            //            if (debug)
            //            {
            //                Handles.color = new Color(1, 0, 1, 1);
            //                Handles.SphereCap(4, _wpos, Quaternion.identity, 0.3f);
            //            }
            //            mDrawNodes.RemoveAt(i);
            //            continue;
            //        }
            //        i++;
            //    }
            //}
        }

        ////通过几何平面获取平面上的鼠标点击位置
        //Plane _plane = new Plane(new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        //float _enter;
        //if (_plane.Raycast(_ray, out _enter))
        //{
        //    //记录笔刷的世界位置
        //    mBrushWorldPosition = _ray.GetPoint(_enter);
        //    //刷新所有Scene窗口
        //    SceneView.RepaintAll();
        //}


        return _node;
    }

    public void OnDrawGizmos()
    {
        Debug.Log(this.ToString() + "OnDrawGizmos");
    }






}
