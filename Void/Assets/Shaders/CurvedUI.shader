Shader "Custom/CurvedUI"
{
    Properties
    {
        _Curvature("Curvature", Float) = 0.1
        _Color("Color", Color) = (1, 1, 1, 1)
        _Offset("Offset", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Curvature;
            fixed4 _Color;
            float _Offset;

            v2f vert(appdata v)
            {
                v2f o;

                // Transform vertex to world space
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

                // Transform world position to screen space
                float4 screenPos = mul(UNITY_MATRIX_VP, worldPos);
                screenPos.xyz /= screenPos.w; // Perspective divide

                // Calculate distance from the center of the screen
                float2 screenCenter = float2(0.5, 0.5); // Center of the screen in normalized coordinates
                float2 screenUV = screenPos.xy * 0.5 + 0.5; // Convert to [0, 1] range
                screenUV.y = 0.5;
                float distanceFromCenter = length(screenUV - screenCenter);

                // Apply curvature based on distance from the center
                // Invert the curvature effect to stretch when curving up and compress when curving down
                worldPos.y += _Curvature * distanceFromCenter * distanceFromCenter + _Offset;

                // Preserve size by adjusting the vertex position in world space
                // This compensates for perspective distortion
                //worldPos.x /= (1.0 + _Curvature * distanceFromCenter);
                worldPos.z /= (1.0 + _Curvature * distanceFromCenter);

                // Transform back to clip space
                o.pos = mul(UNITY_MATRIX_VP, worldPos);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // White color
            }
            ENDCG
        }
    }
}