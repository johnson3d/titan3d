using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline;

namespace EngineNS.Bricks.AdvanceShadow
{
    /// <summary>
    /// Represents a single dirty clipmap page that needs rendering this frame.
    /// </summary>
    public struct FVSMClipmapDirtyPage
    {
        public int Level;
        public int PageX;
        public int PageY;
        public int VirtualPageIndex;
    }

    /// <summary>
    /// A single clipmap level for directional light VSM.
    /// Each level covers a square area centered on the camera, with side length doubling per level.
    /// All levels share the same page resolution but cover progressively larger world-space areas.
    /// Inspired by UE5 VirtualShadowMapClipmap (levels 6~22, each +1 = 2x radius).
    /// </summary>
    public struct FVSMClipmapLevel
    {
        /// <summary>
        /// World-space half-extent of this level's coverage area.
        /// Level N covers a square of side = 2 * HalfExtent centered on the camera.
        /// </summary>
        public float HalfExtent;

        /// <summary>
        /// Orthographic view-projection matrix for this level.
        /// Projects world-space geometry into the shadow page's clip space.
        /// </summary>
        public Matrix ViewProjection;

        /// <summary>
        /// Starting virtual page index for this level in the page table.
        /// Each level occupies PagesPerLevel contiguous entries.
        /// </summary>
        public int PageTableOffset;

        /// <summary>
        /// Number of pages in each dimension for this level (e.g., 4 means 4x4=16 pages).
        /// </summary>
        public int PagesPerDim;

        /// <summary>
        /// Total pages for this level = PagesPerDim * PagesPerDim.
        /// </summary>
        public int TotalPages => PagesPerDim * PagesPerDim;

        /// <summary>
        /// World-space texel size at this level (= 2*HalfExtent / (PagesPerDim * PageResolution)).
        /// Useful for LOD selection and softness calculation.
        /// </summary>
        public float TexelSize;
    }

    /// <summary>
    /// Configuration for the directional light clipmap.
    /// </summary>
    public struct FVSMClipmapConfig
    {
        /// <summary>
        /// Number of clipmap levels (e.g., 8 levels covers ~256x base radius).
        /// </summary>
        public int LevelCount;

        /// <summary>
        /// Base half-extent for level 0 (closest to camera). Typical: 5~10 meters.
        /// </summary>
        public float BaseHalfExtent;

        /// <summary>
        /// Pages per dimension per level (e.g., 4 = 4x4=16 pages per level).
        /// </summary>
        public int PagesPerDim;

        /// <summary>
        /// Resolution of each page in texels.
        /// </summary>
        public int PageResolution;

        /// <summary>
        /// Total virtual pages across all levels = LevelCount * PagesPerDim * PagesPerDim.
        /// </summary>
        public int TotalVirtualPages => LevelCount * PagesPerDim * PagesPerDim;

        public static FVSMClipmapConfig CreateDefault()
        {
            return new FVSMClipmapConfig
            {
                LevelCount = 8,
                BaseHalfExtent = 8.0f,
                PagesPerDim = 4,
                PageResolution = 128,
            };
        }
    }

    /// <summary>
    /// Manages a multi-level clipmap for directional light virtual shadow maps.
    /// Each level is an orthographic projection centered on the camera with doubling coverage radius.
    /// Pages are allocated on-demand from the shared TtVSMPagePool.
    /// 
    /// All page selection and snapping operates in **light-view space** (camera looks along -Z,
    /// right=X, up=Y). This guarantees page selection and ortho projection are perfectly aligned,
    /// preventing UV out-of-bounds when sampling shadow maps with oblique light directions.
    /// </summary>
    public class TtVSMClipmap : IDisposable
    {
        private FVSMClipmapConfig mConfig;
        private FVSMClipmapLevel[] mLevels;
        private TtVSMPagePool mPagePool;
        internal TtCamera[] mClipmapPageCameras;

        // Snapped center in light-view space XY (Z is depth along light direction)
        private Vector3 mSnappedLightViewCenter;

        // World-to-light-view rotation matrix (pure rotation, no translation).
        // Transforms world coords to light-view space where:
        //   X = camera right, Y = camera up, Z = depth (along light direction)
        private Matrix mWorldToLightViewRotation;
        private Matrix mLightViewToWorldRotation; // transpose of above

        // Dirty page tracking: pages that need rendering this frame
        private List<FVSMClipmapDirtyPage> mDirtyPages = new List<FVSMClipmapDirtyPage>();

        /// <summary>
        /// Offset applied to clipmap virtual page indices to avoid collision with QTree node indices.
        /// Set during initialization to QTree.MaxNodeCount (or similar).
        /// </summary>
        public int VirtualPageOffset { get; set; }

        /// <summary>
        /// Light direction (normalized, pointing toward light source).
        /// </summary>
        public Vector3 LightDirection { get; private set; }

        /// <summary>
        /// Snapped center in light-view space (XY used for page selection, Z unused).
        /// Uploaded to GPU as ClipmapCenter so shader can do page selection in light-view space.
        /// </summary>
        public Vector3 SnappedLightViewCenter => mSnappedLightViewCenter;

        /// <summary>
        /// World-to-light-view rotation matrix (3x3 rotation, stored as 4x4 with no translation).
        /// Shader uses this to transform worldPos to light-view space before page selection.
        /// </summary>
        public ref Matrix WorldToLightViewRotation => ref mWorldToLightViewRotation;

        /// <summary>
        /// Pages that need rendering this frame (newly allocated or invalidated).
        /// </summary>
        public IReadOnlyList<FVSMClipmapDirtyPage> DirtyPages => mDirtyPages;

        /// <summary>
        /// Clipmap level data (read-only access for shaders/debug).
        /// </summary>
        public ReadOnlySpan<FVSMClipmapLevel> Levels => mLevels.AsSpan();

        /// <summary>
        /// Configuration.
        /// </summary>
        public ref readonly FVSMClipmapConfig Config => ref mConfig;

        /// <summary>
        /// Total virtual pages allocated for the clipmap in the page table.
        /// </summary>
        public int TotalVirtualPages => mConfig.TotalVirtualPages;

        /// <summary>
        /// Number of dirty pages rendered this frame.
        /// </summary>
        public int DirtyPageCount => mDirtyPages.Count;

        /// <summary>
        /// Number of pages currently mapped (allocated) in the pool for this clipmap.
        /// </summary>
        public int MappedPageCount
        {
            get
            {
                if (mPagePool == null || mLevels == null)
                    return 0;
                int count = 0;
                int pagesPerDim = mConfig.PagesPerDim;
                for (int level = 0; level < mConfig.LevelCount; level++)
                {
                    for (int y = 0; y < pagesPerDim; y++)
                    {
                        for (int x = 0; x < pagesPerDim; x++)
                        {
                            int localIndex = mLevels[level].PageTableOffset + y * pagesPerDim + x;
                            int virtualIndex = localIndex + VirtualPageOffset;
                            if (mPagePool.IsPageMapped(virtualIndex))
                                count++;
                        }
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Number of pages that are static-cached (don't need re-render).
        /// </summary>
        public int CachedPageCount
        {
            get
            {
                if (mPagePool == null || mLevels == null)
                    return 0;
                int count = 0;
                int pagesPerDim = mConfig.PagesPerDim;
                for (int level = 0; level < mConfig.LevelCount; level++)
                {
                    for (int y = 0; y < pagesPerDim; y++)
                    {
                        for (int x = 0; x < pagesPerDim; x++)
                        {
                            int localIndex = mLevels[level].PageTableOffset + y * pagesPerDim + x;
                            int virtualIndex = localIndex + VirtualPageOffset;
                            if (mPagePool.IsStaticCached(virtualIndex))
                                count++;
                        }
                    }
                }
                return count;
            }
        }

        public ref FVSMPageTableEntry GetPageTableEntry(int virtualPageIndex)
        {
            if (mPagePool == null)
                throw new InvalidOperationException("Page pool not initialized");
            return ref mPagePool.GetPageTableEntry(virtualPageIndex);
        }

        /// <summary>
        /// Get per-level debug info: (mapped, cached, dirty) page counts.
        /// </summary>
        public void GetLevelStats(int level, out int mapped, out int cached, out int dirty)
        {
            mapped = 0;
            cached = 0;
            dirty = 0;
            if (mPagePool == null || mLevels == null || level < 0 || level >= mConfig.LevelCount)
                return;

            int pagesPerDim = mConfig.PagesPerDim;
            for (int y = 0; y < pagesPerDim; y++)
            {
                for (int x = 0; x < pagesPerDim; x++)
                {
                    int localIndex = mLevels[level].PageTableOffset + y * pagesPerDim + x;
                    int virtualIndex = localIndex + VirtualPageOffset;
                    if (mPagePool.IsPageMapped(virtualIndex))
                    {
                        mapped++;
                        if (mPagePool.IsStaticCached(virtualIndex))
                            cached++;
                        if (mPagePool.NeedsRendering(virtualIndex))
                            dirty++;
                    }
                }
            }
        }

        public void Initialize(in FVSMClipmapConfig config, TtVSMPagePool pagePool)
        {
            mConfig = config;
            mPagePool = pagePool;
            mLevels = new FVSMClipmapLevel[config.LevelCount];
            mSnappedLightViewCenter = Vector3.Zero;
            mWorldToLightViewRotation = Matrix.Identity;
            mLightViewToWorldRotation = Matrix.Identity;

            // Assign page table offsets for each level
            int offset = 0;
            for (int i = 0; i < config.LevelCount; i++)
            {
                mLevels[i].PagesPerDim = config.PagesPerDim;
                mLevels[i].PageTableOffset = offset;
                offset += mLevels[i].TotalPages;
            }
            mClipmapPageCameras = new TtCamera[config.TotalVirtualPages];
            for (int i = 0; i < config.TotalVirtualPages; i++)
            {
                mClipmapPageCameras[i] = new TtCamera();
            }
        }

        public void Dispose()
        {
            mLevels = null;
        }

        /// <summary>
        /// Update the clipmap for this frame.
        /// Builds the WorldToLightView rotation matrix, snaps the center in light-view space,
        /// then requests pages from the pool and collects dirty pages that need rendering.
        /// </summary>
        /// <param name="cameraWorldPos">Camera world position (clipmap center).</param>
        /// <param name="lightDir">Normalized light direction (toward light, e.g., sun direction).</param>
        public void Update(TtAdvanceShadowMapNode advNode, in DVector3 cameraWorldPos, in Vector3 lightDir)
        {
            LightDirection = lightDir;

            // Build WorldToLightView rotation matrix (same approach as UE's FInverseRotationMatrix)
            // This transforms world-space to light-view space where:
            //   X = right (perpendicular to light, in horizontal plane)
            //   Y = up (perpendicular to light and right)
            //   Z = forward (along light direction = depth)
            var forward = lightDir;
            forward.Normalize();
            var worldUp = MathF.Abs(Vector3.Dot(forward, Vector3.UnitY)) > 0.99f
                ? Vector3.UnitZ
                : Vector3.UnitY;
            var right = Vector3.Cross(worldUp, forward);
            right.Normalize();
            var up = Vector3.Cross(forward, right);
            up.Normalize();

            // For C# row-vector convention (v * M), WorldToLightView must have axes as COLUMNS:
            //   Column0 = right, Column1 = up, Column2 = forward
            // In row-major Matrix(M11..M44) layout, this means:
            //   Row0 = (right.X, up.X, forward.X, 0)
            //   Row1 = (right.Y, up.Y, forward.Y, 0)
            //   Row2 = (right.Z, up.Z, forward.Z, 0)
            mWorldToLightViewRotation = new Matrix(
                right.X, up.X, forward.X, 0,
                right.Y, up.Y, forward.Y, 0,
                right.Z, up.Z, forward.Z, 0,
                0, 0, 0, 1);
            mLightViewToWorldRotation = Matrix.Transpose(mWorldToLightViewRotation);

            // Transform camera position to light-view space
            var camWorldF = new Vector3((float)cameraWorldPos.X, (float)cameraWorldPos.Y, (float)cameraWorldPos.Z);
            var camLV = Vector3.TransformCoordinate(camWorldF, mWorldToLightViewRotation);

            // Snap in light-view space XY using finest level's texel size
            for (int level = 0; level < mConfig.LevelCount; level++)
            {
                float halfExtent = mConfig.BaseHalfExtent * (1 << level);
                mLevels[level].HalfExtent = halfExtent;

                float totalWorldSize = halfExtent * 2.0f;
                float pageWorldSize = totalWorldSize / mConfig.PagesPerDim;
                mLevels[level].TexelSize = pageWorldSize / mConfig.PageResolution;
            }

            // Snap to level 0's PAGE size (not texel size) in light-view space XY.
            // Using page size as snap grid means the center only changes when the camera
            // moves a full page width, dramatically reducing re-render frequency.
            // Within a page, sub-page movement doesn't affect ortho projection correctness
            // because shader page selection and VP projection both reference the same center.
            float snapSize = mLevels[0].HalfExtent * 2.0f / mConfig.PagesPerDim;
            var newSnappedCenter = new Vector3(
                MathF.Floor(camLV.X / snapSize) * snapSize,
                MathF.Floor(camLV.Y / snapSize) * snapSize,
                camLV.Z);

            // If snapped center changed, invalidate ALL pages because their VP matrices
            // are no longer valid (page world positions shift with the center).
            if (mSnappedLightViewCenter.X != newSnappedCenter.X ||
                mSnappedLightViewCenter.Y != newSnappedCenter.Y)
            {
                MarkAllPagesDirty();
            }

            mSnappedLightViewCenter = newSnappedCenter;
            // Request pages and collect dirty ones
            CollectDirtyPages();
        }

        /// <summary>
        /// Allocate pages for all clipmap levels and collect pages that need rendering.
        /// A page is dirty if it was newly allocated or invalidated (dynamic objects moved).
        /// </summary>
        private void CollectDirtyPages()
        {
            mDirtyPages.Clear();
            if (mPagePool == null)
                return;

            int pagesPerDim = mConfig.PagesPerDim;
            for (int level = 0; level < mConfig.LevelCount; level++)
            {
                for (int y = 0; y < pagesPerDim; y++)
                {
                    for (int x = 0; x < pagesPerDim; x++)
                    {
                        int localIndex = mLevels[level].PageTableOffset + y * pagesPerDim + x;
                        int virtualIndex = localIndex + VirtualPageOffset;

                        mPagePool.AllocatePage(virtualIndex);

                        if (mPagePool.NeedsRendering(virtualIndex))
                        {
                            mDirtyPages.Add(new FVSMClipmapDirtyPage
                            {
                                Level = level,
                                PageX = x,
                                PageY = y,
                                VirtualPageIndex = virtualIndex,
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Build a TtCamera for rendering a specific dirty page.
        /// Uses tight depth range derived from caster AABB projected onto light direction,
        /// matching QTree's UpdateShadowMatrix approach for optimal R16F ESM precision.
        /// </summary>
        /// <param name="dirtyPage">Which page to render.</param>
        /// <param name="camera">Output camera.</param>
        /// <param name="world">World for camera offset.</param>
        /// <param name="casterAABB">Merged AABB of all casters overlapping this page.</param>
        /// <param name="outNear">Actual near plane used (for FVSMClipmapPageData).</param>
        /// <param name="outFar">Actual far plane used (for FVSMClipmapPageData).</param>
        public void BuildPageCamera(in FVSMClipmapDirtyPage dirtyPage, TtCamera camera,
            GamePlay.TtWorld world, in DBoundingBox casterAABB,
            out float outNear, out float outFar)
        {
            ref var lv = ref mLevels[dirtyPage.Level];
            float fullSize = lv.HalfExtent * 2.0f;
            float pageSize = fullSize / mConfig.PagesPerDim;

            // Page center in light-view space XY (offset from snapped center)
            float offsetX = -lv.HalfExtent + (dirtyPage.PageX + 0.5f) * pageSize;
            float offsetY = -lv.HalfExtent + (dirtyPage.PageY + 0.5f) * pageSize;

            var pageCenterLV = new Vector3(
                mSnappedLightViewCenter.X + offsetX,
                mSnappedLightViewCenter.Y + offsetY,
                mSnappedLightViewCenter.Z);

            // Transform page center back to world space
            var pageCenterWorld = Vector3.TransformCoordinate(pageCenterLV, mLightViewToWorldRotation).AsDVector();

            // Up vector from our rotation matrix to ensure consistent axis alignment
            var up = new Vector3(mLightViewToWorldRotation.M21, mLightViewToWorldRotation.M22, mLightViewToWorldRotation.M23);

            // Project caster AABB corners onto light direction to get tight depth range
            var lightDirD = LightDirection.AsDVector();
            lightDirD.Normalize();
            double minDist = double.MaxValue;
            double maxDist = double.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var corner = casterAABB.GetCorner(i) - pageCenterWorld;
                var d = DVector3.Dot(corner, lightDirD);
                if (d > maxDist) maxDist = d;
                if (d < minDist) minDist = d;
            }

            var depthRange = maxDist - minDist;
            // Eye must be on the -lightDir side (light source direction) past ALL casters,
            // so that LookAtLH forward = normalize(target - eye) = +lightDir,
            // matching WorldToLightView's forward axis exactly.
            // minDist/maxDist are projections onto +lightDir relative to pageCenter.
            // The -lightDir-most caster point has projection = minDist.
            // Eye must be further in -lightDir than that: offset along -lightDir = max(-minDist, 0) + 1.1
            double eyeDistAlongNegLight = Math.Max(-minDist, 0.0) + 1.1;
            var eye = pageCenterWorld - lightDirD * eyeDistAlongNegLight;

            // Near/far from eye along +lightDir direction:
            // Nearest caster: eyeDistAlongNegLight + minDist (distance from eye to nearest caster)
            // Farthest caster: eyeDistAlongNegLight + maxDist
            outNear = (float)(eyeDistAlongNegLight + minDist - 0.1);
            outFar = (float)(eyeDistAlongNegLight + maxDist + 0.1);
            if (outNear < 0.1f) outNear = 0.1f;

            // Ortho size matches pageSize with small margin for floating-point tolerance
            float orthoSize = pageSize * 1.05f;

            // Shadow page cameras must NOT use CameraOffset. The DirLighting shader does
            // mul(worldPos, pageVP) with raw world coordinates (no offset subtracted).
            // If VP encodes eye-offset, the transform is wrong → shadow flickers when camera moves.
            camera.SetMatrixStartPosition(in DVector3.Zero);
            camera.LookAtLH(eye, pageCenterWorld, in up);
            camera.DoOrthoProjectionForShadow(orthoSize, orthoSize, outNear, outFar, 0, 0);
            camera.UpdateConstBufferData(TtEngine.Instance.GfxDevice.RenderContext, NxRHI.TtCbView.EUpdateMode.Immediately);
        }

        /// <summary>

        /// <summary>
        /// Get the local page index for a dirty page (used to index ClipmapPageBuffer).
        /// </summary>
        public int GetLocalPageIndex(in FVSMClipmapDirtyPage dirtyPage)
        {
            ref var lv = ref mLevels[dirtyPage.Level];
            return lv.PageTableOffset + dirtyPage.PageY * mConfig.PagesPerDim + dirtyPage.PageX;
        }

        /// <summary>
        /// Get the light-view XY bounds of a page. Used for precise caster culling in LV space.
        /// Since the page camera uses orthographic projection aligned to light-view axes,
        /// a caster only contributes to this page if its LV XY projection overlaps this range.
        /// </summary>
        public void GetPageLightViewBounds(in FVSMClipmapDirtyPage dirtyPage,
            out float lvMinX, out float lvMaxX, out float lvMinY, out float lvMaxY)
        {
            ref var lv = ref mLevels[dirtyPage.Level];
            float fullSize = lv.HalfExtent * 2.0f;
            float pageSize = fullSize / mConfig.PagesPerDim;
            float pageHalfSize = pageSize * 0.5f;

            float offsetX = -lv.HalfExtent + (dirtyPage.PageX + 0.5f) * pageSize;
            float offsetY = -lv.HalfExtent + (dirtyPage.PageY + 0.5f) * pageSize;
            float centerLVX = mSnappedLightViewCenter.X + offsetX;
            float centerLVY = mSnappedLightViewCenter.Y + offsetY;

            lvMinX = centerLVX - pageHalfSize;
            lvMaxX = centerLVX + pageHalfSize;
            lvMinY = centerLVY - pageHalfSize;
            lvMaxY = centerLVY + pageHalfSize;
        }

        /// <summary>
        /// Get the world-space XZ bounds of a dirty page.
        /// Page is defined in light-view space, so we compute 4 corners in LV space
        /// and transform back to world to get a conservative XZ AABB for caster culling.
        /// </summary>
        public void GetPageWorldBoundsXZ(in FVSMClipmapDirtyPage dirtyPage, out float minX, out float maxX, out float minZ, out float maxZ)
        {
            ref var lv = ref mLevels[dirtyPage.Level];
            float fullSize = lv.HalfExtent * 2.0f;
            float pageSize = fullSize / mConfig.PagesPerDim;
            float pageHalfSize = pageSize * 0.5f;

            // Page center in light-view space
            float offsetX = -lv.HalfExtent + (dirtyPage.PageX + 0.5f) * pageSize;
            float offsetY = -lv.HalfExtent + (dirtyPage.PageY + 0.5f) * pageSize;
            float centerLVX = mSnappedLightViewCenter.X + offsetX;
            float centerLVY = mSnappedLightViewCenter.Y + offsetY;

            // Extrude the page along light direction (Z in light-view space) to cover casters
            // at different depths. Use the level's half-extent as depth range (matches the
            // spatial extent this level is responsible for — avoids over-extending for low levels).
            float depthExtent = lv.HalfExtent;

            // Transform 4 corners × 2 depth extremes back to world space and compute XZ AABB
            minX = float.MaxValue; maxX = float.MinValue;
            minZ = float.MaxValue; maxZ = float.MinValue;
            for (int cz = 0; cz <= 1; cz++)
            {
                float z = mSnappedLightViewCenter.Z + (cz == 0 ? -depthExtent : depthExtent);
                for (int cy = 0; cy <= 1; cy++)
                {
                    for (int cx = 0; cx <= 1; cx++)
                    {
                        var cornerLV = new Vector3(
                            centerLVX - pageHalfSize + cx * pageSize,
                            centerLVY - pageHalfSize + cy * pageSize,
                            z);
                        var cornerWorld = Vector3.TransformCoordinate(cornerLV, mLightViewToWorldRotation);
                        if (cornerWorld.X < minX) minX = cornerWorld.X;
                        if (cornerWorld.X > maxX) maxX = cornerWorld.X;
                        if (cornerWorld.Z < minZ) minZ = cornerWorld.Z;
                        if (cornerWorld.Z > maxZ) maxZ = cornerWorld.Z;
                    }
                }
            }
        }

        /// <summary>
        /// Mark all rendered dirty pages as cached in the pool after rendering completes.
        /// </summary>
        public void MarkRenderedPagesCached()
        {
            if (mPagePool == null)
                return;

            for (int i = 0; i < mDirtyPages.Count; i++)
            {
                mPagePool.MarkPageCached(mDirtyPages[i].VirtualPageIndex);
            }
        }

        /// <summary>
        /// Mark all clipmap pages as needing re-render.
        /// Called before Update() so that CollectDirtyPages will pick them all up.
        /// </summary>
        public void MarkAllPagesDirty()
        {
            if (mPagePool == null || mLevels == null)
                return;

            int pagesPerDim = mConfig.PagesPerDim;
            for (int level = 0; level < mConfig.LevelCount; level++)
            {
                for (int y = 0; y < pagesPerDim; y++)
                {
                    for (int x = 0; x < pagesPerDim; x++)
                    {
                        int localIndex = mLevels[level].PageTableOffset + y * pagesPerDim + x;
                        int virtualIndex = localIndex + VirtualPageOffset;
                        mPagePool.InvalidatePage(virtualIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Invalidate clipmap pages that overlap a world-space AABB (e.g., a dynamic object moved).
        /// All pages at all levels that intersect the AABB are marked dirty in the page pool.
        /// </summary>
        public void InvalidateRegion(in DBoundingBox worldAABB)
        {
            if (mPagePool == null)
                return;

            // Transform AABB corners to light-view space and compute XY bounds
            float lvMinX = float.MaxValue, lvMaxX = float.MinValue;
            float lvMinY = float.MaxValue, lvMaxY = float.MinValue;
            for (int i = 0; i < 8; i++)
            {
                var cornerD = worldAABB.GetCorner(i);
                var corner = new Vector3((float)cornerD.X, (float)cornerD.Y, (float)cornerD.Z);
                var lv = Vector3.TransformCoordinate(corner, mWorldToLightViewRotation);
                if (lv.X < lvMinX) lvMinX = lv.X;
                if (lv.X > lvMaxX) lvMaxX = lv.X;
                if (lv.Y < lvMinY) lvMinY = lv.Y;
                if (lv.Y > lvMaxY) lvMaxY = lv.Y;
            }

            for (int level = 0; level < mConfig.LevelCount; level++)
            {
                float halfExt = mLevels[level].HalfExtent;
                float centerX = mSnappedLightViewCenter.X;
                float centerY = mSnappedLightViewCenter.Y;
                float levelMinX = centerX - halfExt;
                float levelMinY = centerY - halfExt;
                float levelMaxX = centerX + halfExt;
                float levelMaxY = centerY + halfExt;

                // Skip if AABB doesn't intersect this level in light-view XY
                if (lvMinX > levelMaxX || lvMaxX < levelMinX ||
                    lvMinY > levelMaxY || lvMaxY < levelMinY)
                    continue;

                float pageSize = (halfExt * 2.0f) / mConfig.PagesPerDim;

                // Compute page range that overlaps
                int startX = Math.Max(0, (int)((Math.Max(lvMinX, levelMinX) - levelMinX) / pageSize));
                int endX = Math.Min(mConfig.PagesPerDim - 1, (int)((Math.Min(lvMaxX, levelMaxX) - levelMinX) / pageSize));
                int startY = Math.Max(0, (int)((Math.Max(lvMinY, levelMinY) - levelMinY) / pageSize));
                int endY = Math.Min(mConfig.PagesPerDim - 1, (int)((Math.Min(lvMaxY, levelMaxY) - levelMinY) / pageSize));

                for (int y = startY; y <= endY; y++)
                {
                    for (int x = startX; x <= endX; x++)
                    {
                        int localIndex = mLevels[level].PageTableOffset + y * mConfig.PagesPerDim + x;
                        int virtualIndex = localIndex + VirtualPageOffset;
                        mPagePool.InvalidatePage(virtualIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Invalidate a single world position (e.g., for a point-sized dynamic object).
        /// Invalidates the page at the finest level that contains this point.
        /// </summary>
        public void InvalidatePoint(in Vector3 worldPos)
        {
            if (mPagePool == null)
                return;

            if (WorldToPage(in worldPos, out int level, out int pageX, out int pageY))
            {
                int localIndex = mLevels[level].PageTableOffset + pageY * mConfig.PagesPerDim + pageX;
                int virtualIndex = localIndex + VirtualPageOffset;
                mPagePool.InvalidatePage(virtualIndex);
            }
        }

        /// <summary>
        /// Get the virtual page index for a given (level, pageX, pageY) coordinate.
        /// </summary>
        public int GetVirtualPageIndex(int level, int pageX, int pageY)
        {
            if (level < 0 || level >= mConfig.LevelCount)
                return -1;
            if (pageX < 0 || pageX >= mConfig.PagesPerDim || pageY < 0 || pageY >= mConfig.PagesPerDim)
                return -1;

            return mLevels[level].PageTableOffset + pageY * mConfig.PagesPerDim + pageX;
        }

        /// <summary>
        /// Given a world-space position, return the virtual page index (with VirtualPageOffset applied)
        /// for the finest clipmap level that covers the point.
        /// Returns -1 if the position is outside all clipmap levels.
        /// </summary>
        /// <param name="worldPos">World-space position to query.</param>
        /// <returns>Virtual page index in the page table, or -1 if not covered.</returns>
        public int GetVirtualPageIndexAtWorldPosition(in Vector3 worldPos)
        {
            if (!WorldToPage(in worldPos, out int level, out int pageX, out int pageY))
                return -1;

            int localIndex = mLevels[level].PageTableOffset + pageY * mConfig.PagesPerDim + pageX;
            return localIndex + VirtualPageOffset;
        }

        /// <summary>
        /// Given a world-space position, determine which clipmap level and page it maps to.
        /// Returns the finest level that contains the point.
        /// </summary>
        /// <param name="worldPos">World position to query.</param>
        /// <param name="level">Output: clipmap level index.</param>
        /// <param name="pageX">Output: page X coordinate within the level.</param>
        /// <param name="pageY">Output: page Y coordinate within the level.</param>
        /// <returns>True if the position is within clipmap coverage.</returns>
        public bool WorldToPage(in Vector3 worldPos, out int level, out int pageX, out int pageY)
        {
            level = -1;
            pageX = -1;
            pageY = -1;

            // Transform to light-view space
            var posLV = Vector3.TransformCoordinate(worldPos, mWorldToLightViewRotation);

            // Find the finest level that covers this point in light-view XY
            for (int lv = 0; lv < mConfig.LevelCount; lv++)
            {
                float halfExt = mLevels[lv].HalfExtent;
                float relX = posLV.X - mSnappedLightViewCenter.X;
                float relY = posLV.Y - mSnappedLightViewCenter.Y;

                if (MathF.Abs(relX) <= halfExt && MathF.Abs(relY) <= halfExt)
                {
                    level = lv;
                    float normalizedX = (relX + halfExt) / (halfExt * 2.0f);
                    float normalizedY = (relY + halfExt) / (halfExt * 2.0f);
                    pageX = Math.Clamp((int)(normalizedX * mConfig.PagesPerDim), 0, mConfig.PagesPerDim - 1);
                    pageY = Math.Clamp((int)(normalizedY * mConfig.PagesPerDim), 0, mConfig.PagesPerDim - 1);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Request pages that are visible from the camera for a specific level.
        /// Allocates pages in the shared PagePool for all pages within this level's footprint.
        /// </summary>
        public void RequestLevelPages(int level)
        {
            if (level < 0 || level >= mConfig.LevelCount || mPagePool == null)
                return;

            int pagesPerDim = mConfig.PagesPerDim;
            for (int y = 0; y < pagesPerDim; y++)
            {
                for (int x = 0; x < pagesPerDim; x++)
                {
                    int virtualIndex = GetVirtualPageIndex(level, x, y);
                    mPagePool.AllocatePage(virtualIndex);
                }
            }
        }

        /// <summary>
        /// Get the view-projection matrix for rendering a specific page.
        /// This is a sub-viewport of the level's full ortho projection.
        /// Page center is computed in light-view space and transformed back to world.
        /// </summary>
        public Matrix GetPageViewProjection(int level, int pageX, int pageY)
        {
            if (level < 0 || level >= mConfig.LevelCount)
                return Matrix.Identity;

            ref var lv = ref mLevels[level];
            float fullSize = lv.HalfExtent * 2.0f;
            float pageSize = fullSize / mConfig.PagesPerDim;

            // Page center in light-view space
            float offsetX = -lv.HalfExtent + (pageX + 0.5f) * pageSize;
            float offsetY = -lv.HalfExtent + (pageY + 0.5f) * pageSize;
            var pageCenterLV = new Vector3(
                mSnappedLightViewCenter.X + offsetX,
                mSnappedLightViewCenter.Y + offsetY,
                mSnappedLightViewCenter.Z);

            // Transform back to world space
            var center = Vector3.TransformCoordinate(pageCenterLV, mLightViewToWorldRotation);
            var lightDir = LightDirection;
            return BuildClipmapVP(in center, in lightDir, pageSize * 0.5f);
        }

        /// <summary>
        /// Build an orthographic view-projection matrix looking along the light direction.
        /// Used only for per-level VP (Clipmap.Update). Per-page rendering uses BuildPageCamera.
        /// </summary>
        private static Matrix BuildClipmapVP(in Vector3 center, in Vector3 lightDir, float halfExtent)
        {
            float farDist = halfExtent * 4.0f;
            var eye = new Vector3(center.X - lightDir.X * farDist,
                                  center.Y - lightDir.Y * farDist,
                                  center.Z - lightDir.Z * farDist);

            var up = MathF.Abs(Vector3.Dot(lightDir, Vector3.UnitY)) > 0.99f
                ? Vector3.UnitZ
                : Vector3.UnitY;

            Matrix.LookAtLH(in eye, in center, in up, out var view);
            var proj = Matrix.OrthoLH(halfExtent * 2.0f, halfExtent * 2.0f, 1.0f, farDist * 2.0f);

            return view * proj;
        }
    }
}
