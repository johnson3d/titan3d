// PS_INPUT::Set_vTangent 定在 GlobalDefine.cginc 里(它带 include guard, 重复 include 无害)。
// 不显式写上的话, 纯蒙皮路径(没有 MorphModifier 带着 include 的场景)会报找不到方法。
#include "../Inc/GlobalDefine.cginc"
#include "../CBuffer/VarBase_PerSkinMesh.cginc"

half3 RotateVec(in half3 inPos, in half4 inQuat)
{
	half3 uv = (half3)cross(inQuat.xyz, inPos);
	half3 uuv = (half3)cross(inQuat.xyz, uv);
	uv = uv * (2.0h * inQuat.w);
	uuv *= 2.0h;
	
	return inPos + uv + uuv;
}

half3 transform_quat(half3 v, half4 quat)
{
	return v + (half3)cross(quat.xyz, cross(quat.xyz, v) + quat.w * v) * 2;
}

void DoSkinModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	half3      Pos = 0.0f;
	half3      Normal = 0.0f;    
	float weight = vert.vSkinWeight[0] + vert.vSkinWeight[1] + vert.vSkinWeight[2] + vert.vSkinWeight[3];
	if(weight  == 0.0f)
	{
		vert.vPosition.xyz = (float3)AbsBonePos[vert.vSkinIndex[0]].xyz;
#if USE_PS_Normal == 1
		vert.vNormal.xyz = (float3)vert.vNormal.xyz;	
#endif
		return;
	}
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[0]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[0]])) * (half)vert.vSkinWeight[0];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[1]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[1]])) * (half)vert.vSkinWeight[1];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[2]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[2]])) * (half)vert.vSkinWeight[2];
	Pos.xyz += ((half3)AbsBonePos[vert.vSkinIndex[3]].xyz + transform_quat((half3)vert.vPosition, (half4)AbsBoneQuat[vert.vSkinIndex[3]])) * (half)vert.vSkinWeight[3];
	vsOut.vPosition.xyz = Pos.xyz;

#if USE_PS_Normal == 1
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[0]]) * (half)vert.vSkinWeight[0];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[1]]) * (half)vert.vSkinWeight[1];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[2]]) * (half)vert.vSkinWeight[2];
	Normal.xyz += transform_quat((half3)vert.vNormal.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[3]]) * (half)vert.vSkinWeight[3];
	vsOut.vNormal.xyz = (float3)Normal;	

#if USE_PS_Tangent == 1
	// 切线必须跟着骨骼一起转。不算这一段的后果并不是"略微不准": 往下走到 ShadingEnv 里
	// 切线只会被乘上 WorldMatrix, 骨骼旋转从头到尾没作用到切线上 —— 于是角色抬手、
	// 转头时法线跟着转而切线还指着 bind pose 的方向, CalcNormalMap 拿到的 TBN 整个是歪的,
	// 法线贴图的凹凸会随动作乱漂。
	//
	// 四骨骼各自旋转后线性混合, T 与 N 是分开混的, 关节处两者的正交性会被破坏一点;
	// 而 CalcNormalMap 直接用 (T, N, ±cross(T,N)) 建 TBN 而不做正交化, 没人会在下游帮它拉回来,
	// 所以在这里就把 T 重投影到 N 的切平面内。Normal 此时未归一化(与上面一致, 留给
	// ShadingEnv 的 normalize), 所以得除 dot(n,n) 而不能直接用单位向量的简式。
	half3 Tangent = 0.0f;
	Tangent.xyz += transform_quat((half3)vert.vTangent.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[0]]) * (half)vert.vSkinWeight[0];
	Tangent.xyz += transform_quat((half3)vert.vTangent.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[1]]) * (half)vert.vSkinWeight[1];
	Tangent.xyz += transform_quat((half3)vert.vTangent.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[2]]) * (half)vert.vSkinWeight[2];
	Tangent.xyz += transform_quat((half3)vert.vTangent.xyz, (half4)AbsBoneQuat[vert.vSkinIndex[3]]) * (half)vert.vSkinWeight[3];

	float3 skinnedNormal = (float3)Normal;
	float3 skinnedTangent = (float3)Tangent;
	float skinnedNormalLenSq = dot(skinnedNormal, skinnedNormal);
	if (skinnedNormalLenSq > 1e-12f)
	{
		float3 orthoTangent = skinnedTangent - skinnedNormal * (dot(skinnedNormal, skinnedTangent) / skinnedNormalLenSq);
		// 重投影结果趋零 = 切线被转到与法线几乎平行(或这条管线没切线顶点流,
		// vTangent 恒为 0)。保留未正交化的值 —— 歪一点也比下游 normalize(0) 出 NaN 好。
		if (dot(orthoTangent, orthoTangent) > 1e-12f)
			skinnedTangent = orthoTangent;
	}
	// 只写 xyz: w 是手性, CalcNormalMap 靠它选 ±cross 决定副切线朝向, 而刚体旋转
	// 不会改变手性, 改了反而会把法线贴图整体翻面。
	vsOut.Set_vTangent(skinnedTangent);
#endif
#endif
}

//#define DO_VS_MODIFIER DoSkinModifierVS