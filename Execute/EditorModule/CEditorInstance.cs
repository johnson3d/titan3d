using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace CoreEditor
{
    public partial class CEditorInstance : EngineNS.Editor.CEditorInstance, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        bool mShowEngineContent = false;
        public bool ShowEngineContent
        {
            get => mShowEngineContent;
            set
            {
                mShowEngineContent = value;
                OnPropertyChanged("ShowEngineContent");
            }
        }

        bool mShowEditorContent = false;
        public bool ShowEditorContent
        {
            get => mShowEditorContent;
            set
            {
                mShowEditorContent = value;
                OnPropertyChanged("ShowEditorContent");
            }
        }

        public EngineNS.GamePlay.Actor.GActor ActorBeauty0;
        public EngineNS.GamePlay.Actor.GActor ActorBeauty1;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew0;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew1;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew2;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew3;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew4;
        public EngineNS.GamePlay.Actor.GActor ActorBeautyNew5;
        public EngineNS.GamePlay.Actor.GActor ActorRobot0;
        public EngineNS.GamePlay.Actor.GActor ActorRobot1;
        public EngineNS.GamePlay.Actor.GActor ActorLampstandard0;

        public EngineNS.GamePlay.Actor.GActor ActorBadman0;

        public EngineNS.GamePlay.Actor.GActor ActorFont0;

        public EngineNS.GamePlay.Actor.GActor ActorEye0;
        public EngineNS.GamePlay.Actor.GActor ActorEye1;
        public EngineNS.GamePlay.Actor.GActor ActorEye2;

        public EngineNS.GamePlay.Actor.GActor ActorStandardBox0;

        public EngineNS.GamePlay.Actor.GActor ActorRoom;
        
        public CoreEditor.WorldEditor.EditorControl WorldEditorControl;

        bool mInitialized = false;
        EditorCommon.ViewPort.ViewPortControl MainEdViewport;

        EngineNS.Bricks.GraphDrawer.GraphLines LineDrawer = null;
        public override EngineNS.Graphics.CGfxCamera GetMainViewCamera()
        {
            if (MainEdViewport == null)
                return null;
            return MainEdViewport.Camera;
        }
        public override void SelectWorldActors(List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            var datas = new EditorCommon.ViewPort.ViewPortControl.SelectActorData[actors.Count];
            for(int i=0; i<datas.Length; i++)
            {
                datas[i] = new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                {
                    Actor = actors[i],
                    StartTransMatrix = actors[i].Placement.Transform,
                };
            }
            MainEdViewport.SelectActors(datas);
        }
        public override void SelectWorldActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            MainEdViewport.SelectActor(actor);
        }
        public async System.Threading.Tasks.Task InitEditor(EditorCommon.ViewPort.ViewPortControl editViewport, EngineNS.GamePlay.GGameInstanceDesc desc)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            await EngineNS.CEngine.Instance.OnEngineInited();

            InitWorld(desc);

            var scene = await LoadScene(CEngine.Instance.RenderContext, desc.SceneName);
            if (scene != null)
            {
                mWorld.AddScene(desc.SceneName, scene);
            }

            MainEdViewport = editViewport;
            await MainEdViewport.WaitInitComplated();
            //if(false == await MainEdViewport.InitEnviroment())
            //{
            //    int xxx = 0;
            //}

            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 1.5f, -3.6f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 1.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            MainEdViewport.Camera.LookAtLH(Eye, At, Up);

            MainEdViewport.World = this.World;

            MainEdViewport.RPolicy = new EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile();
            this.RenderPolicy = MainEdViewport.RPolicy;

            await ((EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile)MainEdViewport.RPolicy).Init(RHICtx,
                (uint)(float)MainEdViewport.GetViewPortWidth(), (uint)(float)MainEdViewport.GetViewPortHeight(),
                MainEdViewport.Camera, MainEdViewport.DrawHandle);

            ((EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile)MainEdViewport.RPolicy).mHitProxy.mEnabled = true;

            //if (EngineNS.CEngine.Instance.RenderSystem.RHIType == ERHIType.RHT_D3D11)
            //{
            //    ((EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile)MainEdViewport.RPolicy).mHitProxy.mEnabled = true;
            //}
            //else
            //{
            //    ((EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile)MainEdViewport.RPolicy).mHitProxy.mEnabled = false;
            //}
            
            MainEdViewport.TickLogicEvent = TickMainEdViewport;

            CEngine.Instance.TickManager.AddTickInfo(this);

            var sg = this.World.GetScene(desc.SceneName);
            
            if (false)
            {
                var particle = await EngineNS.GamePlay.McGameInstance.CreateParticleActor2(RName.GetRName("Mesh/sphere001.gms"), RName.GetRName("Macross/particletest.macross"), true);
                particle.Placement.Location = new Vector3(0,10,0);
                this.World.AddActor(particle);
                this.World.GetScene(desc.SceneName).AddActor(particle);
            }

            EngineNS.Profiler.NativeMemory.BeginProfile();

            //CEngine.IsWriteEtc = true;
            //EngineNS.BitmapProc.RefreshAllTxPic();

            //用来处理特殊情况，需要刷新所有存盘数据的代码
            if (false)
            {
                //EngineNS.Rtti.RttiHelper.CacheCleanHistory = true;
                //EngineNS.Rtti.RttiHelper.CanCleanHistoryMetaDatas.Clear();
                //await CEngine.Instance.RefreshAllSaveFiles();
                //await Macross.Program.RefreshSaveFiles();
                //foreach (var plugin in EditorCommon.PluginAssist.PluginManager.Instance.Plugins)
                //{
                //    var rsf = plugin.Value as EditorCommon.PluginAssist.IRefreshSaveFiles;
                //    if (rsf == null)
                //        continue;

                //    await rsf.RefreshSaveFiles();
                //}
                //todo 删除多余的重定向信息，现在还没有做好
                //EngineNS.Rtti.RttiHelper.CacheCleanHistory = false;
                //foreach(var i in EngineNS.Rtti.RttiHelper.CanCleanHistoryMetaDatas)
                //{
                //    bool isRedirection;
                //    try
                //    {
                //        var t1 = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(i.Value, out isRedirection);
                //        if (t1 == null)
                //        {
                //            continue;
                //        }
                //        if (isRedirection)
                //        {
                //            var t = EngineNS.CEngine.Instance.MetaClassManager.FindMetaClass(i.Value);
                //            //清理重定向表
                //            System.Diagnostics.Debug.WriteLine(i.Value);
                //        }
                //        else
                //        {
                //            var t = EngineNS.CEngine.Instance.MetaClassManager.FindMetaClass(t1);
                //            System.Diagnostics.Debug.WriteLine(i.Value);
                //        }
                //    }
                //    catch(Exception ex)
                //    {
                //        EngineNS.Profiler.Log.WriteException(ex);
                //    }
                //}
                //EngineNS.Rtti.RttiHelper.CanCleanHistoryMetaDatas.Clear();
            }

            await EngineNS.CEngine.Instance.EventPoster.AwaitAsyncIOEmpty();
            EngineNS.CEngine.Instance.MacrossDataManager.RefreshMacrossCollector();
            mInitialized = true;

            if (CEngine.Instance.McEngineGetter != null && CEngine.Instance.McEngineGetter.Get(false) != null)
            {
                mWorld.ClearAllScenes();
                await CEngine.Instance.McEngineGetter.Get(false).OnEditorStarted(CEngine.Instance);
            }
            //await CreateExtActors(RHICtx);

            await Init_SYJ();
        }
        private async System.Threading.Tasks.Task CreateExtActors(EngineNS.CRenderContext RHICtx)
        {
            var sg = this.World.DefaultScene;

            {//Sky
                TheSky = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/sky.gms"));
                TheSky.Placement.Scale = new Vector3(0.1F, 0.1F, 0.1F);
                this.World.AddActor(TheSky);
                sg.AddActor(TheSky);
            }

            //if (false)
            {//Room
                var phyCtx = EngineNS.CEngine.Instance.PhyContext;

                var phymtl = phyCtx.LoadMaterial(RName.GetRName("Physics/PhyMtl/default.phymtl"));
                if (phymtl == null)
                {
                    phymtl = phyCtx.CreateMaterial(1, 1, 0.9F);
                    phymtl.Save2Xnd(RName.GetRName("Physics/PhyMtl/default.phymtl"));
                }

                ActorRoom = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/room.gms"));
                ActorRoom.Placement.Location = new Vector3(0.0f, 0.0f, 0.0f);
                ActorRoom.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 0.0f);
                ActorRoom.Placement.Scale /= 40.0f;
                ActorRoom.Tag = "Room";

                var phyComp = await EngineNS.Bricks.PhysicsCore.GPhysicsComponent.CreatePhysicsComponent(ActorRoom,
                    RName.GetRName("Physics/PhyMtl/default.phymtl"),
                    EngineNS.Bricks.PhysicsCore.EPhyActorType.PAT_Static,
                    EngineNS.Bricks.PhysicsCore.EPhysShapeType.PST_TriangleMesh,
                    RName.GetRName("Mesh/room.vms.phygeom"),
                    ActorRoom.Placement.Scale);

                CEngine.Instance.HitProxyManager.MapActor(ActorRoom);
                this.World.AddActor(ActorRoom);
                sg.AddActor(ActorRoom);

                if(false)
                {
                    var roomMesh = ActorRoom.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
                    var meshData = new EngineNS.Graphics.Mesh.CGfxMeshDataProvider();
                    meshData.InitFromMesh(RHICtx, roomMesh.SceneMesh.MeshPrimitives);
                    UInt32 a = 0, b = 0, c = 0;
                    meshData.GetTriangle(0, ref a, ref b, ref c);
                    var vA = meshData.GetPositionOrNormal(EVertexSteamType.VST_Position, a);
                    var vB = meshData.GetPositionOrNormal(EVertexSteamType.VST_Position, b);
                    var vC = meshData.GetPositionOrNormal(EVertexSteamType.VST_Position, c);
                }
                

                if (false)
                {
                    var meshSource = ActorRoom.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>().SceneMesh.MeshPrimitives;
                    meshSource.CookAndSavePhyiscsGeomAsTriMesh(RHICtx, phyCtx);
                }

                if (false)
                {//Recast
                    var rcBuilder = new EngineNS.Bricks.RecastBuilder.RCTileMeshBuilder();
                    rcBuilder.AgentHeight = 2.2f;
                    rcBuilder.AgentMaxClimb = 1.0f;
                    rcBuilder.AgentRadius = 0.25f;
                    //rcBuilder.CellSize = 0.3f;
                    //rcBuilder.EdgeMaxLen = 48.0f;
                    //rcBuilder.RegionMergeSize = 640.0f;
                    //rcBuilder.RegionMinSize = 64.0f;
                    var geom = new EngineNS.Bricks.RecastBuilder.RCInputGeom();
                    var rc = CEngine.Instance.RenderContext;
                    var meshList = new List<EngineNS.Graphics.Mesh.CGfxMeshPrimitives>();
                    var mesh1 = ActorRoom.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>().SceneMesh.MeshPrimitives;
                    var mesh2 = CEngine.Instance.MeshPrimitivesManager.GetMeshPrimitives(RHICtx, RName.GetRName("Mesh/box.vms"), true);
                    meshList.Add(mesh1);
                    meshList.Add(mesh2);
                    var matrixList = new List<Matrix>();
                    matrixList.Add(Matrix.Scaling(1 / 40.0f));
                    matrixList.Add(Matrix.Scaling(5.0f));

                    EngineNS.BoundingBox aabb = new EngineNS.BoundingBox();
                    var mesh = CGeometryMesh.MergeGeoms(RHICtx, meshList, matrixList, ref aabb);
                    geom.LoadMesh(rc, mesh, 1);
                    rcBuilder.InputGeom = geom;
                    var navMesh = rcBuilder.BuildNavi();
                    var navMeshActor = await EngineNS.GamePlay.Actor.GActor.NewNavMeshActorAsync(navMesh);

                    navMeshActor.Placement.Location = new Vector3(0.0f, 0.0f, 0.0f);
                    navMeshActor.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitY, 0.0f);
                    navMeshActor.Placement.Scale = new Vector3(1.0f, 1.0f, 1.0f);
                    this.World.AddActor(navMeshActor);
                    sg.AddActor(navMeshActor);

                    EngineNS.Bricks.HollowMaker.Agent agent = new EngineNS.Bricks.HollowMaker.Agent();

                    EngineNS.Bricks.RecastRuntime.CNavQuery navquery = navMesh.CreateQuery(65535);//[Limits: 0 < value <= 65535]

                    //await agent.BuildGeoScene(this.World, navMesh, navquery, 100.0f);
                }
            }

            {
                var phyCtx = EngineNS.CEngine.Instance.PhyContext;
                var phymtl = phyCtx.CreateMaterial(1, 1, 0.9F);
                
                var frame1 = CEngine.Instance.FrameCount;
                ActorFont0 = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(RName.GetRName("Mesh/text.gms"));
                var frame2 = CEngine.Instance.FrameCount;
                ActorFont0.Placement.Location = new Vector3(0.0f, 10.8f, 3.0f);
                ActorFont0.Placement.Rotation = Quaternion.RotationAxis(Vector3.UnitX, 3.14f);
                ActorFont0.Placement.Scale /= 100.0f;
                ActorFont0.Tag = "Text";
                CEngine.Instance.HitProxyManager.MapActor(ActorFont0);
                this.World.AddActor(ActorFont0);
                sg.AddActor(ActorFont0);
                {
                    var phyComp = await EngineNS.Bricks.PhysicsCore.GPhysicsComponent.CreatePhysicsComponent(ActorFont0,
                            RName.GetRName("Physics/PhyMtl/default.phymtl"),
                            EngineNS.Bricks.PhysicsCore.EPhyActorType.PAT_Dynamic,
                            EngineNS.Bricks.PhysicsCore.EPhysShapeType.PST_Sphere,
                            null,
                            Vector3.UnitXYZ,
                            100.0f);
                }

             
            }

            //Line
            {
                var start = new Vector3(1, 1, 0);
                LineDrawer = await EngineNS.Bricks.GraphDrawer.GraphLinesHelper.Init(start);
                LineDrawer.GraphActor.Placement.Location = Vector3.Zero;
                World.AddActor(LineDrawer.GraphActor);
                sg.AddActor(LineDrawer.GraphActor);
            }

            if (sg.SunActor != null)
            {
                var sunComp = sg.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                if (sunComp != null)
                {
                    sunComp.View = this.RenderPolicy.BaseSceneView;
                }
            }
        }

   
        private async System.Threading.Tasks.Task DoUnitTest()
        {
            await new EngineNS.Bricks.HotfixScript.CSharpScriptHelper().DoTest();
            await new EngineNS.Bricks.GraphDrawer.GraphLinesHelper().DoTest();
            await new EngineNS.Bricks.ExcelTable.ExcelHelper().DoTest();
            await new EngineNS.Bricks.Particle.McParticleHelper().DoTest();
            await new EngineNS.Bricks.RecastBuilder.RCTileMeshHelper().DoTest();
            //await new SuperSocket.SuperSocketHelper().DoTest();
        }
        public void Cleanup()
        {
            MainEdViewport?.Cleanup();
        }

        float t = 0.0f;
        struct LightStruct
        {
            public Vector4 Diffuse;
            public Vector4 Specular;
            public Vector3 Position;
            public float Shiness;
        }
        Vector3 dir = new Vector3(1, 0, 0);
        public void TickMainEdViewport(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            if (!mInitialized)
                return;
            if (EditorCommon.GamePlay.Instance.IsInPIEMode)
                return;

            EngineNS.CEngine.Instance.ThreadRender.PostRenderAction(() =>
            {
                OnCommitRender(vpc);
            });

            OnEditTick(vpc);

            EngineNS.CEngine.Instance.ThreadRender.WaitRender();
        }

        /* Test Computer Shader 
        */
        public override void TickRender()
        {
            base.TickRender();

            //// 调试代码，方便抓帧
            //TestComputeShader();
        }

        private void OnEditTick(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;

            //Vector3 Eye = new Vector3();
            //Eye.SetValue(0.0f, 1.5f, -15.0f);
            //Vector3 At = new Vector3();
            //At.SetValue(0.0f, 1.0f, 0.0f);
            //Vector3 Up = new Vector3();
            //Up.SetValue(0.0f, 1.0f, 0.0f);
            //vpc.Camera.LookAtLH(Eye, At, Up);

            if (true)
            {
                t += (float)EngineNS.MathHelper.V_PI * 0.0125f * EngineNS.CEngine.Instance.EngineElapseTime / 1000;

                if (vpc.Camera.SceneView != null && LineDrawer != null)
                {
                    //LineDrawer.UpdateGeomMesh(RHICtx, 0);
                }
            }

            if (DoCaptureMemory)
            {
                var memStat = EngineNS.Profiler.NativeMemory.CurrentProfiler.CaptureMemory();
                DoCaptureMemory = false;
                if (mPrevStat != null)
                {
                    var delta = EngineNS.Profiler.NativeMemory.CurrentProfiler.GetDelta(mPrevStat, memStat);
                    foreach (var i in delta)
                    {
                        System.Diagnostics.Debug.WriteLine($"{i.Key}: Size={i.Value.Size};Count={i.Value.Count}");
                    }
                }
                mPrevStat = memStat;

                System.Diagnostics.Debug.WriteLine($"Total Alloc Count = {EngineNS.Profiler.NativeMemory.CurrentProfiler.TotalCount - TotalAllocCount}");
                TotalAllocCount = EngineNS.Profiler.NativeMemory.CurrentProfiler.TotalCount;
            }
        }
        private void OnCommitRender(EditorCommon.ViewPort.ViewPortControl vpc)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            this.World.CheckVisible(RHICtx.ImmCommandList, vpc.Camera);
            this.World.Tick();
            vpc.RPolicy?.TickLogic(null, RHICtx);
        }

        public bool DoCaptureMemory = false;
        private Dictionary<string, EngineNS.Profiler.NativeProfiler.NMDesc> mPrevStat;
        private Int64 TotalAllocCount = 0;

        // List里的引用了Key
        Dictionary<RName, List<RName>> mWhoReferenceMeRNamesDic = new Dictionary<RName, List<RName>>();
        // Key引用了List
        Dictionary<RName, List<RName>> mMyReferenceRNameDic = new Dictionary<RName, List<RName>>();
        EngineNS.Thread.Async.TaskLoader.WaitContext mResourceRefInitWaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(mResourceRefInitWaitContext);
        }
        public async Task<bool> InitializeResourceReferenceDictionary()
        {
            var retValue = await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                // Game
                var root = EngineNS.CEngine.Instance.FileManager.ProjectContent;
                var resInfoFiles = EngineNS.CEngine.Instance.FileManager.GetFiles(root, "*" + EditorCommon.Program.ResourceInfoExt, System.IO.SearchOption.AllDirectories);
                var length = resInfoFiles.Count;
                int i = 0;
                foreach (var file in resInfoFiles)
                {
                    try
                    {
                        var tempFile = file.Replace("\\", "/");

                        var info = new EditorCommon.Resources.CommonResourceInfo();
                        if (info.Load(tempFile) == false)
                            continue;
                        var resInfo = EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfo(info.ResourceType);
                        if (resInfo == null)
                            continue;
                        resInfo.Load(tempFile);
                        mMyReferenceRNameDic[resInfo.ResourceName] = new List<RName>(resInfo.ReferenceRNameList);
                        foreach (var res in resInfo.ReferenceRNameList)
                        {
                            List<RName> refFromList;
                            if (!mWhoReferenceMeRNamesDic.TryGetValue(res, out refFromList))
                            {
                                refFromList = new List<RName>(20);
                                mWhoReferenceMeRNamesDic[res] = refFromList;
                            }
                            if (!refFromList.Contains(resInfo.ResourceName))
                                refFromList.Add(resInfo.ResourceName);
                        }
                    }
                    catch (System.Exception e)
                    {
                        EngineNS.Profiler.Log.WriteException(e, "初始化引用关系表");
                    }
                    i++;
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditorSlow);
            if(retValue)
            {
                EngineNS.Thread.Async.TaskLoader.Release(ref mResourceRefInitWaitContext, this);
                return true;
            }
            EngineNS.Thread.Async.TaskLoader.Release(ref mResourceRefInitWaitContext, this);
            return false;
        }

        public async Task RefreshResourceInfoReferenceDictionary(EditorCommon.Resources.ResourceInfo resInfo)
        {
            await AwaitLoad();

            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                List<RName> oldRefs = null;
                mMyReferenceRNameDic.TryGetValue(resInfo.ResourceName, out oldRefs);
                mMyReferenceRNameDic[resInfo.ResourceName] = new List<RName>(resInfo.ReferenceRNameList);
                foreach(var res in resInfo.ReferenceRNameList)
                {
                    if (res == null)
                        continue;
                    if (oldRefs != null && oldRefs.Contains(res))
                        oldRefs.Remove(res);
                    List<RName> refFromList;
                    if(!mWhoReferenceMeRNamesDic.TryGetValue(res, out refFromList))
                    {
                        refFromList = new List<RName>(20);
                        mWhoReferenceMeRNamesDic[res] = refFromList;
                    }
                    if (!refFromList.Contains(resInfo.ResourceName))
                        refFromList.Add(resInfo.ResourceName);
                }

                if(oldRefs != null)
                {
                    foreach (var oldRef in oldRefs)
                    {
                        List<RName> refFromList;
                        if (mWhoReferenceMeRNamesDic.TryGetValue(oldRef, out refFromList))
                        {
                            refFromList.Remove(resInfo.ResourceName);
                        }
                    }
                }

                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditorSlow);
        }
        public async Task RemoveResourceInfo(EditorCommon.Resources.ResourceInfo resInfo)
        {
            await AwaitLoad();

            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                List<RName> oldRefs = null;
                mMyReferenceRNameDic.TryGetValue(resInfo.ResourceName, out oldRefs);
                mMyReferenceRNameDic.Remove(resInfo.ResourceName);
                foreach(var refRes in resInfo.ReferenceRNameList)
                {
                    List<RName> refFromList;
                    if(mWhoReferenceMeRNamesDic.TryGetValue(refRes, out refFromList))
                    {
                        refFromList.Remove(resInfo.ResourceName);
                    }
                }

                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditorSlow);
        }

        public async Task<List<RName>> GetMyReference(RName rName)
        {
            await AwaitLoad();

            List<RName> retVal;
            if (mMyReferenceRNameDic.TryGetValue(rName, out retVal))
                return retVal;
            return new List<RName>();
        }
        public override async Task<List<RName>> GetWhoReferenceMe(RName rName)
        {
            await AwaitLoad();

            List<RName> retVal;
            if (mWhoReferenceMeRNamesDic.TryGetValue(rName, out retVal))
                return retVal;
            return new List<RName>();
        }


        #region Test
        public EngineNS.GamePlay.Actor.GActor TheActor;
        public EngineNS.GamePlay.Actor.GActor TheBox;
        public EngineNS.GamePlay.Actor.GActor TheSky;
        public EngineNS.GamePlay.Actor.GActor ActorTest0;
        public EngineNS.GamePlay.Actor.GActor ActorSphere0;
        EngineNS.Bricks.Animation.SkeletonControl.CGfxLookAt lookAt;

        #region TestGfxMesh
        Vector3[] vertices_pos = new Vector3[]
        {
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f),

            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),

            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),

            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, 1.0f)
        };
        Vector2[] vertices_uv = new Vector2[]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),

            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 1.0f),
        };
        UInt16[] indices = new UInt16[]
        {
            3,1,0,
            2,1,3,

            6,4,5,
            7,4,6,

            11,9,8,
            10,9,11,

            14,12,13,
            15,12,14,

            19,17,16,
            18,17,19,

            22,20,21,
            23,20,22
        };

        void CreateSkeletonAsset(EngineNS.Graphics.Mesh.CGfxMesh mesh)
        {

        }
        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, RName meshName, Vector3 loc, Vector3 scale)
        {
            var actor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(meshName);
            actor.Placement.Location = loc;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.GetScene(scene).AddActor(actor);
            return actor;
        }

        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, RName meshName, Vector3 loc, Quaternion rotation, Vector3 scale)
        {
            var actor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(meshName);
            actor.Placement.Location = loc;
            actor.Placement.Rotation = rotation;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.GetScene(scene).AddActor(actor);
            return actor;
        }

     
        private void CreateLookAt(EngineNS.GamePlay.Actor.GActor actor,string modifyBone,string targetBone , Vector3 loolAtAxis)
        {
            //var meshComp = actor.GetComponent<EngineNS.GamePlay.Component.GMeshComponent>();
            //var animationCom = actor.GetComponent<EngineNS.GamePlay.Component.GMacrossAnimationComponent>();
            //var skinModifier = meshComp.SceneMesh.MdfQueue.FindModifier<EngineNS.Graphics.Mesh.CGfxSkinModifier>();
            //var FinalPose = skinModifier.AnimationPose;
            //lookAt = new EngineNS.Bricks.Animation.SkeletonControl.CGfxLookAt();
            //lookAt.ModifyBoneName = modifyBone;
            //if(targetBone!= null)
            // lookAt.TargetBoneName = targetBone;
            //lookAt.LookAtAxis = Vector3.UnitX;
            //lookAt.Pose = FinalPose;
            //lookAt.Alpha = 1;
            //animationCom.skeletonControls.Add(lookAt);
        }


        private async System.Threading.Tasks.Task<EngineNS.GamePlay.Actor.GActor> CreateMeshActorAsync(EngineNS.GamePlay.GWorld world, RName scene, List<RName> meshesName, Vector3 loc, Vector3 scale)
        {
            var actor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(meshesName);
            actor.Placement.Location = loc;
            actor.Placement.Scale = scale;
            this.World.AddActor(actor);
            this.World.GetScene(scene).AddActor(actor);
            return actor;
        }

        Vector3 targetp = new Vector3(100.5f, 200, 200.4f);
        //void VisitSkeleton(EngineNS.Bricks.Animation.Skeleton.CGfxBoneTable tab, EngineNS.Bricks.Animation.Skeleton.CGfxBone bone)
        //{
        //    System.Diagnostics.Debug.WriteLine($"{bone.BoneDesc.Name} -> {bone.BoneDesc.Parent}");
        //    for (UInt32 i = 0; i < bone.ChildNumber; i++)
        //    {
        //        var index = bone.GetChild(i);
        //        var cbone = tab.GetBone(index);
        //        VisitSkeleton(tab, cbone);
        //    }
        //}
        #endregion

        #endregion
    }
}
