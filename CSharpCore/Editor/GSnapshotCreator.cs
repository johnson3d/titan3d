using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class GSnapshotCreator
    {
        bool m_bRenderingSnapshots = false;
        public bool RenderingSnapshots
        {
            get { return m_bRenderingSnapshots; }
        }

        EngineNS.GamePlay.GWorld mWorld;
        public EngineNS.GamePlay.GWorld World
        {
            get { return mWorld; }
        }
        public EngineNS.RName SkyName = null;
        public EngineNS.RName FloorName = null;
        EngineNS.GamePlay.Actor.GActor mSkyBoxActor { get; set; } = null;
        EngineNS.GamePlay.Actor.GActor mFloorActor { get; set; } = null;
        public EngineNS.GamePlay.Actor.GActor FocusActor { get; set; } = null;

        EngineNS.Graphics.CGfxCamera mCamera;
        public EngineNS.Graphics.CGfxCamera Camera => mCamera;
        public UInt32 mWidth = 256;
        public UInt32 mHeight = 256;

        public EngineNS.Graphics.RenderPolicy.CGfxRP_Snapshot mRP_Snapshot;
        public virtual async System.Threading.Tasks.Task<bool> InitEnviroment()
        {
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

            mRP_Snapshot = new EngineNS.Graphics.RenderPolicy.CGfxRP_Snapshot();
            await mRP_Snapshot.Init(rc, mWidth, mHeight, mCamera, IntPtr.Zero);

            return await InitWorld();
        }
        EngineNS.RName mSceneRName = EngineNS.RName.GetRName("editor/map/exhibition_hall_002.map", RName.enRNameType.Game);
        protected virtual async System.Threading.Tasks.Task<bool> InitWorld()
        {
            mWorld = new EngineNS.GamePlay.GWorld();

            if (false == mWorld.Init())
                return false;

            mWorld.CheckVisibleParam.UsePVS = false;

            EngineNS.GamePlay.SceneGraph.GSceneGraph scene = null;
            var xnd = await EngineNS.IO.XndHolder.LoadXND(mSceneRName.Address + "/scene.map");
            if (xnd != null)
            {
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(xnd.Node.GetName());
                if (type != null)
                {
                    scene = EngineNS.GamePlay.SceneGraph.GSceneGraph.NewSceneGraphWithoutInit(mWorld, type, new EngineNS.GamePlay.SceneGraph.GSceneGraphDesc());
                    scene.World = mWorld;
                    if (await scene.LoadXnd(EngineNS.CEngine.Instance.RenderContext, xnd.Node, mSceneRName) == false)
                        scene = null;
                    else
                    {
                        foreach (var actor in mWorld.Actors.Values)
                        {
                            actor.PreUse(true);
                        }
                    }
                }
            }
            if (scene == null)
                scene = await EngineNS.GamePlay.SceneGraph.GSceneGraph.CreateSceneGraph(mWorld, typeof(EngineNS.GamePlay.SceneGraph.GSceneGraph), null);

            mWorld.AddScene(RName.GetRName("SnapshorCreator"), scene);

            if (scene.SunActor != null)
            {
                var sunComp = scene.SunActor.GetComponent<EngineNS.GamePlay.Component.GDirLightComponent>();
                if (sunComp != null)
                {
                    sunComp.View = mRP_Snapshot.BaseSceneView;
                }
            }

            //if (SkyName != null && SkyName != RName.EmptyName)
            //{
            //    mSkyBoxActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(SkyName);
            //    mSkyBoxActor.PreUse(true);
            //    mSkyBoxActor.Placement.Scale = new EngineNS.Vector3(0.1F, 0.1F, 0.1F);
            //    mWorld.AddEditorActor(mSkyBoxActor);
            //}
            //if (FloorName != null && FloorName != RName.EmptyName)
            //{
            //    mFloorActor = await EngineNS.GamePlay.Actor.GActor.NewMeshActorAsync(FloorName);
            //    mFloorActor.PreUse(true);
            //    mFloorActor.Placement.Scale = new EngineNS.Vector3(10, 0.5f, 10);
            //    mFloorActor.Placement.Location = new Vector3(0, -0.251f, 0);
            //    mWorld.AddEditorActor(mFloorActor);
            //}
            return true;
        }
        public void FinalCleanup()
        {
            mRP_Snapshot.Cleanup();
            mRP_Snapshot = null;
            mCamera.Cleanup();
            mCamera = null;
        }

        public Int64 ElapsedTime
        {
            get { return 50; }
        }
        public static void FocusShow(float x, float y, float width, float height, Vector3 vMax, Vector3 vMin, EngineNS.Graphics.CGfxCamera Camera, EngineNS.GamePlay.GWorld world, double delta = 1.0)
        {
            List<Vector3> VecList = new List<Vector3>();

            if (vMax.X > 10000)
                vMax.X = 10000;
            if (vMax.Y > 10000)
                vMax.Y = 10000;
            if (vMax.Z > 10000)
                vMax.Z = 10000;
            if (vMin.X < -10000)
                vMin.X = -10000;
            if (vMin.Y < -10000)
                vMin.Y = -10000;
            if (vMin.Z < -10000)
                vMin.Z = -10000;

            VecList.Add(vMin);
            VecList.Add(new Vector3(vMax.X, vMin.Y, vMin.Z));
            VecList.Add(new Vector3(vMax.X, vMax.Y, vMin.Z));
            VecList.Add(new Vector3(vMin.X, vMax.Y, vMin.Z));
            VecList.Add(new Vector3(vMin.X, vMax.Y, vMax.Z));
            VecList.Add(new Vector3(vMin.X, vMin.Y, vMax.Z));
            VecList.Add(new Vector3(vMax.X, vMin.Y, vMax.Z));
            VecList.Add(vMax);

            var vObjCenter = (vMax - vMin) * 0.5f + vMin;

            float radius = Vector3.Distance(ref vMax, ref vMin) / 2;

            Vector3 eyePos = new Vector3();
            if (Camera.IsPerspective)
            {
                float fovAngle = 0;
                if (Camera.Aspect > 1)
                {
                    fovAngle = Camera.FoV * 0.5f;
                }
                else
                {
                    fovAngle = (float)(Math.Atan(Math.Tan(Camera.FoV * 0.5) * Camera.Aspect));
                }
                float h = radius / (float)Math.Sin(fovAngle);
                eyePos = vObjCenter - Camera.CameraData.Direction * h;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }

            var dir = Camera.CameraData.Direction;
            Camera.LookAtLH(eyePos, vObjCenter, Camera.CameraData.Up);
            //Camera.LookPosAndDir(eyePos, dir);
            Camera.BeforeFrame();
            Camera.SwapBuffer(true);
        }
        public void CalculateCamera(EngineNS.Graphics.CGfxCamera Camera, double delta)
        {
            if (FocusActor == null)
                return;
            Vector3 vMax = Vector3.UnitXYZ * float.MinValue;
            Vector3 vMin = Vector3.UnitXYZ * float.MaxValue;
            Vector3 tempMax = Vector3.Zero, tempMin = Vector3.Zero;
            BoundingBox aabb = new BoundingBox();
            FocusActor.GetAABB(ref aabb);
            tempMin = aabb.Minimum;
            tempMax = aabb.Maximum;
            vMax.X = System.Math.Max(vMax.X, tempMax.X);
            vMax.Y = System.Math.Max(vMax.Y, tempMax.Y);
            vMax.Z = System.Math.Max(vMax.Z, tempMax.Z);
            vMin.X = System.Math.Min(vMin.X, tempMin.X);
            vMin.Y = System.Math.Min(vMin.Y, tempMin.Y);
            vMin.Z = System.Math.Min(vMin.Z, tempMin.Z);

            FocusShow(0, 0, mWidth, mHeight, vMax, vMin, Camera, mWorld, delta);
        }
        EngineNS.Thread.Async.TaskLoader.WaitContext WaitContext = new EngineNS.Thread.Async.TaskLoader.WaitContext();
        public async System.Threading.Tasks.Task<EngineNS.Thread.Async.TaskLoader.WaitContext> AwaitLoad()
        {
            return await EngineNS.Thread.Async.TaskLoader.Awaitload(WaitContext);
        }

        public delegate void Delegate_LogicTick(GSnapshotCreator sc, object arg);
        public Delegate_LogicTick TickLogicEvent = Default_TickLogic;
        private static void Default_TickLogic(GSnapshotCreator sc, object arg)
        {
            var rc = CEngine.Instance.RenderContext;
            sc.World?.Tick();
            sc.World?.CheckVisible(rc.ImmCommandList, sc.mCamera);
            sc.mRP_Snapshot.OnAfterTickLogicArgument = arg;
            sc.mRP_Snapshot.TickLogic(null, rc);
        }
        int mFrameNum = 1;
        long mDuration = 100;
        //string mStrFileName;
        internal void RenderTick(CCommandList cmd)
        {
            foreach (var actor in mWorld.Actors.Values)
            {
                actor.PreUse(true);
            }

            CalculateCamera(mCamera, 1.0);

            EngineNS.CEngine.Instance.TextureManager.PauseKickResource = true;
            var saveTime = EngineNS.CEngine.Instance.EngineTime;
            var saveElapse = EngineNS.CEngine.Instance.EngineElapseTime;

            var savedCall = mRP_Snapshot.OnAfterTickLogic;
            for (int i = 0; i < mFrameNum; i++)
            {
                EngineNS.CEngine.Instance._UpdateEngineTime(saveTime + mDuration * i / mFrameNum);
                EngineNS.CEngine.Instance.SetPerFrameCBuffer();

                mRP_Snapshot.OnAfterTickLogic = null;
                TickLogicEvent?.Invoke(this, i);
                mRP_Snapshot.TickSync();
                mRP_Snapshot.TickRender(null);

                mRP_Snapshot.OnAfterTickLogic = savedCall;
                TickLogicEvent?.Invoke(this, i);
                mRP_Snapshot.TickSync();
                mRP_Snapshot.TickRender(null);
            }
            EngineNS.CEngine.Instance._ResetTime(saveTime, saveElapse);
            EngineNS.CEngine.Instance.SetPerFrameCBuffer();
            EngineNS.CEngine.Instance.TextureManager.PauseKickResource = false;
        }
        public async System.Threading.Tasks.Task SaveToFile(string strFileName, long duration, int frame = 4)
        {
            mDuration = duration;
            mFrameNum = 1;// frame;
            mRP_Snapshot.OnAfterTickLogic = (InView, InRc, InCmd, InArg) =>
            {
                CTexture2D ReadableTex = null;
                InCmd.CreateReadableTexture2D(ref ReadableTex, mRP_Snapshot.BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0), mRP_Snapshot.BaseSceneView.FrameBuffer);
                EngineNS.CEngine.Instance.GpuFetchManager.RegFetchTexture2D(ReadableTex, (InSrv) =>
                {
                    var blob = new EngineNS.Support.CBlobObject();
                    unsafe
                    {
                        void* pData;
                        uint rowPitch;
                        uint depthPitch;
                        if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
                        {
                            InSrv.BuildImageBlob(blob, pData, rowPitch);
                            InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
                        }
                    }
                    EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                    {
                        var blbArray = new EngineNS.Support.CBlobObject[] { blob };
                        CShaderResourceView.SaveSnap(strFileName, blbArray);
                        return true;
                    }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
                });
            };
            var rp = CEngine.Instance.GetCurrentModule().RenderPolicy as EngineNS.Graphics.RenderPolicy.CGfxRP_EditorMobile;
            if(rp!=null)
            {
                lock(rp.mSnapshots)
                {
                    rp.mSnapshots.Add(this);
                }
            }
            //frame = 1;
            //await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            //var rc = CEngine.Instance.RenderContext;

            //foreach (var actor in mWorld.Actors.Values)
            //{
            //    actor.PreUse(true);
            //}

            //CalculateCamera(mCamera, 1.0);

            //EngineNS.CEngine.Instance.TextureManager.PauseKickResource = true;
            //var saveTime = EngineNS.CEngine.Instance.EngineTime;
            //var saveElapse = EngineNS.CEngine.Instance.EngineElapseTime;
            //EngineNS.Support.CBlobObject[] data = new EngineNS.Support.CBlobObject[frame];
            //mRP_Snapshot.OnAfterTickLogic = (InView, InRc, InCmd, InArg) =>
            //{
            //    CTexture2D ReadableTex = null;
            //    InCmd.CreateReadableTexture2D(ref ReadableTex, mRP_Snapshot.BaseSceneView.FrameBuffer.GetSRV_RenderTarget(0), mRP_Snapshot.BaseSceneView.FrameBuffer);
            //    EngineNS.CEngine.Instance.GpuFetchManager.RegFetchTexture2D(ReadableTex, (InSrv) =>
            //    {
            //        //{
            //        //    var blob = new EngineNS.Support.CBlobObject();
            //        //    unsafe
            //        //    {
            //        //        void* pData;
            //        //        uint rowPitch;
            //        //        uint depthPitch;
            //        //        if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
            //        //        {
            //        //            InSrv.BuildImageBlob(blob, pData, rowPitch);
            //        //            InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
            //        //        }
            //        //    }
            //        //    bool bSave = true;
            //        //    if (bSave)
            //        //    {
            //        //        var blbArray = new EngineNS.Support.CBlobObject[] { blob };
            //        //        CShaderResourceView.SaveSnap(strFileName, blbArray);
            //        //    }
            //        //    return;
            //        //}
            //        if (InArg == null)
            //            InArg = (int)0;
            //        data[(int)InArg] = new EngineNS.Support.CBlobObject();

            //        var t1 = EngineNS.Support.Time.HighPrecision_GetTickCount();
            //        unsafe
            //        {
            //            void* pData;
            //            uint rowPitch;
            //            uint depthPitch;
            //            if (InSrv.Map(CEngine.Instance.RenderContext.ImmCommandList, 0, &pData, &rowPitch, &depthPitch))
            //            {
            //                InSrv.BuildImageBlob(data[(int)InArg], pData, rowPitch);
            //                InSrv.Unmap(CEngine.Instance.RenderContext.ImmCommandList, 0);
            //            }
            //        }
            //        var t2 = EngineNS.Support.Time.HighPrecision_GetTickCount();

            //        System.Diagnostics.Debug.WriteLine($"Fetch Snap time : {t2 - t1}");

            //        int finishCount = 0;
            //        foreach (var i in data)
            //        {
            //            if (i != null)
            //                finishCount++;
            //        }
            //        if (finishCount == data.Length)
            //        {
            //            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            //            {
            //                CShaderResourceView.SaveSnap(strFileName, data);
            //                //foreach (var i in data)
            //                //{
            //                //    i.Dispose();
            //                //}
            //                //data = null;
            //                //System.GC.Collect();
            //                return true;
            //            }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            //        }
            //        return;
            //    });
            //};
            //for (int i = 0; i < frame; i++)
            //{
            //    EngineNS.CEngine.Instance._UpdateEngineTime(saveTime + duration * i / frame);
            //    EngineNS.CEngine.Instance.SetPerFrameCBuffer();

            //    TickLogicEvent?.Invoke(this, i);
            //    mRP_Snapshot.TickSync();
            //    mRP_Snapshot.TickRender(null);

            //    TickLogicEvent?.Invoke(this, i);
            //    mRP_Snapshot.TickSync();
            //    mRP_Snapshot.TickRender(null);
            //}
            //EngineNS.CEngine.Instance._ResetTime(saveTime, saveElapse);
            //EngineNS.CEngine.Instance.SetPerFrameCBuffer();
            //EngineNS.CEngine.Instance.TextureManager.PauseKickResource = false;
            //mRP_Snapshot.OnAfterTickLogic = null;

            //FinalCleanup();
        }
    }
}
