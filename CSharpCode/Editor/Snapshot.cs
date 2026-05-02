using EngineNS.IO;
using EngineNS.NxRHI;
using EngineNS.Thread;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Editor.TtSnapshot;

namespace EngineNS.Editor
{
    public class TtSnapshot
    {
        public NxRHI.TtSrView mTextureRSV;
        public enum ESnapSide
        {
            Center,
            Left,
            Right,
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.ITexture tex, uint x, uint y, uint w, uint h, ESnapSide side = ESnapSide.Center, bool bAsync = true)
        {
            var file = rname.Address + ".snap";
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, file);
            var cpDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            var readable = tex.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, cpDraw.mCoreObject);
            
            if (bAsync)
            {
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd((TtRCmdQueue queue, ref NxRHI.FRCmdInfo info) =>
                {
                    using (var cmd = new FTransientCmd(EQueueType.QU_Default, "TtSnapshot.Save"))
                    {
                        cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                    }
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
                    cpDraw.Dispose();
                    cpDraw = null;
                }, "Copy Snap Texture");
                TtEngine.Instance.EventPoster.RunOn((Thread.Async.FPostEvent<bool>)((state) =>
                {
                    fence.Wait(1);
                    TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd((FRenderCmd)((TtRCmdQueue queue, ref NxRHI.FRCmdInfo info) =>
                    {
                        var gpuDataBlob = new Support.TtBlobObject();
                        var bufferData = new Support.TtBlobObject();
                        readable.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, (IBlobObject)gpuDataBlob.mCoreObject);
                        NxRHI.ITexture.BuildImage2DBlob((IBlobObject)bufferData.mCoreObject, (IBlobObject)gpuDataBlob.mCoreObject, tex.Desc);
                        TtEngine.Instance.EventPoster.RunOn((Thread.Async.FPostEvent<bool>)((state) =>
                        {
                            TtSnapshot.SavePng(ameta, file, (Support.TtBlobObject)bufferData, side);
                            return true;
                        }), Thread.Async.EAsyncTarget.AsyncEditor);
                    }), "Fetch");
                    return true;
                }), Thread.Async.EAsyncTarget.AsyncEditor);
            }
            else
            {
                TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd((TtRCmdQueue queue, ref NxRHI.FRCmdInfo info) =>
                {
                    using (var cmd = new FTransientCmd(EQueueType.QU_Default, "TtSnapshot.Save"))
                    {
                        cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                    }
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
                    cpDraw.Dispose();
                    cpDraw = null;
                }, "Copy Snap Texture");
                fence.Wait(1);
                var gpuDataBlob = new Support.TtBlobObject();
                var bufferData = new Support.TtBlobObject();
                readable.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, (IBlobObject)gpuDataBlob.mCoreObject);
                NxRHI.ITexture.BuildImage2DBlob((IBlobObject)bufferData.mCoreObject, (IBlobObject)gpuDataBlob.mCoreObject, tex.Desc);
                TtSnapshot.SavePng(ameta, file, (Support.TtBlobObject)bufferData, side);
            }
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.TtSrView srv, ESnapSide side = ESnapSide.Center, bool bAsync = true)
        {
            uint w = srv.GetTexture().Desc.Width;
            uint h = srv.GetTexture().Desc.Height;
            Save(rname, ameta, srv.GetTexture(), 0, 0, w, h, side, bAsync);
        }
        public unsafe static StbImageSharp.TtMemImage SavePng(IO.IAssetMeta ameta, string file, Support.TtBlobObject bufferData, ESnapSide side = ESnapSide.Center)
        {
            byte* pPixelData = (byte*)bufferData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return null;
            var pBitmapDesc = (NxRHI.FPixelDesc*)pPixelData;
            pPixelData += sizeof(NxRHI.FPixelDesc);

            StbImageSharp.TtMemImage image = null;
            using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate))// .MemoryStream(pBitmapDesc->Stride * pBitmapDesc->Height))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                image = StbImageSharp.TtMemImage.FromResult(pPixelData, (int)pBitmapDesc->Width, (int)pBitmapDesc->Height, StbImageSharp.ColorComponents.RedGreenBlueAlpha, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                switch (side)
                {
                    case ESnapSide.Center:
                        image = StbImageSharp.ImageProcessor.GetCenterSquare(image);
                        break;
                    case ESnapSide.Left:
                        image = StbImageSharp.ImageProcessor.GetCenterLeft(image);
                        break;
                    case ESnapSide.Right:
                        image = StbImageSharp.ImageProcessor.GetCenterRight(image);
                        break;
                    default:
                        image = StbImageSharp.ImageProcessor.GetCenterSquare(image);
                        break;
                }
                
                if (image.Width > 128)
                {
                    float rate = 128 / (float)image.Width;// pBitmapDesc->Width;
                    int height = (int)((float)image.Height * rate);
                    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, 128, height);
                }

                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                ameta.ResetSnapshot();
            }
            ameta.AddAssetFile(file);
            TtEngine.Instance.SourceControlModule.AddFile(file);
            return image;
        }
        public static async Thread.Async.TtTask<TtSnapshot> Load(IAssetMeta assetMeta)
        {
            var file = assetMeta.GetAssetName().Address + ".snap";
            TtSnapshot result = new TtSnapshot();
            result.mTextureRSV = await TtEngine.Instance.GfxDevice.TextureManager.CreateTexture(file);
            if (result.mTextureRSV == null)
            {
                // 通过引擎统一的串行队列调度 AutoGen, 防止多个资产并发触发 OffscreenRenderer + RenderPolicy 创建导致显存暴涨
                if (await TtEngine.Instance.SnapshotGenQueue.EnqueueAutoGen(assetMeta))
                {
                    result.mTextureRSV = await TtEngine.Instance.GfxDevice.TextureManager.CreateTexture(file);
                    if (result.mTextureRSV != null)
                        return result;
                }
                return null;
            }
            return result;
        }
    }

    /// <summary>
    /// 资产快照自动生成的串行队列。
    /// IAssetMeta.AutoGenSnapshot 内部会创建 TtSnapshotCreator (持有 TtOffscreenRenderer + 完整 RenderPolicy + 渲染目标),
    /// 如果多个资产 (例如内容浏览器一次刷新数百个) 同时触发 AutoGen, 会瞬间产生大量并发的 RenderPolicy 实例,
    /// 直接把显存吃爆。本队列把所有 AutoGen 请求串行化, 一次只跑一个。
    /// 调用方仍然可以正常 await EnqueueAutoGen, 内部通过引擎 EventPoster 实现非阻塞等待。
    /// </summary>
    public class TtSnapshotGenQueue
    {
        class FPendingItem
        {
            public IO.IAssetMeta Meta;
            public bool Result;
            public TtSemaphore FinishedSemaphore;
        }

        readonly object mLock = new object();
        readonly Queue<FPendingItem> mPendingItems = new Queue<FPendingItem>();
        bool mWorkerRunning = false;

        /// <summary>
        /// 当前队列中等待处理的请求数 (不含正在处理的那一个)。
        /// </summary>
        public int PendingCount
        {
            get { lock (mLock) { return mPendingItems.Count; } }
        }

        /// <summary>
        /// 把 AutoGenSnapshot 请求加入串行队列, await 直到该资产真正被处理完成。
        /// 同一时刻全引擎只会有一个 AutoGenSnapshot 在执行, 完成后才会处理下一个。
        /// </summary>
        public async Thread.Async.TtTask<bool> EnqueueAutoGen(IO.IAssetMeta assetMeta)
        {
            if (assetMeta == null)
                return false;

            var item = new FPendingItem { Meta = assetMeta, FinishedSemaphore = TtSemaphore.CreateSemaphore(1) };
            lock (mLock)
            {
                mPendingItems.Enqueue(item);
            }
            EnsureWorker();

            await item.FinishedSemaphore.Await();
            item.FinishedSemaphore.FreeSemaphore();
            return item.Result;
        }

        void EnsureWorker()
        {
            lock (mLock)
            {
                if (mWorkerRunning)
                    return;
                mWorkerRunning = true;
            }

            // 在 AsyncEditor 上启动 worker 协程。worker 内部会一直处理直到队列清空。
            // 注意: 返回的 TtTask 必须通过 AddWaitTask 注册到 TaskCollector,
            //       否则 TtTaskData 无法归还对象池, 见 documents/coding/CodingGuidelines.md §2.2
            TtEngine.Instance.EventPoster.RunOn((Thread.Async.FPostEvent<bool>)((state) =>
            {
                WorkerLoop();
                return true;
            }), Thread.Async.EAsyncTarget.AsyncEditor);
        }

        void WorkerLoop()
        {
            while (true)
            {
                FPendingItem item;
                lock (mLock)
                {
                    if (mPendingItems.Count == 0)
                    {
                        mWorkerRunning = false;
                        return;
                    }
                    item = mPendingItems.Dequeue();
                }

                item.Meta.AutoGenSnapshot().AddWaitTask((task) =>
                {
                    var snapshotTask = (TtTask<bool>)task;
                    item.Result = snapshotTask.DirectResult;
                    item.FinishedSemaphore.Release();
                });
            }
        }
    }

    public class TtSnapshotCreator : IDisposable
    {
        public void Dispose()
        {
            if (Renderer != null)
            {
                Renderer.TickSync();
                Renderer.Dispose();
                Renderer = null;
            }
        }
        Graphics.Pipeline.TtOffscreenRenderer Renderer = new Graphics.Pipeline.TtOffscreenRenderer();
        public uint SnapshotWidth { get; set; } = 256;
        public uint SnapshotHeight { get; set; } = 256;
        public bool SaveAsync { get => !Renderer.DisposeRenderPolicy; set => Renderer.DisposeRenderPolicy = value; }

        public async Thread.Async.TtTask<bool> Initialize(RName renderPolicyName = null)
        {
            if (renderPolicyName == null)
                renderPolicyName = TtEngine.Instance.Config.MainRPolicyName;// RName.GetRName("graphics/deferred.rpolicy", RName.ERNameType.Engine);

            await Renderer.Initialize(renderPolicyName);
            Renderer.SetSize(SnapshotWidth, SnapshotHeight);
            return true;
        }

        public async Thread.Async.TtTask<bool> Snapshot(IO.IAssetMeta assetMeta, bool captureRenderDoc = false)
        {
            if (Renderer == null || assetMeta == null)
                return false;

            var nodes = await assetMeta.GetSnapshotNodes(Renderer);
            var hudElement = assetMeta.GetSnapshotHUD();

            if (SaveAsync)
            {
                var srv = Snapshot(nodes, hudElement, captureRenderDoc);
                if (srv == null)
                    return false;
                TtSnapshot.Save(assetMeta.GetAssetName(), assetMeta, srv, TtSnapshot.ESnapSide.Center, true);
            }
            else
            {
                var image = Snapshot2MemImage(nodes, hudElement, captureRenderDoc);
                if (image == null)
                    return false;

                image = StbImageSharp.ImageProcessor.GetCenterSquare(image);

                if (image.Width > 128)
                {
                    float rate = 128 / (float)image.Width;// pBitmapDesc->Width;
                    int height = (int)((float)image.Height * rate);
                    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, 128, height);
                }

                var file = assetMeta.GetAssetName().Address + ".snap";

                using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate))// .MemoryStream(pBitmapDesc->Stride * pBitmapDesc->Height))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();

                    writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                    assetMeta.ResetSnapshot();
                }
                assetMeta.AddAssetFile(file);
                TtEngine.Instance.SourceControlModule.AddFile(file);
            }

            return true;
        }

        public StbImageSharp.TtMemImage Snapshot2MemImage(List<GamePlay.Scene.TtNode> nodes = null, UI.Controls.TtUIElement hudElement = null, bool captureRenderDoc = false)
        {
            var srv = Snapshot(nodes, hudElement, captureRenderDoc);
            if (srv == null)
                return null;
            return Texture2MemImage(srv);
        }

        public static unsafe StbImageSharp.TtMemImage Texture2MemImage(NxRHI.TtSrView srv)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var tex = srv.GetTexture();
            var fenceDesc = new NxRHI.FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, "NoName");
            var cpDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            var readable = tex.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, cpDraw.mCoreObject);
            TtEngine.Instance.GfxDevice.RenderQueue.QueueCmd((TtRCmdQueue queue, ref NxRHI.FRCmdInfo info) =>
            {
                using (var cmd = new FTransientCmd(EQueueType.QU_Default, "Texture2MemImage"))
                {
                    cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                }
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
                cpDraw.Dispose();
                cpDraw = null;
            }, "Copy Snap Texture");
            fence.Wait(1);
            var gpuDataBlob = new Support.TtBlobObject();
            var bufferData = new Support.TtBlobObject();
            readable.FetchGpuData(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, (IBlobObject)gpuDataBlob.mCoreObject);
            NxRHI.ITexture.BuildImage2DBlob((IBlobObject)bufferData.mCoreObject, (IBlobObject)gpuDataBlob.mCoreObject, tex.Desc);

            byte* pPixelData = (byte*)bufferData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return null;
            var pBitmapDesc = (NxRHI.FPixelDesc*)pPixelData;
            pPixelData += sizeof(NxRHI.FPixelDesc);
            StbImageSharp.TtMemImage image = null;
            var writer = new StbImageWriteSharp.ImageWriter();
            image = StbImageSharp.TtMemImage.FromResult(pPixelData, (int)pBitmapDesc->Width, (int)pBitmapDesc->Height, StbImageSharp.ColorComponents.RedGreenBlueAlpha, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
            return image;
        }

        public NxRHI.TtSrView Snapshot(List<GamePlay.Scene.TtNode> nodes = null, UI.Controls.TtUIElement hudElement = null, bool captureRenderDoc = false)
        {
            if (Renderer == null)
                return null;
            if ((nodes == null || nodes.Count == 0) && hudElement == null)
                return null;

            if (nodes != null && nodes.Count > 0)
            {
                var mergedAABB = DBoundingBox.Empty;
                bool hasValidBounds = false;
                foreach (var node in nodes)
                {
                    node.Parent = Renderer.World.Root;
                    var nodeAABB = node.AABB;
                    if (!nodeAABB.IsEmpty())
                    {
                        if (!hasValidBounds)
                        {
                            mergedAABB = nodeAABB;
                            hasValidBounds = true;
                        }
                        else
                        {
                            mergedAABB = DBoundingBox.Merge(in mergedAABB, in nodeAABB);
                        }
                    }
                }

                if (hasValidBounds)
                {
                    Renderer.RenderPolicy.DefaultCamera.AutoZoom(in mergedAABB);
                }
            }

            if (hudElement != null)
            {
                Renderer.PushHUD(hudElement);
            }

            try
            {
                //warm up
                Renderer.ExecuteRender(true);
                Renderer.TickSync();
                //flush assets
                Thread.TtContextThread.CurrentContext.FlushAllThreadEvents();

                if (captureRenderDoc)
                {
                    TtEngine.Instance.GfxDevice.RenderQueue.CaptureRenderDocFrame = true;
                    TtEngine.Instance.GfxDevice.RenderQueue.BeginFrameCapture();
                }
                
                //real render
                Renderer.ExecuteRender(false);
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.Flush();
                if (captureRenderDoc)
                {
                    TtEngine.Instance.GfxDevice.RenderQueue.EndFrameCapture("Snapshot");
                }

                Renderer.TickSync();
                return Renderer.RenderPolicy.GetFinalShowRSV();
            }
            finally
            {
                if (hudElement != null)
                {
                    Renderer.PopHUD();
                }
            }
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        // 全引擎共享的快照自动生成串行队列, 防止内容浏览器一次性触发大量 AutoGenSnapshot 导致显存暴涨。
        public Editor.TtSnapshotGenQueue SnapshotGenQueue { get; } = new Editor.TtSnapshotGenQueue();
    }
}
