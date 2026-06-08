#ifndef _DEFERRED_OPAQUE_
#define _DEFERRED_OPAQUE_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
#include "DeferredCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"
 
//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif

#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;

	float4 wp4 = mul(float4(output.vPosition.xyz, 1), WorldMatrix);
    output.Set_vWorldPos(wp4.xyz);
    output.Set_vNormal(normalize(mul(float4(output.Get_vNormal(), 0), WorldMatrix).xyz));
    output.Set_vTangent(normalize(mul(float4(output.Get_vTangent().xyz, 0), WorldMatrix).xyz));
#else
	float4 wp4 = float4(output.vPosition.xyz, 1);
#endif

	// 投影矩阵已经不含 jitter, prePos / output.vPosition 都是纯 view-projection 结果.
	// motion vector 由 PS 端按 (currClip - prevClip) 计算, 完全不受 jitter 影响.
	float3 preWorldPos = mul(float4(output.vPosition.xyz, 1), PreWorldMatrix).xyz;
	float4 prePos = mul(float4(preWorldPos, 1), GetPreFrameViewPrjMtx());

	output.vPosition = mul(wp4, GetViewPrjMtx());

#if USE_PS_Custom1 == 1
	// 上一帧 clip-space, 用于 PS 端算 MV. 必须在注入 jitter 之前赋值, 保持纯几何位移.
	output.psCustomUV1 = prePos;
#endif

#if USE_PS_Custom2 == 1
	// 当前帧 clip-space (无 jitter), 用于 PS 端算 MV.
	output.psCustomUV2 = output.vPosition;
#endif

	// 把 jitter 注入到 SV_Position (clip space). 仅影响光栅化采样位置, 不影响:
	//   - psCustomUV1/UV2 (上面已经先赋值好的 MV 用纯净 clip)
	//   - cbPerCamera 的任何矩阵
	//   - 其他 pass (shadow / GI / 反射等) 的几何
	// JitterOffset 是 UV 单位 ((halton-0.5)/viewport_size). clip.xy / w = NDC, NDC*0.5+0.5 = UV.
	// 要让所有深度的像素在屏幕上偏移相同的亚像素量:
	//   clip 偏移 = jitterUV * 2 (UV→NDC) * w (NDC→clip), 透视除法后恒为 jitterUV*2 NDC.
	output.vPosition.xy += JitterOffset.xy * (2.0 * output.vPosition.w);
#if USE_PS_Custom0 == 1
	output.psCustomUV0.w = output.vPosition.w;
#endif

	return output;
}

#include "DeferredBasePassPS.cginc"

PS_OUTPUT PS_Main(PS_INPUT input)
{	
	/*PS_OUTPUT output = (PS_OUTPUT)0;
	output.RT0 = float4(1, 1, 1, 1);
	return output;*/
	return PS_MobileBasePass(input);
}

#endif//#ifndef _DEFERRED_OPAQUE_