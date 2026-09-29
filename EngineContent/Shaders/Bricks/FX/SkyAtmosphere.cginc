#ifndef __FX_SKYATMOSPHERE_H__
#define __FX_SKYATMOSPHERE_H__
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

// 屏幕颜色 / 深度
Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;
Texture2D DepthBuffer;
SamplerState Samp_DepthBuffer;

// 与 C# 端 TtSkyAtmosphereSceneNode.FSkyAtmosphereStruct 逐字段严格对齐(Pack=16)
struct FSkyAtmosphereStruct
{
    float3 SunDirection;    // 太阳光行进方向(指向太阳 = -SunDirection)
    float SunIntensity;

    float3 SunColor;
    float PlanetRadius;     // 行星半径(米)

    float3 GroundAlbedo;
    float AtmosphereHeight; // 大气层厚度(米)

    float MieAnisotropy;    // Mie 各向异性 g
    float SunDiskSize;      // 太阳圆盘 cos 阈值(越接近 1 越小)
    float RayleighScale;    // Rayleigh 散射强度倍率
    float MieScale;         // Mie 散射强度倍率

    float OzoneScale;       // 臭氧吸收倍率
    float Exposure;         // 曝光
    int SampleCount;        // 视线采样数
    int LightSampleCount;   // 向光采样数
};

cbuffer cbSkyAtmosphere DX_AUTOBIND
{
    FSkyAtmosphereStruct Sky;
};

// Hillaire 2020 海平面散射/吸收系数(单位: m^-1, 由 km^-1 换算)
static const float3 kRayleighScattering = float3(5.802e-6, 13.558e-6, 33.1e-6);
static const float3 kMieScattering = float3(3.996e-6, 3.996e-6, 3.996e-6);
static const float3 kMieExtinction = float3(4.44e-6, 4.44e-6, 4.44e-6);
static const float3 kOzoneAbsorption = float3(0.650e-6, 1.881e-6, 0.085e-6);
static const float kRayleighHeight = 8000.0; // 米
static const float kMieHeight = 1200.0;       // 米

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT) 0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    output.psCustomUV0.xyz = CornerRays[input.vVertexID];

    return output;
}

float2 RaySphere(float3 ro, float3 rd, float3 center, float r)
{
    float3 oc = ro - center;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - r * r;
    float disc = b * b - c;
    if (disc < 0.0)
        return float2(-1.0, -1.0);
    float sq = sqrt(disc);
    return float2(-b - sq, -b + sq);
}

// 三层介质密度: rayleigh(指数) / mie(指数) / ozone(帐篷函数), 单位 km 换算成米
float3 SampleMediumDensity(float altitude)
{
    float densityR = exp(-altitude / kRayleighHeight);
    float densityM = exp(-altitude / kMieHeight);
    // 臭氧: 以 25km 为中心, 半宽 15km 的线性帐篷
    float densityO = saturate(1.0 - abs(altitude - 25000.0) / 15000.0);
    return float3(densityR, densityM, densityO);
}

// 向太阳方向的透射率(二次步进)
float3 SunTransmittance(float3 p, float3 sunDir, float3 center, float planetR, float atmosphereR)
{
    float2 atmo = RaySphere(p, sunDir, center, atmosphereR);
    if (atmo.y < 0.0)
        return float3(1.0, 1.0, 1.0);
    // 被行星本体遮挡 -> 完全阴影
    float2 planet = RaySphere(p, sunDir, center, planetR);
    if (planet.x > 0.0)
        return float3(0.0, 0.0, 0.0);

    int steps = max(Sky.LightSampleCount, 1);
    float t = 0.0;
    float tMax = atmo.y;
    float dt = tMax / (float) steps;

    float3 opticalDepth = float3(0, 0, 0);
    [loop]
    for (int i = 0; i < steps; i++)
    {
        float3 sp = p + sunDir * (t + dt * 0.5);
        float alt = length(sp - center) - planetR;
        float3 dens = SampleMediumDensity(max(alt, 0.0));

        float3 extinction = kRayleighScattering * Sky.RayleighScale * dens.x
                          + kMieExtinction * Sky.MieScale * dens.y
                          + kOzoneAbsorption * Sky.OzoneScale * dens.z;
        opticalDepth += extinction * dt;
        t += dt;
    }
    return exp(-opticalDepth);
}

// Rayleigh 相位
float RayleighPhase(float cosTheta)
{
    return 3.0 / (16.0 * 3.1415926) * (1.0 + cosTheta * cosTheta);
}

// Henyey-Greenstein (Mie)
float MiePhase(float cosTheta, float g)
{
    float g2 = g * g;
    float denom = 1.0 + g2 - 2.0 * g * cosTheta;
    return (1.0 - g2) / (4.0 * 3.1415926 * pow(max(denom, 1e-4), 1.5));
}

float3 IntegrateScattering(float3 ro, float3 rd, float tMax, float3 sunDir, out float3 transmittance)
{
    float3 center = float3(0.0, -Sky.PlanetRadius, 0.0);
    float planetR = Sky.PlanetRadius;
    float atmosphereR = Sky.PlanetRadius + Sky.AtmosphereHeight;

    transmittance = float3(1.0, 1.0, 1.0);

    float2 atmo = RaySphere(ro, rd, center, atmosphereR);
    if (atmo.y < 0.0)
        return float3(0, 0, 0);

    float tStart = max(atmo.x, 0.0);
    float tEnd = min(atmo.y, tMax);
    // 视线打到地面则截断
    float2 planet = RaySphere(ro, rd, center, planetR);
    if (planet.x > 0.0)
        tEnd = min(tEnd, planet.x);
    if (tEnd <= tStart)
        return float3(0, 0, 0);

    int steps = max(Sky.SampleCount, 1);
    float dt = (tEnd - tStart) / (float) steps;

    float cosTheta = dot(rd, sunDir);
    float phaseR = RayleighPhase(cosTheta);
    float phaseM = MiePhase(cosTheta, Sky.MieAnisotropy);

    float3 inScatter = float3(0, 0, 0);
    float3 opticalDepth = float3(0, 0, 0);
    float t = tStart;

    [loop]
    for (int i = 0; i < steps; i++)
    {
        float3 p = ro + rd * (t + dt * 0.5);
        float alt = length(p - center) - planetR;
        float3 dens = SampleMediumDensity(max(alt, 0.0));

        float3 extinction = kRayleighScattering * Sky.RayleighScale * dens.x
                          + kMieExtinction * Sky.MieScale * dens.y
                          + kOzoneAbsorption * Sky.OzoneScale * dens.z;
        opticalDepth += extinction * dt;
        float3 viewTrans = exp(-opticalDepth);

        float3 sunTrans = SunTransmittance(p, sunDir, center, planetR, atmosphereR);

        float3 scatterR = kRayleighScattering * Sky.RayleighScale * dens.x * phaseR;
        float3 scatterM = kMieScattering * Sky.MieScale * dens.y * phaseM;

        inScatter += (scatterR + scatterM) * sunTrans * viewTrans * dt;
        t += dt;
    }

    transmittance = exp(-opticalDepth);
    return inScatter * Sky.SunColor * Sky.SunIntensity;
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT) 0;
    float2 uv = input.vUV;

    float4 sceneColor = ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);

    float depth = DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
    float linearZ = LinearFromDepth(depth);
    bool bSky = (linearZ >= ZFar * 0.999);

    float3 rayOrigin = CameraPosition;
    float3 rayDir = normalize(input.Get_ScreenViewVector());
    float3 sunDir = -normalize(Sky.SunDirection);

    // 天空像素: 步进到大气层外; 有几何的像素只算到几何处的空气透视
    float3 sceneWorld = GetWorldPositionFromDepthValue(uv, depth).xyz;
    float tMax = bSky ? 1.0e12 : length(sceneWorld - CameraPosition);

    float3 transmittance;
    float3 atmosphere = IntegrateScattering(rayOrigin, rayDir, tMax, sunDir, transmittance);

    float3 result;
    if (bSky)
    {
        // 太阳圆盘
        float cosSun = dot(rayDir, sunDir);
        float disk = smoothstep(Sky.SunDiskSize, Sky.SunDiskSize + 0.0002, cosSun);
        float3 sunDisk = disk * Sky.SunColor * Sky.SunIntensity * transmittance;
        result = atmosphere + sunDisk;
    }
    else
    {
        // 空气透视: 场景色被大气透射衰减并叠加内散射
        result = sceneColor.rgb * transmittance + atmosphere;
    }

    // 简单曝光 + Reinhard 色调
    result *= Sky.Exposure;
    result = result / (1.0 + result);

    output.RT0 = float4(result, 1.0);
    return output;
}

#endif//__FX_SKYATMOSPHERE_H__
