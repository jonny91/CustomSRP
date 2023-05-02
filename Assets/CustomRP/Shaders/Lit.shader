Shader "CustomRP/Lit"
{
    Properties
    {
        _BaseColor("COLOR", Color) = (.5,.5,.5,1)
        _BaseMap("TEXTURE", 2D) = "white"{}
        _Cutoff("Alpha Cutoff", RANGE(0.0,1.0)) = 0.5
        [Toggle(_CLIPPING)]_Clipping("Alpha Clipping", float) = 0
        [Toggle(_PREMULTIPLY_ALPHA)]_PremulAlpha("Premultiply Alpha", float) = 0

        //金属度和光滑度
        _Metallic("METALLIC", RANGE(0.0,1.0)) = 0
        _Smoothness("SMOOTHNESS", RANGE(0.0,1.0)) = 0.5

        //设置混合模式
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend", Float) = 0
        //默认写入深度缓冲区
        [Enum(Off,0,On,1)]_ZWrite("ZWrite", Float) = 1
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
            TAGS
            {
                "LightMode" = "CustomLit"
            }
            Name "Lit"
            Blend [_SrcBlend][_DstBlend]
            ZWRITE [_ZWrite]
            HLSLPROGRAM
            #pragma target 3.5

            #pragma shader_feature _CLIPPING
            //是否透明通道预乘
            #pragma shader_feature _PREMULTIPLY_ALPHA
            // 增加变体使用shader可以支持instance
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "LitPass.hlsl"
            ENDHLSL
        }
    }
    
    CustomEditor "CustomShaderGUI"
}