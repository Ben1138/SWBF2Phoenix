Shader "Phoenix/PhxHUDCapture"
{
    Properties
    {
        _CaptureIcon ("Capture Icon", 2D) = "white" {}
        _CaptureProgress ("Capture Progress", Float) = 1
        _CaptureColor ("Capture Color", Color) = (1, .0, .0, 1)
        [Toggle] _CaptureDispute ("Capture Disputed", Float) = 0
        _DisputeBlinkRate ("Dispute Blink Rate", Float) = 1
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

        // No culling or depth
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment fragment

            #include "UnityCG.cginc"


            sampler2D _CaptureIcon; 
            float     _CaptureProgress;
            float4    _CaptureColor;
            float     _CaptureDispute;
            float     _DisputeBlinkRate;

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

            v2f vertex(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 fragment(v2f frag) : SV_Target
            {
                fixed4 col = tex2D(_CaptureIcon, frag.uv);
                if (_CaptureDispute > 0)
                {
                    bool blink = ((sin((_Time[1] * 10) / (_DisputeBlinkRate * 2)) + 1) / 2) >= 0.5;
                    if (blink)
                    {
                        col *= _CaptureColor;
                    }
                }
                else if (frag.uv.y < _CaptureProgress)
                {
                    col *= _CaptureColor;
                }
                return col;
            }
            ENDCG
        }
    }
}
