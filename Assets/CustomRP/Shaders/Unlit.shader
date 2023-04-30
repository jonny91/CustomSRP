Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseColor("COLOR", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            // 增加变体使用shader可以支持instance
            #pragma multi_compile_instancing

            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            #include "UnlitPass.hlsl"
            ENDHLSL
        }
    }
}