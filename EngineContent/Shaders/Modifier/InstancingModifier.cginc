
#if HW_VS_STRUCTUREBUFFER == 1
StructuredBuffer<VSInstanceData> VSInstanceDataArray DX_AUTOBIND;//: register(t13);
VSInstanceData GetInstanceData(VS_MODIFIER input)
{
	VSInstanceData result = VSInstanceDataArray[input.vInstanceId];
	//result.InstanceId = input.vInstanceId;
	return result;
}
#define Def_GetInstanceData
#endif

//#define VS_NO_WorldTransform
#define VPS_SpecialData_X_HitProxy

void DoInstancingModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	VSInstanceData instData = GetInstanceData(vert);

    float3 inst_pos = instData.Position.xyz;
    float4 inst_quat = instData.Quat;
    float3 inst_scale = instData.Scale.xyz;
	
	#if defined(INSTANCE_NONE_POS)
	inst_pos = float3(0,0,0);
	#endif
	
	#if defined(INSTANCE_NONE_QUAT)
	inst_quat = float4(0,0,0,1);
	#endif
	
	#if defined(INSTANCE_NONE_SCALE)
	inst_pos = float3(1,1,1);
	#endif
	
    float3 Pos = instData.Position.xyz + QuatRotateVector(vert.vPosition * instData.Scale.xyz, instData.Quat);

	vert.vPosition.xyz = Pos;
    vert.vNormal.xyz = QuatRotateVector(vert.vNormal.xyz, instData.Quat);

	vsOut.vPosition.xyz = Pos;
    vsOut.SetNormal(vert.vNormal);
    vsOut.SetSpecialDataX(instData.HitProxyId);

	if(instData.UserData.x>0)
    {
#if USE_PS_Color == 1
        float grayScale = (float) instData.UserData.x / 255.0f;
        grayScale = pow(grayScale, 3);
        vsOut.vColor.xyz = float3(grayScale, grayScale, grayScale);
#endif
    }
	//vsOut.vWorldPos = Pos;
	
//#if HW_VS_STRUCTUREBUFFER
//	VSInstanceData InstData = VSInstanceDataArray[vert.vInstanceId];
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
