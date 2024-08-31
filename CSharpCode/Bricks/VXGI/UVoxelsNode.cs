using EngineNS.EGui.Slate;
using EngineNS.NxRHI;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public partial class UVoxelsNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public Graphics.Pipeline.TtRenderGraphPin GpuScenePinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("GpuScene");
        public Graphics.Pipeline.TtRenderGraphPin AlbedoPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Albedo");
        public Graphics.Pipeline.TtRenderGraphPin DepthPinInOut = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Depth");

        public Graphics.Pipeline.TtRenderGraphPin VxPoolPinOut = Graphics.Pipeline.TtRenderGraphPin.CreateOutput("VxPool", false, EPixelFormat.PXF_UNKNOWN);

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
            
            VxPoolPinOut.LifeMode = Graphics.Pipeline.TtAttachBuffer.ELifeMode.Imported;
            AddOutput(VxPoolPinOut, NxRHI.EBufferType.BFT_UAV);
        }

        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(VxPoolPinOut);
            attachement.GpuResource = VoxelPool;
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
        public static readonly Vector3ui Dispatch_SetupDimArray1 = new Vector3ui(128,1,1);
        public static readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);
        public static readonly Vector3ui Dispatch_SetupDimArray3 = new Vector3ui(8, 8, 4);
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
        public class SetupVoxelGroupAllocatorShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(Dispatch_SetupDimArray1.X, 1, 1);
            }
            public SetupVoxelGroupAllocatorShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine);
                MainName = "CS_SetupVoxelGroupAllocator";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
                defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
                defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
                defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
                defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
                defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UVoxelsNode;

                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.UavVoxelPool);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                if (srvIdx.IsValidPointer)
                {
                    var attachBuffer = node.GetAttachBuffer(node.GpuScenePinInOut);
                    drawcall.BindUav(srvIdx, attachBuffer.Uav);
                }
            }
        }
        private SetupVoxelGroupAllocatorShading SetupVoxelGroupAllocator;
        private NxRHI.UComputeDraw SetupVoxelGroupAllocatorDrawcall;

        public class InjectVoxelsShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(Dispatch_SetupDimArray2.X, Dispatch_SetupDimArray2.Y, 1);
            }
            public InjectVoxelsShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/VXGI/VoxelInject.compute", RName.ERNameType.Engine);
                MainName = "CS_InjectVoxels";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
                defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
                defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
                defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
                defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
                defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UVoxelsNode;

                var cbIndex = CurrentEffect.FindBinder(TtNameTable.cbGBufferDesc);
                if (node.CBuffer == null)
                {
                    node.CBuffer = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(cbIndex);
                }

                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, node.CBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerCamera");
                if (srvIdx.IsValidPointer)
                {
                    var camera = policy.DefaultCamera;
                    drawcall.BindCBuffer(srvIdx, camera.PerCameraCBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.UavVoxelPool);
                }

                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.GetAttachBuffer(node.GpuScenePinInOut).Uav);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferAbedo");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindSrv(srvIdx, node.GetAttachBuffer(node.AlbedoPinInOut).Srv);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GBufferDepth");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindSrv(srvIdx, node.GetAttachBuffer(node.DepthPinInOut).Srv);
                }
            }
        }
        private InjectVoxelsShading InjectVoxels;
        private NxRHI.UComputeDraw InjectVoxelsDrawcall;

        public class EraseVoxelGroupShading : Graphics.Pipeline.Shader.TtComputeShadingEnv
        {
            public override Vector3ui DispatchArg
            {
                get => new Vector3ui(Dispatch_SetupDimArray3.X, Dispatch_SetupDimArray3.Y, Dispatch_SetupDimArray3.Z);
            }
            public EraseVoxelGroupShading()
            {
                CodeName = RName.GetRName("Shaders/Bricks/VXGI/VoxelInject.compute", RName.ERNameType.Engine);
                MainName = "CS_InjectVoxels";

                this.UpdatePermutation();
            }
            protected override void EnvShadingDefines(in FPermutationId id, NxRHI.UShaderDefinitions defines)
            {
                base.EnvShadingDefines(in id, defines);
                defines.mCoreObject.AddDefine("VxSize", $"{VxSize}");
                defines.mCoreObject.AddDefine("VxGroupPoolSize", $"{VxGroupPoolSize}");
                defines.mCoreObject.AddDefine("VxGroupCubeSide", $"{VxGroupCubeSide}");
                defines.mCoreObject.AddDefine("VxSceneX", $"{VxSceneX}");
                defines.mCoreObject.AddDefine("VxSceneY", $"{VxSceneY}");
                defines.mCoreObject.AddDefine("VxSceneZ", $"{VxSceneZ}");
            }
            public override void OnDrawCall(NxRHI.UComputeDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy)
            {
                var node = drawcall.TagObject as UVoxelsNode;

                var srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbGBufferDesc");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindCBuffer(srvIdx, node.CBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerCamera");
                if (srvIdx.IsValidPointer)
                {
                    var camera = policy.DefaultCamera;
                    drawcall.BindCBuffer(srvIdx, camera.PerCameraCBuffer);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "VxGroupPool");
                if (srvIdx.IsValidPointer)
                {
                    drawcall.BindUav(srvIdx, node.UavVoxelPool);
                }
                srvIdx = drawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "GpuSceneDesc");
                if (srvIdx.IsValidPointer)
                {
                    var attachBuffer = node.GetAttachBuffer(node.GpuScenePinInOut);
                    drawcall.BindUav(srvIdx, attachBuffer.Uav);
                }
            }
        }
        private EraseVoxelGroupShading EraseVoxelGroup;
        private NxRHI.UComputeDraw EraseVoxelGroupDrawcall;
        #endregion

        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.TtRenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
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

            SetupVoxelGroupAllocator = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<SetupVoxelGroupAllocatorShading>();
            
            EraseVoxelGroup = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EraseVoxelGroupShading>();

            InjectVoxels = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<InjectVoxelsShading>();
            
            if (true)
            {
                var material = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
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
        private unsafe void ResetComputeDrawcall(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            CoreSDK.DisposeObject(ref SetupVoxelGroupAllocatorDrawcall);
            SetupVoxelGroupAllocatorDrawcall = rc.CreateComputeDraw();
            
            CoreSDK.DisposeObject(ref EraseVoxelGroupDrawcall);
            EraseVoxelGroupDrawcall = rc.CreateComputeDraw();
            
            
            CoreSDK.DisposeObject(ref InjectVoxelsDrawcall);
            InjectVoxelsDrawcall = rc.CreateComputeDraw();
            
        }
        public override void OnResize(Graphics.Pipeline.TtRenderPolicy policy, float x, float y)
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
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(UVoxelsNode), nameof(TickLogic));
                return mScopeTick;
            }
        } 
        public override unsafe void TickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, bool bClear)
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
                                    SetupVoxelGroupAllocator.SetDrawcallDispatch(this, policy, SetupVoxelGroupAllocatorDrawcall, VxGroupPoolSize, 1, 1, true);
                                    cmd.PushGpuDraw(SetupVoxelGroupAllocatorDrawcall);

                                    cmd.BeginEvent(Name + "Setup");
                                    cmd.FlushDraws();
                                    cmd.EndEvent();
                                }
                                policy.CommitCommandList(cmd);
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
                                        EraseVoxelGroup.SetDrawcallDispatch(this, policy, EraseVoxelGroupDrawcall, 
                                            VxEraseGroupSize.X,
                                            VxEraseGroupSize.Y,
                                            VxEraseGroupSize.Z,
                                            true);
                                        cmd.PushGpuDraw(EraseVoxelGroupDrawcall);
                                        VxEraseGroupSize = Vector3ui.Zero;
                                    }
                                    #endregion

                                    #region inject voxels
                                    {
                                        InjectVoxels.SetDrawcallDispatch(this, policy, InjectVoxelsDrawcall,
                                            DiffuseRTWidth,
                                            DiffuseRTHeight,
                                            1,
                                            true);
                                        //InjectVoxelsDrawcall.Commit(cmd);
                                        cmd.PushGpuDraw(InjectVoxelsDrawcall);
                                    }
                                    #endregion

                                    TickVxDebugger(world, policy);

                                    cmd.FlushDraws();
                                }
                                policy.CommitCommandList(cmd);
                            }
                        }
                        break;
                }
            }   
        }
    }
}
