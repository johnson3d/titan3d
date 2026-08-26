#ifndef _MorphModifier_cginc_
#define _MorphModifier_cginc_

// FMorphDelta 是 C# 端 [TtShaderDefine] 反射注入的 struct (见 CSharpCode/Grapics/Mesh/MorphTarget.cs),
// 必须 include GlobalDefine.cginc 才能拿到注入块, 否则报 X3000 unexpected token 'FMorphDelta'
// 并连带一串误导性的 X3004。详见 CodingGuidelines.md §3.3。
#include "../Inc/GlobalDefine.cginc"

// 逐顶点稠密累加后的 morph 偏移, 长度 = mesh 顶点数, 由 TtMorphModifier 在 OnDrawCall 里绑定。
// TtMorphModifier 始终按顶点数分配并绑定该 buffer (即使权重全零, 内容为零), 因此这里不需要
// 判空分支, 也不会越界。
StructuredBuffer<FMorphDelta> MorphDeltas;

// morph 必须在骨骼混合之前执行: TtMdfQueue2<TtMorphModifier, TtSkinModifier> 保证了
// MdfQueueDoModifiers 里本函数先于 DoSkinModifierVS 发射, 且两者共享同一个局部 vert 副本。
//
// 这里必须同时写 vert 和 vsOut, 原因是各 ShadingEnv 的 VS_Main 里
// Default_VSInput2PSInput(output, input) 在 MdfQueueDoModifiers 之前就跑过了
// (见 ShadingEnv/Deferred/DeferredOpaque.cginc), vsOut 里已经是未形变的副本:
//   - 写 vert : 让后续的 DoSkinModifierVS 拿到已形变的顶点参与骨骼加权 (蒙皮角色路径);
//   - 写 vsOut: 让没有后续 modifier 覆写 vsOut 的场景也能生效 (纯 morph 静态网格路径)。
// 蒙皮路径下 DoSkinModifierVS 随后会整体覆写 vsOut, 因此这里写 vsOut 不会造成冲突。
void DoMorphModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	FMorphDelta delta = MorphDeltas[vert.vVertexID];

	// 字段名用剔掉 m 前缀的裸名: C# 端 FMorphDelta.mDeltaPosition 经引擎反射后在 HLSL 里
	// 叫 DeltaPosition (CodingGuidelines.md §3.2)。
	vert.vPosition.xyz += (float3)delta.DeltaPosition;
	vsOut.vPosition.xyz = vert.vPosition.xyz;

#if USE_PS_Normal == 1
	// DCC 里的法线 delta 是 "目标法线 - 基础法线", 相加即得目标法线; 归一化防止
	// 多个 morph 叠加后长度漂移。
	float3 morphedNormal = vert.vNormal.xyz + (float3)delta.DeltaNormal;
	float morphedNormalLenSq = dot(morphedNormal, morphedNormal);
	if (morphedNormalLenSq > 1e-12f)
	{
		vert.vNormal.xyz = morphedNormal * rsqrt(morphedNormalLenSq);
	}
	vsOut.vNormal.xyz = vert.vNormal.xyz;
#endif
}

#endif //_MorphModifier_cginc_
