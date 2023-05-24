using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PerlinDataChild
{
    [SerializeField] public string layerName;
    [SerializeField] public float weight;
    [SerializeField] public float noiseValueMul;
    [SerializeField] public float noiseSizeMul;
    [SerializeField] public float thresholdValue;
    
    [SerializeField] public List<GameObject> gameObjects;

}
