Shader "Project1026/UI/Stage Select Sky"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [NoScaleOffset] _SkyMask ("Sky Motion Mask", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Motion ("Horizontal Travel (pixels)", Range(0,32)) = 26
        _Speed ("Motion Speed", Range(0,2)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _SkyMask;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _Motion;
            float _Speed;

            struct Input
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            Varyings Vert(Input input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.position);
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                UNITY_BRANCH
                if (input.uv.y < 0.66)
                    return tex2D(_MainTex, input.uv) * input.color;

                float mask = tex2D(_SkyMask, input.uv).r;
                UNITY_BRANCH
                if (mask <= 0.001)
                    return tex2D(_MainTex, input.uv) * input.color;

                float time = _Time.y * _Speed;
                float2 sky = input.uv;
                float wind = sin(sky.x * 7.3 + sky.y * 5.6 - time * 0.102);
                float gust = sin(sky.x * 14.8 - sky.y * 9.2 - time * 0.157);
                float height = smoothstep(0.67, 1.0, sky.y);
                float offset = (0.78 * wind + 0.22 * gust) * lerp(0.58, 1.0, height);
                float sampleX = sky.x * _MainTex_TexelSize.z - 0.5 + _Motion * mask * offset;

                // The source sprite uses Point sampling. Blend adjacent horizontal texels in the sky
                // so that slow motion can move by a fraction of a pixel at a time.
                float left = (floor(sampleX) + 0.5) * _MainTex_TexelSize.x;
                float right = left + _MainTex_TexelSize.x;
                fixed4 a = tex2D(_MainTex, float2(left, sky.y));
                fixed4 b = tex2D(_MainTex, float2(right, sky.y));
                return lerp(a, b, frac(sampleX)) * input.color;
            }
            ENDCG
        }
    }
}
