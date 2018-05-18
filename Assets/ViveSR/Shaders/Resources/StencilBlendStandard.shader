Shader "ViveSR/BlendStandard, Stencil" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendSrc("Blend Src", int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendDst("Blend Dst", int) = 0
		_StencilValue ("StencilRefValue", float) = 1
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Compare", int) = 0	// disable
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestComp("ZTest Compare", int) = 4		// lequal
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200
		Blend [_BlendSrc] [_BlendDst]
		
		ZTest[_ZTestComp]
		Stencil{
			Ref [_StencilValue]
			Comp[_StencilComp]
		}

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows alpha
		#pragma target 3.0
		#pragma multi_compile __  CLIP_PLANE
		#include "../ViveSRCG.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			WORLD_POS_FORCLIP_SURF
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		DECLARE_CLIP_PLANE_VARIABLE

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			CLIP_PLANE_TEST(IN)
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
