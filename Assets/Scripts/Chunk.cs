using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    #region variables
    public MeshRenderer ChunkMeshRenderer;
    public MeshFilter ChunkMeshFilter;

    int _vertexIndex = 0;
    private static float _voxelSize = VoxelData.VoxelSize;
    private static int _increaseToInt = Convert.ToInt32(1 / _voxelSize);
    List<Vector3> _vertices = new List<Vector3>();
    List<int> _triangles = new List<int>();
    List<Vector2> _uvs = new List<Vector2>();
    bool[,,] _voxelMap = new bool[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    #endregion
    private void Start()
    {
        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }
    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    _voxelMap[x, y, z] = true;
                }
            }
        }
    }
    void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x * _voxelSize, y * _voxelSize, z * _voxelSize));
                }
            }
        }
    }
    bool CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x * _increaseToInt);
        int y = Mathf.FloorToInt(pos.y * _increaseToInt);
        int z = Mathf.FloorToInt(pos.z * _increaseToInt);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;

        return _voxelMap[x, y, z];

    }
    void AddVoxelDataToChunk(Vector3 pos)
    {
        for (int j = 0; j < 6; j++)
        {
            if (!CheckVoxel(pos + VoxelData.FaceCheck[j]))
            {
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 0]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 1]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 2]]);
                _vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[j, 3]]);
                _uvs.Add(VoxelData.VoxelUvs[0]);
                _uvs.Add(VoxelData.VoxelUvs[1]);
                _uvs.Add(VoxelData.VoxelUvs[2]);
                _uvs.Add(VoxelData.VoxelUvs[3]);
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

        ChunkMeshFilter.mesh = mesh;
    }
}
