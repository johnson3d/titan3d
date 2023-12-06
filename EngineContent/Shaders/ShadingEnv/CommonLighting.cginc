#ifndef __COMMON_LIGHTING_H__
#define __COMMON_LIGHTING_H__

void GetDirLightingColor(out half3 OutDirLightDiffuseShading, out half3 OutDirLightSpecShading,
	PS_INPUT input, MTL_OUTPUT mtl, float ShadowValue)
{
    half3 WorldPos = input.vWorldPos;
	half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);
    half3 N = normalize((half3)mtl.GetWorldNormal(input));
	half Metallic = (half)mtl.mMetallic;
	half Smoothness = (half)mtl.mRough;
	half Roughness = 1.0h - Smoothness;
	half AbsSpecular = (half)mtl.mAbsSpecular;
	half3 Emissive = (half3)mtl.mEmissive;
	half Transmit = (half)mtl.mTransmit;
	half3 SubAlbedo = sRGB2Linear((half3)mtl.mSubAlbedo);
	half AOs = (half)mtl.mAO;
	half Mask = (half)mtl.mMask;

	half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

	half3 L = -(half3)normalize(gDirLightDirection_Leak.xyz);
	half3 V = (half3)normalize(CameraPosition - WorldPos);
	half3 Cdir = (half3)gDirLightColor_Intensity.rgb;
	half  Idir = (half)gDirLightColor_Intensity.w;
	half Ienv_light = Idir * 0.2h;
	half3 Csky = (half3)mSkyLightColor;
	half3 Cground = (half3)mGroundLightColor;
	half DirLightLeak = (half)gDirLightDirection_Leak.w;

	half Sdiff = 1.0h - Metallic;
	half3 OptDiffShading = Sdiff * Albedo;

	AbsSpecular = 0.08h * AbsSpecular;
	half3 OptSpecShading = AbsSpecular - AbsSpecular * Metallic + Metallic * Albedo;

	half3 H = normalize(L + V);
	half NoLsigned = dot(N, L);
	half NoL = max(NoLsigned, 0.0h);
	half NoH = max(dot(N, H), 0.0h);
	half LoH = max(dot(L, H), 0.0h);
	half NoV = max(dot(N, V), 0.0h);

	//half3 DirLightDiffuseShading = NoL * Idir * Cdir * OptDiffShading * ECCd;
	OutDirLightDiffuseShading = RetroDiffuseMobile(NoL, NoV, LoH, Roughness) * Idir * Cdir * OptDiffShading;

	OutDirLightSpecShading = BRDFMobile(Roughness, N, H, NoH, LoH, NoV, NoL, OptSpecShading) * sqrt(NoL) * Idir * Cdir;
}

half3 GetSkyColor(half3 Albedo, MTL_OUTPUT mtl, float ShadowValue)
{
	half Metallic = (half)mtl.mMetallic;
	half Sdiff = 1.0h - Metallic;
	half3 OptDiffShading = Sdiff * Albedo;

	half3 N = normalize((half3)mtl.mNormal);
	half3 L = -(half3)normalize(gDirLightDirection_Leak.xyz);
	half NoLsigned = dot(N, L);
	half NoL = max(NoLsigned, 0.0h);
	half3 Csky = (half3)mSkyLightColor;
	half3 Cground = (half3)mGroundLightColor;
	half  Idir = (half)gDirLightColor_Intensity.w;
	half Ienv_light = Idir * 0.2h;

	half SkyAtten = min(1.0h, 2.0h - NoL - ShadowValue);
	half3 SkyShading = lerp(Cground, Csky, 0.5h * N.y + 0.5h) * SkyAtten * SkyAtten * OptDiffShading * Ienv_light;
	return SkyShading;
}

#endif//__COMMON_LIGHTING_H__
