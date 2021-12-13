using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public partial class UVoxelsNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        #region ConstantVar
        public const uint VxDescStructSize = 2;
        public const uint VxGroupCubeSide = 4;
        public const float VxSize = 0.25f;
        public const float VxGroupSize = VxSize * (float)VxGroupCubeSide;
        public const uint VxGroupPoolSize = 64 * 64 * 32;
        public const uint VxSceneX = 256;
        public const uint VxSceneZ = 256;
        public const uint VxSceneY = 64;
        public readonly UInt32_3 Dispatch_SetupDimArray1 = new UInt32_3(128,1,1);
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);
        public readonly UInt32_3 Dispatch_SetupDimArray3 = new UInt32_3(8, 8, 4);
        #endregion

        public void Cleanup()
        {
            VoxelPool?.Dispose();
            VoxelPool = null;

            VoxelAllocator?.Dispose();
            VoxelAllocator = null;

            VoxelScene?.Dispose();
            VoxelScene = null;

            UavVoxelPool = null;
            UavVoxelAllocator = null;
            UavVoxelScene = null;

            SrvAbedo = null;
            //UavAbedo?.Dispose();
            //UavAbedo = null;
        }
        
        #region VoxelInject        
        
        public struct FVxGroupId
        {
            public uint Index;
        }
        public RHI.CConstantBuffer CBuffer;

        public RHI.CGpuBuffer VoxelPool;//(group = VxGroupCubeSide*VxGroupCubeSide*VxGroupCubeSide) * VxGroupPoolSize;
        public RHI.CGpuBuffer VoxelAllocator;//VxGroupPoolSize
        public RHI.CGpuBuffer VoxelScene;//VxSceneX * VxSceneZ * VxSceneY 一米一个VXG的设计
        
        public RHI.CUnorderedAccessView UavVoxelPool;
        public RHI.CUnorderedAccessView UavVoxelAllocator;
        public RHI.CUnorderedAccessView UavVoxelScene;
        
        public uint DiffuseRTWidth;
        public uint DiffuseRTHeight;        
        RHI.CShaderResourceView SrvAbedo;
        RHI.CShaderResourceView SrvShadowMask;
        RHI.CShaderResourceView SrvDepth;
        //public RHI.CUnorderedAccessView UavAbedo;
        //public RHI.CUnorderedAccessView UavNormal;

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        private RHI.CShaderDesc CSDesc_SetupVxAllocator;
        private RHI.CComputeShader CS_SetupVxAllocator;
        private RHI.CComputeDrawcall SetupVxAllocatorDrawcall;

        private RHI.CShaderDesc CSDesc_SetupVxScene;
        private RHI.CComputeShader CS_SetupVxScene;
        private RHI.CComputeDrawcall SetupVxSceneDrawcall;

        private RHI.CShaderDesc CSDesc_InjectVoxels;
        private RHI.CComputeShader CS_InjectVoxels;
        private RHI.CComputeDrawcall InjectVoxelsDrawcall;

        private RHI.CShaderDesc CSDesc_EraseVoxelGroup;
        private RHI.CComputeShader CS_EraseVoxelGroup;
        private RHI.CComputeDrawcall EraseVoxelGroupDrawcall;
        #endregion

        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.IRenderPolicy policy, 
            Graphics.Pipeline.Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var desc = new IGpuBufferDesc();
            desc.SetMode(false, true);
            unsafe
            {
                desc.m_ByteWidth = VxGroupPoolSize * (uint)sizeof(int);
                desc.m_StructureByteStride = (uint)sizeof(uint);
            }
            VoxelAllocator = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize;
            UavVoxelAllocator = rc.CreateUnorderedAccessView(VoxelAllocator, in uavDesc);

            unsafe
            {
                desc.m_ByteWidth = VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * VxGroupPoolSize * (uint)sizeof(uint) * VxDescStructSize;
                desc.m_StructureByteStride = (uint)sizeof(uint);
            }
            VoxelPool = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)(VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * VxGroupPoolSize);
            UavVoxelPool = rc.CreateUnorderedAccessView(VoxelPool, in uavDesc);

            unsafe
            {
                desc.m_ByteWidth = VxSceneX * VxSceneZ * VxSceneY * (uint)sizeof(int);
                desc.m_StructureByteStride = (uint)sizeof(int);
            }
            VoxelScene = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)(VxSceneX * VxSceneZ * VxSceneY);
            UavVoxelScene = rc.CreateUnorderedAccessView(VoxelScene, in uavDesc);

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
            defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}"); 
            defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}"); 
            defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
            defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
            defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray1.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray1.z}");
            CSDesc_SetupVxAllocator = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine), 
                "CS_SetupVxAllocator", EShaderType.EST_ComputeShader, defines, null);
            CS_SetupVxAllocator = rc.CreateComputeShader(CSDesc_SetupVxAllocator);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.z}");
            CSDesc_SetupVxScene = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine),
                "CS_SetupVxScene", EShaderType.EST_ComputeShader, defines, null);
            CS_SetupVxScene = rc.CreateComputeShader(CSDesc_SetupVxScene);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.z}");
            CSDesc_InjectVoxels = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VoxelInject.compute", RName.ERNameType.Engine),
                "CS_InjectVoxels", EShaderType.EST_ComputeShader, defines, null);
            CS_InjectVoxels = rc.CreateComputeShader(CSDesc_InjectVoxels);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray3.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray3.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray3.z}");
            CSDesc_EraseVoxelGroup = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VoxelInject.compute", RName.ERNameType.Engine),
                "CS_EraseVoxelGroup", EShaderType.EST_ComputeShader, defines, null);
            CS_EraseVoxelGroup = rc.CreateComputeShader(CSDesc_EraseVoxelGroup);

            var cbIndex = CSDesc_InjectVoxels.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            CBuffer = rc.CreateConstantBuffer2(CSDesc_InjectVoxels, cbIndex);

            if (true)
            {
                var material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
                material.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
                //var rast = material.Rasterizer;
                //rast.FillMode = EFillMode.FMD_WIREFRAME;
                //material.Rasterizer = rast;
                InitVxDebugger(material);
            }

            BasePass.Initialize(rc, debugName);

            SrvAbedo = policy.GetBasePassNode().GBuffers.GetGBufferSRV(0);
            SrvDepth = policy.GetBasePassNode().GBuffers.GetDepthStencilSRV();

            DiffuseRTWidth = policy.GetBasePassNode().GBuffers.RTDesc[0].Width;
            DiffuseRTHeight = policy.GetBasePassNode().GBuffers.RTDesc[0].Height;

            mCurStep = EStep.Setup;

            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            
            SetupVxAllocatorDrawcall = rc.CreateComputeDrawcall();
            SetupVxAllocatorDrawcall.mCoreObject.SetComputeShader(CS_SetupVxAllocator.mCoreObject);
            SetupVxAllocatorDrawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(VxGroupPoolSize, Dispatch_SetupDimArray1.x), 1, 1);
            var srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxAllocatorDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject);
            }
            srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxAllocatorDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject);
            }
            srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxAllocatorDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject);
            }

            SetupVxSceneDrawcall = rc.CreateComputeDrawcall();
            SetupVxSceneDrawcall.mCoreObject.SetComputeShader(CS_SetupVxScene.mCoreObject);
            SetupVxSceneDrawcall.mCoreObject.SetDispatch(
                CoreDefine.Roundup(VxSceneX, Dispatch_SetupDimArray2.x), 
                CoreDefine.Roundup(VxSceneY, Dispatch_SetupDimArray2.y), 
                CoreDefine.Roundup(VxSceneZ, Dispatch_SetupDimArray2.z));            
            srvIdx = CSDesc_SetupVxScene.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupVxSceneDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject);
            }

            EraseVoxelGroupDrawcall = rc.CreateComputeDrawcall();
            EraseVoxelGroupDrawcall.mCoreObject.SetComputeShader(CS_EraseVoxelGroup.mCoreObject);
            
            srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                EraseVoxelGroupDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx != (IShaderBinder*)0)
            {
                var camera = policy.GetBasePassNode().GBuffers.Camera;
                EraseVoxelGroupDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
            }
            srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
            if (srvIdx != (IShaderBinder*)0)
            {
                EraseVoxelGroupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject);
            }
            srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
            if (srvIdx != (IShaderBinder*)0)
            {
                EraseVoxelGroupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject);
            }
            srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                EraseVoxelGroupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject);
            }
            srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                EraseVoxelGroupDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject);
            }

            InjectVoxelsDrawcall = rc.CreateComputeDrawcall();
            InjectVoxelsDrawcall.mCoreObject.SetComputeShader(CS_InjectVoxels.mCoreObject);            
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx != (IShaderBinder*)0)
            {
                var camera = policy.GetBasePassNode().GBuffers.Camera;
                InjectVoxelsDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GBufferAbedo");
            if (srvIdx != (IShaderBinder*)0)
            {
                //CSDesc_InjectVoxels.mCoreObject.GetCBufferDesc(srvIdx, &desc);
                //cmd.CSSetUnorderedAccessView(desc.m_CSBindPoint, UavDiffuseAndDepth.mCoreObject, &nUavInitialCounts);

                InjectVoxelsDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, SrvAbedo.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferShadowMask");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, SrvShadowMask.mCoreObject);
            }
            srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GBufferDepth");
            if (srvIdx != (IShaderBinder*)0)
            {
                InjectVoxelsDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, SrvDepth.mCoreObject);
            }
        }
        public override void OnResize(Graphics.Pipeline.IRenderPolicy policy, float x, float y)
        {
            SrvShadowMask = policy.GetFinalShowRSV();

            SrvAbedo = policy.GetBasePassNode().GBuffers.GetGBufferSRV(0);
            SrvDepth = policy.GetBasePassNode().GBuffers.GetDepthStencilSRV();

            //UavAbedo?.Dispose();
            //UavAbedo = policy.GetBasePassNode().GBuffers.CreateUAV(0);
            DiffuseRTWidth = policy.GetBasePassNode().GBuffers.RTDesc[0].Width;
            DiffuseRTHeight = policy.GetBasePassNode().GBuffers.RTDesc[0].Height;

            ResetComputeDrawcall(policy);
        }
        private enum EStep
        {
            Setup,
            InjectVoxels,
        }
        EStep mCurStep = EStep.Setup;
        public BoundingBox VxSceneBox = new BoundingBox(0, -VxSceneY * VxGroupSize * 0.5f, 0, VxSceneX * VxGroupSize, VxSceneY * VxGroupSize * 0.5f, VxSceneZ * VxGroupSize);
        UInt32_3 EraseVxStart = new UInt32_3(0, 0, 0);
        UInt32_3 VxEraseGroupSize = new UInt32_3(0, 0, 0);
        public void SetEraseBox(in BoundingBox box)
        {
            BoundingBox oBox;
            BoundingBox.And(in VxSceneBox, in box, out oBox);
            if (oBox.IsEmpty())
            {
                VxEraseGroupSize = new UInt32_3(0, 0, 0);
                return;
            }
            var eraseStartPos = (oBox.Minimum - VxSceneBox.Minimum) / VxGroupSize;

            EraseVxStart.x = (UInt32)eraseStartPos.X;
            EraseVxStart.y = (UInt32)eraseStartPos.Y;
            EraseVxStart.z = (UInt32)eraseStartPos.Z;

            var eraseSize = oBox.GetSize() / VxGroupSize;

            VxEraseGroupSize.x = (UInt32)eraseSize.X;
            VxEraseGroupSize.y = (UInt32)eraseSize.Y;
            VxEraseGroupSize.z = (UInt32)eraseSize.Z;
            
        }
        bool bTestErase = false;
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            if (bTestErase)
            {
                SetEraseBox(in VxSceneBox);
            }
            if (CBuffer != null)
            {
                var idx = CBuffer.mCoreObject.FindVar("GBufferSize");
                Vector2 GBufferSize;
                GBufferSize.X = DiffuseRTWidth;
                GBufferSize.Y = DiffuseRTHeight;                
                CBuffer.SetValue(idx, in GBufferSize);

                var zNear = policy.GetBasePassNode().GBuffers.Camera.mCoreObject.mZNear;
                var zfar = policy.GetBasePassNode().GBuffers.Camera.mCoreObject.mZFar;

                var ReconstructPosArg = new Vector2(zfar / (zfar - zNear), zNear * zfar / (zNear - zfar));
                idx = CBuffer.mCoreObject.FindVar("ReconstructPosArg");
                CBuffer.SetValue(idx, in ReconstructPosArg);
                
                idx = CBuffer.mCoreObject.FindVar("VxStartPosition");
                var VxStartPosition = VxSceneBox.Minimum;
                CBuffer.SetValue(idx, in VxStartPosition);
                
                idx = CBuffer.mCoreObject.FindVar("EraseVxStart");
                CBuffer.SetValue(idx, in EraseVxStart); 

                if (VxDebugMesh != null)
                {
                    idx = CBuffer.mCoreObject.FindVar("VxDebugger_IndexCountPerInstance");
                    var meshAtomDesc = VxDebugMesh.MaterialMesh.Mesh.mCoreObject.GetAtom(0, 0);
                    var VxDebugger_IndexCountPerInstance = meshAtomDesc->NumPrimitives * 3;
                    CBuffer.SetValue(idx, in VxDebugger_IndexCountPerInstance);
                }

                var cmd = BasePass.DrawCmdList.mCoreObject;                
                CBuffer.mCoreObject.UpdateDrawPass(cmd, 0);
            }
            switch (mCurStep)
            {
                case EStep.Setup:
                    {
                        if (CS_SetupVxAllocator != null && CS_SetupVxScene != null)
                        {
                            var cmd = BasePass.DrawCmdList.mCoreObject;

                            SetupVxAllocatorDrawcall.mCoreObject.BuildPass(cmd);
                            SetupVxSceneDrawcall.mCoreObject.BuildPass(cmd);

                            //cmd.SetComputeShader(CS_SetupVxAllocator.mCoreObject);
                            //var srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
                            //if (srvIdx != (IShaderBinder*)0)
                            //{
                            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject, &nUavInitialCounts);
                            //}
                            //srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
                            //if (srvIdx != (IShaderBinder*)0)
                            //{
                            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject, &nUavInitialCounts);
                            //}
                            //srvIdx = CSDesc_SetupVxAllocator.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                            //if (srvIdx != (IShaderBinder*)0)
                            //{
                            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject, &nUavInitialCounts);
                            //}
                            //cmd.CSDispatch(CoreDefine.Roundup(VxGroupPoolSize, Dispatch_SetupDimArray1.x), 1, 1);

                            //cmd.SetComputeShader(CS_SetupVxScene.mCoreObject);
                            //srvIdx = CSDesc_SetupVxScene.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
                            //if (srvIdx != (IShaderBinder*)0)
                            //{
                            //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject, &nUavInitialCounts);
                            //}
                            //cmd.CSDispatch(CoreDefine.Roundup(VxSceneX, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(VxSceneY, Dispatch_SetupDimArray2.y), CoreDefine.Roundup(VxSceneZ, Dispatch_SetupDimArray2.z));

                            if (cmd.BeginCommand())
                            {
                                cmd.EndCommand();
                            }
                        }
                        mCurStep = EStep.InjectVoxels;
                    }
                    break;
                case EStep.InjectVoxels:
                    {
                        if (CS_InjectVoxels != null)
                        {
                            var cmd = BasePass.DrawCmdList.mCoreObject;

                            #region erase voxelgroups
                            if (VxEraseGroupSize.AnyNotZero())
                            {
                                EraseVoxelGroupDrawcall.mCoreObject.SetDispatch(
                                    CoreDefine.Roundup(VxEraseGroupSize.x, Dispatch_SetupDimArray3.x),
                                    CoreDefine.Roundup(VxEraseGroupSize.y, Dispatch_SetupDimArray3.y),
                                    CoreDefine.Roundup(VxEraseGroupSize.z, Dispatch_SetupDimArray3.z));
                                EraseVoxelGroupDrawcall.mCoreObject.BuildPass(cmd);
                                //cmd.SetComputeShader(CS_EraseVoxelGroup.mCoreObject);
                                //var srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    var camera = policy.GetBasePassNode().GBuffers.Camera;
                                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
                                //}
                                //srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_EraseVoxelGroup.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject, &nUavInitialCounts);
                                //}
                                //cmd.CSDispatch(CoreDefine.Roundup(VxEraseGroupSize.x, Dispatch_SetupDimArray3.x),
                                //    CoreDefine.Roundup(VxEraseGroupSize.y, Dispatch_SetupDimArray3.y),
                                //    CoreDefine.Roundup(VxEraseGroupSize.z, Dispatch_SetupDimArray3.z));

                                VxEraseGroupSize.SetZero();
                            }
                            #endregion

                            #region inject voxels
                            {
                                InjectVoxelsDrawcall.mCoreObject.SetDispatch(
                                    CoreDefine.Roundup(DiffuseRTWidth, Dispatch_SetupDimArray2.x),
                                    CoreDefine.Roundup(DiffuseRTHeight, Dispatch_SetupDimArray2.y),
                                    1);
                                InjectVoxelsDrawcall.mCoreObject.BuildPass(cmd);
                                //cmd.SetComputeShader(CS_InjectVoxels.mCoreObject);
                                //var srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, CBuffer.mCoreObject);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    var camera = policy.GetBasePassNode().GBuffers.Camera;
                                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxAllocator");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelAllocator.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxPool");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelPool.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GpuSceneDesc");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, policy.GetGpuSceneNode().GpuSceneDescUAV.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "VxScene");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, UavVoxelScene.mCoreObject, &nUavInitialCounts);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GBufferAbedo");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    //CSDesc_InjectVoxels.mCoreObject.GetCBufferDesc(srvIdx, &desc);
                                //    //cmd.CSSetUnorderedAccessView(desc.m_CSBindPoint, UavDiffuseAndDepth.mCoreObject, &nUavInitialCounts);

                                //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, SrvAbedo.mCoreObject);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GBufferShadowMask");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, SrvShadowMask.mCoreObject);
                                //}
                                //srvIdx = CSDesc_InjectVoxels.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "GBufferDepth");
                                //if (srvIdx != (IShaderBinder*)0)
                                //{
                                //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, SrvDepth.mCoreObject);
                                //}
                                //cmd.CSDispatch(CoreDefine.Roundup(DiffuseRTWidth, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(DiffuseRTHeight, Dispatch_SetupDimArray2.y), 1);
                            }
                            #endregion

                            TickVxDebugger(world);

                            if (cmd.BeginCommand())
                            {
                                cmd.EndCommand();
                            }
                        }
                    }
                    break;
            }            
        }
        
        public unsafe override void TickRender(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
