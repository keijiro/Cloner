using UnityEngine;

public class SpikeRenderer : MonoBehaviour
{
    [SerializeField] Mesh _mesh;
    [SerializeField] Material _material;

    [SerializeField] Mesh _baseMesh;

    int _instanceCount;
    Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

    uint[] _drawArgs = new uint[5] { 0, 0, 0, 0, 0 };
    ComputeBuffer _drawArgsBuffer;

    ComputeBuffer _baseBuffer;

    void Start()
    {
        InitBaseBuffer();

        var bufferType = ComputeBufferType.IndirectArguments;
        _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), bufferType);
        _drawArgs[0] = (uint)_mesh.GetIndexCount(0);
        _drawArgs[1] = (uint)_instanceCount;
        _drawArgsBuffer.SetData(_drawArgs);

        _material.SetBuffer("_BaseBuffer", _baseBuffer);
    }

    void OnDestroy()
    {
        _drawArgsBuffer.Release();
        _baseBuffer.Release();
    }

    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _material, _bounds, _drawArgsBuffer);
    }

    void InitBaseBuffer()
    {
        var vertices = _baseMesh.vertices;
        var normals = _baseMesh.normals;
        var tangents = _baseMesh.tangents;

        _instanceCount = vertices.Length;

        var data = new Vector4[_instanceCount * 3];
        for (var i = 0; i < _instanceCount; i++)
        {
            data[i * 3 + 0] = vertices[i]; 
            data[i * 3 + 1] = normals[i];
            data[i * 3 + 2] = tangents[i];
        }

        _baseBuffer = new ComputeBuffer(_instanceCount, 4 * 4 * 3);
        _baseBuffer.SetData(data);
    }
}
