using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;

namespace EngineNS.GamePlay
{
    public class GGameInstanceDesc
    {
        protected static GGameInstanceDesc mInstance;
        public static GGameInstanceDesc Instance
        {
            get
            {
                return mInstance;
            }
        }
        public GGameInstanceDesc()
        {
            if (mInstance != null)
                mInstance = this;
        }
        public RName SceneName
        {
            get;
            set;
        }

        //[ResourcePublishAttribute(enResourceType.Material)]
        [Editor.Editor_PackData()]
        public RName DefaultMaterialName
        {
            get;
            set;
        } = RName.GetRName("Material/defaultmaterial.material");
        [Editor.Editor_PackData()]
        public RName DefaultMaterialInstanceName
        {
            get;
            set;
        } = RName.GetRName("Material/defaultmaterial.instmtl");
        [Editor.Editor_PackData()]
        public RName DefaultShadingEnvShaderName
        {
            get;
            set;
        } = RName.GetRName("Shaders/Dummy.shadingenv");
        [Editor.Editor_PackData()]
        public RName DefaultMapName
        {
            get;
            set;
        } = RName.GetRName("Map/test.map");
        [Editor.Editor_PackData()]
        public RName DefaultTextureName
        {
            get;
            set;
        } = RName.GetRName("Texture/uvchecker.txpic");

        [Editor.Editor_PackData()]
        public RName Face3Txpic
        {
            get;
            set;
        } = RName.GetRName("Texture/face3.txpic");
        [Editor.Editor_PackData()]
        public RName BoziInstmtl
        {
            get;
            set;
        } = RName.GetRName("Material/bozi.instmtl");
        [Editor.Editor_PackData()]
        public RName TypereDirection
        {
            get;
            set;
        } = RName.GetRName("typeredirection.xml");
        [Editor.Editor_PackData()]
        public RName CEngineDesc
        {
            get;
            set;
        } = RName.GetRName("cenginedesc.cfg");
        [Editor.Editor_PackData()]
        public RName RpcMapping
        {
            get;
            set;
        } = RName.GetRName("rpcmapping.xml");
        [Editor.Editor_PackData()]
        public RName Perfview
        {
            get;
            set;
        } = RName.GetRName("GameTable/perfview.cfg");
        [Editor.Editor_PackData()]
        public RName DefaultEnvMap
        {
            get;
            set;
        } = RName.GetRName("Texture/envmap0.txpic");
        [Editor.Editor_PackData()]
        public RName DefaultEyeEnvMap
        {
            get;
            set;
        } = RName.GetRName("Texture/eyeenvmap0.txpic");

        public EngineNS.Input.InputConfiguration InputConfiguration
        {
            get;
            set;
        }
    }

    public class Statistic
    {
        public Statistic(Profiler.PerfViewer viewer)
        {
            PViewer = viewer;
        }
        float mFPS;
        long mFPSStartTime = Support.Time.GetTickCount();
        int mFPSFrameCount = 0;
        public float FPS
        {
            get { return mFPS; }
        }
        public virtual void TickLogic()
        {

        }
        public virtual void TickRender()
        {

        }
        public virtual void TickSync()
        {
            mFPSFrameCount++;
            if (mFPSFrameCount >= 10)
            {
                var t = Support.Time.GetTickCount();
                mFPS = (float)mFPSFrameCount * 1000.0f / (float)(t - mFPSStartTime);
                mFPSFrameCount = 0;
                mFPSStartTime = t;
            }
        }
        public Profiler.PerfViewer PViewer
        {
            get;
            set;
        }
    }


    public interface IModuleInstance
    {
        GGameInstanceDesc Desc
        {
            get;
        }
        Statistic Stat
        {
            get;
        }
        Graphics.CGfxRenderPolicy RenderPolicy
        {
            get;
        }
        GWorld World
        {
            get;
        }
        void OnPause();
        void OnResume(IntPtr window);
        void MouseDown(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        void MouseMove(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        void MouseUp(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        void MouseWheel(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        void MouseEnter(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        void MouseLeave(object host, object sender, Input.Device.Mouse.MouseInputEventArgs e);
        Graphics.CGfxCamera GetMainViewCamera();
    }

    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable)]
    public partial class GGameInstance : IModuleInstance, ITickInfo
    {
        public static int InstanceNumber = 0;
        public GGameInstance()
        {
            InstanceNumber++;
        }
        ~GGameInstance()
        {
            InstanceNumber--;
            Cleanup();
        }
        public virtual void Cleanup()
        {
            if (GameCamera != null)
            {
                GameCamera.Cleanup();
                GameCamera = null;
            }
            if (RenderPolicy != null)
            {
                RenderPolicy.Cleanup();
                RenderPolicy = null;
            }
        }
        protected GGameInstanceDesc mDesc;
        public GGameInstanceDesc Desc
        {
            get { return mDesc; }
        }
        public Statistic Stat
        {
            get;
            set;
        }
        protected GWorld mWorld;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GWorld World
        {
            get { return mWorld; }
        }
        Graphics.CGfxCamera mGameCamera;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Graphics.CGfxCamera GameCamera
        {
            get { return mGameCamera; }
            set
            {
                var saved = mGameCamera;
                mGameCamera = value;
                if(RenderPolicy!=null)
                    RenderPolicy.Camera = value;
                if(saved != null && mGameCamera!=null)
                {
                    mGameCamera.PerspectiveFovLH(mGameCamera.FoV, saved.CameraWidth, saved.CameraHeight, mGameCamera.ZNear, mGameCamera.ZFar);
                    mGameCamera.SetSceneView(CEngine.Instance.RenderContext, saved.SceneView);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetCameraByActor(GActor actor)
        {
            if (actor == null)
                return;
            var cameraComp = actor.GetComponentRecursion<GamePlay.Camera.CameraComponent>();
            if(cameraComp!=null)
            {
                GameCamera = cameraComp.Camera;
                return;
            }
            var camera = actor.GetComponentRecursion<GamePlay.Camera.GCameraComponent>();
            if (camera != null)
            {
                GameCamera = camera.Camera.Camera;
                return;
            }
        }
        public virtual Graphics.CGfxCamera GetMainViewCamera()
        {
            return GameCamera;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool PhysicsDebug
        {
            get;
            set;
        } = false;
        #region Macross
        public static async System.Threading.Tasks.Task<SceneGraph.GSceneGraph> LoadScene(CRenderContext rc, GWorld world, RName name)
        {
            var xnd = await IO.XndHolder.LoadXND(name.Address + "/scene.map");
            if (xnd == null)
            {
                var st = await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(world, typeof(GamePlay.SceneGraph.GSceneGraph), new SceneGraph.GSceneGraphDesc());
                return st;
            }
            else
            {
                var type = Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type == null)
                    return null;
                var st = await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(world, type, null);
                st.Name = name.Name;
                if (false == await st.LoadXnd(rc, xnd.Node, name))
                    return null;

                if (st != null)
                {
                    world.AddScene(name, st);
                    world.SetDefaultScene(st.SceneId);
                }

                if (st.McSceneGetter != null && st.McSceneGetter.Get(false) != null)
                {
                    await st.McSceneGetter.Get(false).OnSceneLoaded(st);
                    st.McSceneGetter.Get(false).OnRegisterInput();
                }
                return st;
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual async System.Threading.Tasks.Task<SceneGraph.GSceneGraph> LoadScene(CRenderContext rc,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Scene)]
            RName name, bool clearWorld, bool useEditorScene = false)
        {
            if (rc == null)
                rc = CEngine.Instance.RenderContext;

#if PWindow
            if (useEditorScene && CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
            {
                // 使用临时文件
                name = EngineNS.RName.GetRName("tempscene", RName.enRNameType.Editor);
            }
#endif

            if (clearWorld)
                mWorld.Cleanup();

            var st = await LoadScene(rc, mWorld, name);

            if (mWorld.DefaultScene!=null && mWorld.DefaultScene.SunActor != null)
            {
                var sunComp = mWorld.DefaultScene?.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                if (sunComp != null)
                {
                    sunComp.View = this.RenderPolicy.BaseSceneView;
                }
            }

            return st;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool AddActorBySceneName(GamePlay.Actor.GActor actor,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Scene)]
            RName sceneName=null)
        {
            var scene = mWorld.DefaultScene;
            if (sceneName != null && sceneName.Name != "")
            {
                scene = mWorld.GetScene(sceneName);
            }

            if (scene == null)
                return false;
            scene.AddActor(actor);
            mWorld.AddActor(actor);
            return true;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.GamePlay.Actor.GActor ControlActor
        {
            get;
            protected set;
        }

       
     
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public async System.Threading.Tasks.Task<GamePlay.Actor.GActor> CreateTitanActor(
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Mesh)]
            RName mesh,
            Vector3 lookOffset, float radius, float height,
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.PhyMaterial)]
            RName phyMtl)
        {
            var titanActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(mesh);
            titanActor.SpecialName = "TitanActor";
            this.World.AddActor(titanActor);
            this.World.DefaultScene.AddActor(titanActor);

            var tpcComp = new EngineNS.GamePlay.Camera.ThirdPersonCameraComponent();
            titanActor.AddComponent(tpcComp);
            tpcComp.SetCamera(this.GameCamera, 4.0f, lookOffset);
            var physxCtr = new EngineNS.Bricks.PhysicsCore.GPhyControllerComponent();
            var phyCtrInit = new EngineNS.Bricks.PhysicsCore.GPhyControllerComponent.GPhyControllerComponentInitializer();
            phyCtrInit.IsBox = false;
            phyCtrInit.CapsuleRadius = radius;
            phyCtrInit.CapsuleHeight = height;
            phyCtrInit.MaterialName = phyMtl;
            await physxCtr.SetInitializer(CEngine.Instance.RenderContext, titanActor, titanActor, phyCtrInit);
            titanActor.AddComponent(physxCtr);

            ControlActor = titanActor;
            return titanActor;
        }
        #endregion

        public enum enGameState
        {
            initializing,
            Initialized,
            Stopping,
            Stopped,
        }
        //public delegate
        public enGameState GameState = enGameState.Stopped;
        public bool EnableTick
        {
            get;
            set;
        } = true;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public UInt32 WindowWidth
        {
            get;
            protected set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public UInt32 WindowHeight
        {
            get;
            protected set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public IntPtr WindowHandle
        {
            get;
            protected set;
        }
        public virtual async System.Threading.Tasks.Task<bool> InitGame(IntPtr WinHandle, UInt32 width, UInt32 height, EngineNS.GamePlay.GGameInstanceDesc desc, EngineNS.Graphics.CGfxCamera camera)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            if (camera == null)
            {
                camera = new EngineNS.Graphics.CGfxCamera();
                camera.Init(rc, false);
            }

            CEngine.Instance.TickManager.AddTickInfo(this);

            await BuildAutoMembers();

            WindowHandle = WinHandle;
            WindowWidth = width;
            WindowHeight = height;
            mDesc = desc;
            mWorld = new GWorld();
            GameCamera = camera;

            if (false == mWorld.Init())
                return false;

            var scene = await LoadScene(rc, desc.SceneName, true);
            if (scene == null)
                return false;

            mWorld.AddScene(desc.SceneName, scene);
            mWorld.SetDefaultScene(scene.SceneId);

            foreach (var i in AutoMembers)
            {
                await i.Key.OnGameInit(i.Value, width, height);
            }

            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 1.0f, -3.0f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 1.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            camera.LookAtLH(Eye, At, Up);
            camera.PerspectiveFovLH(camera.mDefaultFoV, (float)width, (float)height, 0.1f, 1000.0f);

            var mRP_GameMobile = new EngineNS.Graphics.RenderPolicy.CGfxRP_GameMobile();
            await mRP_GameMobile.Init(rc, width, height, camera, WinHandle);
            this.SetGameRenderPolicy(mRP_GameMobile);

            return true;
        }
        public virtual async System.Threading.Tasks.Task OnGameInited()
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(GGameInstance), nameof(TickLogic));
        public Profiler.TimeScope GetLogicTimeScope()
        {
            return ScopeTickLogic;
        }
        public virtual void BeforeFrame()
        {
            GameCamera?.BeforeFrame();
        }
        private System.Action RenderAction = null;
        public virtual void TickLogic()
        {
            Stat?.TickLogic();

            if (this.GameState != enGameState.Initialized)
                return;

            if(RenderAction==null)
            {
                RenderAction = () =>
                {
                    OnGameCommitRender(CEngine.Instance.RenderContext.ImmCommandList);
                };
            }
            CEngine.Instance.ThreadRender.PostRenderAction(RenderAction);

            OnGameTick();

            //OnGameCommitRender();
            CEngine.Instance.ThreadRender.WaitRender();

            if (this.GameState == enGameState.Initialized)
            {
                var mcGame = CEngine.Instance.McGame.Get();
                if (mcGame != null)
                    mcGame.OnGameTick(this);
            }

            //ScopeTickAutoMember.Begin();
            foreach (var i in AutoMembers)
            {
                i.Key.Tick(i.Value);
            }
            //ScopeTickAutoMember.End();
        }
        protected virtual void OnGameTick()
        {
            this.World.Tick();
        }
        protected virtual void OnGameCommitRender(CCommandList cmd)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            if (World == null || RenderPolicy == null)
                return;
            this.World.CheckVisible(cmd, GameCamera);

            RenderPolicy.TickLogic(GameCamera.SceneView, RHICtx);
        }
        public virtual void TickRender()
        {
            Stat?.TickRender();

            if (GameState != enGameState.Initialized)
                return;

            RenderPolicy?.TickRender(RenderPolicy.SwapChain);
        }
        public virtual void TickSync()
        {
            Stat?.TickSync();

            if (GameState != enGameState.Initialized)
                return;

            RenderPolicy?.TickSync();
            GameCamera?.SwapBuffer();
        }
        public virtual void FinalGame()
        {
            if (mWorld != null)
            {
                mWorld.Cleanup();
                mWorld = null;
            }
            foreach (var i in AutoMembers)
            {
                i.Key.Cleanup(i.Value);
            }
        }
        #region Interface
        public virtual void OnPause()
        {
            if (GameState != enGameState.Initialized)
                return;

            RenderPolicy.SwapChain?.OnLost();
        }
        public virtual void OnResume(IntPtr window)
        {
            if (GameState != enGameState.Initialized)
                return;

            this.WindowHandle = window;
            if (RenderPolicy == null)
                return;

            if (RenderPolicy.SwapChain != null)
            {
                CSwapChainDesc desc = new CSwapChainDesc();
                RenderPolicy.SwapChain.GetDesc(ref desc);
                desc.WindowHandle = window;
                
                RenderPolicy.SwapChain.OnRestore(ref desc);
            }
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

        partial void WindowsSizeChanged_UIProcess(UInt32 width, UInt32 height);
        public virtual void OnWindowsSizeChanged(UInt32 width, UInt32 height)
        {
            var RHICtx = EngineNS.CEngine.Instance.RenderContext;
            WindowWidth = width;
            WindowHeight = height;
            WindowsSizeChanged_UIProcess(width, height);
            RenderPolicy?.OnResize(RHICtx, RenderPolicy.SwapChain, width, height);
        }
        public Graphics.CGfxRenderPolicy RenderPolicy
        {
            get;
            private set;
        }
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetGameRenderPolicy(Graphics.CGfxRenderPolicy rp)
        {
            RenderPolicy = rp;
            RenderPolicy.OnDrawUI -= OnDrawUI;
            RenderPolicy.OnDrawUI += OnDrawUI;
        }
        public bool NoPixelShader = false;
        public bool NoPixelWrite = false;
        public CGraphicsProfiler GetGraphicProfiler()
        {
            if (NoPixelShader == false && NoPixelWrite == false)
                return null;
            var profiler = new CGraphicsProfiler();
            profiler.Init(NoPixelShader, NoPixelWrite, CIPlatform.Instance.PlatformType);
            return profiler;
        }

        #region AutoMember
        protected Dictionary<CEngineAutoMemberProcessor, object> AutoMembers = new Dictionary<CEngineAutoMemberProcessor, object>();
        public async System.Threading.Tasks.Task BuildAutoMembers()
        {
            var props = this.GetType().GetProperties();
            foreach (var i in props)
            {
                var attrs = i.GetCustomAttributes(typeof(CEngineAutoMemberAttribute), true);
                if (attrs == null || attrs.Length == 0)
                    continue;
                var am = attrs[0] as CEngineAutoMemberAttribute;
                var processor = System.Activator.CreateInstance(am.Processor) as CEngineAutoMemberProcessor;
                if (processor == null)
                    continue;

                var obj = await processor.CreateObject();
                i.SetValue(this, obj);

                AutoMembers[processor] = obj;
            }
        }
        #endregion
    }

    [Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    [Editor.Editor_MacrossClassIconAttribute("icon/mcgame_64.txpic", RName.enRNameType.Editor)]
    public partial class McGameInstance
    {
        public static int InstanceNumber = 0;
        public McGameInstance()
        {
            InstanceNumber++;
        }
        ~McGameInstance()
        {
            InstanceNumber--;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual async System.Threading.Tasks.Task<bool> OnGameStart(GGameInstance game)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            return true;
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnGameTick(GGameInstance game)
        {

        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnGameStop(GGameInstance game)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        protected GamePlay.GGameInstance mGameInstance;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GamePlay.GGameInstance GameInstance
        {
            get { return mGameInstance; }
        }
        public Macross.MacrossGetter<GamePlay.McGameInstance> McGame
        {
            get;
            set;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public GamePlay.McGameInstance McGameInstance
        {
            get
            {
                return McGame?.Get();
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [return: Editor.Editor_TypeChangeWithParam(0)]
        public GamePlay.McGameInstance GetMcGame(
            [Editor.Editor_TypeFilterAttribute(typeof(GamePlay.McGameInstance))]
            Type type)
        {
            return McGameInstance;
        }
        private int TickableNum = 0;
        public async System.Threading.Tasks.Task<bool> StartGame(Type gameInstanceType, IntPtr WinHandle, UInt32 width, UInt32 height, EngineNS.GamePlay.GGameInstanceDesc desc, EngineNS.Graphics.CGfxCamera camera, RName startScript)
        {
#if PWindow
            //Bricks.RemoteServices.RPCExecManager.Insance.SaveCode();
#endif
            McGame = CEngine.Instance.MacrossDataManager.NewObjectGetter<GamePlay.McGameInstance>(startScript);

            TickableNum = CEngine.Instance.TickManager.GetTickableNum();
            if (mGameInstance != null && mGameInstance.GameState != GamePlay.GGameInstance.enGameState.Stopped)
                return false;

            var savedLogic = CEngine.Instance.ThreadLogic.LimitTime;
            var savedRender = CEngine.Instance.ThreadRHI.LimitTime;
            CEngine.Instance.ThreadLogic.LimitTime = long.MaxValue;
            CEngine.Instance.ThreadRHI.LimitTime = long.MaxValue;

            mGameInstance = System.Activator.CreateInstance(gameInstanceType) as GamePlay.GGameInstance;
            mGameInstance.GameState = GamePlay.GGameInstance.enGameState.initializing;
            try
            {
                if (false == await mGameInstance.InitGame(WinHandle, width, height, desc, camera))
                {
                    CEngine.Instance.ThreadLogic.LimitTime = savedLogic;
                    CEngine.Instance.ThreadRHI.LimitTime = savedRender;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            foreach(var i in AutoMembers)
            {
                await i.Key.OnGameStart(i.Value);
            }

            OnGameStarted();

            CEngine.Instance.ThreadLogic.LimitTime = savedLogic;
            CEngine.Instance.ThreadRHI.LimitTime = savedRender;

            var smp = Thread.ASyncSemaphore.CreateSemaphore(1);
            EngineNS.CEngine.Instance.EventPoster.RunOn(async () =>
            {
                var mcGame = McGame.Get();
                if (mcGame != null)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Macross", "Macross begin mcGame.OnGameStart");
                    try
                    {
                        await mcGame.OnGameStart(mGameInstance);
                    }
                    catch(Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Macross", "Macross end mcGame.OnGameStart");
                }

                smp.Release();
                return true;
            }, Thread.Async.EAsyncTarget.Logic);

            await smp.Await();
            mGameInstance.GameState = GamePlay.GGameInstance.enGameState.Initialized;
            await mGameInstance.OnGameInited();
            return true;
        }
        public async System.Threading.Tasks.Task StopGame()
        {
            McGame.Get()?.OnGameStop(mGameInstance);

            foreach (var i in AutoMembers)
            {
                i.Key.OnGameStop(i.Value);
            }

            var savedLogic = CEngine.Instance.ThreadLogic.LimitTime;
            var savedRender = CEngine.Instance.ThreadRHI.LimitTime;

            CEngine.Instance.ThreadLogic.LimitTime = long.MaxValue;
            CEngine.Instance.ThreadRHI.LimitTime = long.MaxValue;
            await EngineNS.CEngine.Instance.EventPoster.Post(() =>
            {
                while (mGameInstance != null && mGameInstance.GameState != GamePlay.GGameInstance.enGameState.Initialized)
                {
                    System.Threading.Thread.Sleep(50);
                }

                if (mGameInstance == null)
                    return false;
                mGameInstance.GameState = GamePlay.GGameInstance.enGameState.Stopping;
                mGameInstance.FinalGame();
                mGameInstance.GameState = GamePlay.GGameInstance.enGameState.Stopped;
                CEngine.Instance.TickManager.RemoveTickInfo(mGameInstance);
                mGameInstance = null;
                return true;
            }, Thread.Async.EAsyncTarget.Logic);
            //EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //{
            //    if (mGameInstance == null)
            //        return false;
            //    mGameInstance.GameState = GamePlay.GGameInstance.enGameState.Stopping;
            //    mGameInstance.FinalGame();
            //    mGameInstance.GameState = GamePlay.GGameInstance.enGameState.Stopped;
            //    CEngine.Instance.TickManager.RemoveTickInfo(mGameInstance);
            //    mGameInstance = null;
            //    return true;
            //}, Thread.Async.EAsyncTarget.Logic);

            OnGameStoped();

            McGame.Reset();
            CEngine.Instance.MacrossDataManager.CleanDebugContextGCRefrence();

            CEngine.Instance.ThreadLogic.LimitTime = savedLogic;
            CEngine.Instance.ThreadRHI.LimitTime = savedRender;
            //CEngine.Instance.MacrossDataManager.ClearDebugContext();
            GC.Collect();
            GC.WaitForFullGCComplete();
            GC.Collect();
            GC.WaitForFullGCComplete();

            if(TickableNum != CEngine.Instance.TickManager.GetTickableNum())
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
        public virtual void OnGameStarted()
        {

        }
        public virtual void OnGameStoped()
        {

        }
    }
}
