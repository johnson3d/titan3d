using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    [Bricks.CodeBuilder.ContextMenu("CpuCulling", "Culling\\CpuCulling", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtCpuCullingNode : TtRenderGraphNode
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
        public async override System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            CullCameral = policy.DefaultCamera;

            mPolicy = policy;
        }
        TtRenderPolicy mPolicy;
        [Rtti.Meta]
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
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtCpuCullingNode), nameof(TickLogic));
                return mScopeTick;
            }
        } 
        public override unsafe void TickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
        {
            //if (GetInput(0).FindInLinker() == null)
            //{

            //}
            if (FrozenCullCameral != null)
            {
                mVisParameter.CullCamera = FrozenCullCameral;
            }
            else
            {
                mVisParameter.CullCamera = CullCameral;
            }
            using (new Profiler.TimeScopeHelper(ScopeTick))
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
