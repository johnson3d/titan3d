#ifndef _DeferredCommon_H_
#define _DeferredCommon_H_

struct GBufferData
{
	half3	MtlColorRaw;
	half	Metallicity;
	half	Emissive;
	half	Specular;	
	half3	WorldNormal;
	half	Roughness;	
	int		ObjectFlags_2Bit;
};

half3 EncodeNormalXYZ(half3 n)
{
	return half3(n.xyz * 0.5f + 0.5f);
}

half3 DecodeNormalXYZ(half3 enc)
{
	return enc.xyz * 2 - 1;
}

void EncodeGBuffer(GBufferData GBuffer, out float4 rt0, out float4 rt1, out float4 rt2)
{
	rt0.rgb = GBuffer.MtlColorRaw.rgb;
	rt0.w = GBuffer.Metallicity;

	rt1.xyz = EncodeNormalXYZ(GBuffer.WorldNormal.xyz);	
	rt1.w = ((half)GBuffer.ObjectFlags_2Bit) / 4.0h;

	rt2.w = GBuffer.Roughness;
	rt2.r = GBuffer.Emissive;
	rt2.g = GBuffer.Specular;
	rt2.b = 0;
}

GBufferData DecodeGBuffer(half4 rt0, half4 rt1, half4 rt2)
{
	GBufferData output = (GBufferData)0;
	output.MtlColorRaw.rgb = rt0.rgb;
	output.Metallicity = rt0.w;
	
	if (any(rt1.xyz) != 0)
		output.WorldNormal.xyz = DecodeNormalXYZ(rt1.xyz);
	output.Roughness = rt2.w;
	output.ObjectFlags_2Bit = (int)(rt1.w * 4.0h);

	output.Emissive = rt2.r;
	output.Specular = rt2.g;
	return output;
}
#endif//_MobileBasePassPS_H_