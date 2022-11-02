using EngineNS.EGui.Slate;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public partial class UVoxelsNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin GpuScenePinIn = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("GpuScene");
        public Graphics.Pipeline.Common.URenderGraphPin AlbedoPinIn = Graphics.Pipeline.Common.URenderGraphPin.CreateInput("Albedo");
        public Graphics.Pipeline.Common.URenderGraphPin DepthPinIn = Graphics.Pipeline.Common.URenderGraphPin.CreateInput("Depth");
        public Graphics.Pipeline.Common.URenderGraphPin ShadowMaskPinIn = Graphics.Pipeline.Common.URenderGraphPin.CreateInput("ShadowMask");

        public Graphics.Pipeline.Common.URenderGraphPin VxPoolPinOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("VxPool", false, EPixelFormat.PXF_UNKNOWN);
        public Graphics.Pipeline.Common.URenderGraphPin VxScenePinOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("VxScene", false, EPixelFormat.PXF_UNKNOWN);

        public UVoxelsNode()
        {
            Name = "UVoxelsNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(GpuScenePinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            AddInput(AlbedoPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_SRV);
            AddInput(ShadowMaskPinIn, NxRHI.EBufferType.BFT_SRV);

            VxPoolPinOut.LifeMode = Graphics.Pipeline.UAttachBuffer.ELifeMode.Imported;
            AddOutput(VxPoolPinOut, NxRHI.EBufferType.BFT_UAV);
            VxScenePinOut.LifeMode = Graphics.Pipeline.UAttachBuffer.ELifeMode.Imported;
            AddOutput(VxScenePinOut, NxRHI.EBufferType.BFT_UAV);
        }

        public override void FrameBuild()
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(VxPoolPinOut);
            attachement.Buffer = VoxelPool;
            attachement.Uav = UavVoxelPool;

            attachement = RenderGraph.AttachmentCache.ImportAttachment(VxScenePinOut);
            attachement.Buffer = VoxelScene;
            attachement.Uav = UavVoxelScene;
        }
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

        public override void Cleanup()
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

            //UavAbedo?.Dispose();
            //UavAbedo = null;
            base.Cleanup();
        }
        
        #region VoxelInject        
        
        public struct FVxGroupId
        {
            public uint Index;
        }
        public NxRHI.UCbView CBuffer;

        public NxRHI.UBuffer VoxelPool;//(group = VxGroupCubeSide*VxGroupCubeSide*VxGroupCubeSide) * VxGroupPoolSize;
        public NxRHI.UBuffer VoxelAllocator;//VxGroupPoolSize
        public NxRHI.UBuffer VoxelScene;//VxSceneX * VxSceneZ * VxSceneY 一米一个VXG的设计
        
        public NxRHI.UUaView UavVoxelPool;
        public NxRHI.UUaView UavVoxelAllocator;
        public NxRHI.UUaView UavVoxelScene;
        
        public uint DiffuseRTWidth;
        public uint DiffuseRTHeight;
        //public NxRHI.UUaView UavAbedo;
        //public NxRHI.UUaView UavNormal;

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        private NxRHI.UComputeEffect SetupVxAllocator;
        private NxRHI.UComputeDraw SetupVxAllocatorDrawcall;

        private NxRHI.UComputeEffect SetupVxScene;
        private NxRHI.UComputeDraw SetupVxSceneDrawcall;

        private NxRHI.UComputeEffect InjectVoxels;
        private NxRHI.UComputeDraw InjectVoxelsDrawcall;

        private NxRHI.UComputeEffect EraseVoxelGroup;
        private NxRHI.UComputeDraw EraseVoxelGroupDrawcall;
        #endregion

        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault();
            unsafe
            {
                desc.Size = VxGroupPoolSize * (uint)sizeof(int);
                desc.StructureStride = (uint)sizeof(uint);
            }
            VoxelAllocator = rc.CreateBuffer(in desc);
            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(0);
            uavDesc.Buffer.NumElements = (uint)VxGroupPoolSize;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVoxelAllocator = rc.CreateUAV(VoxelAllocator, in uavDesc);

            unsafe
            {
                desc.Size = VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * VxGroupPoolSize * (uint)sizeof(uint) * VxDescStructSize;
                desc.StructureStride = (uint)sizeof(uint);
            }
            VoxelPool = rc.CreateBuffer(in desc);
            uavDesc.SetBuffer(0);
            uavDesc.Buffer.NumElements = (uint)(VxGroupCubeSide * VxGroupCubeSide * VxGroupCubeSide * VxGroupPoolSize);
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVoxelPool = rc.CreateUAV(VoxelPool, in uavDesc);

            unsafe
            {
                desc.Size = VxSceneX * VxSceneZ * VxSceneY * (uint)sizeof(int);
                desc.StructureStride = (uint)sizeof(int);
            }
            VoxelScene = rc.CreateBuffer(in desc);
            uavDesc.SetBuffer(0);
            uavDesc.Buffer.NumElements = (uint)(VxSceneX * VxSceneZ * VxSceneY);
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVoxelScene = rc.CreateUAV(VoxelScene, in uavDesc);

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
            defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}"); 
            defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}"); 
            defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
            defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
            defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray1.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray1.Z}");
            SetupVxAllocator = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine).Address, 
                "CS_SetupVxAllocator", NxRHI.EShaderType.SDT_ComputeShader, null, null, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            SetupVxScene = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine).Address,
                "CS_SetupVxScene" , NxRHI.EShaderType.SDT_ComputeShader, null, null, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");
            InjectVoxels = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/VXGI/VoxelInject.compute", RName.ERNameType.Engine).Address,
                "CS_InjectVoxels", NxRHI.EShaderType.SDT_ComputeShader, null, null, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray3.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray3.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray3.Z}");
            EraseVoxelGroup = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/VXGI/VoxelInject.compute", RName.ERNameType.Engine).Address,
                "CS_EraseVoxelGroup", NxRHI.EShaderType.SDT_ComputeShader, null, null, null, defines, null);

            var cbIndex = InjectVoxels.FindBinder(VNameString.FromString("cbGBufferDesc"));
            CBuffer = rc.CreateCBV(cbIndex);

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

            mCurStep = EStep.Setup;

            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(Graphics.Pipeline.URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            
            SetupVxAllocatorDrawcall = rc.CreateComputeDraw();
            SetupVxAllocatorDrawcall.SetComputeEffect(SetupVxAllocator);
            SetupVxAllocatorDrawcall.SetDispatch(CoreDefine.Roundup(VxGroupPoolSize, Dispatch_SetupDimArray1.X), 1, 1);
            var srvIdx = SetupVxAllocatorDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxAllocator");
            if (srvIdx.IsValidPointer)
            {
                SetupVxAllocatorDrawcall.BindUav(srvIdx, UavVoxelAllocator);
            }
            srvIdx = SetupVxAllocatorDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxPool");
            if (srvIdx.IsValidPointer)
            {
                SetupVxAllocatorDrawcall.BindUav(srvIdx, UavVoxelPool);
            }
            
            SetupVxSceneDrawcall = rc.CreateComputeDraw();
            SetupVxSceneDrawcall.SetComputeEffect(SetupVxScene);
            SetupVxSceneDrawcall.SetDispatch(
                CoreDefine.Roundup(VxSceneX, Dispatch_SetupDimArray2.X), 
                CoreDefine.Roundup(VxSceneY, Dispatch_SetupDimArray2.Y), 
                CoreDefine.Roundup(VxSceneZ, Dispatch_SetupDimArray2.Z));            
            srvIdx = SetupVxSceneDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxScene");
            if (srvIdx.IsValidPointer)
            {
                SetupVxSceneDrawcall.BindUav(srvIdx, UavVoxelScene);
            }

            EraseVoxelGroupDrawcall = rc.CreateComputeDraw();
            EraseVoxelGroupDrawcall.SetComputeEffect(EraseVoxelGroup);
            
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx.IsValidPointer)
            {
                EraseVoxelGroupDrawcall.BindCBuffer(srvIdx, CBuffer);
            }
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx.IsValidPointer)
            {
                var camera = policy.DefaultCamera;
                EraseVoxelGroupDrawcall.BindCBuffer(srvIdx, camera.PerCameraCBuffer);
            }
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxAllocator");
            if (srvIdx.IsValidPointer)
            {
                EraseVoxelGroupDrawcall.BindUav(srvIdx, UavVoxelAllocator);
            }
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxPool");
            if (srvIdx.IsValidPointer)
            {
                EraseVoxelGroupDrawcall.BindUav(srvIdx, UavVoxelPool);
            }
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxScene");
            if (srvIdx.IsValidPointer)
            {
                EraseVoxelGroupDrawcall.BindUav(srvIdx, UavVoxelScene);
            }

            InjectVoxelsDrawcall = rc.CreateComputeDraw();
            InjectVoxelsDrawcall.SetComputeEffect(InjectVoxels);            
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
            if (srvIdx.IsValidPointer)
            {
                InjectVoxelsDrawcall.BindCBuffer(srvIdx, CBuffer);
            }
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx.IsValidPointer)
            {
                var camera = policy.DefaultCamera;
                InjectVoxelsDrawcall.BindCBuffer(srvIdx, camera.PerCameraCBuffer);
            }
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxAllocator");
            if (srvIdx.IsValidPointer)
            {
                InjectVoxelsDrawcall.BindUav(srvIdx, UavVoxelAllocator);
            }
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxPool");
            if (srvIdx.IsValidPointer)
            {
                InjectVoxelsDrawcall.BindUav(srvIdx, UavVoxelPool);
            }
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxScene");
            if (srvIdx.IsValidPointer)
            {
                InjectVoxelsDrawcall.BindUav(srvIdx, UavVoxelScene);
            }
            
        }
        public override void OnResize(Graphics.Pipeline.URenderPolicy policy, float x, float y)
        {
            //UavAbedo?.Dispose();
            //UavAbedo = policy.GetBasePassNode().GBuffers.CreateUAV(0);
            DiffuseRTWidth = (uint)x;// policy.GetBasePassNode().GBuffers.RTDesc[0].Width;
            DiffuseRTHeight = (uint)y;// policy.GetBasePassNode().GBuffers.RTDesc[0].Height;

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

            EraseVxStart.X = (UInt32)eraseStartPos.X;
            EraseVxStart.Y = (UInt32)eraseStartPos.Y;
            EraseVxStart.Z = (UInt32)eraseStartPos.Z;

            var eraseSize = oBox.GetSize() / VxGroupSize;

            VxEraseGroupSize.X = (UInt32)eraseSize.X;
            VxEraseGroupSize.Y = (UInt32)eraseSize.Y;
            VxEraseGroupSize.Z = (UInt32)eraseSize.Z;
            
        }
        bool bTestErase = false;
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UVoxelsNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (bTestErase)
                {
                    SetEraseBox(in VxSceneBox);
                }
                if (CBuffer != null)
                {
                    var idx = CBuffer.ShaderBinder.FindField("GBufferSize");
                    Vector2 GBufferSize;
                    GBufferSize.X = DiffuseRTWidth;
                    GBufferSize.Y = DiffuseRTHeight;
                    CBuffer.SetValue(idx, in GBufferSize);

                    var zNear = policy.DefaultCamera.mCoreObject.mZNear;
                    var zfar = policy.DefaultCamera.mCoreObject.mZFar;

                    var ReconstructPosArg = new Vector2(zfar / (zfar - zNear), zNear * zfar / (zNear - zfar));
                    idx = CBuffer.ShaderBinder.FindField("ReconstructPosArg");
                    CBuffer.SetValue(idx, in ReconstructPosArg);

                    idx = CBuffer.ShaderBinder.FindField("VxStartPosition");
                    var VxStartPosition = VxSceneBox.Minimum;
                    CBuffer.SetValue(idx, in VxStartPosition);

                    idx = CBuffer.ShaderBinder.FindField("EraseVxStart");
                    CBuffer.SetValue(idx, in EraseVxStart);

                    if (VxDebugMesh != null)
                    {
                        idx = CBuffer.ShaderBinder.FindField("VxDebugger_IndexCountPerInstance");
                        var meshAtomDesc = VxDebugMesh.MaterialMesh.Mesh.mCoreObject.GetAtom(0, 0);
                        var VxDebugger_IndexCountPerInstance = meshAtomDesc->NumPrimitives * 3;
                        CBuffer.SetValue(idx, in VxDebugger_IndexCountPerInstance);
                    }

                    var cmd = BasePass.DrawCmdList.mCoreObject;
                    CBuffer.FlushDirty(cmd, false);
                }
                switch (mCurStep)
                {
                    case EStep.Setup:
                        {
                            if (SetupVxAllocator != null && SetupVxScene != null)
                            {
                                var cmd = BasePass.DrawCmdList;

                                var srvIdx = SetupVxAllocatorDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                if (srvIdx.IsValidPointer)
                                {
                                    var attachBuffer = GetAttachBuffer(GpuScenePinIn);
                                    SetupVxAllocatorDrawcall.BindUav(srvIdx, attachBuffer.Uav);
                                }

                                SetupVxAllocatorDrawcall.Commit(cmd);
                                SetupVxSceneDrawcall.Commit(cmd);

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
                            if (InjectVoxels != null)
                            {
                                var cmd = BasePass.DrawCmdList;

                                #region erase voxelgroups
                                if (VxEraseGroupSize != UInt32_3.Zero)
                                {
                                    var srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        var attachBuffer = GetAttachBuffer(GpuScenePinIn);
                                        EraseVoxelGroupDrawcall.BindUav(srvIdx, attachBuffer.Uav);
                                    }

                                    EraseVoxelGroupDrawcall.SetDispatch(
                                        CoreDefine.Roundup(VxEraseGroupSize.X, Dispatch_SetupDimArray3.X),
                                        CoreDefine.Roundup(VxEraseGroupSize.Y, Dispatch_SetupDimArray3.Y),
                                        CoreDefine.Roundup(VxEraseGroupSize.Z, Dispatch_SetupDimArray3.Z));
                                    EraseVoxelGroupDrawcall.Commit(cmd);
                                    VxEraseGroupSize = UInt32_3.Zero;
                                }
                                #endregion

                                #region inject voxels
                                {
                                    var srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        InjectVoxelsDrawcall.BindUav(srvIdx, GetAttachBuffer(GpuScenePinIn).Uav);
                                    }
                                    srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferAbedo");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        InjectVoxelsDrawcall.BindSrv(srvIdx, GetAttachBuffer(AlbedoPinIn).Srv);
                                    }
                                    srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferShadowMask");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        InjectVoxelsDrawcall.BindSrv(srvIdx, GetAttachBuffer(ShadowMaskPinIn).Srv);
                                    }
                                    srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferDepth");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        InjectVoxelsDrawcall.BindSrv(srvIdx, GetAttachBuffer(DepthPinIn).Srv);
                                    }

                                    InjectVoxelsDrawcall.SetDispatch(
                                        CoreDefine.Roundup(DiffuseRTWidth, Dispatch_SetupDimArray2.X),
                                        CoreDefine.Roundup(DiffuseRTHeight, Dispatch_SetupDimArray2.Y),
                                        1);
                                    InjectVoxelsDrawcall.Commit(cmd);
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
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(BasePass.DrawCmdList);
            }   
        }
        
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
