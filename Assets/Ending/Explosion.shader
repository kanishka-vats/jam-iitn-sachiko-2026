
Shader "Sachiko/Explosion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Center ("Energy Center (UV)", Vector) = (0.5,0.5,0,0)
        _Aspect ("Screen Aspect", Float) = 1.7778
        _Radius ("Wave Radius", Float) = 0.1
        _Intensity ("Brightness", Float) = 2.5
        _Distortion ("Distortion", Float) = 0.25
        _RaySharpness ("Fissure Sharpness", Float) = 5
        _Shockwave ("Wave Strength", Float) = 0.8
        _Fade ("Effect Fade", Float) = 1
        _Progress ("Animation Progress", Range(0,1)) = 0
        _TimeScale ("Animation Speed", Float) = 1
        _Whiteout ("Whiteout", Range(0,1)) = 0

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
            float4 _Center;
            float _Aspect;
            float _Radius;
            float _Intensity;
            float _Distortion;
            float _RaySharpness;
            float _Shockwave;
            float _Fade;
            float _Progress;
            float _TimeScale;
            float _Whiteout;
            float4 _ClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPosition = v.vertex;
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise2d(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(cell);
                float b = hash21(cell + float2(1, 0));
                float c = hash21(cell + float2(0, 1));
                float d = hash21(cell + float2(1, 1));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _TimeScale;

                float n1 = noise2d(uv * float2(7, 5) + time * 0.3);
                float n2 = noise2d(uv * float2(17, 12) - time * 0.45);
                float n3 = noise2d(uv * float2(32, 23) + time * 0.7);

                float turbulence = n1 * 0.5 + n2 * 0.32 + n3 * 0.18;

                float2 warpedUV = uv;
                warpedUV.x += (n2 - 0.5) * _Distortion * 0.2;
                warpedUV.y += (n1 - 0.5) * _Distortion * 0.2;

                float sweep = sin(
                    warpedUV.y * 17.0 +
                    warpedUV.x * 6.0 -
                    _Progress * 12.0 +
                    turbulence * 7.0
                );

                float bands = pow(
                    saturate(1.0 - abs(sweep)),
                    max(_RaySharpness * 0.25, 1.0)
                );

                float fissureNoise = noise2d(
                    warpedUV * float2(23, 18) + time
                );

                float fissures = smoothstep(
                    0.58, 0.88,
                    fissureNoise + bands * 0.3
                );

                float2 p = uv - _Center.xy;
                p.x *= _Aspect;

                float dist = length(p);
                float waveFront = _Radius +
                    (turbulence - 0.5) * _Distortion * 0.2;

                float waveWidth = 0.025 + _Progress * 0.05;

                float shockwave = 1.0 - smoothstep(
                    waveWidth,
                    waveWidth * 3.5,
                    abs(dist - waveFront)
                );

                shockwave *= _Shockwave;

                float energy = saturate(
                    fissures * 0.55 +
                    bands * 0.25 +
                    shockwave * 0.5
                );

                float coverage = smoothstep(
                    0.0, 0.85, _Progress
                );

                float3 deepCrimson = float3(0.035, 0.001, 0.008);
                float3 crimson = float3(0.35, 0.003, 0.025);
                float3 hotRed = float3(1.0, 0.025, 0.045);
                float3 hotWhite = float3(1.0, 0.82, 0.72);
                float3 white = float3(1.0, 0.97, 0.94);

                float3 color = lerp(deepCrimson, crimson, energy);
                color = lerp(color, hotRed, saturate(fissures * 0.8 + shockwave * 0.5));
                color = lerp(color, hotWhite, saturate(bands * 0.35 + shockwave * 0.5));
                color = lerp(color, white, saturate(_Whiteout));

                float baseCoverage = coverage * 0.92;
                float alpha = saturate(
                    max(baseCoverage, energy * 0.8) * _Intensity
                );

                alpha *= _Fade * i.color.a;

                fixed4 texColor = tex2D(_MainTex, i.uv) * i.color;
                color *= texColor.rgb;

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(
                    i.localPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return fixed4(color, saturate(alpha));
            }
            ENDCG
        }
    }
}
