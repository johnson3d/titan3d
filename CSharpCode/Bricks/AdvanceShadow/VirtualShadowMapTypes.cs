using System;
using System.Runtime.InteropServices;

namespace EngineNS.Bricks.AdvanceShadow
{
    /// <summary>
    /// Physical page states in the page pool
    /// </summary>
    public enum EVSMPageState : int
    {
        Free = 0,
        Allocated = 1,
        Cached = 2,
    }

    /// <summary>
    /// Page request flags written by the MarkPages compute shader
    /// </summary>
    [Flags]
    public enum EVSMPageRequestFlags : uint
    {
        None = 0,
        Requested = 1 << 0,
        StaticCached = 1 << 1,
        DynamicDirty = 1 << 2,
    }

    /// <summary>
    /// GPU-side page table entry: maps virtual page index → physical page coordinates in the pool atlas.
    /// Stored in a StructuredBuffer consumed by the shadow projection shader.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVSMPageTableEntry")]
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct FVSMPageTableEntry
    {
        /// <summary>
        /// Physical page coordinates (X, Y) in the page pool atlas.
        /// (-1, -1) means unmapped / not allocated.
        /// </summary>
        public Vector2i mPhysicalPageCoord;

        /// <summary>
        /// Frame number when this page was last rendered.
        /// Used for LRU eviction.
        /// </summary>
        public uint mLastRenderedFrame;

        /// <summary>
        /// Combined flags (EVSMPageRequestFlags).
        /// </summary>
        public uint mFlags;

        // ---- Property wrappers (not exported to shader) ----
        public Vector2i PhysicalPageCoord { get => mPhysicalPageCoord; set => mPhysicalPageCoord = value; }
        public uint LastRenderedFrame { get => mLastRenderedFrame; set => mLastRenderedFrame = value; }
        public uint Flags { get => mFlags; set => mFlags = value; }
        public void MarkDynamicDirty()
        {
            mFlags |= (uint) EVSMPageRequestFlags.DynamicDirty;
        }
        public static FVSMPageTableEntry CreateUnmapped()
        {
            return new FVSMPageTableEntry
            {
                mPhysicalPageCoord = new Vector2i(-1, -1),
                mLastRenderedFrame = 0,
                mFlags = 0,
            };
        }

        public bool IsMapped => mPhysicalPageCoord.X >= 0 && mPhysicalPageCoord.Y >= 0;
    }

    /// <summary>
    /// Per-physical-page metadata tracked on CPU side for allocation/eviction.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVSMPhysicalPageInfo")]
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct FVSMPhysicalPageInfo
    {
        /// <summary>
        /// Which virtual page index this physical page is currently backing.
        /// -1 if free.
        /// </summary>
        public int mVirtualPageIndex;

        /// <summary>
        /// Frame number when this page was last requested by screen pixels.
        /// </summary>
        public uint mLastRequestedFrame;

        /// <summary>
        /// Current state of this page.
        /// </summary>
        public int mState;

        /// <summary>
        /// Padding for 16-byte alignment.
        /// </summary>
        public int mPadding;

        // ---- Property wrappers ----
        public int VirtualPageIndex { get => mVirtualPageIndex; set => mVirtualPageIndex = value; }
        public uint LastRequestedFrame { get => mLastRequestedFrame; set => mLastRequestedFrame = value; }
        public int State { get => mState; set => mState = value; }

        public bool IsFree => mState == (int)EVSMPageState.Free;
    }

    /// <summary>
    /// Configuration for the virtual shadow map page pool.
    /// </summary>
    public struct FVSMPoolConfig
    {
        /// <summary>
        /// Resolution of each physical page in texels (e.g., 128 = 128x128).
        /// </summary>
        public int PageResolution;

        /// <summary>
        /// Number of pages per row/column in the physical atlas (e.g., 16 = 16x16 = 256 pages).
        /// </summary>
        public int PoolDimPages;

        /// <summary>
        /// Total available physical pages = PoolDimPages * PoolDimPages.
        /// </summary>
        public int TotalPhysicalPages => PoolDimPages * PoolDimPages;

        /// <summary>
        /// Physical atlas texture resolution = PageResolution * PoolDimPages.
        /// </summary>
        public int PoolTextureResolution => PageResolution * PoolDimPages;

        public static FVSMPoolConfig CreateDefault()
        {
            return new FVSMPoolConfig
            {
                PageResolution = 128,
                PoolDimPages = 16, // 16x16 = 256 physical pages
            };
        }
    }

    /// <summary>
    /// Per-clipmap-page data uploaded to GPU each frame via StructuredBuffer.
    /// Contains the ortho view-projection matrix and near/far for ESM depth linearization.
    /// Indexed by local page index: level * PagesPerDim^2 + pageY * PagesPerDim + pageX.
    /// </summary>
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FVSMClipmapPageData")]
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct FVSMClipmapPageData
    {
        /// <summary>
        /// Ortho view-projection matrix for this page's shadow camera.
        /// Used to transform receiver world position → clip space for depth comparison.
        /// </summary>
        public Matrix mViewProj;

        /// <summary>
        /// Near plane distance of the page camera (typically 1.0).
        /// </summary>
        public float mZNear;

        /// <summary>
        /// Far plane distance of the page camera.
        /// </summary>
        public float mZFar;

        /// <summary>
        /// Padding for 16-byte alignment.
        /// </summary>
        public float mPad0;
        public float mPad1;
    }
}
