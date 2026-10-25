
Shader "Sachiko/Glitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Intensity ("Glitch Intensity", Range(0,2)) = 1
        _Progress ("Glitch Progress", Range(0,1)) = 0
        _Fade ("Effect Fade", Range(0,1)) = 1
        _RGBSplit ("RGB Separation", Range(0,0.1)) = 0.025
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.35
        _BlockStrength ("Digital Block Strength", Range(0,1)) = 0.7
        _NoiseStrength ("Static Strength", Range(0,1)) = 0.25
        _Speed ("Glitch Speed", Float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use UI Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Intensity;
            float _Progress;
            float _Fade;
            float _RGBSplit;
            float _ScanlineStrength;
            float _BlockStrength;
            float _NoiseStrength;
            float _Speed;
            float4 _ClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPosition = v.vertex;
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;

                float active = saturate(_Progress) * _Intensity;

                float row = floor(uv.y * 90.0);
                float rowNoise = hash21(float2(row, floor(time * 18.0)));

                float tear = step(0.82, rowNoise)
                    * (rowNoise - 0.72) * 0.22 * active;

                float blockRow = floor(uv.y * 34.0);
                float blockCol = floor(uv.x * 18.0);
                float blockNoise = hash21(
                    float2(blockCol + floor(time * 12.0), blockRow)
                );

                float blockMask = step(0.84, blockNoise)
                    * _BlockStrength * active;

                float2 shiftedUV = uv;
                shiftedUV.x += tear;
                shiftedUV.x += sin(uv.y * 180.0 + time * 35.0)
                    * 0.003 * active;

                float scan = 0.5 + 0.5 * sin(uv.y * 950.0);
                float scanMask = 1.0 - scan * _ScanlineStrength * active;

                float noise = hash21(
                    floor(uv * float2(420.0, 260.0))
                    + floor(time * 30.0)
                );

                float staticMask = step(1.0 - _NoiseStrength * active, noise);

                float red = tex2D(
                    _MainTex,
                    shiftedUV + float2(_RGBSplit * active, 0)
                ).r;

                float green = tex2D(_MainTex, shiftedUV).g;

                float blue = tex2D(
                    _MainTex,
                    shiftedUV - float2(_RGBSplit * active, 0)
                ).b;

                float3 rgb = float3(red, green, blue);

                float redFlash = step(0.94, rowNoise) * active;
                float cyanFlash = step(0.965, blockNoise) * active;

                rgb = lerp(
                    rgb,
                    float3(1.0, 0.015, 0.08),
                    redFlash * 0.8
                );

                rgb = lerp(
                    rgb,
                    float3(0.0, 0.9, 1.0),
                    cyanFlash * 0.65
                );

                rgb = lerp(
                    rgb,
                    float3(noise, noise, noise),
                    staticMask * 0.7
                );

                float blockAlpha = blockMask * step(0.5, blockNoise);

                rgb = lerp(
                    rgb,
                    float3(0.02, 0.0, 0.04),
                    blockAlpha
                );

                rgb *= scanMask;

                float glitchAlpha = saturate(
                    active * (
                        0.22 +
                        redFlash * 0.8 +
                        cyanFlash * 0.7 +
                        staticMask * 0.25 +
                        blockAlpha * 0.7
                    )
                );

                glitchAlpha *= _Fade * i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                glitchAlpha *= UnityGet2DClipping(
                    i.localPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(glitchAlpha - 0.001);
                #endif

                return fixed4(rgb * i.color.rgb, saturate(glitchAlpha));
            }
            ENDCG
        }
    }
}
