/*************************************************************************************
 *
 * 文 件 名:   CameraRenderer.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-04-22 22:36:59
*************************************************************************************/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    partial void PrepareBuffer();
    partial void DrawUnsupportedShaders();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();
#if UNITY_EDITOR
    private static Material _errorMaterial;

    private static ShaderTagId[] _legacyShaderTagId = new[]
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };

#if UNITY_EDITOR
    private string SampleName { get; set; }
    partial void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        _buffer.name = SampleName = _camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName = bufferName;
#endif


    /// <summary>
    /// 绘制SRP不支持的着色器
    /// </summary>
    partial void DrawUnsupportedShaders()
    {
        //洋红色的错误材质
        if (_errorMaterial == null)
        {
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        //用数组的第一个元素来构建DrawingSettings对象 不用重复创建
        var drawingSettings = new DrawingSettings(_legacyShaderTagId[0], new SortingSettings(_camera))
        {
            overrideMaterial = _errorMaterial
        };
        for (var i = 1; i < _legacyShaderTagId.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagId[i]);
        }

        // 使用默认的即可以了，反正画出来都是不支持的
        var filteringSettings = FilteringSettings.defaultValue;
        //绘制不支持的shaderTag类型的物体
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            //如果切换到了Scene视图，调用此方法完成绘制
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }


#endif
}