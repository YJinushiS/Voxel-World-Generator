using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NoiseSettings
{
    public float NoiseZoom;
    public int Octaves;
    public Vector2 Offset;
    public Vector2 WorldOffset;
    public float Persistance;
    public float RedistributionModifier;
    public float Exponent;
    public NoiseSettings(float noiseZoom, int octaves, Vector2 offset, Vector2 worldOffset, float persistance, float redistributionModifier, float exponent)
    {
        Exponent = exponent;
        NoiseZoom = noiseZoom;
        Octaves = octaves;
        Offset = offset;
        WorldOffset = worldOffset;
        Persistance = persistance;
        RedistributionModifier = redistributionModifier;
    } 
}
