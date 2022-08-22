using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noise
{
    public static float Get2DPerlin(Vector2 position, float offsetX,float offsetY,float scale)
    {
        return Mathf.PerlinNoise((position.x + 0.1f) / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize) * scale + offsetX, (position.y + 0.1f) / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize) * scale + offsetY);
    }
}
