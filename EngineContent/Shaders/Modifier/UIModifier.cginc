
cbuffer cbUIMesh DX_AUTOBIND//
{
	float4x4 AbsTransform[256];
};

float3 TransVec(in float3 inPos, in float4x4 mat)
{
	float w = 1 / ((inPos.x * mat._41) + (inPos.y * mat._42) + (inPos.z * mat._43) + mat._44);
	float3 retVal;
	retVal.x = ((inPos.x * mat._11) + (inPos.y * mat._12) + (inPos.z * mat._13) + mat._14) * w;
	retVal.y = ((inPos.x * mat._21) + (inPos.y * mat._22) + (inPos.z * mat._23) + mat._24) * w;
	retVal.z = ((inPos.x * mat._31) + (inPos.y * mat._32) + (inPos.z * mat._33) + mat._34) * w;
	return retVal;
}

uint GetTransformIndex(uint4 index)
{
	return (index.r << 8) + index.a;
}

void DoUIModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	float3 tempPos = TransVec(vert.vPosition, AbsTransform[GetTransformIndex(vert.vSkinIndex)]);
	vsOut.vPosition.xyz = tempPos;// mul(float4(tempPos.x, tempPos.y, tempPos.z, 1.0), ViewPrjMtx).xyz;

#if USE_PS_Normal == 1
	//vsOut.vNormal.xyz = transform_quat((half3)vert.vNormal.xyz, (half4)AbsControlQuat[vert.vSkinIndex[0]]);
#endif
}

//#define DO_VS_MODIFIER DoUIModifierVS