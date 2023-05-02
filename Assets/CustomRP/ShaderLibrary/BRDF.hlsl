#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#include "./Surface.hlsl"

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

/**
 * \brief 电介质的反射率平均约0.04
 */
#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity(float metallic)
{
    float range = 1.0f - MIN_REFLECTIVITY;
    return range - metallic * range;
}

/**
 * \brief 获取给定表面brdf数据
 * \param surface 
 * \param applyAlphaToDiffuse 开启透明度预乘 
 * \return 
 */
BRDF GetBRDF(Surface surface, bool applyAlphaToDiffuse = false)
{
    BRDF brdf;
    float oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
    brdf.diffuse = surface.color * oneMinusReflectivity;
    if (applyAlphaToDiffuse)
    {
        brdf.diffuse *= surface.alpha;
    }
    //根据能量守恒，表面反射的光能不能超过入射的光能，这意味着镜面反射的颜色应等于表面颜色减去漫反射颜色
    // brdf.specular = surface.color - brdf.diffuse;
    //但这忽略了一个事实，即金属影响镜面反射的颜色，而非金属不影响。非金属的镜面反射应该是白色的，
    //最后我们通过金属度在最小反射率和表面颜色之间进行插值得到BRDF的镜面反射颜色。
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    //光滑度转为实际粗糙度
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}

/**
 * \brief 根据公式得到镜面反射强度
 * \param surface 
 * \param brdf 
 * \param light 
 * \return 
 */
float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    // 归一化的 L + V , 光和视角方向的对角线向量
    // 使用SafeNormalize 避免两个向量在相反的情况下被零除
    float3 h = SafeNormalize(light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1) + 1.00001);
    float normalization = brdf.roughness * 4 + 2;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

/**
 * \brief 直接光照的表面颜色
 * \param surface 
 * \param brdf 
 * \param light 
 * \return 
 */
float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif
