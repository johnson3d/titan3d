using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct ICommandList
    {
        public unsafe bool BeginRenderPass(Graphics.Pipeline.URenderPolicy policy, Graphics.Pipeline.UGraphicsBuffers graphicsBuffers, in EngineNS.IRenderPassClears passClears, string debugName)
        {
            graphicsBuffers.BuildFrameBuffers(policy);
            fixed (EngineNS.IRenderPassClears* pinned_passClears = &passClears)
            {
                return BeginRenderPass(graphicsBuffers.FrameBuffers.mCoreObject, pinned_passClears, debugName);
            }
        }
    }
}

namespace EngineNS.RHI
{
    public class CCommandList : AuxPtrType<ICommandList>
    {
        public uint PassNumber;
        public uint GetPassNumber()
        {
            return mCoreObject.GetPassNumber();
        }
        public bool BeginCommand()
        {
            return mCoreObject.BeginCommand();
        }
        public void EndCommand()
        {
            mCoreObject.EndCommand();
        }
        public unsafe bool BeginRenderPass(Graphics.Pipeline.URenderPolicy policy, Graphics.Pipeline.UGraphicsBuffers graphicsBuffers, in EngineNS.IRenderPassClears passClears, string debugName)
        {
            if (policy != null)
                graphicsBuffers.BuildFrameBuffers(policy);
            fixed (EngineNS.IRenderPassClears* pinned_passClears = &passClears)
            {
                return mCoreObject.BeginRenderPass(graphicsBuffers.FrameBuffers.mCoreObject, pinned_passClears, debugName);
            }
        }
        public unsafe bool BeginRenderPass(Graphics.Pipeline.URenderPolicy policy, Graphics.Pipeline.UGraphicsBuffers graphicsBuffers, string debugName)
        {
            if (policy != null)
                graphicsBuffers.BuildFrameBuffers(policy);
            return mCoreObject.BeginRenderPass(graphicsBuffers.FrameBuffers.mCoreObject, (IRenderPassClears*)0, debugName);
        }
        public void EndRenderPass()
        {
            mCoreObject.EndRenderPass();
        }
        public void PushDrawCall(EngineNS.IDrawCall Pass)
        {
            mCoreObject.PushDrawCall(Pass);
        }
        public void ClearMeshDrawPassArray()
        {
            mCoreObject.ClearMeshDrawPassArray();
        }
        public void BuildRenderPass(int bImmCBuffer)
        {
            mCoreObject.BuildRenderPass(bImmCBuffer);
        }
        public void SetViewport(EngineNS.IViewPort vp)
        {
            mCoreObject.SetViewport(vp);
        }
    }

    public class CPipelineStat : AuxPtrType<IPipelineStat>
    {
        public CPipelineStat()
        {
            mCoreObject = IPipelineStat.CreateInstance();
        }
    }
}
