using System;
using System.ComponentModel;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Common.ColorGrading;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("PostProcessVolume", "Graphics\\PostProcessVolume", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPostProcessVolumeNode.TtPostProcessVolumeData), DefaultNamePrefix = "PostProcessVolume")]
    [Rtti.Meta("")]
    public partial class TtPostProcessVolumeNode : TtVolumeBaseNode
    {
        [Rtti.Meta("")]
        public class TtPostProcessVolumeData : TtVolumeBaseData
        {
            internal TtPostProcessVolumeNode HostNode;

            TtColorGradingSettings mColorGradingSettings = new TtColorGradingSettings();
            [Category("ColorGrading")]
            [Rtti.Meta("")]
            public TtColorGradingSettings ColorGradingSettings
            {
                get => mColorGradingSettings;
                set => mColorGradingSettings = value ?? new TtColorGradingSettings();
            }

            [Category("ColorGrading")]
            [Rtti.Meta("")]
            public bool EnableColorGrading { get; set; } = true;
        }

        // ── Per-Volume LUT generator: lazily created on first use, released after idle ──
        TtColorGradingLUTGenerator mLutGenerator;
        long mLastUsedFrame = -1;

        /// <summary>
        /// Number of frames a LUT stays alive after last use before being released.
        /// 32³×RGBA8 = 128 KB per LUT — releasing idle ones keeps peak VRAM low.
        /// </summary>
        const int LutIdleFrameThreshold = 120; // ~2 seconds at 60 fps

        /// <summary>
        /// The SRV of this Volume's 3D LUT texture. null if LUT is not allocated or color grading disabled.
        /// </summary>
        public NxRHI.TtSrView LutSrv => mLutGenerator?.LutSrv;

        /// <summary>
        /// Returns true when the LUT generator is ready and has a valid LUT texture.
        /// </summary>
        public bool IsLutReady => mLutGenerator?.IsInitialized == true;

        protected override async Thread.Async.TtTask<bool> InitializeNode(
            TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
                data = new TtPostProcessVolumeData();

            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<TtPostProcessVolumeData>().HostNode = this;

            // LUT generator is NOT created here — lazy-init in EnsureLutUpToDate
            return ret;
        }

        [Category("Option")]
        public TtPostProcessVolumeData PostProcessData
        {
            get => GetNodeData<TtPostProcessVolumeData>();
        }

        public override float ComputeInfluence(in DVector3 cameraWorldPos)
        {
            var ppData = PostProcessData;
            if (ppData == null || !ppData.EnableColorGrading)
                return 0f;
            return base.ComputeInfluence(in cameraWorldPos);
        }

        /// <summary>
        /// Ensures this Volume's LUT is up-to-date. Lazily creates the LUT generator on first use.
        /// Only dispatches compute if settings changed. Called by HdrNode each frame for active volumes.
        /// </summary>
        public void EnsureLutUpToDate(TtRenderPolicy policy)
        {
            var ppData = PostProcessData;
            if (ppData == null || !ppData.EnableColorGrading)
                return;

            // Lazy-init: allocate LUT texture only when the volume is actually near the camera
            if (mLutGenerator == null)
            {
                mLutGenerator = new TtColorGradingLUTGenerator();
                mLutGenerator.Initialize().AddWaitTask();
            }

            mLutGenerator.UpdateIfDirty(ppData.ColorGradingSettings, policy);
            mLastUsedFrame = TtEngine.Instance.FrameCount;
        }

        /// <summary>
        /// Release the LUT texture if this volume hasn't been used for LutIdleFrameThreshold frames.
        /// Called periodically (e.g. from HdrNode or world tick) to reclaim VRAM.
        /// </summary>
        public void ReleaseLutIfStale()
        {
            if (mLutGenerator == null)
                return;

            long currentFrame = TtEngine.Instance.FrameCount;
            if (currentFrame - mLastUsedFrame > LutIdleFrameThreshold)
            {
                mLutGenerator.Dispose();
                mLutGenerator = null;
            }
        }

        public override void Dispose()
        {
            mLutGenerator?.Dispose();
            mLutGenerator = null;
            base.Dispose();
        }

        public static async Thread.Async.TtTask<TtPostProcessVolumeNode> AddPostProcessVolumeNode(
            TtWorld world, TtNode parent, TtPostProcessVolumeData data, DVector3 pos, Vector3 extents)
        {
            var scene = parent.GetNearestParentScene();
            var node = await TtNode.SpawnNode<TtPostProcessVolumeNode>(
                parent, null, data, EBoundVolumeType.Box, typeof(TtPlacement)) as TtPostProcessVolumeNode;

            node.Placement.SetTransform(in pos, in Vector3.One, in Quaternion.Identity);
            node.BoundVolume.LocalAABB = new BoundingBox(-extents, extents);
            return node;
        }
    }
}
