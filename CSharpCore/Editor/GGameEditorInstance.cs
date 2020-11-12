using System;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;

namespace EngineNS.Editor
{
    public class CEditorInstanceDesc : EngineNS.GamePlay.GGameInstanceDesc
    {
        public CEditorInstanceDesc()
        {

        }
    }

    public interface IEditorInstanceObject
    {
        void FinalCleanup();
    }

    public abstract class CEditorInstance : DynamicObject, EngineNS.GamePlay.IModuleInstance, ITickInfo
    {
        Dictionary<string, IEditorInstanceObject> mDictionary = new Dictionary<string, IEditorInstanceObject>();
        public GEditOperationProcessor EditOperator
        {
            get;
        } = new GEditOperationProcessor();

        CEditorInstanceDesc mDesc;
        public EngineNS.GamePlay.GGameInstanceDesc Desc
        {
            get => mDesc;
        }
        public EngineNS.GamePlay.Statistic Stat
        {
            get
            {
                return EngineNS.CEngine.Instance.Stat;
            }
        }
        protected GamePlay.GWorld mWorld;
        public GamePlay.GWorld World
        {
            get { return mWorld; }
        }
        public void CleanWorld()
        {
            mWorld?.Cleanup();
            EditOperator?.Cleanup();
            System.GC.Collect();
        }

        public event Action OnWorldLoaded;
        public void _OnWorldLoaded()
        {
            OnWorldLoaded?.Invoke();
        }

        public bool EnableTick
        {
            get;
            set;
        } = true;
        public EngineNS.Graphics.CGfxRenderPolicy RenderPolicy
        {
            get;
            set;
        }
        public IEditorInstanceObject this[string name]
        {
            get
            {
                lock(mDictionary)
                {
                    IEditorInstanceObject obj = null;
                    mDictionary.TryGetValue(name, out obj);
                    return obj;
                }
            }
            set
            {
                lock(mDictionary)
                    mDictionary[name] = value;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            lock(mDictionary)
            {
                var name = binder.Name;
                IEditorInstanceObject res;
                var retVal = mDictionary.TryGetValue(name, out res);
                result = res;
                return retVal;
            }
        }
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            lock(mDictionary)
            {
                mDictionary[binder.Name] = (IEditorInstanceObject)value;
                return true;
            }
        }

        public virtual bool InitWorld(EngineNS.GamePlay.GGameInstanceDesc desc)
        {
            mDesc = desc as CEditorInstanceDesc;
            mWorld = new GamePlay.GWorld();

            if (false == mWorld.Init())
                return false;

            return true;
        }
        public abstract void SelectWorldActors(List<EngineNS.GamePlay.Actor.GActor> actors);
        public abstract void SelectWorldActor(EngineNS.GamePlay.Actor.GActor actor);
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(CEditorInstance), nameof(TickLogic));
        public Profiler.TimeScope GetLogicTimeScope()
        {
            return ScopeTickLogic;
        }
        public virtual void BeforeFrame()
        {
            for (int i = 0; i < EditorTickables.Count; i++)
            {
                if (EditorTickables[i].EnableTick)
                {
                    EditorTickables[i].BeforeFrame();
                }
            }
        }
        public virtual void TickLogic()
        {
            for(int i = 0; i<EditorTickables.Count; i++)
            {
                if(EditorTickables[i].EnableTick)
                {
                    EditorTickables[i].TickLogic();
                }
            }
            //mWorld?.Tick();
            Stat?.TickLogic();
        }
        public virtual async System.Threading.Tasks.Task<GamePlay.SceneGraph.GSceneGraph> LoadScene(CRenderContext rc, RName name)
        {
            var xnd = await IO.XndHolder.LoadXND(name.Address);
            if (xnd == null)
            {
                return await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(this.World, typeof(GamePlay.SceneGraph.GSceneGraph), new GamePlay.SceneGraph.GSceneGraphDesc());
            }
            else
            {
                var type = Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type == null)
                    return null;
                var st = await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(this.World, type, null);
                st.Name = name.Name;
                if (false == await st.LoadXnd(rc, xnd.Node))
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "IO", $"Scene {name} Load faield");
                    System.GC.Collect();
                    return null;
                }
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "IO", $"Scene {name} Load successed");
                System.GC.Collect();
                return st;
            }
        }
        public virtual void TickRender()
        {
            for (int i = 0; i < EditorTickables.Count; i++)
            {
                if (EditorTickables[i].EnableTick)
                {
                    EditorTickables[i].TickRender();
                }
            }
            Stat?.TickRender();
        }
        public virtual void TickSync()
        {
            for (int i = 0; i < EditorTickables.Count; i++)
            {
                if (EditorTickables[i].EnableTick)
                {
                    EditorTickables[i].TickSync();
                }
            }
            Stat?.TickSync();
        }
        public virtual void FinalGame()
        {
            if (mWorld != null)
            {
                mWorld.Cleanup();
                mWorld = null;
            }

            lock(mDictionary)
            {
                foreach (var val in mDictionary.Values)
                {
                    val.FinalCleanup();
                }
                mDictionary.Clear();
            }
        }
        #region Interface
        public virtual void OnPause()
        {

        }
        public virtual void OnResume(IntPtr window)
        {

        }
        public virtual void MouseDown(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        public virtual void MouseMove(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        public virtual void MouseUp(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        public virtual void MouseWheel(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        public virtual void MouseEnter(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        public virtual void MouseLeave(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e)
        {

        }
        #endregion
        public List<ITickInfo> EditorTickables
        {
            get;
        } = new List<ITickInfo>();
        public void AddTickInfo(ITickInfo ticker)
        {
            foreach(var i in EditorTickables)
            {
                if (i == ticker)
                    return;
            }
            EditorTickables.Add(ticker);
        }
        public void RemoveTickInfo(ITickInfo ticker)
        {
            foreach (var i in EditorTickables)
            {
                if (i == ticker)
                {
                    EditorTickables.Remove(ticker);
                }
            }
        }
        public virtual Graphics.CGfxCamera GetMainViewCamera()
        {
            return null;
        }

        [Editor_MenuMethod("Debug", "Engine|Render", "Assist|ShowPhysics")]
        public static bool ShowPhysicsAssist
        {
            get
            {
                return CEngine.PhysicsDebug;
            }
            set
            {
                CEngine.PhysicsDebug = value;
                CEngine.Instance.RemoteServices.AllClientsShowPhysXDebugMesh(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Assist|ShowLights")]
        public static bool ShowLightsAssist
        {
            get
            {
                return CEngine.ShowLightAssist;
            }
            set
            {
                CEngine.ShowLightAssist = value;
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Assist|Show Navgation Collision")]
        public static bool ShowNavCollision
        {
            get
            {
                return CEngine.NavtionDebug;
            }
            set
            {
                CEngine.NavtionDebug = value;
                CEngine.Instance.RemoteServices.AllClientsShowPhysXDebugMesh(value);
            }
        }

        [Editor_MenuMethod("Debug", "Engine|Render", "Assist|Show Navgation Mesh")]
        public static bool RenderNavMesh
        {
            get
            {
                return CEngine.IsRenderNavMesh;
            }
            set
            {
                CEditorInstance editor = (CEditorInstance)CEngine.Instance.GameEditorInstance;
                EngineNS.GamePlay.Actor.GActor actor = editor.World.FindEditorActor( editor.World.NavMeshActorID);
                if (actor != null)
                {
                    actor.Visible = value;
                }
                CEngine.IsRenderNavMesh = value;
                CEngine.Instance.RemoteServices.AllClientsShowNavMesh(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Assist|Show Navgation BoundBox")]
        public static bool RenderNavBoundBox
        {
            get
            {
                return CEngine.IsRenderBoundBox;
            }
            set
            {
                CEngine.IsRenderBoundBox = value;
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Editor", "Image|Need Etc")]
        public static bool WriteEtc
        {
            get
            {
                return CEngine.IsWriteEtc;
            }
            set
            {
                CEngine.IsWriteEtc = value;
            }
        }
        [Editor.Editor_MenuMethod("Debug", "Engine|Logic", "UsePVS")]
        public static bool UsePVS
        {
            get { return CEngine.UsePVS; }
            set
            {
                CEngine.UsePVS = value;
                CEngine.Instance.RemoteServices.AllClientsPVS(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Camera", "FrozenCulling")]
        public static bool FrozenCulling
        {
            get
            {
                var editor = (CEditorInstance)CEngine.Instance.GameEditorInstance;
                if (editor == null)
                    return false;
                var camera = editor.GetMainViewCamera();
                if (camera != null)
                {
                    return camera.LockCulling;
                }
                return false;
            }
            set
            {
                var editor = (CEditorInstance)CEngine.Instance.GameEditorInstance;
                var camera = editor.GetMainViewCamera();
                if (camera != null)
                {
                    camera.LockCulling = value;
                    if (CEngine.Instance.GameInstance != null)
                        CEngine.Instance.GameInstance.GameCamera.LockCulling = camera.LockCulling;

                    var noUse = ShowFrustumViewActor(camera, value);
                }
                CEngine.Instance.RemoteServices.AllClientsFrozenCulling(value);
            }
        }
        static GamePlay.Actor.GActor mFrustumViewActor;
        static Bricks.GraphDrawer.McFrustumGen mFrustumViewGen;
        static Bricks.GraphDrawer.GraphLines mFrustumViewGraphLine;
        static async Task ShowFrustumViewActor(Graphics.CGfxCamera camera, bool show)
        {
            var ins = EngineNS.CEngine.Instance.GameEditorInstance as CEditorInstance;
            if (show)
            {
                var rc = CEngine.Instance.RenderContext;
                var vectors = camera.CullingFrustum.FrustumVectors;
                if (mFrustumViewActor == null)
                {
                    var mtlInst = await CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, CEngineDesc.CameraFrustumMtl);
                    mFrustumViewGen = new Bricks.GraphDrawer.McFrustumGen()
                    {
                        Interval = 0f,
                        Segement = 10f,
                    };
                    mFrustumViewGen.SetFrustum(vectors);
                    mFrustumViewGraphLine = new Bricks.GraphDrawer.GraphLines();
                    mFrustumViewGraphLine.LinesGen = mFrustumViewGen;
                    mFrustumViewGraphLine.UseGeometry = true;
                    await mFrustumViewGraphLine.Init(mtlInst, 0);
                    mFrustumViewActor = mFrustumViewGraphLine.GraphActor;
                    var comp = mFrustumViewGraphLine.GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
                    var mat = Matrix.Identity;
                    comp.OnUpdateDrawMatrix(ref mat);
                }

                mFrustumViewGen.SetFrustum(vectors);
                mFrustumViewGraphLine.UpdateGeomMesh(rc, 0);

                ins.World.AddEditorActor(mFrustumViewActor);
            }
            else
            {
                if(mFrustumViewActor != null)
                    ins.World.RemoveEditorActor(mFrustumViewActor.ActorId);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|CrossPlatformShader")]
        public static bool CrossPlatformShader
        {
            get
            {
                return CEngine.mGenerateShaderForMobilePlatform;
            }
            set
            {
                CEngine.mGenerateShaderForMobilePlatform = value;
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|Instancing")]
        public static bool InstancingRender
        {
            get
            {
                return CEngine.UseInstancing;
            }
            set
            {
                CEngine.UseInstancing = value;
                CEngine.Instance.RemoteServices.AllClientsInstancing(CEngine.UseInstancing);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|Shadow")]
        public static bool EnableShadowRender
        {
            get
            {
                return CEngine.EnableShadow;
            }
            set
            {
                CEngine.EnableShadow = value;
                CEngine.Instance.RemoteServices.AllClientsShadow(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|Open Shader Settings")]
        public static bool OpenShaderEnvSettings
        {
            get
            {
                return true;
            }
            set
            {
                CEngine.ShowPropertyGridInWindows(new Graphics.GfxEnvShaderCodeSettings(), null);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|Bloom")]
        public static bool EnablePostprocess_Bloom
        {
            get
            {
                return CEngine.EnableBloom;
            }
            set
            {
                CEngine.EnableBloom = value;
                CEngine.Instance.RemoteServices.AllClientsBloom(value);
            }
        }

        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|EnableMobileAo")]
        public static bool EnableMobileAo
        {
            get
            {
                return CEngine.EnableMobileAo;
            }
            set
            {
                CEngine.EnableMobileAo = value;
            }
        }
        
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|EnableSunShaft")]
        public static bool EnableSunShaft
        {
            get
            {
                return CEngine.EnableSunShaft;
            }
            set
            {
                CEngine.EnableSunShaft = value;
            }
        }


        [Editor_MenuMethod("Debug", "Engine|Logic", "MTForeach")]
        public static bool MTForeach
        {
            get
            {
                return CEngine.Instance.EventPoster.EnableMTForeach;
            }
            set
            {
                CEngine.Instance.EventPoster.EnableMTForeach = value;
                CEngine.Instance.RemoteServices.AllClientsMTForeach(value);
            }
        }
        private static bool mNoPixelShader;
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|NoPixelShader")]
        public static bool NoPixelShader
        {
            get
            {
                return mNoPixelShader;
            }
            set
            {
                mNoPixelShader = value;
                if (CEngine.Instance.GameInstance != null)
                {
                    CEngine.Instance.GameInstance.NoPixelShader = value;
                    var policy = CEngine.Instance.GameInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_GameMobile;
                    if (policy != null)
                    {
                        var profiler = CEngine.Instance.GameInstance.GetGraphicProfiler();
                        policy.SetGraphicsProfiler(profiler);
                    }
                }
                
                CEngine.Instance.RemoteServices.AllClientsNoPixelShader(value);
            }
        }

        private static bool mNoPixelWrite;
        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|NoPixelWrite")]
        public static bool NoPixelWrite
        {
            get
            {
                return mNoPixelWrite;
            }
            set
            {
                mNoPixelWrite = value;
                if (CEngine.Instance.GameInstance != null)
                {
                    CEngine.Instance.GameInstance.NoPixelWrite = value;
                    var policy = CEngine.Instance.GameInstance.RenderPolicy as Graphics.RenderPolicy.CGfxRP_GameMobile;
                    if (policy != null)
                    {
                        var profiler = CEngine.Instance.GameInstance.GetGraphicProfiler();
                        policy.SetGraphicsProfiler(profiler);
                    }
                }
                CEngine.Instance.RemoteServices.AllClientsNoPixelWrite(value);
            }
        }

        [Editor_MenuMethod("Debug", "Engine|Render", "Graphics|IgnoreGLCall")]
        public static bool IgnorGLCall
        {
            get
            {
                return CEngine.Instance.IgnoreGLCall;
            }
            set
            {
                CEngine.Instance.IgnoreGLCall = value;
                CEngine.Instance.RemoteServices.AllClientsIgnoreGLCall(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Logic", "FullGC")]
        public static bool FullGC
        {
            get
            {
                return true;
            }
            set
            {
                CEngine.Instance.RemoteServices.AllClientsFullGC(value);
            }
        }
        [Editor_MenuMethod("Debug", "Engine|Editor", "SetupEngine")]
        public static bool SetupEngine
        {
            get
            {
                return true;
            }
            set
            {
                Macross.MacrossMethodTable.Instance.RebuildTable();
                Macross.MacrossMethodTable.Instance.SaveManual(RName.GetRName("MacrossManual.xls").Address);
            }
        }

        [Editor_MenuMethod("Debug", "Engine|Editor", "JustTest")]
        public static bool JustTest
        {
            get
            {
                return true;
            }
            set
            {
                var rc = CEngine.Instance.RenderContext;
                if (false)
                {
                    var boxMesh = new EngineNS.Bricks.GpuDriven.Cluster.ClusteredMesh();
                    var sphereMesh = new EngineNS.Bricks.GpuDriven.Cluster.ClusteredMesh();

                    boxMesh.BuildClusterFromMeshSource(rc, RName.GetRName("editor/basemesh/box.vms"));
                    boxMesh.SaveClusteredMesh(RName.GetRName("samplers/mergeinstance/cluster/box.vms.cluster"));
                    EngineNS.IO.FileManager.TrySaveRInfo("MeshCluster", RName.GetRName("samplers/mergeinstance/cluster/box.vms.cluster"), boxMesh);
                    
                    sphereMesh.BuildClusterFromMeshSource(rc, RName.GetRName("editor/basemesh/sphere.vms"));
                    sphereMesh.SaveClusteredMesh(RName.GetRName("samplers/mergeinstance/cluster/sphere.vms.cluster"));
                    EngineNS.IO.FileManager.TrySaveRInfo("MeshCluster", RName.GetRName("samplers/mergeinstance/cluster/sphere.vms.cluster"), sphereMesh);
                }
                var clusterBox = CEngine.Instance.ClusteredMeshManager.GetResource(rc, RName.GetRName("samplers/mergeinstance/cluster/box.vms.cluster"), true);
                var clusterSphere = CEngine.Instance.ClusteredMeshManager.GetResource(rc, RName.GetRName("samplers/mergeinstance/cluster/sphere.vms.cluster"), true);

                var noused = DoJustTest();
            }
        }

        public virtual async Task<List<RName>> GetWhoReferenceMe(RName rName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        //public static Bricks.GpuDriven.GpuScene.SceneDataManager SceneData = new Bricks.GpuDriven.GpuScene.SceneDataManager();

        static int SphereNumber = 10;
        private static async System.Threading.Tasks.Task DoJustTest()
        {
            var rc = CEngine.Instance.RenderContext;

            var cmd = rc.ImmCommandList;

            var SceneData  = new Bricks.GpuDriven.GpuScene.SceneDataManager();
            await SceneData.InitPass(rc, null, true);

            if (SceneData != null)
            {
                //{
                //    var boxActor = await GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/box.gms"));
                //    boxActor.Placement.Location = new Vector3(-5, 5, 0);
                //    SceneData.AddMeshInstance(rc, boxActor.GetComponentMesh(), RName.GetRName("editor/basemesh/box.vms.cluster"), boxActor.Placement);
                //}

                //{
                //    var sphereActor = await GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/sphere.gms"));
                //    sphereActor.Placement.Location = new Vector3(5, 5, 0);
                //    SceneData.AddMeshInstance(rc, sphereActor.GetComponentMesh(), RName.GetRName("editor/basemesh/sphere.vms.cluster"), sphereActor.Placement);
                //}

                for (int i = 0; i < 10; i++)
                {
                    var tmpActor = await GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/box.gms"));
                    tmpActor.Placement.Location = new Vector3(0, (float)i * 1.5f, 0);
                    SceneData.AddMeshInstance(rc, tmpActor.GetComponentMesh(), RName.GetRName("samplers/mergeinstance/cluster/box.vms.cluster"), tmpActor.Placement);
                }

                for (int i = 0; i < SphereNumber; i++)
                {
                    var tmpActor = await GActor.NewMeshActorAsync(RName.GetRName("editor/basemesh/sphere.gms"));
                    tmpActor.Placement.Location = new Vector3((float)i * 1.5f, 2, 0);
                    SceneData.AddMeshInstance(rc, tmpActor.GetComponentMesh(), RName.GetRName("samplers/mergeinstance/cluster/sphere.vms.cluster"), tmpActor.Placement);
                }

                SceneData.UpdateGpuBuffer(rc, cmd, null);
            }

            var editor = CEngine.Instance.GetCurrentModule() as CEditorInstance;
            if (editor != null)
            {
                var editorRPolicy = editor.RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
                editorRPolicy.mForwardBasePass.BeforeBuildRenderPass = (InCamera, InView, InRc, InCmd, InDPLimitter, InGraphicsDebug) =>
                {
                    SceneData.DrawScene(InRc, InCmd, InCamera, InView);
                };

                var ccc = await editor.GetWhoReferenceMe(RName.GetRName("editor/basemesh/box.vms"));
                if (ccc != null)
                    return;
            }
        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        public dynamic GameEditorInstance
        {
            get;
            set;
        } = null;
        public GamePlay.IModuleInstance GetCurrentModule()
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                return GameEditorInstance;
            else
                return this.GameInstance;
        }
    }
}
