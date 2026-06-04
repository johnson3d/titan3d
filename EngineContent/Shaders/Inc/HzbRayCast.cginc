#ifndef _HZB_RAYCAST_H_
#define _HZB_RAYCAST_H_

// ============================================================================
// HZB Screen-Space Ray Cast
//
// Uniform-step linear march with HZB acceleration.
// Algorithm matches UE5 CastScreenSpaceRay (SSRTRayCast.ush):
//
//   1. Divide ray into NumSteps equal steps in Screen space (NDC xy + DeviceZ)
//   2. Convert to UV-Z space: UV [0,1], Z = DeviceZ (NDC-Z)
//   3. Each step: sample HZB at current mip, compare rayZ vs sceneZ
//      - depthDiff = rayZ - sceneZ
//      - hit  = abs(depthDiff + tolerance) < tolerance  →  diff ∈ [-2*tol, 0]
//      - behind = (depthDiff + tolerance) < -tolerance  →  ray went behind geometry
//   4. On hit: TimeLerp between previous and current step for sub-pixel accuracy
//   5. Mip increases with step count (controlled by roughness parameter)
//
// Conventions (TitanEngine):
//   - Inverse-Z: NDC-Z=1 near, NDC-Z=0 far.  min(NDC-Z) = farthest surface.
//   - HZB is Texture2D<float>, single-channel min NDC-Z, mip0 = screen/2.
//   - Screen space: NDC xy ∈ [-1,1], DeviceZ ∈ [0,1].
//   - UV space: UV ∈ [0,1], UV.y = 0.5 - NDC.y * 0.5 (Y-flipped).
// ============================================================================

#include "SysFunction.cginc"
#include "../CBuffer/VarBase_PerCamera.cginc"

// ---- Result structure ----
struct FHzbRayCastResult
{
    bool   bHit;
    float2 HitUV;
    float  HitNdcZ;
    float  HitLinearZ;
};

// ---- Screen edge clipping (UE GetStepScreenFactorToClipAtScreenEdge) ----
//
// Returns a scale factor ∈ (0,1] to multiply RayStepScreen so the ray
// doesn't extend outside NDC [-1,1].
float HzbRayCast_ClipToScreenEdge(float2 rayStartScreen, float2 rayStepScreen)
{
    float invFactor = 0.5 * length(rayStepScreen);
    float2 absEnd = abs(rayStepScreen + rayStartScreen * invFactor);
    float2 overshoot = max(absEnd - invFactor, 0.0);
    float2 scale = 1.0 - overshoot / max(abs(rayStepScreen), 1e-8);
    return min(scale.x, scale.y) / max(invFactor, 1e-8);
}

// ---- Main HZB ray cast (UE CastScreenSpaceRay equivalent) ----
//
// All inputs are in Screen space (NDC xy [-1,1] + DeviceZ [0,1]).
// This matches UE's FSSRTRay: RayStartScreen + RayStepScreen.
//
// Parameters:
//   hzbTex          - HZB texture (R32F, single-channel min NDC-Z)
//   hzbSamp         - Point clamp sampler
//   rayStartScreen  - float3(NDC.xy, DeviceZ) at ray origin
//   rayStepScreen   - float3(deltaNDC.xy, deltaDeviceZ) for the full ray
//   compareTolerance - Depth comparison tolerance (in NDC-Z units).
//                      UE uses max(abs(RayStepScreen.z), slopeTerm) * step.
//                      For Contact Shadow, abs(RayStepScreen.z) * step works well.
//   numSteps        - Number of uniform steps
//   stepOffset      - Sub-step offset ∈ [0,1) for temporal jitter (e.g. dithered)
//   startMipLevel   - Initial HZB mip level (typically 0 for contact shadow, 1 for SSR)
//   roughness       - Controls mip increase rate per step.
//                      0 = always mip 0 (sharp, for contact shadow / mirror SSR).
//                      Higher = coarser mip for rough surfaces (SSR / SSGI).
//
FHzbRayCastResult HzbRayCast(
    Texture2D<float> hzbTex, SamplerState hzbSamp,
    float3 rayStartScreen, float3 rayStepScreen,
    float compareTolerance,
    uint numSteps, float stepOffset,
    float startMipLevel, float roughness)
{
    FHzbRayCastResult result;
    result.bHit = false;
    result.HitUV = 0;
    result.HitNdcZ = 0;
    result.HitLinearZ = 0;

    // Screen → UV-Z: UV = NDC.xy * (0.5, -0.5) + 0.5;  Z = DeviceZ (unchanged)
    float3 rayStartUVz = float3(rayStartScreen.xy * float2(0.5, -0.5) + 0.5, rayStartScreen.z);
    float3 rayStepUVz  = float3(rayStepScreen.xy  * float2(0.5, -0.5),       rayStepScreen.z);

    // Per-step increment
    float stepScale = 1.0 / (float)numSteps;
    float stepCompareTolerance = compareTolerance * stepScale;
    rayStepUVz *= stepScale;

    // Initial position with sub-step offset
    float3 curUVz = rayStartUVz + rayStepUVz * stepOffset;

    float lastDepthDiff = 0.0;
    float mipLevel = startMipLevel;

    [loop]
    for (uint i = 0u; i < numSteps; ++i)
    {
        // Advance one step
        float3 sampleUVz = curUVz + rayStepUVz * (float)(i + 1);

        // Bail if UV is out of [0,1] bounds
        if (any(sampleUVz.xy < 0.0) || any(sampleUVz.xy > 1.0))
            break;

        // Sample HZB at current mip
        float sceneDeviceZ = hzbTex.SampleLevel(hzbSamp, sampleUVz.xy, mipLevel).r;

        // Depth difference (Screen-Z / DeviceZ space):
        //   positive → ray is in front of surface (inverse-Z: ray has larger NDC-Z)
        //   negative → ray is behind surface
        float depthDiff = sampleUVz.z - sceneDeviceZ;

        // Hit test: depthDiff ∈ [-2*tol, 0]
        //   abs(depthDiff + tol) < tol  ↔  -2*tol < depthDiff < 0
        //   i.e. ray is behind surface but within tolerance thickness
        bool isHit = abs(depthDiff + stepCompareTolerance) < stepCompareTolerance;

        if (isHit)
        {
            // TimeLerp: interpolate between previous and current step
            // for sub-pixel accuracy. Uses the sign change in depthDiff.
            float timeLerp = saturate(lastDepthDiff / (lastDepthDiff - depthDiff));
            float intersectStep = (float)i + timeLerp;

            float3 hitUVz = curUVz + rayStepUVz * intersectStep;

            result.bHit = true;
            result.HitUV = hitUVz.xy;
            result.HitNdcZ = hitUVz.z;
            result.HitLinearZ = LinearFromDepth(hitUVz.z);
            return result;
        }

        lastDepthDiff = depthDiff;

        // Increase mip every 2 steps, controlled by roughness.
        // At roughness=0 (contact shadow), mip stays at startMipLevel.
        if ((i & 1u) == 1u)
            mipLevel += (8.0 / (float)numSteps) * roughness;
    }

    return result;
}

// ---- Convenience wrapper: world-space ray → screen-space HZB trace ----
//
// Projects world-space ray to Screen space, builds FSSRTRay-equivalent
// (rayStartScreen + rayStepScreen), clips to screen edge, computes
// compareTolerance, then calls HzbRayCast.
//
FHzbRayCastResult HzbRayCastFromWorldRay(
    Texture2D<float> hzbTex, SamplerState hzbSamp,
    float3 rayOriginWorld, float3 rayDirWorld, float rayLength,
    uint numSteps, float stepOffset, float roughness)
{
    FHzbRayCastResult failResult;
    failResult.bHit = false;
    failResult.HitUV = 0;
    failResult.HitNdcZ = 0;
    failResult.HitLinearZ = 0;

    float3 rayEndWorld = rayOriginWorld + rayDirWorld * rayLength;

    // Project to clip space
    float4 startClip = mul(float4(rayOriginWorld, 1.0), GetViewPrjMtx());
    float4 endClip   = mul(float4(rayEndWorld,    1.0), GetViewPrjMtx());

    // Reject if start is behind camera
    if (startClip.w <= 0.0)
        return failResult;

    // Clip ray end to near plane if behind camera
    if (endClip.w <= 0.0)
    {
        float tClip = startClip.w / (startClip.w - endClip.w) * 0.95;
        rayEndWorld = rayOriginWorld + rayDirWorld * (rayLength * tClip);
        endClip = mul(float4(rayEndWorld, 1.0), GetViewPrjMtx());
        if (endClip.w <= 0.0)
            return failResult;
    }

    // NDC = Clip.xyz / Clip.w
    float3 rayStartScreen = startClip.xyz / startClip.w;
    float3 rayEndScreen   = endClip.xyz   / endClip.w;
    float3 rayStepScreen  = rayEndScreen - rayStartScreen;

    // Clip to screen edge (NDC [-1,1])
    float clipFactor = HzbRayCast_ClipToScreenEdge(rayStartScreen.xy, rayStepScreen.xy);
    clipFactor = min(clipFactor, 1.0);
    rayStepScreen *= clipFactor;

    // CompareTolerance: per-ray Z span scaled by step count.
    // This is the UE formula for perspective projection.
    float compareTolerance = abs(rayStepScreen.z);

    return HzbRayCast(hzbTex, hzbSamp,
                      rayStartScreen, rayStepScreen,
                      compareTolerance,
                      numSteps, stepOffset,
                      0.0, roughness);
}

#endif // _HZB_RAYCAST_H_
