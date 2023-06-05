
cbuffer cbSkinMesh DX_AUTOBIND//
{
	float4 AbsBonePos[360];
	float4 AbsBoneQuat[360];
};

half3 RotateVec(in half3 inPos, in half4 inQuat)
{
	half3 uv = (half3)cross(inQuat.xyz, inPos);
	half3 uuv = (half3)cross(inQuat.xyz, uv);
	uv = uv * (2.0h * inQuat.w);
	uuv *= 2.0h;
	
	return inPos + uv + uuv;
}

half3 transform_quat(half3 v, half4 quat)
{
	return v + (half3)cross(quat.xyz, cross(quat.xyz, v) + quat.w * v) * 2;
}

void DoSkinModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	half3      Pos = 0.0f;
	half3      Normal = 0.0f;    
	float weight = vert.vSkinWeight[0] + vert.vSkinWeight[1] + vert.vSkinWeight[2] + vert.vSkinWeight[3];
	if(weight  == 0.0f)
	{
		vert.vPosition.xyz = (float3)AbsBonePos[vert.vSkinIndex[0]].xyz;
#if USE_PS_Normal == 1
		vert.vNormal.xyz = (float3)vert.vNormal.xyz;	
#endif
		return;
	}
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[0]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[0]])) * (half)vert.vSkinWeight[0];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[1]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[1]])) * (half)vert.vSkinWeight[1];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[2]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[2]])) * (half)vert.vSkinWeight[2];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[3]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[3]])) * (half)vert.vSkinWeight[3];
	vsOut.vPosition.xyz = Pos.xyz;

#if USE_PS_Normal == 1
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[0]]) * (half)vert.vSkinWeight[0];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[1]]) * (half)vert.vSkinWeight[1];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[2]]) * (half)vert.vSkinWeight[2];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[3]]) * (half)vert.vSkinWeight[3];
	vsOut.vNormal.xyz = (float3)Normal;	
#endif
}

//#define DO_VS_MODIFIER DoSkinModifierVS