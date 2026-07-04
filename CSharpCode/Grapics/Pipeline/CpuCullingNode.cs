using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    [Bricks.CodeBuilder.ContextMenu("CpuCulling", "Culling\\CpuCulling", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtCpuCullingNode : TAuxRenderGraphNode<TtCpuCullingNode>
    {
        public TtRenderGraphPin VisiblesOut = TtRenderGraphPin.CreateOutput("Visibles", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);
        public TtCpuCullingNode()
        {
            Name = "CpuCulling";
        }
        ~TtCpuCullingNode()
        {
            mVisParameter.Reset();
        }

        public override void InitNodePins()
        {
            VisiblesOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(VisiblesOut);
        }
        TtCamera CullCameral = null;
        TtCamera FrozenCullCameral = null;
        [Category("Option")]
        public bool IsFrozenCullCameral
        {
            get
            {
                return FrozenCullCameral != null;
            }
            set
            {
                if (value)
                {
                    if (FrozenCullCameral == null)
                        FrozenCullCameral = new TtCamera();
                    if (CullCameral != null)
                        CullCameral.mCoreObject.CopyDataTo(FrozenCullCameral.mCoreObject);
                }
                else
                {
                    FrozenCullCameral = null;
                }
            }
        }
        public async override Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            CullCameral = policy.DefaultCamera;

            mPolicy = policy;
        }
        TtRenderPolicy mPolicy;
        [Rtti.Meta("")]
        public string CullCameraName
        {
            get
            {
                return mVisParameter.CullCamera?.Name;
            }
            set
            {
                var camera = mPolicy.FindCamera(value);
                if (camera == null)
                    return;
                mVisParameter.CullCamera = camera;
            }
        }
        GamePlay.TtWorld.TtVisParameter mVisParameter = new GamePlay.TtWorld.TtVisParameter();
        public GamePlay.TtWorld.TtVisParameter VisParameter
        {
            get => mVisParameter;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeGatherMesh;
        private static Profiler.TimeScope ScopeGatherMesh
        {
            get
            {
                if (mScopeGatherMesh == null)
                    mScopeGatherMesh = new Profiler.TimeScope(typeof(TtCpuCullingNode), "GatherMesh");
                return mScopeGatherMesh;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeUserTick;
        private static Profiler.TimeScope ScopeUserTick
        {
            get
            {
                if (mScopeUserTick == null)
                    mScopeUserTick = new Profiler.TimeScope(typeof(TtCpuCullingNode), "UserTick");
                return mScopeUserTick;
            }
        }
        public delegate void FTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear);
        public FTickLogic UserTickLogic = null;
        public override void Tick(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            //if (GetInput(0).FindInLinker() == null)
            //{

            //}
            if (UserTickLogic!=null)
            {
                using (new Profiler.TimeScopeHelper(ScopeGatherMesh))
                {
                    UserTickLogic(world, policy, frameCmdList, bClear);
                    return;
                }   
            }
            if (FrozenCullCameral != null)
            {
                mVisParameter.CullCamera = FrozenCullCameral;
            }
            else
            {
                mVisParameter.CullCamera = CullCameral;
            }
            using (new Profiler.TimeScopeHelper(ScopeGatherMesh))
            {
                mVisParameter.World = world;
                world.GatherVisibleMeshes(mVisParameter);
            }

            AccumulateStreamingDemands();
        }

        // 屏幕尺寸模型：裁剪完成后，遍历可见 mesh，把每张引用纹理的"屏幕 texel 需求"
        // 以 max 聚合推送给 TtTextureManager 的线程安全快照（供 RebalanceStreaming 反推 wanted mip）。
        // 复用 buffer 避免每帧 GC。
        private readonly List<NxRHI.TtSrViewAMeta> mDemandSrvScratch = new List<NxRHI.TtSrViewAMeta>();
        private ulong mLastDemandFrame = 0;
        private void AccumulateStreamingDemands()
        {
            var gfxConfig = TtEngine.Instance.GfxDevice.Config;
            if (gfxConfig.EnableTextureStreaming == false || gfxConfig.TextureStreamingScreenSpace == false)
                return;

            var camera = mVisParameter.CullCamera;
            // 正交相机无透视距离衰减，屏幕模型退化无意义，跳过。
            if (camera == null || camera.IsOrtho)
                return;

            var frame = TtEngine.Instance.CurrentTickFrame;
            int throttle = gfxConfig.TextureStreamingDemandThrottleFrames;
            if (throttle > 0 && mLastDemandFrame != 0 && frame - mLastDemandFrame < (ulong)throttle)
                return;
            mLastDemandFrame = frame;

            var texMgr = TtEngine.Instance.GfxDevice.TextureManager;
            if (texMgr == null)
                return;

            var camPos = camera.GetPosition();
            float halfHeight = camera.Height * 0.5f;
            float zNear = camera.ZNear;
            if (zNear <= 0.0f)
                zNear = 0.01f;

            var visibleMeshes = mVisParameter.VisibleMeshes;
            for (int m = 0; m < visibleMeshes.Count; m++)
            {
                var mesh = visibleMeshes[m].Mesh;
                if (mesh == null)
                    continue;

                var aabb = mesh.AABB;
                var center = aabb.GetCenter();
                float radius = (float)(aabb.GetMaxSide() * 0.5);

                double dx = center.X - camPos.X;
                double dy = center.Y - camPos.Y;
                double dz = center.Z - camPos.Z;
                float distance = (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz) - radius;
                if (distance < zNear)
                    distance = zNear;

                // UE 半屏约定：normalizedScreenSize = ViewportHeight*0.5 / distance。
                float normalizedScreenSize = halfHeight / distance;

                // mesh 级 texelFactor：取各 Atom 的 StreamingTexelFactor 最大值（缺省 1.0）。
                float texelFactor = 1.0f;
                bool hasFactor = false;
                var subMeshes = mesh.SubMeshes;
                for (int s = 0; s < subMeshes.Count; s++)
                {
                    var prim = mesh.GetMeshPrimitives(s);
                    if (prim == null)
                        continue;
                    if (prim.GetAMeta() is Graphics.Mesh.TtMeshPrimitivesAMeta pmeta)
                    {
                        if (hasFactor == false || pmeta.StreamingTexelFactor > texelFactor)
                        {
                            texelFactor = pmeta.StreamingTexelFactor;
                            hasFactor = true;
                        }
                    }
                }

                float wantedTexels = texelFactor * normalizedScreenSize;

                mDemandSrvScratch.Clear();
                mesh.GatherSrViews(mDemandSrvScratch);
                for (int i = 0; i < mDemandSrvScratch.Count; i++)
                {
                    var ameta = mDemandSrvScratch[i];
                    if (ameta == null)
                        continue;
                    texMgr.AccumulateScreenDemand(ameta.GetAssetName(), wantedTexels, frame);
                }
            }
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            mVisParameter.Reset();
            base.TickSync(policy);
        }
    }
}
