﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Copy", "Copy", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UCopyNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UCopyNode" })]
    public class TtCopyNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src");
        public TtRenderGraphPin DestPinOut = TtRenderGraphPin.CreateOutput("Dest", false, EPixelFormat.PXF_UNKNOWN);
        public TtCopyNode()
        {
            Name = "CopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn, NxRHI.EBufferType.BFT_SRV);
            AddOutput(DestPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            BasePass.Initialize(rc, debugName + ".BasePass");

            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public TtAttachBuffer ResultBuffer;
        public bool IsCpuAceesResult { get; set; } = false;
        public NxRHI.TtCopyDraw mCopyDrawcall;
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(DestPinOut);
            if (SrcPinIn.Attachement.Format != DestPinOut.Attachement.Format ||
                SrcPinIn.Attachement.Width != DestPinOut.Attachement.Width ||
                SrcPinIn.Attachement.Height != DestPinOut.Attachement.Height)
            {
                ResultBuffer = attachement.Clone();
                attachement.GpuResource = ResultBuffer.GpuResource;
                attachement.Srv = ResultBuffer.Srv;
                attachement.Rtv = ResultBuffer.Rtv;
                attachement.Dsv = ResultBuffer.Dsv;
                attachement.Uav = ResultBuffer.Uav;
                DestPinOut.Attachement.Format = SrcPinIn.Attachement.Format;
                DestPinOut.Attachement.Width = SrcPinIn.Attachement.Width;
                DestPinOut.Attachement.Height = SrcPinIn.Attachement.Height;
            }
            if (ResultBuffer != null)
            {
                attachement.GpuResource = ResultBuffer.GpuResource;
                attachement.Srv = ResultBuffer.Srv;
                attachement.Rtv = ResultBuffer.Rtv;
                attachement.Dsv = ResultBuffer.Dsv;
                attachement.Uav = ResultBuffer.Uav;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtCopyNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mCopyDrawcall == null)
                    return;
                var cmdlist = BasePass.DrawCmdList;
                
                using (new NxRHI.TtCmdListScope(cmdlist))
                {
                    var srcPin = GetAttachBuffer(SrcPinIn);
                    var tarPin = GetAttachBuffer(DestPinOut);

                    if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer) && tarPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && tarPin.GpuResource.GetType() == typeof(NxRHI.TtTexture))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && tarPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && tarPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
                    }
                    mCopyDrawcall.BindSrc(srcPin.GpuResource);
                    mCopyDrawcall.BindDest(tarPin.GpuResource);

                    //if (SrcPinIn.Attachement.Format == EPixelFormat.PXF_UNKNOWN)
                    //{
                    //    //SetCopyBuffer(srcPin.Buffer.mCoreObject, 0, tarPin.Buffer.mCoreObject, 0, SrcPinIn.Attachement.Width * SrcPinIn.Attachement.Height);
                    //}
                    //else
                    //{   
                    //    mCopyDrawcall.SetCopyTexture2D(srcPin.Buffer.mCoreObject, 0, 0, 0, tarPin.Buffer.mCoreObject, 0, 0, 0, SrcPinIn.Attachement.Width, SrcPinIn.Attachement.Height);
                    //}

                    cmdlist.PushGpuDraw(mCopyDrawcall);
                    cmdlist.FlushDraws();
                }
                
                policy.CommitCommandList(cmdlist);
            }   
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Copy", "Copy2NextFrame", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class UCopy2NextFrameNode : Graphics.Pipeline.TtRenderGraphNode
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src");
        public TtRenderGraphPin PrevPinOut = TtRenderGraphPin.CreateOutput("Prev", false, EPixelFormat.PXF_UNKNOWN);

        public UCopy2NextFrameNode()
        {
            Name = "Copy2NextFrameNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn, NxRHI.EBufferType.BFT_SRV);
            AddOutput(PrevPinOut, NxRHI.EBufferType.BFT_SRV);
        }
        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(255, 255, 0);
        }
        public override async System.Threading.Tasks.Task Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            BasePass.Initialize(rc, debugName + ".BasePass");

            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public TtAttachBuffer[] ResultBuffer = new TtAttachBuffer[2];
        public TtAttachBuffer Current { get => ResultBuffer[0]; }
        public TtAttachBuffer Previos { get => ResultBuffer[1]; }
        public bool IsCpuAceesResult { get; set; } = false;
        public NxRHI.TtCopyDraw mCopyDrawcall;
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(PrevPinOut);
            if (SrcPinIn.Attachement.Format != PrevPinOut.Attachement.Format ||
                SrcPinIn.Attachement.Width != PrevPinOut.Attachement.Width ||
                SrcPinIn.Attachement.Height != PrevPinOut.Attachement.Height)
            {
                PrevPinOut.Attachement.Format = SrcPinIn.Attachement.Format;
                PrevPinOut.Attachement.Width = SrcPinIn.Attachement.Width;
                PrevPinOut.Attachement.Height = SrcPinIn.Attachement.Height;

                CoreSDK.DisposeObject(ref ResultBuffer[0]);
                CoreSDK.DisposeObject(ref ResultBuffer[1]);
                ResultBuffer[0] = new TtAttachBuffer();
                ResultBuffer[1] = new TtAttachBuffer();
                ResultBuffer[0].BufferDesc = SrcPinIn.Attachement.BufferDesc;
                ResultBuffer[0].CreateBufferViews(in ResultBuffer[0].BufferDesc);

                ResultBuffer[1].BufferDesc = SrcPinIn.Attachement.BufferDesc;
                ResultBuffer[1].CreateBufferViews(in ResultBuffer[0].BufferDesc);
            }
            attachement.GpuResource = Previos.GpuResource;
            attachement.Srv = Previos.Srv;
            attachement.Rtv = Previos.Rtv;
            attachement.Dsv = Previos.Dsv;
            attachement.Uav = Previos.Uav;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtCopyNode), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, bool bClear)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                if (mCopyDrawcall == null)
                    return;
                var cmdlist = BasePass.DrawCmdList;
                using (new NxRHI.TtCmdListScope(cmdlist))
                {
                    var srcPin = GetAttachBuffer(SrcPinIn);

                    if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer) && Current.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && Current.GpuResource.GetType() == typeof(NxRHI.TtTexture))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && Current.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
                    }
                    else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && Current.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                    {
                        mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
                    }
                    mCopyDrawcall.BindSrc(srcPin.GpuResource);
                    mCopyDrawcall.BindDest(Current.GpuResource);

                    //var fp = new NxRHI.FSubResourceFootPrint();
                    //fp.SetDefault();
                    //mCopyDrawcall.mCoreObject.FootPrint = fp;
                    
                    cmdlist.PushGpuDraw(mCopyDrawcall);
                    cmdlist.FlushDraws();
                }
                policy.CommitCommandList(cmdlist);
            }
        }

        public override unsafe void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
            MathHelper.Swap(ref ResultBuffer[0], ref ResultBuffer[1]);
        }
    }
}
