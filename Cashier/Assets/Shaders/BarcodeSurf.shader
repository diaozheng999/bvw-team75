Shader "Custom/BarcodeSurf" {
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_Background ("Background", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows StandardDefaultGI

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0


		#include "UnityPBSLighting.cginc"

		sampler2D _MainTex;

		inline half4 LightingStandardDefaultGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			return LightingStandard(s, viewDir, gi);
		}

		inline void LightingStandardDefaultGI_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Background;
		float3 _Normal;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = lerp(_Background, _Color, c.a);
			//o.Albedo = fixed3(1,0,0);
			// Albedo comes from a texture tinted by color
			//o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Normal = _Normal;
			o.Emission = 0.3;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
