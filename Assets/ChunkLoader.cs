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

    private Vector2Int _currentValue;
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
        
        _minValue.y = (int)(pos.z - viewDistance);
        _minValue.y = _minValue.y.RoundOff();
        
        //Debug.Log(_minValue);
        
        _maxValue.x = (int)(pos.x + viewDistance);
        _maxValue.x = _maxValue.x.RoundOff();
        
        _maxValue.y = (int)(pos.z + viewDistance);
        _maxValue.y = _maxValue.y.RoundOff();


        _currentValue.x = (int) (pos.x);
        _currentValue.x = _currentValue.x.RoundOff();
        
        _currentValue.y = (int) (pos.z);
        _currentValue.y = _currentValue.y.RoundOff();
        Debug.Log(_currentValue);
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

    private void LoadChunkRow(int rowNum)
    {
        for (int i = _minValue.x; i <= _maxValue.x; i+= 10)
        {
            var g = Instantiate(chunk, new Vector3(i, 0, rowNum), Quaternion.identity);
            Mesh mesh = g.GetComponent<MeshFilter>().mesh;
            
            terrainGenerator.GenerateTerrain(mesh, new Vector3(i,0,rowNum));
            g.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    private void LoadChunkCol(int colNum)
    {
        for (int i = _minValue.y; i <= _maxValue.y; i+= 10)
        {
            var g = Instantiate(chunk, new Vector3(colNum, 0, i), Quaternion.identity);
            Mesh mesh = g.GetComponent<MeshFilter>().mesh;
            
            terrainGenerator.GenerateTerrain(mesh, new Vector3(colNum,0,i));
            g.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        if (pos.x < _currentValue.x -10)
        {
            LoadChunkRow(_minValue.x);
            _minValue.x -= 10;
            _currentValue.x -= 10;
        }

        if (pos.x > _currentValue.x +10)
        {
            LoadChunkCol(_maxValue.x);
            _maxValue.x += 10;
            _currentValue.x += 10;
        }
        
        if (pos.z < _currentValue.y -10)
        {
            LoadChunkRow(_minValue.y);
            _minValue.y -= 10;
            _currentValue.y -= 10;
        }

        if (pos.z > _currentValue.y +10)
        {
            LoadChunkCol(_maxValue.y);
            _minValue.y += 10;
            _currentValue.y += 10;
        }
    }
}
    
    

