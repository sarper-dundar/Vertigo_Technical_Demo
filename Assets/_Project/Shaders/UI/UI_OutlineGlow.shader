Shader "VertigoDemo/UI_OutlineGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1, 0.8, 0.2, 1)
        _GlowWidth ("Glow Width", Range(0, 20)) = 4
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2
        _GlowSoftness ("Glow Softness", Range(0.01, 2)) = 0.5
        _SpritePadding ("Sprite Padding", Range(0, 0.4)) = 0.1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowWidth;
            float _GlowIntensity;
            float _GlowSoftness;
            float _SpritePadding;
            float4 _ClipRect;
            fixed4 _TextureSampleAdd;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.texcoord;

                // remap UVs: the sprite is rendered smaller (inset by padding)
                // leaving room around the edges for the glow to render
                float pad = _SpritePadding;
                float2 spriteUV = (uv - pad) / (1.0 - 2.0 * pad);

                // sample the sprite only if we're inside the sprite area
                half4 mainTex = half4(0, 0, 0, 0);
                half mainAlpha = 0;
                if (spriteUV.x >= 0 && spriteUV.x <= 1 && spriteUV.y >= 0 && spriteUV.y <= 1)
                {
                    mainTex = tex2D(_MainTex, spriteUV) + _TextureSampleAdd;
                    mainAlpha = mainTex.a;
                }

                // sample alpha at offsets to build glow mask
                float glowAlpha = 0;
                float totalWeight = 0;
                float2 texelSize = _MainTex_TexelSize.xy * _GlowWidth;
                // scale texel offsets to work in the remapped UV space
                float2 glowOffset = texelSize / (1.0 - 2.0 * pad);

                // 4 rings x 12 taps, closer rings weighted heavier for smooth falloff
                const int TAPS = 12;
                const int RINGS = 4;
                for (int r = 0; r < RINGS; r++)
                {
                    float ringDist = (float)(r + 1) / RINGS;
                    float ringWeight = 1.0 - ringDist * 0.5;
                    float angleOffset = (float)r * 0.26; // stagger rings to hide banding

                    for (int t = 0; t < TAPS; t++)
                    {
                        float angle = (float)t / TAPS * 6.28318 + angleOffset;
                        float2 sampleUV = spriteUV + float2(cos(angle), sin(angle)) * glowOffset * ringDist;
                        if (sampleUV.x >= 0 && sampleUV.x <= 1 && sampleUV.y >= 0 && sampleUV.y <= 1)
                        {
                            half sampleA = (tex2D(_MainTex, sampleUV) + _TextureSampleAdd).a;
                            glowAlpha += sampleA * ringWeight;
                        }
                        totalWeight += ringWeight;
                    }
                }

                glowAlpha /= totalWeight;

                float edge = saturate(glowAlpha - mainAlpha);
                edge = pow(edge, _GlowSoftness);
                edge *= _GlowIntensity;

                half4 glowResult = half4(_GlowColor.rgb * _GlowColor.a * edge, edge * _GlowColor.a);

                half4 color;
                color.rgb = mainTex.rgb * mainAlpha * i.color.rgb + glowResult.rgb * (1.0 - mainAlpha);
                color.a = saturate(mainAlpha * i.color.a + glowResult.a);

                color.rgb *= color.a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
