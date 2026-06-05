#ifndef _HAIR_BRDF_H_
#define _HAIR_BRDF_H_

// Kajiya-Kay Hair BRDF with dual-lobe specular (Marschner-inspired).
// Reference: UE5 HairBsdf.ush, GPU Pro 2 Chapter "Hair Rendering and Shading"
//
// GBuffer convention for Hair:
//   RT1 stores the hair strand tangent direction T (from flowmap/tangent map).
//   RT0.a + RT2.r store the geometric normal (oct-encoded).
//
// Cuticle tilt angle (~3°-5°) determines lobe shifts:
//   R   = -2 * tilt  (primary, toward root) — narrow white highlight
//   TRT = +4 * tilt  (secondary, away from root) — wider colored highlight

static const float HAIR_CUTICLE_TILT = 0.087f; // ~5 degrees in radians (stronger shift for visible angel ring)

// Kajiya-Kay diffuse: soft wrap lighting along strand tangent
half3 HairDiffuse(half3 T, half3 L, half3 albedo)
{
    half TdotL = dot(T, L);
    half sinTL = sqrt(max(0.0h, 1.0h - TdotL * TdotL));
    // Wrap diffuse for softer terminator
    half wrapDiffuse = sinTL * 0.5h + 0.5h;
    return albedo * wrapDiffuse;
}

// Kajiya-Kay specular lobe along strand tangent
// Uses higher exponent range for sharp angel-ring highlights.
half HairSpecularKajiyaKay(half3 T, half3 H, half roughness)
{
    half ToH = dot(T, H);
    half sinTH = sqrt(max(0.0h, 1.0h - ToH * ToH));

    // Directional attenuation: fade out when T and H point same direction
    half dirAtten = smoothstep(-1.0h, 0.0h, ToH);

    // Higher exponent = sharper highlight. Range: [8, 500] depending on roughness.
    // For hair roughness 0.2~0.5, this gives exponent 80~20, producing visible streaks.
    half roughnessClamped = max(roughness, 0.06h);
    half exponent = 2.0h / (roughnessClamped * roughnessClamped);
    exponent = clamp(exponent, 8.0h, 500.0h);

    return dirAtten * pow(sinTH, exponent);
}

// Shift tangent along normal direction for anisotropic highlight offset.
// shiftAmount should be large enough to visually separate the two lobes.
half3 ShiftTangent(half3 T, half3 N, half shiftAmount)
{
    return normalize(T + N * shiftAmount);
}

// Dual-lobe hair specular with per-pixel shift offset.
// T: strand tangent from GBuffer (flowmap-driven)
// N: geometric normal from GBuffer
// shiftOffset: per-pixel shift from tangent-map alpha (0~1, remapped to [-0.5, 0.5])
void HairShadingSeparated(
    half3 T, half3 L, half3 V, half3 N,
    half3 albedo, half3 specularColor, half roughness,
    half shiftOffset, half3 lightColor,
    out half3 outDiffuse, out half3 outSpecular)
{
    half3 H = normalize(L + V);

    // Per-pixel shift offset adds variation (breaks up uniform look)
    half pixelShift = shiftOffset * 0.3h;

    // R lobe: primary specular, shifted toward root, narrow, white-ish
    half primaryShift = -2.0h * HAIR_CUTICLE_TILT + pixelShift;
    half3 T1 = ShiftTangent(T, N, primaryShift);
    half spec1 = HairSpecularKajiyaKay(T1, H, roughness);

    // TRT lobe: secondary specular, shifted away from root, wider, colored by albedo
    half secondaryShift = 4.0h * HAIR_CUTICLE_TILT + pixelShift;
    half3 T2 = ShiftTangent(T, N, secondaryShift);
    half spec2 = HairSpecularKajiyaKay(T2, H, roughness * 1.6h);

    outDiffuse = HairDiffuse(T, L, albedo) * lightColor;

    // Primary lobe: strong white specular; Secondary lobe: softer, colored
    outSpecular = (specularColor * spec1 * 2.0h + albedo * spec2 * 0.8h) * lightColor;
}

// Legacy overload without normal and shift (backward compat)
void HairShadingSeparated(
    half3 T, half3 L, half3 V, half3 albedo,
    half3 specularColor, half roughness,
    half3 lightColor,
    out half3 outDiffuse, out half3 outSpecular)
{
    half3 N = normalize(cross(cross(T, V), T));
    HairShadingSeparated(T, L, V, N, albedo, specularColor, roughness, 0.0h, lightColor,
        outDiffuse, outSpecular);
}

// Combined version
half3 HairShadingCombined(half3 T, half3 L, half3 V, half3 albedo,
    half3 specularColor, half roughness, half3 lightColor)
{
    half3 diffuse, specular;
    HairShadingSeparated(T, L, V, albedo, specularColor, roughness, lightColor, diffuse, specular);
    return diffuse + specular;
}

#endif
