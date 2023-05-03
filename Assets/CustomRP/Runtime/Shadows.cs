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