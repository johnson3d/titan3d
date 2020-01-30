using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.Profiler;

namespace EngineNS.GamePlay
{
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class GOffScreenViewer : ITickInfo
    {
        ~GOffScreenViewer()
        {
            FinalCleanup();
        }
        EngineNS.GamePlay.GWorld mWorld;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.GamePlay.GWorld World
        {
            get { return mWorld; }
        }
        EngineNS.Graphics.CGfxCamera mCamera;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Graphics.CGfxCamera Camera => mCamera;

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public UISystem.UIHost UIHost
        {
            get
            {
                return mRP_OffScreen.BaseSceneView.UIHost;
            }
        }

        public bool EnableTick
        {
            get;
            set;
        } = true;

        public UInt32 mWidth = 256;
        public UInt32 mHeight = 256;
        private EngineNS.Graphics.RenderPolicy.CGfxRP_OffScreen mRP_OffScreen;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.CShaderResourceView OffScreenTexture
        {
            get
            {
                return mRP_OffScreen.mCopyPostprocessPass.mScreenView.FrameBuffer.GetSRV_RenderTarget(0);
            }
        }
        public EngineNS.CShaderResourceView OffScreenBaseTexture
        {
            get
            {
                return mRP_OffScreen.BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void BindScreenTexture2Mesh(Graphics.Mesh.CGfxMesh mesh, UInt32 atom, string shaderVar)
        {
            if(atom>= mesh.MtlMeshArray.Length)
                return;
            CTextureBindInfo txInfo = new CTextureBindInfo();
            for (int i=0; i < mesh.MtlMeshArray[atom].PrebuildPassArray.Length; i++)
            {
                var pass = mesh.MtlMeshArray[atom].PrebuildPassArray[i];
                if (pass == null || pass.GpuProgram==null)
                    continue;
                var TexBinder = pass.ShaderResources;
                var SamplerBinder = pass.ShaderSamplerBinder;
                var Shader = pass.GpuProgram;

                if (Shader.FindTextureBindInfo(pass.MtlInst, shaderVar, ref txInfo))
                {
                    TexBinder.PSBindTexture(txInfo.PSBindPoint, OffScreenTexture);
                    TexBinder.SetUserControlTexture(txInfo.PSBindPoint, true);
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public virtual async System.Threading.Tasks.Task<bool> InitEnvWithScene(UInt32 w, UInt32 h, 
            [Editor.Editor_RNameType(Editor.Editor_RNameTypeAttribute.Scene)]
            RName map, bool startDraw = true)
        {
            if (w == 0)
                w = 128;
            if (h == 0)
                h = 128;
            if (map == null)
                map = RName.EmptyName;
            mWidth = w;
            mHeight = h;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            mCamera = new EngineNS.Graphics.CGfxCamera();
            mCamera.Init(rc, false);

            mCamera.PerspectiveFovLH(mCamera.mDefaultFoV, (float)mWidth, (float)mHeight, 0.1f, 1000.0f);
            mCamera.BeforeFrame();
            //mCamera.SwapBuffer(false);

            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 50.0f, -30.0f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 0.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            mCamera.LookAtLH(Eye, At, Up);

            mRP_OffScreen = new EngineNS.Graphics.RenderPolicy.CGfxRP_OffScreen();
            mRP_OffScreen.OnDrawUI += (CCommandList cmd, Graphics.View.CGfxScreenView view) =>
            {
                if (UIHost != null)
                    UIHost.Draw(CEngine.Instance.RenderContext, cmd, view);
            };
            await mRP_OffScreen.Init(rc, mWidth, mHeight, mCamera, IntPtr.Zero);
            UIHost.Initializer.Id = 1;
            UIHost.WindowSize = new SizeF(mWidth, mHeight);

            mWorld = new EngineNS.GamePlay.GWorld();

            if (false == mWorld.Init())
                return false;

            mWorld.CheckVisibleParam.UsePVS = false;
            SceneGraph.GSceneGraph scene = null;
            IO.XndHolder xnd = await IO.XndHolder.LoadXND(map.Address + "/scene.map");
            if (xnd == null)
            {
                scene = await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(this.World, typeof(GamePlay.SceneGraph.GSceneGraph), new SceneGraph.GSceneGraphDesc());
                mWorld.AddScene(map, scene);
            }
            else
            {
                var type = Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type == null)
                    return false;
                scene = await GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(this.World, type, null);
                if (false == await scene.LoadXnd(rc, xnd.Node, map))
                    return false;

                if (scene != null)
                {
                    World.AddScene(map, scene);
                    mWorld.SetDefaultScene(scene.SceneId);

                    if (mWorld.DefaultScene.SunActor != null)
                    {
                        var sunComp = mWorld.DefaultScene?.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                        if (sunComp != null)
                        {
                            sunComp.View = mRP_OffScreen.BaseSceneView;
                        }
                    }
                    if (scene.McSceneGetter != null && scene.McSceneGetter.Get(false) != null)
                    {
                        await scene.McSceneGetter.Get(false).OnSceneLoaded(scene);
                        scene.McSceneGetter.Get(false).OnRegisterInput();
                    }
                }
            }

            if (startDraw)
                this.Start();

            return true;
        }
        public virtual async System.Threading.Tasks.Task<bool> InitEnviroment(UInt32 w, UInt32 h, string name)
        {
            mWidth = w;
            mHeight = h;
            var rc = EngineNS.CEngine.Instance.RenderContext;

            mCamera = new EngineNS.Graphics.CGfxCamera();
            mCamera.Init(rc, false);

            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 0.0f, -3.0f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 0.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            mCamera.LookAtLH(Eye, At, Up);

            mCamera.PerspectiveFovLH(mCamera.mDefaultFoV, (float)mWidth, (float)mHeight, 0.1f, 1000.0f);
            mCamera.BeforeFrame();
            mCamera.SwapBuffer(false);

            mRP_OffScreen = new EngineNS.Graphics.RenderPolicy.CGfxRP_OffScreen();
            mRP_OffScreen.OnDrawUI += (CCommandList cmd, Graphics.View.CGfxScreenView view) =>
            {
                if (UIHost != null)
                    UIHost.Draw(CEngine.Instance.RenderContext, cmd, view);
            };
            await mRP_OffScreen.Init(rc, mWidth, mHeight, mCamera, IntPtr.Zero);

            return await InitWorld(name);
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void Start()
        {
            CEngine.Instance.TickManager.AddTickInfo(this);
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void Stop()
        {
            CEngine.Instance.TickManager.RemoveTickInfo(this);
        }
        protected virtual async System.Threading.Tasks.Task<bool> InitWorld(string name)
        {
            mWorld = new EngineNS.GamePlay.GWorld();

            if (false == mWorld.Init())
                return false;

            mWorld.CheckVisibleParam.UsePVS = false;
            var scene = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(mWorld, typeof(EngineNS.GamePlay.SceneGraph.GSceneGraph), null);

            mWorld.AddScene(RName.GetRName(name), scene);
            return true;
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void FinalCleanup()
        {
            CEngine.Instance.TickManager.RemoveTickInfo(this);
            if (mWorld != null)
            {
                mWorld.Cleanup();
                mWorld = null;
            }
            if (mRP_OffScreen != null)
            {
                mRP_OffScreen.Cleanup();
                mRP_OffScreen = null;
            }
            if (mCamera != null)
            {
                mCamera.Cleanup();
                mCamera = null;
            }
        }

        public virtual void BeforeFrame()
        {

        }

        public virtual void TickLogic()
        {
            var rc = CEngine.Instance.RenderContext;
            World?.Tick();
            World?.CheckVisible(rc.ImmCommandList, mCamera);
            UIHost?.Commit(rc.ImmCommandList);
            mRP_OffScreen.TickLogic(null, rc);
        }

        public virtual void TickRender()
        {
            mRP_OffScreen.TickRender(null);
        }
        public virtual void TickSync()
        {
            mCamera.SwapBuffer(false);
            mRP_OffScreen.TickSync();
        }

        public static Profiler.TimeScope ScopeTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(GOffScreenViewer), nameof(TickLogic));
        public Profiler.TimeScope GetLogicTimeScope()
        {
            return ScopeTickLogic;
        }
    }
}
