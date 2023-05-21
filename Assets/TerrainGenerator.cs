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
    [SerializeField] private Gradient gradient;

    [SerializeField] private List<PerlinData> perlinLayers;
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

    //[SerializeField] private Texture2D previewTex = new Texture2D(1024, 1024);

    public Texture2D GenerateTerrain(Mesh mesh, Vector3 origin)
    {
        MeshBuilder builder = new MeshBuilder();

        Vector3[] normal = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        Vector3[] pos = new Vector3[4];
        int[] vertex = new int[4];
        Dictionary<Vector2, float> perlin = new Dictionary<Vector2, float>();

        Texture2D chunktexture = new Texture2D(_triCount.x, _triCount.y);

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
                    chunktexture.SetPixel(x, y, gradient.Evaluate(GetPerlinNoiseValue(origin.x +xValue, origin.z +yValue, perlin)/noiseValueMul));
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
        mesh.RecalculateNormals();
        //Debug.Log(chunktexture.height);
        //Debug.Log(chunktexture.GetPixel(10,10));
        chunktexture.wrapMode = TextureWrapMode.Mirror;
        chunktexture.Apply();
        return chunktexture;
        //.sharedMesh = mesh;

    }

    private float GetPerlinNoiseValue(float x, float y, float noiseSizeMul, List<PerlinData> subMaps, float perlinMod, float thresholdValue, out bool threshold)
    {
        float result = Mathf.PerlinNoise(perlinMod +x * noiseSizeMul, perlinMod +y * noiseSizeMul) ;
        if (thresholdValue <= result )
        {
            threshold = true;
            foreach (var map in subMaps)
            {
                result += GetPerlinNoiseValue(x, y, map.noiseSizeMul, map.subMaps, perlinMod * 2.5f, map.thresholdValue, out _)*map.noiseValueMul*
                          Mathf.Max(result-thresholdValue, 0f);
            }
            return result;
            if (thresholdValue == 0)
            {
                return result;
            }
            return (result-thresholdValue)*(1/(1f-thresholdValue))+thresholdValue;
            
        }
        threshold = false;
        //Debug.Log(result);
        return 1f-(Mathf.Pow((result-thresholdValue)* (1/(1f-thresholdValue)) , 2)*15f);
        
        
        
    }
    
    private float GetPerlinNoiseValue(float x, float y, Dictionary<Vector2, float> perlin)
    {
        if (perlin.TryGetValue(new Vector2(x,y), out float perlinvalue))
        {
            return perlinvalue;
        }

        float result = 1;
        int originalMod = perlinMod;
        foreach (var perlinData in perlinLayers)
        {
            float perlinResult = GetPerlinNoiseValue(x, y, perlinData.noiseSizeMul, perlinData.subMaps, perlinMod,
                perlinData.thresholdValue, out bool threshold);
            
            if (threshold )
            {
                float weightLeftOver = 1 - perlinData.weight;
                result *= weightLeftOver;
                result += perlinData.weight * perlinResult * perlinData.noiseValueMul;
            }
            else
            {
                //Debug.Log(perlinResult);
                float newWeight = perlinData.weight * Mathf.Lerp(0f,1f,  perlinResult);
                //Debug.Log(newWeight);
                float weightLeftOver = 1 - newWeight;
                result *= weightLeftOver;
                result += newWeight * perlinResult * (perlinData.noiseValueMul/(1/perlinData.thresholdValue));
                //Mathf.Lerp(0f, perlinResult * noiseValueMul, perlinResult);
            }
            
            perlinMod /= 2;
        }

        perlinMod = originalMod;
        perlin.Add(new Vector2(x,y), result);
        
        return result;
    }
    
    
}
