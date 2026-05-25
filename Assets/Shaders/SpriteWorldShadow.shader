Shader "Custom/Sprite World Shadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShadowTex ("World Shadow Texture", 2D) = "white" {}
        _ShadowCenter ("Shadow Center XY", Vector) = (0,0,0,0)
        _ShadowSize ("Shadow Size XY", Vector) = (10,10,0,0)
        _ShadowTint ("Shadow Tint", Color) = (0,0,0,1)
        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.45
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

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
                float2 texcoord : TEXCOORD0;
                float2 worldXY : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _ShadowTex;
            fixed4 _Color;
            fixed4 _ShadowTint;
            float4 _ShadowCenter;
            float4 _ShadowSize;
            float4 _WorldShadowCenter;
            float4 _WorldShadowSize;
            float _ShadowStrength;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;

                float4 worldPosition = mul(unity_ObjectToWorld, IN.vertex);
                OUT.worldXY = worldPosition.xy;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, IN.texcoord) * IN.color;

                float useWorldProjection = step(0.0001, abs(_WorldShadowSize.x) + abs(_WorldShadowSize.y));
                float2 shadowCenter = lerp(_ShadowCenter.xy, _WorldShadowCenter.xy, useWorldProjection);
                float2 shadowSize = lerp(_ShadowSize.xy, _WorldShadowSize.xy, useWorldProjection);
                float2 safeShadowSize = max(abs(shadowSize), float2(0.0001, 0.0001));
                float2 shadowUV = (IN.worldXY - shadowCenter) / safeShadowSize + 0.5;
                fixed4 shadow = tex2D(_ShadowTex, shadowUV);

                float insideShadow = step(0.0, shadowUV.x) * step(shadowUV.x, 1.0) *
                                     step(0.0, shadowUV.y) * step(shadowUV.y, 1.0);
                float shadowAmount = shadow.a * insideShadow * _ShadowStrength;

                spriteColor.rgb = lerp(spriteColor.rgb, spriteColor.rgb * _ShadowTint.rgb, shadowAmount);
                spriteColor.rgb *= spriteColor.a;

                return spriteColor;
            }
            ENDCG
        }
    }
}
