using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UScreenTilingNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public Common.URenderGraphPin DepthPinIn = Common.URenderGraphPin.CreateInput("Depth");
        public Common.URenderGraphPin PointLightsPinIn = Common.URenderGraphPin.CreateInput("PointLights");
        public Common.URenderGraphPin TilingPinOut = Common.URenderGraphPin.CreateOutput("Tiling", false, EPixelFormat.PXF_UNKNOWN);
        public UScreenTilingNode()
        {
            Name = "ScreenTilingNode";
        }
        
        public override void InitNodePins()
        {
            AddInput(DepthPinIn, NxRHI.EBufferType.BFT_DSV | NxRHI.EBufferType.BFT_SRV);
            AddInput(PointLightsPinIn, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            TilingPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(TilingPinOut, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
        }
        public unsafe override void FrameBuild(Graphics.Pipeline.URenderPolicy policy)
        {
            TilingPinOut.Attachement.Height = TileX * TileY;
            TilingPinOut.Attachement.Width = (uint)sizeof(FTileData);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(TilingPinOut);
            attachement.Buffer = TileBuffer;
            attachement.Srv = TileSRV;
            attachement.Uav = TileUAV;
        }
        public readonly Vector3ui Dispatch_SetupDimArray2 = new Vector3ui(32, 32, 1);
        public readonly Vector3ui Dispatch_PushPointLights = new Vector3ui(32, 32, 1);
        //这里要注意检查一下GL,Metal,VK的固定数组对齐模式
        //对于cbuffer，数组肯定是每个元素都必须是16 bytes对齐
        //但是这里是RWStructureBuffer UAV，在dx上看不是每个元素都要求16字节对齐，而是真的一个uint数组
        //如果其他RHI也没问题，那么这样可以减少CS中下标操作内存的计算开销，虽然uint比ushort大一些内存
        public const uint MaxNumOfPointLight = 32;//Same with GpuSceneCommon.cginc

        public uint TileSize = 32;//Same with GpuSceneCommon.cginc
        public uint TileX;
        public uint TileY;

        public unsafe struct FTileData
        {
            public Vector3 BoxMin;
            public uint Pad0;
            public Vector3 BoxMax;
            public uint NumPointLight;
            //[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = NumOfPointLightDataArray)]
            //public fixed uint size[(int)MaxNumOfPointLight / 2];
            public fixed uint size[(int)MaxNumOfPointLight];
        }

        public NxRHI.UBuffer TileBuffer;
        public NxRHI.UUaView TileUAV;
        public NxRHI.USrView TileSRV;

        private NxRHI.UComputeEffect SetupTileData;
        private NxRHI.UComputeDraw SetupTileDataDrawcall;

        private NxRHI.UComputeEffect PushLightToTileData;
        private NxRHI.UComputeDraw PushLightToTileDataDrawcall;

        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName + ".BasePass");

            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.Z}");

            SetupTileData = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/ScreenSpace/Tiling.compute", RName.ERNameType.Engine),
                "CS_SetupTileData", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);
            
            PushLightToTileData = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Compute/ScreenSpace/Tiling.compute", RName.ERNameType.Engine),
                "CS_PushLightToTileData", NxRHI.EShaderType.SDT_ComputeShader, null, defines, null);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (SetupTileData == null)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var gpuScene = policy.GetGpuSceneNode();// .FindNode("GpuSceneNode") as Common.UGpuSceneNode;
            var ConfigCBuffer = policy.GetGpuSceneNode().PerGpuSceneCBuffer;

            CoreSDK.DisposeObject(ref SetupTileDataDrawcall);
            SetupTileDataDrawcall = rc.CreateComputeDraw();
            SetupTileDataDrawcall.SetComputeEffect(SetupTileData);
            var srvIdx = SetupTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx.IsValidPointer)
            {
                SetupTileDataDrawcall.BindCBuffer(srvIdx, ConfigCBuffer);
            }
            srvIdx = SetupTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx.IsValidPointer)
            {
                var camera = policy.DefaultCamera;
                SetupTileDataDrawcall.BindCBuffer(srvIdx, camera.PerCameraCBuffer);
            }
            srvIdx = SetupTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "DstBuffer");
            if (srvIdx.IsValidPointer)
            {
                SetupTileDataDrawcall.BindUav(srvIdx, TileUAV);
            }
            SetupTileDataDrawcall.mCoreObject.SetDispatch(MathHelper.Roundup(TileX, Dispatch_SetupDimArray2.X), MathHelper.Roundup(TileY, Dispatch_SetupDimArray2.Y), 1);

            CoreSDK.DisposeObject(ref PushLightToTileDataDrawcall);
            PushLightToTileDataDrawcall = rc.CreateComputeDraw();
            PushLightToTileDataDrawcall.SetComputeEffect(PushLightToTileData);
            srvIdx = PushLightToTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx.IsValidPointer)
            {
                PushLightToTileDataDrawcall.BindCBuffer(srvIdx, ConfigCBuffer);
            }            
            srvIdx = PushLightToTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_UAV, "DstBuffer");
            if (srvIdx.IsValidPointer)
            {
                PushLightToTileDataDrawcall.BindUav(srvIdx, TileUAV);
            }
            PushLightToTileDataDrawcall.SetDispatch(MathHelper.Roundup(TileX, Dispatch_SetupDimArray2.X), MathHelper.Roundup(TileY, Dispatch_SetupDimArray2.Y), 1);
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref TileUAV);

            CoreSDK.DisposeObject(ref TileSRV);

            CoreSDK.DisposeObject(ref TileBuffer);

            CoreSDK.DisposeObject(ref SetupTileDataDrawcall);
            CoreSDK.DisposeObject(ref PushLightToTileDataDrawcall);

            base.Dispose();
        }
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {
            TileX = MathHelper.Roundup((uint)x, TileSize);
            TileY = MathHelper.Roundup((uint)y, TileSize);

            TileUAV?.Dispose();
            TileUAV = null;

            TileBuffer?.Dispose();
            TileBuffer = null;

            TileSRV?.Dispose();
            TileSRV = null;

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var desc = new NxRHI.FBufferDesc();
            desc.SetDefault(false);
            desc.Type = NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV;
            desc.Size = TileX * TileY * (uint)sizeof(FTileData);
            desc.StructureStride = (uint)sizeof(FTileData);
            TileBuffer = rc.CreateBuffer(in desc);
            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(0);
            uavDesc.Buffer.NumElements = (uint)TileX * TileY;
            uavDesc.Buffer.StructureByteStride = desc.StructureStride;
            TileUAV = rc.CreateUAV(TileBuffer, in uavDesc);
            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetBuffer(0);
            srvDesc.Buffer.NumElements = (uint)TileX * TileY;
            srvDesc.Buffer.StructureByteStride = (uint)sizeof(FTileData);
            TileSRV = rc.CreateSRV(TileBuffer, in srvDesc);

            ResetComputeDrawcall(policy);
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UScreenTilingNode), nameof(TickLogic));
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (TileX == 0 || TileY == 0)
                    return;
                var gpuScene = policy.GetGpuSceneNode();// .FindNode("GpuSceneNode") as Common.UGpuSceneNode;

                var cmd = BasePass.DrawCmdList;
                cmd.BeginCommand();

                var ConfigCBuffer = policy.GetGpuSceneNode().PerGpuSceneCBuffer;
                if (ConfigCBuffer != null)
                {
                    var idx = ConfigCBuffer.ShaderBinder.FindField("LightNum");
                    if (gpuScene != null)
                    {
                        var LightNum = gpuScene.PointLights.DataArray.Count;
                        ConfigCBuffer.SetValue(idx, in LightNum);
                    }
                    Vector2ui tile;
                    tile.X = TileX;
                    tile.Y = TileY;
                    idx = ConfigCBuffer.ShaderBinder.FindField("TileNum");
                    ConfigCBuffer.SetValue(idx, in tile);

                    //ConfigCBuffer.FlushDirty(false);
                }

                #region Setup
                {
                    var srvIdx = SetupTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "DepthBuffer");
                    if (srvIdx.IsValidPointer)
                    {
                        var depth = this.GetAttachBuffer(DepthPinIn);
                        SetupTileDataDrawcall.BindSrv(srvIdx, depth.Srv);
                    }
                    //SetupTileDataDrawcall.Commit(cmd);
                    cmd.PushGpuDraw(SetupTileDataDrawcall);
                }
                #endregion

                #region PushLights
                {
                    var srvIdx = PushLightToTileDataDrawcall.FindBinder(NxRHI.EShaderBindType.SBT_SRV, "GpuScene_PointLights");
                    if (srvIdx.IsValidPointer)
                    {
                        var attachBuffer = this.GetAttachBuffer(PointLightsPinIn);
                        if (attachBuffer.Srv != null)
                        {
                            PushLightToTileDataDrawcall.BindSrv(srvIdx, attachBuffer.Srv);
                            //PushLightToTileDataDrawcall.Commit(cmd);
                            cmd.PushGpuDraw(PushLightToTileDataDrawcall);
                        }
                    }

                }
                #endregion

                cmd.FlushDraws();
                cmd.EndCommand();
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmdlist(cmd);
            }   
        }
    }
}
