
#if HW_VS_STRUCTUREBUFFER == 1
StructuredBuffer<VSInstantData> VSInstantDataArray DX_AUTOBIND;//: register(t13);
VSInstantData GetInstanceData(VS_MODIFIER input)
{
	VSInstantData result = VSInstantDataArray[input.vInstanceId];
	//result.InstanceId = input.vInstanceId;
	return result;
}
#define Def_GetInstanceData
#endif

//#define VS_NO_WorldTransform
#define VPS_SpecialData_X_HitProxy

float3 InstancingRotateVec(in float3 inPos, in float4 inQuat)
{
	float3 uv = cross(inQuat.xyz, inPos);
	float3 uuv = cross(inQuat.xyz, uv);
	uv = uv * (2.0f * inQuat.w);
	uuv *= 2.0f;
	
	return inPos + uv + uuv;
}

void DoInstancingModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	VSInstantData instData = GetInstanceData(vert);

	float3 Pos = instData.Position.xyz + InstancingRotateVec(vert.vPosition * instData.Scale.xyz, instData.Quat);

	vert.vPosition.xyz = Pos;
	vert.vNormal.xyz = InstancingRotateVec(vert.vNormal.xyz, instData.Quat);

	vsOut.vPosition.xyz = Pos;
	vsOut.vNormal = vert.vNormal;
	vsOut.SpecialData.x = instData.HitProxyId;

	//vsOut.vWorldPos = Pos;
	
	/*vsOut.SpecialData.x = vert.vInstScale.w;

	for (int i = 0; i < vert.vInstScale.w; i++)
	{
		uint lightIndex = vert.vF4_1[i];
		vsOut.PointLightIndices[i] = (uint)lightIndex;
	}*/
//#if HW_VS_STRUCTUREBUFFER
//	VSInstantData InstData = VSInstantDataArray[vert.vInstanceId];
//	vert.vPosition = mul(float4(vert.vPosition.xyz, 1), InstData.WorldMatrix).xyz;
//	vert.vNormal = normalize(mul(float4(vert.vNormal.xyz, 0), InstData.WorldMatrix).xyz);
//	vert.vTangent.xyz = normalize(mul(float4(vert.vTangent.xyz, 0), InstData.WorldMatrix).xyz);
//	vsOut.vWorldPos = vert.vPosition;
//
//	vsOut.SpecialData.x = vert.vInstanceId;
//#else
//	float3 Pos = vert.vInstPos.xyz + InstancingRotateVec(vert.vPosition * vert.vInstScale.xyz, vert.vInstQuat);
//	
//	vert.vPosition.xyz = Pos;
//	vert.vNormal.xyz = InstancingRotateVec(vert.vNormal.xyz, vert.vInstQuat);
//	
//	vsOut.SpecialData.x = vert.vInstScale.w;
//
//	for(int i=0;i<vert.vInstScale.w;i++)
//	{
//		uint lightIndex = vert.vF4_1[i];
//		vsOut.PointLightIndices[i] = (uint)lightIndex;
//	}
//#endif
}
