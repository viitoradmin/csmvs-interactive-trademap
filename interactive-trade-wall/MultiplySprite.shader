Shader "Custom/LightSweepProcedural"
{
    Properties
    {
        _Color      ("Sweep Color", Color) = (1, 0.9, 0.7, 0.25)
        _BandCenter ("Band Center (0-1)", Range(-0.5, 1.5)) = 0.0
        _BandWidth  ("Band Half Width", Range(0.0, 1.0)) = 0.15
        _Softness   ("Edge Softness", Range(0.0, 1.0)) = 0.1
        _Vertical   ("Vertical Sweep (1 = vertical, 0 = horizontal)", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        // standard sprite-style alpha blending
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float  _BandCenter;
            float  _BandWidth;
            float  _Softness;
            float  _Vertical;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;    // 0..1 across sprite
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Choose X or Y depending on vertical/horizontal mode
                float coord = lerp(i.uv.x, i.uv.y, saturate(_Vertical));

                // Distance from center line
                float d = abs(coord - _BandCenter);

                // Band intensity: 1 at center, 0 outside width+softness
                float inner = _BandWidth;
                float outer = _BandWidth + _Softness;
                float band  = 1.0 - smoothstep(inner, outer, d);

                fixed4 col = _Color;
                col.a *= band;       // modulate alpha by band strength

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}