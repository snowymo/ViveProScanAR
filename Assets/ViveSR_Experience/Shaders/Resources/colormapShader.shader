Shader "ViveSR_Experience/colormapShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_colorMap("colormap", 2D) = "white" {}
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
		sampler2D _colorMap;
	struct Input {
		float2 uv_MainTex;
	};


	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		// Albedo comes from a texture tinted by color
		//fixed4 c = tex2D (_MainTex, IN.uv_MainTex)*10.0;


		fixed2 uv = IN.uv_MainTex;


		fixed3 col = tex2D(_MainTex, fixed2(uv.x,1-uv.y)).rgb;
		//o.Albedo = fixed3(0.0, 0.5, 0.0);

		if (col.x < 0){

			o.Albedo = fixed3(0.0,0.0,0.0);
		}
		else{



			if (col.x > 250)
			{
			
				col.x = 250;
			
			}
				
			if (col.x < 30)
			{
				col.x = 30;
			}
		
			fixed2 new_uv = fixed2(0.5, (col.x - 30.0) / 220);
			if (new_uv.y < 0.05)
				new_uv.y = 0.05;
			if (new_uv.y > 0.95)
				new_uv.y = 0.95;


			fixed3 colMap = tex2D(_colorMap, new_uv).rgb;
			//col.x
			o.Albedo = colMap;


		}




	}
	ENDCG
	}
		FallBack "Diffuse"
}
