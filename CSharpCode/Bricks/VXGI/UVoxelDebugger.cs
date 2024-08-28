using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public class TtVoxelDebugModifier : Graphics.Pipeline.Shader.IMeshModifier
    {
        public void Dispose()
        {

        }
        public string ModifierNameVS { get => "DoVoxelDebugMeshVS"; }
        public string ModifierNamePS { get => "DoVoxelDebugMeshPS"; }
        public RName SourceName
        {
            get
            {
                return RName.GetRName("shaders/Bricks/VXGI/VxDebugModifier.cginc", RName.ERNameType.Engine);
            }
        }
        public unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, string includeOriName)
        {
            return (NxRHI.FShaderCode*)0;
        }
        public string GetUniqueText()
        {
            return "";
        }
        public NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, };
        }
        public Graphics.Pipeline.Shader.EPixelShaderInput[] GetPSNeedInputs()
        {
            return null;
        }
        public void Initialize(Graphics.Mesh.TtMaterialMesh materialMesh)
        {

        }
        public unsafe void OnDrawCall(Graphics.Pipeline.Shader.TtMdfQueueBase mdfQueue1, NxRHI.ICommandList cmd, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtMesh.TtAtom atom)
        {
            UMdfVoxelDebugMesh mdfQueue = mdfQueue1 as UMdfVoxelDebugMesh;
            var vxNode = mdfQueue.MdfDatas as UVoxelsNode;
            if (vxNode == null)
            {
                return;
            }
            var binder = drawcall.FindBinder("VxDebugInstanceSRV");
            drawcall.BindSRV(binder, vxNode.VoxelDebugger.Srv);
            drawcall.BindIndirectDrawArgsBuffer(vxNode.VxIndirectDebugDraws.GpuBuffer, 5 * 4);
        }
    }
    public class UMdfVoxelDebugMesh : Graphics.Pipeline.Shader.TtMdfQueue1<TtVoxelDebugModifier>
    {
    }

    [Bricks.CodeBuilder.ContextMenu("Voxelize", "GI\\Voxelize", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    partial class UVoxelsNode
    {
        #region VxDebugger
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVoxelDebugger")]
        public struct FVoxelDebugger
        {
            public Vector3 mPosition;
            public float mScale;

            public Vector3 mColor;
            public float mPad0;
        }
        public TtGpuBuffer<FVoxelDebugger> VoxelDebugger = new TtGpuBuffer<FVoxelDebugger>();
        public TtGpuBuffer<FVoxelDebugger> VoxelGroupDebugger = new TtGpuBuffer<FVoxelDebugger>();
        public TtGpuBuffer<uint> VxIndirectDebugDraws = new TtGpuBuffer<uint>();

        public class SetupVxDebuggerShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(2, 1, 1);
            }
            public SetupVxDebuggerShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine);
                MainName = "CS_SetupVxDebugger";
                
                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.AddDefine("VxSize", $"{VxSize}");
                defines.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
                defines.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
                defines.AddDefine("VxSceneX", $"{VxSceneX}");
                defines.AddDefine("VxSceneY", $"{VxSceneY}");
                defines.AddDefine("VxSceneZ", $"{VxSceneZ}");
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UVoxelsNode;

                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");///???
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, node.CBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxIndirectDebugDraws");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.VxIndirectDebugDraws.Uav);
                }
            }
        }

        public class CollectVxDebuggerShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(Dispatch_SetupDimArray1.X, 1, 1);
            }
            public CollectVxDebuggerShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine);
                MainName = "CS_CollectVxDebugger";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.AddDefine("VxSize", $"{VxSize}");
                defines.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
                defines.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
                defines.AddDefine("VxSceneX", $"{VxSceneX}");
                defines.AddDefine("VxSceneY", $"{VxSceneY}");
                defines.AddDefine("VxSceneZ", $"{VxSceneZ}");
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UVoxelsNode;
                // renwind test
                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.UavVoxelPool);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxIndirectDebugDraws");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.VxIndirectDebugDraws.Uav);
                }

                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, node.CBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VoxelGroupDebugger");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.VoxelGroupDebugger.Uav);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VoxelDebugger");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.VoxelDebugger.Uav);
                }
            }
        }

        private SetupVxDebuggerShading SetupVxDebugger;
        private NxRHI.UComputeDraw SetupVxDebuggerDrawcall;

        private CollectVxDebuggerShading CollectVxDebugger;
        private NxRHI.UComputeDraw CollectVxDebuggerDrawcall;

        public Graphics.Mesh.TtMesh VxDebugMesh;
        public GamePlay.Scene.TtMeshNode VxDebugMeshNode;

        private unsafe void ResetComputeDrawcall()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            CoreSDK.DisposeObject(ref SetupVxDebuggerDrawcall);
            SetupVxDebuggerDrawcall = rc.CreateComputeDraw();

            CoreSDK.DisposeObject(ref CollectVxDebuggerDrawcall);
            CollectVxDebuggerDrawcall = rc.CreateComputeDraw();
        }
        public void ResetDebugMeshNode(GamePlay.UWorld world)
        {
            var node = world.Root.FindFirstChild("Debug_VoxelDebugMeshNode");
            if (node != null)
            {
                node.Parent = null;
            }

            if (VxDebugMesh != null)
            {
                var material = VxDebugMesh.MaterialMesh.SubMeshes[0].Materials[0];
                VxDebugMesh = new Graphics.Mesh.TtMesh();
                var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
                var rectMesh = rect.ToMesh();
                var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
                materials[0] = material;
                VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<UMdfVoxelDebugMesh>.TypeDesc);
                VxDebugMesh.MdfQueue.MdfDatas = this;
            }

            var task = AddDebugMeshNodeToWorld(world);
        }
        public async System.Threading.Tasks.Task AddDebugMeshNodeToWorld(GamePlay.UWorld world)
        {
            var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), VxDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
            meshNode.NodeData.Name = "Debug_VoxelDebugMeshNode";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

            VxDebugMeshNode = meshNode;
        }

        private async Thread.Async.TtTask<bool> InitVxDebugger(Graphics.Pipeline.Shader.TtMaterial material)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            unsafe
            {
                VoxelGroupDebugger.SetSize(VxGroupPoolSize, (void*)0, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                VoxelDebugger.SetSize((uint)VxGroupPoolSize * VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide, (void*)0, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

                VxIndirectDebugDraws.SetSize(2 * 5, (void*)0, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_IndirectArgs);
            }
            
            CoreSDK.DisposeObject(ref SetupVxDebuggerDrawcall);
            SetupVxDebuggerDrawcall = rc.CreateComputeDraw();
            SetupVxDebugger = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupVxDebuggerShading>();
            
            CollectVxDebugger = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<CollectVxDebuggerShading>();

            VxDebugMesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = material;
            VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<UMdfVoxelDebugMesh>.TypeDesc);
            VxDebugMesh.MdfQueue.MdfDatas = this;

            ResetComputeDrawcall();

            return true;
        }
        #endregion

        bool mDebugVoxels = false;
        [Rtti.Meta]
        public bool DebugVoxels
        {
            get => mDebugVoxels;
            set
            {
                mDebugVoxels = value;
                if (value)
                {
                    VxDebugMeshNode?.UnsetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                }
                else
                {
                    VxDebugMeshNode?.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.Invisible);
                }
            }
        }

        private unsafe void TickVxDebugger(GamePlay.UWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            if (DebugVoxels == false)
            {
                return;
            }
            
            var cmd = BasePass.DrawCmdList;

            SetupVxDebugger.SetDrawcallDispatch(this, policy, SetupVxDebuggerDrawcall, 
                                        1, 1, 1, true);
            cmd.PushGpuDraw(SetupVxDebuggerDrawcall);

            CollectVxDebugger.SetDrawcallDispatch(this, policy, CollectVxDebuggerDrawcall,
                                        VxGroupPoolSize,
                                        1,
                                        1,
                                        true);
            cmd.PushGpuDraw(CollectVxDebuggerDrawcall);

            if (VxDebugMesh != null && world.Root.FindFirstChild("Debug_VoxelDebugMeshNode") == null)
            {
                var task = AddDebugMeshNodeToWorld(world);
            }
        }
    }
}
