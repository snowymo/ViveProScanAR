Shader "ViveSR_Experience/sharpShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
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

	void surf(Input IN, inout SurfaceOutputStandard o) {


		fixed2 uv = IN.uv_MainTex;
		fixed3 sample_F[9];
		fixed3 vFragColour;
		//fixed2 _MainTex_size = fixed2(1150.0, 750.0);
		fixed2 _MainTex_size = fixed2(612.0, 460.0);

		for (int j = 0; j < 3; ++j) {
			for (int i = 0; i < 3; ++i) {
				sample_F[j * 3 + i] = tex2D(_MainTex, uv + fixed2(i - 2, j - 2) / _MainTex_size).rgb*2.6;
			}
		}
		vFragColour = 9.0 * sample_F[4];

		for (int i = 0; i < 9; i++)
		{
			if (i != 4)
				vFragColour -= (sample_F[i]*1.04);
		}
		
		o.Albedo = vFragColour;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
