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

			float _numberParticles;
			float _sizeParticle;

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (uint id : SV_VertexID)
			{
				v2f o;

				uint k = id%6;
				int idi = id/6;
				float3 pos = float3(cos(2.0*3.14159*idi/_numberParticles+_Time.y),sin(2.0*3.14159*idi/_numberParticles+_Time.y),0.0);
				pos*=10.0;
				float3 f = float3(0.0,0.0,0.0);
				float d = _sizeParticle;

				//Depending on vertex index we go to one or other corner of the quad
				f.x = lerp(d,-d,step(3.0,(k-2+6)%6));
				f.y = lerp(d,-d,step(3.0,(k-1+6)%6));

				o.vertex = mul(unity_ObjectToWorld,float4(f+pos,1.0));
				o.vertex = mul(UNITY_MATRIX_VP,o.vertex);
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
