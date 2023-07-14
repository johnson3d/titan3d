#ifndef	_CLUSTER_CULLING_H_
#define _CLUSTER_CULLING_H_

#include "../../Inc/VertexLayout.cginc"
#include "FRaster.cginc"

struct FrustumCullData
{
    float3 RectMin;
    float3 RectMax;
};
struct FScreenRect
{
    int4	Pixels;

    // For HZB sampling
    int4	HZBTexels;
    int		HZBLevel;

    float	Depth;
};

struct FrustumParams
{
    float4 GpuDrivenCameraPlanes[6];

    float3 GpuDrivenFrustumMinPoint;
    float3 GpuDrivenFrustumMaxPoint;
};
cbuffer cbCameraFrustum DX_AUTOBIND
{
    FrustumParams FrustumInfo;
};

Texture2D<float2> HZBTexture;
SamplerState Samp_HZBTexture;

ByteAddressBuffer SrcClusterBuffer;
RWByteAddressBuffer VisClusterBuffer;

bool BoxCullFrustum(int clusterId)
{
    FClusterData clusterData = ClusterBuffer[clusterId];

    float3 center = (clusterData.BoundMin + clusterData.BoundMax) / 2;
    float3 extent = (clusterData.BoundMax - clusterData.BoundMin) / 2;

    float3 minPos = clusterData.BoundMin;
    float3 maxPos = clusterData.BoundMax;
    
    float outOfRange = dot(FrustumInfo.GpuDrivenFrustumMinPoint > maxPos, 1) + dot(FrustumInfo.GpuDrivenFrustumMaxPoint < minPos, 1);
    if (outOfRange > 0.5)
        return true;

    for (uint i = 0; i < 6; ++i)
    {
        float4 plane = FrustumInfo.GpuDrivenCameraPlanes[i];
        float3 absNormal = abs(plane.xyz);
        if ((dot(center, plane.xyz) - dot(absNormal, extent)) > -plane.w)
        {
            return true;
        }
    }
    return false;
}

int MipLevelForRect(int4 RectPixels, int DesiredFootprintPixels)
{
    const int MaxPixelOffset = DesiredFootprintPixels - 1;
    const int MipOffset = (int)log2((float)DesiredFootprintPixels) - 1;

    // Calculate lowest mip level that allows us to cover footprint of the desired size in pixels.
    // Start by calculating separate x and y mip level requirements.
    // 2 pixels of mip k cover 2^(k+1) pixels of mip 0. To cover at least n pixels of mip 0 by two pixels of mip k we need k to be at least k = ceil( log2( n ) ) - 1.
    // For integer n>1: ceil( log2( n ) ) = floor( log2( n - 1 ) ) + 1.
    // So k = floor( log2( n - 1 )
    // For integer n>1: floor( log2( n ) ) = firstbithigh( n )
    // So k = firstbithigh( n - 1 )
    // As RectPixels min/max are both inclusive their difference is one less than number of pixels (n - 1), so applying firstbithigh to this difference gives the minimum required mip.
    // NOTE: firstbithigh is a FULL rate instruction on GCN while log2 is QUARTER rate instruction.
    int2 MipLevelXY = firstbithigh(RectPixels.zw - RectPixels.xy);

    // Mip level needs to be big enough to cover both x and y requirements. Go one extra level down for 4x4 sampling.
    // firstbithigh(0) = -1, so clamping with 0 here also handles the n=1 case where mip 0 footprint is just 1 pixel wide/tall.
    int MipLevel = max(max(MipLevelXY.x, MipLevelXY.y) - MipOffset, 0);

    // MipLevel now contains the minimum MipLevel that can cover a number of pixels equal to the size of the rectangle footprint, but the HZB footprint alignments are quantized to powers of two.
    // The quantization can translate down the start of the represented range by up to 2^k-1 pixels, which can decrease the number of usable pixels down to 2^(k+1) - 2^k-1.
    // Depending on the alignment of the rectangle this might require us to pick one level higher to cover all rectangle footprint pixels.
    // Note that testing one level higher is always enough as this guarantees 2^(k+2) - 2^k usable pixels after alignment, which is more than the 2^(k+1) required pixels.

    // Transform coordinates down to coordinates of selected mip level and if they are not within reach increase level by one.
    MipLevel += any((RectPixels.zw >> MipLevel) - (RectPixels.xy >> MipLevel) > MaxPixelOffset) ? 1 : 0;

    return MipLevel;
}

FScreenRect GetScreenRect(int clusterId)
{
    FClusterData cluster = ClusterBuffer[clusterId];

    float3 minPos = cluster.BoundMin;
    float3 maxPos = cluster.BoundMax;

    FrustumCullData hzbData;

    // TODO:
    hzbData.RectMin = float3(99999, 99999, 99999);
    hzbData.RectMax = float3(-99999, -99999, -99999);

    #define EVAL_POINTS(PC0, PC1) \
        float3 PS0      = PC0.xyz / PC0.w; \
        float3 PS1      = PC1.xyz / PC1.w; \
        hzbData.RectMin    = min3(hzbData.RectMin, PS0, PS1); \
        hzbData.RectMax    = max3(hzbData.RectMax, PS0, PS1);

    float3 extent = maxPos - minPos;

    float dx = mul(float4(extent.x, 0, 0, 1), cluster.WVPMatrix);
    float dy = mul(float4(extent.y, 0, 0, 1), cluster.WVPMatrix);
    float dz = mul(float4(extent.z, 0, 0, 1), cluster.WVPMatrix);

    float4 PC000, PC111;
    {
        PC000 = mul(float4(minPos, 1), cluster.WVPMatrix);
        PC111 = mul(float4(maxPos, 1), cluster.WVPMatrix);

        EVAL_POINTS(PC000, PC111);
    }

    float4 PC100, PC001;
    {
        PC100 = PC000 + dx;
        PC001 = PC000 + dz;

        EVAL_POINTS(PC100, PC001);
    }

    float4 PC010, PC110;
    {
        PC010 = PC000 + dy;
        PC110 = PC100 + dy;

        EVAL_POINTS(PC010, PC110);
    }

    float4 PC011, PC101;
    {
        PC011 = PC010 + dz;
        PC101 = PC100 + dz;

        EVAL_POINTS(PC011, PC101);
    }
    #undef EVAL_POINTS


    FScreenRect ScreenRect;

    ScreenRect.Depth = hzbData.RectMax.z;
    ScreenRect.Pixels = float4(hzbData.RectMin.xy, hzbData.RectMax.xy); 
    ScreenRect.HZBTexels = int4(ScreenRect.Pixels.xy, max(ScreenRect.Pixels.xy, ScreenRect.Pixels.zw));

    ScreenRect.HZBLevel = MipLevelForRect(ScreenRect.HZBTexels, 4);
    // Transform HZB Mip 0 coordinates to coordinates of selected Mip level.
    ScreenRect.HZBTexels >>= ScreenRect.HZBLevel;

    return ScreenRect;
}

bool HZBCulling(int clusterId)
{
    FScreenRect ScreenRect = GetScreenRect(clusterId);

    int MipLevel = ScreenRect.HZBLevel;
    
    float4 Depth;
    Depth.x = HZBTexture.SampleLevel(Samp_HZBTexture, ScreenRect.HZBTexels.xw, MipLevel).r;
    Depth.y = HZBTexture.SampleLevel(Samp_HZBTexture, ScreenRect.HZBTexels.zw, MipLevel).r;
    Depth.z = HZBTexture.SampleLevel(Samp_HZBTexture, ScreenRect.HZBTexels.zy, MipLevel).r;
    Depth.w = HZBTexture.SampleLevel(Samp_HZBTexture, ScreenRect.HZBTexels.xy, MipLevel).r;

    float MinDepth = min(min3(Depth.x, Depth.y, Depth.z), Depth.w);
    
    return ScreenRect.Depth >= MinDepth;
}
bool IsVisible(uint clusterId)
{
    bool isFrustumCull = BoxCullFrustum(clusterId);
    
    if (!isFrustumCull)
    {
        return HZBCulling(clusterId);
    }
    
    return true;
}

groupshared uint MaxSrcCount;

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CS_ClusterCullingMain(uint DispatchThreadId : SV_DispatchThreadID, uint3 LocalThreadId : SV_GroupThreadID, uint3 GroupId : SV_GroupID)
{
    if (LocalThreadId.x == 0)
    {
        MaxSrcCount = SrcClusterBuffer.Load(0);
    }
    GroupMemoryBarrierWithGroupSync();
    
    if (DispatchThreadId.x >= MaxSrcCount)
    {
        return;
    }
    
    /*float HZBDepth = HZBTexture.SampleLevel(Samp_HZBTexture, int2(0, 0), 0).r;
    if (HZBDepth > 0)
        return;*/

    uint clusterId = SrcClusterBuffer.Load((1 + DispatchThreadId.x) * 4);
    if (!IsVisible(clusterId))
    {
        return;
    }
    // TODO:
    int index = 0;
    VisClusterBuffer.InterlockedAdd(0, 1, index);
    //VisClusterBuffer&ClusterBuffer [0] is the count of array
    VisClusterBuffer.Store((1 + index) * 4, clusterId);
}

#endif//_CLUSTER_CULLING_H_