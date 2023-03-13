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
    [Header("Trees")]
    public float TreeZoneScale = 0.05f;
    [Range(0f,1f)]
    public float TreeZoneTreshold = 0.6f;
    public float TreePlacementScale = 0.57f;
    [Range(0f, 1f)]
    public float TreePlacementTreshold = 0.8f;

    public int TreeMaxHeight = 96;
    public int TreeMinHeight = 40;
    [Header("Lodes")]
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
