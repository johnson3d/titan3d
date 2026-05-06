using System;

namespace EngineNS.NxRHI
{
    /// <summary>
    /// GPU → CPU 异步回读的公共实现。
    ///
    /// 抽出来是因为 <see cref="TtBuffer.AsyncFetchGpuData"/> 和 <see cref="TtTexture.AsyncFetchGpuData"/>
    /// 走的是同一条低层路径, 区别只在 "怎么 CreateReadable" 一句:
    ///   - TtBuffer:  mCoreObject.CreateReadable(rc, (int)subRes, cpDraw)
    ///   - TtTexture: mCoreObject.CreateReadable(rc, (int)subRes, cpDraw)  (签名相同, 但走的是 ITexture 端)
    ///
    /// 调用方通过 <paramref name="createReadable"/> 委托提供这一句, helper 负责剩下的:
    ///   1) 把 cpDraw 通过 FTransientCmd 提交到默认队列
    ///   2) IncreaseSignal(fence)
    ///   3) 在 EventPoster TPools 工作线程上 fence.Wait(1)  (不阻塞调用线程, 也不阻塞渲染线程)
    ///   4) FetchGpuData 把 staging 数据拷到 blob
    ///   5) semaphore.Release() 唤醒调用线程
    ///   6) cleanup (cpDraw / readable / fence / semaphore)
    ///
    /// ⚠️ <b>调用方注意 blob 头部布局</b>: native 端 <c>IBuffer::FetchGpuData</c> /
    /// <c>ITexture::FetchGpuData</c> 都会先 PushData 两个 UINT (RowPitch + DepthPitch)
    /// 再 PushData 真正的 raw bytes, 所以 <c>blob.Size == 8 + Desc.Size</c>, 真数据
    /// 从 <c>(byte*)blob.DataPointer + 8</c> 开始。Texture 路径还需要按 RowPitch 走
    /// 行 stride (GPU 会做 cache line padding, 不等于 width * pixel_size)。详见
    /// <c>Documents/Coding/CodingGuidelines.md §1.5.3</c> 规则 5。
    ///
    /// 详细规约见 <c>Documents/Coding/CodingGuidelines.md §1.5.3.1</c> 和 <c>§1.6 (EventPoster)</c>。
    /// </summary>
    internal static class TtAsyncReadback
    {
        /// <summary>
        /// 执行一次完整的 readback 流程。
        /// </summary>
        /// <param name="createReadable">
        /// 调用方提供: 给定一个空白 cpDraw, 返回为它配套的 staging IBuffer。
        /// 内部会自动调 native CreateReadable, 把 GpuAccess 资源 → CpuAccess staging 资源的
        /// copy command record 进 cpDraw。helper 后续只负责把 cpDraw 提交并 fence wait。
        /// </param>
        /// <param name="blob">调用方分配的 blob, await 返回 true 时数据已就绪。</param>
        /// <param name="debugName">debug 标记, 同时用于 fence 和 FTransientCmd 的命名。</param>
        public static async Thread.Async.TtTask<bool> RunReadbackAsync(
            Func<NxRHI.ICopyDraw, NxRHI.IBuffer> createReadable,
            EngineNS.IBlobObject blob,
            string debugName)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            // 1) 准备 fence + cpDraw + readable staging buffer
            var fenceDesc = new FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, debugName);
            var cpDraw = rc.CreateCopyDraw();
            var readable = createReadable(cpDraw.mCoreObject);

            // 2) semaphore: 由 TPools 工作线程在完成时 Release, 调用线程通过 Await 非阻塞挂起
            var semaphore = Thread.TtSemaphore.CreateSemaphore(1);

            bool result = false;

            // 3) 投递到 EventPoster TPools 工作线程, 在那里做 fence.Wait + FetchGpuData
            //    不能在调用线程做 fence.Wait, 否则会阻塞渲染/逻辑线程
            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                try
                {
                    using (var cmd = new FTransientCmd(EQueueType.QU_Default, debugName))
                    {
                        cmd.CmdList.PushGpuDraw(cpDraw.mCoreObject);
                    }
                    rc.GpuQueue.IncreaseSignal(fence);
                    fence.Wait(1);
                    result = readable.FetchGpuData(rc.mCoreObject, 0, blob);
                    return true;
                }
                catch (Exception e)
                {
                    result = false;
                    Profiler.Log.WriteLineSingle("[TtAsyncReadback:" + debugName + "] " + e.ToString());
                    return false;
                }
                finally
                {
                    semaphore.Release();
                }
            }, Thread.Async.EAsyncTarget.TPools);

            // 4) 调用线程: 非阻塞挂起, 等 TPools 完成
            await semaphore.Await();
            semaphore.FreeSemaphore();

            // 5) 资源清理: cpDraw 是 TtAuxPtr; readable 是 native IBuffer (Dispose 会 Release)
            cpDraw.Dispose();
            readable.Dispose();
            fence.Dispose();
            return result;
        }
    }
}
