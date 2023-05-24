using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{
    private Mesh _terrain;

    [SerializeField] private Vector2Int size;

    [SerializeField] private Vector2 quadSize;

    [SerializeField] private float noiseValueMul;
    [SerializeField] private float noiseSizeMul;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Texture2D[] textures;
    [SerializeField] private Texture2D[] normalMaps;
     private Texture2DArray texture2DArray;
    [SerializeField] private Texture2DArray texture2DArrayNormals;
    [SerializeField] private ComputeShader textureShader;

    [SerializeField][NonReorderable] private List<PerlinData> perlinLayers;
    
    private int perlinMod = 100;
    private Vector2Int _triCount;
    [SerializeField]private Texture2D chunkPerlinTexture;
    private MeshCollider _collider;
    private int mainkernel;
    // Start is called before the first frame update
    private void Awake()
    {
        //TODO: create code to size quads nicely;
        //quadSize.x -= (size.x % quadSize.x) / (size.x / quadSize.x);
        //Debug.Log((size.x % quadSize.x) / (size.x / quadSize.x));
        
        _triCount.x =(int) (size.x / quadSize.x);
        _triCount.y =(int) (size.y / quadSize.y);
        
        chunkPerlinTexture = new Texture2D(_triCount.x, _triCount.y);
        builders = new Queue<MeshBuilder>();
        mainkernel = textureShader.FindKernel("CSMain");
        Application.targetFrameRate = 60;
        
        texture2DArray =
            new Texture2DArray(2048, 2048, gradient.colorKeys.Length, TextureFormat.ARGB32, false);
        // //texture2DArrayNormals = new Texture2DArray(2048, 2048, gradient.colorKeys.Length, TextureFormat.ARGB32, false);
        for (int i = 0; i < gradient.colorKeys.Length; i++)
        {
            
            //normalMaps[i] = pastNormals.Length > i?pastNormals[i] : null;
            
            texture2DArray.SetPixels(textures[i]?.GetPixels(0), i);
            //texture2DArrayNormals.SetPixels(normalMaps[i]?.GetPixels(0), i);
        }
        
        
        texture2DArray.Apply();
    }

    void Start()
    {
        _terrain = GetComponent<MeshFilter>().mesh;
        _collider = GetComponent<MeshCollider>();
        
        
        
        //GenerateTerrain(_terrain, Vector3.zero);
    }

    private void OnValidate()
    {
        var pasttextures = textures;
        //var pastNormals = normalMaps;
        textures = new Texture2D[gradient.colorKeys.Length];
        //normalMaps = new Texture2D[gradient.colorKeys.Length];
        // texture2DArray =
        //     new Texture2DArray(2048, 2048, gradient.colorKeys.Length, TextureFormat.ARGB32, false);
        // //texture2DArrayNormals = new Texture2DArray(2048, 2048, gradient.colorKeys.Length, TextureFormat.ARGB32, false);
        for (int i = 0; i < gradient.colorKeys.Length; i++)
        {
            textures[i] = pasttextures.Length > i?pasttextures[i] : null;
            //normalMaps[i] = pastNormals.Length > i?pastNormals[i] : null;
            
            // texture2DArray.SetPixels(textures[i]?.GetPixels(0), i);
            //texture2DArrayNormals.SetPixels(normalMaps[i]?.GetPixels(0), i);
        }
        
        
        // texture2DArray.Apply();
        //texture2DArrayNormals.Apply();
        
    }

    
    // Update is called once per frame
    void Update()
    {
        lock (builders)
        {
            for (int i = 0; i < builders.Count; i++)
            {

                MeshBuilder builder = builders.Dequeue();
                builder?.Build();
                
                RenderTexture outputTexture = new RenderTexture(256, 256,0);
                outputTexture.enableRandomWrite = true;
                outputTexture.format = RenderTextureFormat.RGB111110Float;
                outputTexture.Create();
                builder.g.GetComponent<MeshRenderer>().material.mainTexture = outputTexture;
                textureShader.SetTexture(mainkernel,"outputTexture",outputTexture);
                chunkPerlinTexture = new Texture2D(_triCount.x, _triCount.y);
                chunkPerlinTexture.SetPixels(builder?.pixels, 0);
                chunkPerlinTexture.Apply();
                textureShader.SetTexture(mainkernel, "inputTexture", chunkPerlinTexture);
                
                textureShader.SetInt("inputTextureSizeX", _triCount.x);
                textureShader.SetInt("inputTextureSizeY", _triCount.y);
                Random.InitState(123);
                Vector3 pos = builder.g.transform.position;
                int seed = (int)(pos.x*3467 + pos.z * 44565);
                float xValue = SeededRandom(0.0f, 10.0f, seed);
                float zValue = SeededRandom(0.0f, 10.0f, seed);
                pos.y = chunkPerlinTexture.GetPixel((int)xValue*2, (int)zValue*2).r *
                        perlinLayers.Sum(e => e.noiseValueMul + e.subMaps.Sum(f => f.noiseValueMul));
                
                pos.x += xValue;
                pos.z += zValue;
                if (SeededRandom(0, 10, seed)>6f) 
                {
                    PerlinData gameObjectToInstantiate = perlinLayers[0];
                    foreach (var layer in perlinLayers)
                    {
                        if (layer.thresholdValue != 0 && layer.thresholdValue < pos.y/perlinLayers.Sum(e => e.noiseValueMul))
                        {
                            gameObjectToInstantiate = layer;
                        }
                    
                    
                    }

                    
                    Instantiate(gameObjectToInstantiate.gameObjects[SeededRandom(0, gameObjectToInstantiate.gameObjects.Count, seed)], new Vector3(pos.x, pos.y, pos.z),
                        Quaternion.identity);
                }
                
                
                
                textureShader.SetTexture(mainkernel, "inputTextures", texture2DArray);
                
                float[] timestamps = gradient.colorKeys.Select(e => e.time).ToArray();
                ComputeBuffer floatBuffer = new ComputeBuffer(timestamps.Length, sizeof(float));
                floatBuffer.SetData(timestamps);
                textureShader.SetBuffer(mainkernel, "timeStamps",floatBuffer);
                
                textureShader.Dispatch(mainkernel,1,1,1);
                
                floatBuffer.Release();
                builder.g.GetComponent<MeshCollider>().sharedMesh = builder.mesh;
            }
        }
        
    }

    private float SeededRandom(float minValue, float maxvalue, int seed)
    {
        Random.InitState(seed);
        return Random.Range(minValue, maxvalue);
    }
    
    private int SeededRandom(int minValue, int maxvalue, int seed)
    {
        Random.InitState(seed);
        return Random.Range(minValue, maxvalue);
    }

    public void GenerateTerrainAsync(Mesh mesh, Vector3 origin, GameObject g)
    {

        Task generationTask = new Task(() => GenerateTerrain(mesh, origin, g));
        generationTask.Start();
        //Thread generationThread = new Thread((() => GenerateTerrain(mesh, origin, g)));
        //generationThread.Start();
    }

    private Queue<MeshBuilder> builders = new Queue<MeshBuilder>();
    //[SerializeField] private Texture2D previewTex = new Texture2D(1024, 1024);

    public void GenerateTerrain(Mesh mesh, Vector3 origin, GameObject g)
    {
        MeshBuilder builder = new MeshBuilder(mesh);
        builder.g = g;
        Vector3[] normal = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        Vector3[] pos = new Vector3[4];
        int[] vertex = new int[4];
        Color[] perlin = new Color[(_triCount.x) * (_triCount.y)];

        builder.pixels = perlin;

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
                    pos[i] = new Vector3(xValue, GetPerlinNoiseValue(origin.x +xValue, origin.z +yValue, perlin, origin), yValue);
                    //chunktexture.SetPixel(x, y, gradient.Evaluate(GetPerlinNoiseValue(origin.x +xValue, origin.z +yValue, perlin)/perlinLayers.Sum(e=>e.noiseValueMul)));
                    Profiler.EndSample();
                }
                
                int mod = 0;
                for (int i = 0; i < 6; i++)
                {
                    
                    var p = Vector3.Cross(pos[1+mod] - pos[0+mod], pos[(2+mod)%4] - pos[0+mod]);
                    normal[0+mod] += p;
                    normal[1+mod] += p;
                    normal[(2+mod)%4] += p;
                    if (i == 3)
                    {
                        mod+=2;
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


        lock (builders)
        {
            builders.Enqueue(builder);
        }
        
        //Debug.Log(chunktexture.height);
        //Debug.Log(chunktexture.GetPixel(10,10));
        //chunktexture.wrapMode = TextureWrapMode.Mirror;
        //chunktexture.Apply();
        /*
        
        */
        //return null;
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
        return  Mathf.Max((result)*(1/(thresholdValue)), 0f);
        //(Mathf.Pow((result-thresholdValue)* (1/(1f-thresholdValue)) , 2)*15f);

        

    }
    
    private float GetPerlinNoiseValue(float x, float y, Color[] perlin, Vector3 origins)
    {
        /*
        if (perlin.TryGetValue(new Vector2(x,y), out float perlinvalue))
        {
            return perlinvalue;
        }
        */

        float result = 1;
        int originalMod = perlinMod;
        foreach (var perlinData in perlinLayers)
        {
            float perlinResult = GetPerlinNoiseValue(x, y, perlinData.noiseSizeMul, perlinData.subMaps, originalMod,
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
            
            originalMod /= 2;
        }

        
        int xValue = (int) ((x - origins.x)*(1/quadSize.x));
        int yValue = (int) ((y - origins.z)*(1/quadSize.y));
        xValue = Math.Min(xValue, 19);
        yValue = Math.Min(yValue, 19);
        //Debug.Log(xValue+ "   "+ yValue);
        perlin[xValue + yValue*_triCount.x] =  new Color((result/perlinLayers.Sum(e=>e.noiseValueMul+e.subMaps.Sum(f=>f.noiseValueMul))),0,0);
        
        //perlin.Add(new Vector2(x,y), result);
        
        return result;
    }
    
    
}
