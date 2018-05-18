Shader "ViveSR_Experience/nightShader" {
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
		float width = 612.0;
		float height = 460.0;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed2 uv = IN.uv_MainTex;
			fixed2 _MainTex_size = fixed2(width, height);

			fixed3 c = tex2D(_MainTex, uv).rgb;
			float d = length(fixed2(uv.x - 0.5, uv.y - 0.56));


			fixed4 cycle = fixed4(1.2 - d / 0.2, 1.0 - d / 0.2, 1.0 - d / 0.2, 1.0);

			o.Albedo = (d > 0.6) ? fixed3(0.0, 0.0, 0.0) : fixed3(c.x*cycle.x*0.1, c.y*cycle.x, c.z*cycle.x*0.2);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
