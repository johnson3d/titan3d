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

        // 每个 face 独立的 RenderPolicy. 共用一个 RP 会导致 GetDrawCall 按
        // TargetViewIdentifier 复用 drawcall, 6 面只有第一面的 camera cbuffer
        // 真正生效, 其余 5 面渲染方向全错.
        public TtRenderPolicy[] RenderPolicies { get; private set; }

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
        TtRCmdQueue[] mImmCmdQueues;              // 每个 face RP 独立的 cmd queue
        uint mFaceSize;
        EPixelFormat mCubeFormat = EPixelFormat.PXF_UNKNOWN;  // cube 实际创建时用的 format
        bool mCapturedAtLeastOnce;

        // 一次性 RenderDoc 抓帧标志. 由外层 (TtSceneCubeCapture 的 detail 按钮)
        // 在调用 CaptureCubeFaces 前 set true, CaptureCubeFaces 内部会把
        // BeginFrameCapture/EndFrameCapture 包在整个 6 面循环外, 然后自动复位.
        // 不持续抓 — 抓一帧就够定位问题.
        // 模板见 GpuNodeBase.cs:32-43.
        public bool CaptureRenderDocNextFrame;

        // 签名变化: 接收 6 个独立的 RenderPolicy (每 face 一个).
        // cube format 必须等于 RP final RT 的真实格式 — D3D12 CopyTextureRegion
        // 是物理字节拷贝, bpp 不一致直接崩. 真实格式在第一次拍摄时从
        // RP.RootNode.ColorAttachement.Format 反查, cube 资源延迟创建.
        public bool Initialize(TtWorld world, TtRenderPolicy[] policies, uint faceSize)
        {
            if (policies == null || policies.Length != kFaceCount)
                return false;

            CaptureWorld = world;
            RenderPolicies = policies;

            FaceCameras = new TtCamera[kFaceCount];
            mImmCmdQueues = new TtRCmdQueue[kFaceCount];

            for (int f = 0; f < kFaceCount; f++)
            {
                var rp = RenderPolicies[f];
                if (rp == null)
                    continue;

                rp.IsImmediateFlushCBuffer = true;

                // 每个 RP 只注册自己那一面的 camera, 直接作为 DefaultCamera.
                FaceCameras[f] = new TtCamera();
                rp.AddCamera(kFaceCameraName + f, FaceCameras[f]);
                rp.SetDefaultCamera(kFaceCameraName + f);

                // 每个 RP 独立的 ImmRenderer cmd queue.
                rp.CmdQueue = new TtRCmdQueue();
                mImmCmdQueues[f] = rp.CmdQueue;
            }

            OnResize(faceSize);

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

            ReleaseCubeResources();
            if (RenderPolicies != null)
            {
                for (int f = 0; f < kFaceCount; f++)
                    RenderPolicies[f]?.OnResize(faceSize, faceSize);
            }
        }

        // 标准 cube face 顺序 (与 D3D cubemap subresource 一致):
        //   0: +X    1: -X    2: +Y    3: -Y    4: +Z    5: -Z
        //
        // up_hint 传给 LookAtLH, 通过 cross(up_hint, forward) 推导出 right 轴.
        // ±X/±Z 四个面用标准 (0,1,0) 即可; ±Y 两个面的 up_hint 需要精确匹配
        // cubemap 采样 shader 中 texel UV 布局的 right/up 方向.
        //
        // 推导 (以 shader Slate_TextureCubeViewer.cginc 中的采样公式为准):
        //   +Y face: float3(uv.x*2-1, 1, uv.y*2-1)
        //     → 画面右(+U)=+X, 画面上(-V)=-Z
        //     → 需要 right=(1,0,0), yaxis=(0,0,-1)
        //     → cross(up_hint, (0,1,0))=(1,0,0) → up_hint=(0,0,1) = Forward
        //
        //   -Y face: float3(uv.x*2-1, -1, 1-uv.y*2)
        //     → 画面右(+U)=+X, 画面上(-V)=+Z
        //     → 需要 right=(1,0,0), yaxis=(0,0,1)
        //     → cross(up_hint, (0,-1,0))=(1,0,0) → up_hint=(0,0,1) = Forward
        static readonly Vector3[] kFaceForward =
        {
            Vector3.Right,    // +X
            Vector3.Left,     // -X
            Vector3.Up,       // +Y
            Vector3.Down,     // -Y
            Vector3.Forward,  // +Z
            Vector3.Backward, // -Z
        };
        static readonly Vector3[] kFaceUp =
        {
            Vector3.Up,       // +X face: up = +Y
            Vector3.Up,       // -X face: up = +Y
            Vector3.Backward, // +Y face: up = -Z  (cross((-Z),(+Y))=(+X,0,0) → right=+X)
            Vector3.Forward,  // -Y face: up = +Z  (cross((+Z),(-Y))=(+X,0,0) → right=+X)
            Vector3.Up,       // +Z face: up = +Y
            Vector3.Up,       // -Z face: up = +Y
        };

        /// <summary>
        /// 根据 capture 节点的世界位置更新 6 个 face camera 的 LookAt 和透视投影.
        /// up_hint 经过精确推导, 保证 LookAtLH 的 cross 运算对所有 6 面
        /// 都产生与 D3D cubemap 采样约定一致的 right/yaxis 方向.
        /// </summary>
        public void UpdateFaceCameras(in DVector3 eyePos, uint cubeFaceSize, float nearPlane, float farPlane)
        {
            if (FaceCameras == null)
                return;

            for (int f = 0; f < kFaceCount; f++)
            {
                var fwd = kFaceForward[f];
                var up = kFaceUp[f];
                var lookAt = new DVector3(
                    eyePos.X + fwd.X * 100.0,
                    eyePos.Y + fwd.Y * 100.0,
                    eyePos.Z + fwd.Z * 100.0);
                var cam = FaceCameras[f];
                cam.LookAtLH(in eyePos, in lookAt, in up);
                // FOV 90°, aspect 1:1 — cube 渲染的硬性要求, 不允许配置.
                cam.mCoreObject.PerspectiveFovLH(MathHelper.PI * 0.5f, cubeFaceSize, cubeFaceSize, nearPlane, farPlane);
            }
        }

        // 一次性渲染全部 6 面 + 同步刷 GPU. 调用方 (TtSceneCubeCapture.TickLogic)
        // 已经做完 interval 节流和相机/Volume 更新, 这里只负责"切相机 -> 跑 RP ->
        // copy final RT 到 cube 第 f 面" 这个内循环.
        public void CaptureCubeFaces(float ellapse)
        {
            if (RenderPolicies == null || FaceCameras == null)
                return;

            // RenderDoc 抓帧: 用第一个 RP 的 cmd queue 来 begin/end.
            bool docCapture = CaptureRenderDocNextFrame && mImmCmdQueues?[0] != null;
            if (docCapture)
            {
                mImmCmdQueues[0].CaptureRenderDocFrame = true;
                mImmCmdQueues[0].BeginFrameCapture();
            }

            for (int f = 0; f < kFaceCount; f++)
            {
                var faceRP = RenderPolicies[f];
                var faceQueue = mImmCmdQueues?[f];
                if (faceRP == null)
                    continue;

                faceRP.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.BeginEvent($"BeginFace:{f}");
                }, "BeginFace");

                // 每个 RP 的 DefaultCamera 在 Initialize 时就固定为该面的 camera,
                // 不需要 SetDefaultCamera 切换. 只需刷新 cbPerCamera 到 GPU.
                faceRP.DefaultCamera.UpdateConstBufferData(
                    TtEngine.Instance.GfxDevice.RenderContext,
                    NxRHI.TtCbView.EUpdateMode.Immediately);

                faceRP.BeginTick(CaptureWorld);
                faceRP.Tick(CaptureWorld, null);
                faceRP.EndTick(CaptureWorld);

                faceQueue?.FlushExecute(true);

                if (faceRP.RootNode == null || faceRP.RootNode.ColorAttachement == null)
                {
                    faceRP.TickSync();
                    continue;
                }
                var srcAttach = faceRP.RootNode.ColorAttachement;
                var srcResource = srcAttach.GpuResource;
                if (srcResource == null)
                {
                    faceRP.TickSync();
                    continue;
                }

                EnsureCubeResources(srcAttach.BufferDesc.Format);
                if (mCubeTexture == null)
                {
                    faceRP.TickSync();
                    continue;
                }

                CopyFinalRTToCubeFace(faceRP, faceQueue, srcResource, f);

                faceRP.QueueCmd((TtRCmdQueue queue, ref FRCmdInfo info) =>
                {
                    TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.EndEvent($"EndFace:{f}");
                }, "EndFace");

                faceQueue?.FlushExecute(true);
                faceRP.TickSync();
            }

            // 最终 flush, 保证 cube 6 面对下游立即可见.
            for (int f = 0; f < kFaceCount; f++)
                mImmCmdQueues?[f]?.FlushExecute(true);

            if (docCapture)
            {
                mImmCmdQueues[0].EndFrameCapture("CubeCapture");
                CaptureRenderDocNextFrame = false;
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
        void CopyFinalRTToCubeFace(TtRenderPolicy faceRP, TtRCmdQueue faceQueue, TtGpuResource srcTex, int face)
        {
            if (mCopyDraw == null || mCubeTexture == null)
                return;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var cmdlist = NxRHI.TtCommandList.GetCmdList();
            using (new TtCmdListScope(cmdlist, "CubeCapture.CopyFace"))
            {
                mCopyDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                mCopyDraw.BindSrc(srcTex);
                mCopyDraw.BindDest(mCubeTexture);
                mCopyDraw.SrcSubResource = 0;
                mCopyDraw.DestSubResource = (uint)face;
                mCopyDraw.DstX = 0;
                mCopyDraw.DstY = 0;
                mCopyDraw.DstZ = 0;

                cmdlist.PushGpuDraw(mCopyDraw);
                cmdlist.FlushDraws();
            }
            faceRP?.CommitCommandList(cmdlist, "CubeCapture.CopyFace");
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
            // mImmCmdQueues 也指向各 RP.CmdQueue, RP 内部会 Dispose 它们.
            mImmCmdQueues = null;
            RenderPolicies = null;
            CaptureWorld = null;
        }
    }
}
