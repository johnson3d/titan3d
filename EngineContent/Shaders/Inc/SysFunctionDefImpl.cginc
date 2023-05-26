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