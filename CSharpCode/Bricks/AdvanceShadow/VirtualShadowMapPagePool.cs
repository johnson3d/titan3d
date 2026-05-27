using System;
using System.Collections.Generic;
using EngineNS.NxRHI;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.AdvanceShadow
{
    /// <summary>
    /// Manages a shared physical page pool atlas and virtual-to-physical page table.
    /// Inspired by UE5's Virtual Shadow Map architecture: all shadow pages share one large
    /// physical atlas texture, with a page table providing virtual→physical mapping.
    /// Pages are allocated on demand and evicted via LRU when the pool is full.
    /// </summary>
    public class TtVSMPagePool : IDisposable
    {
        private FVSMPoolConfig mConfig;
        private FVSMPageTableEntry[] mPageTable;
        private FVSMPhysicalPageInfo[] mPhysicalPages;
        private Stack<int> mFreeList;
        private uint mCurrentFrame;

        /// <summary>
        /// The physical atlas depth texture (D16_UNORM, PoolTextureResolution x PoolTextureResolution).
        /// All shadow pages are rendered into sub-regions of this single large texture.
        /// </summary>
        public TtTexture PhysicalPoolTexture;

        /// <summary>
        /// SRV for sampling the physical pool in projection shaders.
        /// </summary>
        public TtSrView PhysicalPoolSRV;

        /// <summary>
        /// DSV for rendering depth into the physical pool (used during shadow rendering).
        /// </summary>
        public TtDepthStencilView PhysicalPoolDSV;

        /// <summary>
        /// GPU-side page table buffer (StructuredBuffer&lt;FVSMPageTableEntry&gt;).
        /// Consumed by the shadow projection shader to look up physical coordinates.
        /// </summary>
        public TtCpu2GpuBuffer<FVSMPageTableEntry> PageTableBuffer;

        /// <summary>
        /// Maximum number of virtual pages this pool supports (= QTree total node count).
        /// </summary>
        public int MaxVirtualPages => mPageTable?.Length ?? 0;

        /// <summary>
        /// Pool configuration.
        /// </summary>
        public ref readonly FVSMPoolConfig Config => ref mConfig;

        /// <summary>
        /// Number of currently allocated (non-free) physical pages.
        /// </summary>
        public int AllocatedPageCount => mConfig.TotalPhysicalPages - mFreeList.Count;

        /// <summary>
        /// Number of free physical pages remaining.
        /// </summary>
        public int FreePageCount => mFreeList.Count;

        public void Initialize(int maxVirtualPages, in FVSMPoolConfig config)
        {
            mConfig = config;
            mCurrentFrame = 0;

            InitializePageTable(maxVirtualPages);
            InitializePhysicalPages();
            CreateGpuResources();
        }

        public void Dispose()
        {
            PageTableBuffer?.Dispose();
            PageTableBuffer = null;

            PhysicalPoolSRV?.Dispose();
            PhysicalPoolSRV = null;

            PhysicalPoolDSV?.Dispose();
            PhysicalPoolDSV = null;

            CoreSDK.DisposeObject(ref PhysicalPoolTexture);
        }

        /// <summary>
        /// Called once per frame before processing page requests.
        /// </summary>
        public void BeginFrame()
        {
            mCurrentFrame++;
        }

        /// <summary>
        /// Allocate a physical page for the given virtual page index.
        /// If the pool is full, evicts the least recently used page.
        /// </summary>
        /// <returns>Physical page linear index, or -1 on failure.</returns>
        public int AllocatePage(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return -1;

            // Already mapped?
            ref var entry = ref mPageTable[virtualPageIndex];
            if (entry.IsMapped)
            {
                entry.mLastRenderedFrame = mCurrentFrame;
                return entry.mPhysicalPageCoord.Y * mConfig.PoolDimPages + entry.mPhysicalPageCoord.X;
            }

            int physicalIndex = AllocatePhysicalPage();
            if (physicalIndex < 0)
                return -1;

            // Map virtual → physical
            int pageX = physicalIndex % mConfig.PoolDimPages;
            int pageY = physicalIndex / mConfig.PoolDimPages;

            entry.mPhysicalPageCoord = new Vector2i(pageX, pageY);
            entry.mLastRenderedFrame = mCurrentFrame;
            entry.mFlags = (uint)EVSMPageRequestFlags.Requested;

            // Update physical page info
            ref var physInfo = ref mPhysicalPages[physicalIndex];
            physInfo.mVirtualPageIndex = virtualPageIndex;
            physInfo.mLastRequestedFrame = mCurrentFrame;
            physInfo.mState = (int)EVSMPageState.Allocated;

            return physicalIndex;
        }

        /// <summary>
        /// Mark a virtual page as cached (rendered successfully this frame).
        /// </summary>
        public void MarkPageCached(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return;

            ref var entry = ref mPageTable[virtualPageIndex];
            if (!entry.IsMapped)
                return;

            entry.mLastRenderedFrame = mCurrentFrame;
            entry.mFlags = (uint)EVSMPageRequestFlags.StaticCached;

            int physIndex = entry.mPhysicalPageCoord.Y * mConfig.PoolDimPages + entry.mPhysicalPageCoord.X;
            ref var physInfo = ref mPhysicalPages[physIndex];
            physInfo.mLastRequestedFrame = mCurrentFrame;
            physInfo.mState = (int)EVSMPageState.Cached;
        }

        /// <summary>
        /// Free a specific virtual page, returning its physical page to the free list.
        /// </summary>
        public void FreePage(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return;

            ref var entry = ref mPageTable[virtualPageIndex];
            if (!entry.IsMapped)
                return;

            int physIndex = entry.mPhysicalPageCoord.Y * mConfig.PoolDimPages + entry.mPhysicalPageCoord.X;
            FreePhysicalPage(physIndex);

            entry = FVSMPageTableEntry.CreateUnmapped();
        }

        /// <summary>
        /// Invalidate a page (mark it dirty so it will be re-rendered next frame).
        /// Does NOT free it — the physical allocation is kept for potential re-use.
        /// </summary>
        public void InvalidatePage(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return;

            ref var entry = ref mPageTable[virtualPageIndex];
            entry.mFlags |= (uint)EVSMPageRequestFlags.DynamicDirty;
        }

        /// <summary>
        /// Check if a virtual page is currently mapped to a physical page.
        /// </summary>
        public bool IsPageMapped(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return false;
            return mPageTable[virtualPageIndex].IsMapped;
        }

        /// <summary>
        /// Check if a page needs rendering (newly allocated or dirty).
        /// </summary>
        public bool NeedsRendering(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return false;

            ref var entry = ref mPageTable[virtualPageIndex];
            if (!entry.IsMapped)
                return false;

            bool isNewlyAllocated = (entry.mFlags & (uint)EVSMPageRequestFlags.Requested) != 0;
            bool isDirty = (entry.mFlags & (uint)EVSMPageRequestFlags.DynamicDirty) != 0;
            return isNewlyAllocated || isDirty;
        }

        /// <summary>
        /// Get the viewport rect in the physical atlas for a given virtual page.
        /// </summary>
        public FViewPort GetPageViewport(int virtualPageIndex)
        {
            ref var entry = ref mPageTable[virtualPageIndex];
            int pageRes = mConfig.PageResolution;

            return new FViewPort
            {
                TopLeftX = entry.mPhysicalPageCoord.X * pageRes,
                TopLeftY = entry.mPhysicalPageCoord.Y * pageRes,
                Width = pageRes,
                Height = pageRes,
                MinDepth = 0.0f,
                MaxDepth = 1.0f,
            };
        }

        /// <summary>
        /// Flush the CPU-side page table to the GPU buffer.
        /// Call after all allocations/frees for this frame are done.
        /// </summary>
        public unsafe void FlushPageTableToGpu(ICommandList cmd)
        {
            if (PageTableBuffer == null)
                return;

            fixed (FVSMPageTableEntry* ptr = &mPageTable[0])
            {
                PageTableBuffer.UpdateData(0, ptr, mPageTable.Length * sizeof(FVSMPageTableEntry));
            }
            PageTableBuffer.Flush2GPU(cmd);
        }

        /// <summary>
        /// Evict pages that haven't been requested for maxAge frames.
        /// Called during BeginFrame or after page marking.
        /// </summary>
        public void EvictStalePages(uint maxAge)
        {
            for (int i = 0; i < mPhysicalPages.Length; i++)
            {
                ref var physInfo = ref mPhysicalPages[i];
                if (physInfo.IsFree)
                    continue;

                uint age = mCurrentFrame - physInfo.mLastRequestedFrame;
                if (age > maxAge)
                {
                    int virtualIndex = physInfo.mVirtualPageIndex;
                    if (virtualIndex >= 0 && virtualIndex < mPageTable.Length)
                    {
                        mPageTable[virtualIndex] = FVSMPageTableEntry.CreateUnmapped();
                    }
                    FreePhysicalPage(i);
                }
            }
        }

        /// <summary>
        /// Clear the "newly allocated" and "dirty" flags after rendering is done.
        /// </summary>
        public void ClearRenderFlags()
        {
            for (int i = 0; i < mPageTable.Length; i++)
            {
                ref var entry = ref mPageTable[i];
                if (entry.IsMapped)
                {
                    entry.mFlags &= ~(uint)(EVSMPageRequestFlags.Requested | EVSMPageRequestFlags.DynamicDirty);
                }
            }
        }

        #region Phase 3: Static/Dynamic Separation
        public ref FVSMPageTableEntry GetPageTableEntry(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                throw new ArgumentOutOfRangeException(nameof(virtualPageIndex));
            return ref mPageTable[virtualPageIndex];
        }

        /// <summary>
        /// Check if a page is purely static-cached (rendered once, no dynamic objects).
        /// Static pages skip re-rendering entirely until explicitly invalidated.
        /// </summary>
        public bool IsStaticCached(int virtualPageIndex)
        {
            if (virtualPageIndex < 0 || virtualPageIndex >= mPageTable.Length)
                return false;

            ref var entry = ref mPageTable[virtualPageIndex];
            if (!entry.IsMapped)
                return false;

            return (entry.mFlags & (uint)EVSMPageRequestFlags.StaticCached) != 0
                && (entry.mFlags & (uint)EVSMPageRequestFlags.DynamicDirty) == 0;
        }

        /// <summary>
        /// Invalidate all pages that contain dynamic objects (mark them dirty).
        /// Called at the beginning of each frame for pages whose dynamic casters moved.
        /// </summary>
        /// <param name="dynamicPageIndices">Virtual page indices that have dynamic casters.</param>
        public void InvalidateDynamicPages(ReadOnlySpan<int> dynamicPageIndices)
        {
            for (int i = 0; i < dynamicPageIndices.Length; i++)
            {
                InvalidatePage(dynamicPageIndices[i]);
            }
        }

        /// <summary>
        /// Collect all pages that need rendering this frame (newly allocated OR dirty).
        /// Skips purely static-cached pages. Used by the shadow rendering loop to avoid
        /// redundant re-draws of static geometry.
        /// </summary>
        /// <param name="output">List to append pages needing rendering.</param>
        public void CollectPagesNeedingRendering(List<int> output)
        {
            output.Clear();
            for (int i = 0; i < mPageTable.Length; i++)
            {
                if (NeedsRendering(i))
                    output.Add(i);
            }
        }

        #endregion

        #region Private Implementation

        private void InitializePageTable(int maxVirtualPages)
        {
            mPageTable = new FVSMPageTableEntry[maxVirtualPages];
            for (int i = 0; i < mPageTable.Length; i++)
            {
                mPageTable[i] = FVSMPageTableEntry.CreateUnmapped();
            }
        }

        private void InitializePhysicalPages()
        {
            int totalPages = mConfig.TotalPhysicalPages;
            mPhysicalPages = new FVSMPhysicalPageInfo[totalPages];
            mFreeList = new Stack<int>(totalPages);

            for (int i = totalPages - 1; i >= 0; i--)
            {
                mPhysicalPages[i] = new FVSMPhysicalPageInfo
                {
                    mVirtualPageIndex = -1,
                    mLastRequestedFrame = 0,
                    mState = (int)EVSMPageState.Free,
                    mPadding = 0,
                };
                mFreeList.Push(i);
            }
        }

        private void CreateGpuResources()
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            int poolRes = mConfig.PoolTextureResolution;

            // Create the physical pool depth texture (atlas)
            var desc = new FTextureDesc();
            desc.SetDefault();
            desc.BindFlags = EBufferType.BFT_SRV | EBufferType.BFT_DSV;
            desc.Width = (uint)poolRes;
            desc.Height = (uint)poolRes;
            desc.MipLevels = 1;
            desc.Format = EPixelFormat.PXF_D16_UNORM;

            PhysicalPoolTexture = rc.CreateTexture(in desc);
            PhysicalPoolTexture.SetDebugName("VSM_PhysicalPagePool");

            // Create SRV
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Format = EPixelFormat.PXF_D16_UNORM;
            srvDesc.Texture2D.MipLevels = 1;
            srvDesc.Texture2D.MostDetailedMip = 0;
            PhysicalPoolSRV = rc.CreateSRV(PhysicalPoolTexture, in srvDesc);

            // Create DSV
            var dsvDesc = new FDsvDesc();
            dsvDesc.SetDefault();
            dsvDesc.Type = EDsvType.DSV_Texture2D;
            dsvDesc.Format = EPixelFormat.PXF_D16_UNORM;
            dsvDesc.Width = (uint)poolRes;
            dsvDesc.Height = (uint)poolRes;
            dsvDesc.MipLevel = 0;
            PhysicalPoolDSV = rc.CreateDSV(PhysicalPoolTexture, in dsvDesc);

            // Create GPU page table buffer
            PageTableBuffer = new TtCpu2GpuBuffer<FVSMPageTableEntry>();
            PageTableBuffer.Initialize(EBufferType.BFT_SRV);
            PageTableBuffer.SetSize(mPageTable.Length);
        }

        private int AllocatePhysicalPage()
        {
            if (mFreeList.Count > 0)
                return mFreeList.Pop();

            // Pool full — evict the oldest page (LRU)
            return EvictLRUPage();
        }

        private int EvictLRUPage()
        {
            int oldestIndex = -1;
            uint oldestFrame = uint.MaxValue;

            for (int i = 0; i < mPhysicalPages.Length; i++)
            {
                ref var info = ref mPhysicalPages[i];
                if (info.IsFree)
                    continue;

                if (info.LastRequestedFrame < oldestFrame)
                {
                    oldestFrame = info.LastRequestedFrame;
                    oldestIndex = i;
                }
            }

            if (oldestIndex < 0)
                return -1;

            // Unmap the old virtual page
            int oldVirtualIndex = mPhysicalPages[oldestIndex].VirtualPageIndex;
            if (oldVirtualIndex >= 0 && oldVirtualIndex < mPageTable.Length)
            {
                mPageTable[oldVirtualIndex] = FVSMPageTableEntry.CreateUnmapped();
            }

            // Reset physical page
            mPhysicalPages[oldestIndex].VirtualPageIndex = -1;
            mPhysicalPages[oldestIndex].State = (int)EVSMPageState.Free;
            mPhysicalPages[oldestIndex].LastRequestedFrame = 0;

            return oldestIndex;
        }

        private void FreePhysicalPage(int physicalIndex)
        {
            if (physicalIndex < 0 || physicalIndex >= mPhysicalPages.Length)
                return;

            ref var info = ref mPhysicalPages[physicalIndex];
            info.VirtualPageIndex = -1;
            info.State = (int)EVSMPageState.Free;
            info.LastRequestedFrame = 0;

            mFreeList.Push(physicalIndex);
        }

        #endregion
    }
}
