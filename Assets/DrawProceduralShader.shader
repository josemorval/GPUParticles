// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "DrawProceduralShader"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			StructuredBuffer<float4> _quadCornersBuffer;
			StructuredBuffer<float4> _positionsBuffer;

			float _numberParticles;

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (uint id : SV_VertexID)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(_positionsBuffer[id/6]+_quadCornersBuffer[id%6]);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(1.0,0.0,0.0,1.0);
			}
			ENDCG
		}
	}
}
