Shader "Ocean/Whirpool Surface Alpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
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
				fixed4 col = tex2D(_MainTex, i.uv);

				float2 uv_from_cent = (i.uv - float2(.5, .5)) * 2.;
				float r2 = dot(uv_from_cent, uv_from_cent);


				UNITY_APPLY_FOG(i.fogCoord, col);

				if(r2 > 1.) {
					col.a = .0;
				} else {
					float phase = 0;
					float x = uv_from_cent.x;
					float y = uv_from_cent.y;
					const float M_PI_2 = 1.57079632679489661923;
					const float PI = 3.14159265358979323846;
					if(x > .0 && y > .0) {
						phase = atan(x / y);
					} else if(x > .0 && y == .0) {
						phase = M_PI_2;
					} else if (x > .0 && y < .0) {
						phase = M_PI_2 - atan(y/x);
					} else if (x == .0 && y < .0) {
						phase = PI;
					} else if (x < .0 && y < .0) {
						phase = PI + atan(x / y);
					} else if (x < .0 && y == .0) {
						phase = M_PI_2 + PI;
					} else if (x < .0 && y > .0) {
						phase = M_PI_2 + PI - atan(y/x);
					} else if (x == 0 && y > .0) {
						phase = PI * 2.;
					}

					const float frequency = 1.;
					const float speed = 100.;

					float effect = sin(
						((phase + r2) * frequency) -
						(_Time * speed * (1.0 - r2))
					);

					col.rgb = effect;

					col.a *= _Alpha * (1 - r2);
					col.rgb *= r2;
				}

				return col;
			}
			ENDCG
		}
	}
}
