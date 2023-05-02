/*************************************************************************************
 *
 * 文 件 名:   CustomShaderGUI.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-05-02 15:15:52
*************************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor _editor;
    private Object[] _materials;
    private MaterialProperty[] _properties;
    private bool _showPresets;

    bool Clipping
    {
        set => SetProperty("_Clipping", "_CLIPPING", value);
    }

    bool PremultiplyAlpha
    {
        set => SetProperty("_PremulAlpha", "_PREMULTIPLY_ALPHA", value);
    }

    BlendMode SrcBlend
    {
        set => SetProperty("_SrcBlend", (float)value);
    }

    BlendMode DstBlend
    {
        set => SetProperty("_DstBlend", (float)value);
    }

    private bool ZWrite
    {
        set => SetProperty("_ZWrite", value ? 1f : 0f);
    }

    private bool HasProperty(string name) => FindProperty(name, _properties, false) != null;
    bool HasPremultiplyAlpha => HasProperty("_PremulAlpha");

    RenderQueue RenderQueue
    {
        set
        {
            foreach (var o in _materials)
            {
                var material = (Material)o;
                material.renderQueue = (int)value;
            }
        }
    }

    bool PresetButton(string name)
    {
        if (GUILayout.Button(name))
        {
            //属性重置
            _editor.RegisterPropertyChangeUndo(name);
            return true;
        }

        return false;
    }

    void OpaquePreset()
    {
        if (PresetButton("Opaque"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.Geometry;
        }
    }

    void ClipPreset()
    {
        if (PresetButton("Clip"))
        {
            Clipping = true;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.One;
            DstBlend = BlendMode.Zero;
            ZWrite = true;
            RenderQueue = RenderQueue.AlphaTest;
        }
    }

    void FadePreset()
    {
        if (PresetButton("Fade"))
        {
            Clipping = false;
            PremultiplyAlpha = false;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    void TransparentPreset()
    {
        if (HasPremultiplyAlpha && PresetButton("Transparent"))
        {
            Clipping = false;
            PremultiplyAlpha = true;
            SrcBlend = BlendMode.SrcAlpha;
            DstBlend = BlendMode.OneMinusSrcAlpha;
            ZWrite = false;
            RenderQueue = RenderQueue.Transparent;
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        _editor = materialEditor;
        _materials = materialEditor.targets;
        _properties = properties;

        EditorGUILayout.Space(2);
        _showPresets = EditorGUILayout.Foldout(_showPresets, "Presets", true);
        if (_showPresets)
        {
            OpaquePreset();
            ClipPreset();
            FadePreset();
            TransparentPreset();
        }
    }

    private bool SetProperty(string name, float value)
    {
        var materialProperty = FindProperty(name, _properties, false);
        if (materialProperty != null)
        {
            materialProperty.floatValue = value;
            return true;
        }

        return false;
    }

    private void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }

    private void SetKeyword(string keyword, bool enable)
    {
        if (enable)
        {
            foreach (Material material in _materials)
            {
                material.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material material in _materials)
            {
                material.DisableKeyword(keyword);
            }
        }
    }
}