Shader "Unlit/ClipSDF"
{
    Properties
    {
        _SDF ("SDF", 2D) = "white" {}
        _SDF_Rev ("SDF Rev", 2D) = "white" {}
        _Clip ("Clip", Range(-1.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _SDF;
            sampler2D _SDF_Rev;
            float _Clip;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float r = tex2D(_SDF, i.uv).r;
                float g = tex2D(_SDF_Rev, i.uv).g;
                r = lerp(0, 1, step(r, pow(_Clip, 3)));
                g = lerp(0, 1, step(g, pow(-_Clip, 10)));
                float t = lerp(1 - g, r, step(0, _Clip));
                fixed4 col = fixed4(t, t, t, 1);
                return col;
            }
            ENDCG
        }
    }
}
