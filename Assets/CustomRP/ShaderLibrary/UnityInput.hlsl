#ifndef CUSTOM_UNITY_INPUT_INCLUDED
#define CUSTOM_UNITY_INPUT_INCLUDED

//定义从模型空间转换到世界空间的转换矩阵
float4x4 unity_ObjectToWorld;
float4x4  unity_WorldToObject;
//包含一些转换信息
float4 unity_WorldTransformParams;

//从世界空间转换到裁剪空间的矩阵
float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 glstate_matrix_projection;

#endif
