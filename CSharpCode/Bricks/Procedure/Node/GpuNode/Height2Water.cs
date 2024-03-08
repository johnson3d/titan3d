using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node.GpuNode
{
    [Bricks.CodeBuilder.ContextMenu("WaterBasin", "Float1\\WaterBasin", UPgcGraph.PgcEditorKeyword)]
    public class TtWaterBasinNode : TtGpuNodeBase
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.USrView.AssetExt)]
        public RName RainMap
        {
            get;
            set;
        }
        [Rtti.Meta]
        public int Step { get; set; } = 16;
        [Rtti.Meta]
        public float RainScalar { get; set; } = 20.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (Policy == null)
                return false;
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            var buffer = Input.GetGpuTexture2D<float>();
            Policy.OnResize(Input.Width, Input.Height);
            var h2flow = Policy.FindNode<GpuShading.TtHeigh2FlowMapNode>();
            var heightAttachement = Policy.AttachmentCache.ImportAttachment(h2flow.HeightPinIn);
            heightAttachement.SetImportedBuffer(buffer);

            var waterBuffer = Output.GetGpuBuffer<float>();
            var incWater = Policy.FindNode<GpuShading.TtErosionIncWaterNode>();
            incWater.Rain = RainMap;
            incWater.RainScalar = RainScalar;
            var incWaterAttachement = Policy.AttachmentCache.ImportAttachment(incWater.WaterPinInOut);
            incWaterAttachement.SetImportedBuffer(waterBuffer);

            var waterBasin = Policy.FindNode<GpuShading.TtWaterBasinNode>();
            waterBasin.Step = Step;

            var ending = Policy.RootNode as GpuShading.TtGpuFetchNode;

            h2flow.DispatchThread.X = (uint)Input.Width;
            h2flow.DispatchThread.Y = (uint)Input.Height;
            h2flow.DispatchThread.Z = 1;

            incWater.DispatchThread.X = (uint)Input.Width;
            incWater.DispatchThread.Y = (uint)Input.Height;
            incWater.DispatchThread.Z = 1;

            waterBasin.DispatchThread.X = (uint)Input.Width;
            waterBasin.DispatchThread.Y = (uint)Input.Height;
            waterBasin.DispatchThread.Z = 1;

            GpuProcess();

            ending.mFinishFence.WaitToExpect();
            var readTexture = ending.ReadableTexture;
            var blob = new Support.UBlobObject();
            readTexture.FetchGpuData(0, blob.mCoreObject);
            using (var reader = IO.UMemReader.CreateInstance((byte*)blob.DataPointer, blob.Size))
            {
                uint rowPitch, depthPitch;
                reader.Read(out rowPitch);
                reader.Read(out depthPitch);
                rowPitch = (uint)(sizeof(float) * Output.Width);
                var pImage = ((byte*)blob.DataPointer + reader.GetPosition());

                for (int y = 0; y < Output.Height; y++)
                {
                    for (int x = 0; x < Output.Width; x++)
                    {
                        ref var v = ref ((float*)pImage)[x];
                        Output.SetPixel<float>(x, y, 0, in v);
                    }
                    pImage += rowPitch;
                }
            }
            return true;
        }
    }
}
