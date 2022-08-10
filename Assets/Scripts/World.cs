using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform Player;
    public Vector3 Spawn;

    public int seed;

    private System.Random _random = new System.Random();

    public Material VoxelAtlas;
    public VoxelType[] VoxelTypes;

    Chunk[,] _chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> _activeChunks = new List<ChunkCoord>();
    ChunkCoord _playerLastChunkCoord;

    private void Start()
    {
        UnityEngine.Random.InitState(seed);
        GenerateWorld();
        _playerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);

    }

    private void Update()
    {

        //if (!GetChunkCoordFromVector3(Player.transform.position).Equals(_playerLastChunkCoord))
        //CheckViewDistance();

    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidthInVoxels);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidthInVoxels);
        return new ChunkCoord(x, z);

    }
    private void GenerateWorld()
    {

        for (int x = 0; x < VoxelData.WorldSizeInChunks; x++)
        {
            for (int z = 0; z < VoxelData.WorldSizeInChunks; z++)
            {

                CreateChunk(x, z);

            }
        }

        Spawn = new Vector3(VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize / 2, (VoxelData.ChunkHeightInVoxels) * VoxelData.VoxelSize + 2, VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize / 2);
        Player.position = Spawn;

    }

    private void CheckViewDistance()
    {

        int chunkX = Mathf.FloorToInt(Player.position.x / VoxelData.ChunkWidthInVoxels);
        int chunkZ = Mathf.FloorToInt(Player.position.z / VoxelData.ChunkWidthInVoxels);

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(_activeChunks);

        for (int x = chunkX - VoxelData.ViewDistanceInChunks / 2; x < chunkX + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = chunkZ - VoxelData.ViewDistanceInChunks / 2; z < chunkZ + VoxelData.ViewDistanceInChunks / 2; z++)
            {

                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(x, z))
                {

                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (_chunks[x, z] == null)
                        CreateChunk(thisChunk.x, thisChunk.z);
                    else if (!_chunks[x, z].isActive)
                    {
                        _chunks[x, z].isActive = true;
                        _activeChunks.Add(thisChunk);
                    }
                    // Check if this chunk was already in the active chunks list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {

                        //if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                            previouslyActiveChunks.RemoveAt(i);

                    }

                }
            }
        }

        foreach (ChunkCoord coord in previouslyActiveChunks)
            _chunks[coord.x, coord.z].isActive = false;

    }

    bool IsChunkInWorld(int x, int z)
    {

        if (x > 0 && x < VoxelData.WorldSizeInChunks - 1 && z > 0 && z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;

    }
    bool IsVoxelInWorld(Vector3 pos)
    {

        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize && pos.y >= 0 && pos.y < VoxelData.ChunkHeightInVoxels * VoxelData.VoxelSize && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize)
            return true;
        else
            return false;

    }
    private void CreateChunk(int x, int z)
    {

        _chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        _activeChunks.Add(new ChunkCoord(x, z));


    }
    public byte GetVoxel(Vector3 pos)
    {

        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }
        else if (pos.y < 1 * VoxelData.VoxelSize)
        {
            return 1;
        }
        else if (pos.y > (VoxelData.ChunkHeightInVoxels - 5) * VoxelData.VoxelSize)
        {
            float tempNoise = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, 1f);
            if (tempNoise < 1f / 3) return 8;
            else if (tempNoise < 2f / 3) return 9;
            else return 10;
        }
        else if (pos.y < (VoxelData.ChunkHeightInVoxels - 4) * VoxelData.VoxelSize && pos.y > (VoxelData.ChunkHeightInVoxels - 21) * VoxelData.VoxelSize)
        {
            return Convert.ToByte(_random.Next(5, 8));
        }
        else
        {
            return Convert.ToByte(_random.Next(2, 5));
        }

    }

}

[Serializable]
public class VoxelType
{
    public string VoxelName;
    public bool IsSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }
}

