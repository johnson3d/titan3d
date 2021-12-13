#include "ParticleCommon.cginc"

StructuredBuffer<FParticle>			sbParticleInstance DX_NOBIND;
ByteAddressBuffer					sbAlives DX_NOBIND;

void DoNebulaModifierVS(inout PS_INPUT vsOut, inout VS_INPUT vert)
{
	//float3 Pos = vert.vInstPos.xyz + QuatRotatePosition(vert.vPosition * vert.vInstScale.xyz, vert.vInstQuat);
	
	uint idx = sbAlives.Load(vert.vInstanceId * 4 + 16);
	FParticle inst = sbParticleInstance[idx];
	float3 Pos = inst.Location + vert.vPosition.xyz * inst.Scale;
	vsOut.vPosition.xyz = Pos;
}

void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)
{
	DoNebulaModifierVS(output, input);
}

void MdfQueueDoModifiersPS(inout MTL_OUTPUT output, PS_INPUT input)
{
	
}

#define MDFQUEUE_FUNCTION

//#define MDFQUEUE_FUNCTION_PS

//PS_INPUT VS_Main( uint verteID : SV_VertexID )
//{
//	PS_INPUT output = (PS_INPUT)0;
//	VS_INPUT input = (VS_INPUT)0;
//
//	uint x = verteID & 0x0000FFFF;
//	uint z = (verteID >> 16);
//	
//	half2 uv;
//	half3 nor;
//	half3 pos = GetPosition(x, z, uv, nor);
//
//	input.vPosition = pos;
//	input.vNormal = nor;
//	//input.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);
//
//	input.vUV = uv;
//
//	Default_VSInput2PSInput(output, input);
//	
//	output.vWorldPos = output.vPosition.xyz;
//	output.vPosition = mul(float4(output.vWorldPos, 1), ViewPrjMtx);
//	
//	output.psCustomUV0.w = output.vPosition.w;
//
//	return output;
//}
//
//PS_OUTPUT PS_Main(PS_INPUT input)
//{
//	/*PS_OUTPUT output = (PS_OUTPUT)0;
//	MTL_OUTPUT mtl = Default_PSInput2Material(input);
//
//#ifndef DO_PS_MATERIAL
//#define DO_PS_MATERIAL DoDefaultPSMaterial
//#endif
//	DO_PS_MATERIAL(input, mtl);
//
//	output.RT0 = half4(mtl.mAlbedo.xyz, mtl.mAlpha);
//	return output;*/
//	return PS_MobileBasePass(input);
//}