/*************************************************************************************
 *
 * 文 件 名:   Shadows.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-05-03 13:43:45
*************************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    /// <summary>
    /// 可投射阴影的定向光数量
    /// </summary>
    private const int MaxShadowedDirectionalLightCount = 1;

    struct ShadowedDirectionalLight
    {
        /// <summary>
        /// 因为不知道哪个可见光源会产生阴影，
        /// 所以定义一个ShadowedDirectionalLight结构体来追踪可见光的索引，
        /// 然后创建一个该结构体类型的数组，存储所有能产生阴影的可见光索引。
        /// </summary>
        public int VisibleLightIndex;
    }

    private ShadowedDirectionalLight[] ShadowedDirectionalLights =
        new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    /// <summary>
    /// 已储存的可投射阴影的平行光数量
    /// </summary>
    private int ShadowedDirectionalLightCount;

    private const string bufferName = "Shadows";

    private CommandBuffer _buffer = new CommandBuffer()
    {
        name = bufferName
    };

    private ScriptableRenderContext _context;
    private CullingResults _cullingResults;
    private ShadowSettings _shadowSettings;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this._context = context;
        this._cullingResults = cullingResults;
        this._shadowSettings = shadowSettings;

        ShadowedDirectionalLightCount = 0;
    }

    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
    }

    /// <summary>
    /// 渲染定向光阴影
    /// </summary>
    private void RenderDirectionalShadows()
    {
        //创建RT
        int atlasSize = (int)_shadowSettings.directional.AtlasSize;
        //指定类型是阴影贴图
        _buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize,
            32, FilterMode.Bilinear,
            RenderTextureFormat.Shadowmap);
        //创建渲染纹理后调用buffer.SetRenderTarget方法来指定渲染数据存储到渲染纹理而不是帧缓冲区中，
        //该方法中后两个参数用于指定如何加载和存储渲染纹理的数据，
        //它的初始状态我们不关心，因为会立即清除它。渲
        //染纹理的目的是存储阴影数据，所以使用RenderBufferStoreAction.Store模式。
        //最后调用buffer.ClearRenderTarget方法清除渲染目标的数据，这里我们只关心深度缓冲，所以只需清除它。
        _buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);
        ExecuteBuffer();

        for (int i = 0; i < ShadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, atlasSize);
        }

        _buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    /// <summary>
    /// 渲染单个光源阴影
    /// </summary>
    /// <param name="index"></param>
    /// <param name="tileSize"></param>
    private void RenderDirectionalShadows(int index, int tileSize)
    {
        ShadowedDirectionalLight light = ShadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(_cullingResults, index);
        //3. 阴影贴图本质也是一张深度图，它记录了从光源位置出发，到能看到的场景中距离它最近的表面位置（深度信息）。
        //但是方向光并没有一个真实位置，
        //我们要做地是找出与光的方向匹配的视图和投影矩阵，
        //并给我们一个裁剪空间的立方体，
        //该立方体与包含光源阴影的摄影机的可见区域重叠，这些数据的获取我们不用自己去实现，
        //可以直接调用cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives方法，
        //它需要9个参数。第1个是可见光的索引，
        //第2、3、4个参数用于设置阴影级联数据，后面我们会处理它，
        //第5个参数是阴影贴图的尺寸，
        //第6个参数是阴影近平面偏移，
        //我们先忽略它。最后三个参数都是输出参数，一个是视图矩阵，一个是投影矩阵，一个是ShadowSplitData对象，
        //它描述有关给定阴影分割（如定向级联）的剔除信息。
        _cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.VisibleLightIndex, 0, 1,
            Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

        shadowSettings.splitData = splitData;
        _buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExecuteBuffer();
        _context.DrawShadows(ref shadowSettings);
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        //光源开启了阴影投影 并且 阴影强度不为0
        if (ShadowedDirectionalLightCount < MaxShadowedDirectionalLightCount &&
            light.shadows != LightShadows.None &&
            light.shadowStrength > 0f &&
            // 是否在阴影最大投射距离内，有被光源影响 且 需要投影的物体存在
            _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            ShadowedDirectionalLights[ShadowedDirectionalLightCount++] = new ShadowedDirectionalLight()
            {
                VisibleLightIndex = visibleLightIndex,
            };
        }
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    public void Cleanup()
    {
        _buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }
}