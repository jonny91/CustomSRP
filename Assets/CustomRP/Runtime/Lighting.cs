/*************************************************************************************
 *
 * 文 件 名:   Lighting.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-05-01 17:40:45
*************************************************************************************/

using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string bufferName = "Lighting";
    private CullingResults _cullingResults;

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    /// <summary>
    /// 限制最大可见平行光数量
    /// </summary>
    private const int maxDirLightCount = 4;

    // private static int dirLightColorId = Shader.PropertyToID("_DirectionLightColor");
    // private static int dirLightDirectionId = Shader.PropertyToID("_DirectionLightDirection");
    private static int dirLightCountId = Shader.PropertyToID("_DirectionLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionLightColors");
    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionLightDirections");

    //存储可见光的颜色和方向
    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    private Shadows _shadows = new Shadows();

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this._cullingResults = cullingResults;

        buffer.BeginSample(bufferName);
        //传递阴影数据
        _shadows.Setup(context, cullingResults, shadowSettings);
        
        // 发送光源数据
        // SetupDirectionLight();
        SetupLights();
        _shadows.Render();
        
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    /// <summary>
    /// 发送多个光源数据
    /// </summary>
    private void SetupLights()
    {
        //得到所有可见光
        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;

        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            var visibleLight = visibleLights[i];
            //如果是方向光，才进行数据存储
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                //当超过数量，终止
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }

        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
    }

    /// <summary>
    /// 将场景主光源的颜色和方向传递给GPU
    /// </summary>
    // private void SetupDirectionLight()
    // {
    //     Light light = RenderSettings.sun;
    //     buffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
    //     buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
    // }

    /// <summary>
    /// 将可见光的光照颜色和方向存储到数组
    /// </summary>
    /// <param name="index"></param>
    /// <param name="visibleLight"></param>
    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        // 光照方向通过visibleLight.localToWorldMatrix获取，第三列为光源的前向向量
        /*
        visibleLight.localToWorldMatrix 存储了光源在世界坐标系中的变换矩阵，其中包含了位置、旋转和缩放信息。
        具体来说，矩阵的前三列分别代表了光源的右、上和前方向向量（也就是光源的X、Y、Z轴方向），第四列代表了光源在世界坐标系中的位置。
        这个矩阵可以用来将局部坐标系下的顶点坐标转换到世界坐标系下，使其与光源进行正确的相交计算。
         */
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        _shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    /// <summary>
    /// 释放阴影贴图RT内存
    /// </summary>
    public void Cleanup()
    {
        _shadows.Cleanup();
    }
}