#ifndef _DeferredCommon_H_
#define _DeferredCommon_H_
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/SystemEnumDefine.cginc"

struct FGBufferData : FGBufferDataBase
{
    bool HasCustomData_10Bit()
    {
        return USE_OCTAHEDRON_NORMAL != 0;
    }
    // --- Subsurface Profile accessors ---
    // For Subsurface pixels, CustomData stores profile index (0~255 normalized to 0~1)
    int GetSubsurfaceProfileIndex()
    {
        return (int)(CustomData * 255.0h + 0.5h);
    }

    void SetDisableEnvColor()
    {
        RenderFlags_10Bit |= ERenderFlags_DisableEnvColor;
    }
    bool IsDisableEnvColor()
    {
        return (RenderFlags_10Bit & ERenderFlags_DisableEnvColor) != 0;
    }

    void SetShadingMode(int mode)
    {
        RenderFlags_10Bit = (RenderFlags_10Bit & ~SHADINGMODE_BIT_MASK) | ((mode << SHADINGMODE_BIT_OFFSET) & SHADINGMODE_BIT_MASK);
    }
    int GetShadingMode()
    {
        return (RenderFlags_10Bit & SHADINGMODE_BIT_MASK) >> SHADINGMODE_BIT_OFFSET;
    }
    bool IsPBR()
    {
        return GetShadingMode() == EShadingMode_PBR;
    }
    bool IsSubsurface()
    {
        return GetShadingMode() == EShadingMode_Subsurface;
    }
		
    float3 GetViewspaceNormal()
    {
        return normalize(mul(float4(WorldNormal.xyz, 0), CameraViewMatrix).xyz);
    }
    
	bool IsAcceptShadow()
	{
        return (RenderFlags_10Bit & ERenderFlags_AcceptShadow) != 0;
    }

	bool IsUnlit()
	{
        return (RenderFlags_10Bit & ERenderFlags_UnLight) != 0;
    }
    void SetUnlit(bool bUnlit)
    {
        if (bUnlit)
        {
            RenderFlags_10Bit |= ERenderFlags_UnLight;
        }
        else
        {
            RenderFlags_10Bit &= ~ERenderFlags_UnLight;
        }
    }

	void EncodeGBuffer(out float4 rt0, out float4 rt1, out float4 rt2, out float4 rt3)
	{
		rt0.rgb = MtlColorRaw.rgb;
		rt0.a = CustomData;

		#if USE_OCTAHEDRON_NORMAL == 0
            rt1.rgb = (half3)EncodeNormalXYZ(WorldNormal.xyz);
            rt3.b = ((half) RenderFlags_10Bit) / 1023.0h;
		#else
		    rt1.rg = (half2)OctEncode(WorldNormal.xyz);
            rt1.b = ((half) RenderFlags_10Bit) / 1023.0h;
            rt3.b = ((half) CustomData_10Bit) / 1023.0h;
		#endif
		rt1.w = Mask;

		rt2.r = Metallicity;
		rt2.g = Specular;
		rt2.b = Roughness;
    	rt2.a = AO;

		rt3.rg = EncodeMotionVector(MotionVector.xy);
        rt3.a = saturate(Opacity);
    }

	void DecodeGBuffer(half4 rt0, half4 rt1, half4 rt2, half4 rt3)
	{
		MtlColorRaw.rgb = rt0.rgb;
        
		#if USE_OCTAHEDRON_NORMAL == 0
            WorldNormal.xyz = (half3) DecodeNormalXYZ(rt1.rgb).xyz;
            RenderFlags_10Bit = (int) (rt3.b * 1023.0h + 0.5h);
            CustomData_10Bit = 0;
		#else
            WorldNormal.xyz = OctDecode(rt1.rg);
            RenderFlags_10Bit = (int) (rt1.b * 1023.0h + 0.5h);
            CustomData_10Bit = (int) (rt3.b * 1023.0h + 0.5h);
		#endif
        Mask = rt1.a;
        
		Metallicity = rt2.r;
		Specular = rt2.g;
		Roughness = rt2.b;
        AO = rt2.a;

        MotionVector.xy = (half2)DecodeMotionVector(rt3.rg);
        CustomData = rt0.a;
        Opacity = rt3.a;
    }
};

#endif//_MobileBasePassPS_H_