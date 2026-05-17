#ifndef _HEMISPHERE_SKY_FALLBACK_INC_
#define _HEMISPHERE_SKY_FALLBACK_INC_

#include "../../../Inc/Math.cginc"
#include "HemisphereMapping.cginc"

// Hemisphere Sky Fallback for Screen-Space Ray Tracing
// When SS-RT fails (ray exits screen, hits depth discontinuity, etc.),
// use pre-captured hemisphere sky texture as ambient lighting fallback.
//
// Usage in miss/fallback path:
//   float3 skyFallback = EvaluateSkyFallback(rayDir, normal, roughness);

// Resources expected to be bound by the pipeline
// Texture2D HemisphereSkyTexture : register(t?);
// SamplerState SampHemisphereSky : register(s?);

// Evaluate sky fallback radiance for a given ray direction
// rayDirection: world-space direction of the failed SS-RT ray (normalized)
// hemisphereSkyTex: pre-generated hemisphere-mapped sky texture
// samplerState: bilinear or trilinear sampler
float3 EvaluateSkyFallback(
    Texture2D hemisphereSkyTex,
    SamplerState samplerState,
    float3 rayDirection)
{
    // Ensure direction points to upper hemisphere for valid lookup
    float3 lookupDir = rayDirection;
    lookupDir.y = max(lookupDir.y, 0.001);
    lookupDir = normalize(lookupDir);

    return SampleHemisphereMap(hemisphereSkyTex, samplerState, lookupDir);
}

// Roughness-aware sky fallback using pre-filtered mip chain
// Rougher surfaces sample higher mip levels for broader sky integration
// roughness: material roughness [0, 1]
// maxMipLevel: maximum mip level available in the hemisphere sky texture
float3 EvaluateSkyFallbackRough(
    Texture2D hemisphereSkyTex,
    SamplerState samplerState,
    float3 rayDirection,
    float roughness,
    float maxMipLevel)
{
    float3 lookupDir = rayDirection;
    lookupDir.y = max(lookupDir.y, 0.001);
    lookupDir = normalize(lookupDir);

    float mipLevel = roughness * maxMipLevel;
    return SampleHemisphereMapLod(hemisphereSkyTex, samplerState, lookupDir, mipLevel);
}

// Combined fallback with horizon darkening and ground color blending
// Smoothly transitions from sky to ground color near the horizon
// to avoid harsh seams where hemisphere mapping reaches its boundary
float3 EvaluateSkyFallbackWithHorizon(
    Texture2D hemisphereSkyTex,
    SamplerState samplerState,
    float3 rayDirection,
    float roughness,
    float maxMipLevel,
    float3 groundColor,
    float horizonFadeWidth)
{
    float3 dir = normalize(rayDirection);

    // Elevation angle: 0 at horizon, 1 at zenith, negative below horizon
    float elevation = dir.y;

    // Below horizon: return ground color directly
    if (elevation <= 0.0)
        return groundColor;

    // Compute sky radiance from hemisphere map
    float mipLevel = roughness * maxMipLevel;
    float3 skyColor = SampleHemisphereMapLod(hemisphereSkyTex, samplerState, dir, mipLevel);

    // Smooth blend near horizon to avoid boundary artifacts
    float horizonBlend = saturate(elevation / max(horizonFadeWidth, 0.001));
    horizonBlend = horizonBlend * horizonBlend; // quadratic ease-in

    return lerp(groundColor, skyColor, horizonBlend);
}

// Directional ambient occlusion weighted fallback
// Modulates sky contribution by bent normal visibility
float3 EvaluateSkyFallbackAO(
    Texture2D hemisphereSkyTex,
    SamplerState samplerState,
    float3 rayDirection,
    float3 bentNormal,
    float ambientOcclusion,
    float roughness,
    float maxMipLevel)
{
    float3 skyColor = EvaluateSkyFallbackRough(
        hemisphereSkyTex, samplerState,
        rayDirection, roughness, maxMipLevel);

    // Weight by bent normal alignment and AO factor
    float bentWeight = saturate(dot(normalize(rayDirection), normalize(bentNormal)));
    return skyColor * ambientOcclusion * lerp(1.0, bentWeight, 0.5);
}

#endif // _HEMISPHERE_SKY_FALLBACK_INC_
