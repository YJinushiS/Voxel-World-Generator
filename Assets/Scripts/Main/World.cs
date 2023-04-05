using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
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

    Chunk[,,] _chunks = new Chunk[VoxelData.WorldWidthInChunks, VoxelData.WorldHeightInChunks, VoxelData.WorldWidthInChunks];
    
    List<ChunkCoord> _activeChunks = new();
    public ChunkCoord PlayerChunkCoord;
    ChunkCoord _playerLastChunkCoord;

    List<ChunkCoord> _chunksToCreate = new();
    List<Chunk> _chunksToUpdate = new();
    public Queue<Chunk> ChunksToDraw = new();


    private bool _isCreatingChunks;

    bool _applyingModifications = false;

    Queue<Queue<VoxelMod>> _modifications = new();

    private static ProfilerMarker _generatingMarker = new(ProfilerCategory.Loading, "Generating");
    #endregion
    private void Awake()
    {
        UnityEngine.Random.InitState(_seed);
        Debug.Log(UnityEngine.Random.value);
        Debug.Log(UnityEngine.Random.value);
        _random = UnityEngine.Random.value;
        Debug.Log(_random);
        _multyOctaveNoiseSettings = new NoiseSettings(Biome.TerrainScale, Biome.TerrainOctaves, new Vector2(_random, _random), new Vector2(-_random, -_random), Biome.TerrainPersistance, Biome.TerrainRedistributionModifier, Biome.Exponent, Biome.TreeZoneScale, Biome.TreeZoneTreshold);
        GenerateWorld();
        _playerLastChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
    }

    private void Update()
    {
        PlayerChunkCoord = GetChunkCoordFromVector3(Player.transform.position);
        if (!PlayerChunkCoord.Equals(_playerLastChunkCoord))
            CheckViewDistance();

        if (!_applyingModifications)
            ApplyModifications();

        if (_chunksToCreate.Count > 0 )
        {
            CreateChunks();
        }

        if (_chunksToUpdate.Count> 0)
        {
            UpdateChunk();
        }

        if(ChunksToDraw.Count > 0)
        {
            lock (ChunksToDraw)
            {
                if (ChunksToDraw.Peek().IsEditable)
                {
                    ChunksToDraw.Dequeue().CreateMesh();
                }
            }
        }

    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        int y = Mathf.FloorToInt(pos.y / (VoxelData.ChunkHeightInVoxels * VoxelData.VoxelSize));
        int z = Mathf.FloorToInt(pos.z / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        return new ChunkCoord(x, y, z);

    }
    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        int y = Mathf.FloorToInt(pos.y / (VoxelData.ChunkHeightInVoxels * VoxelData.VoxelSize));
        int z = Mathf.FloorToInt(pos.z / (VoxelData.ChunkWidthInVoxels * VoxelData.VoxelSize));
        return _chunks[x, y, z];
    }
    private void GenerateWorld()
    {
        _generatingMarker.Begin();
        for (int x = (VoxelData.WorldWidthInChunks/2)-VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldWidthInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int y = (VoxelData.WorldHeightInChunks / 2) - VoxelData.ViewDistanceInChunks; y < (VoxelData.WorldHeightInChunks / 2) + VoxelData.ViewDistanceInChunks; y++)
            {
                for (int z = (VoxelData.WorldWidthInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldWidthInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
                {

                    _chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this, true);
                    _activeChunks.Add(new ChunkCoord(x, y, z));

                }
            }
        }

        _spawn = new Vector3(VoxelData.WorldWidthInVoxels * VoxelData.VoxelSize / 2, 72, VoxelData.WorldWidthInVoxels * VoxelData.VoxelSize / 2);
        Player.position = _spawn;
        _generatingMarker.End();
        Debug.Log("Generated");
    }

    void CreateChunks()
    {
        ChunkCoord c = _chunksToCreate[0];
        _chunksToCreate.RemoveAt(0);
        _activeChunks.Add(c);
        _chunks[c.X, c.Y, c.Z].Initialize();
    }
    void UpdateChunk()
    {
        bool updated = false;
        int index = 0;

        while(!updated && index < _chunksToUpdate.Count - 1)
        {
            if (_chunksToUpdate[index].IsEditable)
            {
                _chunksToUpdate[index].UpdateChunk();
                _chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else { index++; }
        }
    }

    void ApplyModifications()
    {

        _applyingModifications = true;

        while (_modifications.Count > 0)
        {
            Queue<VoxelMod> queue = _modifications.Dequeue();
            while (queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.Position);

                if (_chunks[c.X, c.Y, c.Z] == null)
                {
                    _chunks[c.X, c.Y, c.Z] = new Chunk(c, this, true);
                    _activeChunks.Add(c);
                }

                _chunks[c.X, c.Y, c.Z].Modifications.Enqueue(v);

                if (!_chunksToUpdate.Contains(_chunks[c.X, c.Y, c.Z]))
                    _chunksToUpdate.Add(_chunks[c.X, c.Y, c.Z]);

            }
        }

        _applyingModifications = false;

    }

    private void CheckViewDistance()
    {

        ChunkCoord coord = GetChunkCoordFromVector3(Player.position);

        _playerLastChunkCoord = PlayerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new(_activeChunks);

        for (int x = coord.X - VoxelData.ViewDistanceInChunks; x < coord.X + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int y = coord.Y - VoxelData.ViewDistanceInChunks; y < coord.Y + VoxelData.ViewDistanceInChunks; y++)
            {
                for (int z = coord.Z - VoxelData.ViewDistanceInChunks; z < coord.Z + VoxelData.ViewDistanceInChunks; z++)
                {

                    // If the chunk is within the world bounds and it has not been created.
                    if (IsChunkInWorld(new ChunkCoord(x, y, z)))
                    {

                        ChunkCoord thisChunk = new(x, y, z);

                        if (_chunks[x, y, z] == null)
                        {
                            _chunks[x, y, z] = new Chunk(new ChunkCoord(x, y, z), this, false);
                            _chunksToCreate.Add(new ChunkCoord(x, y, z));
                        }
                        else if (!_chunks[x, y, z].IsActive)
                        {
                            _chunks[x, y, z].IsActive = true;
                        }
                        _activeChunks.Add(thisChunk);
                        // Check if this chunk was already in the active chunks list.
                        for (int i = 0; i < previouslyActiveChunks.Count; i++)
                        {

                            if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, y, z)))
                                previouslyActiveChunks.RemoveAt(i);

                        }

                    }
                }
            }
        }

        foreach (ChunkCoord chunkCoord in previouslyActiveChunks)
            _chunks[chunkCoord.X, chunkCoord.Y, chunkCoord.Z].IsActive = false;

    }

    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.X >= 0 && coord.X < VoxelData.WorldWidthInChunks && coord.Y >= 0 && coord.Y < VoxelData.WorldHeightInChunks && coord.Z >= 0 && coord.Z < VoxelData.WorldWidthInChunks)
            return true;
        else
            return false;

    }
    public bool IsVoxelInWorld(Vector3 pos)
    {

        if (pos.x >= 0 && pos.x < VoxelData.WorldWidthInVoxels && pos.y >= 0 && pos.y < VoxelData.WorldHeightInVoxels && pos.z >= 0 && pos.z < VoxelData.WorldWidthInVoxels)
            return true;
        else
            return false;

    }
    public bool CheckForVoxel(Vector3 pos)
    { //maybe error
        ChunkCoord thisChunk = new(pos);
        if (!IsChunkInWorld(thisChunk))
            return false;
        if (_chunks[thisChunk.X, thisChunk.Y, thisChunk.Z] != null && _chunks[thisChunk.X, thisChunk.Y, thisChunk.Z].IsEditable)
        {
            return VoxelTypes[_chunks[thisChunk.X, thisChunk.Y, thisChunk.Z].GetVoxelFromGlobalVector3(pos)].IsSolid;
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
        ChunkCoord thisChunk = new(pos);
        if (!IsChunkInWorld(thisChunk))
            return false;
        if (_chunks[thisChunk.X,thisChunk.Y, thisChunk.Z] != null && _chunks[thisChunk.X, thisChunk.Y, thisChunk.Z].IsEditable)
        {
            return VoxelTypes[_chunks[thisChunk.X, thisChunk.Y, thisChunk.Z].GetVoxelFromGlobalVector3(pos)].IsTransparent;
        }
        return VoxelTypes[GetVoxel(pos)].IsTransparent;
    }
    public byte GetVoxel(Vector3 position)
    {
        int yPos = Mathf.FloorToInt((position.y) * Chunk.IncreaseToInt);
        #region Imuttable Pass

        //If Outside the World, return air
        if (!IsVoxelInWorld(position))
        {
            return 0;
        }
        // if BottomVoxel of World, return Azov
        if (yPos == 0)
        {
            return 1;
        }
        #endregion
        #region Basic Terrain Pass
        //int terrainHeight = (Mathf.FloorToInt((VoxelData.ChunkHeightInVoxels) * Noise.Get2DPerlin(new Vector2(pos.x, pos.z),Random,Random, 0.5f)));
        int terrainHeight = (Mathf.FloorToInt(Biome.TerrainHeight * MultyOctaveNoise.Redistribution(MultyOctaveNoise.GetOctavePerlin(position.x, position.z, _multyOctaveNoiseSettings), _multyOctaveNoiseSettings)))+ Biome.SolidGroundHeight;
        byte voxelValue;
        if (yPos <= terrainHeight && yPos >= terrainHeight - 3)
        {
            float tempNoise = (Noise.Get2DPerlin(new Vector2(position.x, position.z), _random,_random, 0.5f));
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
                    if(MultyOctaveNoise.Get3DOctavePerlin(position,lode.Scale, _multyOctaveNoiseSettings, _random / lode.Offset, lode.Threshold))
                    {
                        voxelValue = lode.VoxelID;
                    }
                }
            }
        }
        #endregion
        #region Tree Pass
        if (yPos == terrainHeight)
        {
            if(MultyOctaveNoise.GetTreeZoneOctavePerlin(position.x, position.z, _multyOctaveNoiseSettings) > Biome.TreeZoneTreshold)
            {
                voxelValue = 1;
                if(MultyOctaveNoise.GetTreePlacementOctavePerlin(position.x, position.z, _multyOctaveNoiseSettings) > Biome.TreePlacementTreshold)
                {
                    _modifications.Enqueue(Structure.MakeTree(position, Biome.TreeMinHeight, Biome.TreeMaxHeight, _multyOctaveNoiseSettings));
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
                return bottomFaceTexture;
            case 3:
                return topFaceTexture;
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

public class VoxelMod
{
    public Vector3 Position;
    public byte ID;

    public VoxelMod ()
    {
        Position = new Vector3();
        ID = 0;
    }
    public VoxelMod (Vector3 position, byte id)
    {
        Position = position;
        ID = id;
    }
}

