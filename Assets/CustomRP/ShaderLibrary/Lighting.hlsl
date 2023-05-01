#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

/**
 * \brief 计算入射光照
 * \param surface 
 * \param light 
 * \return 
 */
float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction)) * light.color;
}


/**
 * \brief 根据物体表面信息获取最终光照结果
 * \param surface 
 * \param light 
 * \return 
 */
float3 GetLighting(Surface surface, Light light)
{
    return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting(Surface surface)
{
    float3 color = 0;
    for (int i = 0; i < GetDirectionalLightCount(); i++)
    {
        color += GetLighting(surface, GetDirectionalLight(i));
    }
    return color;
}


#endif
