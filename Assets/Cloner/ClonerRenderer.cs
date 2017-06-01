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

        [SerializeField] float _noiseAmplitude = 1;

        public float noiseAmplitude {
            get { return _noiseAmplitude; }
            set { _noiseAmplitude = value; }
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
        ComputeBuffer _pointBuffer;
        ComputeBuffer _transformBuffer;
        MaterialPropertyBlock _props;
        Vector3 _noiseOffset;

        #endregion

        #region Compute configurations

        const int kThreadCount = 64;

        int ThreadGroupCount {
            get { return _pointCloud.pointCount/ kThreadCount; }
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
            _pointBuffer = _pointCloud.CreateComputeBuffer();
            _transformBuffer = new ComputeBuffer(InstanceCount, 3 * 4 * 4);

            // Initialize the update kernel.
            var kernel = _compute.FindKernel("ClonerUpdate");
            _compute.SetBuffer(kernel, "PointBuffer", _pointBuffer);
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
            _pointBuffer.Release();
            _transformBuffer.Release();
        }

        void Update()
        {
            _noiseOffset += _noiseMotion * Time.deltaTime;

            // Update the transform buffer.
            var kernel = _compute.FindKernel("ClonerUpdate");
            _compute.SetFloat("BaseScale", _templateScale);
            _compute.SetFloat("NoiseFrequency", _noiseFrequency);
            _compute.SetFloat("NoiseAmplitude", _noiseAmplitude);
            _compute.SetVector("NoiseOffset", _noiseOffset);
            _compute.Dispatch(kernel, ThreadGroupCount, 1, 1);

            // Draw the meshes with instancing.
            _material.SetVector("_GradientA", _gradient.coeffsA);
            _material.SetVector("_GradientB", _gradient.coeffsB);
            _material.SetVector("_GradientC", _gradient.coeffsC2);
            _material.SetVector("_GradientD", _gradient.coeffsD2);

            // Draw the meshes with instancing.
            Graphics.DrawMeshInstancedIndirect(
                _template, 0, _material, _template.bounds,
                _drawArgsBuffer, 0, _props
            );
        }

        #endregion
    }
}
