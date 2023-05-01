/*************************************************************************************
 *
 * 文 件 名:   CameraRenderer.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-04-22 22:36:59
*************************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    private const string bufferName = "Render Camera";
    static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");

    private CommandBuffer _buffer = new CommandBuffer()
    {
        name = bufferName
    };

    private ScriptableRenderContext _context;
    private Camera _camera;
    
    public void Render(ScriptableRenderContext context, Camera camera,
        bool useDynamicBatching, bool useGPUInstancing)
    {
        this._context = context;
        this._camera = camera;

        //设置缓冲区名字
        PrepareBuffer();

        //在Game视图绘制的几何体和绘制到Scene视图中
        //因为此操作会给场景添加几何体，所以在Cull之前
        PrepareForSceneWindow();

        //只渲染相机视野内的物体，视野外的要剔除掉
        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
        // 绘制SRP不支持的着色器
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }


    private void Setup()
    {
        //设置相机属性和矩阵
        _context.SetupCameraProperties(_camera);
        var flags = _camera.clearFlags;
        //为了绘制下一帧正确, 清除旧的数据
        //FrameDebug 里 如果之前缓冲区里有东西 会调用DrawGL清理，这样性能其实不好
        //没有东西的时候(SetupCameraProperties 放前面) 会调用Clear(color+Z+stencil)清理
        _buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        _buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 提交缓冲区渲染命令
    /// </summary>
    private void Submit()
    {
        _buffer.EndSample(SampleName);
        ExecuteBuffer();
        _context.Submit();
    }

    private void ExecuteBuffer()
    {
        //从缓冲区复制命令
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    /// <summary>
    /// 绘制可见物
    /// </summary>
    private void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
    {
        //绘制顺序和指定渲染相机
        //确定相机的透明排序模式 是否使用正交或者基于距离的排序
        var sortingSettings = new SortingSettings(_camera)
        {
            criteria = SortingCriteria.CommonOpaque
        };
        //设置渲染的Shader Pass和排序模式
        var drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings)
        {
            //设置渲染批处理使用状态
            enableDynamicBatching = useDynamicBatching,
            enableInstancing = useGPUInstancing,
        };
        //设置哪些类型的渲染队列可以被绘制
        var filteringSettings = new FilteringSettings(RenderQueueRange.all);
        //绘制图像
        //1.绘制不透明物体
        _context.DrawRenderers(
            _cullingResults,
            ref drawingSettings,
            ref filteringSettings);

        //2.绘制天空盒
        _context.DrawSkybox(_camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        //只绘制transparent的透明物体
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        //3.绘制透明物体
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private CullingResults _cullingResults;

    bool Cull()
    {
        ScriptableCullingParameters p;
        if (_camera.TryGetCullingParameters(out p))
        {
            //最后可见的物体
            //执行剔除
            _cullingResults = _context.Cull(ref p);
            return true;
        }

        return false;
    }
}