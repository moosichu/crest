﻿Shader "Ocean/Whirpool Surface Alpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
		[Toggle] _ShowFieldTexture("Show Field Texture", float) = 0.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			// Depth offset to stop intersection with water. "Factor" and "Units". typical seems to be (-1,-1). (-0.5,0) gives
			// pretty good results for me when alpha geometry is fairly well matched but fails when alpha geo is too low res.
			// the ludicrously large value below seems to work in most of my tests.
			Offset 0, -1000000

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			// Whether to just display the velocity field
			#pragma shader_feature _SHOWFIELDTEXTURE_ON

			#include "UnityCG.cginc"
			#include "../OceanLODData.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			half _Alpha;

			// MeshScaleLerp, FarNormalsWeight, LODIndex (debug), unused
			uniform float4 _InstanceData;

			v2f vert (appdata v)
			{
				v2f o;

				// move to world
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);

				// vertex snapping and lod transition
				float lodAlpha = ComputeLodAlpha(worldPos, _InstanceData.x);

				// sample shape textures - always lerp between 2 scales, so sample two textures
				half3 n = half3(0., 1., 0.);
				half det = 0., signedOceanDepth = 0.;
				// sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
				float wt_0 = (1. - lodAlpha) * _WD_Params_0.z;
				float wt_1 = (1. - wt_0) * _WD_Params_1.z;
				// sample displacement textures, add results to current world pos / normal / foam
				const float2 wxz = worldPos.xz;
				SampleDisplacements(_WD_Sampler_0, _WD_OceanDepth_Sampler_0, _WD_Pos_Scale_0.xy, _WD_Params_0.y, _WD_Params_0.w, _WD_Params_0.x, wxz, wt_0, worldPos, n, det, signedOceanDepth);
				SampleDisplacements(_WD_Sampler_1, _WD_OceanDepth_Sampler_1, _WD_Pos_Scale_1.xy, _WD_Params_1.y, _WD_Params_1.w, _WD_Params_1.x, wxz, wt_1, worldPos, n, det, signedOceanDepth);

				// view-projection
				o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.));

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);

				float2 uv_from_cent = (i.uv - float2(.5, .5)) * 2.;
				float r2 = dot(uv_from_cent, uv_from_cent);

				float r       =           .1; // eye of whirlpool radius
				const float R =            1; // whirlpool radius
				float2 o      = float2(0, 0); // origin
				float  s      =           .5; // whirlpool 'swirlyness'
				float2 p      = uv_from_cent; // our current position
				float  V      =        100; // maximum whirlpool speed

				float r_2 = r * r;

				float2 PtO  =       o - p;    // vector from position to origin
				float  lPtO = length(PtO);

				if(lPtO >= R) {
					col = fixed4(0,0,0,0);
				} else if (lPtO <= r) {
					col = fixed4(0,0,0,0);
				} else {
					float c = 1.0 - ((lPtO - r) / (R - r));

					float w1 = fmod(_Time, .1);
					float w2 = fmod(_Time + .05, .1);

					// dynamically calvulate current value of velocity field
					// (TODO: Make this a texture lookup?)
					float2 v = V * c * normalize(
						(s * c * normalize(float2(-PtO.y, PtO.x))) +
						((s - 1.0) * (c - 1.0) * normalize(PtO))
					);

					const float PI = 3.14159265358979323846;

					#if _SHOWFIELDTEXTURE_ON
					col = fixed4(v.x, v.y, 0, 1);
					#else
					// Currently using cos to weight each sampling of the texture,
					// probably not the best way to do this.
					col += .25 * (1.0 + cos((w1 * PI * 20.0) - PI)) * tex2D(_MainTex, float2(i.uv - (v * w1 * .05)));
					col += .25 * (1.0 + cos((w2 * PI * 20.0) - PI)) * tex2D(_MainTex, float2(i.uv - (v * w2 * .05)));
					#endif
				}

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
