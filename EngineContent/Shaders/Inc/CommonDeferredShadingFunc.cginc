#ifndef	_CommonDeferredShadingFunc_FXH_
#define _CommonDeferredShadingFunc_FXH_

static const half Shininess[16] = { 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 80, 128, 256, 512, 1024, 4096};


float4	UnwrapViewPosition( float4 PosProj, float vDepth )
{
	// Position
	float4 VPos = PosProj;
	VPos.xy /= VPos.ww;
	VPos.z	= vDepth;
	VPos.w	= 1.0f;
	// Inverse Projection Matrix
	VPos		= mul( VPos, GetPrjMtxInverse(true));
	VPos.xyz	/= VPos.www;
	VPos.w		= 1.0f;
	return VPos;
}

float4	UnwrapWorldPosition( float4 PosProj, float vDepth )
{
	// Position
	float4 VPos = PosProj;
	VPos.xy /= VPos.ww;
	VPos.z	= vDepth;
	VPos.w	= 1.0f;
	// Inverse ViewProjection Matrix
	VPos		= mul( VPos, GetViewPrjMtxInverse(true));
	VPos.xyz	/= VPos.www;
	VPos.w		= 1.0f;
	return VPos;
}


half3 EncodeNormalXYZ(half3 n)
{
	return half3(n.xyz * 0.5f + 0.5f);
}

half3 DecodeNormalXYZ(half3 enc)
{
	return enc.xyz * 2 - 1;
}

float4 EncodeNormal(float3 n)
{
	/* method 1
	*/
	return float4(n.xy*0.5+0.5,0,0);

	/* method 3
	float2 enc = (float2(atan2(n.y, n.x) / PI, n.z) + 1.0)*0.5;
	return float4(enc.x, enc.y, 0, 0);
	*/

	/* method 4
	float f = sqrt(8*n.z+8);
    return float4(n.xy / f + 0.5,0,0);
	*/

	/* method 7
	float scale = 1.7777;
	float2 enc = n.xy / (n.z + 1);
	enc /= scale;
	enc = enc*0.5 + 0.5;
	return float4(enc, 0, 0);
	*/
}

float3 DecodeNormal(float2 enc)
{
	/* method 1
	*/
	float3 n;
    n.xy = enc*2-1;
	n.z = sqrt(1 - dot(n.xy, n.xy));
	//n.z = -sqrt(1-dot(n.xy, n.xy));
    return n;

	/* method 3
	float2 ang = enc * 2 - 1;
	float2 scth;
	sincos(ang.x * PI, scth.x, scth.y);
	float2 scphi = float2(sqrt(1.0 - ang.y*ang.y), ang.y);
	float3 n = float3(scth.y*scphi.x, scth.x*scphi.x, scphi.y);
	return n;
	*/

	/* method 4
	float2 fenc = enc*4-2;
    float f = dot(fenc,fenc);
    float g = sqrt(1-f/4);
    float3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
	*/

	/* method 7
	float scale = 1.7777;
	float3 nn =
		float3(enc.x, enc.y,0)*float3(2 * scale, 2 * scale, 0) +
		float3(-scale, -scale, 1);
	float g = 2.0 / dot(nn.xyz, nn.xyz);
	float3 n;
	n.xy = g*nn.xy;
	n.z = g - 1;
	return n;
	*/
}

// @param Scalar clamped in 0..1 range
// @param Mask 0..1
// @return 8bit in range 0..1
half Encode71(half Scalar, uint Mask)
{
	return (half)(
		127.0 / 255.0 * saturate(Scalar) +
		128.0 / 255.0 * Mask);
}

//DataSevenBit is in [0.0f, 1.0f];
//DataOneBit can only be 0.0f or 1.0f;
half Encode71(half DataSevenBit, half DataOneBit)
{
	return (half)(127.0 / 255.0 * saturate(DataSevenBit) +128.0 / 255.0 * DataOneBit);
}


// 8bit reinterpretation as 7bit,1bit
// @param Scalar 0..1
// @param Mask 0..1
// @return 7bit in 0.1
half Decode71(half Scalar, out uint Mask)
{
	Mask = (uint)(Scalar > 0.5);

	return (half)((Scalar - 0.5 * Mask) * 2.0);
}

//there will be data loss,but it's acceptable cause it's relatively small enough;
half Decode71(half DataCoded, out half DataOneBit)
{
	DataOneBit = (half)(DataCoded > 0.5);
	half DataSevenBit = (DataCoded - 0.5 * DataOneBit) * 2.0;
	return DataSevenBit;
}


half Encode44(half Value1, half Value2)
{
	return (half)((16.0 * floor(Value1 * 15.0) + floor(Value2*15.0))) / 255.0;
}

half2 Decode44(half Value)
{
	half2 Result;
	Result.x = (half)floor(Value * 255.0 / 16.0) / 15.0;
	Result.y = (half)fmod(Value * 255.0, 16.0) / 15.0;
	return Result;
}

half EncodeBloomSpecularShininess(half Value1, half Value2)
{
	return (16.0 * Value1 * 15.0 + Value2*15.0) / 255.0;

//	SpecularShininess = SpecularShininess;
//	return (Bloom*15.f*16.f + SpecularShininess *15)/256.f;
}

half trunc_f (float x) 
{ 
	return (half)(x < 0.0 ? (half)(-floor(-x)) : (half)floor(x));
}

half2 DecodeBloomSpecularShininess(half Value)
{
	half2 Result;
	Result.x = (half)floor(Value * 255.0 / 16.0) / 15.0;
	Result.y = (half)fmod(Value * 255.0, 16.0);
	return Result;
	/*
	float t = BloomSpecularShininess * 256.f;
	Result.x = (float)trunc_f(t/16.f);
	Result.y = (t - Result.x*16.f)/15.f;
	Result.x = Result.x/15.f;
	Result.y = Result.y * 128.f;
	*/
}

float EncodeBloomMetallicShininess(float Bloom, float Metallicity, float SpecularShininess)
{
	uint BloomMask = (uint)(Bloom > 0.5f);
	float BloomMetallic = Encode71((half)Metallicity, BloomMask);

	return EncodeBloomSpecularShininess((half)BloomMetallic, (half)SpecularShininess);
}

float3 DecodeBloomMetallicShininess(float BloomSpecularShininess)
{
	float2 Value = DecodeBloomSpecularShininess((half)BloomSpecularShininess);
	float BloomMetallic = Value.x;
	uint BloomMask = 0;
	float Metallicity = Decode71((half)BloomMetallic, BloomMask);
	float Shininess = Value.y;

	return float3((float)BloomMask, Metallicity, Shininess);
}

float DecodeBloom(float4 InGBuffer2)
{
	uint BloomMask = 0;
	Decode71((half)InGBuffer2.a, BloomMask);
	return (float)BloomMask;
}

#define SHADINGMODELID_UNLIT					0
#define SHADINGMODELID_STANDARD			1
#define SHADINGMODELID_EYE						2
#define SHADINGMODELID_CLOTH				3
#define SHADINGMODELID_SUBSURFACE		4
#define SHADINGMODELID_NUM					16
#define SHADINGMODELID_MASK					0xF

float EncodeShadingModelId(float ShadingModelId)
{
	//return (float)ShadingModelId / (float)0xFF;
	return float(ShadingModelId / SHADINGMODELID_MASK);
}

float DecodeShadingModelId(float InPackedChannel)
{
	//return (int)round(InPackedChannel * (float)0xFF);
	return (float)round(InPackedChannel * SHADINGMODELID_MASK);
}

struct Simple_GBufferData
{
	//MRT0
	half3 MtlColorRaw;
	half   Metallicity;
	//half   IsEmissive;
	//half Shininess;
	//MRT1
	half3 WorldNormal;
	half   Roughness;
	//half SpecularBoost;
	
	half3 MtlColorDiffuse;
	half3 MtlColorSpecular;
};

void EncodeGBuffer(
	Simple_GBufferData GBuffer,
	out half4 MRT0,
	out half4 MRT1 )
{
	MRT0.rgb = GBuffer.MtlColorRaw;
	MRT0.a = GBuffer.Metallicity; //Encode71(GBuffer.Metallicity, GBuffer.IsEmissive);
	//MRT0.a = Encode44(clamp(GBuffer.Metallicity, 0, 1), clamp(GBuffer.Shininess, 0, 1));
	
	MRT1.rgb = EncodeNormalXYZ(GBuffer.WorldNormal);
	MRT1.a = GBuffer.Roughness;//Encode71(GBuffer.Roughness, GBuffer.IsEmissive);
	
	//Warning: in simple shading,SpecularBoost is for light specular color calculation;but in PBS,it's for material specular color calculation;
	//MRT1.a = Encode71(GBuffer.SpecularBoost, GBuffer.IsEmissive);
	//	MRT1.a = EncodeBloomMetallicShininess(clamp(GBuffer.Bloom, 0, 1), clamp(GBuffer.Metallicity, 0, 1), clamp(GBuffer.Shininess, 0, 1));
}

Simple_GBufferData DecodeGBuffer(
	in half4 MRT0,
	in half4 MRT1 )
{
	Simple_GBufferData GBufferInst;

//#ifndef D3D9 
	//GBufferInst.MtlColorRaw.rgb = sRGB2Linear(MRT0.rgb);
//#else
	GBufferInst.MtlColorRaw.rgb = MRT0.rgb;
//#endif
	GBufferInst.Metallicity = MRT0.a;
	/*half EmittingFlag = 0.0;
	GBufferInst.Metallicity = Decode71(MRT0.a, EmittingFlag);
	GBufferInst.IsEmissive = EmittingFlag;*/
	/*half2 ms = Decode44(MRT0.a);
	GBufferInst.Metallicity = ms.r;*/
	//int index = floor(ms.g * 15);
	//GBufferInst.Shininess = pow(2,ms.g * 15);// Shininess[clamp(index, 0, 15)];
	//GBufferInst.Shininess = Shininess[clamp(ms.g * 15, 0, 15)];
	
	GBufferInst.WorldNormal = DecodeNormalXYZ(MRT1.rgb);
	GBufferInst.Roughness = MRT1.a;
	//Warning: in simple shading,SpecularBoost is for light specular color calculation;but in PBS,it's for material specular color calculation;
	//GBufferInst.SpecularBoost = Decode71(MRT1.a, EmittingFlag);
	

	//GBufferInst.MtlColorRaw.rgb * (1.0 - GBufferInst.Metallicity);
	GBufferInst.MtlColorDiffuse = GBufferInst.MtlColorRaw.rgb - GBufferInst.MtlColorRaw.rgb * GBufferInst.Metallicity;
	//lerp(0.04, GBufferInst.MtlColorRaw.rgb, GBufferInst.Metallicity);
	half TempSpecBoost = 0;//0.04 - 0.04 * GBufferInst.Metallicity;
	half3 SpecBoostValue = half3(TempSpecBoost, TempSpecBoost, TempSpecBoost);
	GBufferInst.MtlColorSpecular = SpecBoostValue + GBufferInst.MtlColorRaw.rgb * GBufferInst.Metallicity;
	
	return GBufferInst;
}



//here comes pbr;
struct PBS_GBufferData
{
	//MRT0
	float3 MtlColorRaw;
	//MRT1
	float3 WorldNormal;
	float Bloom;
	//MRT2
	float Metallicity;
	float SpecularBoost;
	float Roughness;
	float ShadingModelID;
	//MtlColorDiffuse = func0( MtlColorRaw, Metallicity, SpecularBoost );
	//MtlColorSpecular = func1( MtlColorRaw, SpecularBoost, Metallicity );
	float3 MtlColorDiffuse;
	float3 MtlColorSpecular;
	
};

void EncodeGBuffer(
	PBS_GBufferData GBuffer,
	out float4 MRT0,
	out float4 MRT1,
	out float4 MRT2)
{
#if SHADINGMODELID_UNLIT
	
		MRT0 = 0;
		MRT1 = 0;
		MRT2 = 0;
	
#else
	
		MRT0.rgb = GBuffer.MtlColorRaw;
		MRT0.a = 1.0f;

		MRT1.rgb = EncodeNormalXYZ((half3)GBuffer.WorldNormal);
		//MRT1.a = GBuffer.Bloom;
		MRT1.a = 1.0f;

		MRT2.r = GBuffer.Metallicity;
		MRT2.g = GBuffer.SpecularBoost;
		MRT2.b = GBuffer.Roughness;
		//MRT2.a = EncodeShadingModelId(GBuffer.ShadingModelID);
		MRT2.a = 1.0f;
#endif
}

PBS_GBufferData DecodeGBuffer(
	in float4 MRT0,
	in float4 MRT1,
	in float4 MRT2 )
{
	PBS_GBufferData GBufferInst;
	//Warning: all the lighting algorithm must be in linear color space, else you will do math in a world where 1+1 = 3...
	//MRT0
	GBufferInst.MtlColorRaw.rgb = sRGB2Linear((half3)MRT0.rgb );
	//MRT1
	GBufferInst.WorldNormal = DecodeNormalXYZ((half3)MRT1.rgb );
	GBufferInst.Bloom = MRT1.a;
	//MRT2
	GBufferInst.Metallicity = MRT2.r;
	GBufferInst.SpecularBoost = MRT2.g;
	GBufferInst.Roughness = MRT2.b;
	//GBufferInst.ShadingModelID = DecodeShadingModelId( MRT2.a );
	GBufferInst.ShadingModelID = 1.0f;

	GBufferInst.MtlColorDiffuse = GBufferInst.MtlColorRaw * ( 1.0f - GBufferInst.Metallicity );
	GBufferInst.MtlColorSpecular = lerp( 0.16f * GBufferInst.SpecularBoost.xxx, GBufferInst.MtlColorRaw, GBufferInst.Metallicity );

	return GBufferInst;
}


struct LightColor
{
	float3 mDiffuse;
	float3 mSpecular;
};

#endif