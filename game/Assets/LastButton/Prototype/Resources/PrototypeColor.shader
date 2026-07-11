Shader "LastButton/PrototypeColor"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct AppData
            {
                float4 vertex : POSITION;
            };

            struct VertexToFragment
            {
                float4 position : SV_POSITION;
            };

            fixed4 _Color;

            VertexToFragment Vert(AppData input)
            {
                VertexToFragment output;
                output.position = UnityObjectToClipPos(input.vertex);
                return output;
            }

            fixed4 Frag(VertexToFragment input) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
