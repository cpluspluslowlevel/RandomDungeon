Shader "RandomDungeon/URP/TileMap"
{

    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {

        Tags { "RenderType"="Opaque"
               "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _Color;

            struct VS_INPUT
            {
                float4 position : POSITION;
            };

            struct VS_OUTPUT
            {
                float4 position : SV_POSITION;
            };

            VS_OUTPUT vert (VS_INPUT input)
            {
    
                VS_OUTPUT output;
                output.position = TransformObjectToHClip(input.position.xyz);
    
                return output;
    
            }

            half4 frag (VS_OUTPUT input) : SV_Target
            {
                half4 col = half4(1.0f, 1.0f, 0.0f, 1.0f);
                return _Color;
            }

            ENDHLSL

        }

    }

}
