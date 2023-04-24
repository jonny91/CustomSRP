Shader "CustomRP/Unlit"
{
    Properties
    {
        _BaseColor("COLOR", Color) = (1,1,1,1)
    }
    SubShader
    {

        Pass
        {
            HLSLPROGRAM
            #include "UnlitPass"
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            ENDHLSL
        }
    }
}