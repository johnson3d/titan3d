#ifndef __EXP_HEIGHT_FOG_H__
#define __EXP_HEIGHT_FOG_H__

// 指数高度雾 (Exponential Height Fog)
// 注意: TitanEngine 使用左手系、Y-up、单位=米, 高度轴是 .y (UE 原版用 .z, 移植时已改为 .y)
// FogDensity   : 雾在参考高度处的浓度系数
// FogHeightFalloff : 浓度随高度衰减的速率 (值越大, 高处雾越稀薄)
// StartDistance    : 距相机多远开始起雾
// InscatterColor / InscatteringExponent / LightPosition : 太阳方向的内散射(逆光泛光)控制
struct FFogStruct
{
    /// Uniform Begin
    float3 FogColor;
    float MinFogOpacity; 

    float FogDensity;
    float FogEnd;
    float FogHeightFalloff;
    float StartDistance;

    float3 InscatterColor;
    float InscatteringExponent;

    float3 LightPosition;
    float InscatterStartDistance;
    /// Uniform End

    float3 GetExpHeightFogColor(float3 worldPos, float3 color, float linearDepth, float3 ray, float2 uv)
    {
        // worldPos 由调用方传入 GetWorldPositionFromDepthValue(uv,depth).xyz, 已是真实世界坐标,
        // 不要再用视空间近似 (CameraPosition + linearDepth*ray) 覆盖它, 否则相机有 pitch 时高度算错.
        float3 toFrag = worldPos - CameraPosition;
        float rayLength = length(toFrag);

        // 相机所在高度的基础雾浓度 (FogEnd 复用为参考高度 h0, 米)
        float densityAtCamera = FogDensity * exp2(-FogHeightFalloff * (CameraPosition.y - FogEnd));

        // 沿视线的高度积分项; falloff -> 0 时取解析极限 1, 避免 0/0 出 NaN
        float falloff = FogHeightFalloff * (worldPos.y - CameraPosition.y);
        float lineIntegral = (abs(falloff) > 1e-3f) ? (1.0f - exp2(-falloff)) / falloff : 1.0f;

        // 超过 StartDistance 之后才累积的光学厚度
        float dist = max(rayLength - StartDistance, 0.0f);
        float opticalDepth = densityAtCamera * lineIntegral * dist;

        // 指数雾的正确映射: 透射率 exp2(-tau), 不透明度 = 1 - 透射率.
        // (原移植代码直接 saturate(tau), 远距离 tau 巨大 -> 全屏爆白, 云被雾色抹掉)
        float fog = 1.0f - exp2(-opticalDepth);
        fog = saturate(max(fog, MinFogOpacity));

        // 太阳方向的内散射 (逆光泛光)
        float3 viewDir = (rayLength > 1e-4f) ? (toFrag / rayLength) : float3(0.0f, 0.0f, 1.0f);
        float sunAmount = pow(saturate(dot(viewDir, normalize(LightPosition.xyz))), InscatteringExponent);

        float dirExponentialHeightLineIntegral = max(rayLength - InscatterStartDistance, 0.0f);
        float DirectionalInscatteringFogFactor = saturate(exp2(-dirExponentialHeightLineIntegral));
        sunAmount *= (1 - DirectionalInscatteringFogFactor);

        //在雾色与内散射色之间按 sunAmount 混合
        float3 fogColor = lerp(FogColor.rgb, InscatterColor.rgb, sunAmount);

        return lerp(color.rgb, fogColor.rgb, fog);
    }
};

#endif
//
