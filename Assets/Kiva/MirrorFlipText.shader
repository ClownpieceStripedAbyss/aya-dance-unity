Shader "Kiva/MirrorFlipText"
{
    Properties {
        [PerRendererData] _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            int _VRChatMirrorMode;
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            half4 _MainTex_TexelSize;

            // struct appdata_t
            // {
            //     float4 vertex   : POSITION;
            //     float4 color    : COLOR;
            //     float2 texcoord : TEXCOORD0;
            //     UNITY_VERTEX_INPUT_INSTANCE_ID
            // };

            // struct v2f
            // {
            //     float4 vertex   : SV_POSITION;
            //     fixed4 color    : COLOR;
            //     float2 texcoord  : TEXCOORD0;
            //     float4 worldPosition : TEXCOORD1;
            //     UNITY_VERTEX_OUTPUT_STEREO
            // };

            // v2f vert(appdata_t v)
            // {
            //     v2f OUT;
            //     UNITY_SETUP_INSTANCE_ID(v);
            //     UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

            //     if (_VRChatMirrorMode) {
            //         v.texcoord.x = 1 - v.texcoord.x;
            //     }

            //     OUT.worldPosition = v.vertex;
            //     OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
            //     OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            //     OUT.color = v.color * _Color;

            //     return OUT;
            // }

            // fixed4 frag(v2f IN) : SV_Target
            // {
            //     half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

            //     #ifdef UNITY_UI_CLIP_RECT
            //     color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
            //     #endif

            //     #ifdef UNITY_UI_ALPHACLIP
            //     clip (color.a - 0.001);
            //     #endif

            //     return color;
            // }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color    : COLOR;
            };

            v2f vert (appdata v)
            {
                float2 uv = v.uv;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                o.color = v.color * _Color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float2 uv = v.uv;
                float2 uv = i.uv.xy / _MainTex_TexelSize.zw;
                // uv = lerp(uv, float2(1.0 - uv.x, uv.y), _VRChatMirrorMode);
                uv.xy *= _MainTex_TexelSize.zw;

                fixed4 col = (tex2D(_MainTex, uv) + _TextureSampleAdd) * i.color;
                return col;
            }
            ENDCG
        }
    }
}
