using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int CutoffId = Shader.PropertyToID("_Cutoff");
    private static int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    Color baseColor = Color.white;

    [SerializeField, Range(0, 1f)]
    private float Cutoff = 0.5f;

    private static MaterialPropertyBlock _block;

    private void OnValidate()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }

        _block.SetColor(BaseColorId, baseColor);
        _block.SetFloat(CutoffId, Cutoff);

        GetComponent<Renderer>().SetPropertyBlock(_block);
    }

    private void Awake()
    {
        OnValidate();
    }
}