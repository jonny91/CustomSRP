/*********************************************************************************
 *****
 * 文 件 名:   CustomRenderPipelineAsset.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-04-22 22:19:19
*************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool UseDynamicBatching = true;

    [SerializeField]
    private bool UseGPUInstancing = true;

    [SerializeField]
    private bool UseSRPBatcher = true;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(UseDynamicBatching, UseGPUInstancing, UseSRPBatcher);
    }
}