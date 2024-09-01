using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node.GpuNode
{
    //https://file.notion.so/f/f/9ea74915-0ba8-4149-8301-d1ee5ae8e483/ab83df73-97af-4a74-90d0-c0f7a55bafaf/2009_HowtoextractrivernetworksandcatchmentboundariesfromDEM.pdf?id=5e7d8919-1654-44d9-af39-f96b77b63f75&table=block&spaceId=9ea74915-0ba8-4149-8301-d1ee5ae8e483&expirationTimestamp=1711713600000&signature=jggggsGBzKBLhmMB_s5yf8jt-hjsyx5BrGX20Jn86BA&downloadName=2009_HowtoextractrivernetworksandcatchmentboundariesfromDEM.pdf
    [Bricks.CodeBuilder.ContextMenu("WaterBasin", "Float1\\WaterBasin", UPgcGraph.PgcEditorKeyword)]
    public class TtWaterBasinNode : TtGpuNodeBase
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
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
            incWater.TextureWidth = Output.Width;
            incWater.TextureHeight = Output.Height;
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
            var blob = new Support.TtBlobObject();
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
