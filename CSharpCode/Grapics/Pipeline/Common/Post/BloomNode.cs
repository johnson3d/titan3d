using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using EngineNS.GamePlay;

namespace EngineNS.Graphics.Pipeline.Common.Post
{
    //Bloom: https://zhuanlan.zhihu.com/p/525500877
    public class TtBloomNode : TtRenderGraphNode
    {
        public TtRenderGraphPin ColorPinIn = TtRenderGraphPin.CreateInputOutput("Color");
        public TtRenderGraphPin ResultPinOut = TtRenderGraphPin.CreateOutput("Result", true, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public TtBloomNode()
        {
            Name = "BloomNode";

            mBloomStruct.SetDefault();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinIn, NxRHI.EBufferType.BFT_SRV);
            AddOutput(ResultPinOut, NxRHI.EBufferType.BFT_RTV | NxRHI.EBufferType.BFT_SRV);

            base.InitNodePins();
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        struct FBloomStruct
        {
            public void SetDefault()
            {
                DownSampleSigma = 1.0f;
            }
            public float DownSampleSigma;
        }
        FBloomStruct mBloomStruct;
        [Category("Option")]
        [Rtti.Meta]
        public float DownSampleSigma
        {
            get => mBloomStruct.DownSampleSigma;
            set => mBloomStruct.DownSampleSigma = value;
        }
        [Category("Option")]
        [Rtti.Meta]
        public int NumDownSample { get; set; } = 5;
        public TtGaussNode[] DownSampleNodes = null;
        public TtGaussAdditiveNode[] UpSampleNodes = null;

        public override async Task Initialize(URenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);

            DownSampleNodes = new TtGaussNode[NumDownSample];
            var curOutPin = ColorPinIn;
            for (int i = 0; i < NumDownSample; i++)
            {
                var dsNode = new TtGaussNode();
                dsNode.InitNodePins();
                dsNode.RenderGraph = policy;
                dsNode.Name = $"{this.Name}:Down{i}";
                dsNode.ResultPinOut.IsAutoResize = false;
                await dsNode.Initialize(policy, dsNode.Name);

                dsNode.ColorPinIn.Attachement.SetDesc(curOutPin.Attachement);
                DownSampleNodes[i] = dsNode;
                curOutPin = dsNode.ResultPinOut;
            }

            UpSampleNodes = new TtGaussAdditiveNode[NumDownSample - 1];
            var prevMipPin = DownSampleNodes[NumDownSample - 1].ResultPinOut;
            for (int i = UpSampleNodes.Length - 1; i >= 0; i--)
            {
                var usNode = new TtGaussAdditiveNode();
                usNode.InitNodePins();
                usNode.RenderGraph = policy;
                usNode.Name = $"{this.Name}:Up{i}";
                usNode.ResultPinOut.IsAutoResize = false;
                await usNode.Initialize(policy, usNode.Name);

                usNode.Color1PinIn.Attachement.SetDesc(DownSampleNodes[i].ResultPinOut.Attachement);
                usNode.Color2PinIn.Attachement.SetDesc(prevMipPin.Attachement);
                UpSampleNodes[i] = usNode;

                prevMipPin = usNode.ResultPinOut;
            }
        }
        public override void OnResize(URenderPolicy policy, float x, float y)
        {
            base.OnResize(policy, x, y);

            var w = (uint)x;
            var h = (uint)y;

            for (int i = 0; i < NumDownSample; i++)
            {
                w = w / 2;
                h = h / 2;
                if (w == 0)
                    w = 1;
                if (h == 0)
                    h = 1;
                DownSampleNodes[i].OnResize(policy, w, h);
                DownSampleNodes[i].ResultPinOut.Attachement.Width = w;
                DownSampleNodes[i].ResultPinOut.Attachement.Height = h;

                if (i != NumDownSample - 1)
                {
                    UpSampleNodes[i].OnResize(policy, w, h);
                    UpSampleNodes[i].ResultPinOut.Attachement.Width = w;
                    UpSampleNodes[i].ResultPinOut.Attachement.Height = h;
                }
            }
        }
        public override void TickLogic(UWorld world, URenderPolicy policy, bool bClear)
        {
            foreach (var i in DownSampleNodes)
            {
                i.BeforeTickLogic(policy);
                i.TickLogic(world, policy, bClear);
            }

            for (int i = UpSampleNodes.Length - 1; i >= 0; i--)
            {
                UpSampleNodes[i].BeforeTickLogic(policy);
                UpSampleNodes[i].TickLogic(world, policy, bClear);
            }

            MoveAttachment(UpSampleNodes[0].ResultPinOut, ResultPinOut);

            foreach (var i in DownSampleNodes)
            {
                RenderGraph.AttachmentCache.RemoveAttachement(i.ResultPinOut.Attachement.AttachmentName);
            }
            foreach (var i in UpSampleNodes)
            {
                RenderGraph.AttachmentCache.RemoveAttachement(i.ResultPinOut.Attachement.AttachmentName);
            }

            base.TickLogic(world, policy, bClear);
        }
        public override void TickSync(URenderPolicy policy)
        {
            foreach (var i in DownSampleNodes)
            {
                i.TickSync(policy);
            }
            foreach (var i in UpSampleNodes)
            {
                if (i == null)
                    continue;
                i.TickSync(policy);
            }
            base.TickSync(policy);
        }
    }
}
