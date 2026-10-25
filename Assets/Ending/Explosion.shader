
Shader "Sachiko/Explosion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Center ("Explosion Center", Vector) = (0.5,0.5,0,0)
        _Aspect ("Screen Aspect", Float) = 1.777
        _Radius ("Explosion Radius", Float) = 0.1
        _Intensity ("Explosion Intensity", Float) = 1
        _Distortion ("Ray Distortion", Float) = 0.15
        _RaySharpness ("Ray Sharpness", Float) = 5
        _Shockwave ("Shockwave Strength", Float) = 0.8
        _Fade ("Explosion Fade", Float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float4 _Center;
            float _Aspect;
            float _Radius;
            float _Intensity;
            float _Distortion;
            float _RaySharpness;
            float _Shockwave;
            float _Fade;
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
                o.localPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv - _Center.xy;
                p.x *= _Aspect;

                float distanceFromCenter = length(p);
                float angle = atan2(p.y, p.x);

                float rayPattern = 0.5 + 0.5 * cos(
                    angle * 22.0 +
                    sin(angle * 7.0) * _Distortion * 8.0
                );

                float sharpRays = pow(
                    saturate(rayPattern),
                    max(_RaySharpness, 1.0)
                );

                float rayLength = _Radius * (
                    0.72 + sharpRays * 0.48
                );

                float coreRadius = _Radius * 0.3;

                float core = 1.0 - smoothstep(
                    0.0,
                    coreRadius,
                    distanceFromCenter
                );

                float rays = (1.0 - smoothstep(
                    rayLength * 0.82,
                    rayLength,
                    distanceFromCenter
                )) * sharpRays;

                float shockwaveDistance = abs(
                    distanceFromCenter - _Radius * 0.82
                );

                float shockwave = (
                    1.0 - smoothstep(
                        _Radius * 0.035,
                        _Radius * 0.09,
                        shockwaveDistance
                    )
                ) * _Shockwave;

                float outerGlow = 1.0 - smoothstep(
                    _Radius * 0.45,
                    _Radius * 1.2,
                    distanceFromCenter
                );

                float alpha = saturate(
                    max(core, max(rays, shockwave)) +
                    outerGlow * 0.2
                );

                float heat = saturate(
                    core * 1.2 + shockwave * 0.35
                );

                float3 orange = float3(1.0, 0.12, 0.015);
                float3 yellow = float3(1.0, 0.55, 0.04);
                float3 white = float3(1.0, 0.95, 0.75);

                float3 color = lerp(
                    orange,
                    yellow,
                    saturate(sharpRays + shockwave * 0.4)
                );

                color = lerp(color, white, heat);

                color *= _Intensity;

                alpha *= _Fade * i.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(
                    i.localPosition.xy,
                    _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif

                return fixed4(color * i.color.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
}
