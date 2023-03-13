using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoiseSettings
{
    public float NoiseScale = 0.01f;
    public int Octaves = 10;
    public Vector2 Offset = Vector2.zero;
    public Vector2 WorldOffset = Vector2.zero;
    public float Persistance = 0.5f;
    public float RedistributionModifier = 1.2f;
    public float Exponent = 5f;

    public float TreeZoneScale = 0.05f;
    public float TreeZoneTreshold = 0.6f;

    public float TreePlacementScale = 0.57f;
    public float TreePlacementTreshold = 0.8f;
    public NoiseSettings(float noiseScale, int octaves, Vector2 offset, Vector2 worldOffset, float persistance, float redistributionModifier, float exponent, float treeZoneScale, float treeZoneTreshold)
    {
        Exponent = exponent;
        NoiseScale = noiseScale;
        Octaves = octaves;
        Offset = offset;
        WorldOffset = worldOffset;
        Persistance = persistance;
        RedistributionModifier = redistributionModifier;
        TreeZoneScale = treeZoneScale;
        TreeZoneTreshold = treeZoneTreshold;
    } 
}
