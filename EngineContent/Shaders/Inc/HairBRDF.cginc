#ifndef _HAIR_BRDF_H_
#define _HAIR_BRDF_H_

// Simplified Kajiya-Kay Hair BRDF for deferred lighting.
// Reference: UE5 HairBsdf.ush (Marschner model)
//
// GBuffer convention for Hair:
//   WorldNormal stores the hair strand tangent direction T (pointing toward root).
//   No per-pixel custom data needed — all parameters are physical constants.
//
// Cuticle tilt angle (~2°) determines all three lobe shifts:
//   R   = -2 * tilt  (primary, toward root)
//   TT  = +1 * tilt  (transmission)
//   TRT = +4 * tilt  (secondary, away from root)

static const float HAIR_CUTICLE_TILT = 0.035f; // ~2 degrees in radians

// Kajiya-Kay diffuse: soft wrap lighting along strand tangent
half3 HairDiffuse(half3 T, half3 L, half3 albedo)
{
    half sinTL = sqrt(max(0.0h, 1.0h - Pow2(dot(T, L))));
    return albedo * sinTL;
}

// Kajiya-Kay specular lobe along strand tangent
half HairSpecularKajiyaKay(half3 T, half3 H, half roughness)
{
    half ToH = dot(T, H);
    half sinTH = sqrt(max(0.0h, 1.0h - ToH * ToH));
    half dirAtten = smoothstep(-1.0h, 0.0h, ToH);
    half exponent = max(2.0h / max(roughness * roughness, 0.002h), 1.0h);
    return dirAtten * pow(sinTH, exponent);
}

// Shift tangent along normal-like direction for anisotropic highlight offset
half3 ShiftTangent(half3 T, half3 shiftDir, half shiftAmount)
{
    return normalize(T + shiftDir * shiftAmount);
}

// Dual-lobe hair specular with physically-derived shifts from cuticle tilt.
// T: strand tangent, B: bitangent (cross(T, V) approximate for shift direction)
void HairShadingSeparated(
    half3 T, half3 L, half3 V, half3 albedo,
    half3 specularColor, half roughness,
    half3 lightColor,
    out half3 outDiffuse, out half3 outSpecular)
{
    half3 H = normalize(L + V);

    // Approximate bitangent for shifting tangent (perpendicular to T in the T-V plane)
    half3 B = normalize(cross(T, V));

    // R lobe: primary specular, shifted toward root (-2 * tilt), narrow
    half3 T1 = ShiftTangent(T, B, -2.0h * HAIR_CUTICLE_TILT);
    half spec1 = HairSpecularKajiyaKay(T1, H, roughness);

    // TRT lobe: secondary specular, shifted away (+4 * tilt), wider, colored
    half3 T2 = ShiftTangent(T, B, 4.0h * HAIR_CUTICLE_TILT);
    half spec2 = HairSpecularKajiyaKay(T2, H, roughness * 2.0h);

    outDiffuse = HairDiffuse(T, L, albedo) * lightColor;
    outSpecular = (specularColor * spec1 + albedo * spec2 * 0.5h) * lightColor;
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
