Shader "Custom/Normals" {
	Properties {
		_MainTex ("", 2D) = "white" {}
	}

	
	SubShader {
		Pass {
			CGINCLUDE

			#include <HLSLSupport.cginc>
			#include <UnityShaderVariables.cginc>

			float4 Output(float depth01, float3 normal)
			{
				float3 c = normal * 0.5 + 0.5;
				return float4(c, 1);
			}
		ENDCG
		}

		Tags { "RenderType" = "Opaque" } // Render only object that are opaque (look through windows)
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				
				struct v2f {
					float4 pos : SV_POSITION;
					float4 nz : TEXCOORD0;
					UNITY_VERTEX_OUTPUT_STEREO
				};
				
				v2f vert( appdata_base v ) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.nz.xyz = COMPUTE_VIEW_NORMAL;
					o.nz.w = COMPUTE_DEPTH_01;
					return o;
				}
				
				float4 frag(v2f i) : SV_Target {
					return Output (i.nz.w, i.nz.xyz);
				}
				ENDCG
		}
	}

	SubShader {
		Tags { "RenderType" = "TransparentCutout" }
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 nz : TEXCOORD1;
					UNITY_VERTEX_OUTPUT_STEREO
				};
				
				uniform float4 _MainTex_ST;
				v2f vert( appdata_base v ) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.nz.xyz = COMPUTE_VIEW_NORMAL;
					o.nz.w = COMPUTE_DEPTH_01;
					return o;
				}
				uniform sampler2D _MainTex;
				float4 frag(v2f i) : SV_Target {
					return Output (i.nz.w, i.nz.xyz);
				}
	ENDCG
		}
	}
		Fallback Off
}
