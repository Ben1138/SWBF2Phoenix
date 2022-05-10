
Shader "Unlit/HologramShader"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _Scale("Scale", Float) = 1.0
    }
    SubShader
    {
        Lighting Off
        ZWrite Off
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        Tags {"Queue" = "Transparent"}
        Color[_Color]
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Scale;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.vertex.x = UnityObjectToClipPos(v.vertex * _Scale);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 _Color;

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
