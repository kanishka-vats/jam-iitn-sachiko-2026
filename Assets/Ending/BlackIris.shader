
Shader "Sachiko/BlackIris"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Radius ("Iris Radius", Range(0, 2)) = 1.5
        _Aspect ("Screen Aspect", Float) = 1.7778
        _Center ("Focus Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Alpha ("Effect Alpha", Range(0, 1)) = 1

        // Required for Unity UI masking and stencil support.
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
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _Center;
            float _Radius;
            float _Aspect;
            float _Alpha;
            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV coordinates and _Center must both be normalized
                // relative to the full-screen IrisImage.
                float2 p = i.uv - _Center.xy;

                // Correct the circle for the screen's aspect ratio.
                p.x *= _Aspect;

                float distanceFromCenter = length(p);

                // Transparent in the centre; black outside.
                float edgeWidth = max(fwidth(distanceFromCenter), 0.002);

                float outsideCircle = smoothstep(
                    _Radius - edgeWidth,
                    _Radius + edgeWidth,
                    distanceFromCenter
                );

                fixed4 texColor = tex2D(_MainTex, i.uv) * i.color;

                fixed4 result;
                result.rgb = 0;
                result.a = outsideCircle * _Alpha * texColor.a;

                #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(
                    i.worldPosition.xy, _ClipRect
                );
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
                #endif

                return result;
            }
            ENDCG
        }
    }
}
