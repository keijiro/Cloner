Shader "Cloner/Default"
{
	Properties
    {
        _Scale("Scale", Float) = 1
        _NoiseAmp("Noise Amplitude", Float) = 1
        _MainTex("Albedo", 2D) = "white" {}
        _UVScale("UV Scale", Float) = 1
        _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Range(0, 1)) = 1
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

		struct Input
        {
            float2 uv_MainTex;
        };

        half _Scale;
        half _NoiseAmp;
        sampler2D _MainTex;
        half _UVScale;
        sampler2D _NormalMap;
        half _NormalScale;
		half4 _Color;
		half _Smoothness;
		half _Metallic;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

        struct PointBuffer
        {
            float4 position;
            float4 normal;
            float4 tangent;
        };

        StructuredBuffer<PointBuffer> _PointBuffer;

        #endif

        void setup()
        {
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            float3 position = _PointBuffer[unity_InstanceID].position.xyz;
            float3 normal = _PointBuffer[unity_InstanceID].normal.xyz;
            float4 tangent = _PointBuffer[unity_InstanceID].tangent;

            float3 sn1 = snoise_grad(position * 2 + float3(0, _Time.x * 3, 0));
            float3 sn2 = snoise_grad(position * 2 - float3(_Time.x * 3, 0, 0));
            float3 dfn = cross(sn1, sn2);

            float3 ry = normalize(normal + dfn * _NoiseAmp);
            float3 rx = normalize(cross(ry, tangent.xyz) * tangent.w);
            float3 rz = normalize(cross(rx, ry));

            float scale = max(0, (0.3 + sn1.x * 0.15) * (sin(_Time.y) * 0.3 + 0.7)) * _Scale;

            {
                float3 v1 = rx * scale;
                float3 v2 = ry * scale;
                float3 v3 = rz * scale;

                unity_ObjectToWorld = float4x4(
                    v1.x, v2.x, v3.x, position.x,
                    v1.y, v2.y, v3.y, position.y,
                    v1.z, v2.z, v3.z, position.z,
                    0, 0, 0, 1
                );
            }

            {
                float3 v1 = rx / scale;
                float3 v2 = ry / scale;
                float3 v3 = rz / scale;

                unity_WorldToObject = float4x4(
                    v1.x, v1.y, v1.z, -position.x,
                    v2.x, v2.y, v2.z, -position.y,
                    v3.x, v3.y, v3.z, -position.z,
                    0, 0, 0, 1
                );
            }

        #endif
        }

		void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex * _UVScale;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            uv.x += frac(unity_InstanceID * 0.9389431);
            uv.y += frac(unity_InstanceID * 0.7493248);
#endif
            fixed4 c = tex2D(_MainTex, uv) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
            o.Normal = UnpackScaleNormal(tex2D(_NormalMap, uv), _NormalScale);
		}

		ENDCG
	}
	FallBack "Diffuse"
}
