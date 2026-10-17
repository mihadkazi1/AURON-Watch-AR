Shader "AURON/WristOccluder"
{
    SubShader
    {
        // Render BEFORE opaque geometry (1990 < 2000) so the back strap fails the Z-Test
        Tags { "RenderType"="Opaque" "Queue"="Geometry-10" }
        ColorMask 0
        ZWrite On
        ZTest LEqual
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target { return 0; }
            ENDCG
        }
    }
}