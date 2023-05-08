using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Profiling;

public class TerrainGenerator : MonoBehaviour
{
    private Mesh _terrain;

    [SerializeField] private Vector2Int size;

    [SerializeField] private Vector2 quadSize;

    [SerializeField] private float noiseValueMul;
    [SerializeField] private float noiseSizeMul;

    
    private int perlinMod = 100;
    private Vector2Int _triCount;
    
    private MeshCollider _collider;
    // Start is called before the first frame update
    private void Awake()
    {
        //TODO: create code to size quads nicely;
        //quadSize.x -= (size.x % quadSize.x) / (size.x / quadSize.x);
        //Debug.Log((size.x % quadSize.x) / (size.x / quadSize.x));
        
        _triCount.x =(int) (size.x / quadSize.x);
        _triCount.y =(int) (size.y / quadSize.y);
    }

    void Start()
    {
        _terrain = GetComponent<MeshFilter>().mesh;
        _collider = GetComponent<MeshCollider>();
        
        //GenerateTerrain(_terrain, Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateTerrainAsync(Mesh mesh, Vector3 origin)
    {
        Thread generationThread = new Thread((() => GenerateTerrain(mesh, origin)));
        generationThread.Start();
    }

    public void GenerateTerrain(Mesh mesh, Vector3 origin)
    {
        MeshBuilder builder = new MeshBuilder();

        Vector3[] normal = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        Vector3[] pos = new Vector3[4];
        int[] vertex = new int[4];
        Dictionary<Vector2, float> perlin = new Dictionary<Vector2, float>();
        for (int y = 0; y < _triCount.y; y++)
        {
            for (int x = 0; x < _triCount.x; x++)
            {
                
                
                for (int i = 0; i < vertex.Length; i++)
                {
                    float xValue =  quadSize.x * x;

                    xValue += quadSize.x * ((i/2)%2);
                    
                    float yValue =  quadSize.y * y;
                    yValue += quadSize.y * ((i/2 +i)%2);

                    uv[i] = new Vector2(xValue / size.x, yValue/size.y);
                    
                    Profiler.BeginSample("Noise");
                    pos[i] = new Vector3(xValue, GetPerlinNoiseValue(origin.x +xValue, origin.z +yValue, perlin), yValue);
                    
                    Profiler.EndSample();
                }

                int mod = 0;
                for (int i = 0; i < 6; i++)
                {
                    
                    var p = Vector3.Cross(pos[1+mod] - pos[0+mod], pos[2+mod] - pos[0+mod]);
                    normal[0+mod] += p;
                    normal[1+mod] += p;
                    normal[2+mod] += p;
                    if (i == 3)
                    {
                        mod++;
                    }
                }
                
                //normal = Vector3.Cross(pos[1]- pos[2], pos[1]- pos[0]).normalized;
                
                vertex[0] = builder.AddVertex(pos[0], normal[0].normalized, uv[0]);
                vertex[1] = builder.AddVertex(pos[1], normal[1].normalized, uv[1]);
                
                
                //normal = Vector3.Cross(pos[3]- pos[0],pos[3]- pos[2] ).normalized;
                
                vertex[2] = builder.AddVertex(pos[2], normal[2].normalized, uv[2]);
                vertex[3] = builder.AddVertex(pos[3], normal[3].normalized, uv[3]);
                /*
                Vector3 pos = new Vector3(xValue, GetPerlinNoiseValue(xValue, yValue), yValue);
                vertex[0] = builder.AddVertex(pos, normal, uv);
                
                pos = new Vector3(xValue + quadSize.x, GetPerlinNoiseValue(xValue + quadSize.x, yValue), yValue);
                vertex[1] = builder.AddVertex(pos, normal, uv);
                
                pos = new Vector3(xValue, GetPerlinNoiseValue(xValue, yValue + quadSize.y), yValue + quadSize.y);
                vertex[2] = builder.AddVertex(pos, normal, uv);
                
                pos = new Vector3(xValue + quadSize.x, GetPerlinNoiseValue(xValue + quadSize.x, yValue + quadSize.y), yValue + quadSize.y);
                vertex[3] = builder.AddVertex(pos, normal, uv);
                */
                
               
                
                builder.AddQuad(vertex[0], vertex[1], vertex[2], vertex[3]);
                
            }
        }

        
        builder.Build(mesh);
        
        //.sharedMesh = mesh;
        
    }

    private float GetPerlinNoiseValue(float x, float y)
    {
        return Mathf.PerlinNoise(perlinMod +x * noiseSizeMul, perlinMod +y * noiseSizeMul) * noiseValueMul;
    }
    
    private float GetPerlinNoiseValue(float x, float y, Dictionary<Vector2, float> perlin)
    {
        if (perlin.TryGetValue(new Vector2(x,y), out float perlinvalue))
        {
            return perlinvalue;
        }

        return Mathf.PerlinNoise(perlinMod +x * noiseSizeMul, perlinMod +y * noiseSizeMul) * noiseValueMul;
    }
}
