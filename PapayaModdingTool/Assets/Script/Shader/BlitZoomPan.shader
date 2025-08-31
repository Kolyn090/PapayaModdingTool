Shader "Hidden/BlitZoomPan"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Zoom("Zoom", Float) = 1.0
        _PanOffset("PanOffset", Vector) = (0,0,0,0)
        _Scale("Scale", Vector) = (1,1,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Zoom;
            float2 _PanOffset;
            float2 _Scale;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Center UV, apply zoom and scale
                float2 uv = (i.uv - 0.5) / (_Zoom * _Scale) + 0.5 + _PanOffset;
                uv = clamp(uv, 0.0, 1.0); // Prevent repeats
                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}