#ifndef __FX_VOLUMECLOUD_H__
#define __FX_VOLUMECLOUD_H__
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

// 噪声图 
Texture3D CloudNoiseTex;
SamplerState Samp_CloudNoiseTex;
Texture2D WeatherTex;
SamplerState Samp_WeatherTex;
// 屏幕纹理
Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;

struct FShadingStruct
{
    float4 CloudColor;
    float4 ShadowColor;
    float4 LightColor;
    
    float CloudDensity;
    float CloudCoverage;
    float CloudHeightMin;
    float CloudHeightMax;
    
    float2 CloudScale;
    float LightAbsorption;
    float DarknessThreshold;
    
    float3 LightDir;
    int MaxSteps;
};

cbuffer cbShadingEnv DX_AUTOBIND
{
    FShadingStruct ShadingStruct;
};

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT) 0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    output.psCustomUV0.xyz = CornerRays[input.vVertexID];

    return output;
}

// 光线与AABB相交测试
bool RayBoxIntersection(float3 rayOrigin, float3 rayDir,
                                   float3 boxMin, float3 boxMax,
                                   out float tMin, out float tMax)
{
    float3 invRayDir = 1.0 / rayDir;
    float3 t1 = (boxMin - rayOrigin) * invRayDir;
    float3 t2 = (boxMax - rayOrigin) * invRayDir;
                
    float3 tMin3 = min(t1, t2);
    float3 tMax3 = max(t1, t2);
                
    tMin = max(max(tMin3.x, tMin3.y), tMin3.z);
    tMax = min(min(tMax3.x, tMax3.y), tMax3.z);
                
    return tMax > max(tMin, 0.0);
}
            
// 获取云密度
float GetCloudDensity(float3 worldPos)
{
    // 计算UV
    float2 uv = worldPos.xz * ShadingStruct.CloudScale;
    float height = worldPos.y;
                
    // 高度因子
    float heightFactor = saturate((height - ShadingStruct.CloudHeightMin) / (ShadingStruct.CloudHeightMax - ShadingStruct.CloudHeightMin));
    float heightGradient = 4.0 * heightFactor * (1.0 - heightFactor);
                
    // 采样天气图
    float4 weatherData = WeatherTex.SampleLevel(Samp_WeatherTex, uv, 0);
    float coverage = weatherData.a * ShadingStruct.CloudCoverage;
                
    // 基础密度
    float baseDensity = CloudNoiseTex.Sample(Samp_CloudNoiseTex, float3(uv * 0.5, height * 0.0005)).r;
                
    // 添加细节
    float detailNoise = CloudNoiseTex.Sample(Samp_CloudNoiseTex, float3(uv * 2.0, height * 0.001)).r * 0.5;
    // 侵蚀效果
    baseDensity = saturate(baseDensity - detailNoise * 0.2);
                
    // 应用覆盖率和高度，这个公式需要想办法处理边缘的柔和过渡，否则可能出现一条直线
    float density = saturate(baseDensity - (1.0 - coverage)) * heightGradient;
                
    // 重映射
    density = saturate(density * ShadingStruct.CloudDensity * 4.0);
                
    return density;
}
            
// 光线步进中的光照计算
float LightMarch(float3 pos)
{
    float3 lightStep = ShadingStruct.LightDir * 50.0; // 光照步长
    float totalDensity = 0.0;
    float transmittance = 1.0;
                
    // 向光源方向步进
    [loop]
    for (int i = 0; i < 8; i++)
    {
        pos += lightStep;
        float density = GetCloudDensity(pos);
        totalDensity += density;
                    
        // 计算透射率
        transmittance *= exp(-density * ShadingStruct.LightAbsorption);
                    
        // 提前退出
        if (transmittance < ShadingStruct.DarknessThreshold)
            break;
    }
                
    return transmittance;
}
            
// 主光线步进函数
float4 RayMarchClouds(float3 rayOrigin, float3 rayDir, float maxDistance)
{
    // 定义云层边界框
    float3 boxMin = float3(-10000, ShadingStruct.CloudHeightMin, -10000);
    float3 boxMax = float3(10000, ShadingStruct.CloudHeightMax, 10000);
                
                // 计算与云层相交
    float tMin, tMax;
    if (!RayBoxIntersection(rayOrigin, rayDir, boxMin, boxMax, tMin, tMax))
        return float4(0, 0, 0, 0);
                
    // 限制距离
    tMin = max(tMin, 0);
    tMax = min(tMax, maxDistance);
                
    // 计算步长
    float rayLength = tMax - tMin;
    float stepSize = rayLength / ShadingStruct.MaxSteps;
                
    // 随机起始偏移（减少条带伪影）
    float offset = frac(sin(dot(rayDir, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
    float3 currentPos = rayOrigin + rayDir * (tMin + offset * stepSize);
                
    // 累积颜色和透明度
    float3 totalColor = float3(0, 0, 0);
    float transmittance = 1.0;
                
    [loop]
    for (int i = 0; i < ShadingStruct.MaxSteps; i++)
    {
        // 采样密度
        float density = GetCloudDensity(currentPos);
                    
        if (density > 0.01)
        {
            // 计算光照
            float lightTransmittance = LightMarch(currentPos);
                        
            // 基础颜色
            float3 cloudColor = lerp(ShadingStruct.ShadowColor.rgb, ShadingStruct.CloudColor.rgb,
                                                saturate(lightTransmittance * 2.0));
                        
            // 乘以光照颜色
            cloudColor *= ShadingStruct.LightColor.rgb;
                        
            // 计算衰减
            float alpha = 1.0 - exp(-density * stepSize * 0.1);
                        
            // 累积颜色（从前到后混合）
            totalColor += transmittance * cloudColor * alpha;
            transmittance *= (1.0 - alpha);
                        
            // 提前退出
            if (transmittance < 0.01)
                break;
        }
                    
        // 前进
        currentPos += rayDir * stepSize;
                    
        // 边界检查
        if (distance(currentPos, rayOrigin) > tMax)
            break;
    }
                
    return float4(totalColor, 1.0 - transmittance);
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT) 0;
    float2 uv = input.vUV;
    // 获取背景颜色
    float4 sceneColor = ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
                
    // 计算世界空间射线
    float3 rayOrigin = CameraPosition;
    float3 rayDir = input.Get_ScreenViewVector();
    rayDir = normalize(rayDir);
                
    // 执行光线步进
    float4 cloudColor = RayMarchClouds(rayOrigin, rayDir, gZFar);
                
    // 与场景混合（预乘Alpha混合）
    float3 result = sceneColor.rgb * (1.0 - cloudColor.a) + cloudColor.rgb;
                
    output.RT0 = float4(result, 1);
    return output;
}

#endif//__FX_VOLUMECLOUD_H__