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
    List<int> _transparentTriangles = new List<int>();
    Material[] _voxelAtlases = new Material[2];
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
        _chunkObject.transform.position = new Vector3((Coord.X * VoxelData.ChunkWidthInVoxels) * VoxelSize, (Coord.Y * VoxelData.ChunkHeightInVoxels), (Coord.Z * VoxelData.ChunkWidthInVoxels) * VoxelSize);
        _chunkMeshFilter = _chunkObject.AddComponent<MeshFilter>();
        _chunkMeshRenderer = _chunkObject.AddComponent<MeshRenderer>();

        _voxelAtlases[0] = WorldObj.VoxelAtlas;
        _voxelAtlases[1] = WorldObj.VoxelAtlasTransparent;

        _chunkMeshRenderer.materials = _voxelAtlases;

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
        yCheck -= Mathf.FloorToInt(_chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        VoxelMap[xCheck, yCheck, zCheck] = newID;
        //UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        UpdateChunk();
    }
    public void EditVoxelWithoutUpdate(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(_chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        VoxelMap[xCheck, yCheck, zCheck] = newID;
        //UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
    }
    public void EditVoxelsInSphere(Vector3 pos, int radius, byte newID) //Need Optimization
    {
        int updateChunkVariant = 0;
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3 newPos = new Vector3(xCheck, yCheck, zCheck);
                    newPos = AddVoxelInRadius(newPos, x, y, z);
                    if (Vector3.Distance(new Vector3(xCheck, yCheck, zCheck), newPos) <= (float)radius)
                    {
                        if ((newPos.x >= 0 && newPos.x < VoxelData.ChunkWidthInVoxels) && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && (newPos.z >= 0 && newPos.z < VoxelData.ChunkWidthInVoxels))
                        {
                            VoxelMap[(int)newPos.x, (int)newPos.y, (int)newPos.z] = newID;
                            //UpdateSurroundingVoxels((int)newPos.x, (int)newPos.y, (int)newPos.z);
                        }
                        else
                        {
                            if (newPos.x > VoxelData.ChunkWidthInVoxels -1 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && (newPos.z >= 0 && newPos.z < VoxelData.ChunkWidthInVoxels))
                            {
                                if (updateChunkVariant < 1)
                                    updateChunkVariant = 1;
                            }
                            else if (newPos.x < 0 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && (newPos.z >= 0 && newPos.z < VoxelData.ChunkWidthInVoxels))
                            {
                                if (updateChunkVariant < 2)
                                    updateChunkVariant = 2;
                            }
                            else if ((newPos.x >= 0 && newPos.x < VoxelData.ChunkWidthInVoxels) && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z > VoxelData.ChunkWidthInVoxels -1)
                            {
                                if (updateChunkVariant < 3)
                                    updateChunkVariant = 3;
                            }
                            else if ((newPos.x >= 0 && newPos.x < VoxelData.ChunkWidthInVoxels) && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z < 0)
                            {
                                if (updateChunkVariant < 4)
                                    updateChunkVariant = 4;
                            }
                            else if (newPos.x > VoxelData.ChunkWidthInVoxels -1 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z > VoxelData.ChunkWidthInVoxels -1)
                            {

                                updateChunkVariant = 5;
                            }
                            else if (newPos.x < 0 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z < 0)
                            {

                                updateChunkVariant = 6;
                            }
                            else if (newPos.x > VoxelData.ChunkWidthInVoxels-1 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z < 0)
                            {
                                updateChunkVariant = 7;
                            }
                            else if (newPos.x < 0 && (newPos.y >= 0 && newPos.y < VoxelData.ChunkHeightInVoxels) && newPos.z > VoxelData.ChunkWidthInVoxels -1)
                            {
                                updateChunkVariant = 8;
                            }
                            newPos = new Vector3(newPos.x + Mathf.FloorToInt(_chunkObject.transform.position.x), newPos.y, newPos.z + Mathf.FloorToInt(_chunkObject.transform.position.z));
                            WorldObj.GetChunkFromVector3(newPos).EditVoxelWithoutUpdate(newPos, newID);
                        }
                    }
                }
        switch (updateChunkVariant)
        {
            case 1:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                break;
            case 2:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                break;
            case 3:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                break;
            case 4:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                break;
            case 5:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                break;
            case 6:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                break;
            case 7:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x + VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z - VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                break;
            case 8:
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x - VoxelData.ChunkWidthInVoxels, pos.y, pos.z)).UpdateChunk();
                WorldObj.GetChunkFromVector3(new Vector3(pos.x, pos.y, pos.z + VoxelData.ChunkWidthInVoxels)).UpdateChunk();
                break;
        }
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
    public Vector3 AddVoxelInRadius(Vector3 pos, int x, int y, int z)
    {
        Vector3 gridPoint = new Vector3(Mathf.FloorToInt(pos.x + x), Mathf.FloorToInt(pos.y + y), Mathf.FloorToInt(pos.z + z));
        return gridPoint;
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
    public void UpdateChunk()
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
            return WorldObj.CheckIfVoxelTransparent(pos + position);

        return WorldObj.VoxelTypes[VoxelMap[x, y, z]].IsTransparent;

    }
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(_chunkObject.transform.position.x);
        yCheck -= Mathf.FloorToInt(_chunkObject.transform.position.y);
        zCheck -= Mathf.FloorToInt(_chunkObject.transform.position.z);
        return VoxelMap[xCheck, yCheck, zCheck];
    }
    void UpdateMeshData(Vector3 pos)
    {
        byte voxelID = VoxelMap[(int)(pos.x * IncreaseToInt), (int)(pos.y * IncreaseToInt), (int)(pos.z * IncreaseToInt)];
        bool isTransparent = WorldObj.VoxelTypes[voxelID].IsTransparent;
        for (int j = 0; j < 6; j++)
        {
            if (CheckVoxel(pos + VoxelData.FaceCheck[j]))
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

    void CreateMesh()
    {


        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(_triangles.ToArray(), 0);
        mesh.SetTriangles(_transparentTriangles.ToArray(), 1);

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