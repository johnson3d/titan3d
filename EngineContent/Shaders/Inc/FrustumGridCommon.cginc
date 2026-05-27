#ifndef _FRUSTUM_GRID_COMMON_INC_
#define _FRUSTUM_GRID_COMMON_INC_

// Frustum 3D Grid utilities
// Mirrors TtFrustumGrid3D.cs on the C# side.
// XY: screen pixels / CellPixelSize,  Z: log-distributed depth slices.
//
// cbuffer fields expected (add to your per-scene or per-camera cbuffer):
//   int3   FrustumGridSize;          // (sizeX, sizeY, sizeZ)
//   float3 FrustumGridZParams;       // (B, O, S) from CalculateGridZParams
//   uint   FrustumGridPixelSizeShift;// log2(CellPixelSize)
//   uint   FrustumGridTotalCells;    // sizeX * sizeY * sizeZ

struct FFrustumGridCellHeader
{
    uint Count;
    uint DataStartOffset;
};

// ---------------------------------------------------------------------------
// Coordinate helpers
// ---------------------------------------------------------------------------

// Pixel position + linear view depth -> 3D cell coordinate
uint3 ComputeFrustumGridCellCoord(uint2 pixelPos, float viewDepth,
    uint gridPixelSizeShift, float3 gridZParams, int3 gridSize)
{
    uint zSlice = (uint)max(0, log2(viewDepth * gridZParams.x + gridZParams.y) * gridZParams.z);
    zSlice = min(zSlice, (uint)(gridSize.z - 1));
    return uint3(pixelPos >> gridPixelSizeShift, zSlice);
}

// 3D cell coord -> flat buffer index.  Layout: (z * sizeY + y) * sizeX + x
uint ComputeFrustumGridCellIndex(uint3 cellCoord, int3 gridSize)
{
    return (cellCoord.z * (uint)gridSize.y + cellCoord.y) * (uint)gridSize.x + cellCoord.x;
}

// Combined: pixel + depth -> flat index
uint ComputeFrustumGridCellIndexFromPixel(uint2 pixelPos, float viewDepth,
    uint gridPixelSizeShift, float3 gridZParams, int3 gridSize)
{
    uint3 coord = ComputeFrustumGridCellCoord(pixelPos, viewDepth,
        gridPixelSizeShift, gridZParams, gridSize);
    return ComputeFrustumGridCellIndex(coord, gridSize);
}

// Inverse: Z slice -> view-space depth at the start of that slice
float ComputeDepthFromZSlice(uint zSlice, float3 gridZParams)
{
    // slice = log2(z * B + O) * S  =>  z = (2^(slice/S) - O) / B
    return max(0.0f, (exp2((float)zSlice / gridZParams.z) - gridZParams.y) / gridZParams.x);
}

// ---------------------------------------------------------------------------
// Grid buffer declarations for shading passes (read-only SRV)
// ---------------------------------------------------------------------------
StructuredBuffer<FFrustumGridCellHeader> PointGridHeaders DX_AUTOBIND;
StructuredBuffer<uint> PointGridDataIndices DX_AUTOBIND;

StructuredBuffer<FFrustumGridCellHeader> SpotGridHeaders DX_AUTOBIND;
StructuredBuffer<uint> SpotGridDataIndices DX_AUTOBIND;

#endif // _FRUSTUM_GRID_COMMON_INC_
