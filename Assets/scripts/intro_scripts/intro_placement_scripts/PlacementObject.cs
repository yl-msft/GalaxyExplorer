using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlacementObject : MonoBehaviour
{
    private readonly List<Vector4> _randoms = new List<Vector4>();
    [SerializeField, HideInInspector]
    private Mesh _targetMesh;
    private float _time;
    private Material _material;

    private const float s_divider = (float) 1.0d / int.MaxValue;

    public Mesh SourceMesh;
    public int Seed;
    public float Speed = 5f;
    
    
    private static readonly int SelfTime = Shader.PropertyToID("_SelfTime");
    private static readonly int Active = Shader.PropertyToID("_Active");

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material; //instanced!! (not saved, not shared)
    }

    private void Start()
    {
        GenerateData();
    }

    private void Update()
    {
        _time += Time.deltaTime * Speed;
        _material.SetFloat(SelfTime, _time);
    }

    private static float GetRandFloat01(Random random)
    {
        return random.Next() * s_divider;
    }

    public void SetActive(bool active)
    {
        _material.SetFloat(Active, active?1f:0f);
    }

    public void GenerateData()
    {
        if(SourceMesh == null) return;

        Debug.Log("generating data");
        var random = new Random(Seed);
        
        var vertices = SourceMesh.vertices;
        
        _randoms.Clear();
        
        foreach (var vertex in vertices)
        {
            _randoms.Add(new Vector4(
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random),
                GetRandFloat01(random)
            ));
        }

        _targetMesh = new Mesh {vertices = vertices, triangles = SourceMesh.triangles};
        _targetMesh.SetUVs(0, _randoms);
        _targetMesh.RecalculateBounds();
        _targetMesh.UploadMeshData(false);
        GetComponent<MeshFilter>().mesh = _targetMesh;
        Debug.Log("data generated");
    }
}
