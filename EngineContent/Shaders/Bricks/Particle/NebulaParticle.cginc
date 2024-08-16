#include "ParticleCommon.cginc"

StructuredBuffer<FParticle>			sbParticleInstance DX_AUTOBIND;
ByteAddressBuffer					sbAlives DX_AUTOBIND;
//StructuredBuffer<float>				sbAlives DX_AUTOBIND;

void DoNebulaModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	//float3 Pos = vert.vInstPos.xyz + QuatRotatePosition(vert.vPosition * vert.vInstScale.xyz, vert.vInstQuat);
	
	uint idx = sbAlives.Load(vert.vInstanceId * 4 + 4);
	//uint idx = sbAlives[vert.vInstanceId * 4 + 16];
#if RHI_TYPE != RHI_DX11
	//idx += vert.vMultiDrawId;
#endif
	FParticle inst = sbParticleInstance[idx];
    float4 quat = float4(0, 0, 0, 1);
    if (EmitterData.Flags & EParticleEmitterStyles_YawFaceToCameral)
    {
        quat = QuatFromAxisAngle(float3(0, 1, 0), EmitterData.CameralEuler.y);
    }
    else if (EmitterData.Flags & EParticleEmitterStyles_FreeFaceToCameral)
    {
        half3 angles = EmitterData.CameralEuler;
        angles.z = 0;
        quat = QuatFromEuler(angles.xyz);
    }
    else
    {
        half4 angles = ToColor4f(inst.Rotator) * (PI * 2);
        quat = QuatFromEuler(angles.xyz); //QuatFromAxisAngle(float3(0, 1, 0), angles.y);
    }
    float3 Pos = inst.Location + QuatRotateVector(vert.vPosition.xyz * inst.Scale, quat); //QuateFromAxisAngle
    vsOut.vPosition.xyz = Pos;
    vert.vNormal.xyz = QuatRotateVector(vert.vNormal.xyz, quat);
    vsOut.Set_vNormal(vert.vNormal);
    vsOut.vColor = (float4) ToColor4f(inst.Color);

}

//#define MDFQUEUE_FUNCTION_PS

//PS_INPUT VS_Main( uint verteID : SV_VertexID )
//{
//	PS_INPUT output = (PS_INPUT)0;
//	VS_MODIFIER input = (VS_MODIFIER)0;
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