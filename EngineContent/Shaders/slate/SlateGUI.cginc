#include "../Inc/GlobalDefine.cginc"

VK_BIND(0) cbuffer ProjectionMatrixBuffer DX_BIND_B(0)
{
    float4x4 ProjectionMatrix;
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
HLSL=2021
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
HLSL=2021
Meta End:(PS_Main)**/
float4 PS_Main(PS_INPUT input) : SV_Target
{
    if ((int)(input.col.a * 255.0f) == 1)
    {
        float4 out_col = FontTexture.Sample(Samp_FontTexture, input.uv);
        out_col.rgb = out_col.rgb * input.col.rgb;
        out_col.a = 1.0f;
        return out_col;
    }
    else
    {
        float4 out_col = input.col * FontTexture.Sample(Samp_FontTexture, input.uv);
        return out_col;
    }    
}

///