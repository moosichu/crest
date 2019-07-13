Shader "Crest/Underwater Horizon Marker"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		// We need to render just before the ocean does
		Tags { "Queue"="Geometry+509" }

		Pass
		{
			Stencil {
				Ref 2
				Comp always
				Pass replace
				ZFail replace
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _HorizonHeight;
			float _HorizonRoll;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				// TODO: Move verts so that they always fill up the the horizon
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.vertex.y *= _HorizonHeight;
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb = fixed3(0.0, 0.0, 0.0);
				return col;
			}
			ENDCG
		}
	}
}
