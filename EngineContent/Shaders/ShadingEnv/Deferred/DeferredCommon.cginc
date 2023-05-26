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
	half2	MotionVector;

	static half3 EncodeNormalXYZ(half3 n)
	{
		return half3(n.xyz * 0.5f + 0.5f);
	}

	static half3 DecodeNormalXYZ(half3 enc)
	{
		return enc.xyz * 2 - 1;
	}
	bool IsAcceptShadow()
	{
		return (ObjectFlags_2Bit & 1) != 0;
	}

	bool IsUnlit()
	{
		//return GBuffer.ObjectFlags_2Bit == 1;
		return (ObjectFlags_2Bit & (2)) != 0;
	}

	void EncodeGBuffer(out float4 rt0, out float4 rt1, out float4 rt2, out float4 rt3)
	{
		rt0.rgb = MtlColorRaw.rgb;
		rt0.w = Metallicity;

		rt1.xyz = EncodeNormalXYZ(WorldNormal.xyz);
		rt1.w = ((half)ObjectFlags_2Bit) / 3.0h;

		rt2.w = Roughness;
		rt2.r = Emissive;
		rt2.g = Specular;
		rt2.b = 0;

		rt3.rg = EncodeMotionVector(MotionVector.xy);
		rt3.zw = 0;
	}

	void DecodeGBuffer(half4 rt0, half4 rt1, half4 rt2, half4 rt3)
	{
		MtlColorRaw.rgb = rt0.rgb;
		Metallicity = rt0.w;

		if (any(rt1.xyz) != 0)
			WorldNormal.xyz = DecodeNormalXYZ(rt1.xyz);
		Roughness = rt2.w;
		ObjectFlags_2Bit = (int)(rt1.w * 4.0h);

		Emissive = rt2.r;
		Specular = rt2.g;

		MotionVector.xy = DecodeMotionVector(rt3.rg);
	}
};

#endif//_MobileBasePassPS_H_