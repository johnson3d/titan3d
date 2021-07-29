using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.VXGI
{
    public class UVoxelsNode : Graphics.Pipeline.Common.URenderGraphNode
    {
        public void Cleanup()
        {
            VoxelPool?.Dispose();
            VoxelPool = null;

            VoxelAllocator?.Dispose();
            VoxelAllocator = null;

            VoxelScene?.Dispose();
            VoxelScene = null;
        }
        public struct FVxGroupId
        {
            uint Index;
        }
        public RHI.CGpuBuffer VoxelPool;//(group = 4*4*4) * num_of_group;
        public RHI.CGpuBuffer VoxelAllocator;//num_of_group = (64 * 64 * 32) 做成一个分配链，解决alloc和free效率问题
        public RHI.CGpuBuffer VoxelScene;//256 * 256 * 64 一米一个VXG的设计

        public RHI.CUnorderedAccessView UavVoxelAllocator;

        public Graphics.Pipeline.UDrawBuffers BasePass = new Graphics.Pipeline.UDrawBuffers();
        private RHI.CShaderDesc CSDesc_SetupVxAllocator;
        private RHI.CComputeShader CS_SetupVxAllocator;
        public override async System.Threading.Tasks.Task Initialize(Graphics.Pipeline.IRenderPolicy policy, 
            Graphics.Pipeline.Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var desc = new IGpuBufferDesc();
            desc.SetMode(false, true);
            unsafe
            {
                desc.m_ByteWidth = 64 * 64 * 32 * (uint)sizeof(FVxGroupId);
                desc.m_StructureByteStride = (uint)sizeof(FVxGroupId);
            }
            VoxelAllocator = rc.CreateGpuBuffer(ref desc, IntPtr.Zero);

            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.ToDefault();
            uavDesc.Buffer.NumElements = (uint)(64 * 64 * 32);
            UavVoxelAllocator = rc.CreateUnorderedAccessView(VoxelAllocator, ref uavDesc);

            unsafe
            {
                desc.m_ByteWidth = 256 * 256 * 64 * (uint)sizeof(FVxGroupId);
                desc.m_StructureByteStride = (uint)sizeof(FVxGroupId);
            }
            VoxelScene = rc.CreateGpuBuffer(ref desc, IntPtr.Zero);

            CSDesc_SetupVxAllocator = rc.CreateShaderDesc(RName.GetRName("Shaders/Compute/VXGI/VoxelAllocator.compute", RName.ERNameType.Engine), 
                "CS_SetupVxAllocator", EShaderType.EST_ComputeShader, null);
            CS_SetupVxAllocator = rc.CreateComputeShader(CSDesc_SetupVxAllocator);

            BasePass.Initialize(rc, debugName);
        }
        public override unsafe void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.IRenderPolicy policy, bool bClear)
        {
            if (CS_SetupVxAllocator != null)
            {
                UInt32 nUavInitialCounts = 1;
                var cmd = BasePass.DrawCmdList.mCoreObject;
                cmd.SetComputeShader(CS_SetupVxAllocator.mCoreObject);
                cmd.CSSetUnorderedAccessView(2, UavVoxelAllocator.mCoreObject, &nUavInitialCounts);
                cmd.CSDispatch(64 / 32, 64 / 32, 32 / 1);

                cmd.BeginCommand();
                //cmd.BeginRenderPass((RenderPassDesc*)0, new IFrameBuffers(), "VoxelsNode");
                //cmd.BuildRenderPass(0);
                //cmd.EndRenderPass();
                cmd.EndCommand();
            }
        }
        public unsafe void TickRender(Graphics.Pipeline.IRenderPolicy policy)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            var cmdlist_hp = BasePass.CommitCmdList.mCoreObject;
            cmdlist_hp.Commit(rc.mCoreObject);
        }
        public unsafe void TickSync(Graphics.Pipeline.IRenderPolicy policy)
        {
            BasePass.SwapBuffer();
        }
    }
}
