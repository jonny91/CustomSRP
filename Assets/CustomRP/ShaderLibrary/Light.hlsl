#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4
//方向光数据
CBUFFER_START(_CustomLight)
// float3 _DirectionLightColor;
// float3 _DirectionLightDirection;
int _DirectionLightCount;
float4 _DirectionLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
float4 _DirectionLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

//灯光属性
struct Light
{
    float3 color;
    float3 direction;
};

int GetDirectionalLightCount()
{
    return _DirectionLightCount;
}

//获取平行光属性
Light GetDirectionalLight(int index)
{
    Light light;
    light.color = _DirectionLightColors[index].rgb;
    light.direction = _DirectionLightDirections[index].xyz;

    return light;
}

#endif
