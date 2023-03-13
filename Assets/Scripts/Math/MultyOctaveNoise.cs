using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultyOctaveNoise
{
    public static float RemapValue(float value, float initialMin, float initialMax, float outputMin, float outputMax)
    {
        return outputMin + (value - initialMin) * (outputMax - outputMin) / (initialMax - initialMin);
    }

    public static float RemapValue01(float value, float outputMin, float outputMax)
    {
        return outputMin + (value - 0) * (outputMax - outputMin) / (1 - 0);
    }

    public static int RemapValue01ToInt(float value, float outputMin, float outputMax)
    {
        return (int)RemapValue01(value, outputMin, outputMax);
    }

    public static float Redistribution(float noise, NoiseSettings settings)
    {
        return Mathf.Pow(noise * settings.RedistributionModifier, settings.Exponent);
    }

    public static float GetOctavePerlin(float x, float z, NoiseSettings settings)
    {
        x *= settings.NoiseScale;
        z *= settings.NoiseScale;
        x += settings.NoiseScale;
        z += settings.NoiseScale;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < settings.Octaves; i++)
        {
            total += Mathf.PerlinNoise((settings.Offset.x + settings.WorldOffset.x + x) * frequency, (settings.Offset.y + settings.WorldOffset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= settings.Persistance;
            frequency *= 2;
        }

        return total / amplitudeSum;
    }
    public static bool Get3DOctavePerlin(Vector3 position, float scale,NoiseSettings settings,float offset, float threshold)
    {
        float x = (position.x + offset + 0.1f) * scale;
        float y = (position.y + offset + 0.1f) * scale;
        float z = (position.z + offset + 0.1f) * scale;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);
        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold)
            return true;
        else return false;
    }

    public static float GetTreeZoneOctavePerlin(float x, float z, NoiseSettings settings)
    {
        x *= settings.TreeZoneScale;
        z *= settings.TreeZoneScale;
        x += settings.TreeZoneScale;
        z += settings.TreeZoneScale;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < 1; i++)
        {
            total += Mathf.PerlinNoise((settings.Offset.x + settings.WorldOffset.x + x) * frequency, (settings.Offset.y + settings.WorldOffset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= 1;
            frequency *= 2;
        }

        return total / amplitudeSum;
    }
    public static float GetTreePlacementOctavePerlin(float x, float z, NoiseSettings settings)
    {
        x *= settings.TreePlacementScale;
        z *= settings.TreePlacementScale;
        x += settings.TreePlacementScale;
        z += settings.TreePlacementScale;

        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float amplitudeSum = 0;  // Used for normalizing result to 0.0 - 1.0 range
        for (int i = 0; i < 1; i++)
        {
            total += Mathf.PerlinNoise((settings.Offset.x + settings.WorldOffset.x + x) * frequency, (settings.Offset.y + settings.WorldOffset.y + z) * frequency) * amplitude;

            amplitudeSum += amplitude;

            amplitude *= 1;
            frequency *= 2;
        }

        return total / amplitudeSum;
    }
}
