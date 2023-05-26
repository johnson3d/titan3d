
cbuffer cbPerTerrain DX_AUTOBIND
{
	float3 StartPosition;
	float GridSize;
	float HeightStep;
	float UVStep;
};

Texture2D		HeightMapTexture DX_AUTOBIND;
SamplerState	Samp_HeightMapTexture DX_AUTOBIND;

Texture2D		NormalMapTexture DX_AUTOBIND;
SamplerState	Samp_NormalMapTexture DX_AUTOBIND;

half3 GetPosition(half x, half z, out half2 uv, out half3 nor)
{
	uv = half2(((half)x) * UVStep, ((half)z) * UVStep);
	half4 altInfo = (half4)HeightMapTexture.SampleLevel(Samp_HeightMapTexture, uv.xy, 0);
	half2 du = half2(UVStep, 0);
	half2 uv_du = uv + du;
	half4 v_du = (half4)HeightMapTexture.SampleLevel(Samp_HeightMapTexture, uv_du.xy, 0);
	half2 dv = half2(0, UVStep);
	half2 uv_dv = uv + dv;
	half4 v_dv = (half4)HeightMapTexture.SampleLevel(Samp_HeightMapTexture, uv_dv.xy, 0);
	float3 A = float3(GridSize, (v_du.r - altInfo.r) * HeightStep, 0);
	float3 B = float3(0, (v_dv.r - altInfo.r) * HeightStep, -GridSize);

	nor = cross(A, B);
	nor = normalize(nor);

	half3 pos;
	//pos.y = (altInfo.r * 256 * 256 + altInfo.g * 256) * HeightStep;
	pos.y = altInfo.r * HeightStep;;
	pos.x = x * GridSize;
	pos.z = z * GridSize;

	//nor = NormalMapTexture.SampleLevel(Samp_NormalMapTexture, uv.xy, 0).xyz;

	pos += StartPosition;
	return pos;
}

void DoTerrainModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	uint x = vert.vVertexID & 0x0000FFFF;
	uint z = (vert.vVertexID >> 16);

	half2 uv;
	half3 nor;
	half3 pos = GetPosition(x, z, uv, nor);

	vsOut.vPosition.xyz = pos;
	vsOut.vNormal = nor;
	//vsOut.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);

	vsOut.vUV = uv;

	vert.vPosition = vsOut.vPosition;
	vert.vNormal = vsOut.vPosition;
	vert.vUV = vsOut.vUV;
}

void MdfQueueDoModifiers(inout PS_INPUT output, VS_MODIFIER input)
{
	DoTerrainModifierVS(output, input);
}

#define MDFQUEUE_FUNCTION

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
//	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));
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