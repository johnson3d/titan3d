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
    bool IsHair()
    {
        return GetShadingMode() == EShadingMode_Hair;
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

    // All MRT packing logic is centralized here.
    // Semantic fields (SubsurfaceProfileIndex, SpecOcclusion, etc.) are written
    // directly by the base pass; EncodeGBuffer decides how to compress them
    // into 4 render targets based on ShadingMode.
	void EncodeGBuffer(out float4 rt0, out float4 rt1, out float4 rt2, out float4 rt3)
	{
		rt0.rgb = MtlColorRaw.rgb;

		// RenderFlags → rt3.b (R10G10B10A2_UNORM, 10-bit integer)
		rt3.b = ((half) RenderFlags_10Bit) / 1023.0h;

		if (IsHair())
		{
			// Hair: rt1 stores tangent, normal oct-encoded into rt0.a + rt2.r
			half2 normalOct = (half2)OctEncode(WorldNormal.xyz);
			rt0.a = normalOct.x;
			rt2.r = normalOct.y;

			#if USE_OCTAHEDRON_NORMAL == 0
				rt1.rgb = (half3)EncodeNormalXYZ(WorldTangent.xyz);
			#else
				rt1.rg = (half2)OctEncode(WorldTangent.xyz);
				rt1.b = 0;
			#endif
		}
		else
		{
			// rt0.a packs a per-ShadingMode scalar:
			//   Subsurface → SubsurfaceProfileIndex (normalized to 0~1)
			//   PBR/other  → SpecOcclusion
			if (IsSubsurface())
			{
				rt0.a = saturate((half)SubsurfaceProfileIndex / 255.0h);
			}
			else
			{
				rt0.a = (half)SpecOcclusion;
			}

			#if USE_OCTAHEDRON_NORMAL == 0
				rt1.rgb = (half3)EncodeNormalXYZ(WorldNormal.xyz);
			#else
				rt1.rg = (half2)OctEncode(WorldNormal.xyz);
				rt1.b = 0;
			#endif

			rt2.r = Metallicity;
		}

		rt1.w = Mask;
		rt2.g = Specular;
		rt2.b = Roughness;
    	rt2.a = AO;

		rt3.rg = EncodeMotionVector(MotionVector.xy);
        rt3.a = saturate(Opacity);
    }

    // Decode MRT back into semantic GBuffer fields.
	void DecodeGBuffer(half4 rt0, half4 rt1, half4 rt2, half4 rt3)
	{
		MtlColorRaw.rgb = rt0.rgb;
        
		// RenderFlags from rt3.b
		RenderFlags_10Bit = (int) (rt3.b * 1023.0h + 0.5h);

		#if USE_OCTAHEDRON_NORMAL == 0
            half3 decodedDir = (half3) DecodeNormalXYZ(rt1.rgb).xyz;
		#else
            half3 decodedDir = OctDecode(rt1.rg);
		#endif

        if (IsHair())
        {
            WorldTangent = decodedDir;
            WorldNormal = (half3)OctDecode(half2(rt0.a, rt2.r));
            Metallicity = 0;
            SubsurfaceProfileIndex = 0;
            SpecOcclusion = 0;
        }
        else
        {
            WorldNormal = decodedDir;
            WorldTangent = half3(0, 0, 0);
            Metallicity = rt2.r;

            // Unpack rt0.a back to the correct semantic field
            if (IsSubsurface())
            {
                SubsurfaceProfileIndex = (half)(rt0.a * 255.0h + 0.5h);
                SpecOcclusion = 0;
            }
            else
            {
                SubsurfaceProfileIndex = 0;
                SpecOcclusion = rt0.a;
            }
        }

        Mask = rt1.a;
		Specular = rt2.g;
		Roughness = rt2.b;
        AO = rt2.a;

        MotionVector.xy = (half2)DecodeMotionVector(rt3.rg);
        Opacity = rt3.a;
    }


};

#endif//_MobileBasePassPS_H_