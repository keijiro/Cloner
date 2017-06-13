// Cloner - An example of use of procedural instancing.
// https://github.com/keijiro/Cloner

Shader "Cloner/Prepass"
{
    SubShader
    {
        Pass
        {
        Colormask 0

        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma target 5.0

        #include "UnityCG.cginc"

        float4x4 _LocalToWorld;

        #if SHADER_TARGET >= 45

        StructuredBuffer<float4> _TransformBuffer;
        uint _InstanceCount;

        #endif

        float4 vert(float4 vertex : POSITION, uint instanceID : SV_InstanceID) : SV_POSITION
        {
            float4x4 o2w;

            #if SHADER_TARGET >= 45

            uint id = instanceID;

            // Retrieve a transformation from TransformBuffer.
            float4 ps = _TransformBuffer[id + _InstanceCount * 0];
            float3 bx = _TransformBuffer[id + _InstanceCount * 1];
            float3 by = _TransformBuffer[id + _InstanceCount * 2];
            float3 bz = cross(bx, by);

            // Object to world matrix.
            float3 v1 = bx * ps.w;
            float3 v2 = by * ps.w;
            float3 v3 = bz * ps.w;

            o2w = float4x4(
                v1.x, v2.x, v3.x, ps.x,
                v1.y, v2.y, v3.y, ps.y,
                v1.z, v2.z, v3.z, ps.z,
                0, 0, 0, 1
            );

            o2w = mul(_LocalToWorld, o2w);

            #else

            o2w = _LocalToWorld;

            #endif

            return mul(UNITY_MATRIX_VP, mul(o2w, vertex));
        }

        void frag()
        {
        }

        ENDCG
        }
    }
}
