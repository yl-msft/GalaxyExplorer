using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[ExecuteInEditMode]
public class ForceTractorShaderTester : MonoBehaviour
{
    private const int Subdivisions = 10;
    private const float s_divider = (float) 1.0d / int.MaxValue;
    
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private MaterialPropertyBlock _propertyBlock;
    private Vector4[] _normals, _tangents, _biNormals;
    private readonly List<Vector4> _randoms = new List<Vector4>();

    public Vector4[] Points = {
        Vector3.zero, Vector3.one*.1f, Vector3.one*.2f, Vector3.one*.3f, Vector3.one*.4f,
        Vector3.one*.5f, Vector3.one*.6f, Vector3.one*.7f, Vector3.one*.8f, Vector3.one*.9f
//        Vector3.zero, Vector3.up*.1f, Vector3.up*.2f, Vector3.up*.3f, Vector3.up*.4f,
//        Vector3.up*.5f, Vector3.up*.6f, Vector3.up*.7f, Vector3.up*.8f, Vector3.up*.9f
    };
    
    private static readonly int PointsPropertyId = Shader.PropertyToID("_Points");
    private static readonly int TangentsPropertyId = Shader.PropertyToID("_Tangents");
    private static readonly int NormalsPropertyId = Shader.PropertyToID("_Normals");
    private static readonly int BiNormalsPropertyId = Shader.PropertyToID("_BiNormals");

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        _normals = new Vector4[Subdivisions];
        _biNormals = new Vector4[Subdivisions];
        _tangents = new Vector4[Subdivisions];
        _propertyBlock = new MaterialPropertyBlock();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter = GetComponent<MeshFilter>();
        GenerateData();
    }
    
    private static float GetRandFloat01(Random random)
    {
        return random.Next() * s_divider;
    }
    
    private void GenerateData()
    {
        if(_meshFilter == null || _meshFilter.sharedMesh == null) return;

        var random = new Random();

        var mesh = _meshFilter.sharedMesh;

        var vertices = mesh.vertices;
        
        _randoms.Clear();
        
        for(var i=0;i<vertices.Length;i++)
        {
            _randoms.Add(new Vector4(
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random)
            ));
        }

        mesh.SetUVs(1, _randoms);
        mesh.UploadMeshData(false);
        GetComponent<MeshFilter>().mesh = mesh;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePoints();
        UpdateBounds();
    }

    private void UpdatePoints()
    {
        for (var i = 0; i < Subdivisions; i++)
        {
            var a = Mathf.Max(0, i - 1);
            var b = Mathf.Min(Subdivisions - 1, i + 1);
            _tangents[i] = (Points[b] - Points[a]).normalized;
            var rotation = Quaternion.FromToRotation(Vector3.forward, _tangents[i]);
            _normals[i] = rotation * Vector3.up;
            _biNormals[i] = rotation * Vector3.right;
        }
        _propertyBlock.SetVectorArray(PointsPropertyId, Points);
        _propertyBlock.SetVectorArray(TangentsPropertyId, _tangents);
        _propertyBlock.SetVectorArray(NormalsPropertyId, _normals);
        _propertyBlock.SetVectorArray(BiNormalsPropertyId, _biNormals);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void UpdateBounds()
    {
        var bounds = _meshFilter.sharedMesh.bounds;
        bounds.SetMinMax(Vector3.zero, transform.worldToLocalMatrix.MultiplyPoint(Points[Subdivisions-1]));
        _meshFilter.sharedMesh.bounds = bounds;
    }
}
