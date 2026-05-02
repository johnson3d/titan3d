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
        }
        public override void TickSync(TtRenderPolicy policy)
        {
            mVisParameter.Reset();
            base.TickSync(policy);
        }
    }
}
