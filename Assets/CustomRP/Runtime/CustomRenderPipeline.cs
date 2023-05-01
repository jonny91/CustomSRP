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
    //每一帧把可以进行批处理的模型网格进行合并，再把合并好的数据传递给CPU，使用同一个材质进行渲染
    //有很多限制，比如使用逐对象材质属性失效、顶点属性规模小于900
    private readonly bool _useDynamicBatching;
    private readonly bool _useGPUInstancing;
    private readonly bool _useSRPBatcher;

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        this._useDynamicBatching = useDynamicBatching;
        this._useGPUInstancing = useGPUInstancing;
        this._useSRPBatcher = useSRPBatcher;
        GraphicsSettings.useScriptableRenderPipelineBatching = this._useSRPBatcher;
        //灯光强度转到线性空间线
        GraphicsSettings.lightsUseLinearIntensity = true;
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