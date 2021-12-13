/// MeshCutter in MeshCutter Project
/// --- 	SUMMARY : 	---
///
/// --- 	NOTES : 	---
///
using UnityEngine;

public class MeshCutter : MonoBehaviour
{
    private static readonly string cutMeshKernelName = "CutMesh";
    private static readonly string sliceTrianglesKernelName = "SliceTriangles";
    private static readonly string planePointName = "planePoint";
    private static readonly string planeNormalName = "planeNormal";
    private static readonly string meshPointsBufferName = "meshPointsBuffer";
    private static readonly string trianglesBufferName = "trianglesBuffer";
    private static readonly string slicedTrianglesBufferName = "slicedTrianglesBuffer";


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

        int _kernelIndex = cutterShader.FindKernel(cutMeshKernelName);
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
        Buffer<SlicedTriangleData> _slicedTriangles = new Buffer<SlicedTriangleData>(0,1);

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

                if (_triangles[i].Side.x != _triangles[i].Side.y && _triangles[i].Side.y == _triangles[i].Side.z) // get the unique point
                {
                    _slicedTriangles.Add(new SlicedTriangleData
                    {
                        Triangle1Vertex1 = cutMesh.mesh.vertices[_triangles[i].Vertices.x],
                        Triangle1Vertex2 = cutMesh.mesh.vertices[_triangles[i].Vertices.y],
                        Triangle1Vertex3 = cutMesh.mesh.vertices[_triangles[i].Vertices.z],
                        Triangle2Vertex1 = Vector3.zero,
                        Triangle2Vertex2 = Vector3.zero,
                        Triangle2Vertex3 = Vector3.zero,
                        Triangle3Vertex1 = Vector3.zero,
                        Triangle3Vertex2 = Vector3.zero,
                        Triangle3Vertex3 = Vector3.zero,
                        FirstSide = _triangles[i].Side.x
                    });
                }
                else if (_triangles[i].Side.x != _triangles[i].Side.y && _triangles[i].Side.x == _triangles[i].Side.z)
                {
                    _slicedTriangles.Add(new SlicedTriangleData
                    {
                        Triangle1Vertex1 = cutMesh.mesh.vertices[_triangles[i].Vertices.y],
                        Triangle1Vertex2 = cutMesh.mesh.vertices[_triangles[i].Vertices.z],
                        Triangle1Vertex3 = cutMesh.mesh.vertices[_triangles[i].Vertices.x],
                        Triangle2Vertex1 = Vector3.zero,
                        Triangle2Vertex2 = Vector3.zero,
                        Triangle2Vertex3 = Vector3.zero,
                        Triangle3Vertex1 = Vector3.zero,
                        Triangle3Vertex2 = Vector3.zero,
                        Triangle3Vertex3 = Vector3.zero,
                        FirstSide = _triangles[i].Side.y
                    });
                }
                else
                {
                    _slicedTriangles.Add(new SlicedTriangleData
                    {
                        Triangle1Vertex1 = cutMesh.mesh.vertices[_triangles[i].Vertices.z],
                        Triangle1Vertex2 = cutMesh.mesh.vertices[_triangles[i].Vertices.x],
                        Triangle1Vertex3 = cutMesh.mesh.vertices[_triangles[i].Vertices.y],
                        Triangle2Vertex1 = Vector3.zero,
                        Triangle2Vertex2 = Vector3.zero,
                        Triangle2Vertex3 = Vector3.zero,
                        Triangle3Vertex1 = Vector3.zero,
                        Triangle3Vertex2 = Vector3.zero,
                        Triangle3Vertex3 = Vector3.zero,
                        FirstSide = _triangles[i].Side.z
                    });
                }
            }

        }

        // Set Sliced Triangles Buffer
        _slicedTriangles.Resize();
        _kernelIndex = cutterShader.FindKernel(sliceTrianglesKernelName);
        ComputeBuffer _slicedTriangleBuffer = new ComputeBuffer(_slicedTriangles.Array.Length, sizeof(float) * (9 * 3) + sizeof(int));
        _slicedTriangleBuffer.SetData(_slicedTriangles.Array);
        cutterShader.SetBuffer(_kernelIndex, slicedTrianglesBufferName, _slicedTriangleBuffer);
        cutterShader.Dispatch(_kernelIndex, (_slicedTriangles.Array.Length / 8) + 1, 1, 1);
        _slicedTriangleBuffer.GetData(_slicedTriangles.Array);
        _slicedTriangleBuffer.Dispose();

        int _side = 0;
        Vector3 _triangleNormal = Vector3.zero;
        for (int i = 0; i < _slicedTriangles.Array.Length; i++)
        {
            _side = _slicedTriangles[i].FirstSide;
            _triangleNormal = Vector3.Cross(_slicedTriangles[i].Triangle1Vertex2 - _slicedTriangles[i].Triangle1Vertex1, _slicedTriangles[i].Triangle1Vertex3 - _slicedTriangles[i].Triangle1Vertex1);
            for (int j = 0; j < 3; j++)
            {
                if (_side > 0)
                    _rightMeshData.AddPoint(_slicedTriangles[i].GetVertexFromIndex(j), _triangleNormal, Vector2.zero);
                else _leftMeshData.AddPoint(_slicedTriangles[i].GetVertexFromIndex(j), _triangleNormal, Vector2.zero);
            }
            for (int j = 3; j < 9; j++)
            {
                if (_side > 0)
                    _leftMeshData.AddPoint(_slicedTriangles[i].GetVertexFromIndex(j), _triangleNormal, Vector2.zero);
                else _rightMeshData.AddPoint(_slicedTriangles[i].GetVertexFromIndex(j), _triangleNormal, Vector2.zero);
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
        _filter.transform.position -= _planeNormal.normalized * .1f;
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

public struct TriangleData
{
    public Vector3Int Vertices;
    public Vector3Int Side;
}

public struct SlicedTriangleData
{
    public Vector3 Triangle1Vertex1;
    public Vector3 Triangle1Vertex2;
    public Vector3 Triangle1Vertex3;
    public Vector3 Triangle2Vertex1;
    public Vector3 Triangle2Vertex2;
    public Vector3 Triangle2Vertex3;
    public Vector3 Triangle3Vertex1;
    public Vector3 Triangle3Vertex2;
    public Vector3 Triangle3Vertex3;
    public int FirstSide;

    public Vector3 GetVertexFromIndex(int _index)
    {
        switch (_index)
        {
            case 0:
                return Triangle1Vertex1;
            case 1:
                return Triangle1Vertex2;
            case 2:
                return Triangle1Vertex3;
            case 3:
                return Triangle2Vertex1;
            case 4:
                return Triangle2Vertex2;
            case 5:
                return Triangle2Vertex3;
            case 6:
                return Triangle3Vertex1;
            case 7:
                return Triangle3Vertex2;
            case 8:
                return Triangle3Vertex3;
            default:
                return Vector3.zero;
        }
    }
}