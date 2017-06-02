using UnityEngine;
using Klak.Chromatics;

namespace Cloner
{
    public sealed class ClonerRenderer : MonoBehaviour
    {
        #region Basic settings

        [SerializeField] PointCloud _pointCloud;

        public PointCloud pointCloud {
            get { return _pointCloud; }
        }

        [SerializeField] Mesh _template;

        public Mesh template {
            get { return _template; }
        }

        [SerializeField] float _templateScale = 0.1f;

        public float templateScale {
            get { return _templateScale; }
            set { _templateScale = value; }
        }

        [SerializeField] Material _material;

        public Material material {
            get { return _material; }
        }

        [SerializeField] CosineGradient _gradient;

        public CosineGradient gradient {
            get { return _gradient; }
            set { _gradient = value; }
        }

        #endregion

        #region Noise field parameters

        [SerializeField] float _noiseFrequency = 4;

        public float noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        [SerializeField] float _directionNoise = 1;

        public float directionNoise {
            get { return _directionNoise; }
            set { _directionNoise = value; }
        }

        [SerializeField] float _scaleNoise = 0.1f;

        public float scaleNoise {
            get { return _scaleNoise; }
            set { _scaleNoise = value; }
        }

        [SerializeField] Vector3 _noiseMotion = Vector3.up * 0.2f;

        public Vector3 noiseMotion {
            get { return _noiseMotion; }
            set { _noiseMotion = value; }
        }

        #endregion

        #region Hidden attributes

        [SerializeField, HideInInspector] ComputeShader _compute;

        #endregion

        #region Private fields

        ComputeBuffer _drawArgsBuffer;
        ComputeBuffer _positionBuffer;
        ComputeBuffer _normalBuffer;
        ComputeBuffer _tangentBuffer;
        ComputeBuffer _transformBuffer;
        MaterialPropertyBlock _props;
        Vector3 _noiseOffset;

        #endregion

        #region Compute configurations

        const int kThreadCount = 64;

        int ThreadGroupCount {
            get { return _pointCloud.pointCount / kThreadCount; }
        }

        int InstanceCount {
            get { return ThreadGroupCount * kThreadCount; }
        }

        #endregion

        #region MonoBehaviour functions

        void Start()
        {
            // Initialize the indirect draw args buffer.
            _drawArgsBuffer = new ComputeBuffer(
                1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
            );

            _drawArgsBuffer.SetData(new uint[5] {
                _template.GetIndexCount(0),
                (uint)InstanceCount, 0, 0, 0
            });

            // Allocate compute buffers.
            _positionBuffer = _pointCloud.CreatePositionBuffer();
            _normalBuffer = _pointCloud.CreateNormalBuffer();
            _tangentBuffer = _pointCloud.CreateTangentBuffer();
            _transformBuffer = new ComputeBuffer(InstanceCount, 3 * 4 * 4);

            // Initialize the update kernel.
            var kernel = _compute.FindKernel("ClonerUpdate");
            _compute.SetBuffer(kernel, "PositionBuffer", _positionBuffer);
            _compute.SetBuffer(kernel, "NormalBuffer", _normalBuffer);
            _compute.SetBuffer(kernel, "TangentBuffer", _tangentBuffer);
            _compute.SetBuffer(kernel, "TransformBuffer", _transformBuffer);
            _compute.SetInt("InstanceCount", InstanceCount);

            // Initialize the material.
            _material.SetBuffer("_TransformBuffer", _transformBuffer);
            _material.SetInt("_InstanceCount", InstanceCount);

            // This property block is used only for avoiding an instancing bug.
            _props = new MaterialPropertyBlock();
            _props.SetFloat("_UniqueID", Random.value);
        }

        void OnDestroy()
        {
            _drawArgsBuffer.Release();
            _positionBuffer.Release();
            _normalBuffer.Release();
            _tangentBuffer.Release();
            _transformBuffer.Release();
        }

        void Update()
        {
            _noiseOffset += _noiseMotion * Time.deltaTime;

            // Update the transform buffer.
            var kernel = _compute.FindKernel("ClonerUpdate");
            _compute.SetFloat("BaseScale", _templateScale);
            _compute.SetFloat("NoiseFrequency", _noiseFrequency);
            _compute.SetFloat("DirectionNoise", _directionNoise);
            _compute.SetFloat("ScaleNoise", _scaleNoise);
            _compute.SetVector("NoiseOffset", _noiseOffset);
            _compute.Dispatch(kernel, ThreadGroupCount, 1, 1);

            // Draw the meshes with instancing.
            _material.SetVector("_GradientA", _gradient.coeffsA);
            _material.SetVector("_GradientB", _gradient.coeffsB);
            _material.SetVector("_GradientC", _gradient.coeffsC2);
            _material.SetVector("_GradientD", _gradient.coeffsD2);

            // Draw the meshes with instancing.
            Graphics.DrawMeshInstancedIndirect(
                _template, 0, _material,
                new Bounds(Vector3.zero, Vector3.one * 10),
                _drawArgsBuffer, 0, _props
            );
        }

        #endregion
    }
}
