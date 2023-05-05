using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private Mesh _terrain;

    [SerializeField] private Vector2 size;

    [SerializeField] private Vector2 quadSize;

    [SerializeField] private float noiseValueMul;
    [SerializeField] private float noiseSizeMul;
    // Start is called before the first frame update
    void Start()
    {
        _terrain = GetComponent<MeshFilter>().mesh;
        GenerateTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GenerateTerrain()
    {
        MeshBuilder builder = new MeshBuilder();

        Vector3 normal = new Vector3(0, 1, 0);
        Vector2 uv = new Vector2(0, 0);
        int[] vertex = new int[4];
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {

                Vector3[] pos = new Vector3[4];
                for (int i = 0; i < vertex.Length; i++)
                {
                    float xValue = quadSize.x * x;

                    xValue += quadSize.x * ((i/2)%2);
                    
                    float yValue = quadSize.y * y;
                    yValue += quadSize.y * ((i/2 +i)%2);
                    
                    
                    pos[i] = new Vector3(xValue, GetPerlinNoiseValue(xValue, yValue), yValue);
                    
                }
                normal = Vector3.Cross(pos[1]- pos[0], pos[1]- pos[2]);
                
                vertex[0] = builder.AddVertex(pos[0], normal, uv);
                vertex[1] = builder.AddVertex(pos[1], normal, uv);
                
                
                normal = Vector3.Cross(pos[3]- pos[2], pos[3]- pos[0]);
                
                vertex[2] = builder.AddVertex(pos[2], normal, uv);
                vertex[3] = builder.AddVertex(pos[3], normal, uv);
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
        
        builder.Build(_terrain);
    }

    private float GetPerlinNoiseValue(float x, float y)
    {
        return Mathf.PerlinNoise(x * noiseSizeMul, y * noiseSizeMul) * noiseValueMul;
    }
}
