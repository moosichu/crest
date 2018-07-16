Shader "Ocean/Whirpool Surface Alpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
		[Toggle] _ShowFieldTexture("Show Field Texture", float) = 0.0
		[Toggle] _Foam3DLighting("Foam 3D Lighting", float) = 0.0
		_PhaseMul("Phase Mul", float) = 0.0
		_Swirl("Swirl", float) = 0.5
		_MaxSpeed("Max Speed", float) = 100
		_FoamScale("Foam Scale", float) = 100
		_WaveFoamFeather("Wave Foam Feather", Range(0.001,1.0)) = 0.32
		_WaveFoamLightScale("Wave Foam Light Scale", Range(0.0, 2.0)) = 0.7
		_FoamWhiteColor("White Foam Color", Color) = (1.0, 1.0, 1.0, 1.0)

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
			#pragma shader_feature _FOAM3DLIGHTING_ON

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

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform half _Alpha;
			uniform float _PhaseMul;
			uniform float _Swirl;
			uniform float _MaxSpeed;
			uniform float _FoamScale;
			uniform half _WaveFoamFeather;
			uniform half _WaveFoamLightScale;
			uniform half4 _FoamWhiteColor;
			uniform fixed4 _LightColor0; // TODO: workout what this is

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

			half3 AmbientLight()
			{
				return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
			}

			void ComputeFoam(half i_determinant, float2 i_worldXZUndisplaced, float2 i_worldXZ, half3 i_n, half i_shorelineFoam, float i_pixelZ, float i_sceneZ, half3 i_view, float3 i_lightDir, out half4 o_whiteFoamCol)
			{
				half foamAmount = 1;
				// Wave foam - compute foam amount from determinant
				// > 1: Stretch
				// < 1: Squash
				// < 0: Overlap
				// foamAmount += _WaveFoamStrength * saturate(_WaveFoamCoverage - i_determinant);
				// // Shoreline foam
				// foamAmount += i_shorelineFoam;
				// // feather foam very close to shore
				// foamAmount *= saturate((i_sceneZ - i_pixelZ) / _ShorelineFoamMinDepth);
				float2 _WindDirXZ = float2(.5, .5);

				// White foam on top, with black-point fading
				float2 foamUV = (i_worldXZUndisplaced + 0.05 * _Time * _WindDirXZ) / _FoamScale + 0.02 * i_n.xz;
				half foamTexValue = tex2D(_MainTex, foamUV).r;
				half whiteFoam = foamTexValue * (smoothstep(foamAmount + _WaveFoamFeather, foamAmount, 1. - foamTexValue)) * _FoamWhiteColor.a;

				#if _FOAM3DLIGHTING_ON
				// Scale up delta by Z - keeps 3d look better at distance. better way to do this?
				float2 dd = float2(0.25 * i_pixelZ * _FoamTexture_TexelSize.x, 0.);
				half foamTexValue_x = tex2D(_MainTex, foamUV + dd.xy).r;
				half foamTexValue_z = tex2D(_MainTex, foamUV + dd.yx).r;
				half whiteFoam_x = foamTexValue_x * (smoothstep(foamAmount + _WaveFoamFeather, foamAmount, 1. - foamTexValue_x)) * _FoamWhiteColor.a;
				half whiteFoam_z = foamTexValue_z * (smoothstep(foamAmount + _WaveFoamFeather, foamAmount, 1. - foamTexValue_z)) * _FoamWhiteColor.a;

				// compute a foam normal that is rounded at the edge
				half sqrt_foam = sqrt(whiteFoam);
				half dfdx = sqrt(whiteFoam_x) - sqrt_foam, dfdz = sqrt(whiteFoam_z) - sqrt_foam;
				half3 fN = normalize(half3(-dfdx, _WaveFoamNormalsY, -dfdz));
				half foamNdL = max(0., dot(fN, i_lightDir));
				o_whiteFoamCol.rgb = _FoamWhiteColor.rgb * (AmbientLight() + _WaveFoamLightScale * _LightColor0 * foamNdL);
				#else // _FOAM3DLIGHTING_ON
				o_whiteFoamCol.rgb = _FoamWhiteColor.rgb * (AmbientLight() + _WaveFoamLightScale * _LightColor0);
				#endif // _FOAM3DLIGHTING_ON

				o_whiteFoamCol.a = min(2. * whiteFoam, 1.);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = fixed4(0,0,0,1);

				float2 uv_from_cent = (i.uv - float2(.5, .5)) * 2.;

				float r       =           .1; // eye of whirlpool radius
				const float R =            1; // whirlpool radius
				float2 o      = float2(0, 0); // origin
				float  s      =       _Swirl; // whirlpool 'swirlyness', can vary from 0 - 1
				float2 p      = uv_from_cent; // our current position
				float  V      =        _MaxSpeed; // maximum whirlpool speed

				float2 PtO  =       o - p;    // vector from position to origin
				float  lPtO = length(PtO);

				if(lPtO >= R) {
					col = fixed4(0,0,0,0);
				} else if (lPtO <= r) {
					col = fixed4(0,0,0,0);
				} else {
					float c = 1.0 - ((lPtO - r) / (R - r));

					float phase = _PhaseMul * length(i.uv-0.5);
					const float half_period = .05;
					const float period = half_period * 2;
					float w1 = fmod(_Time + phase, period);
					float w2 = fmod(_Time + phase + half_period, period);

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
					col.a = min(2. * col.r, 1.) - length(uv_from_cent) * length(uv_from_cent);
					col.a = clamp(col.a, 0, 1);
					#endif
				}

				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
