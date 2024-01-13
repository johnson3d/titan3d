#ifndef _DeferredCommon_H_
#define _DeferredCommon_H_
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/SystemEnumDefine.cginc"

#define GBUFFER_NORMAL_VIEWSPACE 1

struct GBufferData
{
	half3	MtlColorRaw;
	half	Metallicity;
	// half	Emissive;
	half	Specular;	
    half	AO;
	half3	WorldNormal;
	half	Roughness;	
	int		ObjectFlags_2Bit;
    int		RenderFlags_10Bit;
	half2	MotionVector;
	
    void SetDisableEnvColor()
    {
        RenderFlags_10Bit |= ERenderFlags_DisableEnvColor;
    }
    bool IsDisableEnvColor()
    {
        return (RenderFlags_10Bit & ERenderFlags_DisableEnvColor) != 0;
    }
		
    float3 GetViewspaceNormal()
    {
        return normalize(mul(float4(WorldNormal.xyz, 0), CameraViewMatrix).xyz);
    }

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
        return (ObjectFlags_2Bit & ObjectFlags_2Bit_Shadow) != 0;
    }

	bool IsUnlit()
	{
		//return GBuffer.ObjectFlags_2Bit == 1;
        return (ObjectFlags_2Bit & ObjectFlags_2Bit_Lighting) != 0;
    }
    void SetUnlit(bool bUnlit)
    {
        if (bUnlit)
        {
            ObjectFlags_2Bit |= ObjectFlags_2Bit_Lighting;
        }
        else
        {
            ObjectFlags_2Bit &= ~ObjectFlags_2Bit_Lighting;
        }
    }

	void EncodeGBuffer(out float4 rt0, out float4 rt1, out float4 rt2, out float4 rt3)
	{
		rt0.rgb = MtlColorRaw.rgb;
		rt0.w = Metallicity;

		#if GBUFFER_NORMAL_VIEWSPACE == 1
        float2 ViewSpaceNorm = GetViewspaceNormal().xy * 0.5f + 0.5f;
        rt1.xy = ViewSpaceNorm;
        rt1.z = ((half) RenderFlags_10Bit) / 1023.0h; //asfloat(RenderFlags_10Bit); //
		#else
		rt1.xyz = EncodeNormalXYZ(WorldNormal.xyz);
		#endif
		
        rt1.w = ((half) ObjectFlags_2Bit) / 3.0h; //asfloat(ObjectFlags_2Bit); //

		rt2.w = Roughness;
		rt2.r = 0;
		// rt2.r = Emissive;
		rt2.g = Specular;
		rt2.b = AO;

		rt3.rg = EncodeMotionVector(MotionVector.xy);
		rt3.zw = 0;
	}

	void DecodeGBuffer(half4 rt0, half4 rt1, half4 rt2, half4 rt3)
	{
		MtlColorRaw.rgb = rt0.rgb;
		Metallicity = rt0.w;

		if (any(rt1.xyz) != 0)
        {
		#if GBUFFER_NORMAL_VIEWSPACE == 1
            float3 vn;
            vn.xy = rt1.xy * 2.0f - 1.0f;
            vn.z = -sqrt(saturate(1.0f - dot(vn.xy, vn.xy)));
            WorldNormal.xyz = mul(float4(vn.xyz, 0), CameraViewInverse).xyz;
            RenderFlags_10Bit = (int) (rt1.z * 1024.0h); //asint(rt1.z); //
		#else
            WorldNormal.xyz = DecodeNormalXYZ(rt1.xyz);
			RenderFlags_10Bit = 0��
		#endif
        }
		Roughness = rt2.w;
        ObjectFlags_2Bit = (int) (rt1.w * 4.0h); //asint(rt1.w); //

		// Emissive = rt2.r;
		Specular = rt2.g;
        AO = rt2.b;

		MotionVector.xy = DecodeMotionVector(rt3.rg);
	}
};

#endif//_MobileBasePassPS_H_