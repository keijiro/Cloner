Shader "Cloner/Default"
{
    Properties
    {
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
        #pragma instancing_options procedural:setup

        struct Input { float2 uv_MainTex; };

        sampler2D _MainTex;
        half _UVScale;
        sampler2D _NormalMap;
        half _NormalScale;
        half4 _Color;
        half _Smoothness;
        half _Metallic;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> _TransformBuffer;
        uint _InstanceCount;
        #endif

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            uint id = unity_InstanceID;

            float4 ps = _TransformBuffer[id + _InstanceCount * 0];
            float3 rx = _TransformBuffer[id + _InstanceCount * 1];
            float3 ry = _TransformBuffer[id + _InstanceCount * 2];
            float3 rz = cross(rx, ry);

            float3 v1 = rx * ps.w;
            float3 v2 = ry * ps.w;
            float3 v3 = rz * ps.w;

            unity_ObjectToWorld = float4x4(
                v1.x, v2.x, v3.x, ps.x,
                v1.y, v2.y, v3.y, ps.y,
                v1.z, v2.z, v3.z, ps.z,
                0, 0, 0, 1
            );

            float3 v4 = rx / ps.w;
            float3 v5 = ry / ps.w;
            float3 v6 = rz / ps.w;

            unity_WorldToObject = float4x4(
                v1.x, v1.y, v1.z, -ps.x,
                v2.x, v2.y, v2.z, -ps.x,
                v3.x, v3.y, v3.z, -ps.x,
                0, 0, 0, 1
            );

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
