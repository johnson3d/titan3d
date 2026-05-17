#ifndef _HEMISPHERE_MAPPING_INC_
#define _HEMISPHERE_MAPPING_INC_

#include "../../../Inc/Math.cginc"

// Hemisphere Texture Mapping Utilities
// Provides bidirectional mapping between upper hemisphere directions and 2D UV coordinates.
// Used for sky fallback textures when screen-space ray tracing fails.
//
// Mapping method: Octahedral hemisphere projection (equal-area, low distortion)
// Reference: [Claberg 2008] "A Survey of Efficient Representations for Independent Unit Vectors"
// Only the upper hemisphere (z >= 0) is mapped to the full [0,1]^2 UV square.

// Maps a unit direction on the upper hemisphere (z >= 0) to [0,1]^2 UV
// Uses octahedral equal-area projection for minimal distortion
float2 HemisphereDirToUV(float3 direction)
{
    float3 normalizedDir = normalize(direction);

    // Fold into upper hemisphere if needed
    normalizedDir.z = max(normalizedDir.z, 0.0);
    normalizedDir = normalize(normalizedDir);

    // Octahedral projection: project onto L1 unit sphere then unfold upper hemisphere
    float invL1Norm = 1.0 / (abs(normalizedDir.x) + abs(normalizedDir.y) + abs(normalizedDir.z));
    float2 octUV = normalizedDir.xy * invL1Norm;

    // Remap from [-1,1] to [0,1]
    return octUV * 0.5 + 0.5;
}

// Maps a [0,1]^2 UV back to a unit direction on the upper hemisphere (z >= 0)
float3 HemisphereUVToDir(float2 uv)
{
    // Remap from [0,1] to [-1,1]
    float2 octUV = uv * 2.0 - 1.0;

    // Inverse octahedral mapping for upper hemisphere
    float3 direction;
    direction.z = 1.0 - abs(octUV.x) - abs(octUV.y);
    direction.xy = octUV;

    // direction.z is guaranteed >= 0 for valid inputs within [0,1]^2
    direction.z = max(direction.z, 0.0);
    return normalize(direction);
}

// Compute texel solid angle weight for importance-correct integration
// Accounts for the non-uniform area-to-angle mapping of octahedral projection
float HemisphereTexelSolidAngle(float2 uv, float textureSize)
{
    float3 direction = HemisphereUVToDir(uv);
    // The Jacobian of the octahedral map: solid angle per texel ~ z component
    // Normalized so that integral over hemisphere = 2*PI
    float texelArea = 1.0 / (textureSize * textureSize);
    return 2.0 * PI * direction.z * texelArea;
}

// Bilinear sample helper with hardware filtering
// sampleTexture: the hemisphere-mapped texture
// sampleSampler: bilinear/trilinear sampler
// direction: world-space direction (upper hemisphere assumed)
float3 SampleHemisphereMap(Texture2D sampleTexture, SamplerState sampleSampler, float3 direction)
{
    float2 uv = HemisphereDirToUV(direction);
    return sampleTexture.SampleLevel(sampleSampler, uv, 0).rgb;
}

// Roughness-aware mip-level sample for pre-filtered sky
float3 SampleHemisphereMapLod(Texture2D sampleTexture, SamplerState sampleSampler, float3 direction, float mipLevel)
{
    float2 uv = HemisphereDirToUV(direction);
    return sampleTexture.SampleLevel(sampleSampler, uv, mipLevel).rgb;
}

#endif // _HEMISPHERE_MAPPING_INC_
