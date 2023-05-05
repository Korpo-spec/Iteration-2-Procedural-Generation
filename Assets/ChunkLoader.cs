using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    [SerializeField] private float viewDistance;
    
    [SerializeField] private GameObject chunk;

    [SerializeField] private TerrainGenerator terrainGenerator;

    private Dictionary<Vector2, GameObject> _loadedChunks;

    private Vector2Int _minValue;
    private Vector2Int _maxValue;
    // Start is called before the first frame update
    void Start()
    {
        _loadedChunks = new Dictionary<Vector2, GameObject>();
        
        
        InitValues();
        LoadInitChunk();
        
    }

    private void InitValues()
    {
        Vector3 pos = transform.position;
        _minValue.x = (int)(pos.x - viewDistance);
        _minValue.x = _minValue.x.RoundOff();
        
        _minValue.y = (int)(pos.y - viewDistance);
        _minValue.y = _minValue.y.RoundOff();
        
        //Debug.Log(_minValue);
        
        _maxValue.x = (int)(pos.x + viewDistance);
        _maxValue.x = _maxValue.x.RoundOff();
        
        _maxValue.y = (int)(pos.y + viewDistance);
        _maxValue.y = _maxValue.y.RoundOff();
        //Debug.Log(_maxValue);
    }

    private void LoadInitChunk()
    {
        for (int i = _minValue.y; i <=  _maxValue.y; i+=10)
        {
            for (int j = _minValue.x; j <= _maxValue.x; j+=10)
            {
                var g = Instantiate(chunk, new Vector3(j, 0, i), Quaternion.identity);
                Mesh mesh = g.GetComponent<MeshFilter>().mesh;
                
                terrainGenerator.GenerateTerrain(mesh, new Vector3(j,0,i));
                g.GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
