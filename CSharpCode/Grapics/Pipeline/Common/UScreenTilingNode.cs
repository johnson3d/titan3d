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
            AddInput(DepthPinIn, EGpuBufferViewType.GBVT_Dsv | EGpuBufferViewType.GBVT_Srv);
            AddInput(PointLightsPinIn, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Uav);
            TilingPinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(TilingPinOut, EGpuBufferViewType.GBVT_Uav | EGpuBufferViewType.GBVT_Srv);
        }
        public unsafe override void FrameBuild()
        {
            TilingPinOut.Attachement.Height = TileX * TileY;
            TilingPinOut.Attachement.Width = (uint)sizeof(FTileData);

            var attachement = RenderGraph.AttachmentCache.ImportAttachment(TilingPinOut);
            attachement.Buffer = TileBuffer;
            attachement.Srv = TileSRV;
            attachement.Uav = TileUAV;
        }
        public readonly UInt32_3 Dispatch_SetupDimArray2 = new UInt32_3(32, 32, 1);
        public readonly UInt32_3 Dispatch_PushPointLights = new UInt32_3(32, 32, 1);
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

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        
        public RHI.CGpuBuffer TileBuffer;
        public RHI.CUnorderedAccessView TileUAV;
        public RHI.CShaderResourceView TileSRV;

        private RHI.CShaderDesc CSDesc_SetupTileData;
        private RHI.CComputeShader CS_SetupTileData;
        private RHI.CComputeDrawcall SetupTileDataDrawcall;

        private RHI.CShaderDesc CSDesc_PushLightToTileData;
        private RHI.CComputeShader CS_PushLightToTileData;
        private RHI.CComputeDrawcall PushLightToTileDataDrawcall;

        public async override System.Threading.Tasks.Task Initialize(URenderPolicy policy, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            BasePass.Initialize(rc, debugName);

            var defines = new RHI.CShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray2.x}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray2.y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray2.z}");

            CSDesc_SetupTileData = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/ScreenSpace/Tiling.compute", RName.ERNameType.Engine),
                "CS_SetupTileData", EShaderType.EST_ComputeShader, defines, null);
            CS_SetupTileData = rc.CreateComputeShader(CSDesc_SetupTileData);

            CSDesc_PushLightToTileData = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/ScreenSpace/Tiling.compute", RName.ERNameType.Engine),
                "CS_PushLightToTileData", EShaderType.EST_ComputeShader, defines, null);
            CS_PushLightToTileData = rc.CreateComputeShader(CSDesc_PushLightToTileData);
        }
        private unsafe void ResetComputeDrawcall(URenderPolicy policy)
        {
            if (CS_SetupTileData == null)
                return;
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var gpuScene = policy.GetGpuSceneNode();// .FindNode("GpuSceneNode") as Common.UGpuSceneNode;
            var ConfigCBuffer = policy.GetGpuSceneNode().PerGpuSceneCBuffer;

            SetupTileDataDrawcall = rc.CreateComputeDrawcall();
            SetupTileDataDrawcall.mCoreObject.SetComputeShader(CS_SetupTileData.mCoreObject);
            var srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupTileDataDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, ConfigCBuffer.mCoreObject);
            }
            srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
            if (srvIdx != (IShaderBinder*)0)
            {
                var camera = policy.DefaultCamera;
                SetupTileDataDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
            }
            srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                SetupTileDataDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, TileUAV.mCoreObject);
            }
            SetupTileDataDrawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(TileX, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(TileY, Dispatch_SetupDimArray2.y), 1);

            PushLightToTileDataDrawcall = rc.CreateComputeDrawcall();
            PushLightToTileDataDrawcall.mCoreObject.SetComputeShader(CS_PushLightToTileData.mCoreObject);
            srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
            if (srvIdx != (IShaderBinder*)0)
            {
                PushLightToTileDataDrawcall.mCoreObject.GetCBufferResources().BindCS(srvIdx->m_CSBindPoint, ConfigCBuffer.mCoreObject);
            }            
            srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
            if (srvIdx != (IShaderBinder*)0)
            {
                PushLightToTileDataDrawcall.mCoreObject.GetUavResources().BindCS(srvIdx->m_CSBindPoint, TileUAV.mCoreObject);
            }
            PushLightToTileDataDrawcall.mCoreObject.SetDispatch(CoreDefine.Roundup(TileX, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(TileY, Dispatch_SetupDimArray2.y), 1);
        }
        public override void Cleanup()
        {
            TileUAV?.Dispose();
            TileUAV = null;

            TileSRV?.Dispose();
            TileSRV = null;

            TileBuffer?.Dispose();
            TileBuffer = null;

            base.Cleanup();
        }
        public override unsafe void OnResize(URenderPolicy policy, float x, float y)
        {
            TileX = CoreDefine.Roundup((uint)x, TileSize);
            TileY = CoreDefine.Roundup((uint)y, TileSize);

            TileUAV?.Dispose();
            TileUAV = null;

            TileBuffer?.Dispose();
            TileBuffer = null;

            TileSRV?.Dispose();
            TileSRV = null;

            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var desc = new IGpuBufferDesc();
            desc.SetMode(false, true);
            desc.m_ByteWidth = TileX * TileY * (uint)sizeof(FTileData);
            desc.m_StructureByteStride = (uint)sizeof(FTileData);
            TileBuffer = rc.CreateGpuBuffer(in desc, IntPtr.Zero);
            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.SetBuffer();
            uavDesc.Buffer.NumElements = (uint)TileX * TileY;
            TileUAV = rc.CreateUnorderedAccessView(TileBuffer, in uavDesc);
            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetBuffer();
            srvDesc.Buffer.NumElements = (uint)TileX * TileY;
            srvDesc.mGpuBuffer = TileBuffer.mCoreObject;
            TileSRV = rc.CreateShaderResourceView(in srvDesc);

            ResetComputeDrawcall(policy);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, bool bClear)
        {
            if (TileX == 0 || TileY == 0)
                return;
            var gpuScene = policy.GetGpuSceneNode();// .FindNode("GpuSceneNode") as Common.UGpuSceneNode;

            var cmd = BasePass.DrawCmdList;

            var ConfigCBuffer = policy.GetGpuSceneNode().PerGpuSceneCBuffer;
            if (ConfigCBuffer != null)
            {
                var idx = ConfigCBuffer.mCoreObject.FindVar("LightNum");                
                if (gpuScene != null)
                {
                    var LightNum = gpuScene.PointLights.DataArray.Count;
                    ConfigCBuffer.SetValue(idx, in LightNum);
                }
                UInt32_2 tile;
                tile.x = TileX;
                tile.y = TileY;
                idx = ConfigCBuffer.mCoreObject.FindVar("TileNum");
                ConfigCBuffer.SetValue(idx, in tile);

                ConfigCBuffer.mCoreObject.UpdateDrawPass(cmd.mCoreObject, 1);
            }
            
            #region Setup
            {
                var srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
                if (srvIdx != (IShaderBinder*)0)
                {
                    var depth = this.GetAttachBuffer(DepthPinIn);
                    SetupTileDataDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, depth.Srv.mCoreObject);
                }
                SetupTileDataDrawcall.BuildPass(cmd);
                //cmd.SetComputeShader(CS_SetupTileData.mCoreObject);
                //var srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, ConfigCBuffer.mCoreObject);
                //}
                //srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerCamera");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    var camera = policy.GetBasePassNode().GBuffers.Camera;
                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, camera.PerCameraCBuffer.mCoreObject);
                //}
                //srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, TileUAV.mCoreObject, &nUavInitialCounts);
                //}
                //srvIdx = CSDesc_SetupTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "DepthBuffer");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    var depth = policy.GetBasePassNode().GBuffers.DepthStencilSRV;
                //    cmd.CSSetShaderResource(srvIdx->CSBindPoint, depth.mCoreObject);
                //}
                //cmd.CSDispatch(CoreDefine.Roundup(TileX, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(TileY, Dispatch_SetupDimArray2.y), 1);
            }
            #endregion

            #region PushLights
            {
                var srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
                if (srvIdx != (IShaderBinder*)0)
                {
                    var attachBuffer = this.GetAttachBuffer(PointLightsPinIn);
                    if (attachBuffer.Srv != null)
                        PushLightToTileDataDrawcall.mCoreObject.GetShaderRViewResources().BindCS(srvIdx->CSBindPoint, attachBuffer.Srv.mCoreObject);
                }
                PushLightToTileDataDrawcall.BuildPass(cmd);
                //cmd.SetComputeShader(CS_PushLightToTileData.mCoreObject);
                //var srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, "cbPerGpuScene");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetConstantBuffer(srvIdx->m_CSBindPoint, ConfigCBuffer.mCoreObject);
                //}
                //if (gpuScene != null)
                //{
                //    srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, "GpuScene_PointLights");
                //    if (srvIdx != (IShaderBinder*)0)
                //    {
                //        cmd.CSSetShaderResource(srvIdx->CSBindPoint, gpuScene.PointLights.DataSRV.mCoreObject);
                //    }
                //}
                //srvIdx = CSDesc_PushLightToTileData.mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, "DstBuffer");
                //if (srvIdx != (IShaderBinder*)0)
                //{
                //    cmd.CSSetUnorderedAccessView(srvIdx->m_CSBindPoint, TileUAV.mCoreObject, &nUavInitialCounts);
                //}
                //cmd.CSDispatch(CoreDefine.Roundup(TileX, Dispatch_SetupDimArray2.x), CoreDefine.Roundup(TileY, Dispatch_SetupDimArray2.y), 1);
            }
            #endregion

            if (cmd.BeginCommand())
            {
                cmd.EndCommand();
            }
        }
        public unsafe override void TickRender(Graphics.Pipeline.URenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe override void TickSync(Graphics.Pipeline.URenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
