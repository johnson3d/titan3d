#ifndef __SUNSHAFT_COMMON_SHADING_H__
#define __SUNSHAFT_COMMON_SHADING_H__
#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

struct FSunShaftStruct
{
    float4 BlurRadius4;
    float4 SunPosition;

    float LumThreshold;
    float DepthThreshole;
    float BlurDecay;
    int BlurSampleCount;
};

cbuffer cbShadingEnv DX_AUTOBIND
{
    FSunShaftStruct SunShaftStruct;
}

#endif
//