#include "../Inc/GlobalDefine.cginc"

VK_BIND(0) cbuffer ProjectionMatrixBuffer DX_BIND_B(0)
{
    float4x4 ProjectionMatrix;
    int4 ColorMask;
    int IsNormalMap;
};

struct VS_INPUT
{
    VK_LOCATION(0) float2 pos : POSITION;
    VK_LOCATION(2) float2 uv  : TEXCOORD0;
    VK_LOCATION(3) float4 col : COLOR0;
};

struct PS_INPUT
{
    VK_LOCATION(0) float4 pos : SV_POSITION;
    VK_LOCATION(2) float4 col : COLOR0;
    VK_LOCATION(3) float2 uv  : TEXCOORD0;
};

VK_BIND(0) Texture2D FontTexture DX_AUTOBIND;
VK_BIND(1) sampler Samp_FontTexture DX_AUTOBIND;
/**Meta Begin:(VS_Main)
HLSL=none
Meta End:(VS_Main)**/
PS_INPUT VS_Main(VS_INPUT input)
{
    PS_INPUT output;
    output.pos = mul(ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
    output.col = input.col;
    output.uv = input.uv;
    
    return output;
}

/**Meta Begin:(PS_Main)
HLSL=none
Meta End:(PS_Main)**/
float4 PS_Main(PS_INPUT input) : SV_Target
{
    float4 finalColor = FontTexture.Sample(Samp_FontTexture, input.uv);
    if (IsNormalMap == 1)
    {
        float3 N;
        N.xy = finalColor.xy * 2.0h - 1.0h;
        N.z = sqrt(1.0h - dot(N.xy, N.xy));
        finalColor.b = N.z;
    }

    int ExtractChannelCount = ColorMask.r + ColorMask.g + ColorMask.b + ColorMask.a;
    if(ExtractChannelCount == 1)
    {
        finalColor.rgb = finalColor.r * ColorMask.r + finalColor.g * ColorMask.g + finalColor.b * ColorMask.b + finalColor.a * ColorMask.a;
        finalColor.a = 1;
    }
    else
    {
        finalColor.r = finalColor.r * ColorMask.r;
        finalColor.g = finalColor.g * ColorMask.g;
        finalColor.b = finalColor.b * ColorMask.b;
        if(ColorMask.a == 1)
            finalColor.a = finalColor.a * ColorMask.a;
        else
            finalColor.a = 1;
    }
    return finalColor;
}

///