#ifndef _AdvanceShadow_cginc_
#define _AdvanceShadow_cginc_
#include "../../Inc/GlobalDefine.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"

Texture2DArray GShadowMapArray DX_AUTOBIND;
SamplerState Samp_GShadowMapArray DX_AUTOBIND;
StructuredBuffer<FAdvShadowNodeData> QTreeNodeBuffer DX_AUTOBIND;

// Virtual → physical page table (indexed by virtual page index)
StructuredBuffer<FVSMPageTableEntry> VSMPageTable DX_AUTOBIND;

// Per-clipmap-page data: VP matrix + near/far (indexed by local page index within clipmap)
StructuredBuffer<FVSMClipmapPageData> ClipmapPageBuffer DX_AUTOBIND;

cbuffer cbAdvanceShadow DX_AUTOBIND
{
    float2 BoxMin;
    float2 BoxMax;
    
    int NodeCount;
    int PageCount;
    float MaxShadowDistance;
    int MaxDeepLevel;
    
    float EsmConstant;
    float MaxExp;
    float GaussSigma;
    int PoolDimPages;

    int ClipmapPageResolution;
    int ClipmapLevelCount;
    float ClipmapBaseHalfExtent;
    int ClipmapPagesPerDim;

    int ClipmapVirtualPageOffset;
    float3 ClipmapCenter; // snapped center in light-view space (XY)

    int UseESM;
    int PcfRadius;        // PCF kernel half-size: 1=3x3, 2=5x5, 3=7x7, etc.
    float PcfDepthBias;   // depth bias for shadow comparison
    float _cbPad0;

    // World-to-light-view rotation matrix (3x3 stored as 3 float4 rows, w unused)
    float4 WorldToLightViewRow0; // right axis
    float4 WorldToLightViewRow1; // up axis
    float4 WorldToLightViewRow2; // forward (light dir) axis

    FAdvShadowLayerData LayerData[32];
};

// ---- Clipmap shadow lookup ----

// Transform world position to light-view space using the rotation matrix in cbuffer
float3 WorldToLightView(float3 worldPos)
{
    float3 result;
    result.x = dot(worldPos, WorldToLightViewRow0.xyz);
    result.y = dot(worldPos, WorldToLightViewRow1.xyz);
    result.z = dot(worldPos, WorldToLightViewRow2.xyz);
    return result;
}

// Determine clipmap level from light-view space XY distance to clipmap center.
// Returns -1 if outside all levels.
int GetClipmapLevel(float2 lightViewXY)
{
    float2 rel = lightViewXY - ClipmapCenter.xy;
    for (int lv = 0; lv < ClipmapLevelCount; lv++)
    {
        float halfExt = ClipmapBaseHalfExtent * (float)(1 << lv);
        if (abs(rel.x) <= halfExt && abs(rel.y) <= halfExt)
            return lv;
    }
    return -1;
}

// Get shadow value from clipmap for a world position.
// ---- QTree shadow lookup (legacy / local lights) ----

int GetPageNode(float2 pos)
{
    if (any(pos < BoxMin) || any(pos >= BoxMax))
        return -1;
    
    float dist = length(pos - CameraPosition.xz);
    if (dist > MaxShadowDistance)
        return -1;
    int level = (int) ((MaxShadowDistance - dist) * MaxDeepLevel / MaxShadowDistance);
    float2 gridSize = LayerData[level].LayerGridSize;
    
    int2 sigment = (int2) ((pos - BoxMin) / gridSize);
    int2 layer = LayerData[level].LayerStartAndSide;
    int index = layer.x + (sigment.y * layer.y + sigment.x);
    if (index >= NodeCount)
        return -1;
    
    return index;
}

//https://blog.csdn.net/Jaihk662/article/details/127259797
float GetESMValue(float sampleDepth, float near, float far)
{
    float linearDepth = LinearFromDepth(sampleDepth, near, far);
    
    float safeC = 1.0f;
    float safeInput = min(EsmConstant * safeC * linearDepth, MaxExp);
    float esmValue = exp(safeInput);
    
    return esmValue;
}

// ---- Clipmap shadow lookup (directional light) ----

// Returns shadow attenuation (0=full shadow, 1=lit).
float GetClipmapShadow(float3 worldPos)
{
    // Transform world position to light-view space for page selection
    float3 posLV = WorldToLightView(worldPos);

    int level = GetClipmapLevel(posLV.xy);
    if (level < 0)
        return 1.0f;

    // Determine page coordinates within this level from light-view XY
    float halfExt = ClipmapBaseHalfExtent * (float)(1 << level);
    float2 rel = posLV.xy - ClipmapCenter.xy;
    float2 normalized = (rel + halfExt) / (halfExt * 2.0f);
    int2 pageCoord = clamp(int2(normalized * ClipmapPagesPerDim), int2(0, 0), int2(ClipmapPagesPerDim - 1, ClipmapPagesPerDim - 1));

    // Local page index (within clipmap, used for ClipmapPageBuffer)
    int pagesPerLevel = ClipmapPagesPerDim * ClipmapPagesPerDim;
    int localPageIndex = level * pagesPerLevel + pageCoord.y * ClipmapPagesPerDim + pageCoord.x;

    // Virtual page index (global, used for VSMPageTable)
    int virtualPageIndex = localPageIndex + ClipmapVirtualPageOffset;

    // Look up physical page from the page table
    FVSMPageTableEntry pageEntry = VSMPageTable[virtualPageIndex];
    if (pageEntry.PhysicalPageCoord.x < 0)
        return 1.0f;

    // Physical array slice index
    int sliceIndex = pageEntry.PhysicalPageCoord.y * PoolDimPages + pageEntry.PhysicalPageCoord.x;

    // Fetch per-page VP matrix and near/far from ClipmapPageBuffer
    FVSMClipmapPageData pageData = ClipmapPageBuffer[localPageIndex];

    // Skip pages with no valid camera data (no casters were gathered)
    if (pageData.ZFar <= 0.0f)
        return 1.0f;

    // Transform receiver to this page's clip space using the camera VP
    float4 clipPos = mul(float4(worldPos, 1.0f), pageData.ViewProj);
    clipPos.xyz /= clipPos.w;

    // NDC to [0,1] UV
    float2 shadowUV = clipPos.xy * 0.5f + 0.5f;
    shadowUV.y = 1.0f - shadowUV.y;
    float receiverDepth = clipPos.z;

    // Clamp UV to valid range (page selection guarantees we're close to [0,1],
    // small overshoot from axis alignment tolerance is handled by clamping)
    shadowUV = saturate(shadowUV);
    receiverDepth = saturate(receiverDepth);

    // Sample shadow map from the physical page using VP-projected UV
    float occluderDepth = GShadowMapArray.SampleLevel(Samp_GShadowMapArray, float3(shadowUV, sliceIndex), 0).r;

    if (UseESM)
    {
        receiverDepth = max(receiverDepth - 0.003f, 0.0f);
        float esmValue = GetESMValue(receiverDepth, pageData.ZNear, pageData.ZFar);
        return saturate(occluderDepth / esmValue);
    }
    else
    {
        // PCF soft shadow with configurable kernel size.
        // Skip samples that would fall outside [0,1] UV to avoid
        // bleeding across page boundaries in the texture array.
        float texelSize = 1.0f / (float)ClipmapPageResolution;
        float shadow = 0.0f;
        int r = min(PcfRadius, 7);
        float sampleCount = 0.0f;
        float margin = texelSize * 0.5f;

        // Convert world-space bias (PcfDepthBias) to NDC-space for this page
        float depthRange = pageData.ZFar - pageData.ZNear;
        float ndcBias = depthRange > 0.0f ? PcfDepthBias / depthRange : 0.0f;

        for (int oy = -r; oy <= r; oy++)
        {
            for (int ox = -r; ox <= r; ox++)
            {
                float2 offsetUV = shadowUV + float2(ox, oy) * texelSize;
                if (offsetUV.x < margin || offsetUV.x > 1.0f - margin ||
                    offsetUV.y < margin || offsetUV.y > 1.0f - margin)
                    continue;
                float sampleDepth = GShadowMapArray.SampleLevel(Samp_GShadowMapArray, float3(offsetUV, sliceIndex), 0).r;
#if USE_INVERSE_Z == 1
                shadow += (receiverDepth + ndcBias) >= sampleDepth ? 1.0f : 0.0f;
#else
                shadow += (receiverDepth - ndcBias) <= sampleDepth ? 1.0f : 0.0f;
#endif
                sampleCount += 1.0f;
            }
        }
        return sampleCount > 0.0f ? shadow / sampleCount : 1.0f;
    }
}

#endif//_AdvanceShadow_cginc_