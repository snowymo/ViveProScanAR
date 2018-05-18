Shader "ViveSR_Experience/sketchShader" {
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

		float LS = 4.0;
		float width = 612.0;
		float height = 460.0;
		fixed3 W = fixed3(0.2125, 0.7154, 0.0721);
		fixed4 textureColor = tex2D(_MainTex, IN.uv_MainTex)*LS;

		fixed2 stp0 = fixed2(1.0 / width, 0.0);
		fixed2 st0p = fixed2(0.0, 1.0 / width);
		fixed2 stpp = fixed2(1.0 / width, 1.0 / height);
		fixed2 stpm = fixed2(1.0 / width, -1.0 / height);

		float i00 = dot(textureColor, W);
		float im1m1 = dot(tex2D(_MainTex, IN.uv_MainTex - stpp).rgb*LS, W);
		float ip1p1 = dot(tex2D(_MainTex, IN.uv_MainTex + stpp).rgb*LS, W);
		float im1p1 = dot(tex2D(_MainTex, IN.uv_MainTex - stpm).rgb*LS, W);
		float ip1m1 = dot(tex2D(_MainTex, IN.uv_MainTex + stpm).rgb*LS, W);
		float im10 = dot(tex2D(_MainTex, IN.uv_MainTex - stp0).rgb*LS, W);
		float ip10 = dot(tex2D(_MainTex, IN.uv_MainTex + stp0).rgb*LS, W);
		float i0m1 = dot(tex2D(_MainTex, IN.uv_MainTex - st0p).rgb*LS, W);
		float i0p1 = dot(tex2D(_MainTex, IN.uv_MainTex + st0p).rgb*LS, W);
		float h = -im1p1 - 2.0 * i0p1 - ip1p1 + im1m1 + 2.0 * i0m1 + ip1m1;
		float v = -im1m1 - 2.0 * im10 - im1p1 + ip1m1 + 2.0 * ip10 + ip1p1;

		float mag = 0.8 - length(fixed2(h, v));
		fixed3 target = fixed3(mag, mag, mag);

		//o.Albedo = lerp(textureColor, target, 0.5);
		o.Albedo = lerp(target, textureColor.rgb, 0.5);
		//o.Albedo = fixed3(0.0, 0.0, 1.0);


		//float3 tc = float3(1.0, 0.0, 0.0);
		//fixed4 col = tex2D(_MainTex, IN.uv_MainTex);
		//float3 colors[3];
		//colors[0] = float3(0.0, 0.0, 1.0);
		//colors[1] = float3(1.0, 1.0, 0.0);
		//colors[2] = float3(1.0, 0.0, 0.0);
		//float lum = (col.r + col.g + col.b) / 3.;
		//int ix = (lum < 0.5) ? 0 : 1;
		//tc = lerp(colors[ix], colors[ix + 1], (lum - float(ix)*0.5) / 0.5);



		// Albedo comes from a texture tinted by color
		//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
		//o.Albedo = tc.rgb;
		// Metallic and smoothness come from slider variables
		//o.Metallic = _Metallic;
		//o.Smoothness = _Glossiness;
		//o.Alpha = 1.0;
	}
	ENDCG
	}
		FallBack "Diffuse"
}
