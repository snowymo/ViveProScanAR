Shader "ViveSR/Wireframe"
{
	Properties
    {
        _Thickness ("Wire Thickness", RANGE(0, 800)) = 100
        _Color ("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
		[Enum(Normal, 4, Always, 0)] _ZTest("Z Test Func", Float) = 4
		_StencilValue ("StencilRefValue", float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("Stencil Compare", int) = 0	// disable
    }

    SubShader
    {	
		Tags { "Queue" = "Geometry-1" "RenderType"="Opaque" }

		Stencil{
			Ref  [_StencilValue]
			Comp [_StencilComp]
		}

		Pass
        {
			ZTest [_ZTest]
			ZWrite On
			ColorMask 0
		}

		GrabPass{ "_SeeThroughBeforeWireframeTex" }

        Pass
        {
			// http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf
            CGPROGRAM
			#pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
			#pragma multi_compile __  CLIP_PLANE
            #include "UnityCG.cginc"
			#include "../ViveSRCG.cginc"

            uniform float _Thickness;
            uniform float4 _Color; 
			sampler2D _SeeThroughBeforeWireframeTex;
			DECLARE_CLIP_PLANE_VARIABLE

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 cPos : SV_POSITION;
				WORLD_POS_FORCLIP(0)
            };

            struct g2f
            {
                float4 cPos : SV_POSITION;				
				float3 screenPos : TEXCOORD0;
                float4 dist : TEXCOORD1;
				WORLD_POS_FORCLIP(2)
            };
            
            v2g vert (appdata v)
            {
                v2g o;
                o.cPos = UnityObjectToClipPos(v.vertex);
				COMPUTE_CLIP_WORLD_POS(o, v.vertex)
                return o;
            }
            
            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                float2 p0 = i[0].cPos.xy / i[0].cPos.w;
                float2 p1 = i[1].cPos.xy / i[1].cPos.w;
                float2 p2 = i[2].cPos.xy / i[2].cPos.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                // To find the distance to the opposite edge, we take the
                // formula for finding the area of a triangle Area = Base/2 * Height, 
                // and solve for the Height = (Area * 2)/Base.
                // We can get the area of a triangle by taking its cross product
                // divided by 2.  However we can avoid dividing our area/base by 2
                // since our cross product will already be double our area.
                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _Thickness;

                g2f o;

				ASSIGN_CLIP_WORLD_POS_GEOSHADER( i[0], o )
                o.cPos = i[0].cPos;
				o.screenPos = o.cPos.xyw;
                o.dist.xyz = float3( (area / length(edge0)), 0.0, 0.0) * o.cPos.w * wireThickness;
                o.dist.w = 1.0 / o.cPos.w;
                triangleStream.Append(o);

				ASSIGN_CLIP_WORLD_POS_GEOSHADER( i[1], o )
                o.cPos = i[1].cPos;
				o.screenPos = o.cPos.xyw;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.cPos.w * wireThickness;
                o.dist.w = 1.0 / o.cPos.w;
                triangleStream.Append(o);

				ASSIGN_CLIP_WORLD_POS_GEOSHADER( i[2], o )
                o.cPos = i[2].cPos;
				o.screenPos = o.cPos.xyw;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.cPos.w * wireThickness;
                o.dist.w = 1.0 / o.cPos.w;
                triangleStream.Append(o);
            }

            float4 frag (g2f i) : SV_Target
            {
				CLIP_PLANE_TEST(i)

				float2 scrPos = i.screenPos.xy / i.screenPos.z;
				scrPos.x = 0.5 + scrPos.x * 0.5;
				scrPos.y = 0.5 - scrPos.y * 0.5;

                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];
				float4 bgColor = tex2D(_SeeThroughBeforeWireframeTex, scrPos);

				// not on a line
				if (minDistanceToEdge > 0.9)
					return bgColor;

				// Smooth our line out
                float t = exp2(-2 * minDistanceToEdge * minDistanceToEdge);
                fixed4 finalColor = lerp(bgColor, _Color, t);
                return finalColor;
            }
			ENDCG
        }
    }
}
