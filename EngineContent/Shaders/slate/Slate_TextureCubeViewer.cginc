#include "../Inc/GlobalDefine.cginc"

VK_BIND(0) cbuffer ProjectionMatrixBuffer DX_BIND_B(0)
{
    float4x4 ProjectionMatrix;
    int4 ColorMask;
    int IsNormalMap;
};

struct VS_INPUT_SLATE
{
    VK_LOCATION(0) float2 pos : POSITION;
    VK_LOCATION(2) float2 uv  : TEXCOORD0;
    VK_LOCATION(3) float4 col : COLOR0;
};

struct PS_INPUT_SLATE
{
    VK_LOCATION(0) float4 pos : SV_POSITION;
    VK_LOCATION(2) float4 col : COLOR0;
    VK_LOCATION(3) float2 uv  : TEXCOORD0;
};

VK_BIND(0) TextureCube FontTexture DX_AUTOBIND;
VK_BIND(1) sampler Samp_FontTexture DX_AUTOBIND;
/**Meta Begin:(VS_Main)
HLSL=none
Meta End:(VS_Main)**/
PS_INPUT_SLATE VS_Main(VS_INPUT_SLATE input)
{
    PS_INPUT_SLATE output;
    output.pos = mul(ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
    output.col = input.col;
    output.uv = input.uv;
    
    return output;
}

bool FillColor(TextureCube tex, sampler samp, float2 inputUV, out float4 finalColor)
{
    float faceWidth = 1.f / 4.f;
    float faceHeight = 1.f / 3.f;
    float2 faceSize = float2(faceWidth, faceHeight);

    // face0
    float2 faceUV0Start = float2(faceWidth * 2, faceHeight);
    float2 faceUV0End = faceUV0Start + faceSize;
    if(inputUV.x > faceUV0Start.x && inputUV.x < faceUV0End.x && inputUV.y > faceUV0Start.y && inputUV.y < faceUV0End.y)
    {
        float2 uv = (inputUV - faceUV0Start) / faceSize;
        finalColor = tex.Sample(samp, float3(1, 1-uv.y*2, 1-uv.x*2));
        return true;
    }
    // face1
    float2 faceUV1Start = float2(faceWidth * 0, faceHeight);
    float2 faceUV1End = faceUV1Start + faceSize;
    if(inputUV.x > faceUV1Start.x && inputUV.x < faceUV1End.x && inputUV.y > faceUV1Start.y && inputUV.y < faceUV1End.y)
    {
        float2 uv = (inputUV - faceUV1Start) / faceSize;
        finalColor = tex.Sample(samp, float3(-1, 1-uv.y*2, uv.x*2-1));
        return true;
    }
    // face4
    float2 faceUV4Start = float2(faceWidth * 1, faceHeight);
    float2 faceUV4End = faceUV4Start + faceSize;
    if(inputUV.x > faceUV4Start.x && inputUV.x < faceUV4End.x && inputUV.y > faceUV4Start.y && inputUV.y < faceUV4End.y)
    {
        float2 uv = (inputUV - faceUV4Start) / faceSize;
        finalColor = tex.Sample(samp, float3(uv.x*2-1, 1-uv.y*2, 1));
        return true;
    }
    // face5
    float2 faceUV5Start = float2(faceWidth * 3, faceHeight);
    float2 faceUV5End = faceUV5Start + faceSize;
    if(inputUV.x > faceUV5Start.x && inputUV.x < faceUV5End.x && inputUV.y > faceUV5Start.y && inputUV.y < faceUV5End.y)
    {
        float2 uv = (inputUV - faceUV5Start) / faceSize;
        finalColor = tex.Sample(samp, float3(1-uv.x*2, 1-uv.y*2, -1));
        return true;
    }
    // face2
    float2 faceUV2Start = float2(faceWidth * 1, faceHeight * 0);
    float2 faceUV2End = faceUV2Start + faceSize;
    if(inputUV.x > faceUV2Start.x && inputUV.x < faceUV2End.x && inputUV.y > faceUV2Start.y && inputUV.y < faceUV2End.y)
    {
        float2 uv = (inputUV - faceUV2Start) / faceSize;
        finalColor = tex.Sample(samp, float3(uv.x*2-1, 1, uv.y*2-1));
        return true;
    }
    // face3
    float2 faceUV3Start = float2(faceWidth * 1, faceHeight * 2);
    float2 faceUV3End = faceUV3Start + faceSize;
    if(inputUV.x > faceUV3Start.x && inputUV.x < faceUV3End.x && inputUV.y > faceUV3Start.y && inputUV.y < faceUV3End.y)
    {
        float2 uv = (inputUV - faceUV3Start) / faceSize;
        finalColor = tex.Sample(samp, float3(uv.x*2-1, -1, 1-uv.y*2));
        return true;
    }
         
    finalColor = 0;
    return false;
}

/**Meta Begin:(PS_Main)
HLSL=none
Meta End:(PS_Main)**/
float4 PS_Main(PS_INPUT_SLATE input) : SV_Target
{
    float4 finalColor = 0;
    FillColor(FontTexture, Samp_FontTexture, input.uv, finalColor);           
    // float4 finalColor = FontTexture.Sample(Samp_FontTexture, float3(1, 1-input.uv.y*2, 1-input.uv.x*2));

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