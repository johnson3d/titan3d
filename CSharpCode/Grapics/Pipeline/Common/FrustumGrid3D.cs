using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Pipeline.Common
{
    /// <summary>
    /// A 3D grid in view-frustum space, similar to UE's Clustered Forward LightGrid.
    /// XY cells are screen-space tiles; Z cells are logarithmically distributed depth slices.
    /// Each cell can hold indices into external data arrays (lights, fog densities, etc.).
    /// </summary>
    public class TtFrustumGrid3D : IDisposable
    {
        #region Grid Configuration
        /// <summary>
        /// Pixel size of each XY cell (default 64, matching UE's r.Forward.LightGridPixelSize).
        /// Must be power of two for shift-based division on GPU.
        /// </summary>
        public uint CellPixelSize { get; set; } = 64;

        /// <summary>
        /// Number of depth slices along the Z axis (default 32, matching UE's r.Forward.LightGridSizeZ).
        /// </summary>
        public int GridSizeZ { get; set; } = 32;

        /// <summary>
        /// Controls how slices are distributed between near/far.
        /// Higher values push more slices toward the near plane (default 4.05, matching UE).
        /// </summary>
        public float DepthDistributionScale { get; set; } = 4.05f;

        /// <summary>
        /// Maximum number of data indices per cell (e.g., lights per cell).
        /// Exceeding this silently clips; GPU linked-list mode can lift this limit later.
        /// </summary>
        public uint MaxIndicesPerCell { get; set; } = 32;
        #endregion

        #region Computed Grid Dimensions
        /// <summary>
        /// Number of cells along X (computed from screen width / CellPixelSize).
        /// </summary>
        public int CulledGridSizeX { get; private set; }

        /// <summary>
        /// Number of cells along Y (computed from screen height / CellPixelSize).
        /// </summary>
        public int CulledGridSizeY { get; private set; }

        /// <summary>
        /// Total number of cells (SizeX * SizeY * GridSizeZ).
        /// </summary>
        public int TotalCellCount { get; private set; }

        /// <summary>
        /// Viewport width in pixels (set during OnResize).
        /// </summary>
        public float ViewportWidth { get; private set; }

        /// <summary>
        /// Viewport height in pixels (set during OnResize).
        /// </summary>
        public float ViewportHeight { get; private set; }

        /// <summary>
        /// log2(CellPixelSize) — used by GPU for shift-based division.
        /// </summary>
        public int CellPixelSizeShift { get; private set; }

        /// <summary>
        /// ZParams (B, O, S) for the logarithmic depth distribution.
        /// Shader formula: slice = log2(viewDepth * B + O) * S
        /// </summary>
        public Vector3 GridZParams { get; private set; }
        #endregion

        #region GPU Resources
        private TtGpuBuffer<FCellHeader> GridBuffer;
        #endregion

        #region Z-Params Calculation (ported from UE CalculateGridZParams)
        /// <summary>
        /// Calculate the Z distribution parameters.
        /// Ported from UE's CalculateGridZParams in RenderUtils.h.
        /// </summary>
        /// <remarks>
        /// The mapping is: slice = log2(z * B + O) * S
        /// where B, O are solved so that:
        ///   - slice 0 maps to nearPlane + NearOffset
        ///   - slice (gridSizeZ-1) maps to farPlane
        /// </remarks>
        public static Vector3 CalculateGridZParams(float nearPlane, float farPlane, float depthDistributionScale, int gridSizeZ)
        {
            const double NearOffset = 0.095 * 100.0; // 9.5 world units, same as UE
            double distributionScale = depthDistributionScale;

            double nearWithOffset = nearPlane + NearOffset;
            double far = farPlane;

            // O = (F - N * 2^(gridSizeZ/S)) / (F - N)
            double offset = (far - nearWithOffset * Math.Pow(2.0, gridSizeZ / distributionScale)) / (far - nearWithOffset);
            // B = (1 - O) / N
            double scale = (1.0 - offset) / nearWithOffset;

            return new Vector3((float)scale, (float)offset, (float)distributionScale);
        }

        /// <summary>
        /// Compute the Z slice index for a given view-space depth.
        /// Matches the GPU-side ComputeLightGridCellCoordinate.
        /// </summary>
        public int ComputeZSlice(float viewDepth)
        {
            var zParams = GridZParams;
            int slice = (int)Math.Max(0, Math.Log2(viewDepth * zParams.X + zParams.Y) * zParams.Z);
            return Math.Min(slice, GridSizeZ - 1);
        }

        /// <summary>
        /// Inverse: compute the view-space depth at the start of a given Z slice.
        /// Useful for debug visualization.
        /// </summary>
        public float ComputeDepthFromZSlice(int zSlice)
        {
            var zParams = GridZParams;
            // slice = log2(z * B + O) * S  →  z = (2^(slice/S) - O) / B
            float depth = (float)((Math.Pow(2.0, (double)zSlice / zParams.Z) - zParams.Y) / zParams.X);
            return Math.Max(depth, 0.0f);
        }
        #endregion

        #region Grid Cell Indexing
        /// <summary>
        /// Compute cell coordinate from pixel position and view-space depth.
        /// </summary>
        public Vector3i ComputeCellCoordinate(uint pixelX, uint pixelY, float viewDepth)
        {
            int cellX = (int)(pixelX >> CellPixelSizeShift);
            int cellY = (int)(pixelY >> CellPixelSizeShift);
            int cellZ = ComputeZSlice(viewDepth);
            return new Vector3i(
                Math.Min(cellX, CulledGridSizeX - 1),
                Math.Min(cellY, CulledGridSizeY - 1),
                cellZ);
        }

        /// <summary>
        /// Linearize a 3D cell coordinate to a flat buffer index.
        /// Layout: (z * SizeY + y) * SizeX + x
        /// </summary>
        public int ComputeCellIndex(in Vector3i cellCoord)
        {
            return (cellCoord.Z * CulledGridSizeY + cellCoord.Y) * CulledGridSizeX + cellCoord.X;
        }

        /// <summary>
        /// Unpack a flat cell index back to 3D coordinates.
        /// </summary>
        public Vector3i UnpackCellIndex(int flatIndex)
        {
            int sliceSize = CulledGridSizeX * CulledGridSizeY;
            int z = flatIndex / sliceSize;
            int remainder = flatIndex % sliceSize;
            int y = remainder / CulledGridSizeX;
            int x = remainder % CulledGridSizeX;
            return new Vector3i(x, y, z);
        }
        #endregion

        #region Per-Cell Data (CPU side, for building / debugging)
        /// <summary>
        /// GPU-visible header per cell: stores count + start offset into a global index list.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct FCellHeader
        {
            /// <summary>Number of indices stored in this cell.</summary>
            public uint Count;
            /// <summary>Start offset into the global data index buffer.</summary>
            public uint DataStartOffset;
        }
        #endregion

        #region Resize / Rebuild
        /// <summary>
        /// Recalculate grid dimensions when the screen resolution or camera changes.
        /// Recreates GPU buffers.
        /// </summary>
        public unsafe void OnResize(float screenWidth, float screenHeight, float cameraNear, float cameraFar)
        {
            ViewportWidth = screenWidth;
            ViewportHeight = screenHeight;

            CellPixelSizeShift = (int)Math.Log2(CellPixelSize);

            CulledGridSizeX = DivideAndRoundUp((int)screenWidth, (int)CellPixelSize);
            CulledGridSizeY = DivideAndRoundUp((int)screenHeight, (int)CellPixelSize);
            TotalCellCount = CulledGridSizeX * CulledGridSizeY * GridSizeZ;

            // Reserve last slice for extended range (same as UE)
            GridZParams = CalculateGridZParams(cameraNear, cameraFar + 10.0f, DepthDistributionScale, GridSizeZ - 1);

            RecreateGpuBuffers();
        }

        private unsafe void RecreateGpuBuffers()
        {
            DisposeGpuResources();

            if (TotalCellCount <= 0)
                return;

            GridBuffer = new TtGpuBuffer<FCellHeader>();
            GridBuffer.SetSize((uint)TotalCellCount, null,
                NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        }
        #endregion

        #region Accessors for GPU Binding
        public TtGpuBuffer<FCellHeader> GetGridBuffer() => GridBuffer;
        public NxRHI.TtBuffer GpuBuffer => GridBuffer?.GpuBuffer as NxRHI.TtBuffer;
        public NxRHI.TtSrView Srv => GridBuffer?.Srv;
        public NxRHI.TtUaView Uav => GridBuffer?.Uav;
        #endregion

        #region Dispose
        public void Dispose()
        {
            DisposeGpuResources();
        }

        private void DisposeGpuResources()
        {
            GridBuffer?.Dispose();
            GridBuffer = null;
        }
        #endregion

        #region Utility
        private static int DivideAndRoundUp(int numerator, int denominator)
        {
            return (numerator + denominator - 1) / denominator;
        }
        #endregion
    }
}
