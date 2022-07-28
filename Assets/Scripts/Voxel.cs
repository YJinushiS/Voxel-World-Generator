using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Voxel : MonoBehaviour
{
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Vector3 _position;
    [SerializeField] private List<Vector3> _vertices = new List<Vector3>();
    [SerializeField] private List<int> _triangles = new List<int>();
    [SerializeField] private List<Vector2> _uvs = new List<Vector2>();
    private int _lastVertex;
    private void Start()
    {
        //initialise the mesh
        _mesh = new Mesh();

        //create the mesh data
        DrawCube();

        //set the mesh data
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.SetUVs(0, _uvs.ToArray());
        //set the mesh
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void DrawCube()
    {
        FrontGenerateFace();
    }

    private void FrontGenerateFace()
    {
        _lastVertex = _vertices.Count;
        //declare vertices
        _vertices.Add(_position + Vector3.forward);//0
        _vertices.Add(_position + Vector3.forward + Vector3.up);//1
        _vertices.Add(_position + Vector3.forward + Vector3.right+ Vector3.up);//2
        _vertices.Add(_position + Vector3.forward + Vector3.right);//3
        //first triangle
        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex+1);
        _triangles.Add(_lastVertex+2);
        //second triangle
        _triangles.Add(_lastVertex);
        _triangles.Add(_lastVertex+2);
        _triangles.Add(_lastVertex+3);

    }
}
