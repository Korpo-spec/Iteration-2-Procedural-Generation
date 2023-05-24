using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PerlinData
{
    [SerializeField] public string layerName;
    [SerializeField] public float weight;
    [SerializeField] public float noiseValueMul;
    [SerializeField] public float noiseSizeMul;
    [SerializeField] public float thresholdValue;
    [SerializeField][NonReorderable] public List<PerlinData> subMaps;
    [SerializeField] public List<GameObject> gameObjects;

}
