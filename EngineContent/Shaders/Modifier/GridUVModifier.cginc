cbuffer cbGridUVMesh DX_AUTOBIND//
{
	float2 UVMin;
	float2 UVMax;
};

void DoGridUVModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	float4 lightmapUV = vert.vLightMap;
	float2 uv = vert.vUV;
	
	float2 u = float2(lightmapUV.x, lightmapUV.y);
	float2 v = float2(lightmapUV.z, lightmapUV.w);
	float2 min_v = float2(UVMin.y, UVMin.y);
	float2 max_v = float2(UVMax.y, UVMax.y);
	float2 min_u = float2(UVMin.x, UVMin.x);
	float2 max_u = float2(UVMax.x, UVMax.x);
	float2 t1 = lerp(min_u, max_u, u);
	float2 t2 = lerp(min_v, max_v, v);

	float2 slt[4] =
	{
		float2(t1.x, t2.x),
		float2(t1.x, t2.y),
		float2(t1.y, t2.y),
		float2(t1.y, t2.x),
	};
	//outUV = slt[(int)uv.x];
	vsOut.vUV = slt[(int)uv.x];
}

//#define DO_VS_MODIFIER DoSkinModifierVS