Shader "Phoenix/PhxLoadIcon"
{
    Properties
    {
        _Speed("Speed", Range(1.0, 100.0)) = 10.0
        _Percent("Percentage", Range(0.0, 1.0)) = 1.0
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

        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            float _Speed;
            float _Percent;

            // Inspired from: https://www.shadertoy.com/view/ls3SR2

            float rand(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
            }

            float pattern(float2 uv, float s, float time)
            {
                float2 puv2 = uv * s;
                float flow = .65 + .35 * (cos(puv2.x * 8.0 + 9.0 * sin(puv2.y + time * .75)));

                float lineWidth = .885 + .1 * flow;
                float linePattern = 0.0;
                float lineGradient = float(pow(.4 + .3 * length(uv - .5), 3.));


                float lineX = float(max(0.0, -lineWidth - sin((puv2.x + puv2.y) * 200.0)));
                float lineY = float(max(0.0, -lineWidth - cos((-puv2.x + puv2.y) * 200.0)));

                linePattern += 11.0 * lineGradient * (lineX + lineY);

                ////dots
                float d = max(0.0, 0.7 - pow(length(fmod((puv2 - float2(.138, .153)) * 63.6500, 2.0) - .5), .40));
                float4 dots = float4(d, d, d, d);
                if (dots.x > (.20 + .075 * flow))
                    linePattern = .5;
                linePattern += dots.x * .25;


                d = max(0.0, 0.7 - pow(length(fmod((puv2 - float2(.4360, .4515)) * 63.6500, 2.0) - .5), .40));
                dots = float4(d, d, d, d);
                if (dots.x > (.25 + .05 * flow))
                    linePattern = .5;
                return linePattern;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 fragColor = float4(0, 0, 0, 0);

                float2 uv = i.uv;
                float2 uv2 = uv + float2(0, -0.17);//* float2(1.0, _ScreenParams.y / _ScreenParams.x);
                float time = _Time * _Speed;


                //circles
                ////////////////////////////////////////////////
                float circles = 0.0;
                float cflow = .75 + .25 * (cos(time * 6.2 + uv.x * 34.0 + 5.0 * sin(uv.y * 3.4 + time * 3.35)));
                float dflow = .75 + .25 * (cos(uv.x * 12.0 + 12.0 * sin(uv.y * 19.0 + time * 4.75)));

                float2 uvc = (uv2 - float2(0.5, .28)) * 1.95 + float2(0.0, -.1);
                float theta = atan2(uvc.x, uvc.y);
                float c = length(uvc);

                float teth2 = (acos((uv.x - .5) / length(uv - .5)));

                float rt = ((sin(time + theta * 4.0 * sin((theta + time) * 1.0))));
                float rt2 = ((sin(theta * 2.0 - time + cos(theta * 5.0 + time))));

                // outer boarder
                if (c < .228 && c > 0.225)  // outerCircle
                    circles += .5 + dflow;

                if (c < .223 && c > 0.205)  // dotted 
                    circles += .75 * (.5 + .5 * sin(theta * 200.0 + time * 10.0));

                if (c < .203 && c > 0.200)  // outerCircle
                    circles += .5;



                float layer1 = lerp(.04, .08, clamp(_Percent / .33, 0, 1));
                float layer2 = lerp(.08, .13, clamp((_Percent - .33) / .33, 0, 1));
                float layer3 = lerp(.13, .18, clamp((_Percent - .66) / .33, 0, 1));

                // loading - layer 3 (outermost)
                if (c < layer3 && c > 0.13)
                    if (rt < 0.4 && rt < 0.1)
                        circles += .55 + cflow;

                // loading - layer 2
                if (c < layer2 && c > 0.08)
                    if (rt2 < 0.3 && rt2 < 0.7)
                        circles += .55 + dflow;

                // loading - layer 3 (innermost)
                if (c < layer1 && c > 0.04)
                    if (rt < 0.84 && rt < 0.4)
                        circles += .45;

                // inner rotating dots
                if (c < .030 && c > 0.025)  
                    circles += .35 * (.5 + .5 * sin(theta * 50.0 + time * 10.0));

                // give it some pattern
                //circles += max(0.0, circles*pattern(uv2, 5.4, time));
                fragColor += circles * cflow;

                return fragColor;
            }
            ENDCG
        }
    }
}
