#ifndef	_FONT_SDF_H_
#define _FONT_SDF_H_

struct TtFontSDF
{
    //Texture2D FontTexture;
    //SamplerState SampFontTexture;
    bool      UseLight;
    float     LowThreshold;
    float     HighThreshold;
    float     SmoothValue;
    float3    BaseColor;
    float3    BorderColor;
    float3    VertexToLightECS;
    void SetDefault()
    {
        UseLight = false;
        LowThreshold = 0;
        HighThreshold = 0.8;
        SmoothValue = 0.5;
        BaseColor = float3(1, 1, 1);
        BorderColor = float3(1, 0, 0);
        VertexToLightECS = float3(1, 0, 0);
    }
    float4 GetFontSDF(int effect, float alpha)
    {
        float4 color;
        //float alpha = FontTexture.Sample(SampFontTexture, uv);
        if (effect == 0) 
        {
            color.xyz = BaseColor.xyz;
            color.a = alpha;
        }
        else if (effect == 1)
        {
            // Softened edge.
            color.rgb = BaseColor;
            color.a = smoothstep(0.5 - SmoothValue,
                0.5 + SmoothValue,
                alpha);
        }
        else if (effect == 2)
        {
            // Sharp edge.
            if (alpha >= LowThreshold)
            {
                color.rgb = BaseColor;
                color.a = 1.0;
            }
            else
            {
                color.rgb = BaseColor;
                color.a = 0.0;
                /*color.a = smoothstep(0,
                    LowThreshold,
                    alpha / LowThreshold);*/
            }
        }
        else if (effect == 3)
        {
            // Sharp edge with outer glow.
            if (alpha >= LowThreshold)
            {
                color.rgb = BaseColor;
                color.a = 1.0;
            }
            else
            {
                color.rgb = BorderColor;
                color.a = alpha;
            }
        }
        else if (effect == 4) 
        {
            // With border.
            if (alpha >= LowThreshold && alpha <= HighThreshold) 
            {
                color.rgb = BorderColor;
                color.a = 1.0;
            }
            else if (alpha >= HighThreshold)
            {
                color.rgb = BaseColor;
                color.a = 1.0;
            }
            else
            {
                color.rgb = BaseColor;
                color.a = 0.0;
            }
        }
        else if (effect == 5) 
        {
            color.rgb = BaseColor;
            // Softened edge.
            if (alpha < 0.5)
            {
                color.a = smoothstep(0.5 - SmoothValue,
                    0.5 + SmoothValue,
                    alpha);
            }
            else
            {
                color.a = 0.5 - smoothstep(0.5 - SmoothValue,
                    0.5 + SmoothValue,
                    0.75 * alpha);
            }
        }
        else
        {
            // Rect box for debugging
            color.rgb = BaseColor;
            color.a = 1.0;
        }

        if (UseLight)
        {
            float dist = distance(VertexToLightECS, float3(0.0, 0.0, 0.0));
            color.rgb = color.rgb / sqrt(dist);
        }
        return color;
    }
};

float4 GetFontSDF(int effect, float3 baseColor, float3 borderColor, float alpha, float lowThreshold, float highThreshold, float smoothValue)
{
    TtFontSDF font;
    font.SetDefault();
    font.BaseColor = baseColor;
    font.BorderColor = borderColor;
    font.LowThreshold = lowThreshold;
    font.HighThreshold = highThreshold;
    font.SmoothValue = smoothValue;
    return font.GetFontSDF(effect, alpha);
}

#endif//_FONT_SDF_H_