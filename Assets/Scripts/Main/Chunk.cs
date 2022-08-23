using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    #region Variables
    public ChunkCoord Coord;
    MeshRenderer _chunkMeshRenderer;
    MeshFilter _chunkMeshFilter;

    GameObject _chunkObject;

    int _vertexIndex = 0;
    private System.Random _random = new System.Random();
    public static float VoxelSize = VoxelData.VoxelSize;
    public static int IncreaseToInt = Convert.ToInt32(1 / VoxelSize);
    List<Vector3> _vertices = new List<Vector3>();
    List<int> _triangles = new List<int>();
    List<Vector2> _uvs = new List<Vector2>();

    public byte[,,] VoxelMap = new byte[VoxelData.ChunkWidthInVoxels, VoxelData.ChunkHeightInVoxels, VoxelData.ChunkWidthInVoxels];

    public World WorldObj;

    private bool _isActive;
    public bool IsVoxelMapPopulated = false;
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
        _chunkObject.transform.position = new Vector3((Coord.X * VoxelData.ChunkWidthInVoxels) * VoxelSize, 0f, (Coord.Z * VoxelData.ChunkWidthInVoxels) * VoxelSize);
        _chunkMeshFilter = _chunkObject.AddComponent<MeshFilter>();
        _chunkMeshRenderer = _chunkObject.AddComponent<MeshRenderer>();

        _chunkMeshRenderer.material = WorldObj.VoxelAtlas;
        _chunkObject.transform.SetParent(WorldObj.transform);

        PopulateVoxelMap();
        UpdateChunk();
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
    Vector3 position
    {

        get { return _chunkObject.transform.position; }

    }
    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidthInVoxels - 1 * VoxelData.VoxelSize || y < 0 || y > VoxelData.ChunkHeightInVoxels - 1 * VoxelData.VoxelSize || z < 0 || z > VoxelData.ChunkWidthInVoxels - 1 * VoxelData.VoxelSize)
            return false;
        else return true;

    }
    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        VoxelMap[xCheck, yCheck, zCheck] = newID;
        UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        UpdateChunk();
    }
    void UpdateSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
        for (int j = 0; j < 6; j++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.FaceCheck[j];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                WorldObj.GetChunkFromVector3(currentVoxel + position).UpdateChunk();
            }
        }

    }
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    VoxelMap[x, y, z] = WorldObj.GetVoxel(new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize) + position);
                }
            }
        }
        IsVoxelMapPopulated = true;
    }
    void UpdateChunk()
    {
        ClearMeshData();
        for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (WorldObj.VoxelTypes[VoxelMap[x, y, z]].IsSolid)
                        UpdateMeshData(new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize));
                }
            }
        }
        CreateMesh();
    }
    void ClearMeshData()
    {
        _vertexIndex = 0;
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
    }
    public byte GetVoxelFromMap(Vector3 pos)
    {

        pos -= position;

        return VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

    }
    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x * IncreaseToInt);
        int y = Mathf.FloorToInt(pos.y * IncreaseToInt);
        int z = Mathf.FloorToInt(pos.z * IncreaseToInt);

        if (!IsVoxelInChunk(x, y, z))
            return WorldObj.CheckForVoxel(pos + position);

        return WorldObj.VoxelTypes[VoxelMap[x, y, z]].IsSolid;

    }
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        return VoxelMap[xCheck, yCheck, zCheck];
    }
    void UpdateMeshData(Vector3 pos)
    {
        for (int j = 0; j < 6; j++)
        {
            if (!CheckVoxel(pos + VoxelData.FaceCheck[j]))
            {
                byte voxelID = VoxelMap[(int)(pos.x * IncreaseToInt), (int)(pos.y * IncreaseToInt), (int)(pos.z * IncreaseToInt)];

                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 3]]);
                AddTexture(WorldObj.VoxelTypes[voxelID].GetTextureID(j));
                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
                _vertexIndex += 4;
            }

        }
    }

    void CreateMesh()
    {


        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.uv = _uvs.ToArray();

        mesh.RecalculateNormals();

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
        _uvs.Add(new Vector2(x + VoxelData.NormalizedVoxelTextureSizeInAtlas, y + VoxelData.NormalizedVoxelTextureSizeInAtlas));
    }
}
public class ChunkCoord
{

    public int X;
    public int Z;

    public ChunkCoord()
    {
        X = 0;
        Z = 0;
    }
    public ChunkCoord(int _x, int _z)
    {

        X = _x;
        Z = _z;

    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        X = xCheck / VoxelData.ChunkWidthInVoxels;
        Z = zCheck / VoxelData.ChunkWidthInVoxels;
    }

    public bool Equals(ChunkCoord other)
    {

        if (other == null)
            return false;
        else if (other.X == X && other.Z == Z)
            return true;
        else
            return false;

    }
}