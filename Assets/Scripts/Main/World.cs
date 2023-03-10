using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    #region Variables
    public Transform Player;
    [SerializeField]private Vector3 _spawn;
    [SerializeField]private NoiseSettings _multyOctaveNoiseSettings;
    
    [SerializeField]private int _seed;
    [SerializeField]private float _random;
    [Space]
    public Material VoxelAtlas;
    public Material VoxelAtlasTransparent;
    public Material Water;
    public VoxelType[] VoxelTypes;
    public BiomeAttributes Biome;

    Chunk[,] _chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];
    List<ChunkCoord> _activeChunks = new List<ChunkCoord>();
    public ChunkCoord PlayerChunkCoord;
    ChunkCoord _playerLastChunkCoord;
    List<ChunkCoord> _chunksToCreate = new List<ChunkCoord>();
    private bool _isCreatingChunks;
    #endregion
    private void Awake()
    {
        UnityEngine.Random.InitState(_seed);
        Debug.Log(UnityEngine.Random.value);
        Debug.Log(UnityEngine.Random.value);
        _random = UnityEngine.Random.value;
        Debug.Log(_random);
        _multyOctaveNoiseSettings = new NoiseSettings(Biome.TerrainScale, Biome.TerrainOctaves, new Vector2(_random, _random), new Vector2(-_random, -_random), Biome.TerrainPersistance, Biome.TerrainRedistributionModifier, Biome.Exponent);
        GenerateWorld();
        _playerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
    }

    private void Update()
    {
        PlayerChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
        if (!PlayerChunkCoord.Equals(_playerLastChunkCoord))
            CheckViewDistance();
        if(_chunksToCreate.Count > 0 && !_isCreatingChunks)
        {
            StartCoroutine("CreateChunks");
        }

    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        int z = Mathf.FloorToInt(pos.z / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        return new ChunkCoord(x, z);

    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        int z = Mathf.FloorToInt(pos.z / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        return _chunks[x, z];
    }
    private void GenerateWorld()
    {

        for (int x = (VoxelData.WorldSizeInChunks/2)-VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {

                _chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                _activeChunks.Add(new ChunkCoord(x, z));

            }
        }

        _spawn = new Vector3(VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize / 2, (VoxelData.ChunkHeightInVoxels - 16) * VoxelData.VoxelSize, VoxelData.WorldSizeInVoxels * VoxelData.VoxelSize / 2);
        Player.position = _spawn;

    }

    IEnumerator CreateChunks()
    {
        _isCreatingChunks = true;
        while(_chunksToCreate.Count > 0)
        {
            _chunks[_chunksToCreate[0].X, _chunksToCreate[0].Z].Initialize();
            _chunksToCreate.RemoveAt(0);
            yield return null;
        }
        _isCreatingChunks = false;
    }

    private void CheckViewDistance()
    {

        ChunkCoord coord = GetChunkCoordFromVector3(Player.position);

        _playerLastChunkCoord = PlayerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(_activeChunks);

        for (int x = coord.X - VoxelData.ViewDistanceInChunks; x < coord.X + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.Z - VoxelData.ViewDistanceInChunks; z < coord.Z + VoxelData.ViewDistanceInChunks; z++)
            {

                // If the chunk is within the world bounds and it has not been created.
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {

                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (_chunks[x, z] == null) { 
                        _chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                    _chunksToCreate.Add(new ChunkCoord(x, z));
                }
                else if (!_chunks[x, z].IsActive)
                {
                    _chunks[x, z].IsActive = true;
                }
                    _activeChunks.Add(thisChunk);
                    // Check if this chunk was already in the active chunks list.
                    for (int i = 0; i < previouslyActiveChunks.Count; i++)
                    {

                        if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                            previouslyActiveChunks.RemoveAt(i);

                    }

                }
            }
        }

        foreach (ChunkCoord chunkCoord in previouslyActiveChunks)
            _chunks[chunkCoord.X, chunkCoord.Z].IsActive = false;

    }

    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.X >= 0 && coord.X < VoxelData.WorldSizeInChunks && coord.Z >= 0 && coord.Z < VoxelData.WorldSizeInChunks)
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
    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk)||pos.y <0 ||pos.y>VoxelData.ChunkHeightInVoxels*VoxelData.VoxelSize)
            return false;
        if (_chunks[thisChunk.X, thisChunk.Z] != null && _chunks[thisChunk.X,thisChunk.Z].IsVoxelMapPopulated)
        {
            return VoxelTypes[_chunks[thisChunk.X, thisChunk.Z].GetVoxelFromGlobalVector3(pos)].IsSolid;
        }
        return VoxelTypes[GetVoxel(pos)].IsSolid;

        //int xCheck = Mathf.FloorToInt(x * Chunk.IncreaseToInt);
        //int yCheck = Mathf.FloorToInt(y * Chunk.IncreaseToInt);
        //int zCheck = Mathf.FloorToInt(z * Chunk.IncreaseToInt);

        //int xChunk = xCheck / VoxelData.ChunkWidthInVoxels;
        //int zChunk = zCheck / VoxelData.ChunkWidthInVoxels;

        //xCheck -= (xChunk * VoxelData.ChunkWidthInVoxels);
        //zCheck -= (zChunk * VoxelData.ChunkWidthInVoxels);

        //return VoxelTypes[_chunks[xChunk, zChunk].VoxelMap[xCheck, yCheck, zCheck]].IsSolid;
    }
    public bool CheckIfVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeightInVoxels * VoxelData.VoxelSize)
            return false;
        if (_chunks[thisChunk.X, thisChunk.Z] != null && _chunks[thisChunk.X, thisChunk.Z].IsVoxelMapPopulated)
        {
            return VoxelTypes[_chunks[thisChunk.X, thisChunk.Z].GetVoxelFromGlobalVector3(pos)].IsTransparent;
        }
        return VoxelTypes[GetVoxel(pos)].IsTransparent;
    }
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y * Chunk.IncreaseToInt);
        #region Imuttable Pass

        //If Outside the World, return air
        if (!IsVoxelInWorld(pos))
        {
            return 0;
        }
        // if BottomVoxel of chunk, return Azov
        if (yPos == 0)
        {
            return 1;
        }
        #endregion
        #region Basic Terrain Pass
        //int terrainHeight = (Mathf.FloorToInt((VoxelData.ChunkHeightInVoxels) * Noise.Get2DPerlin(new Vector2(pos.x, pos.z),Random,Random, 0.5f)));
        int terrainHeight = (Mathf.FloorToInt(Biome.TerrainHeight * MultyOctaveNoise.Redistribution(MultyOctaveNoise.GetOctavePerlin(pos.x, pos.z, _multyOctaveNoiseSettings), _multyOctaveNoiseSettings)))+ Biome.SolidGroundHeight;
        byte voxelValue;
        if (yPos <= terrainHeight && yPos >= terrainHeight - 3)
        {
            float tempNoise = (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), _random,_random, 0.5f));
            if (tempNoise < 1f / 3)
                voxelValue = 8;
            else if (tempNoise < 2f / 3)
                voxelValue = 9;
            else
                voxelValue = 10;
        }
        else if (yPos < terrainHeight - 3 && yPos >= terrainHeight - 12)
            voxelValue = 7;
        else if (yPos < terrainHeight - 12)
            voxelValue = 3;
        else
        {
            return 0;
        }
        #endregion
        #region Second Pass
        if (voxelValue == 3)
        {
            foreach (Lode lode in Biome.Lodes)
            {
                if (yPos > lode.MinHeight && yPos < lode.MaxHeight)
                {
                    if(MultyOctaveNoise.Get3DOctavePerlin(pos,lode.Scale, _multyOctaveNoiseSettings, _random / lode.Offset, lode.Threshold))
                    {
                        voxelValue = lode.VoxelID;
                    }
                }
            }
        }
        #endregion
        return voxelValue;
    }

}

[Serializable]
public class VoxelType
{
    public string VoxelName;
    public bool IsSolid;
    public bool IsTransparent;
    public Sprite icon;

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

