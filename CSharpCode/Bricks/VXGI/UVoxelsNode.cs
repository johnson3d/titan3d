using EngineNS.EGui.Slate;
using EngineNS.NxRHI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public partial class UVoxelsNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Graphics.Pipeline.Common.URenderGraphPin GpuScenePinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("GpuScene");
        public Graphics.Pipeline.Common.URenderGraphPin AlbedoPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Albedo");
        public Graphics.Pipeline.Common.URenderGraphPin DepthPinInOut = Graphics.Pipeline.Common.URenderGraphPin.CreateInputOutput("Depth");

        public Graphics.Pipeline.Common.URenderGraphPin VxPoolPinOut = Graphics.Pipeline.Common.URenderGraphPin.CreateOutput("VxPool", false, EPixelFormat.PXF_UNKNOWN);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        unsafe struct FVoxelGroup
        {
            public Vector3i GroupIndex;
            public uint KeyState;
            public fixed uint Value[4*4*4];
	        //void SetVoxel(Vector3i offset, uint color)
         //   {
         //       Value[offset.z][offset.y][offset.x] = color;
         //   }
         //   uint GetVoxel(Vector3i offset)
         //   {
         //       return Value[offset.z][offset.y][offset.x];
         //   }
        }

        public UVoxelsNode()
        {
            Name = "UVoxelsNode";
        }
        public override void InitNodePins()
        {
            AddInputOutput(GpuScenePinInOut, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

            AddInputOutput(AlbedoPinInOut, NxRHI.EBufferType.BFT_SRV);
            AddInputOutput(DepthPinInOut, NxRHI.EBufferType.BFT_SRV);
            
            VxPoolPinOut.LifeMode = Graphics.Pipeline.UAttachBuffer.ELifeMode.Imported;
            AddOutput(VxPoolPinOut, NxRHI.EBufferType.BFT_UAV);
        }

        public override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(VxPoolPinOut);
            attachement.Buffer = VoxelPool;
            attachement.Uav = UavVoxelPool;
        }
        #region ConstantVar
        public const uint VxGroupCubeSide = 4;
        public const float VxSize = 0.25f;
        public const float VxGroupSize = VxSize * (float)VxGroupCubeSide;
        public const uint VxGroupPoolSize = 64 * 64 * 32;
        public const uint VxSceneX = 256;
        public const uint VxSceneZ = 256;
        public const uint VxSceneY = 64;
        public readonly Vector3ui Dispatch_SetupDimArray1 = new Vector3ui(128,1,1);
        public readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);
        public readonly Vector3ui Dispatch_SetupDimArray3 = new Vector3ui(8, 8, 4);
        #endregion
        public override void Dispose()
        {
            UavVoxelPool = null;

            CoreSDK.DisposeObject(ref VoxelPool);
            CoreSDK.DisposeObject(ref VxDebugMesh);
            CoreSDK.DisposeObject(ref SetupVxDebuggerDrawcall);
            CoreSDK.DisposeObject(ref CollectVxDebuggerDrawcall);
            CoreSDK.DisposeObject(ref VoxelGroupDebugger);
            CoreSDK.DisposeObject(ref VoxelDebugger);
            CoreSDK.DisposeObject(ref VxIndirectDebugDraws);
            CoreSDK.DisposeObject(ref UavVoxelGroupDebugger);
            CoreSDK.DisposeObject(ref UavVoxelDebugger);
            CoreSDK.DisposeObject(ref SrvVoxelDebugger);
            CoreSDK.DisposeObject(ref UavVxIndirectDebugDraws);

            //UavAbedo?.Dispose();
            //UavAbedo = null;
            base.Dispose();
        }
        
        #region VoxelInject        
        
        public NxRHI.UCbView CBuffer;

        public NxRHI.UBuffer VoxelPool;//(group = VxGroupCubeSide*VxGroupCubeSide*VxGroupCubeSide) * VxGroupPoolSize;
        public NxRHI.UUaView UavVoxelPool;
        
        public uint DiffuseRTWidth;
        public uint DiffuseRTHeight;
        //public NxRHI.UUaView UavAbedo;
        //public NxRHI.UUaView UavNormal;

        private NxRHI.UComputeEffect SetupVoxelGroupAllocator;
        private NxRHI.UComputeDraw SetupVoxelGroupAllocatorDrawcall;

        private NxRHI.UComputeEffect InjectVoxels;
        private NxRHI.UComputeDraw InjectVoxelsDrawcall;

        private NxRHI.UComputeEffect EraseVoxelGroup;
        private NxRHI.UComputeDraw EraseVoxelGroupDrawcall;
        #endregion

        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault(false, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
            desc.Type = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;            
            unsafe
            {
                desc.Size = VxGroupPoolSize * (uint)sizeof(FVoxelGroup);
                desc.StructureStride = (uint)sizeof(FVoxelGroup);
            }
            VoxelPool = rc.CreateBuffer(in desc);
            var uavDesc = new FUavDesc();
            uavDesc.SetBuffer(false);
            uavDesc.Format = EPixelFormat.PXF_UNKNOWN;
            uavDesc.Buffer.NumElements = (uint)(VxGroupPoolSize);
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            UavVoxelPool = rc.CreateUAV(VoxelPool, in uavDesc);
            System.Diagnostics.Debug.Assert(UavVoxelPool != null);

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
            defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}"); 
            defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}"); 
            defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
            defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
            defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"1");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            SetupVoxelGroupAllocator = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine),
                "CS_SetupVoxelGroupAllocator", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"1");
            InjectVoxels = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/VXGI/VoxelInject.compute", RName.ERNameType.Engine),
                "CS_InjectVoxels", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray3.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray3.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray3.Z}");
            EraseVoxelGroup = await UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/VXGI/VoxelInject.compute", RName.ERNameType.Engine),
                "CS_EraseVoxelGroup", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);

            var cbIndex = InjectVoxels.FindBinder(VNameString.FromString("cbGBufferDesc"));
            CBuffer = rc.CreateCBV(cbIndex);

            if (true)
            {
                var material = await UEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
                material.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_Translucent;
                //var rast = material.Rasterizer;
                //rast.FillMode = EFillMode.FMD_WIREFRAME;
                //material.Rasterizer = rast;
                await InitVxDebugger(material);
            }

            BasePass.Initialize(rc, debugName + ".BasePass");

            mCurStep = EStep.Setup;

            //ResetComputeDrawcall(policy);
        }
        private unsafe void ResetComputeDrawcall(Graphics.Pipeline.URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            CoreSDK.DisposeObject(ref SetupVoxelGroupAllocatorDrawcall);
            SetupVoxelGroupAllocatorDrawcall = rc.CreateComputeDraw();
            SetupVoxelGroupAllocatorDrawcall.SetComputeEffect(SetupVoxelGroupAllocator);
            SetupVoxelGroupAllocatorDrawcall.SetDispatch(MathHelper.Roundup(VxGroupPoolSize, Dispatch_SetupDimArray1.X), 1, 1);
            var srvIdx = SetupVoxelGroupAllocatorDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
            if (srvIdx.IsValidPointer)
            {
                SetupVoxelGroupAllocatorDrawcall.BindUav(srvIdx, UavVoxelPool);
            }

            CoreSDK.DisposeObject(ref EraseVoxelGroupDrawcall);
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
            srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
            if (srvIdx.IsValidPointer)
            {
                EraseVoxelGroupDrawcall.BindUav(srvIdx, UavVoxelPool);
            }

            CoreSDK.DisposeObject(ref InjectVoxelsDrawcall);
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
            srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
            if (srvIdx.IsValidPointer)
            {
                InjectVoxelsDrawcall.BindUav(srvIdx, UavVoxelPool);
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
        Vector3ui EraseVxStart = new Vector3ui(0, 0, 0);
        Vector3ui VxEraseGroupSize = new Vector3ui(0, 0, 0);
        public void SetEraseBox(in BoundingBox box)
        {
            BoundingBox oBox;
            BoundingBox.And(in VxSceneBox, in box, out oBox);
            if (oBox.IsEmpty())
            {
                VxEraseGroupSize = new Vector3ui(0, 0, 0);
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
                        var meshAtomDesc = VxDebugMesh.SubMeshes[0].Atoms[0].GetMeshAtomDesc(0);
                        var VxDebugger_IndexCountPerInstance = meshAtomDesc->NumPrimitives * 3;
                        CBuffer.SetValue(idx, in VxDebugger_IndexCountPerInstance);
                    }
                }
                switch (mCurStep)
                {
                    case EStep.Setup:
                        {
                            if (SetupVoxelGroupAllocator != null)
                            {
                                var cmd = BasePass.DrawCmdList;

                                using (new NxRHI.TtCmdListScope(cmd))
                                {
                                    var srvIdx = SetupVoxelGroupAllocatorDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                    if (srvIdx.IsValidPointer)
                                    {
                                        var attachBuffer = GetAttachBuffer(GpuScenePinInOut);
                                        SetupVoxelGroupAllocatorDrawcall.BindUav(srvIdx, attachBuffer.Uav);
                                    }

                                    //SetupVoxelGroupAllocatorDrawcall.Commit(cmd);
                                    cmd.PushGpuDraw(SetupVoxelGroupAllocatorDrawcall);

                                    cmd.FlushDraws();
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

                                using (new NxRHI.TtCmdListScope(cmd))
                                {
                                    #region erase voxelgroups
                                    if (VxEraseGroupSize != Vector3ui.Zero)
                                    {
                                        var srvIdx = EraseVoxelGroupDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                        if (srvIdx.IsValidPointer)
                                        {
                                            var attachBuffer = GetAttachBuffer(GpuScenePinInOut);
                                            EraseVoxelGroupDrawcall.BindUav(srvIdx, attachBuffer.Uav);
                                        }

                                        EraseVoxelGroupDrawcall.SetDispatch(
                                            MathHelper.Roundup(VxEraseGroupSize.X, Dispatch_SetupDimArray3.X),
                                            MathHelper.Roundup(VxEraseGroupSize.Y, Dispatch_SetupDimArray3.Y),
                                            MathHelper.Roundup(VxEraseGroupSize.Z, Dispatch_SetupDimArray3.Z));
                                        //EraseVoxelGroupDrawcall.Commit(cmd);
                                        cmd.PushGpuDraw(EraseVoxelGroupDrawcall);
                                        VxEraseGroupSize = Vector3ui.Zero;
                                    }
                                    #endregion

                                    #region inject voxels
                                    {
                                        var srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                                        if (srvIdx.IsValidPointer)
                                        {
                                            InjectVoxelsDrawcall.BindUav(srvIdx, GetAttachBuffer(GpuScenePinInOut).Uav);
                                        }
                                        srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferAbedo");
                                        if (srvIdx.IsValidPointer)
                                        {
                                            InjectVoxelsDrawcall.BindSrv(srvIdx, GetAttachBuffer(AlbedoPinInOut).Srv);
                                        }
                                        srvIdx = InjectVoxelsDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferDepth");
                                        if (srvIdx.IsValidPointer)
                                        {
                                            InjectVoxelsDrawcall.BindSrv(srvIdx, GetAttachBuffer(DepthPinInOut).Srv);
                                        }

                                        InjectVoxelsDrawcall.SetDispatch(
                                            MathHelper.Roundup(DiffuseRTWidth, Dispatch_SetupDimArray2.X),
                                            MathHelper.Roundup(DiffuseRTHeight, Dispatch_SetupDimArray2.Y),
                                            1);
                                        //InjectVoxelsDrawcall.Commit(cmd);
                                        cmd.PushGpuDraw(InjectVoxelsDrawcall);
                                    }
                                    #endregion

                                    TickVxDebugger(world);

                                    cmd.FlushDraws();
                                }
                            }
                        }
                        break;
                }
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(BasePass.DrawCmdList);
            }   
        }
    }
}
