/// MeshCutter in MeshCutter Project
/// --- 	SUMMARY : 	---
///
/// --- 	NOTES : 	---
///
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    private static readonly string kernelName = "CutMesh";
    private static readonly string planePointName = "planePoint";
    private static readonly string planeNormalName = "planeNormal";
    private static readonly string meshPointsBufferName = "meshPointsBuffer";
    private static readonly string trianglesBufferName = "trianglesBuffer";

    #region Fields and Properties
    [SerializeField] private ComputeShader cutterShader = null;
    [SerializeField] private MeshFilter cutMesh = null;
    [SerializeField] private Vector3[] cutpoints = new Vector3[3];
    #endregion

    #region Methods
    void CutMesh()
    {
        // Init Triangles struct
        TriangleData[] _triangles = new TriangleData[cutMesh.mesh.triangles.Length / 3];
        int _index = 0;
        for (int i = 0; i < cutMesh.mesh.triangles.Length; i+=3)
        {
            _triangles[_index] = new TriangleData
            {
                Vertices = new Vector3Int(cutMesh.mesh.triangles[i], cutMesh.mesh.triangles[i + 1], cutMesh.mesh.triangles[i + 2]),
                Side = Vector3Int.zero
            };
            _index++;
        }
        Vector3 _planeNormal = Vector3.Cross(cutpoints[1] - cutpoints[0], cutpoints[2] - cutpoints[0]);

        int _kernelIndex = cutterShader.FindKernel(kernelName);
        // Set Mesh Points
        ComputeBuffer _meshPointsBuffer = new ComputeBuffer(cutMesh.mesh.vertexCount, sizeof(float) * 3);
        _meshPointsBuffer.SetData(cutMesh.mesh.vertices);
        // Set Triangles Buffer
        ComputeBuffer _triangleBuffer = new ComputeBuffer(_triangles.Length, sizeof(int) * 6);
        _triangleBuffer.SetData(_triangles);

        cutterShader.SetFloats(planePointName,new float[] { cutpoints[0].x, cutpoints[0].y, cutpoints[0].z});
        cutterShader.SetFloats(planeNormalName,new float[] { _planeNormal.x, _planeNormal.y, _planeNormal.z});
        cutterShader.SetBuffer(_kernelIndex, meshPointsBufferName, _meshPointsBuffer);
        cutterShader.SetBuffer(_kernelIndex, trianglesBufferName, _triangleBuffer);
        cutterShader.Dispatch(_kernelIndex, (_triangles.Length / 8) + 1, 1, 1);
        _triangleBuffer.GetData(_triangles);

        // Dispose
        _meshPointsBuffer.Dispose();
        _triangleBuffer.Dispose();

        CutMeshData _rightMeshData = new CutMeshData();
        CutMeshData _leftMeshData = new CutMeshData();

        for (int i = 0; i < _triangles.Length; i++)
        {
            if (_triangles[i].Side.x > 0 && _triangles[i].Side.y > 0 && _triangles[i].Side.z > 0 )
            {
                _rightMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.x], cutMesh.mesh.normals[_triangles[i].Vertices.x], Vector2.zero);
                _rightMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.y], cutMesh.mesh.normals[_triangles[i].Vertices.y], Vector2.zero);
                _rightMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.z], cutMesh.mesh.normals[_triangles[i].Vertices.z], Vector2.zero);
            }
            else if (_triangles[i].Side.x < 0 && _triangles[i].Side.y < 0 && _triangles[i].Side.z < 0)
            {
                _leftMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.x], cutMesh.mesh.normals[_triangles[i].Vertices.x], Vector2.zero);
                _leftMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.y], cutMesh.mesh.normals[_triangles[i].Vertices.y], Vector2.zero);
                _leftMeshData.AddPoint(cutMesh.mesh.vertices[_triangles[i].Vertices.z], cutMesh.mesh.normals[_triangles[i].Vertices.z], Vector2.zero);
            }
            else
            {
                Vector3 _pO, _lA, _lB, _pNormal, _pX, _pY;
                int _side = 0;
                if (_triangles[i].Side.x != _triangles[i].Side.y && _triangles[i].Side.y == _triangles[i].Side.z) // get the unique point
                {
                    _side = _triangles[i].Side.x;
                    _pNormal = cutMesh.mesh.normals[_triangles[i].Vertices.x];

                    _pO = cutMesh.mesh.vertices[_triangles[i].Vertices.x];                                                   // origin of the intersection
                    _pX = cutMesh.mesh.vertices[_triangles[i].Vertices.y];
                    _pY = cutMesh.mesh.vertices[_triangles[i].Vertices.z];

                    _lA = cutMesh.mesh.vertices[_triangles[i].Vertices.y] - cutMesh.mesh.vertices[_triangles[i].Vertices.x]; // direction of the line A
                    _lB = cutMesh.mesh.vertices[_triangles[i].Vertices.z] - cutMesh.mesh.vertices[_triangles[i].Vertices.x]; // direction of the line B
                }
                else if (_triangles[i].Side.x != _triangles[i].Side.y && _triangles[i].Side.x == _triangles[i].Side.z)
                {
                    _side = _triangles[i].Side.y;
                    _pNormal = cutMesh.mesh.normals[_triangles[i].Vertices.y];

                    _pO = cutMesh.mesh.vertices[_triangles[i].Vertices.y];
                    _pX = cutMesh.mesh.vertices[_triangles[i].Vertices.x];
                    _pY = cutMesh.mesh.vertices[_triangles[i].Vertices.z];

                    _lA = cutMesh.mesh.vertices[_triangles[i].Vertices.x] - cutMesh.mesh.vertices[_triangles[i].Vertices.y]; 
                    _lB = cutMesh.mesh.vertices[_triangles[i].Vertices.z] - cutMesh.mesh.vertices[_triangles[i].Vertices.y]; 
                }
                else
                {
                    _side = _triangles[i].Side.z;
                    _pNormal = cutMesh.mesh.normals[_triangles[i].Vertices.z];

                    _pO = cutMesh.mesh.vertices[_triangles[i].Vertices.z];
                    _pX = cutMesh.mesh.vertices[_triangles[i].Vertices.x];
                    _pY = cutMesh.mesh.vertices[_triangles[i].Vertices.y];

                    _lA = cutMesh.mesh.vertices[_triangles[i].Vertices.y] - cutMesh.mesh.vertices[_triangles[i].Vertices.z];
                    _lB = cutMesh.mesh.vertices[_triangles[i].Vertices.x] - cutMesh.mesh.vertices[_triangles[i].Vertices.z];
                }
                Vector3 _pA = _pO + _lA * (Vector3.Dot(cutpoints[0] - _pO, _planeNormal) / Vector3.Dot(_lA, _planeNormal)); // First intersection Point
                Vector3 _pB = _pO + _lB * (Vector3.Dot(cutpoints[0] - _pO, _planeNormal) / Vector3.Dot(_lB, _planeNormal)); // Second intersection Point

                if(_side > 0) // If the unique point is on the right
                {
                    _rightMeshData.AddPoint(_pO, _pNormal, Vector2.zero);
                    if(GeometryHelper.IsClockwiseTriangle(_pO, _pA, _pB, _pNormal))
                    {
                        _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                    }
                    else
                    {
                        _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                    }
                    // Add other side triangles
                    _leftMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                    if (GeometryHelper.IsClockwiseTriangle(_pX, _pA, _pB, _pNormal))
                    {
                        _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                    }
                    else
                    {
                        _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                    }
                    _leftMeshData.AddPoint(_pY, _pNormal, Vector2.zero);
                    if (Mathf.Abs(Vector3.Dot(Vector3.Cross((_pX - _pY).normalized, (_pA - _pY).normalized), _pNormal)) > Mathf.Abs(Vector3.Dot(Vector3.Cross((_pX - _pY).normalized, (_pB - _pY).normalized), _pNormal)))
                    {
                        if (GeometryHelper.IsClockwiseTriangle(_pY, _pX, _pA, _pNormal))
                        {
                            _leftMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                            _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        }
                        else
                        {
                            _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                            _leftMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                        }
                    }
                    else
                    {
                        if (GeometryHelper.IsClockwiseTriangle(_pY, _pX, _pB, _pNormal))
                        {
                            _leftMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                            _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        }
                        else
                        {
                            _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                            _leftMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                        }
                    }
                }
                else // If the unique point is on the left
                {
                    _leftMeshData.AddPoint(_pO, _pNormal, Vector2.zero);
                    if (GeometryHelper.IsClockwiseTriangle(_pO, _pA, _pB, _pNormal))
                    {
                        _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                    }
                    else
                    {
                        _leftMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        _leftMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                    }
                    // Add other side triangles
                    _rightMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                    if (GeometryHelper.IsClockwiseTriangle(_pX, _pA, _pB, _pNormal))
                    {
                        _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                    }
                    else
                    {
                        _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                    }
                    _rightMeshData.AddPoint(_pY, _pNormal, Vector2.zero);
                    if(Mathf.Abs(Vector3.Dot(Vector3.Cross((_pX - _pY).normalized, (_pA - _pY).normalized), _pNormal)) > Mathf.Abs(Vector3.Dot(Vector3.Cross((_pX - _pY).normalized, (_pB - _pY).normalized), _pNormal)))
                    {
                        if (GeometryHelper.IsClockwiseTriangle(_pY, _pX, _pA, _pNormal))
                        {
                            _rightMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                            _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                        }
                        else
                        {
                            _rightMeshData.AddPoint(_pA, _pNormal, Vector2.zero);
                            _rightMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                        }
                    }
                    else
                    {
                        if (GeometryHelper.IsClockwiseTriangle(_pY, _pX, _pB, _pNormal))
                        {
                            _rightMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                            _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                        }
                        else
                        {
                            _rightMeshData.AddPoint(_pB, _pNormal, Vector2.zero);
                            _rightMeshData.AddPoint(_pX, _pNormal, Vector2.zero);
                        }
                    }
                }
            }

        }
        _rightMeshData.Resize();
        _leftMeshData.Resize();

        Mesh _rightMesh = new Mesh();
        _rightMesh.SetVertices(_rightMeshData.Vertices.Array);
        _rightMesh.SetTriangles(_rightMeshData.Triangles.Array, 0);
        _rightMesh.SetNormals(_rightMeshData.Normals.Array);
        _rightMesh.SetUVs(0, _rightMeshData.UVs.Array);

        cutMesh.mesh = _rightMesh;

        MeshFilter _filter = Instantiate(cutMesh, cutMesh.transform.position, Quaternion.identity, cutMesh.transform.parent);
        Mesh _leftMesh = new Mesh();
        _leftMesh.SetVertices(_leftMeshData.Vertices.Array);
        _leftMesh.SetTriangles(_leftMeshData.Triangles.Array, 0);
        _leftMesh.SetNormals(_leftMeshData.Normals.Array);
        _leftMesh.SetUVs(0, _leftMeshData.UVs.Array);

        _filter.mesh = _leftMesh;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CutMesh();
            Debug.Break();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < cutpoints.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + cutpoints[i], .1f);
            if (i < cutpoints.Length - 1)
                Gizmos.DrawLine( transform.position + cutpoints[i], transform.position + cutpoints[i + 1]);
        }
    }
    #endregion

}

[System.Serializable]
public struct TriangleData
{
    public Vector3Int Vertices;
    public Vector3Int Side;
}