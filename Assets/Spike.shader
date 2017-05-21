Shader "Spike"
{
	Properties
    {
		_Color("Color", Color) = (1, 1, 1, 1)
		_Smoothness("Smoothness", Range(0, 1)) = 0
		_Metallic("Metallic", Range(0, 1)) = 0
	}
	SubShader
    {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM

		#pragma surface surf Standard addshadow
        #pragma multi_compile_instancing
        #pragma instancing_options procedural:setup

        #include "SimplexNoiseGrad3D.cginc"

		struct Input { float dummy; };

		half4 _Color;
		half _Smoothness;
		half _Metallic;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

        struct BaseBuffer
        {
            float4 position;
            float4 normal;
            float4 tangent;
        };

        StructuredBuffer<BaseBuffer> _BaseBuffer;

        #endif

        // Pseudo random number generator with 2D coordinates
        float UVRandom(float u, float v)
        {
            float f = dot(float2(12.9898, 78.233), float2(u, v));
            return frac(43758.5453 * sin(f));
        }

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            float3 position = _BaseBuffer[unity_InstanceID].position.xyz;
            float3 normal = _BaseBuffer[unity_InstanceID].normal.xyz;
            float4 tangent = _BaseBuffer[unity_InstanceID].tangent;

            float3 d = snoise_grad(position * 2 + _Time.x * 3);

            float3 v2 = normalize(normal + d * 2.1);
            float3 v3 = normalize(cross(v2, tangent.xyz) * tangent.w);
            float3 v1 = normalize(cross(v2, v3));

            float s = (0.3 + d.x * 0.15) * (sin(_Time.y) * 0.3 + 0.7);
            s *= 0.1;

            v1 *= s;
            v2 *= s;
            v3 *= s;

            float4x4 m = float4x4(
                v1.x, v2.x, v3.x, position.x,
                v1.y, v2.y, v3.y, position.y,
                v1.z, v2.z, v3.z, position.z,
                0, 0, 0, 1
            );

            v1 /= s;
            v2 /= s;
            v3 /= s;
            v1 /= s;
            v2 /= s;
            v3 /= s;

            float4x4 im = float4x4(
                v1.x, v1.y, v1.z, -position.x,
                v2.x, v2.y, v2.z, -position.y,
                v3.x, v3.y, v3.z, -position.z,
                0, 0, 0, 1
            );

            unity_ObjectToWorld = m;
            unity_WorldToObject = im;

        #endif
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
        {
			o.Albedo = _Color.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
