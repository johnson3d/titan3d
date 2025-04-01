using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtGpuSceneNode
    {
        public TtRenderGraphPin InstancePinOut = TtRenderGraphPin.CreateOutput("Instances", false, EPixelFormat.PXF_UNKNOWN, EBufferType.BFT_SRV | EBufferType.BFT_UAV);
        public struct FGpuSceneInstance
        {
            public Matrix Matrix;
            public uint DataStart;
            public uint DataSize;
        }
        public TtCpu2GpuBuffer<FGpuSceneInstance> mSceneInstances = new TtCpu2GpuBuffer<FGpuSceneInstance>();
        public TtCpu2GpuBuffer<uint> mSceneInstanceData = new TtCpu2GpuBuffer<uint>();
        private Stack<uint> IdAllocator = new Stack<uint>();
        public List<uint> AliveInstances = new List<uint>();
        public uint AllocInstance()
        {
            if (IdAllocator.Count == 0)
                return uint.MaxValue;
            return IdAllocator.Pop();
        }
        public void FreeInstance(uint index)
        {
            IdAllocator.Push(index);
        }
        public void SetInstance<T>(uint index, in Matrix matrix, in T data) where T : unmanaged
        {

        }
        public void SetInstance(uint index, in Matrix matrix)
        {

        }
        private void Dispose_Instance()
        {

        }
        private unsafe void Initialize_Instance(TtRenderPolicy policy, string debugName)
        {
            const uint MaxInstance = 4096;
            mSceneInstances.SetSize((int)MaxInstance);
            mSceneInstanceData.SetSize((int)MaxInstance * 4);
            for (uint i = 0; i < MaxInstance; i++)
            {
                IdAllocator.Push(MaxInstance - 1 - i);
            }
        }
        private void TickLogic_Instance(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList cmd)
        {

        }
        private void FrameBuild_Instance()
        {

        }
    }
}
