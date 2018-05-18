Shader "ViveSR_Experience/thermalShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float3 tc = float3(1.0, 0.0, 0.0);
			fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
			float3 colors[3];
			colors[0] = float3(0.0, 0.0, 1.0);
			colors[1] = float3(1.0, 1.0, 0.0);
			colors[2] = float3(1.0, 0.0, 0.0);
			float lum = (col.r + col.g + col.b) / 3.;
			int ix = (lum < 0.5) ? 0 : 1;
			tc = lerp(colors[ix], colors[ix + 1], (lum - float(ix)*0.5) / 0.5);



			// Albedo comes from a texture tinted by color
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = tc.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			//o.Alpha =1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
