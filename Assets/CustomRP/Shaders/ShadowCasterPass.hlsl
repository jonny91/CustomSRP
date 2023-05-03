#ifndef CUSTOM_SHADOW_CASTER_PASS_INCLUDED
#define CUSTOM_SHADOW_CASTER_PASS_INCLUDED

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
UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes
{
    float3 positionOS: POSITION;
    float2 baseUV: TEXCOORD0;
    //表面法线
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS: SV_POSITION;
    float2 baseUV: VAR_BASE_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ShadowCasterPassVertex(Attributes input)
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

void ShadowCasterPassFragment(Varyings input)
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.baseUV);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 base = baseMap * baseColor;
    #if defined(_CLIPPING)
    clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
    #endif
}

#endif
