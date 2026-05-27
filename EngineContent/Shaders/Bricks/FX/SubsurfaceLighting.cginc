#ifndef _SUBSURFACE_LIGHTING_H_
#define _SUBSURFACE_LIGHTING_H_

// Subsurface scattering lighting utilities.
// Extracted from DeferredDirLighting.cginc for reuse across deferred / forward pipelines.
//
// Requirements:
//   - FSubsurfaceProfile struct and SubsurfaceProfiles StructuredBuffer must be declared
//     by the including shader before #include of this file.
//   - Standard lighting vectors (N, V, L, etc.) are passed as function parameters.

struct FSubsurfaceLightingInput
{
    half3 Normal;
    half3 ViewDir;
    half3 LightDir;
    half  NoLsigned;       // dot(N, L) unclamped
    half  NoL;             // saturate(dot(N, L))
    half3 Albedo;          // linear-space albedo
    half3 OptDiffShading;  // (1 - Metallic) * Albedo
    half3 LightColor;      // directional light color
    half  LightIntensity;  // directional light intensity
};

struct FSubsurfaceLightingResult
{
    half3 Diffuse;         // SSS diffuse (wrap + back-scatter), before shadow/AO
    half3 BackScatter;     // back-scatter term alone (for debug)
};

// Compute subsurface wrap diffuse + back-scatter for a single directional light.
// sssProfile: the profile fetched from SubsurfaceProfiles[index]
FSubsurfaceLightingResult ComputeSubsurfaceDirLight(
    in FSubsurfaceLightingInput input,
    in FSubsurfaceProfile sssProfile)
{
    FSubsurfaceLightingResult result = (FSubsurfaceLightingResult)0;

    half sssOpacity = (half)sssProfile.SubsurfaceOpacity;

    // Wrap diffuse: opacity controls how far the wrap extends beyond the terminator
    half wrapWidth = lerp(0.1h, 0.7h, sssOpacity);
    half wrapNoL = saturate((input.NoLsigned + wrapWidth) / (1.0h + wrapWidth));

    // Back-scatter: lower power = broader scatter lobe when opacity is high
    half scatterPower = lerp(16.0h, 4.0h, sssOpacity);
    half3 scatterDir = input.LightDir + input.Normal * lerp(0.2h, 0.8h, sssOpacity);
    half VdotScatter = saturate(dot(input.ViewDir, -scatterDir));
    half3 backScatter = pow(VdotScatter, scatterPower) * sssOpacity
                      * (half3)sssProfile.ScatterColor * input.Albedo
                      * input.LightIntensity * input.LightColor;

    // Diffuse with wrap lighting (will be blurred by separable SSS blur pass)
    half3 sssDiffuse = wrapNoL * input.LightIntensity * input.LightColor * input.OptDiffShading;

    result.Diffuse = sssDiffuse + backScatter;
    result.BackScatter = backScatter;
    return result;
}

// Compose final SSS base shading from directional light result + sky + specular.
// Returns: BaseShading (diffuse, to be written to separated diffuse RT)
//          outSpecular (specular, to be written to separated specular RT)
void ComposeSubsurfaceShading(
    in half3 sssDirDiffuse,          // from ComputeSubsurfaceDirLight().Diffuse
    in half3 skyShading,
    in half  finalShadowValue,       // min(1, shadow + leak)
    in half  shadowValue,            // raw shadow
    in half3 dirLightSpecShading,
    in half3 envSpec,
    in half  ambientOcclusion,
    out half3 outBaseShading,
    out half3 outSpecular)
{
    outBaseShading = (sssDirDiffuse * finalShadowValue + skyShading) * ambientOcclusion;
    outSpecular = (dirLightSpecShading * shadowValue + envSpec) * ambientOcclusion;
}

#endif // _SUBSURFACE_LIGHTING_H_
