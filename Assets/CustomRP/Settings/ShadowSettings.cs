/*************************************************************************************
 *
 * 文 件 名:   ShadowSettings.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-05-03 13:10:35
*************************************************************************************/

using System;
using UnityEngine;

[Serializable]
public class ShadowSettings
{
    /// <summary>
    /// 阴影最大距离
    /// </summary>
    [Min(0f)]
    public float MaxDistance = 100f;

    /// <summary>
    /// 阴影贴图大小
    /// </summary>
    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }

    /// <summary>
    /// 方向光阴影配置
    /// </summary>
    [Serializable]
    public struct Directional
    {
        public TextureSize AtlasSize;
    }

    public Directional directional = new Directional()
    {
        AtlasSize = TextureSize._1024
    };
}