using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Biome/Biome Attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string BiomeName;
    public int SolidGroundHeight;
    public int TerrainHeight;
    [Space]
    [Header("Noise Settings")]
    [Range(0f, 5f)]
    public float TerrainScale;
    [Range(0, 10)]
    public int TerrainOctaves;
    [Range(0f, 10f)]
    public float TerrainPersistance;
    [Range(0f, 10f)]
    public float TerrainRedistributionModifier;
    [Range(0f, 10f)]
    public float Exponent;
    public Lode[] Lodes;
}
[System.Serializable]
public class Lode
{
    public string LodeName;
    public byte VoxelID;
    public int MinHeight;
    public int MaxHeight;
    public float Scale;
    public float Threshold;
    public float Offset;
}
