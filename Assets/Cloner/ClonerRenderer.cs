using UnityEngine;

namespace Cloner
{
    public sealed class ClonerRenderer : MonoBehaviour
    {
        [SerializeField] Mesh _template;
        [SerializeField] Material _material;
        [SerializeField] PointCloud _pointCloud;

        uint[] _drawArgs = new uint[5] { 0, 0, 0, 0, 0 };
        ComputeBuffer _drawArgsBuffer;

        ComputeBuffer _pointBuffer;

        Bounds _bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

        void Start()
        {
            var bufferType = ComputeBufferType.IndirectArguments;
            _drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), bufferType);
            _drawArgs[0] = (uint)_template.GetIndexCount(0);
            _drawArgs[1] = (uint)_pointCloud.pointCount;
            _drawArgsBuffer.SetData(_drawArgs);

            _pointBuffer = _pointCloud.CreateComputeBuffer();

            _material.SetBuffer("_PointBuffer", _pointBuffer);
        }

        void OnDestroy()
        {
            _drawArgsBuffer.Release();
            _pointBuffer.Release();
        }

        void Update()
        {
            Graphics.DrawMeshInstancedIndirect(_template, 0, _material, _bounds, _drawArgsBuffer);
        }
    }
}
