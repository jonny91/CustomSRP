#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"

//CBUFFER_START(UnityPerMaterial)
// float4 _BaseColor;
//CBUFFER_END

//纹理和采样器是着色器资源，必须在全局定义，不能放入缓冲区
//定义一张2D纹理
TEXTURE2D(_BaseMap);
//定义采样器
SAMPLER(sampler_BaseMap);


UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
// 获取纹理的平铺和偏移值，设置给每个实例，所以可以在缓冲区中定义
UNITY_DEFINE_INSTANCED_PROP(float4, _BaseMap_ST)
UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes
{
    float3 positionOS: POSITION;
    float2 baseUV: TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS: SV_POSITION;
    float2 baseUV: VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float3 positionWS = TransformObjectToWorld(input.positionOS);
    output.positionCS = TransformWorldToHClip(positionWS);
    //计算缩放和偏移后的UV坐标
    float4 baseST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseMap_ST);
    output.baseUV = input.baseUV * baseST.xy + baseST.zw;
    return output;
}

half4 UnlitPassFragment(Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    half4 c = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 base = c * baseMap;

    #if defined(_CLIPPING)
    //透明度低于阈值的片元被舍弃
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif

    return base;
}

#endif
