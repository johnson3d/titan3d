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
    public class URenderGraphNode : Thread.Async.IJob
    {
        public Thread.Async.IJobSystem JobSystem { get; set; }
        public Thread.Async.EJobState JobState { get; set; }
        public URenderGraphBuffer[] InputGpuBuffers { get; }
        public URenderGraphSRV[] InputShaderResourceViews { get; }
        public URenderGraphBuffer[] OutputGpuBuffers { get; }
        public URenderGraphSRV[] OutputShaderResourceViews { get; }
        public virtual void DoWork()
        {

        }
        public bool IsDependencyCompleted
        {
            get
            {
                foreach (var i in InputGpuBuffers)
                {
                    if (i.OutputNode != null && i.OutputNode.JobState == Thread.Async.EJobState.Working)
                    {
                        return false;
                    }
                }
                foreach (var i in InputShaderResourceViews)
                {
                    if (i.OutputNode != null && i.OutputNode.JobState == Thread.Async.EJobState.Working)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
    public class URenderGraph
    {
        public void Update()
        {
            
        }
    }
}
