using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    [Bricks.CodeBuilder.ContextMenu("Copy", "Copy\\Copy", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UCopyNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UCopyNode" })]
    public class TtCopyNode : TAuxRenderGraphNode<TtCopyNode>
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin DestPinOut = TtRenderGraphPin.CreateOutput("Dest", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV);
        public TtCopyNode()
        {
            Name = "CopyNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
            AddOutput(DestPinOut);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            
            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void Dispose()
        {
            //CoreSDK.DisposeObject(ref ResultBuffer);
            CoreSDK.DisposeObject(ref mCopyDrawcall); 
            base.Dispose();
        }
        public NxRHI.TtCopyDraw mCopyDrawcall;
        [Rtti.Meta]
        [Category("Option")]
        public TtAttachBuffer.ELifeMode OutputLifeMode { get; set; } = TtAttachBuffer.ELifeMode.Transient;
        //public TtAttachBuffer DestAttachement = new TtAttachBuffer();
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            //var attachement = RenderGraph.AttachmentCache.ImportAttachment(DestPinOut, DestAttachement);
            //if (SrcPinIn.Attachement.Format != DestPinOut.Attachement.Format ||
            //    SrcPinIn.Attachement.Width != DestPinOut.Attachement.Width ||
            //    SrcPinIn.Attachement.Height != DestPinOut.Attachement.Height)
            //{
            //    ResultBuffer = attachement.Clone();
            //    attachement.GpuResource = ResultBuffer.GpuResource;
            //    attachement.Srv = ResultBuffer.Srv;
            //    attachement.Rtv = ResultBuffer.Rtv;
            //    attachement.Dsv = ResultBuffer.Dsv;
            //    attachement.Uav = ResultBuffer.Uav;
            //    DestPinOut.Attachement.Format = SrcPinIn.Attachement.Format;
            //    DestPinOut.Attachement.Width = SrcPinIn.Attachement.Width;
            //    DestPinOut.Attachement.Height = SrcPinIn.Attachement.Height;
            //}
            //if (ResultBuffer != null)
            //{
            //    attachement.GpuResource = ResultBuffer.GpuResource;
            //    attachement.Srv = ResultBuffer.Srv;
            //    attachement.Rtv = ResultBuffer.Rtv;
            //    attachement.Dsv = ResultBuffer.Dsv;
            //    attachement.Uav = ResultBuffer.Uav;
            //}
        }
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            base.BeforeTickLogic(policy);
            var srcPin = GetAttachBuffer(SrcPinIn);
            DestPinOut.Attachement.Format = srcPin.BufferDesc.Format;
            DestPinOut.Attachement.Width = srcPin.BufferDesc.Width;
            DestPinOut.Attachement.Height= srcPin.BufferDesc.Height;
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();

            using (new NxRHI.TtCmdListScope(cmdlist, "Copy"))
            {
                var srcPin = GetAttachBuffer(SrcPinIn);
                var ResultBuffer = GetAttachBuffer(DestPinOut);
                ResultBuffer.LifeMode = OutputLifeMode;

                if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtTexture))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
                }
                mCopyDrawcall.BindSrc(srcPin.GpuResource);
                mCopyDrawcall.BindDest(ResultBuffer.GpuResource);

                //if (SrcPinIn.Attachement.Format == EPixelFormat.PXF_UNKNOWN)
                //{
                //    //SetCopyBuffer(srcPin.Buffer.mCoreObject, 0, tarPin.Buffer.mCoreObject, 0, SrcPinIn.Attachement.Width * SrcPinIn.Attachement.Height);
                //}
                //else
                //{   
                //    mCopyDrawcall.SetCopyTexture2D(srcPin.Buffer.mCoreObject, 0, 0, 0, tarPin.Buffer.mCoreObject, 0, 0, 0, SrcPinIn.Attachement.Width, SrcPinIn.Attachement.Height);
                //}

                cmdlist.PushGpuDraw(mCopyDrawcall);
                cmdlist.BeginEvent(Name);
                cmdlist.FlushDraws();
                cmdlist.EndEvent();
            }

            policy.CommitCommandList(cmdlist, "Copy");
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Copy", "Copy\\Copy2Readback", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtCopy2ReadbackNode : TAuxRenderGraphNode<TtCopy2ReadbackNode>
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin DestPinOut = TtRenderGraphPin.CreateOutput("Dest", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_NONE);
        public TtCopy2ReadbackNode()
        {
            Name = "Copy2ReadbackNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
            DestPinOut.Attachement.BufferDesc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
            AddOutput(DestPinOut);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);

            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref ResultBuffer);
            CoreSDK.DisposeObject(ref mCopyDrawcall);
            base.Dispose();
        }
        public TtAttachBuffer ResultBuffer;
        public NxRHI.TtCopyDraw mCopyDrawcall;
        [Rtti.Meta]
        [Category("Option")]
        public TtAttachBuffer.ELifeMode OutputLifeMode { get; set; } = TtAttachBuffer.ELifeMode.Imported;
        public override void BeforeTickLogic(TtRenderPolicy policy)
        {
            base.BeforeTickLogic(policy);
            var srcPin = GetAttachBuffer(SrcPinIn);
            //ulong rowSize = 0;
            //ulong totalSize = 0;
            if (srcPin.GpuResource is NxRHI.TtTexture)
            {
                var fp = new NxRHI.FSubResourceFootPrint();
                ulong rowSize = 0;
                ulong totalSize = 0;
                ((NxRHI.TtTexture)srcPin.GpuResource).GetFootprint(ref fp, ref rowSize, ref totalSize);
                DestPinOut.Attachement.Width = (uint)totalSize;
                DestPinOut.Attachement.Height = 1;
            }
            else if (srcPin.GpuResource is NxRHI.TtBuffer)
            {
                DestPinOut.Attachement.Width = srcPin.BufferDesc.Width;
            }

        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();

            using (new NxRHI.TtCmdListScope(cmdlist, "Copy2Readback"))
            {
                var srcPin = GetAttachBuffer(SrcPinIn);
                ResultBuffer = GetAttachBuffer(DestPinOut);
                ResultBuffer.LifeMode = OutputLifeMode;

                if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtBuffer) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Buffer;
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtTexture))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Texture;
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Texture2Buffer;
                    ref var fp = ref mCopyDrawcall.FootPrint;
                    ulong rowSize = 0;
                    ulong totalSize = 0;
                    ((NxRHI.TtTexture)srcPin.GpuResource).GetFootprint(ref fp, ref rowSize, ref totalSize);
                }
                else if (srcPin.GpuResource.GetType() == typeof(NxRHI.TtTexture) && ResultBuffer.GpuResource.GetType() == typeof(NxRHI.TtBuffer))
                {
                    mCopyDrawcall.Mode = NxRHI.ECopyDrawMode.CDM_Buffer2Texture;
                }
                mCopyDrawcall.BindSrc(srcPin.GpuResource);
                mCopyDrawcall.BindDest(ResultBuffer.GpuResource);

                //if (SrcPinIn.Attachement.Format == EPixelFormat.PXF_UNKNOWN)
                //{
                //    //SetCopyBuffer(srcPin.Buffer.mCoreObject, 0, tarPin.Buffer.mCoreObject, 0, SrcPinIn.Attachement.Width * SrcPinIn.Attachement.Height);
                //}
                //else
                //{   
                //    mCopyDrawcall.SetCopyTexture2D(srcPin.Buffer.mCoreObject, 0, 0, 0, tarPin.Buffer.mCoreObject, 0, 0, 0, SrcPinIn.Attachement.Width, SrcPinIn.Attachement.Height);
                //}

                cmdlist.PushGpuDraw(mCopyDrawcall);
                cmdlist.BeginEvent(Name);
                cmdlist.FlushDraws();
                cmdlist.EndEvent();
            }

            policy.CommitCommandList(cmdlist, "Copy");
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Copy", "Copy\\Copy2NextFrame", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.Graphics.Pipeline.Common.UCopy2NextFrameNode@EngineCore", "EngineNS.Graphics.Pipeline.Common.UCopy2NextFrameNode" })]
    public class TtCopy2NextFrameNode : TAuxRenderGraphNode<TtCopy2NextFrameNode>
    {
        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);
        public TtRenderGraphPin PrevPinOut = TtRenderGraphPin.CreateOutput("Prev", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV);

        public TtCopy2NextFrameNode()
        {
            Name = "Copy2NextFrameNode";
        }
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
            AddOutput(PrevPinOut);
            SrcPinIn.Attachement.BufferDesc.Flags = NxRHI.EResourceMiscFlag.RM_COPY_SRC;
        }
        public override Color4b GetTileColor()
        {
            return Color4b.FromRgb(255, 255, 0);
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);
            
            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref PrevOutAttachement);
            CoreSDK.DisposeObject(ref ResultBuffer[0]);
            CoreSDK.DisposeObject(ref ResultBuffer[1]);
            CoreSDK.DisposeObject(ref mCopyDrawcall); 
            base.Dispose();
        }
        public TtAttachBuffer[] ResultBuffer = new TtAttachBuffer[2];
        public TtAttachBuffer Current { get => ResultBuffer[0]; }
        public TtAttachBuffer Previos { get => ResultBuffer[1]; }
        public bool IsCpuAceesResult { get; set; } = false;
        public NxRHI.TtCopyDraw mCopyDrawcall;
        TtAttachBuffer PrevOutAttachement = new TtAttachBuffer();
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(PrevPinOut, PrevOutAttachement);
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
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;
            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();
            using (new NxRHI.TtCmdListScope(cmdlist, "Copy2Next"))
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
                cmdlist.BeginEvent(Name);
                cmdlist.FlushDraws();
                cmdlist.EndEvent();
            }
            policy.CommitCommandList(cmdlist, "Copy2Next");
        }

        public override unsafe void TickSync(TtRenderPolicy policy)
        {
            base.TickSync(policy);
            MathHelper.Swap(ref ResultBuffer[0], ref ResultBuffer[1]);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Debugger", "Debugger", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    public class TtDebuggerNode : TAuxRenderGraphNode<TtDebuggerNode>
    {
        public class TtRDGDebugger : IRootForm
        {
            public bool Visible { get; set; } = true;
            public uint DockId { get; set; }
            public ImGuiWindowClass DockKeyClass { get; }
            public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
            public TtRDGDebugger()
            {

            }
            public unsafe void Dispose()
            {
            }
            public async Thread.Async.TtTask<bool> Initialize()
            {
                await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
                return true;
            }
            public TtDebuggerNode RDGNode;
            Vector2 Offset = Vector2.Zero;
            public void OnDraw()
            {
                var result = EGui.UIProxy.DockProxy.BeginMainForm($"RDG Visual Debugger:{RDGNode?.Name}", this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
                if (result)
                {
                    if (RDGNode != null && RDGNode.ResultBuffer.BufferDesc.Format != EPixelFormat.PXF_UNKNOWN)
                    {
                        var winPos = ImGuiAPI.GetWindowPos();
                        var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                        var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                        var DrawOffset = new Vector2();
                        DrawOffset.SetValue(winPos.X + vpMin.X, winPos.Y + vpMin.Y);
                        DrawOffset += Offset;

                        var cmdlist = ImGuiAPI.GetWindowDrawList();
                        var size = ImGuiAPI.GetWindowSize();
                        //RDGNode.ResultBuffer.Srv.TagObject = "RDG Debugger";
                        ImTextureRef imTextureRef = new ImTextureRef();
                        imTextureRef.m__TexID = (ulong)RDGNode.ResultBuffer.Srv.GetTextureHandle();
                        cmdlist.AddImage(imTextureRef, DrawOffset, DrawOffset + (vpMax - vpMin), in Vector2.Zero, in Vector2.One, 0xFFFFFFFF);
                    }
                }
                EGui.UIProxy.DockProxy.EndMainForm(result);
            }
        }

        public TtDebuggerNode()
        {
            Name = "Debugger";
        }
        public TtRDGDebugger mDebugger;
        [Category("Option")]
        public bool ShowDebugger
        {
            get
            {
                return mDebugger != null;
            }
            set
            {
                if (value == true)
                {
                    if (mDebugger == null)
                    {
                        mDebugger = new TtRDGDebugger();
                        mDebugger.RDGNode = this;
                    }
                    TtEngine.RootFormManager.RegRootForm(mDebugger);
                }
                else
                {
                    if (mDebugger != null)
                    {
                        TtEngine.RootFormManager.UnregRootForm(mDebugger);
                        mDebugger = null;
                    }
                }
            }
        }

        public TtRenderGraphPin SrcPinIn = TtRenderGraphPin.CreateInput("Src", NxRHI.EBufferType.BFT_SRV);
        public override void InitNodePins()
        {
            AddInput(SrcPinIn);
            SrcPinIn.IsAllowInputNull = true;
        }
        public override async Thread.Async.TtTask Initialize(TtRenderPolicy policy, string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            await base.Initialize(policy, debugName);

            mCopyDrawcall = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
        }
        public override void Dispose()
        {
            ShowDebugger = false;
            CoreSDK.DisposeObject(ref ResultBuffer);
            CoreSDK.DisposeObject(ref mCopyDrawcall);
            base.Dispose();
        }
        public TtAttachBuffer ResultBuffer;
        public bool IsCpuAceesResult { get; set; } = false;
        public NxRHI.TtCopyDraw mCopyDrawcall;
        public override void FrameBuild(Graphics.Pipeline.TtRenderPolicy policy)
        {
            
        }
        public override unsafe void TickLogic(GamePlay.TtWorld world, TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (mCopyDrawcall == null)
                return;

            var srcPin = GetAttachBuffer(SrcPinIn);
            if (ResultBuffer == null || SrcPinIn.Attachement.Format != ResultBuffer.BufferDesc.Format ||
                SrcPinIn.Attachement.Width != ResultBuffer.BufferDesc.Width ||
                SrcPinIn.Attachement.Height != ResultBuffer.BufferDesc.Height)
            {
                ResultBuffer = srcPin.Clone();
            }

            var cmdlist = TtEngine.Instance.GfxDevice.RenderContext.CmdListManager.GetCmdList();

            using (new NxRHI.TtCmdListScope(cmdlist, "Debugger"))
            {
                var tarPin = ResultBuffer;

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

                cmdlist.PushGpuDraw(mCopyDrawcall);
                cmdlist.BeginEvent(Name);
                cmdlist.FlushDraws();
                cmdlist.EndEvent();
            }

            policy.CommitCommandList(cmdlist, "Debugger");
        }
    }
}
