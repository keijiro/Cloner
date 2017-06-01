using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Cloner
{
    //
    // Serializable point cloud object
    //
    // This is useful to reduce memory allocation during run time.
    // (Reading vertex data from a mesh doesn't only allocates vector arrays
    // from GC memory, but also requires "Read/Write Enabled" option on the
    // mesh inporter that spends heap memory.)
    //
    public sealed class PointCloud : ScriptableObject
    {
        #region Public properties and methods

        /// Number of points.
        public int pointCount {
            get { return _data.Length / 3; }
        }

        /// Create a compute buffer with the point data.
        /// The returned buffer must be released by the caller.
        public ComputeBuffer CreateComputeBuffer()
        {
            var buffer = new ComputeBuffer(pointCount, 4 * 4 * 3);
            buffer.SetData(_data);
            return buffer;
        }

        #endregion

        #region Serialized data fields

        [SerializeField] Vector4[] _data;

        #endregion

        #region Editor functions

        #if UNITY_EDITOR

        public void InitWithMesh(Mesh mesh)
        {
            // Input points
            var inVertices = mesh.vertices;
            var inNormals = mesh.normals;
            var inTangents = mesh.tangents;

            // Enumerate unique points.
            var outVertices = new List<Vector3>();
            var outIndices = new List<int>();

            for (var i = 0; i < inVertices.Length; i++)
            {
                if (!outVertices.Any(_ => _ == inVertices[i]))
                {
                    outVertices.Add(inVertices[i]);
                    outIndices.Add(i);
                }
            }

            // Push all the vertices to the output buffer.
            var outBuffer = new List<Vector4>();
            foreach (var i in outIndices) outBuffer.Add(inVertices[i]);
            foreach (var i in outIndices) outBuffer.Add(inNormals[i]);
            foreach (var i in outIndices) outBuffer.Add(inTangents[i]);

            _data = outBuffer.ToArray();
        }

        #endif

        #endregion
    }
}
