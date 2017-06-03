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

        #pragma surface surf Standard vertex:vert addshadow
        #pragma instancing_options procedural:setup
        #pragma target 5.0

        struct Input
        {
            float2 uv_MainTex;
            float Param : COLOR;
        };

        sampler2D _MainTex;
        half _UVScale;
        sampler2D _NormalMap;
        half _NormalScale;
        half4 _Color;
        half _Smoothness;
        half _Metallic;

        half3 _GradientA;
        half3 _GradientB;
        half3 _GradientC;
        half3 _GradientD;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructuredBuffer<float4> _TransformBuffer;
        uint _InstanceCount;
        #endif

        half3 CosineGradient(half param)
        {
            half3 c = _GradientB * cos(_GradientC * param + _GradientD);
            return GammaToLinearSpace(saturate(c + _GradientA));
        }

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

            uint id = unity_InstanceID;

            float4 ps = _TransformBuffer[id + _InstanceCount * 0];
            float3 rx = _TransformBuffer[id + _InstanceCount * 1];
            float3 ry = _TransformBuffer[id + _InstanceCount * 2];
            float3 rz = cross(rx, ry);

            float3 v1 = rx * ps.w*1.4;
            float3 v2 = ry * ps.w*1.4;
            float3 v3 = rz * ps.w;

            unity_ObjectToWorld = float4x4(
                v1.x, v2.x, v3.x, ps.x,
                v1.y, v2.y, v3.y, ps.y,
                v1.z, v2.z, v3.z, ps.z,
                0, 0, 0, 1
            );

            float3 v4 = rx / ps.w/1.4;
            float3 v5 = ry / ps.w/1.4;
            float3 v6 = rz / ps.w;

            unity_WorldToObject = float4x4(
                v1.x, v1.y, v1.z, -ps.x,
                v2.x, v2.y, v2.z, -ps.x,
                v3.x, v3.y, v3.z, -ps.x,
                0, 0, 0, 1
            );

            #endif
        }

        void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            v.color = 0.5 + _TransformBuffer[unity_InstanceID + _InstanceCount * 2].w;
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {

            float2 uv = IN.uv_MainTex * _UVScale;
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            uint id = unity_InstanceID;
            uv.x += frac(id * 0.9389431);
            uv.y += frac(id * 0.7493248);
            #endif

            half3 c = tex2D(_MainTex, uv).xyz * _Color * CosineGradient(IN.Param);

            o.Albedo = c;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = UnpackScaleNormal(tex2D(_NormalMap, uv), _NormalScale);
        }

        ENDCG
    }
    FallBack "Diffuse"
}
