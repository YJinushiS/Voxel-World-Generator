using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    #region variables
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

    byte[,,] _voxelMap = new byte[VoxelData.ChunkWidthInVoxels, VoxelData.ChunkHeightInVoxels, VoxelData.ChunkWidthInVoxels];

    public World WorldObj;
    #endregion
    public Chunk(ChunkCoord coord,World world)
    {
        Coord = coord;
        _chunkObject = new GameObject();
        _chunkObject.transform.position = new Vector3((Coord.x * VoxelData.ChunkWidthInVoxels)* VoxelSize, 0f, (Coord.z * VoxelData.ChunkWidthInVoxels)*VoxelSize);

        WorldObj = world;
        _chunkMeshFilter = _chunkObject.AddComponent<MeshFilter>();
        _chunkMeshRenderer = _chunkObject.AddComponent<MeshRenderer>();

        _chunkMeshRenderer.material = WorldObj.VoxelAtlas;
        _chunkObject.transform.SetParent(WorldObj.transform);

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }
    public bool isActive
    {

        get { return _chunkObject.activeSelf; }
        set { _chunkObject.SetActive(value); }

    }
    Vector3 position
    {

        get { return _chunkObject.transform.position; }

    }
    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidthInVoxels - 1*VoxelData.VoxelSize || y < 0 || y > VoxelData.ChunkHeightInVoxels - 1* VoxelData.VoxelSize || z < 0 || z > VoxelData.ChunkWidthInVoxels - 1* VoxelData.VoxelSize)
            return false;
        else return true;

    }
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    _voxelMap[x, y, z] = WorldObj.GetVoxel(new Vector3(x*VoxelSize, y*VoxelSize, z*VoxelSize) + position);
                }
            }
        }
    }
    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeightInVoxels; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidthInVoxels; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidthInVoxels; z++)
                {
                    if (WorldObj.VoxelTypes[_voxelMap[x, y, z]].IsSolid)
                        AddVoxelDataToChunk(new Vector3(x * VoxelSize, y * VoxelSize, z * VoxelSize));
                }
            }
        }
    }
    public byte GetVoxelFromMap(Vector3 pos)
    {

        pos -= position;

        return _voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

    }
    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x * IncreaseToInt);
        int y = Mathf.FloorToInt(pos.y * IncreaseToInt);
        int z = Mathf.FloorToInt(pos.z * IncreaseToInt);

        if (!IsVoxelInChunk(x, y, z))
            return WorldObj.VoxelTypes[WorldObj.GetVoxel(pos + position)].IsSolid;

        return WorldObj.VoxelTypes[_voxelMap[x, y, z]].IsSolid;

    }
    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int j = 0; j < 6; j++)
        {
            if (!CheckVoxel(pos + VoxelData.FaceCheck[j]))
            {
                byte voxelID = _voxelMap[(int)(pos.x * IncreaseToInt), (int)(pos.y * IncreaseToInt), (int)(pos.z * IncreaseToInt)];

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

    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {

        x = _x;
        z = _z;

    }

    public bool Equals(ChunkCoord other)
    {

        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }

}