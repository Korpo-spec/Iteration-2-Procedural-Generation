using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
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

        Vector3 normal = new Vector3(0, 1, 0);
        Vector2 uv = new Vector2(0, 0);
        int[] vertex = new int[4];
        for (int y = 0; y <= _triCount.y; y++)
        {
            for (int x = 0; x <= _triCount.x; x++)
            {

                Vector3[] pos = new Vector3[4];
                for (int i = 0; i < vertex.Length; i++)
                {
                    float xValue =  quadSize.x * x;

                    xValue += quadSize.x * ((i/2)%2);
                    
                    float yValue =  quadSize.y * y;
                    yValue += quadSize.y * ((i/2 +i)%2);
                    
                    Profiler.BeginSample("Noise");
                    pos[i] = new Vector3(xValue, GetPerlinNoiseValue(origin.x +xValue, origin.z +yValue), yValue);
                    Profiler.EndSample();
                }
                normal = Vector3.Cross(pos[1]- pos[2], pos[1]- pos[0]);
                
                vertex[0] = builder.AddVertex(pos[0], normal, Vector2.zero);
                vertex[1] = builder.AddVertex(pos[1], normal, new Vector2(0,1));
                
                
                normal = Vector3.Cross(pos[3]- pos[0],pos[3]- pos[2] );
                
                vertex[2] = builder.AddVertex(pos[2], normal, new Vector2(1,1));
                vertex[3] = builder.AddVertex(pos[3], normal, new Vector2(1,0));
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
}
