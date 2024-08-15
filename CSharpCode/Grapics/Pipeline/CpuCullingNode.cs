using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline
{
    [Bricks.CodeBuilder.ContextMenu("CpuCulling", "Culling\\CpuCulling", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtCpuCullingNode : TtRenderGraphNode
    {
        public TtRenderGraphPin VisiblesOut = TtRenderGraphPin.CreateOutput("Visibles", false, EPixelFormat.PXF_UNKNOWN);
        public TtCpuCullingNode()
        {
            Name = "CpuCulling";
        }
        public override void InitNodePins()
        {
            VisiblesOut.LifeMode = TtAttachBuffer.ELifeMode.Imported;
            AddOutput(VisiblesOut, NxRHI.EBufferType.BFT_NONE);
        }
        public async override System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            mVisParameter.CullCamera = policy.DefaultCamera;

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
        GamePlay.UWorld.UVisParameter mVisParameter = new GamePlay.UWorld.UVisParameter();
        public GamePlay.UWorld.UVisParameter VisParameter
        {
            get => mVisParameter;
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
        {
            //if (GetInput(0).FindInLinker() == null)
            //{

            //}
            mVisParameter.World = world;
            world.GatherVisibleMeshes(mVisParameter);
        }
    }
}
