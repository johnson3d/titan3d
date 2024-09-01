using Assimp;
using EngineNS.Bricks.Procedure.Node.GpuNode;
using EngineNS.Bricks.RenderPolicyEditor;
using EngineNS.GamePlay;
using EngineNS.Graphics.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Procedure.Node
{

    [Bricks.CodeBuilder.ContextMenu("GpuErosion", "Float1\\GpuErosion", UPgcGraph.PgcEditorKeyword)]
    public class TtGpuErosionNode : TtGpuNodeBase
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName RainMap
        {
            get;
            set;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (Policy == null)
                return false;
            var Input = graph.BufferCache.FindBuffer(SrcPin);
            var Output = graph.BufferCache.FindBuffer(ResultPin);
            var buffer = Input.GetGpuBuffer<float>();
            var incWater = Policy.FindNode<GpuShading.TtErosionIncWaterNode>();
            incWater.Rain = RainMap;
            var waterAttachement = Policy.AttachmentCache.ImportAttachment(incWater.WaterPinInOut);
            waterAttachement.SetImportedBuffer(buffer);
            var ending = Policy.RootNode as GpuShading.TtGpuFetchNode;

            incWater.DispatchThread.X = (uint)Input.Width;
            incWater.DispatchThread.Y = (uint)Input.Height;
            incWater.DispatchThread.Z = 1;

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
                        Output.SetFloat1(x, y, 0, ((float*)pImage)[x]);
                    }
                    pImage += rowPitch;
                }
            }   
            return true;
        }
    }
}
