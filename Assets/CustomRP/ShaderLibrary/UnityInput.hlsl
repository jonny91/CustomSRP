#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
//这些信息在渲染时会被传递给着色器，以便计算每个像素的最终颜色。它是一个常量缓冲区，不能被修改。
CBUFFER_START(UnityPerDraw)
//定义从模型空间转换到世界空间的转换矩阵
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
//包含一些转换信息
float4 unity_WorldTransformParams;
float4 unity_LODFade;
CBUFFER_END

//从世界空间转换到裁剪空间的矩阵
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;
//相机位置
float3 _WorldSpaceCameraPos;

#endif
