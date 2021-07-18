using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class URenderGraphBuffer
    {
        public string Name { get; set; }
        public RHI.CGpuBuffer GpuBuffer;
        public URenderGraphNode OutputNode;
        public string OutputPinName { get; set; }

        //public RHI.CGpuBuffer GetGpuBuffer()
        //{
        //    if (OutputNode == null)
        //        return null;

        //    foreach(var i in OutputNode.OutputGpuBuffers)
        //    {
        //        if (i.Name == OutputPinName)
        //            return i.GpuBuffer;
        //    }

        //}
    }
    public class URenderGraphSRV
    {
        public string Name { get; set; }
        public RHI.CShaderResourceView SRV;

        public URenderGraphNode OutputNode;
        public string OutputPinName { get; set; }
    }
    public class URenderGraphNode
    {
        public Thread.Async.IJobSystem JobSystem { get; set; }
        public Thread.Async.EJobState JobState { get; set; }
        public URenderGraphBuffer[] InputGpuBuffers { get; set; }
        public URenderGraphSRV[] InputShaderResourceViews { get; set; }
        public URenderGraphBuffer[] OutputGpuBuffers { get; set; }
        public URenderGraphSRV[] OutputShaderResourceViews { get; set; }

        public virtual async System.Threading.Tasks.Task Initialize(IRenderPolicy policy, Shader.UShadingEnv shading, EPixelFormat fmt, EPixelFormat dsFmt, float x, float y, string debugName)
        {
            await Thread.AsyncDummyClass.DummyFunc();
        }
        public virtual void TickLogic(GamePlay.UWorld world, IRenderPolicy policy, bool bClear)
        {

        }
        public virtual void TickRender()
        {

        }
        public virtual void TickSync()
        {

        }
    }
    public class URenderGraph
    {
        public void Update()
        {
            
        }
    }
}
