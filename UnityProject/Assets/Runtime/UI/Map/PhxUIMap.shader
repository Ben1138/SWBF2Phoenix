Shader "Phoenix/PhxUIMap"
{
    Properties
    {
        _MapTex ("Map Texture", 2D) = "white" {}
        _MapTexSize("Map Texture Size", Float) = 1
        _CPTex ("CP Icon", 2D) = "white" {}
        _CPTexSize ("CP Icon Size", Float) = 1
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

            float2 _SpriteSize;     // Size of sprite host in pixels
            float2 _MapTexOffset;   // UV offset of map texture
            float2 _MapOffset;      // World space offset
            float _MapTexSize;      // Size modifier of Map texture
            float _CPTexSize;       // Size modifier of CP icon
            float _Zoom;            // World units per UV
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

            half4 _MapTex_TexelSize;
            half4 _CPTex_TexelSize;

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
                float2 mapPxSize    = float2(_MapTex_TexelSize.z, _MapTex_TexelSize.w);
                float2 cpIconPxSize = float2(_CPTex_TexelSize.z, _CPTex_TexelSize.w);

                // where is this 1.15 comming from? 
                // Take a look at the Zoom calculation in PhxUIMap.cs
                float2 mapTexZoom = _Zoom * 1.15 / (mapPxSize * _MapTexSize);

                float2 zoomUV = frag.uv * mapTexZoom - (mapTexZoom / 2);
                float2 offsetUV = (_MapOffset * mapTexZoom / _Zoom) + _MapTexOffset;

                fixed4 col = tex2D(_MapTex, float2(0.5, 0.5) + zoomUV - offsetUV);
                col.a = _Alpha;

                float2 CPMapSize = cpIconPxSize * _CPTexSize / float2(_SpriteSize.x, _SpriteSize.y);
                float2 CPMapHalfSize = CPMapSize / 2;
                
                for (int i = 0; i < _CPCount; ++i)
                {
                    int cpIdx = i / 2;
                    int cpVecIdx = (i % 2) * 2;
                    float2 cpWorldPos = float2(_CPPositions[cpIdx][cpVecIdx], _CPPositions[cpIdx][cpVecIdx + 1]);
                    
                    float2 cpMapPos = ((cpWorldPos + _MapOffset) / _Zoom) + float2(0.5, 0.5);
                    float2 diff = abs(frag.uv - cpMapPos);
                    
                    float2 cpUVMin = cpMapPos - CPMapHalfSize;
                    float2 cpUV = ((frag.uv - cpUVMin) / CPMapHalfSize) / 2;

                    if (diff.x < CPMapHalfSize.x && diff.y < CPMapHalfSize.y)
                    {
                        fixed4 cpCol = tex2D(_CPTex, cpUV);
                        col = col * (1 - cpCol.a) + cpCol.a * (cpCol * _CPColors[i]);

                        // display CPs always full opaque
                        col.a = lerp(_Alpha, 1, cpCol.a);
                    }

                    if (_CPSelected[i] > 0.2)
                    {
                        float c = ring(cpUV, _CPSelected[i] * 3, 0.1, 0.5);
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
