using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    Color baseColor = Color.white;

    private static MaterialPropertyBlock _block;

    private void OnValidate()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }

        _block.SetColor(BaseColorId, baseColor);

        GetComponent<Renderer>().SetPropertyBlock(_block);
    }

    private void Awake()
    {
        OnValidate();
    }
}