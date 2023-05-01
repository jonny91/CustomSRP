/*************************************************************************************
 *
 * 文 件 名:   CustomRenderPipeline.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-04-22 22:30:25
*************************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _renderer = new CameraRenderer();
    private readonly bool _useDynamicBatching;
    private readonly bool _useGPUInstancing;
    private readonly bool _useSRPBatcher;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        this._useDynamicBatching = useDynamicBatching;
        this._useGPUInstancing = useGPUInstancing;
        this._useSRPBatcher = useSRPBatcher;
        GraphicsSettings.useScriptableRenderPipelineBatching = this._useSRPBatcher;
    }

    /// <summary>
    /// 每一帧都会调用Render方法进行画面渲染
    /// </summary>
    /// <param name="context">SRP底层渲染接口之一</param>
    /// <param name="cameras">相机对象组，存储了参与这一帧渲染的所有相机对象</param>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //可以让每个相机使用不同的渲染方式绘制画面
        foreach (var camera in cameras)
        {
            _renderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing);
        }
    }
}