using System;
using UnityEngine;

[Serializable]
public struct PerlinData
{
    [SerializeField] public string layerName;
    [SerializeField] public float weight;
    [SerializeField] public float noiseValueMul;
    [SerializeField] public float noiseSizeMul;
    [SerializeField] public float thresholdValue;
    
}
