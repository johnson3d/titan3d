#ifndef __FX_VOLUMETRICCLOUD_H__
#define __FX_VOLUMETRICCLOUD_H__
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

// 3D 噪声(基础形状 + 高频细节侵蚀共用一张纹理, 分别用不同的采样尺度)
Texture3D CloudNoiseTex;
SamplerState Samp_CloudNoiseTex;
// 天气图: r=覆盖率(coverage) g=密度(density) b=云类型(type) a=海拔(elevation)
Texture2D WeatherTex;
SamplerState Samp_WeatherTex;
// 屏幕颜色
Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;
// 场景深度(用于遮挡剔除, 云不应画在实体几何前面)
Texture2D DepthBuffer;
SamplerState Samp_DepthBuffer;

// 与 C# 端 TtVolumetricCloudSceneNode.FShadingStruct 逐字段严格对齐(Pack=16)
struct FShadingStruct
{
    float4 CloudColor;       // 云基色
    float4 AmbientColor;     // 环境/天空散射色(原 ShadowColor)
    float4 LightColor;       // 太阳色 * 强度

    float CloudDensity;
    float CloudCoverage;
    float CloudBottom;       // 云层底部海拔(米)
    float CloudTop;          // 云层顶部海拔(米)

    float2 CloudScale;       // 天气图 xz 平铺尺度
    float LightAbsorption;   // 消光系数
    float DarknessThreshold; // 提前退出阈值

    float3 LightDir;         // 太阳光行进方向(指向太阳 = -LightDir)
    int MaxSteps;            // 主步进次数

    float PhaseG;            // Henyey-Greenstein 前向散射
    float PhaseG2;           // 后向散射
    float PhaseBlend;        // 双叶混合
    int LightSteps;          // 向光步进次数

    float PowderScale;       // Beer-Powder 暗边强度
    float DetailScale;       // 细节侵蚀强度
    float PlanetRadius;      // 行星半径(米), 用于球壳大气
    float BaseNoiseScale;    // 基础形状噪声世界尺度
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

// GPU 整数位混合 hash (pcg3d, Jarzynski & Olano 2020).
// frac(p * 常数) 一类浮点 hash 喂连续整数像素坐标时容易形成短周期相关性;
// pcg3d 用整数位混合，避免把这种屏幕空间相关性带入密度积分。它不是当前
// 球面等高线伪影的根因，但仍比旧的浮点 hash 更适合作为像素/样本随机源。
uint3 Pcg3d(uint3 v)
{
    v = v * 1664525u + 1013904223u;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    v ^= v >> 16u;
    v.x += v.y * v.z;
    v.y += v.z * v.x;
    v.z += v.x * v.y;
    return v;
}
// 取 [0,1) 均匀随机数
float Hash13(uint3 v)
{
    return (float) (Pcg3d(v).x & 0x00FFFFFFu) / 16777216.0;
}

float CloudRemap(float v, float lo, float hi, float nlo, float nhi)
{
    return nlo + saturate((v - lo) / (hi - lo)) * (nhi - nlo);
}

static const float CLOUD_NOISE_TEXTURE_SIZE = 64.0;
static const float CLOUD_NOISE_MAX_MIP = 6.0;

// 根据样本 footprint 选择 3D 噪声 mip。采样器使用 MIN_MAG_MIP_LINEAR，
// 直接以 fractional LOD 做三线性过滤，避免逐 texel 坐标重映射产生轴对齐平台。
float SampleCloudNoise(float3 uv, float uvSpan)
{
    float lod = clamp(log2(max(uvSpan * CLOUD_NOISE_TEXTURE_SIZE, 1.0)),
                      0.0, CLOUD_NOISE_MAX_MIP);
    return CloudNoiseTex.SampleLevel(Samp_CloudNoiseTex, uv, lod).r;
}

// 远处一个 2500m 噪声周期仍覆盖多个像素时会完整重复，透视压缩后形成摩尔纹。
// 这里将 30~120km（按云层厚度缩放）逐渐映射到 mip0~mip6，直接在密度域
// 收敛到体均值，避免屏幕空间模糊跨云边产生 halo。
float FarNoiseUVSpan(float sampleDist, float thickness)
{
    float farFade = smoothstep(thickness * 12.0, thickness * 48.0, sampleDist);
    return exp2(farFade * CLOUD_NOISE_MAX_MIP) / CLOUD_NOISE_TEXTURE_SIZE;
}

// 天气图没有 mip，仅对 mip0 的低频数据做平滑坐标重建。
float2 SmoothTexCoord2D(float2 uv, float texSize)
{
    float2 c = uv * texSize - 0.5;
    float2 i = floor(c);
    float2 f = frac(c);
    f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    return (i + f + 0.5) / texSize;
}

// Henyey-Greenstein 相位函数
float HenyeyGreenstein(float cosTheta, float g)
{
    float g2 = g * g;
    float denom = 1.0 + g2 - 2.0 * g * cosTheta;
    return (1.0 - g2) / (4.0 * 3.1415926 * pow(max(denom, 1e-4), 1.5));
}

// UE5 SamplePhaseFunction: 双叶 HG 混合(前向 + 后向), 产生银边/前向散射
float DualLobePhase(float cosTheta, float g0, float g1, float blend)
{
    float p0 = HenyeyGreenstein(cosTheta, g0);
    float p1 = HenyeyGreenstein(cosTheta, g1);
    return p0 + blend * (p1 - p0);
}

// 稳定计算球面海拔, 即 length(p - center) - radius。直接这么写会在地球半径
// 量级上先把 length 量化到约 0.5m, 再减去两个约 6.36e6 的数; 逐样本 heightFrac
// 因而变成离散等高线, 俯视时会显影为以天底点为圆心的密集同心曲线。
// 将径向差拆成切平面高度 + 水平曲率项, 避免大数相减：
// sqrt((R+h)^2+xz^2)-R = h + xz^2/(sqrt((R+h)^2+xz^2)+(R+h))。
float StableSphericalAltitude(float3 p, float3 center, float radius)
{
    float vertical = p.y - (center.y + radius);
    float2 horizontal = p.xz - center.xz;
    float horizontalSq = dot(horizontal, horizontal);
    float radialY = radius + vertical;
    float radialLength = sqrt(radialY * radialY + horizontalSq);
    return vertical + horizontalSq / max(radialLength + radialY, 1e-6);
}

// 光线与以 center 为球心, 半径 r 的球相交, 返回近/远两个 t (米)。
// 行星半径达 ~6.4e6 米, 判别式与求根若写成大数相减都会在掠射角量化成台阶,
// 表现为俯视时绕天底点会聚的同心/编织摩尔纹, 必须三处都回避大数相减:
//   1. 判别式不能写 rr*rr - |perp|^2: 掠射时 |perp|->rr, 两个恒为 ~4e13 的数相减,
//      其 ULP(~4e6 m^2) 与 disc 真实值无关, disc->0 时 sqrt 被量化成米级台阶。
//      改用 disc = b*b - c: 掠射时 b*b 与 c 一起缩到 ~7.6e10, ULP 随之小两三个数量级,
//      抵消区精度不再塌陷。(缩放 S 只改指数不改尾数, 对相对精度无用, 故去掉。)
//   2. c = |oc|^2 - r^2 = (|oc| - r)(|oc| + r) = H*(H+2r), 其中 H 是海拔差,
//      用 StableSphericalAltitude 稳定求出, 全程不出现 4e13 - 4e13。
//   3. 求根用韦达定理: 先取 -b 与 sq 同号(加法、不抵消)的那一支 q, 另一根 = c / q。
float2 RaySphere(float3 ro, float3 rd, float3 center, float r)
{
    float3 oc = ro - center;
    float b = dot(oc, rd);            // = |oc|*cos(视线与径向夹角), rd 已归一化, 良态
    float H = StableSphericalAltitude(ro, center, r);   // |oc| - r, 米, 稳定
    float c = H * (H + 2.0 * r);                        // = |oc|^2 - r^2, 无大数相减

    // 判别式: 掠射(b^2≈c)时两操作数一起变小, 不再是 4e13 级的灾难抵消
    float disc = b * b - c;
    if (disc < 0.0)
        return float2(-1.0, -1.0);
    float sq = sqrt(disc);

    float q = (b > 0.0) ? (-b - sq) : (-b + sq);        // 不抵消的那一支
    float other = (abs(q) > 1e-4) ? (c / q) : q;
    return float2(min(q, other), max(q, other));
}

// 云类型驱动的垂直剖面: type 越大越接近积云(更饱满更高)
float CloudHeightGradient(float heightFrac, float cloudType)
{
    // 底部软过渡
    float lower = CloudRemap(heightFrac, 0.0, 0.1 + cloudType * 0.05, 0.0, 1.0);
    // 顶部软过渡(积云更高)
    float topEnd = lerp(0.35, 1.0, saturate(cloudType));
    float upper = CloudRemap(heightFrac, topEnd * 0.6, topEnd, 1.0, 0.0);
    return saturate(lower * upper);
}

// 噪声在竖直方向的平铺数. 竖直世界周期 = thickness / VERTICAL_TILES, 与水平的
// 1/BaseNoiseScale 完全不同, 做 Nyquist 判据时必须分开算(见 NoiseUVSpan)
static const float VERTICAL_TILES = 2.0;

// 一步位移在基础噪声 UV 空间里跨越的长度(单位: 噪声纹理的一个平铺周期).
// 用世界米做 Nyquist 判据会漏掉竖直方向: 竖直周期只有水平的 1/5, 而俯视/仰视时
// 每步的竖直位移最大, 于是只有俯仰视角会出摩尔纹, 横看时看不出来
float NoiseUVSpan(float3 dir, float stepLen)
{
    float thick = max(ShadingStruct.CloudTop - ShadingStruct.CloudBottom, 1.0);
    float3 rate = float3(dir.x * ShadingStruct.BaseNoiseScale,
                         dir.y * VERTICAL_TILES / thick,
                         dir.z * ShadingStruct.BaseNoiseScale);
    return length(rate) * stepLen;
}

// 每像素角射线导数(弧度/像素): 去掉沿光线分量后除 |viewVec|。
// 横向项与纵向(掠射)项共用这对导数, 避免重复计算也避免两处漂移。
void ComputeScreenRayDerivatives(float3 rayDir, float3 viewVec, out float3 dpx, out float3 dpy)
{
    float invLen = 1.0 / max(length(viewVec), 1e-6);
    // CornerRays 角序: 0=左上 1=右上 2=右下 3=左下(已核对 MeshDataProvider.cs).
    float3 dVx = (CornerRays[1].xyz - CornerRays[0].xyz) * ViewportSizeAndRcp.z;
    float3 dVy = (CornerRays[3].xyz - CornerRays[0].xyz) * ViewportSizeAndRcp.w;
    dpx = (dVx - dot(dVx, rayDir) * rayDir) * invLen;
    dpy = (dVy - dot(dVy, rayDir) * rayDir) * invLen;
}

// 横向 footprint: 相邻像素光线发散造成的横向采样间距, 映射到各向异性噪声 UV 速率.
// 返回值 * 采样绝对距离 = 该样本在噪声 UV 空间的横向 footprint.
float ScreenUVSpanPerDist(float3 dpx, float3 dpy)
{
    float thick = max(ShadingStruct.CloudTop - ShadingStruct.CloudBottom, 1.0);
    float3 rate = float3(ShadingStruct.BaseNoiseScale, VERTICAL_TILES / thick, ShadingStruct.BaseNoiseScale);
    return max(length(rate * dpx), length(rate * dpy));
}

// 纵向(沿视线) footprint: 掠射时相邻像素的球壳交点距离差很大, 样本位置沿视线
// 大幅滑移, 仅横向项会低估 2~7x 导致高频八度欠过滤 -> 地平线扇形摩尔纹.
// 隐函数定理得 ∂t/∂pixel = -t·(N·∂ₚ d)/(N·d); 分母 |N·d|=sin(仰角) 在掠射时→ 0.
// 但云壳曲率使光线弯离视线, 走不满整个厚度后 footprint 饱和, 故用 g_min=sqrt(2*thick/rho)
// 将分母封底(≈pitch 1.6°), 既防除零爆炸又随云层厚度自然封顶.
float LongitudinalNoiseSpan(float3 pos, float3 planetCenter, float3 rayDir,
                            float3 dpx, float3 dpy, float uvSpanPerMeter,
                            float sampleDist, float thickness)
{
    float3 dc = pos - planetCenter;
    float rho = length(dc);
    float3 N = dc / max(rho, 1.0);
    float av = max(abs(dot(N, dpx)), abs(dot(N, dpy)));   // 每像素仰角变化率
    float g = abs(dot(N, rayDir));                        // = sin(仰角)
    float gMin = sqrt(2.0 * thickness / max(rho, 1.0));   // 曲率/厚度封顶
    float gEff = max(g, gMin);
    return uvSpanPerMeter * (sampleDist * av / gEff);
}

// 采样某点云密度。noiseUVSpan 是当前样本在基础噪声 UV 空间的 footprint，
// 同时驱动 mip 预过滤和高频八度淡出。
float SampleCloudDensity(float3 p, float3 planetCenter, bool bDetail, float noiseUVSpan)
{
    float altitude = StableSphericalAltitude(p, planetCenter, ShadingStruct.PlanetRadius);
    float heightFrac = saturate((altitude - ShadingStruct.CloudBottom) /
                                max(ShadingStruct.CloudTop - ShadingStruct.CloudBottom, 1.0));
    if (heightFrac <= 0.0 || heightFrac >= 1.0)
        return 0.0;

    // 天气图: r=coverage g=density b=type
    float2 wuv = p.xz * ShadingStruct.CloudScale;
    float4 weather = WeatherTex.SampleLevel(Samp_WeatherTex, SmoothTexCoord2D(wuv, 512.0), 0);
    float coverage = saturate(weather.r * ShadingStruct.CloudCoverage);
    if (coverage <= 0.0)
        return 0.0;
    float cloudType = weather.b;

    float grad = CloudHeightGradient(heightFrac, cloudType);
    if (grad <= 0.0)
        return 0.0;

    // 基础形状噪声。竖直方向使用层内归一化高度，使细节尺度与云层厚度解耦。
    float3 baseUV = float3(p.x * ShadingStruct.BaseNoiseScale,
                          heightFrac * VERTICAL_TILES,
                          p.z * ShadingStruct.BaseNoiseScale);

    // 高频八度在接近一个周期的 footprint 时平滑淡出，权重同步归一化。
    float midWeight = 0.3 * (1.0 - smoothstep(0.25, 0.5, noiseUVSpan * 3.0));
    float base = (SampleCloudNoise(baseUV, noiseUVSpan) * 0.7 +
                  SampleCloudNoise(baseUV * 3.0, noiseUVSpan * 3.0) * midWeight) /
                 (0.7 + midWeight);

    // 用覆盖率裁剪基础噪声得到蓬松边缘，垂直剖面在 remap 后应用。
    float density = CloudRemap(base, 1.0 - coverage, 1.0, 0.0, 1.0);
    density = saturate(density) * grad;
    if (density <= 0.0)
        return 0.0;

    // 高频细节侵蚀仅用于主视线；向光步进可跳过以节省开销。
    float detailFade = 1.0 - smoothstep(0.25, 0.5, noiseUVSpan * 4.0);
    if (bDetail && detailFade > 0.0)
    {
        float detail = SampleCloudNoise(baseUV * 4.0, noiseUVSpan * 4.0);
        float erode = detail * ShadingStruct.DetailScale *
                      (1.0 - heightFrac * 0.5) * detailFade;
        density = saturate(CloudRemap(density, erode, 1.0, 0.0, 1.0));
    }

    return density * ShadingStruct.CloudDensity * (0.5 + weather.g);
}

// 向太阳步进, 返回向光光学厚度.
// jitter 是逐像素的起点偏移: 固定起点的 6 步采样会在屏幕上给出沿太阳方向的硬条带
// (就是那些像下雨一样的竖纹), 抖开后退化成背景噪声
float LightMarch(float3 p, float3 sunDir, float3 planetCenter, float jitter,
                 float minNoiseUVSpan)
{
    // 低于 16 步时单步会把 2500m 云层压到 8³/4³ 的粗 mip，
    // 自阴影会显露成几百米的大块。三线性单 tap 已回收原双 tap 的开销。
    int steps = max(ShadingStruct.LightSteps, 16);
    // 向光步进的总长度要能穿过整个云层，否则自遮挡不够，云没有明暗对比。
    float stepLen = (ShadingStruct.CloudTop - ShadingStruct.CloudBottom) / (float) steps;
    float3 s = sunDir * stepLen;
    // 自阴影不能重新引入主视线已过滤掉的高频，故保留主样本 footprint 作为下限。
    float uvSpan = max(NoiseUVSpan(sunDir, stepLen), minNoiseUVSpan);
    float opticalDepth = 0.0;

    p += s * jitter;

    [loop]
    for (int i = 0; i < steps; i++)
    {
        p += s;
        float d = SampleCloudDensity(p, planetCenter, false, uvSpan);
        opticalDepth += d * stepLen;
    }
    return opticalDepth;
}

// 多散射八度近似(UE5: MsScatt/MsExtin/MsPhase 三因子逐八度减半)
float3 SunTransmittance(float lightOpticalDepth, float cosTheta)
{
    const int MSCOUNT = 3;
    // HenyeyGreenstein 带了 1/4π 归一化, 各向同性时只有 0.08.
    // LightColor 是 LDR 颜色(0~1), 不把 4π 补回来的话太阳项会小到看不见, 云只剩环境光的平色
    const float PHASE_NORM = 12.5663706;
    // 三个八度的 scatterFactor 是 1 + 0.5 + 0.25 = 1.75, 不归一化的话整体会被放大 1.75 倍,
    // 叠上环境光直接顶到 LDR 上限变成白片
    const float MS_NORM = 1.0 / 1.75;

    float scatterFactor = 1.0;
    float extinFactor = 1.0;
    float phaseFactor = 1.0;

    float3 energy = float3(0.0, 0.0, 0.0);
    [unroll]
    for (int ms = 0; ms < MSCOUNT; ms++)
    {
        float phase = DualLobePhase(cosTheta * phaseFactor, ShadingStruct.PhaseG,
                                    ShadingStruct.PhaseG2, ShadingStruct.PhaseBlend);
        float beer = exp(-lightOpticalDepth * ShadingStruct.LightAbsorption * extinFactor);
        energy += scatterFactor * beer * phase * PHASE_NORM;

        scatterFactor *= 0.5;
        extinFactor *= 0.5;
        phaseFactor *= 0.5;
    }
    return energy * MS_NORM * ShadingStruct.LightColor.rgb;
}

// 主光线步进
float4 RayMarchClouds(float3 rayOrigin, float3 rayDir, float sceneDist, uint2 pixelSeed, float screenUVPerDist, float3 dpx, float3 dpy)
{
    float3 planetCenter = float3(0.0, -ShadingStruct.PlanetRadius, 0.0);
    float innerR = ShadingStruct.PlanetRadius + ShadingStruct.CloudBottom;
    float outerR = ShadingStruct.PlanetRadius + ShadingStruct.CloudTop;

    float altitude = StableSphericalAltitude(rayOrigin, planetCenter, ShadingStruct.PlanetRadius);

    float2 inner = RaySphere(rayOrigin, rayDir, planetCenter, innerR);
    float2 outer = RaySphere(rayOrigin, rayDir, planetCenter, outerR);
    // 行星本体: 当它挡在云层前面时必须把步进范围截断
    float2 ground = RaySphere(rayOrigin, rayDir, planetCenter, ShadingStruct.PlanetRadius);

    // 稳定的云层壳穿越长度. 直接 tEnd - tStart(= inner 根 - outer 根)会把两个各含一次
    // sqrt(b^2-c)(b^2~4e13)的球壳根相减, 每壳的 sqrt 独立量化成亚米级台阶, 俯视时两套
    // 台阶互拍, 显影为绕天底会聚的编织摩尔纹。
    // 有理化消除相减: |near_i-near_o| = |far_i-far_o| = |sqOuter-sqInner|
    //   = (outerR^2-innerR^2)/(sqOuter+sqInner)。分子 = thickness*(innerR+outerR) 是精确闭式
    //   (无大数相减), 分母是两 sqrt 之和(纯加法、无抵消), 整体一次除法平滑, 台阶互拍消失。
    float3 oc = rayOrigin - planetCenter;
    float b = dot(oc, rayDir);
    float hInner = StableSphericalAltitude(rayOrigin, planetCenter, innerR);
    float hOuter = StableSphericalAltitude(rayOrigin, planetCenter, outerR);
    float sqInner = sqrt(max(b * b - hInner * (hInner + 2.0 * innerR), 0.0));
    float sqOuter = sqrt(max(b * b - hOuter * (hOuter + 2.0 * outerR), 0.0));
    float shellSpan = (ShadingStruct.CloudTop - ShadingStruct.CloudBottom) * (innerR + outerR);
    float slabLen = shellSpan / max(sqOuter + sqInner, 1e-3);

    float tStart, tEnd;
    if (altitude < ShadingStruct.CloudBottom)
    {
        // 相机在云层下方(常见情况): 从穿出内壳到穿出外壳
        if (inner.y < 0.0)
            return float4(0, 0, 0, 0);
        tStart = inner.y;
        tEnd = tStart + slabLen;   // 稳定长度替代 outer.y - inner.y
    }
    else if (altitude > ShadingStruct.CloudTop)
    {
        // 相机在云层上方往下看
        if (outer.x < 0.0)
            return float4(0, 0, 0, 0);
        tStart = outer.x;
        // 命中内壳时用稳定长度替代 inner.x - outer.x; 只擦到外壳时退回外壳远交点
        tEnd = (inner.x > 0.0) ? (tStart + slabLen) : outer.y;
    }
    else
    {
        // 相机就在云层里: 终点是单个球壳根, 不涉及两根相减, 不产生编织
        tStart = 0.0;
        tEnd = (inner.x > 0.0) ? inner.x : outer.y;
    }

    tStart = max(tStart, 0.0);
    // 地表遮挡: 朝下的光线先打到行星表面, 它之后的云壳交点在行星另一侧(上万公里外),
    // 不截断的话就会去那个 float 已经无法区分相邻采样点的尺度上步进, 表现就是俯视时满屏椒盐噪点
    if (ground.x > 0.0)
        tEnd = min(tEnd, ground.x);
    // 场景深度剔除: 被实体几何遮挡的部分不步进
    tEnd = min(tEnd, sceneDist);
    if (tEnd <= tStart)
        return float4(0, 0, 0, 0);

    int steps = max(ShadingStruct.MaxSteps, 1);
    float thickness = max(ShadingStruct.CloudTop - ShadingStruct.CloudBottom, 1.0);

    // 追踪距离上限: 地平线方向云层斜距能到上百公里, 再远的部分对画面贡献很小
    float maxTraceDist = thickness * 80.0;
    float fullLength = tEnd - tStart;
    float rayLength = min(fullLength, maxTraceDist);
    // 只有因为距离上限被截断时才淡出尾段, 否则会把正常的云层顶部也擦掉
    float bTruncated = (fullLength > rayLength * 1.01) ? 1.0 : 0.0;

    // 步长随距离线性增长(几何增长的解析闭式): 均分 rayLength 时近地平线方向单步会
    // 跨过整个云层, 每像素只命中一两个采样点, 云碎成噪点; 增长式步进保证近处细密。
    // 关键: 步长必须是 t 的纯函数 stepAt(t) = step0 + t*(growth-1), 绝不能逐迭代累乘
    // (coarseStep*=growth)。累乘让"同一物理深度的步长"取决于状态机的行进历史, 各像素的
    // 粗格边界随天顶角连续漂移又高度相干, 被密度梯度显影成绕天底会聚的编织摩尔纹。
    const float growth = 1.015;
    float sumFactor = (pow(growth, (float) steps) - 1.0) / (growth - 1.0);
    float step0 = clamp(rayLength / sumFactor, thickness * 0.01, thickness * 0.08);
    float growthSlope = growth - 1.0;
    // 命中云后把当前粗格就地细分的段数: 单段光学厚度必须远小于 1, 否则进入面被切成硬边
    const int SUB = 4;

    float t = 0.0;
    float3 sunDir = -normalize(ShadingStruct.LightDir);
    float cosTheta = dot(rayDir, sunDir);
    float3 scatteredLight = float3(0, 0, 0);
    float transmittance = 1.0;
    // 单位步长在噪声 UV 空间的跨度. rayDir 整条光线不变, 循环内只需乘当前步长
    float uvSpanPerMeter = NoiseUVSpan(rayDir, 1.0);

    // 单级确定性步进: 粗格边界只锚在 t=0、只由 t 决定(对所有像素同一套规则),
    // 不再有"入云点漂移"锚定的细格相位, 也没有 bInCloud/emptyRun 状态机与 growth 累乘。
    // 空格整格跳过、不积分不采细节; 命中就把当前这一格就地细分成 SUB 段积分。
    // 相邻像素的段划分完全一致, 无法自组织成相干相位场 -> 编织摩尔纹被切断。
    [loop]
    for (int i = 0; i < steps && t < rayLength; i++)
    {
        float coarseLen = step0 + t * growthSlope;      // 粗格长度: t 的纯函数
        float cellLen = min(coarseLen, rayLength - t);
        // 尾段淡出按格首 t 取值, 与像素无关, 让距离上限处的截断变成渐变
        float tailFade = lerp(1.0, 1.0 - smoothstep(0.75, 1.0, t / rayLength), bTruncated);
        // 粗检测样点加逐像素抖动: 以前固定在格子正中心(0.5), 所有像素共享
        // 同一判定边界 -> "跳过还是积分" 的翻转面是世界锁定的等距壳, 俯视显现
        // 为绕天底会聚的同心弧. 给探针加逐像素偏移后, 翻转面在相邻像素间随机错开,
        // 相干的弧退化为不可见的背景噪声(不需要 TAA 来收敛, 因为探针只是开关,
        // 真正积分的 SUB 段样本仍各自独立抖动)
        float probeOff = Hash13(uint3((uint2) pixelSeed, (uint) i + 0x7531u));
        float probeDist = tStart + t + probeOff * cellLen;
        float3 probePos = rayOrigin + rayDir * probeDist;
        // 探针只负责保守判空，使用实际细分积分尺度选择 LOD；若按整个粗格
        // 选择 mip，硬门会把粗 mip 的块状轮廓放大成整格跳过。
        float probeSpan = max(uvSpanPerMeter * (cellLen / (float) SUB),
                              screenUVPerDist * probeDist);
        probeSpan = max(probeSpan, FarNoiseUVSpan(probeDist, thickness));
        probeSpan = max(probeSpan, LongitudinalNoiseSpan(
            probePos, planetCenter, rayDir, dpx, dpy, uvSpanPerMeter, probeDist, thickness));
        float probeDens = SampleCloudDensity(
            probePos, planetCenter, false, probeSpan) * tailFade;
        if (probeDens <= 0.001)
        {
            t += cellLen;   // 空格: 整格跳过, 不积分
            continue;
        }

        // 命中：就地把当前粗格细分成 SUB 段积分。主样本必须使用真实 subLen，
        // 否则以 coarseLen 估算 footprint 会把中远距离细节过度滤除。
        float subLen = cellLen / (float) SUB;
        [loop]
        for (int s = 0; s < SUB; s++)
        {
            // 逐样本抖动只为打散段内的常量近似，是白噪声、只产生颗粒不产生规则网。
            float sampleJitter = Hash13(uint3((uint2) pixelSeed, (uint) (i * SUB + s)));
            float ts = t + ((float) s + sampleJitter) * subLen;
            float sampleDist = tStart + ts;
            float3 pos = rayOrigin + rayDir * sampleDist;
            float sampleSpan = max(uvSpanPerMeter * subLen,
                                   screenUVPerDist * sampleDist);
            sampleSpan = max(sampleSpan, FarNoiseUVSpan(sampleDist, thickness));
            sampleSpan = max(sampleSpan, LongitudinalNoiseSpan(
                pos, planetCenter, rayDir, dpx, dpy, uvSpanPerMeter, sampleDist, thickness));
            float density = SampleCloudDensity(
                pos, planetCenter, true, sampleSpan) * tailFade;
            if (density > 0.001)
            {
                // 向光步进抖动与主步进解相关，否则误差同相叠加会结成块斑。
                float lightJitter = Hash13(uint3(
                    (uint2) pixelSeed, (uint) (i * SUB + s) + 0x9E37u));
                float lightOD = LightMarch(
                    pos, sunDir, planetCenter, lightJitter, sampleSpan);

                // Beer-Powder: 参考长度用密度本身，不能用步长。
                float powder = 1.0 - exp(-density * 4.0);
                powder = lerp(1.0, powder, saturate(ShadingStruct.PowderScale));

                // SunTransmittance 内部已逐八度乘过 phase, 这里不能再乘
                float3 sunLum = SunTransmittance(lightOD, cosTheta) * powder;
                float3 ambient = ShadingStruct.AmbientColor.rgb;

                // 能量守恒解析积分(Frostbite/Hillaire). luminance 乘 extinction 而非 density
                float extinction = max(density * ShadingStruct.LightAbsorption, 1e-5);
                float3 luminance = (sunLum * ShadingStruct.CloudColor.rgb + ambient) * extinction;
                float stepTrans = exp(-extinction * subLen);
                float3 integScatt = (luminance - luminance * stepTrans) / extinction;

                scatteredLight += transmittance * integScatt;
                transmittance *= stepTrans;

                if (transmittance < ShadingStruct.DarknessThreshold)
                    break;
            }
        }
        if (transmittance < ShadingStruct.DarknessThreshold)
            break;
        t += cellLen;
    }

    return float4(scatteredLight, 1.0 - transmittance);
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

    // 场景深度 -> 到不透明几何的距离; 天空像素给一个很大的距离
    float depth = DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
    float linearZ = LinearFromDepth(depth);
    float sceneDist = 1.0e12;
    if (linearZ < ZFar * 0.999)
    {
        float3 worldPos = GetWorldPositionFromDepthValue(uv, depth).xyz;
        sceneDist = length(worldPos - CameraPosition);
    }

    float3 rayOrigin = CameraPosition;
    float3 viewVec = input.Get_ScreenViewVector();
    float3 rayDir = normalize(viewVec);
    // 每像素角射线导数, 供横向与纵向(掠射) footprint 共用
    float3 dpx, dpy;
    ComputeScreenRayDerivatives(rayDir, viewVec, dpx, dpy);
    // 屏幕空间每像素噪声 UV footprint 系数(见 ScreenUVSpanPerDist), 传入积分做 Nyquist 淡出
    float screenUVPerDist = ScreenUVSpanPerDist(dpx, dpy);

    // 整数像素坐标作为 per-pixel hash 种子；raymarch 内再加入样本序号。
    uint2 pixelSeed = (uint2) floor(uv * ViewportSizeAndRcp.xy);

    float4 cloud = RayMarchClouds(rayOrigin, rayDir, sceneDist, pixelSeed, screenUVPerDist, dpx, dpy);

    // 预乘 alpha 混合
    float3 result = sceneColor.rgb * (1.0 - cloud.a) + cloud.rgb;

    output.RT0 = float4(result, 1.0);
    return output;
}

#endif//__FX_VOLUMETRICCLOUD_H__
