using EngineNS.Graphics.Pipeline;
using EngineNS.NxRHI;
using System;

namespace EngineNS.GamePlay.Scene
{
    // -------------------------------------------------------------------------
    // TtCubeWorldRenderer
    //   TtSceneCubeCapture 内部使用的"6 面 cube 渲染封装". 不复用
    //   TtWorldRenderer 是因为 cube 的 6 个相机 + 6 次 RP 调度 + cube 资源管理
    //   语义和 2D scene capture 差太多, 强行继承会让 base class 充满 if/else.
    //
    // 对外契约 (被 TtSceneCubeCapture 直接调用):
    //   - new TtCubeWorldRenderer()                            : 无参构造, 全部资源延迟到 Initialize
    //   - Initialize(world, policy, faceSize, hdr) -> bool     : 初始化 6 相机 + 1 cube 纹理 + 1 cube SRV + cmd queue
    //   - OnResize(uint faceSize)                              : 单面分辨率改了, 重建 cube 资源 + RP.OnResize
    //   - CaptureCubeFaces(float ellapse)                      : 一次性渲染全部 6 面, 同步刷 GPU. 要在 TickLogic 里被节流后调用
    //   - Dispose()                                            : 释放 cube 资源 + cmd queue + 6 cameras. 注意: RP 的生命周期归
    //     调用方 (TtSceneCubeCapture) 管, 这里不释放
    //   - 字段: RenderPolicy / CaptureWorld / FaceCameras[6] / CubeSrv / IsReady
    //
    // 资源生命周期约束:
    //   1. cube Texture/SRV 在第一次 CaptureCubeFaces 之前由 EnsureCubeResources() 延迟创建
    //   2. OnResize 时 cube SRV / cube Texture 都重建, RP.OnResize 同步
    //   3. RP 是上层传进来的 (上层 capture 节点 ConstructCaptureWorld 时 new 出来的),
    //      由上层负责 Dispose; 这里只释放自己持有的: cube 资源 + CopyDraw + 6 cameras
    //
    // 与 RP final RT 拷贝路径:
    //   每面跑完 RP 后, GetFinalShowRSV() 拿到 root node 的 ColorAttachement,
    //   它的 GpuResource (TtTexture) 就是源, 用 TtCopyDraw 通过
    //   DestSubResource = face 拷到 cube 的对应 array slice
    //   (cube 1 mip 时 sub-resource 索引 = mip * arraySize + face = face).
    // -------------------------------------------------------------------------
    public class TtCubeWorldRenderer : IDisposable
    {
        public const int kFaceCount = 6;

        // 给 RP 的 6 个 cube 面相机, 在 RP.CameraAttachments 里登记的名字前缀.
        // CaptureCubeFaces 用 SetDefaultCamera(kFaceCameraName + f) 切换面.
        public const string kFaceCameraName = "CubeCaptureFace_";

        public TtWorld CaptureWorld { get; private set; }
        public TtRenderPolicy RenderPolicy { get; private set; }

        // 6 个相机, 每面一个. 由 TtSceneCubeCapture.UpdateCubeCameras 负责 LookAt/PerspectiveFov.
        public TtCamera[] FaceCameras { get; private set; }

        // Cube SRV, 给下游 (水面反射等) 直接采的最终产物. 在 EnsureCubeResources
        // 创建后保持不变, 直到 OnResize / Dispose.
        public TtSrView CubeSrv => mCubeSrv;
        public bool IsReady => mCubeSrv != null && mCapturedAtLeastOnce;

        // ---------- 内部资源 ----------
        TtTexture mCubeTexture;
        TtSrView mCubeSrv;
        TtCopyDraw mCopyDraw;                     // 复用一份, 每面切 DestSubResource 后 push
        TtRCmdQueue mImmCmdQueue;                 // ImmRenderer 风格: 每次 CaptureCubeFaces 同步刷 GPU
        uint mFaceSize;
        EPixelFormat mCubeFormat = EPixelFormat.PXF_UNKNOWN;  // cube 实际创建时用的 format
        bool mCapturedAtLeastOnce;

        // 一次性 RenderDoc 抓帧标志. 由外层 (TtSceneCubeCapture 的 detail 按钮)
        // 在调用 CaptureCubeFaces 前 set true, CaptureCubeFaces 内部会把
        // BeginFrameCapture/EndFrameCapture 包在整个 6 面循环外, 然后自动复位.
        // 不持续抓 — 抓一帧就够定位问题.
        // 模板见 GpuNodeBase.cs:32-43.
        public bool CaptureRenderDocNextFrame;

        // 注意签名变化: 去掉了 captureHDR 参数. cube format 必须等于 RP final RT
        // 的真实格式 — D3D12 CopyTextureRegion 是物理字节拷贝, 要求源/目的 bpp
        // 完全一致, 用户随便选 HDR/LDR 而不管 RP 真实输出格式 = 必崩.
        // 真实格式从 RenderPolicy.RootNode.ColorAttachement.Format 在第一次拍摄时
        // 反查 (那时 RP 已经跑过一次 Tick, attachment 才被建出来), 因此 cube
        // 资源不在 Initialize 创建, 而是延迟到 CaptureCubeFaces 第一面拷贝前.
        public bool Initialize(TtWorld world, TtRenderPolicy policy, uint faceSize)
        {
            CaptureWorld = world;
            RenderPolicy = policy;
            RenderPolicy.IsImmediateFlushCBuffer = true;
            //mFaceSize = faceSize;
            OnResize(faceSize);

            FaceCameras = new TtCamera[kFaceCount];
            for (int f = 0; f < kFaceCount; f++)
            {
                FaceCameras[f] = new TtCamera();
                // 把 6 面相机登记到 RP.CameraAttachments. RP.DefaultCamera 是只读
                // property, 必须用 SetDefaultCamera(name) 走 CameraAttachments 切换.
                RenderPolicy?.AddCamera(kFaceCameraName + f, FaceCameras[f]);
            }

            // 用 ImmRenderer 风格的 RCmdQueue: CaptureCubeFaces 一次性提交 6 面 +
            // 同步 FlushExecute, 这样 cube SRV 在 TickLogic 返回时已经填好,
            // 下游同帧就能消费. 与 TtWorldImmRenderer.Initialize (SceneCapture.cs:46) 同款做法.
            if (RenderPolicy != null)
                RenderPolicy.CmdQueue = new TtRCmdQueue();
            mImmCmdQueue = RenderPolicy?.CmdQueue;

            mCopyDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            mCopyDraw.SetDebugName("CubeCapture.FaceCopy");

            // cube 资源延迟到 CaptureCubeFaces 创建 (需要先知道 RP final RT 的格式).
            return true;
        }

        public void OnResize(uint faceSize)
        {
            if (mFaceSize == faceSize)
                return;
            mFaceSize = faceSize;

            // 只释放 cube 资源, 不重建 — 等下次 CaptureCubeFaces 跑完 RP 后,
            // 再用真实的 RP final RT format 重建 cube (避免格式漂移).
            ReleaseCubeResources();
            RenderPolicy?.OnResize(faceSize, faceSize);
        }

        // 一次性渲染全部 6 面 + 同步刷 GPU. 调用方 (TtSceneCubeCapture.TickLogic)
        // 已经做完 interval 节流和相机/Volume 更新, 这里只负责"切相机 -> 跑 RP ->
        // copy final RT 到 cube 第 f 面" 这个内循环.
        public void CaptureCubeFaces(float ellapse)
        {
            if (RenderPolicy == null || FaceCameras == null)
                return;

            // cube 资源在这里延迟创建, 因为要等 RP 跑过一次拿到 final RT 真实
            // format. 第一面循环里 RP.Tick 完成后会触发 EnsureCubeResources(format).

            // 一次性 RenderDoc 抓帧: 包住整个 6 面循环 (含每面的 FlushExecute
            // 和拷贝 + 末尾的 final flush). 抓帧目标必须是真正提交 GPU 的那条
            // cmd queue, 也就是本 renderer 自己的 mImmCmdQueue (ImmRenderer 模式
            // 下就是 RenderPolicy.CmdQueue). 模板: GpuNodeBase.cs:32-43.
            bool docCapture = CaptureRenderDocNextFrame && mImmCmdQueue != null;
            if (docCapture)
            {
                mImmCmdQueue.CaptureRenderDocFrame = true;
                mImmCmdQueue.BeginFrameCapture();
            }

            for (int f = 0; f < kFaceCount; f++)
            {
                RenderPolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent($"BeginFace:{f}");
                }, "BeginFace");
                // 1) 切到第 f 面的相机 (PerspectiveFov / LookAt 已由 TtSceneCubeCapture
                //    在 Tick 之前刷新过). 走 RP.SetDefaultCamera(name).
                RenderPolicy.SetDefaultCamera(kFaceCameraName + f);

                // 2) 跑一次 RP. 与 TtWorldRenderer.TickLogic (SceneCapture.cs:30-32) 一致.
                RenderPolicy.BeginTick(CaptureWorld);
                RenderPolicy.Tick(CaptureWorld, null);
                RenderPolicy.EndTick(CaptureWorld);

                // 3) 同步 flush GPU, 让 RP 的 final RT 到达可读状态. ImmRenderer
                //    模式: 每面提交后立即 flush, 避免后续 CopyDraw 读到上一帧的
                //    final RT (RP 自己的内部 RT 是单 buffer 的, 不 flush 会被
                //    下一面渲染覆盖).
                mImmCmdQueue?.FlushExecute(true);

                // 4) 拿 RP 根节点的 ColorAttachement.GpuResource 作为拷贝源.
                //    走这个路径而不是 GetFinalShowRSV().StreamingTexture, 是因为后者
                //    只对 TtTextureManager 流式加载的纹理有效, RP 的 final SRV 上为 null.
                //    Copy2SwapChainNode.cs:140-143 也是直接拿 attachBuffer.GpuResource.
                if (RenderPolicy.RootNode == null || RenderPolicy.RootNode.ColorAttachement == null)
                {
                    RenderPolicy.TickSync();
                    continue;
                }
                var srcAttach = RenderPolicy.RootNode.ColorAttachement;
                var srcResource = srcAttach.GpuResource;
                if (srcResource == null)
                {
                    RenderPolicy.TickSync();
                    continue;
                }

                // 5) 用 RP final RT 的真实 format 创建 cube. 必须在拷贝前确保 cube
                //    存在且 format 一致 — D3D12 CopyTextureRegion 是物理字节拷贝,
                //    源/目的 bpp 不一致会硬报错 #874 COPYTEXTUREREGION_FORMATMISMATCH.
                //    例如 RP final = R8G8B8A8 (32bpp), cube 若按用户随手勾的 HDR
                //    建成 R16G16B16A16 (64bpp), 拷贝直接崩.
                // TtAttachBuffer 自身没有 Format 字段, 真实定义在 GraphicsBuffers.cs:86
                // 只暴露 BufferDesc / GpuResource / Texture / Rtv/Srv. format 在
                // BufferDesc.Format (FAttachBufferDesc.Format, EPixelFormat).
                EnsureCubeResources(srcAttach.BufferDesc.Format);
                if (mCubeTexture == null)
                {
                    RenderPolicy.TickSync();
                    continue;
                }

                CopyFinalRTToCubeFace(srcResource, f);

                RenderPolicy.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent($"EndFace:{f}");
                }, "EndFace");

                mImmCmdQueue?.FlushExecute(true);
                RenderPolicy.TickSync();
            }

            // 拷贝完 6 面后再 flush 一次, 保证 cube 6 面对下游消费者立即可见.
            mImmCmdQueue?.FlushExecute(true);

            // 收尾 RenderDoc 抓帧 — 在最后一次 FlushExecute 之后, 确保所有
            // GPU 命令都被记录进 .rdc. EndFrameCapture 的 name 参数会成为
            // RenderDoc UI 里 capture 列表的标题, 用节点 debug 名方便区分.
            if (docCapture)
            {
                mImmCmdQueue.EndFrameCapture("CubeCapture");
                CaptureRenderDocNextFrame = false;   // 一次性, 不持续抓
            }

            mCapturedAtLeastOnce = true;
        }

        // ---------- Internal: cube 资源管理 ----------
        // format 由调用方传入 — 必须等于 RP final RT 的真实 format, 见
        // CaptureCubeFaces 第 5 步注释. 如果 cube 已存在且 format 一致, 直接复用;
        // format 变了 (比如 RP 切了一个), 自动重建.
        void EnsureCubeResources(EPixelFormat format)
        {
            if (format == EPixelFormat.PXF_UNKNOWN)
                return;                                       // RP attachment format 还没 ready, 等下一帧
            if (mCubeTexture != null && mCubeFormat == format)
                return;                                       // 复用现有 cube
            if (mCubeTexture != null)
                ReleaseCubeResources();                       // format 变了, 重建

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            // ① cube Texture: ArraySize=6, 1 mip, BindFlags = SRV only.
            //    --- 为什么不显式设 CubeFaces ---
            //    FTextureDesc 没有 CubeFaces 字段 (那是 TtPicDesc 上的, 用于
            //    PNG/DDS 资产元数据, 见 Texture.cs:331). 运行时 GPU 资源是
            //    ArraySize=6 + ESrvType.ST_TextureCube SRV 这一对组合让 RHI
            //    把它当 cube 用, D3D12 层就只看 ArraySize+SRV.ViewDimension,
            //    不需要 desc 上额外标志.
            //    --- MipLevels / ArraySize 不能省 ---
            //    SetDefault() 之后必须显式指定, 否则带 RTV/UAV 的纹理在 D3D12
            //    会报 DXGI_ERROR_INVALID_CALL (DenoiseNode.cs:325 同款注释).
            //    --- Format 必须等于 RP final RT 真实格式 ---
            //    CopyTextureRegion 是物理字节拷贝, bpp 不一致直接崩
            //    (#874 COPYTEXTUREREGION_FORMATMISMATCH). 因此格式不能由用户
            //    随手勾选, 而是从 RP 的 ColorAttachement.BufferDesc.Format 反查.
            //    --- BindFlags 只留 SRV ---
            //    cube 不直接当 RTV 写, 走 "RP final RT -> CopyDraw 到 cube
            //    第 f 面" 路径. 加 BFT_RTV 也不会用上.
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = mFaceSize;
            texDesc.Height = mFaceSize;
            texDesc.MipLevels = 1;
            texDesc.ArraySize = 6;
            texDesc.Format = format;
            texDesc.BindFlags = EBufferType.BFT_SRV;
            mCubeTexture = rc.CreateTexture(in texDesc);
            if (mCubeTexture == null)
                return;                                       // 创建失败直接返回, 不要继续建 SRV
            mCubeTexture.SetDebugName("CubeCapture.Cube");
            mCubeFormat = format;

            // ② cube SRV (TextureCube view, 给下游水面 / IBL 直接采).
            //    union 字段填法严格对齐 DenoiseNode.cs:347-349 的告警:
            //    "SetTexture2D() 后必须填 MipLevels / MostDetailedMip, 否则
            //    D3D12 报 DXGI_ERROR_INVALID_CALL". cube 同理, MostDetailedMip
            //    缺省值不可信, 必须显式 = 0.
            //    SetTexture2D() 是 reset union 用的 (仓库里没有 SetTextureCube),
            //    然后切 Type = ST_TextureCube + 填 TextureCube union.
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_TextureCube;
            srvDesc.Format = texDesc.Format;
            srvDesc.TextureCube.MipLevels = 1;
            srvDesc.TextureCube.MostDetailedMip = 0;
            mCubeSrv = rc.CreateSRV(mCubeTexture, in srvDesc);
            if (mCubeSrv != null)
                mCubeSrv.SetDebugName("CubeCapture.CubeSrv");
        }

        void ReleaseCubeResources()
        {
            CoreSDK.DisposeObject(ref mCubeSrv);
            CoreSDK.DisposeObject(ref mCubeTexture);
            mCubeFormat = EPixelFormat.PXF_UNKNOWN;
            mCapturedAtLeastOnce = false;
        }

        // 把 RP 的 final RT (单张 2D 纹理, sub=0) 拷到 cube 的第 face 面 (sub=face).
        // 与 Copy2SwapChainNode.cs:140-153 同款 TtCopyDraw 用法, 但额外指定
        // SrcSubResource / DestSubResource 选取 cube 的 array slice.
        // 形参类型用接口 TtGpuResource (Buffer.cs:76) 而不是 TtTexture, 因为
        // ColorAttachement.GpuResource 真实类型就是接口 TtGpuResource —
        // TtTexture / TtBuffer 等具体类都实现该接口. TtCopyDraw.BindSrc/BindDest
        // 也接同一接口, 不需要向下转型.
        void CopyFinalRTToCubeFace(TtGpuResource srcTex, int face)
        {
            if (mCopyDraw == null || mCubeTexture == null)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = rc.CmdListManager.GetCmdList();
            using (new TtCmdListScope(cmdlist, "CubeCapture.CopyFace"))
            {
                mCopyDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                mCopyDraw.BindSrc(srcTex);
                mCopyDraw.BindDest(mCubeTexture);
                mCopyDraw.SrcSubResource = 0;                  // RP final RT: 单 mip / 单 array slice
                mCopyDraw.DestSubResource = (uint)face;        // cube: mip=0, faceSlice=face -> sub = face
                mCopyDraw.DstX = 0;
                mCopyDraw.DstY = 0;
                mCopyDraw.DstZ = 0;

                cmdlist.PushGpuDraw(mCopyDraw);
                cmdlist.FlushDraws();
            }
            RenderPolicy?.CommitCommandList(cmdlist, "CubeCapture.CopyFace");
        }

        public void Dispose()
        {
            ReleaseCubeResources();
            CoreSDK.DisposeObject(ref mCopyDraw);

            if (FaceCameras != null)
            {
                for (int i = 0; i < FaceCameras.Length; i++)
                    FaceCameras[i] = null;
                FaceCameras = null;
            }

            // RP 是上层 (TtSceneCubeCapture) 持有 + 释放的, 这里只解引用.
            // mImmCmdQueue 也指向 RP.CmdQueue, RP 内部会 Dispose 它.
            mImmCmdQueue = null;
            RenderPolicy = null;
            CaptureWorld = null;
        }
    }
}
