using System;
using System.Threading;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Chunk
{
    #region Variables

    public ChunkCoord Coord;
    MeshRenderer _chunkMeshRenderer;
    MeshFilter _chunkMeshFilter;

    GameObject _chunkObject;
    int _vertexIndex = 0;
    private System.Random _random = new();
    public static float VoxelSize = VoxelData.VoxelSize;
    public static int IncreaseToInt = Convert.ToInt32(1 / VoxelSize);
    List<Vector3> _vertices = new();
    List<int> _triangles = new();
    List<int> _transparentTriangles = new();
    Material[] _voxelAtlases = new Material[2];
    List<Vector2> _uvs = new();

    public byte[] VoxelMap =
        new byte[VoxelData.ChunkWidthInVoxels * VoxelData.ChunkHeightInVoxels * VoxelData.ChunkWidthInVoxels];

    public Queue<VoxelMod> Modifications = new();

    public World WorldObj;

    public Vector3 Position;

    private bool _isActive;
    private bool _isVoxelMapPopulated = false;
    private bool _threadLocked = false;

    private static ProfilerMarker _meshingMarker = new(ProfilerCategory.Loading, "Meshing");

    private static ProfilerMarker _generatingVoxelsInChunkMarker =
        new(ProfilerCategory.Loading, "Generating Voxels In Chunk");

    #endregion

    public Chunk(ChunkCoord coord, World world, bool generateOnLoad)
    {
        Coord = coord;
        WorldObj = world;

        IsActive = true;

        if (generateOnLoad)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        _chunkObject = new GameObject();
        _chunkObject.transform.position = new Vector3((Coord.X * VoxelData.ChunkWidthInVoxels) * VoxelSize,
            (Coord.Y * VoxelData.ChunkHeightInVoxels), (Coord.Z * VoxelData.ChunkWidthInVoxels) * VoxelSize);
        _chunkMeshFilter = _chunkObject.AddComponent<MeshFilter>();
        _chunkMeshRenderer = _chunkObject.AddComponent<MeshRenderer>();

        _voxelAtlases[0] = WorldObj.VoxelAtlas;
        _voxelAtlases[1] = WorldObj.VoxelAtlasTransparent;

        _chunkMeshRenderer.materials = _voxelAtlases;

        _chunkObject.transform.SetParent(WorldObj.transform);
        Position = _chunkObject.transform.position;

        Thread secondaryThread = new Thread(new ThreadStart(PopulateVoxelMap));
        secondaryThread.Start();
    }

    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (_chunkObject != null)
                _chunkObject.SetActive(value);
        }
    }

    public bool IsEditable
    {
        get
        {
            if (!_isVoxelMapPopulated || _threadLocked)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > VoxelData.ChunkWidthInVoxels - 1 * VoxelData.VoxelSize || y < 0 ||
            y > VoxelData.ChunkHeightInVoxels - 1 * VoxelData.VoxelSize || z < 0 ||
            z > VoxelData.ChunkWidthInVoxels - 1 * VoxelData.VoxelSize)
            return false;
        else return true;
    }

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(Position.x);
        yCheck -= Mathf.FloorToInt(Position.y);
        zCheck -= Mathf.FloorToInt(Position.z);
        int index = xCheck + yCheck * VoxelData.ChunkHeightSq + zCheck * VoxelData.ChunkWidthInVoxels;
        VoxelMap[index] = newID;
        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        PrivateUpdateChunk();
    }

    public void EditVoxelWithoutUpdate(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(Position.x);
        yCheck -= Mathf.FloorToInt(Position.y);
        zCheck -= Mathf.FloorToInt(Position.z);
        int index = xCheck + yCheck * VoxelData.ChunkHeightSq + zCheck * VoxelData.ChunkWidthInVoxels;
        VoxelMap[index] = newID;
        //UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
    }

    public void EditVoxelsInSphere(Vector3 pos, int radius, byte newID) //Need Optimization
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(Position.x);
        yCheck -= Mathf.FloorToInt(Position.y);
        zCheck -= Mathf.FloorToInt(Position.z);

        for (int x = -radius; x <= radius; x++)
        for (int y = -radius; y <= radius; y++)
        for (int z = -radius; z <= radius; z++)
        {
            Vector3 newPos = new(xCheck, yCheck, zCheck);
            newPos = AddVoxelInRadius(newPos, x, y, z);
            if (Vector3.Distance(new(xCheck, yCheck, zCheck), newPos) <= (float)radius)
            {
                if ((newPos.x >= 0 && newPos.x < VoxelData.ChunkWidthInVoxels) &&
                    (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) &&
                    (newPos.z >= 0 && newPos.z < VoxelData.ChunkWidthInVoxels))
                {
                    int index = (int)(newPos.x) + (int)(newPos.y) * VoxelData.ChunkHeightSq +
                                (int)(newPos.z) * VoxelData.ChunkWidthInVoxels;
                    VoxelMap[index] = newID;
                    //UpdateSurroundingVoxels((int)newPos.x, (int)newPos.y, (int)newPos.z);
                }
                else
                {
                    newPos = new Vector3(newPos.x + Mathf.FloorToInt(Position.x),
                        newPos.y + Mathf.FloorToInt(Position.y), newPos.z + Mathf.FloorToInt(Position.z));
                    WorldObj.GetChunkFromVector3(newPos).EditVoxelWithoutUpdate(newPos, newID);
                }
            }
        }

        UpdateChunk();

        #region Top Chunk Layer

        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y + VoxelData.ChunkHeightInVoxels,
            pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y + VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y + VoxelData.ChunkHeightInVoxels,
            pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y + VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();

        #endregion

        #region Medium Chunk Layer

        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y,
            pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y,
            pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y,
            pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y,
            pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();

        #endregion

        #region Bottom Chunk Layer

        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y - VoxelData.ChunkHeightInVoxels,
            pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels,
            pos.y - VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y - VoxelData.ChunkHeightInVoxels,
            pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
        WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y - VoxelData.ChunkHeightInVoxels, pos.z)).UpdateChunk();

        #endregion
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new(x, y, z);
        for (int j = 0; j < 6; j++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.FaceCheck[j];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                WorldObj.GetChunkFromVector3(currentVoxel + Position).UpdateChunk();
            }
        }
    }

    public Vector3 AddVoxelInRadius(Vector3 pos, int x, int y, int z)
    {
        Vector3 gridPoint = new(Mathf.FloorToInt(pos.x + x), Mathf.FloorToInt(pos.y + y), Mathf.FloorToInt(pos.z + z));
        return gridPoint;
    }

    void PopulateVoxelMap()
    {
        _generatingVoxelsInChunkMarker.Begin();
        for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    int index = x + y * VoxelData.ChunkHeightSq + z * VoxelData.ChunkWidthInVoxels;
                    VoxelMap[index] =
                        WorldObj.GetVoxel(new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize) + Position);
                }
            }
        }

        PrivateUpdateChunk();
        _isVoxelMapPopulated = true;
        _generatingVoxelsInChunkMarker.End();
    }

    public void UpdateChunk()
    {
        Thread secondaryThread = new Thread(new ThreadStart(PrivateUpdateChunk));
        secondaryThread.Start();
    }

    private void PrivateUpdateChunk()
    {
        _meshingMarker.Begin();
        _threadLocked = true;
        while (Modifications.Count > 0)
        {
            VoxelMod v = Modifications.Dequeue();
            Vector3 position = v.Position -= Position;
            int index = (int)(position.x) + (int)(position.y) * VoxelData.ChunkHeightSq +
                        (int)(position.z) * VoxelData.ChunkWidthInVoxels;
            VoxelMap[index] = v.ID;
        }

        ClearMeshData();
        for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
        {
            for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    int index = x + y * VoxelData.ChunkHeightSq + z * VoxelData.ChunkWidthInVoxels;
                    if (WorldObj.VoxelTypes[VoxelMap[index]].IsSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        lock (WorldObj.ChunksToDraw)
        {
            WorldObj.ChunksToDraw.Enqueue(this);
        }

        _threadLocked = false;
        _meshingMarker.End();
        _generatingVoxelsInChunkMarker.End();
    }

    void ClearMeshData()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _transparentTriangles.Clear();
        _uvs.Clear();
    }

    public byte GetVoxelFromMap(Vector3 pos)
    {
        pos -= Position;
        int index = (int)(pos.x) + (int)(pos.y) * VoxelData.ChunkHeightSq + (int)(pos.z) * VoxelData.ChunkWidthInVoxels;
        return VoxelMap[index];
    }

    bool CheckVoxelIsTransparent(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return WorldObj.CheckIfVoxelTransparent(pos + Position);
        int index = x + y * VoxelData.ChunkHeightSq + z * VoxelData.ChunkWidthInVoxels;
        return WorldObj.VoxelTypes[VoxelMap[index]].IsTransparent;
    }

    bool CheckVoxelIsSolid(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return WorldObj.CheckForVoxel(pos + Position);
        int index = x + y * VoxelData.ChunkHeightSq + z * VoxelData.ChunkWidthInVoxels;
        return WorldObj.VoxelTypes[VoxelMap[index]].IsSolid;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(Position.x);
        yCheck -= Mathf.FloorToInt(Position.y);
        zCheck -= Mathf.FloorToInt(Position.z);
        int index = xCheck + yCheck * VoxelData.ChunkHeightSq + zCheck * VoxelData.ChunkWidthInVoxels;
        return VoxelMap[index];
    }

    void UpdateMeshData(Vector3 pos)
    {
        int index = (int)(pos.x) + (int)(pos.y) * VoxelData.ChunkHeightSq + (int)(pos.z) * VoxelData.ChunkWidthInVoxels;
        byte voxelID = VoxelMap[index];
        bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
        for (int j = 0; j < 6; j++)
        {
            Vector3 faceCheck = pos + VoxelData.FaceCheck[j] + Position;
            if (j == 2 && !WorldObj.IsVoxelInWorld(faceCheck))
            {
            }
            else if ((CheckVoxelIsTransparent(pos) != CheckVoxelIsTransparent(pos + VoxelData.FaceCheck[j]) ||
                      !CheckVoxelIsSolid(pos + VoxelData.FaceCheck[j])) &&
                     WorldObj.IsVoxelInWorld(faceCheck))
            {
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 3]]);
                AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(j));
                if (!isTransparent)
                {
                    _triangles.Add(_vertexIndex);
                    _triangles.Add(_vertexIndex + 1);
                    _triangles.Add(_vertexIndex + 2);
                    _triangles.Add(_vertexIndex + 2);
                    _triangles.Add(_vertexIndex + 1);
                    _triangles.Add(_vertexIndex + 3);
                }
                else
                {
                    _transparentTriangles.Add(_vertexIndex);
                    _transparentTriangles.Add(_vertexIndex + 1);
                    _transparentTriangles.Add(_vertexIndex + 2);
                    _transparentTriangles.Add(_vertexIndex + 2);
                    _transparentTriangles.Add(_vertexIndex + 1);
                    _transparentTriangles.Add(_vertexIndex + 3);
                }

                _vertexIndex += 4;
            }
        }
    }

    public void CreateMesh()
    {
        Mesh mesh = new();
        mesh.vertices = _vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(_triangles.ToArray(), 0);
        mesh.SetTriangles(_transparentTriangles.ToArray(), 1);

        mesh.uv = _uvs.ToArray();

        mesh.RecalculateNormals();
        UnityEngine.Object.Destroy(_chunkMeshFilter.sharedMesh);
        _chunkMeshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.VoxelAtlasSize;
        float x = textureID - (y * VoxelData.VoxelAtlasSize);

        x *= VoxelData.NormalizedVoxelTextureSizeInAtlas;
        y *= VoxelData.NormalizedVoxelTextureSizeInAtlas;

        y = 1f - y - VoxelData.NormalizedVoxelTextureSizeInAtlas;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + VoxelData.NormalizedVoxelTextureSizeInAtlas));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedVoxelTextureSizeInAtlas, y));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedVoxelTextureSizeInAtlas,
            y + VoxelData.NormalizedVoxelTextureSizeInAtlas));
    }
}

public class ChunkCoord
{
    public int X;
    public int Y;
    public int Z;

    public ChunkCoord()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }

    public ChunkCoord(int _x, int _y, int _z)
    {
        X = _x;
        Y = _y;
        Z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        X = xCheck / VoxelData.ChunkWidthInVoxels;
        Y = yCheck / VoxelData.ChunkHeightInVoxels;
        Z = zCheck / VoxelData.ChunkWidthInVoxels;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z && other.Y == Y)
            return true;
        else
            return false;
    }
}