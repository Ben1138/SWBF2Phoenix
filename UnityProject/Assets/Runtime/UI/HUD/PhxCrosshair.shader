Shader "Phoenix/PhxCrosshair"
{
    Properties
    {
        _CrosshairSpeed("CrosshairSpeed", Range(1.0, 100.0)) = 10.0
        _Ammo("Ammo", Range(0.0, 100.0)) = 50.0
        _Magazin("Magazin", Range(1.0, 100.0)) = 50.0
        _Fixation("Fixation", Range(0, 1)) = 0.0
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

        // No culling or depth
        Cull Off ZWrite Off ZTest Always Blend SrcAlpha OneMinusSrcAlpha

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

            float _CrosshairSpeed;
            float _Ammo;
            float _Magazin;
            float _Fixation;
            float4 _Color;


            // Inspired from: 
            //    https://www.shadertoy.com/view/ls3SR2
            //    https://www.shadertoy.com/view/WdcGRM


            #define PI 		3.14159265359
            #define TWO_PI  6.28318530718
            #define SMOOTH  0.005


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float radial_progress(float2 UV, float ir, float or, float VAL)
            {
                float2 	uv = (UV * 2.0) - 1.0;
                //uv.x /= 9.0 / 16.0; //remove this line in UE (compensating for 16:9 on ShaderToy)

                //float ir = 0.75;
                //float or = 0.95;
                float d = length(uv);
                float ring = smoothstep(or + SMOOTH, or - SMOOTH, d) - smoothstep(ir + SMOOTH, ir - SMOOTH, d);
                float a = atan2(uv.y, uv.x) - PI / 2.;
                float theta = (a < 0.0) ? (a + TWO_PI) / TWO_PI : a / TWO_PI;
                float bar = step(theta, VAL);
                float ui = ring * bar;

                return ui;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 fragColor = float4(0, 0, 0, 0);

                float circles = 0.0;

                float2 uvc = i.uv - float2(.5, .5);
                float amm = _Ammo / _Magazin;

                float theta = atan2(uvc.x, uvc.y);
                float c = length(uvc);

                // outer border
                if (c < .5 && c > .485)
                    circles = 1;

                // ammunition
                if (c < .45 && c > .4)
                {
                    circles = 3 * sin(theta * _Magazin);
                }

                // inner border
                if (c < .37 && c > .36)
                    circles = .5;

                float crosshairRadius = lerp(.33, .15, _Fixation);

                // rotating crosshair
                if (c < crosshairRadius && c > crosshairRadius - .03)
                {
                    float speed = lerp(_CrosshairSpeed, 0, _Fixation);
                    float time = _Time * speed;

                    // the +0.3 is for alignment purposes only
                    theta += 0.3;
                    circles = 3 * sin(theta * 5.0 + time * 10.0);
                }

                fragColor = clamp(circles, 0, 1);
                fragColor *= _Color;
                fragColor.a -= radial_progress(i.uv, .75, .95, 1. - amm);

                return fragColor;
            }
            ENDCG
        }
    }
}
