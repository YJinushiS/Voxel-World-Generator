using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class VoxelData
{
    public static readonly int ChunkHeightInVoxels = 32;
    public static readonly int ChunkWidthInVoxels = 32;
    public static readonly int WorldWidthInChunks = 40;
    public static readonly int WorldHeightInChunks = 20;
    public static readonly int ViewDistanceInChunks = 4;
    public static int WorldWidthInVoxels
    {

        get { return WorldWidthInChunks * ChunkWidthInVoxels; }

    }
    public static int WorldHeightInVoxels
    {

        get { return WorldHeightInChunks * ChunkHeightInVoxels; }

    }

    public static float VoxelSize = 1f;
    public static readonly int VoxelAtlasSize = 256;
    public static float NormalizedVoxelTextureSizeInAtlas { get { return 1f / (float)VoxelAtlasSize; } }

    public static readonly Vector3[] VoxelVerts = new Vector3[8]
    {
        new Vector3(0, 0, 0), new Vector3(VoxelSize, 0,0),
        new Vector3(VoxelSize, VoxelSize,0), new Vector3(0,VoxelSize,0),
        new Vector3(0, 0, VoxelSize), new Vector3(VoxelSize, 0,VoxelSize),
        new Vector3(VoxelSize, VoxelSize,VoxelSize), new Vector3(0,VoxelSize,VoxelSize)

    };
    public static readonly Vector3[] FaceCheck = new Vector3[6]
    {
        new Vector3(0,0,-VoxelSize),
        new Vector3(0,0,VoxelSize),
        new Vector3(0,VoxelSize,0),
        new Vector3(0,-VoxelSize,0),
        new Vector3(-VoxelSize,0,0),
        new Vector3(VoxelSize,0,0)
    };
    public static readonly long[,] VoxelTris = new long[6, 4]
    {
      // 0  1  2  2  1  3
        {0, 3, 1, 2}, // Back Face
		{5, 6, 4, 7}, // Front Face
		{3, 7, 2, 6}, // Top Face
		{1, 5, 0, 4}, // Bottom Face
		{4, 7, 0, 3}, // Left Face
		{1, 2, 5, 6}  // Right Face

    };
    public static readonly Vector2[] VoxelUvs = new Vector2[4] {

        new Vector2 (0.0f, 0.0f),
        new Vector2 (0.0f, 1.0f),
        new Vector2 (1.0f, 0.0f),
        new Vector2 (1.0f, 1.0f)

    };
}
