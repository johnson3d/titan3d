#ifndef _DECAL_PS_H_
#define _DECAL_PS_H_

#include "../../Inc/GlobalDefine.cginc"
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/Math.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/SystemEnumDefine.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

// ---------------------------------------------------------------------------
// 延迟贴花 (直写 GBuffer, 无 DBuffer) —— 材质驱动
//
// 光栅化贴花投影盒 -> 按深度重建世界坐标 -> 变换到盒局域空间 -> 盒外 discard
//   -> 把 PS_INPUT 的插值量改写成"被投影表面"语境 -> 走标准材质路径 (DO_PS_MATERIAL)
//   -> 输出到 GBuffer 的 MRT, 由逐 RT 的硬件混合完成合成。
//
// 贴花的颜色 / 法线 / PBR 参数全部来自贴花材质的材质图 (对应 UE 的
// MaterialDomain=Deferred Decal): 材质图看到的 UV 是投影 UV, 世界坐标是被投影
// 表面的坐标, TBN 是贴花自己的切线基 —— 因此法线图 / 世界坐标节点 / 过程纹理等
// 一切材质图手段都能直接用。贴花贴图与 cbPerMaterial 由引擎的材质绑定机制
// (Mesh.TtAtom.BuildDrawCall) 自动挂上, 本文件不声明任何贴花纹理。
//
// RT 绑定与混合 (统一 3-RT, 由 C# 端 TtDecalPassNode 的 PSO 配置, 写掩码只开 RGB):
//   slot0 = GBuffer rt0 (base color)  混合 SrcAlpha/InvSrcAlpha
//   slot1 = GBuffer rt1 (法线)        ENV_DECAL_WRITE_NORMAL ? 开写关混合 : 写掩码 0
//   slot2 = GBuffer rt2 (R/M/S)       混合 SrcAlpha/InvSrcAlpha
// ENV_DECAL_WRITE_NORMAL 由材质在 TtMaterial.UpdateShaderCode 注入 (仅 RL_Decal 材质),
// 材质没有法线手段 (NormalNone) 时 C# 端自动降级为 0, 本文件 #ifndef 兜底。
//
// 统一 3-RT 后 rt1 是 RTV 不能再当 SRV 读, 角度淡出的基准法线改用几何法线
// (ddx/ddy 重建 + 朝相机翻转) —— 对角度淡出反而更合适: 它只反映表面朝向,
// 不被 BasePass 已写进 rt1 的着色法线扰动影响。
//
// 写掩码只开 RGB 是为了保住各 RT 的 alpha: rt0.a (SpecOcclusion/SSSProfile/ShiftOffset)、
// rt1.a (Mask)、rt2.a (AO)。rt3 (motion vector / RenderFlags) 完全不绑, 只当 SRV 读。
// ---------------------------------------------------------------------------

#ifndef ENV_DECAL_WRITE_NORMAL
#define ENV_DECAL_WRITE_NORMAL 0
#endif

cbuffer cbDecal DX_AUTOBIND
{
    // 三组通道的混合权重 + 全局强度。权重只控制"改多少",
    // 值本身来自贴花材质图 (mtl.mAlbedo / mRough / ...), 与 UE 的 DecalBlendMode 权重语义一致
    float DecalColorWeight;
    float DecalNormalWeight;
    float DecalMaterialWeight;
    float DecalGlobalIntensity;

    float DecalAngleFadeCos;     // cos(AngleFadeDegree), 低于该值不接受贴花
    float DecalEdgeFade;         // 盒体边缘淡出比例 (局域单位, 0~0.5)
    float DecalFadeStart;        // 开始按距离淡出 (米)
    float DecalFadeEnd;          // 完全淡出 (米)
};

Texture2D<float> DecalDepthBuffer DX_AUTOBIND;
SamplerState Samp_DecalDepthBuffer DX_AUTOBIND;

// rt3 永远不是本 pass 的 RTV, 因此任何模式下都能安全地当 SRV 读 (取 RenderFlags 判 ShadingMode)
Texture2D DecalGBufferRT3 DX_AUTOBIND;
SamplerState Samp_DecalGBufferRT3 DX_AUTOBIND;

struct PS_OUTPUT_DECAL
{
    float4 RT0 : SV_Target0;              // base color, a = 颜色混合权重
    float4 RTNormal : SV_Target1;         // 编码后的世界法线 (写不写由 PSO 决定)
    float4 RTMaterial : SV_Target2;       // R/M/S, a = 材质混合权重
};

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;
    Default_VSInput2PSInput(output, input);

#ifdef MDFQUEUE_FUNCTION
    MdfQueueDoModifiers(output, input);
#endif

    // 单位盒 [-0.5,0.5]^3 -> 世界 (WorldMatrix 含 Placement 的 Scale, 即盒子边长)
    float4 wp4 = mul(float4(output.vPosition.xyz, 1), WorldMatrix);
    output.Set_vWorldPos(wp4.xyz);
    output.vPosition = mul(wp4, GetViewPrjMtx());
    return output;
}

PS_OUTPUT_DECAL PS_Main(PS_INPUT input)
{
    PS_OUTPUT_DECAL output = (PS_OUTPUT_DECAL)0;

    // SV_Position 是像素坐标; ViewportSizeAndRcp.zw = 1/宽高
    float2 uv = input.vPosition.xy * ViewportSizeAndRcp.zw;

    float rawDepth = DecalDepthBuffer.SampleLevel(Samp_DecalDepthBuffer, uv, 0).r;
    // 天空没有表面, 不接受贴花
#if USE_INVERSE_Z == 1
    if (rawDepth <= 0.0)
#else
    if (rawDepth >= 1.0)
#endif
        discard;

    float3 worldPos = GetWorldPositionFromDepthValue(uv, rawDepth).xyz;

    // ---- 投影盒剔除 ----
    // WorldMatrixInverse 直接就是"世界 -> 盒局域 [-0.5,0.5]^3", 不需要额外传矩阵
    float3 localPos = mul(float4(worldPos, 1), WorldMatrixInverse).xyz;
    if (any(abs(localPos) > 0.5))
        discard;

    // ---- 贴花自己的世界空间基向量 (从 WorldMatrix 直接取, 免传 cbuffer) ----
    // UV 的 +U = 局域 +X, +V = 局域 -Y, 投影方向 = 局域 +Z
    float3 decalT = normalize(mul(float4(1, 0, 0, 0), WorldMatrix).xyz);
    float3 decalB = normalize(mul(float4(0, -1, 0, 0), WorldMatrix).xyz);
    float3 decalProjDir = normalize(mul(float4(0, 0, 1, 0), WorldMatrix).xyz);

    // ---- 基准法线: 几何法线 ----
    // (rt1 是 RTV 读不了; 几何法线对角度淡出也更合适 —— 只反映表面朝向,
    //  不被 BasePass 已写进 rt1 的着色法线扰动影响)
    float3 baseNormal = normalize(cross(ddx(worldPos), ddy(worldPos)));
    // 几何法线的朝向取决于三角形绕序, 统一翻到朝向相机那一侧
    if (dot(baseNormal, CameraPosition - worldPos) < 0.0)
        baseNormal = -baseNormal;

    // ---- 角度淡出: 表面越背对投影方向越不接受贴花 (避免侧壁拉伸) ----
    float facing = dot(baseNormal, -decalProjDir);
    if (facing <= DecalAngleFadeCos)
        discard;
    float angleFade = saturate((facing - DecalAngleFadeCos) / max(1.0 - DecalAngleFadeCos, 1e-4));

    // ---- 盒体边缘淡出 ----
    float edgeFade = 1.0;
    if (DecalEdgeFade > 1e-4)
    {
        float3 t = saturate((0.5 - abs(localPos)) / DecalEdgeFade);
        edgeFade = min(min(t.x, t.y), t.z);
    }

    // ---- 距离淡出 (逐像素算, 比 CPU 端算更准) ----
    float viewDist = length(CameraPosition - worldPos);
    float fadeEnd = max(DecalFadeEnd, DecalFadeStart + 0.001);
    float distanceFade = 1.0 - saturate((viewDist - DecalFadeStart) / (fadeEnd - DecalFadeStart));

    // ---- 把材质图放进"被投影表面"的语境 ----
    // 覆盖 PS_INPUT 的插值量, 材质图看到的一切都处于被投影表面坐标系:
    //   UV       = 投影 UV (局域 XY 映射到 [0,1]^2)
    //   WorldPos = 被投影表面的世界坐标
    //   Normal/Tangent = 贴花自己的切线基 (TBN 的 N 用几何法线, T 用贴花 U 方向)
    // 之后 DO_PS_MATERIAL 走的完全是引擎标准材质路径, 包括 CalcNormalMap 的
    // 法线图解包 (B 基由 CalcNormalMap 用 cross(T, N) 现算, 方向恰好是贴花的 V 方向)。
    float2 decalUV = float2(localPos.x + 0.5, 0.5 - localPos.y);
    input.Set_vUV(decalUV);
    input.Set_vWorldPos(worldPos);

    float3 N = baseNormal;
    float3 T = decalT - N * dot(N, decalT);
    float tLen = length(T);
    if (tLen < 1e-4)
        discard;   // 投影方向与表面几乎平行, 切线基退化
    T /= tLen;
#if USE_PS_Tangent == 1
    input.vTangent.w = 0;   // 手性: w=0 走 CalcNormalMap 的 +cross 分支
#endif
    input.Set_vNormal(N);
    input.Set_vTangent(T);

    // ---- 标准材质路径 (与 DeferredBasePassPS 一致) ----
    MTL_OUTPUT mtl = Default_PSInput2Material(input);
#ifndef DO_PS_MATERIAL
#define DO_PS_MATERIAL DoDefaultPSMaterial
#endif
    DO_PS_MATERIAL(input, mtl);

    // ---- 贴花 alpha: 材质图 Opacity × 节点淡出 ----
    float alpha = (float)mtl.mAlpha * DecalGlobalIntensity * angleFade * edgeFade * distanceFade;
    if (alpha <= 0.001)
        discard;

    // ---- rt0: base color (硬件混合, 权重走 SV_Target0.a) ----
    // 通道语义与 DeferredBasePassPS 的 GBuffer.MtlColorRaw 一致 (mAlbedo 直存 + 自发光)
    output.RT0.rgb = (float3)mtl.mAlbedo + (float3)mtl.mEmissive;
    output.RT0.a = saturate(alpha * DecalColorWeight);

    // ---- rt2: Roughness / Metallic / Specular ----
    // 通道顺序必须与 DeferredCommon.cginc 的 EncodeGBuffer 一致:
    //   rt2.r = Metallicity, rt2.g = Specular, rt2.b = Roughness, rt2.a = AO(写掩码挡掉)
    // mRough 在 Deferred 端就是 roughness 语义 (无 1-x 换算, 与 BasePass 一致)
    output.RTMaterial.r = mtl.mMetallic;
    output.RTMaterial.g = mtl.mAbsSpecular;
    output.RTMaterial.b = mtl.mRough;
    output.RTMaterial.a = saturate(alpha * DecalMaterialWeight);

#if ENV_DECAL_WRITE_NORMAL
    // Hair 的 rt1 存的是切线, 且法线被拆到两张 RT 里 —— 覆盖会破坏各向异性高光的切线基
    half4 rt3 = (half4)DecalGBufferRT3.SampleLevel(Samp_DecalGBufferRT3, uv, 0);
    int renderFlags = (int)(rt3.b * 1023.0h + 0.5h);
    int shadingMode = (renderFlags & SHADINGMODE_BIT_MASK) >> SHADINGMODE_BIT_OFFSET;
    if (shadingMode == EShadingMode_Hair)
        discard;

    // 材质图输出的世界法线。NormalNone 时 GetWorldNormal 返回零向量 ——
    // 理论上 C# 端已降级 (UpdateShaderCode 不注入 ENV_DECAL_WRITE_NORMAL) 不会走到这,
    // 这里退化成 baseNormal 让 lerp 结果等于"不写", 防止 normalize(0) 出 NaN
    float3 decalNormalWS = mtl.GetWorldNormal(input);
    float nLen = length(decalNormalWS);
    decalNormalWS = (nLen > 1e-4) ? decalNormalWS / nLen : baseNormal;

    // rt1 上硬件混合是关闭的, 所以淡出必须在这里烤进法线值本身:
    // 世界空间 lerp(基准法线, 贴花法线, w), w -> 0 时写回的就是基准法线, 等于没写。
    float normalWeight = saturate(alpha * DecalNormalWeight);
    float3 worldNormal = normalize(lerp(baseNormal, decalNormalWS, normalWeight));

    // 编码方式必须与 DeferredCommon.cginc 的 EncodeGBuffer 非 Hair 分支完全一致
    #if USE_OCTAHEDRON_NORMAL == 0
        output.RTNormal.rgb = (float3)EncodeNormalXYZ(worldNormal);
    #else
        output.RTNormal.rg = (float2)OctEncode(worldNormal);
        output.RTNormal.b = 0;
    #endif
    output.RTNormal.a = 1;   // 被写掩码挡掉, 只是别留未初始化值
#endif

    return output;
}

#endif // _DECAL_PS_H_
