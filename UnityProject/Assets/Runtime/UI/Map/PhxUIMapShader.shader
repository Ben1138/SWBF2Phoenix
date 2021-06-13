Shader "Phoenix/PhxUIMapShader"
{
    Properties
    {
        _MapTex ("Map Texture", 2D) = "white" {}
        _CPTex ("CP Icon", 2D) = "white" {}
        _MapOffsetX("Map Offset X", Float) = 0
        _MapOffsetY("Map Offset Y", Float) = 0
        _Zoom("Zoom", Float) = 200
        _Alpha("Alpha", Float) = 0.8

        _Radius("Radius", Float) = 0.8
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

            float4 _CPPositions[32];
            float4 _CPColors[64];
            float _CPSelected[64];
            int _CPCount;

            float _MapOffsetX;
            float _MapOffsetY;
            float _Zoom;
            float _Alpha;
            float _Radius;

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

            sampler2D _MapTex;
            sampler2D _CPTex;

            float circle(float2 st, float radius, float smoothRadius)
            {
                float2 dist = st + float2(-0.5, -0.5);
                return 1. - smoothstep(radius - (radius * smoothRadius),
                    radius + (radius * smoothRadius),
                    dot(dist, dist) * 4.0);
            }

            float ring(float2 st, float radius, float smoothRadius, float thickness)
            {
                return circle(st, radius, smoothRadius) - circle(st, radius - thickness, smoothRadius);
            }

            fixed4 fragment(v2f frag) : SV_Target
            {
                fixed4 col = tex2D(_MapTex, frag.uv);
                col.a = _Alpha;

                const float2 CPMapSize = float2(0.015, 0.015);
                
                for (int i = 0; i < _CPCount; ++i)
                {
                    int cpIdx = i / 2;
                    int cpVecIdx = (i % 2) * 2;
                    float2 cpWorldPos = float2(_CPPositions[cpIdx][cpVecIdx], _CPPositions[cpIdx][cpVecIdx + 1]);
                    //cpWorldPos += float2(_Zoom / 2, _Zoom / 2);

                    float2 cpMapPos = ((cpWorldPos + float2(_MapOffsetX, _MapOffsetY)) / _Zoom) + float2(0.5, 0.5);
                    float2 diff = abs(frag.uv - cpMapPos);

                    float2 cpUVMin = cpMapPos - CPMapSize;
                    float2 cpUV = ((frag.uv - cpUVMin) / CPMapSize) / 2;

                    if (diff.x < CPMapSize.x && diff.y < CPMapSize.y)
                    {
                        //col = fixed4(cpUV, 0, 1);
                        //col = tex2D(_MapTex, cpUV);
                        fixed4 cpCol = tex2D(_CPTex, cpUV);
                        col = col * (1 - cpCol.a) + cpCol.a * (cpCol * _CPColors[i]);

                        // display CPs always full opaque
                        col.a = lerp(_Alpha, 1, cpCol.a);
                    }

                    if (_CPSelected[i] > 0.2)
                    {
                        float2 cpRingUV = cpUV;

                        float c = ring(cpRingUV, _CPSelected[i] * 3, 0.1, 0.5);
                        col = lerp(col, _CPColors[i], c);
                    }
                }

                float c = circle(frag.uv, _Radius, 0.05);
                col.a = lerp(0, col.a, c);

                return col;
            }
            ENDCG
        }
    }
}
