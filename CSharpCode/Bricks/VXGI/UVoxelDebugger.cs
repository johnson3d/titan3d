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
        public override EVertexSteamType[] GetNeedStreams()
        {
            return new EVertexSteamType[] { EVertexSteamType.VST_Position, };
        }
        protected override void UpdateShaderCode()
        {
            var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine($"#include \"{RName.GetRName("Shaders/Compute/VXGI/VxDebugModifier.cginc", RName.ERNameType.Engine).Address}\"");
            codeBuilder.AddLine("void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)");
            codeBuilder.PushBrackets();
            {
                codeBuilder.AddLine($"DoVoxelDebugMeshVS(output, input);");
            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("void MdfQueueDoModifiersPS(inout PS_INPUT input, inout MTL_OUTPUT mtl)");
            codeBuilder.PushBrackets();
            {
                codeBuilder.AddLine($"DoVoxelDebugMeshPS(input, mtl);");
            }
            codeBuilder.PopBrackets();

            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION");
            codeBuilder.AddLine("#define MDFQUEUE_FUNCTION_PS");

            SourceCode = new IO.CMemStreamWriter();
            SourceCode.SetText(codeBuilder.ClassCode);
        }
        RHI.CShaderResources mAttachSRVs = null;
        public unsafe override void OnDrawCall(Graphics.Pipeline.URenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Graphics.Pipeline.URenderPolicy policy, Graphics.Mesh.UMesh mesh)
        {
            var vxNode = this.MdfDatas as UVoxelsNode;
            if (vxNode == null)
            {
                return;
            }                
            if (mAttachSRVs == null)
            {
                mAttachSRVs = new RHI.CShaderResources();

                var bindInfo = drawcall.Effect.ShaderProgram.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv,"VxDebugInstanceSRV");
                if (bindInfo != (IShaderBinder*)0)
                {
                    mAttachSRVs.mCoreObject.BindVS(bindInfo->VSBindPoint, vxNode.SrvVoxelDebugger.mCoreObject);
                }
            }
            drawcall.mCoreObject.BindAttachSRVs(mAttachSRVs.mCoreObject);
            drawcall.mCoreObject.SetIndirectDraw(vxNode.VxIndirectDebugDraws.mCoreObject, (uint)sizeof(UVoxelsNode.FIndirectDrawArgs));
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
        public RHI.CGpuBuffer VoxelGroupDebugger;
        public RHI.CGpuBuffer VoxelDebugger;
        public RHI.CGpuBuffer VxIndirectDebugDraws;

        public RHI.CUnorderedAccessView UavVoxelGroupDebugger;
        public RHI.CUnorderedAccessView UavVoxelDebugger;
        public RHI.CShaderResourceView SrvVoxelDebugger;
        public RHI.CUnorderedAccessView UavVxIndirectDebugDraws;

        private RHI.CShaderDesc CSDesc_SetupVxDebugger;
        private RHI.CComputeShader CS_SetupVxDebugger;
        private RHI.CComputeDrawcall SetupVxDebuggerDrawcall;

        private RHI.CShaderDesc CSDesc_CollectVxDebugger;
        private RHI.CComputeShader CS_CollectVxDebugger;
        private RHI.CComputeDrawcall CollectVxDebuggerDrawcall;

        public Graphics.Mesh.UMesh VxDebugMesh;
        public GamePlay.Scene.UMeshNode VxDebugMeshNode;

        private unsafe void ResetComputeDrawcall()
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            SetupVxDebuggerDrawcall = rc.CreateComputeDrawcall();
            SetupVxDebuggerDrawcall.mCoreObject.SetComputeShader(CS_SetupVxDebugger.mCoreObject);
            SetupVxDebuggerDrawcall.mCoreObject.SetDispatch(1, 1, 1);

            var srvIdx = CSDesc_SetupVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");///???
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxDebuggerDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            }
            srvIdx = CSDesc_SetupVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxIndirectDebugDraws");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxDebuggerDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVxIndirectDebugDraws.mCoreObject);
            }

            CollectVxDebuggerDrawcall = rc.CreateComputeDrawcall();
            CollectVxDebuggerDrawcall.mCoreObject.SetComputeShader(CS_CollectVxDebugger.mCoreObject);
            CollectVxDebuggerDrawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(VxSceneX, Dispatch_SetupDimArray3.x), 
                CoreDefine.Roundup(VxSceneY, Dispatch_SetupDimArray3.y), 
                CoreDefine.Roundup(VxSceneZ, Dispatch_SetupDimArray3.z));

            srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                CollectVxDebuggerDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            }
            srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VoxelGroupDebugger");
            if (srvIdx != (IShaderBinder*)0)
            {
                CollectVxDebuggerDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelGroupDebugger.mCoreObject);
            }
            srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VoxelDebugger");
            if (srvIdx != (IShaderBinder*)0)
            {
                CollectVxDebuggerDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelDebugger.mCoreObject);
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
                var rect = Graphics.Mesh.CMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
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

            var desc = new IGpuBufferDesc();
            desc.SetMode(false, true);
            desc.Type = EGpuBufferType.GBT_UavBuffer | EGpuBufferType.GBT_TBufferBuffer;
            desc.m_ByteWidth = VxGroupPoolSize * (uint)sizeof(FVoxelDebugger);
            desc.m_StructureByteStride = (uint)sizeof(FVoxelDebugger);
            VoxelGroupDebugger = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize;
            UavVoxelGroupDebugger = rc.CreateUnorderedAccessView(VoxelGroupDebugger, in uavDesc);

            desc.m_ByteWidth = VxGroupPoolSize * VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * (uint)sizeof(FVoxelDebugger);
            desc.m_StructureByteStride = (uint)sizeof(FVoxelDebugger);
            VoxelDebugger = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize * VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide;
            UavVoxelDebugger = rc.CreateUnorderedAccessView(VoxelDebugger, in uavDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetBuffer();
            srvDesc.mGpuBuffer = VoxelDebugger.mCoreObject;
            srvDesc.Buffer.NumElements = uavDesc.Buffer.NumElements;
            SrvVoxelDebugger = rc.CreateShaderResourceView(in srvDesc);

            desc.m_ByteWidth = 2 * (uint)sizeof(FIndirectDrawArgs);
            desc.m_StructureByteStride = (uint)sizeof(uint);
            desc.MiscFlags = (uint)(EResourceMiscFlag.DRAWINDIRECT_ARGS | EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);
            VxIndirectDebugDraws = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            desc.MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_STRUCTURED;

            uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
            uavDesc.Buffer.NumElements = 2 * 5;
            uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
            UavVxIndirectDebugDraws = rc.CreateUnorderedAccessView(VxIndirectDebugDraws, in uavDesc);
            uavDesc.Format = EPixelFormat.PXF_UNKNOWN;

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
            defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
            defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
            defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
            defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
            defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");

            defines.mCoreObject.AddDefine("DispatchX", $"2");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            CSDesc_SetupVxDebugger = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine),
                "CS_SetupVxDebugger", EShaderType.EST_ComputeShader, defines, null);
            CS_SetupVxDebugger = rc.CreateComputeShader(CSDesc_SetupVxDebugger);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray3.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray3.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray3.z}");
            CSDesc_CollectVxDebugger = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VxVisualDebugger.compute", RName.ERNameType.Engine),
                "CS_CollectVxDebugger", EShaderType.EST_ComputeShader, defines, null);
            CS_CollectVxDebugger = rc.CreateComputeShader(CSDesc_CollectVxDebugger);

            VxDebugMesh = new Graphics.Mesh.UMesh();
            var rect = Graphics.Mesh.CMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = material;
            VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<UMdfVoxelDebugMesh>.TypeDesc);
            VxDebugMesh.MdfQueue.MdfDatas = this;

            ResetComputeDrawcall();
        }
        #endregion

        bool mDebugVoxels = false;
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
            SetupVxDebuggerDrawcall.BuildPass(cmd);
            CollectVxDebuggerDrawcall.BuildPass(cmd);
            //UInt32 nUavInitialCounts = 1;
            //cmd.SetComputeShader(CS_SetupVxDebugger.mCoreObject);
            //var srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            //}
            //srvIdx = CSDesc_SetupVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxIndirectDebugDraws");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVxIndirectDebugDraws.mCoreObject, &nUavInitialCounts);
            //}
            //cmd.CSDispatch(1, 1, 1);

            //cmd.SetComputeShader(CS_CollectVxDebugger.mCoreObject);
            //var srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            //}
            //srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VoxelGroupDebugger");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelGroupDebugger.mCoreObject, &nUavInitialCounts);
            //}
            //srvIdx = CSDesc_CollectVxDebugger.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VoxelDebugger");
            //if (srvIdx != (IShaderBinder*)0)
            //{
            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelDebugger.mCoreObject, &nUavInitialCounts);
            //}
            //cmd.CSDispatch(CoreDefine.Roundup(VxSceneX, Dispatch_SetupDimArray3.x), CoreDefine.Roundup(VxSceneY, Dispatch_SetupDimArray3.y), CoreDefine.Roundup(VxSceneZ, Dispatch_SetupDimArray3.z));

            if (VxDebugMesh != null && world.Root.FindFirstChild("Debug_VoxelDebugMeshNode") == null)
            {
                var task = AddDebugMeshNodeToWorld(world);
            }
        }
    }
}
