/*************************************************************************************
 *
 * 文 件 名:   MeshBall.cs
 * 描    述: 
 * 
 * 创 建 者：  洪金敏 
 * 创建时间：  2023-05-01 12:25:07
*************************************************************************************/

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int metallicId = Shader.PropertyToID("_Metallic");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField]
    private Mesh Mesh = default;

    [SerializeField]
    private Material Material = default;

    private Matrix4x4[] _matrices = new Matrix4x4[1023];
    private Vector4[] _baseColors = new Vector4[1023];
    private float[] _metallic = new float[1023];
    private float[] _smoothness = new float[1023];

    private MaterialPropertyBlock _block;

    private void Awake()
    {
        for (var i = 0; i < _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f,
                Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                Vector3.one * Random.Range(0.5f, 1.5f));
            _baseColors[i] = new Vector4(
                Random.value,
                Random.value,
                Random.value,
                Random.Range(0.5f, 1.0f));
            _metallic[i] = Random.value < 0.25f ? 1f : 0f;
            _smoothness[i] = Random.Range(0.05f, 0.95f);
        }
    }

    private void Update()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
            _block.SetVectorArray(baseColorId, _baseColors);
            _block.SetFloatArray(metallicId, _metallic);
            _block.SetFloatArray(smoothnessId, _smoothness);
        }

        Graphics.DrawMeshInstanced(Mesh, 0,
            Material, _matrices,
            _matrices.Length, _block);
    }
}