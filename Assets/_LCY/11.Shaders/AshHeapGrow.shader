Shader "Custom/AshHeapGrow_EventControlled"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _GrowSpeed("Grow Speed", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _GrowSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // 랜덤 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
            }

            // 개선된 FBM
            float fbm(float2 st)
            {
                float value = 0.0;
                float amplitude = 0.5;
                for (int i = 0; i < 6; i++)
                {
                    value += amplitude * rand(st);
                    st *= 2.5;
                    amplitude *= 0.4;
                }
                return value;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 ashColor = tex2D(_MainTex, i.uv).rgb;

                float3 mask = step(0.5, ashColor);
                float intensity = (mask.r + mask.g + mask.b) / 3.0;

                float grow = fbm(i.uv * _GrowSpeed + _Time.x) * intensity;

                float3 finalColor = ashColor * grow * _Color.rgb;
                float alpha = (grow > 0.01) ? 1.0 : 0.0;

                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }

    FallBack "Transparent/VertexLit"
}
