using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Node.GpuNode
{
    [Bricks.CodeBuilder.ContextMenu("Heigh2Flow", "Float1\\Heigh2Flow", UPgcGraph.PgcEditorKeyword)]
    public class TtHeigh2FlowMapNode : TtGpuNodeBase
    {
        public TtHeigh2FlowMapNode()
        {
            ResultDesc = UBufferCreator.CreateInstance<USuperBuffer<Vector2, FFloat2Operator>>(-1, -1, -1);
        }
        public override UBufferCreator GetResultDesc()
        {
            return ResultDesc;
        }
        public override UBufferCreator GetOutBufferCreator(Bricks.NodeGraph.PinOut pin)
        {
            if (ResultPin == pin)
            {
                return ResultDesc;
            }
            return null;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (Policy == null)
                return false;
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            var buffer = Input.GetGpuTexture2D<float>();
            Policy.OnResize(Input.Width, Input.Height);
            //Input.Upload2GpuTexture2D()
            var h2flow = Policy.FindNode<GpuShading.TtHeigh2FlowMapNode>();
            var heightAttachement = Policy.AttachmentCache.ImportAttachment(h2flow.HeightPinIn);
            heightAttachement.SetImportedBuffer(buffer);
            var ending = Policy.RootNode as GpuShading.TtGpuFetchNode;

            h2flow.DispatchThread.X = (uint)Input.Width;
            h2flow.DispatchThread.Y = (uint)Input.Height;
            h2flow.DispatchThread.Z = 1;

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
                var pImage = ((byte*)blob.DataPointer + reader.GetPosition());

                for (int y = 0; y < Output.Height; y++)
                {
                    for (int x = 0; x < Output.Width; x++)
                    {
                        Output.SetFloat2(x, y, 0, ((Vector2*)pImage)[x]);
                    }
                    pImage += rowPitch;
                }
            }
            return true;
        }
    }
}
