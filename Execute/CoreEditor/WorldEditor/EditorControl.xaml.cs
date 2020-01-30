using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EngineNS.IO;
using EngineNS.Profiler;

namespace CoreEditor.WorldEditor
{
    /// <summary>
    /// EditorControl.xaml 的交互逻辑
    /// </summary>
    public partial class EditorControl : UserControl, DockControl.IDockAbleControl, EngineNS.ITickInfo
    {
        #region IDockAbleControl
        public bool IsShowing { get; set; }
        public bool IsActive { get; set; }

        public string KeyValue
        {
            get;
        }
        public int Index { get; set; }

        public string DockGroup => "";

        public void EndDrag()
        {
        }

        public DockControl.IDockAbleControl LoadElement(XmlNode node)
        {
            return null;
        }

        public void SaveElement(XmlNode node, XmlHolder holder)
        {
        }

        public void StartDrag()
        {
        }
        #endregion

        public string UndoRedoKey
        {
            get { return "WorldEditOperation"; }
        }

        public bool EnableTick
        {
            get;
            set;
        }

        // 场景同时只能打开一个，所以这里用一个静态变量来判断是否处理完成
        public bool IsWorldOpening = false;
        public SceneResourceInfo CurrentResourceInfo = null;
        public EngineNS.Bricks.RecastBuilder.RCTileMeshBuilder RCTileMeshBuilder = new EngineNS.Bricks.RecastBuilder.RCTileMeshBuilder();

        EngineNS.Bricks.GraphDrawer.GraphLines GraphLines = new EngineNS.Bricks.GraphDrawer.GraphLines();
        EngineNS.Bricks.GraphDrawer.McMulLinesGen LinesGen = new EngineNS.Bricks.GraphDrawer.McMulLinesGen();

        public EditorControl()
        {
            InitializeComponent();

            EngineNS.CEngine.Instance.TickManager.AddTickInfo(this);
            VP1.UndoRedoKey = UndoRedoKey;
            CompCtrl.LinkedPropertyGrid = PG;
            CompCtrl.UndoRedoKey = UndoRedoKey;
            ModulesCtrl.LinkedPropertyGrid = PG;
            Outliner.LinkedPropertyGrid = PG;
            Outliner.UndoRedoKey = UndoRedoKey;
            Action action = async () =>
            {
                await CEditorEngine.Instance.AwaitEngineInited();


                System.Reflection.PropertyInfo prop = EngineNS.CEngine.Instance.GameEditorInstance.GetType().GetProperty("EditOperator");
                //var op = prop.GetValue(EngineNS.CEngine.Instance.GameEditorInstance, null);
                var op = EngineNS.CEngine.Instance.GameEditorInstance.EditOperator;
                if (op != null)
                {
                    ((EngineNS.Editor.GEditOperationProcessor)op).OnSelectedActorsChanged += OnSelectedActorsChanged;
                }
            };
            action();

            var editor = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
            editor.WorldEditorControl = this;

            //sceneGraph.BindViewPort(VP1);
            //sceneGraph.OnSelectSceneGraphs = SceneGraphCtrl_OnSelectSceneGraphs;
            //nodes.BindViewPort(VP1);

            LoadEditorConfig();
            NAV.Instance = RCTileMeshBuilder;

            VP1.AddNavActorEvent -= AddActorEvent;
            VP1.AddNavActorEvent += AddActorEvent;

            VP1.DRemoveActor -= DeleteActorEvent;
            VP1.DRemoveActor += DeleteActorEvent;

            VP1.OnSelectAcotrs -= SelectActors;
            VP1.OnSelectAcotrs += SelectActors;

            VP1.AddMouseUpEvent(DrawPanel_MouseUp_Default);
            //AutoBuilding.show

        }

        public void SaveEditorConfig()
        {
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.AddAttrib("RCTileMeshBuilder");
            attr.Version = 0;
            attr.BeginWrite();
            attr.WriteMetaObject(RCTileMeshBuilder);
            attr.EndWrite();

            EngineNS.IO.XndHolder.SaveXND(EngineNS.CEngine.Instance.FileManager.ProjectContent + "EditorControlConfig.dat", xnd);
        }
        public void LoadEditorConfig()
        {
            if (EngineNS.CEngine.Instance.FileManager.FileExists(EngineNS.CEngine.Instance.FileManager.ProjectContent + "EditorControlConfig.dat"))
            {
                var xnd = EngineNS.IO.XndHolder.SyncLoadXND(EngineNS.CEngine.Instance.FileManager.ProjectContent + "EditorControlConfig.dat");

                var attr = xnd.Node.FindAttrib("RCTileMeshBuilder");
                if (attr != null)
                {
                    attr.BeginRead();
                    RCTileMeshBuilder = attr.ReadMetaObject(null) as EngineNS.Bricks.RecastBuilder.RCTileMeshBuilder;
                    attr.EndRead();
                }
            }
        }

        bool mIsInitGraphLines = false;
        bool mNeedInitGraphLines = true;
        public async System.Threading.Tasks.Task InitGraphLines()
        {
            if (mIsInitGraphLines && mNeedInitGraphLines == false)
                return;

            mNeedInitGraphLines = false;

            LinesGen.Interval = 0.1f;
            LinesGen.Segement = 0.2f;
            GraphLines.LinesGen = LinesGen;
            var mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
            EngineNS.CEngine.Instance.RenderContext,
            EngineNS.RName.GetRName("editor/volume/mi_volume_octree.instmtl"));//rotator

            var init = await GraphLines.Init(mtl, 0.0f);

            GraphLines.GraphActor.Placement.Location = EngineNS.Vector3.Zero;
            VP1.World.AddEditorActor(GraphLines.GraphActor);
            //VP1.World.DefaultScene.AddActor(GraphLines.GraphActor);

            mIsInitGraphLines = true;
        }

        private void AddActorToNavModifierVolume(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (RCTileMeshBuilder.InputGeom == null)
                return;

            var component = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>();
            if (component == null)
                return;

            EngineNS.Support.CBlobObject blob = new EngineNS.Support.CBlobObject();
            EngineNS.BoundingBox box = component.GetBox();
            EngineNS.Vector3[] corners = box.GetCorners();
            EngineNS.Vector3[] Points = new EngineNS.Vector3[8 * 3];
            Points[0] = corners[0];
            Points[1] = corners[1];

            Points[2] = corners[0];
            Points[3] = corners[4];

            Points[4] = corners[0];
            Points[5] = corners[3];

            Points[6] = corners[1];
            Points[7] = corners[5];

            Points[8] = corners[1];
            Points[9] = corners[2];

            Points[10] = corners[2];
            Points[11] = corners[3];

            Points[12] = corners[2];
            Points[13] = corners[6];

            Points[14] = corners[3];
            Points[15] = corners[7];

            Points[16] = corners[4];
            Points[17] = corners[5];

            Points[18] = corners[4];
            Points[19] = corners[7];

            Points[20] = corners[5];
            Points[21] = corners[6];

            Points[22] = corners[6];
            Points[23] = corners[7];

            List<float> data = new List<float>();
            for (int i = 0; i < 24; i++)
            {
                data.Add(Points[i].X);
                data.Add(Points[i].Y);
                data.Add(Points[i].Z);
            }

            float[] blobdata = data.ToArray();
            unsafe
            {
                fixed (float* p = &blobdata[0])
                {
                    blob.PushData((IntPtr)p, (uint)(sizeof(float) * blobdata.Length));
                }
            }

            
            RCTileMeshBuilder.InputGeom.CreateConvexVolumes(component.RCAreaType, blob, ref box.Minimum, ref box.Maximum);

        }

        public void SelectActors(object sender, EditorCommon.ViewPort.ViewPortControl.SelectActorData[] e)
        {
            RefreshActorControlInfo();
        }

        public void RefreshActorControlInfo()
        {
            var actorumn = VP1.World.Actors.Count;
            var selectunm = VP1.SelectActtorCount;

            UIActorsinfo.Text = actorumn + " 个actor(选择了 " + selectunm + " )";
        }
        private List<WeakReference<EngineNS.GamePlay.Actor.GActor>> NavModifierVolumes = new List<WeakReference<EngineNS.GamePlay.Actor.GActor>>();
        private List<WeakReference<EngineNS.GamePlay.Actor.GActor>> NavMeshBoundVolumes = new List<WeakReference<EngineNS.GamePlay.Actor.GActor>>();
        private void AddActorEvent(EngineNS.GamePlay.Actor.GActor actor)
        {
            // return;
            var component = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>();
            if (component != null)
            {
                //if (RCTileMeshBuilder.InputGeom == null)
                //{
                //    var result = BuildNavtion();
                //}

                AddActorToNavModifierVolume(actor);
                //var test = BuildNavMesh();

                if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.NoWalk)
                {
                    actor.PlacementChange -= RefreshNavModifierVolumes;
                    actor.PlacementChange += RefreshNavModifierVolumes;

                    NavModifierVolumes.Add(new WeakReference<EngineNS.GamePlay.Actor.GActor>(actor));
                }
                else if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.Walk)
                {
                    actor.PlacementChange -= RefreshNavMeshBoundVolumes;
                    actor.PlacementChange += RefreshNavMeshBoundVolumes;

                    NavMeshBoundVolumes.Add(new WeakReference<EngineNS.GamePlay.Actor.GActor>(actor));
                }
            }

            RefreshActorControlInfo();
        }

        private void DeleteActorEvent(EngineNS.GamePlay.Actor.GActor actor)
        {
            RefreshActorControlInfo();

            if (RCTileMeshBuilder.InputGeom == null)
                return;

            var component = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>();
            if (component != null)
            {
                RCTileMeshBuilder.InputGeom.DeleteConvexVolumesByArea(component.RCAreaType);
                if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.NoWalk)
                {
                    actor.PlacementChange -= RefreshNavModifierVolumes;
                    RefreshNavModifierVolumes();
                }
                else if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.Walk)
                {
                    actor.PlacementChange -= RefreshNavMeshBoundVolumes;
                    RefreshNavMeshBoundVolumes();
                }
            }
        }

        private void RefreshNavModifierVolumes()
        {
            if (RCTileMeshBuilder.InputGeom == null)
                return;

            RCTileMeshBuilder.InputGeom.DeleteConvexVolumesByArea(0);
 
            foreach (var weak in NavModifierVolumes)
            {
                EngineNS.GamePlay.Actor.GActor actor;
                if (weak.TryGetTarget(out actor))
                {
                    AddActorToNavModifierVolume(actor);
                }
            }

            var test = BuildNavMesh();
        }
        private void RefreshNavMeshBoundVolumes()
        {
            if (RCTileMeshBuilder.InputGeom == null)
                return;

            RCTileMeshBuilder.InputGeom.DeleteConvexVolumesByArea(EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.Walk);

            foreach (var weak in NavMeshBoundVolumes)
            {
                EngineNS.GamePlay.Actor.GActor actor;
                if (weak.TryGetTarget(out actor))
                {
                    AddActorToNavModifierVolume(actor);
                }
            }

            var test = BuildNavMesh();
        }


        void SceneGraphCtrl_OnSelectSceneGraphs(List<EngineNS.GamePlay.SceneGraph.ISceneNode> graphNodes)
        {
            //nodes.UnSelectAll();
            VP1.SelectActor(null);
            PG.Instance = graphNodes.ToArray();
            if(graphNodes.Count > 0)
            {
                var scene = graphNodes[0] as EngineNS.GamePlay.SceneGraph.GSceneGraph;
                if (scene != null)
                {
                    ModulesCtrl.SetSceneGraph(scene);
                    ModulesCtrl.Visibility = Visibility.Visible;
                }
                else
                    ModulesCtrl.Visibility = Visibility.Collapsed;
            }
            CompCtrl.Visibility = Visibility.Collapsed;
        }

        public void OnSelectedActorsChanged(object sender, List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            if (sender != VP1)
                return;
            if (actors.Count > 0)
            {
                //sceneGraph.UnSelectAll();
            }
            //nodes.FocusActorItems(actors);
            bool haveInvisible = false;
            List<EngineNS.GamePlay.Actor.GActor> showActors = new List<EngineNS.GamePlay.Actor.GActor>();
            for(int i = 0;i<actors.Count;++i)
            {
                if(!(actors[i].Tag is EditorCommon.Controls.Outliner.InvisibleInOutliner))
                {
                    showActors.Add(actors[i]);
                }
                else
                {
                    haveInvisible = true;
                }
            }
            if (haveInvisible && showActors.Count == 0)
            {

            }
            else
            {
                this.PG.Instance = showActors;
                this.CompCtrl.SetActors(showActors);
                if (showActors == null || showActors.Count == 0)
                    CompCtrl.Visibility = Visibility.Collapsed;
                else
                {
                    CompCtrl.Visibility = Visibility.Visible;
                    ModulesCtrl.Visibility = Visibility.Collapsed;
                }
            }

            RefreshActorControlInfo();
        }
        bool mMenuInitialized = false;
        void InitMenus()
        {
            if (mMenuInitialized)
                return;

            mMenuInitialized = true;
            var menuDatas = new Dictionary<string, EditorCommon.Menu.MenuItemDataBase>();
            var menuData = new EditorCommon.Menu.MenuItemData_ShowHideControl("Viewport");
            menuData.MenuNames = new string[] { "Window", "Level Editor|Viewports", "Viewport" };
            menuData.Count = 4;
            menuData.OperationControlType = typeof(EditorCommon.ViewPort.ViewPortControl);
            menuData.Icons = new ImageSource[] { new BitmapImage(new Uri("/ResourceLibrary;component/Icons/Icons/icon_tab_Viewports_40x.png", UriKind.Relative)) };
            menuDatas[menuData.KeyName] = menuData;

            EditorCommon.Menu.GeneralMenuManager.GenerateMenuItems(Menu_Main, menuDatas);
            menuData.BindOperationControl(0, VP1);

            EditorCommon.Menu.GeneralMenuManager.Instance.GenerateGeneralMenuItems(Menu_Main);

            var cbData = EditorCommon.Menu.GeneralMenuManager.Instance.GetMenuData("ContentBrowser") as EditorCommon.Menu.MenuItemData_ShowHideControl;
            cbData?.BindOperationControl(BrowserCtrl.Index, BrowserCtrl);
            //nodes.BindViewPort(VP1);
        }

        EngineNS.GamePlay.SceneGraph.GSceneGraph mProgressScene;
        public void ShowLoadingProgress(EngineNS.GamePlay.SceneGraph.GSceneGraph scene)
        {
            mProgressScene = scene;
            ProgressBar_Loading.Visibility = Visibility.Visible;
            ProgressBar_Loading.Value = 0;
            EnableTick = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitMenus();
            EngineDesc.Instance = EngineNS.CEngine.Instance.Desc;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Buildnavmesh_Click(object sender, RoutedEventArgs e)
        {
            var test = BuildNavtion();

            SaveEditorConfig();
        }

        private void Buildnavquery_Click(object sender, RoutedEventArgs e)
        {
            var test = AddNavMeshToWorld();
        }

        private async System.Threading.Tasks.Task BuildNavtion()
        {
            var meshList = new List<EngineNS.Graphics.Mesh.CGfxMeshPrimitives>();
            var matrixList = new List<EngineNS.Matrix>();

            List<EngineNS.Bricks.PhysicsCore.CPhyShape> physhapes = new List<EngineNS.Bricks.PhysicsCore.CPhyShape>();
            using (var i = VP1.World.DefaultScene.Actors.GetEnumerator())
            {
                while (i.MoveNext())
                {
                    EngineNS.GamePlay.Actor.GActor actor = i.Current.Value;
                    //if ((actor.mComponentFlags & EngineNS.GamePlay.Actor.GActor.EComponentFlags.HasPhysics)
                    EngineNS.Bricks.PhysicsCore.GPhysicsComponent component = actor.GetComponentRecursion<EngineNS.Bricks.PhysicsCore.GPhysicsComponent>();
                    EngineNS.Bricks.PhysicsCore.CollisionComponent.GPhysicsCollisionComponent component2 = actor.GetComponentRecursion<EngineNS.Bricks.PhysicsCore.CollisionComponent.GPhysicsCollisionComponent>();
                    if ((component != null && component.PhyActor != null && component.IsEnableNavgation) || (component2 != null && component2.IsEnableNavgation))
                    {
                        //foreach (EngineNS.Bricks.PhysicsCore.CPhyShape shape in component.PhyActor.Shapes)
                        //{
                        //    if (shape.DebugActor == null)
                        //    {
                        //        shape.OnEditorCommitVisual(EngineNS.CEngine.Instance.RenderContext, VP1.Camera, actor);
                        //    }

                        //    if (shape.DebugActor != null)
                        //    {
                        //        EngineNS.GamePlay.Component.GMeshComponent meshcomponent = shape.DebugActor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                        //        meshList.Add(meshcomponent.SceneMesh.MeshPrimitives);
                        //        matrixList.Add(actor.Placement.WorldMatrix);
                        //    }

                        //}

                        //if (component.PhyActor != null && component.PhyActor.Shapes != null)
                        //{
                        //foreach (var shape in component.PhyActor.Shapes)
                        //{
                        //    if (shape.AreaType > 1 && shape.AreaType < 16)
                        //    {
                        //        physhapes.Add(shape);
                        //        if (shape.DebugActor == null)
                        //        {
                        //            shape.OnEditorCommitVisual(EngineNS.CEngine.Instance.RenderContext, VP1.Camera, actor);
                        //        }
                        //    }
                        //}
                        //}
                        EngineNS.GamePlay.Component.GMeshComponent meshcomponent = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                        if (meshcomponent != null)
                        {
                            meshList.Add(meshcomponent.SceneMesh.MeshPrimitives);
                            matrixList.Add(actor.Placement.WorldMatrix);
                        }


                    }
                }
            }

            //判断为空就不做任何操作了
            if (meshList.Count == 0)
                return;

            EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
            var mesh = EngineNS.CGeometryMesh.MergeGeoms(EngineNS.CEngine.Instance.RenderContext, meshList, matrixList, ref aabb);
            var geom = new EngineNS.Bricks.RecastBuilder.RCInputGeom();
            geom.LoadMesh(EngineNS.CEngine.Instance.RenderContext, mesh, 1);
            //处理动态区域
            foreach (var shape in physhapes)
            {        
                //EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
                //await shape.AwaitLoadDebugActor();
                //shape.DebugActor.GetAABB(ref aabb);
                //EngineNS.Vector3[] vpoints = aabb.GetCorners();
                //float[] points = new float[vpoints.Length * 3];
                //for (int i = 0; i < vpoints.Length; i++)
                //{
                //    points[i * 3] = vpoints[i].X;
                //    points[i * 3 + 1] = vpoints[i].Y;
                //    points[i * 3 + 2] = vpoints[i].Z;
                //}

                //EngineNS.Support.CBlobObject blob = new EngineNS.Support.CBlobObject();
                //unsafe
                //{
                //    fixed (float* p = &points[0])
                //    {
                //        IntPtr ptr = (IntPtr)p;
                //        blob.PushData(ptr, (uint)(sizeof(float) * points.Length));
                //    }
                //}
                //geom.CreateConvexVolumes(shape.AreaType, blob);
            }
            RCTileMeshBuilder.InputGeom = geom;

            //单独放这里处理一次 不能放在BuildNavMesh
            foreach (var weak in NavMeshBoundVolumes)
            {
                EngineNS.GamePlay.Actor.GActor actor;
                if (weak.TryGetTarget(out actor))
                {
                    AddActorToNavModifierVolume(actor);
                }
            }
  
            foreach (var weak in NavModifierVolumes)
            {
                EngineNS.GamePlay.Actor.GActor actor;
                if (weak.TryGetTarget(out actor))
                {
                    AddActorToNavModifierVolume(actor);
                }
            }
            await BuildNavMesh();
        }

        public void DrawPanel_MouseUp_Default(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var actordatas = VP1.GetSelectedActors();
            if (actordatas != null && actordatas.Length == 1)
            {
                if (actordatas[0].Actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>() != null)
                {
                    var test = BuildNavMesh(true);
                }
            }
        }
        private async System.Threading.Tasks.Task BuildNavMesh(bool force = false)
        {
            if (force == false)
            {
                if (VP1.IsMouseDown)
                    return;
            }

            if (RCTileMeshBuilder.InputGeom == null)
                return;
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                //处理offmeshlink
                if (VP1.World.DefaultScene.NavAreaActor != null)
                {
                    foreach (var actor in VP1.World.DefaultScene.NavAreaActor.GetChildrenUnsafe())
                    {
                        EngineNS.Bricks.RecastRuntime.NavLinkProxyComponent NavLinkProxyComponent = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavLinkProxyComponent>();
                        if (NavLinkProxyComponent != null)
                        {
                            RCTileMeshBuilder.InputGeom.AddOffMeshConnections(NavLinkProxyComponent.StartPos, NavLinkProxyComponent.EndPos, NavLinkProxyComponent.Radius, NavLinkProxyComponent.Direction);
                        }
                    }
                }

                VP1.World.DefaultScene.NavMesh = RCTileMeshBuilder.BuildNavi();

                EngineNS.Graphics.Mesh.CGfxMeshPrimitives pri = VP1.World.DefaultScene.NavMesh.CreateRenderMeshPrimitives(EngineNS.CEngine.Instance.RenderContext);
                if (pri == null)
                    return false;
                EngineNS.GamePlay.Actor.GActor debugactor = await VP1.World.DefaultScene.NavMesh.CreateRenderActor(EngineNS.CEngine.Instance.RenderContext, pri);
                if (VP1.World.NavMeshActorID != null)
                {
                    VP1.World.RemoveEditorActor(VP1.World.NavMeshActorID);
                }
                debugactor.Visible = EngineNS.CEngine.IsRenderNavMesh;
                VP1.World.AddEditorActor(debugactor);
                //if (VP1.World.DefaultScene.NavAreaActor == null)
                //{
                //    VP1.World.DefaultScene.CreateNavActor();
                //}
                //VP1.World.DefaultScene.NavAreaActor.Children.Add(debugactor);
                //VP1.DRefreshActors();
                VP1.World.NavMeshActorID = debugactor.ActorId;
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
           
        }

        private void Build_GeoScene_Click(object sender, RoutedEventArgs e)
        {
            BuildGeoScene(VP1.World.DefaultScene);
        }

        public async System.Threading.Tasks.Task AddNavMeshToWorld()
        {
            if (VP1.World.DefaultScene.NavMesh == null)
                return;

            EngineNS.Bricks.RecastRuntime.CNavMesh navmesh = VP1.World.DefaultScene.NavMesh;
            //EngineNS.GamePlay.Actor.GActor navMeshActor = await EngineNS.GamePlay.Actor.GActor.NewNavMeshActorAsync(navmesh);
            //navMeshActor.Placement.Location = new EngineNS.Vector3(0.0f, 0.0f, 0.0f);
            //navMeshActor.Placement.Rotation = EngineNS.Quaternion.RotationAxis(EngineNS.Vector3.UnitY, 0.0f);
            //navMeshActor.Placement.Scale = new EngineNS.Vector3(1.0f, 1.0f, 1.0f);
            //VP1.World.AddActor(navMeshActor);
            //VP1.World.DefaultScene.AddActor(navMeshActor);


            //EngineNS.Graphics.Mesh.CGfxMeshPrimitives pri = navmesh.CreateRenderMeshPrimitives(EngineNS.CEngine.Instance.RenderContext);
            //EngineNS.GamePlay.Actor.GActor debugactor = await navmesh.CreateRenderActor(EngineNS.CEngine.Instance.RenderContext, pri);
            //VP1.World.AddActor(debugactor);
            //VP1.World.DefaultScene.AddActor(debugactor);

            //VP1.World.DefaultScene.NavMesh = navmesh;
            VP1.World.DefaultScene.NavQuery = navmesh.CreateQuery(RCTileMeshBuilder.MaxNodes);//[Limits: 0 < value <= 65535]

            VP1.World.DefaultScene.CreateNavCrowd(VP1.World.DefaultScene.NavQuery, VP1.World.DefaultScene.NavMesh, 5.0f);
            //VP1.World.DefaultScene.NavCrowd.AddAgent()
        }

        public bool? CanClose()
        {
            return false;
        }

        public void RefreshWhenWorldLoaded()
        {
            PG.Instance = null;
            CompCtrl.SetActors(null);
            //nodes.RefreshActors();
            //sceneGraph.RefreshFromWorld(VP1.World);
            Outliner.BindingWorld(VP1.World);
            Outliner.SetViewPort(VP1);
            CompCtrl.ViewPort = VP1;
            if (VP1.World.DefaultScene.SunActor != null)
            {
                var sunComp = VP1.World.DefaultScene.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                if (sunComp != null)
                {
                    var editorInstance = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
                    sunComp.View = editorInstance.RenderPolicy.BaseSceneView;
                }
            }

            VP1.World.RemoveEditorActor(VP1.World.NavMeshActorID);
            NavModifierVolumes.Clear();
            NavMeshBoundVolumes.Clear();
            if (VP1.World.DefaultScene.NavAreaActor != null)
            {
                //var test = BuildNavtion();
                List<EngineNS.GamePlay.Actor.GActor> Children = VP1.World.DefaultScene.NavAreaActor.GetChildrenUnsafe();
                foreach (var actor in Children)
                {
                    var component = actor.GetComponent<EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent>();
                    if (component != null)
                    {
                       
                        if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.NoWalk)
                        {
                            actor.PlacementChange -= RefreshNavModifierVolumes;
                            actor.PlacementChange += RefreshNavModifierVolumes;

                            NavModifierVolumes.Add(new WeakReference<EngineNS.GamePlay.Actor.GActor>(actor));
                        }
                        else if (component.RCAreaType == EngineNS.Bricks.RecastRuntime.NavMeshBoundVolumeComponent.AreaType.Walk)
                        {
                            actor.PlacementChange -= RefreshNavMeshBoundVolumes;
                            actor.PlacementChange += RefreshNavMeshBoundVolumes;

                            NavMeshBoundVolumes.Add(new WeakReference<EngineNS.GamePlay.Actor.GActor>(actor));
                        }

                        if (RCTileMeshBuilder.InputGeom == null)
                        {
                            //var result = BuildNavtion();
                        }
                        AddActorToNavModifierVolume(actor);
                    }
                }

                //test = BuildNavMesh();
            }

            var test = CreateNavRenderMesh();
            RefreshActorControlInfo();
        }

        private async Task CreateNavRenderMesh()
        {
            if (VP1.World.DefaultScene.NavMesh != null)
            {
                EngineNS.Graphics.Mesh.CGfxMeshPrimitives pri = VP1.World.DefaultScene.NavMesh.CreateRenderMeshPrimitives(EngineNS.CEngine.Instance.RenderContext);
                if (pri == null)
                    return;
                EngineNS.GamePlay.Actor.GActor debugactor = await VP1.World.DefaultScene.NavMesh.CreateRenderActor(EngineNS.CEngine.Instance.RenderContext, pri);
                if (VP1.World.NavMeshActorID != null)
                {
                    VP1.World.RemoveEditorActor(VP1.World.NavMeshActorID);
                }

                debugactor.Visible = EngineNS.CEngine.IsRenderNavMesh;
                VP1.World.AddEditorActor(debugactor);

                VP1.World.NavMeshActorID = debugactor.ActorId;
            }
        }

        public void Closed() { }

        private void IconTextBtn_Save_Click(object sender, RoutedEventArgs e)
        {
            var noUse = Save();
        }

        async Task Save()
        {
            var editor = EngineNS.CEngine.Instance.GameEditorInstance as CoreEditor.CEditorInstance;
            using (var i = editor.World.GetSceneEnumerator())
            {
                while(i.MoveNext())
                {
                    var scene = i.Current.Value;
                    if (scene.SceneFilename == null)
                        continue;

                    await scene.SaveScene(scene.SceneFilename, VP1.Camera);

                    // 生成缩略图
                    var snapShotFile = scene.SceneFilename.Address + EditorCommon.Program.SnapshotExt;
                    var data = new EngineNS.Support.CBlobObject[1];
                    data[0] = new EngineNS.Support.CBlobObject();
                    var rc = EngineNS.CEngine.Instance.RenderContext;
                    VP1.RPolicy.mCopyPostprocessPass.mScreenView.FrameBuffer.GetSRV_RenderTarget(0).Save2Memory(rc, data[0], EngineNS.EIMAGE_FILE_FORMAT.PNG);
                    EngineNS.CShaderResourceView.SaveSnap(snapShotFile, data);
                }
            }

            await CurrentResourceInfo?.Save(true);
        }

        private void IconTextBtn_Build_Click(object sender, RoutedEventArgs e)
        {
            var noUse = BuildScene();
            // PVS废弃
            //var buildDesc = new EngineNS.Bricks.OcclusionPVS.BuildDesc();
            //buildDesc.ResetDefault();

            //var rc = EngineNS.CEngine.Instance.RenderContext;
            //foreach (var sc in VP1.World.Scenes)
            //{
            //    if (string.IsNullOrEmpty(sc.Value.SceneFilename.Name))
            //        throw new InvalidOperationException();

            //    var builder = new EngineNS.Bricks.OcclusionPVS.CPvsBuilder();
            //    builder.Init(ref buildDesc);

            //    List<Guid> actorsId = new List<Guid>();
            //    foreach (var actor in sc.Value.Actors)
            //    {
            //        // 计算Id和Guid对应关系
            //        actorsId.Add(actor.Value.ActorId);
            //        var actorId = actorsId.Count - 1;
            //        foreach (var comp in actor.Value.Components)
            //        {
            //            var meshComp = comp.Value as EngineNS.GamePlay.Component.GMeshComponent;
            //            if (meshComp == null)
            //                continue;

            //            var mat = actor.Value.Placement.WorldMatrix;
            //            builder.AddModelInstance(rc, meshComp.SceneMesh.MeshPrimitives, actorId, ref mat, true);
            //        }
            //    }

            //    var xndHolder = XndHolder.NewXNDHolder();
            //    var pvsDataNode = xndHolder.Node.AddNode("PVSData", 0, 0);
            //    var idsAtt = pvsDataNode.AddAttrib("Ids");
            //    idsAtt.BeginWrite();
            //    idsAtt.Write((int)actorsId.Count);
            //    foreach (var id in actorsId)
            //    {
            //        idsAtt.Write(id);
            //    }
            //    idsAtt.EndWrite();
            //    builder.Build(pvsDataNode);

            //    var pvsFile = sc.Value.SceneFilename.Address + "/pvs.dat";
            //    XndHolder.SaveXND(pvsFile, xndHolder);
            //}
        }
        public void BeforeFrame()
        {

        }
        public void TickLogic()
        {
            if(mProgressScene != null)
            {
                var noUse = EngineNS.CEngine.Instance.EventPoster.Post(() =>
                {
                    ProgressBar_Loading.Value = mProgressScene.LoadingProgress;
                    if (ProgressBar_Loading.Value >= 1)
                    {
                        ProgressBar_Loading.Visibility = Visibility.Collapsed;
                        EnableTick = false;
                    }
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }

        public void TickRender()
        {
        }

        public void TickSync()
        {
        }

        public TimeScope GetLogicTimeScope()
        {
            return null;
        }

    }
}
