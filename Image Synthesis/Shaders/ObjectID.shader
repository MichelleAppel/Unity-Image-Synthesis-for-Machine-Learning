Shader "Custom/ObjectId" {
    Properties {
        _MainTex ("", 2D) = "white" {}
        _ObjectColor ("Object Color", Color) = (1, 1, 1, 1)
    }

    CGINCLUDE
    #include <HLSLSupport.cginc>
    fixed4 _ObjectColor;
    float4 Output() {
        return _ObjectColor;
    }
    ENDCG

    SubShader {
        Tags { "RenderType" = "Opaque" }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
                
            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                return Output();
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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform float4 _MainTex_ST;
            v2f vert(appdata_base v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            uniform sampler2D _MainTex;

            float4 frag(v2f i) : SV_Target {
                return Output();
            }
            ENDCG
        }
    }
    Fallback Off
}
