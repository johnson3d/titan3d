
VS_MODIFIER VS_INPUT_TO_VS_MODIFIER(VS_INPUT input)
{
	VS_MODIFIER result = (VS_MODIFIER)0;
#if USE_VS_Position == 1
	result.vPosition = input.vPosition;
#endif
#if USE_VS_Normal == 1
	result.vNormal = input.vNormal;
#endif
#if USE_VS_Tangent == 1
	result.vTangent = input.vTangent;
#endif
#if USE_VS_Color == 1
#if MTL_ID_64BITVCOLORALPHA == 1
	result.vColor.rgb = input.vColor.rgb;
	result.vColor.a = input.vColor.a * 255 / 63;
#else
	result.vColor = input.vColor;
#endif
#endif
#if USE_VS_UV == 1
	result.vUV = input.vUV;
#endif
#if USE_VS_LightMap == 1
	result.vLightMap = input.vLightMap;
#endif
#if USE_VS_SkinIndex == 1
	result.vSkinIndex = input.vSkinIndex;
#endif
#if USE_VS_SkinWeight == 1
	result.vSkinWeight = input.vSkinWeight;
#endif
#if USE_VS_TerrainIndex == 1
	result.vTerrainIndex = input.vTerrainIndex;
#endif
#if USE_VS_TerrainGradient == 1
	result.vTerrainGradient = input.vTerrainGradient;
#endif
#if USE_VS_InstPos == 1
	result.vInstPos = input.vInstPos;
#endif
#if USE_VS_InstQuat == 1
	result.vInstQuat = input.vInstQuat;
#endif
#if USE_VS_InstScale == 1
	result.vInstScale = input.vInstScale;
#endif
#if USE_VS_F4_1 == 1
	result.vF4_1 = input.vF4_1;
#endif
#if USE_VS_F4_2 == 1
	result.vF4_2 = input.vF4_2;
#endif
#if USE_VS_F4_3 == 1
	result.vF4_3 = input.vF4_3;
#endif
	result.vVertexID = input.vVertexID;
	result.vInstanceId = input.vInstanceId;
#if RHI_TYPE == RHI_VK
	result.vMultiDrawId = input.vMultiDrawId;
#elif RHI_TYPE == RHI_DX12
	result.vMultiDrawId = MultiDrawId;
#endif

	return result;
}

half3 MTL_OUTPUT::GetFinalNormal(PS_INPUT input)
{
#if MTL_NORMAL_MODE==MTL_NORMALNONE
    return half3(0, 0, 0);
#elif MTL_NORMAL_MODE==MTL_NORMALMAP
	half3 worldNorm = normalize(mNormal);
#if USE_PS_Normal == 1 && USE_PS_Tangent == 1
    NormalMap((half3)mNormal, (half4) input.vTangent, (half3) input.vNormal, worldNorm);
#endif
    return worldNorm;
#elif MTL_NORMAL_MODE==MTL_NORMAL	
	return (half3) normalize(mNormal);
#else
	return (half3) normalize(mNormal);
#endif
}

#if !defined(Def_GetTerrainDiffuse)
float3 GetTerrainDiffuse(float2 uv, PS_INPUT input)
{
	return float3(1,1,1);
}
#endif

#if !defined(Def_GetTerrainNormal)
float3 GetTerrainNormal(float2 uv, PS_INPUT input)
{
	return float3(1, 1, 1);
}
#endif

#if !defined(Def_GetInstanceData)
VSInstantData GetInstanceData(VS_MODIFIER input)
{
	VSInstantData result = (VSInstantData)0;
#if USE_VS_InstPos == 1
	result.Position = input.vInstPos;
#endif
#if USE_VS_InstQuat == 1
	result.Quat = input.vInstQuat;
#endif
#if USE_VS_InstScale == 1
	result.Scale = input.vInstScale.xyz;
	result.HitProxyId = (uint)input.vInstScale.w;
#endif
	//result.InstanceId = input.vInstanceId;
	return result;
}
#endif