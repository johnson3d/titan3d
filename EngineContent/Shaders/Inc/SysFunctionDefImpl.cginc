

float3 MTL_OUTPUT::GetWorldNormal(PS_INPUT input)
{
#if MTL_NORMAL_MODE==MTL_NORMALNONE
    return float3(0, 0, 0);
#elif MTL_NORMAL_MODE==MTL_NORMALMAP
	half3 worldNorm = normalize(mNormal);
#if USE_PS_Normal == 1 && USE_PS_Tangent == 1
    NormalMap((float3)mNormal, (half4) input.vTangent, (float3) input.vNormal, worldNorm);
#endif
    return worldNorm;
#elif MTL_NORMAL_MODE==MTL_NORMAL	
	return (float3) normalize(mNormal);
#else
	return (float3) normalize(mNormal);
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
FVSInstanceData GetInstanceData(VS_MODIFIER input)
{
    FVSInstanceData result = (FVSInstanceData) 0;
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