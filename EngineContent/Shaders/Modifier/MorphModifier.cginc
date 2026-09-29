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

#if USE_PS_Normal == 1 && USE_PS_Tangent == 1
	// 切线必须跟着一起 morph: CalcNormalMap 直接用 (T, N, ±cross(T,N)) 建 TBN, 不做任何
	// 正交化 —— 法线换成了 morph 后的朝向而切线还停在基础网格的朝向, TBN 就是歪的,
	// 形变越大法线贴图的扰动方向偏得越离谱。
	//
	// 两步走, 少一步都不够:
	//   1. 加 DeltaTangent。导入器用形变后的位置配合 UV 重算切线再作差(与法线同源),
	//      这一项带的是切线绕法线的旋转 —— 各向异性的拉伸/剪切会让 UV 梯度方向在 3D 里
	//      转一下, 那部分信息只能靠真 delta 拿到, 光靠重投影是变不出来的。
	//   2. 沿 morph 后的法线做 Gram-Schmidt 重投影。多个 morph 线性叠加、以及重算本身的
	//      离散误差, 都会让 T 不再严格垂直于 N; 而 B = ±cross(T,N) 天然垂直于 N,
	//      所以 TBN 里唯一的非正交来源就是 T 自己, 重投影一次就能把它清掉。
	//
	// w 是手性, 绝对不能碰: CalcNormalMap 靠它选 ±cross 决定副切线朝向, 改了会让
	// 法线贴图整体翻面。Set_vTangent 只写 xyz 正是为此。
	float3 morphedTangent = vert.vTangent.xyz + (float3)delta.DeltaTangent;
	morphedTangent = morphedTangent - vert.vNormal.xyz * dot(vert.vNormal.xyz, morphedTangent);
	float morphedTangentLenSq = dot(morphedTangent, morphedTangent);
	// 重投影结果趋零有两种来路: 形变把切线转到与法线几乎平行, 或者这条管线本就没有
	// 切线顶点流(vTangent 恒为 0 且 delta 也是 0)。两种情况都保留原值 —— 歪一点也比把
	// 零向量塞进 TBN 好, 后者会让整块表面的法线输出 NaN。
	if (morphedTangentLenSq > 1e-12f)
	{
		vert.vTangent.xyz = morphedTangent * rsqrt(morphedTangentLenSq);
	}
	vsOut.Set_vTangent(vert.vTangent.xyz);
#endif
}

#endif //_MorphModifier_cginc_
