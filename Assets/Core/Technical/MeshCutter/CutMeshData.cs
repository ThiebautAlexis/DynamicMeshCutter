/// CutMeshData in #PROJECTNAME#
/// --- 	SUMMARY : 	---
///
/// --- 	NOTES : 	---
///
using UnityEngine;

public class CutMeshData
{

    #region Fields and Properties
    public Buffer<int> Triangles    {get ; private set;} = new Buffer<int>(0, 1);
    public Buffer<Vector3> Vertices {get ; private set;} = new Buffer<Vector3>(0, 1);
    public Buffer<Vector3> Normals  {get ; private set;} = new Buffer<Vector3>(0, 1);
    public Buffer<Vector2> UVs      {get ; private set;} = new Buffer<Vector2>(0, 1);
    #endregion                                           

    #region Methods
    public void AddPoint(Vector3 _vertex, Vector3 _normal, Vector2 _uv)
    {
        if(Vertices.Contains(_vertex))
        {
            for (int _i = 0; _i < Vertices.Array.Length; _i++)
            {
                // In this case, the point is already in the array
                if(Vertices[_i] == _vertex && Normals[_i] == _normal && UVs[_i] == _uv)
                {
                    Triangles.Add(_i);
                    return;
                }
            }
            // In this case, the vertex is already in the Vertices buffer but the normal or the UV of the point are new
            Triangles.Add(Vertices.Array.Length);
            Vertices.Add(_vertex);
            Normals.Add(_normal);
            UVs.Add(_uv);
        }
        else
        {
            Triangles.Add(Vertices.Array.Length);
            Vertices.Add(_vertex);
            Normals.Add(_normal);
            UVs.Add(_uv);
        }
    }

    public void Resize()
    {
        Triangles.Resize();
        Vertices.Resize();
        Normals.Resize();
        UVs.Resize();
    }
    #endregion

}
