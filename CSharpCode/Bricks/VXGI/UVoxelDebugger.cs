using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public class UMdfVoxelDebugMesh : Graphics.Pipeline.Shader.UMdfQueue
    {
        public UMdfVoxelDebugMesh()
        {
            UpdateShaderCode();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position, };
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine($"#include \"{RName.GetRName("Shaders/Bricks/VXGI/VxDebugModifier.cginc", RName.ERNameType.Engine).Address}\"", ref sourceCode);
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                codeBuilder.AddLine($"DoVoxelDebugMeshVS(output, input);", ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);

            codeBuilder.AddLine("void MdfQueueDoModifiersPS(inout PS_INPUT input, inout MTL_OUTPUT mtl)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                codeBuilder.AddLine($"DoVoxelDebugMeshPS(input, mtl);", ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION", ref sourceCode);
            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION_PS", ref sourceCode);

            SourceCode = new NxRHI.UShaderCode();
            SourceCode.TextCode = sourceCode;
        }
        public unsafe override void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh, int atom)
        {
            base.OnDrawCall(shadingType, drawcall, policy, mesh, atom);

            var vxNode = this.MdfDatas as UVoxelsNode;
            if (vxNode == null)
            {
                return;
            }
            //if (mAttachSRVs == null)
            //{
            //    mAttachSRVs = new RHI.CShaderResources();

            //    var bindInfo = drawcall.Effect.ShaderProgram.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv,"VxDebugInstanceSRV");
            //    if (bindInfo != (IShaderBinder*)0)
            //    {
            //        mAttachSRVs.mCoreObject.BindVS(bindInfo->VSBindPoint, vxNode.SrvVoxelDebugger.mCoreObject);
            //    }
            //}
            var binder = drawcall.FindBinder("VxDebugInstanceSRV");
            drawcall.BindSRV(binder, vxNode.SrvVoxelDebugger);
            drawcall.mCoreObject.BindIndirectDrawArgsBuffer(vxNode.VxIndirectDebugDraws.mCoreObject, (uint)sizeof(UVoxelsNode.FIndirectDrawArgs));
        }
    }

    partial class UVoxelsNode
    {
        #region VxDebugger
        public struct FIndirectDrawArgs
        {
            public uint IndexCountPerInstance;
            public uint InstanceCount;
            public uint StartIndexLocation;
            public int BaseVertexLocation;
            public uint StartInstanceLocation;
        }
        public struct FVoxelDebugger
        {
            public Vector3 Position;
            public float Scale;

            public Vector3 Color;
            public float Pad0;
        }
        public NxRHI.UBuffer VoxelGroupDebugger;
        public NxRHI.UBuffer VoxelDebugger;
        public NxRHI.UBuffer VxIndirectDebugDraws;

        public NxRHI.UUaView UavVoxelGroupDebugger;
        public NxRHI.UUaView UavVoxelDebugger;
        public NxRHI.USrView SrvVoxelDebugger;
        public NxRHI.UUaView UavVxIndirectDebugDraws;

        private NxRHI.UComputeEffect SetupVxDebugger;
        private NxRHI.UComputeDraw SetupVxDebuggerDrawcall;

        private NxRHI.UComputeEffect CollectVxDebugger;
        private NxRHI.UComputeDraw CollectVxDebuggerDrawcall;

        public Graphics.Mesh.UMesh VxDebugMesh;
        public GamePlay.Scene.UMeshNode VxDebugMeshNode;

        private unsafe void ResetComputeDrawcall()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            CoreSDK.DisposeObject(ref SetupVxDebuggerDrawcall);
            SetupVxDebuggerDrawcall = rc.CreateComputeDraw();
            SetupVxDebuggerDrawcall.SetComputeEffect(SetupVxDebugger);
            SetupVxDebuggerDrawcall.SetDispatch(1, 1, 1);

            var srvIdx = SetupVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");///???
            if (srvIdx.IsValidPointer)
            {
                SetupVxDebuggerDrawcall.BindCBuffer(srvIdx, CBuffer);
            }
            srvIdx = SetupVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxIndirectDebugDraws");
            if (srvIdx.IsValidPointer)
            {
                SetupVxDebuggerDrawcall.BindUav(srvIdx, UavVxIndirectDebugDraws);
            }

            CoreSDK.DisposeObject(ref CollectVxDebuggerDrawcall);
            CollectVxDebuggerDrawcall = rc.CreateComputeDraw();
            CollectVxDebuggerDrawcall.SetComputeEffect(CollectVxDebugger);
            CollectVxDebuggerDrawcall.SetDispatch(MathHelper.Roundup(VxGroupPoolSize, Dispatch_SetupDimArray1.X),
                1,
                1);

            // renwind test
            srvIdx = CollectVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
            if (srvIdx.IsValidPointer)
            {
                CollectVxDebuggerDrawcall.BindUav(srvIdx, UavVoxelPool);
            }
            srvIdx = CollectVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxIndirectDebugDraws");
            if (srvIdx.IsValidPointer)
            {
                CollectVxDebuggerDrawcall.BindUav(srvIdx, UavVxIndirectDebugDraws);
            }

            srvIdx = CollectVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx.IsValidPointer)
            {
                CollectVxDebuggerDrawcall.BindCBuffer(srvIdx, CBuffer);
            }
            srvIdx = CollectVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VoxelGroupDebugger");
            if (srvIdx.IsValidPointer)
            {
                CollectVxDebuggerDrawcall.BindUav(srvIdx, UavVoxelGroupDebugger);
            }
            srvIdx = CollectVxDebuggerDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VoxelDebugger");
            if (srvIdx.IsValidPointer)
            {
                CollectVxDebuggerDrawcall.BindUav(srvIdx, UavVoxelDebugger);
            }
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
                var material = VxDebugMesh.MaterialMesh.Materials[0];
                VxDebugMesh = new Graphics.Mesh.UMesh();
                var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
                var rectMesh = rect.ToMesh();
                var materials = new Graphics.Pipeline.Shader.UMaterial[1];
                materials[0] = material;
                VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<UMdfVoxelDebugMesh>.TypeDesc);
                VxDebugMesh.MdfQueue.MdfDatas = this;
            }

            var task = AddDebugMeshNodeToWorld(world);
        }
        public async System.Threading.Tasks.Task AddDebugMeshNodeToWorld(GamePlay.UWorld world)
        {
            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), VxDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
            meshNode.NodeData.Name = "Debug_VoxelDebugMeshNode";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

            VxDebugMeshNode = meshNode;
        }

        private unsafe void InitVxDebugger(Graphics.Pipeline.Shader.UMaterial material)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault(false, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            desc.Type = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;// | NxRHI.EBufferType.TBuffer;
            desc.Size = VxGroupPoolSize * (uint)sizeof(FVoxelDebugger);
            desc.StructureStride = (uint)sizeof(FVoxelDebugger);
            VoxelGroupDebugger = rc.CreateBuffer(in desc);
            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(false);
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVoxelGroupDebugger = rc.CreateUAV(VoxelGroupDebugger, in uavDesc);

            desc.Size = VxGroupPoolSize * VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * (uint)sizeof(FVoxelDebugger);
            desc.StructureStride = (uint)sizeof(FVoxelDebugger);
            VoxelDebugger = rc.CreateBuffer(in desc);
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize * VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide;
            UavVoxelDebugger = rc.CreateUAV(VoxelDebugger, in uavDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetBuffer(false);
            srvDesc.Buffer.NumElements = uavDesc.Buffer.NumElements;
            srvDesc.Buffer.StructureByteStride = desc.StructureStride;
            SrvVoxelDebugger = rc.CreateSRV(VoxelDebugger, in srvDesc);

            desc.Size = 2 * (uint)sizeof(FIndirectDrawArgs);
            desc.StructureStride = (uint)sizeof(uint);
            desc.Type = NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_IndirectArgs;
            desc.MiscFlags = NxRHI.EResourceMiscFlag.RM_DRAWINDIRECT_ARGS | NxRHI.EResourceMiscFlag.RM_BUFFER_ALLOW_RAW_VIEWS;
            VxIndirectDebugDraws = rc.CreateBuffer(in desc);
            desc.MiscFlags = NxRHI.EResourceMiscFlag.RM_BUFFER_STRUCTURED;

            uavDesc.SetBuffer(true);
            uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
            uavDesc.Buffer.NumElements = 2 * 5;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVxIndirectDebugDraws = rc.CreateUAV(VxIndirectDebugDraws, in uavDesc);
            uavDesc.Format = EPixelFormat.PXF_UNKNOWN;

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
            defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
            defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
            defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
            defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
            defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");

            defines.mCoreObject.AddDefine("DispatchX", $"2");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            SetupVxDebugger = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine),
                "CS_SetupVxDebugger", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);
            
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            CollectVxDebugger = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine),
                "CS_CollectVxDebugger", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            VxDebugMesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = material;
            VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<UMdfVoxelDebugMesh>.TypeDesc);
            VxDebugMesh.MdfQueue.MdfDatas = this;

            ResetComputeDrawcall();
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
                    VxDebugMeshNode?.UnsetStyle(GamePlay.Scene.UNode.ENodeStyles.Invisible);
                }
                else
                {
                    VxDebugMeshNode?.SetStyle(GamePlay.Scene.UNode.ENodeStyles.Invisible);
                }
            }
        }

        private unsafe void TickVxDebugger(GamePlay.UWorld world)
        {
            if (DebugVoxels == false)
            {
                return;
            }
            
            var cmd = BasePass.DrawCmdList;
            //SetupVxDebuggerDrawcall.Commit(cmd);
            //CollectVxDebuggerDrawcall.Commit(cmd);

            cmd.PushGpuDraw(SetupVxDebuggerDrawcall);
            cmd.PushGpuDraw(CollectVxDebuggerDrawcall);

            if (VxDebugMesh != null && world.Root.FindFirstChild("Debug_VoxelDebugMeshNode") == null)
            {
                var task = AddDebugMeshNodeToWorld(world);
            }
        }
    }
}
