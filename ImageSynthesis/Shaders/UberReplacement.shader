// Based on builtin Internal-DepthNormalsTexture.shader
// EncodeDepthNormal() is replaced with custom Output() function


Shader "Hidden/UberReplacement" {
	Properties {
	_MainTex ("", 2D) = "white" {}
	_Cutoff ("", Float) = 0.5
	_Color ("", Color) = (1,1,1,1)

	_ObjectColor ("Object Color", Color) = (1,1,1,1)
	_CategoryColor ("Catergory Color", Color) = (0,1,0,1)
	_Outlines ("Outlines", Color) = (1,1,1,1)
	_GroundTruth ("Ground Truth", int) = 1
}

	
SubShader {	
	//Tags { "RenderType" = "Opaque"} // Render only object that are opaque (look through windows)
	Pass {
		
	CGINCLUDE

fixed4 _ObjectColor;
fixed4 _CategoryColor;
fixed4 _Outlines;
int _GroundTruth;
int _OutputMode;

	
// remap depth: [0 @ eye .. 1 @ far] => [0 @ near .. 1 @ far]
inline float Linear01FromEyeToLinear01FromNear(float depth01)
{
	float near = _ProjectionParams.y;
	float far = _ProjectionParams.z;
	return (depth01 - near/far) * (1 + near/far);
}

fixed4 Output(float depth01, float3 normal)
{
	/* see ImageSynthesis.cs
	enum ReplacelementModes {
		ObjectId 			= 0,
		CatergoryId			= 1,
		DepthCompressed		= 2,
		DepthMultichannel	= 3,
		Normals				= 4
	};*/

	if (_OutputMode == 0) // ObjectId
	{
		return _ObjectColor;
	}
	else if (_OutputMode == 1) // CatergoryId
	{
		return _CategoryColor;
	}
	else if (_OutputMode == 2) // DepthCompressed
	{
		float linearZFromNear = Linear01FromEyeToLinear01FromNear(depth01); 
		float k = 0.25; // compression factor
		return pow(linearZFromNear, k);
	}
	else if (_OutputMode == 3) // DepthMultichannel
	{
		float lowBits = frac(depth01 * 256);
		float highBits = depth01 - lowBits / 256;
		return float4(lowBits, highBits, depth01, 1);
	}
	else if (_OutputMode == 4) // Normals
	{
		// [-1 .. 1] => [0 .. 1]
		float3 c = normal * 0.5 + 0.5;
		return float4(c, 1);
	}
	else if (_OutputMode == 5) // Outlines
	{
		return _Outlines;
	}
	else if (_OutputMode == 6) // Ground truth
	{
		return (float) _GroundTruth/1000 + 0.005;
	}

	// unsupported _OutputMode
	return float4(1, 0.5, 0.5, 1);
}
	
ENDCG
		}

// Support for different RenderTypes
// The following code is based on builtin Internal-DepthNormalsTexture.shader

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
uniform fixed _Cutoff;
uniform fixed4 _Color;
float4 frag(v2f i) : SV_Target {
	return Output (i.nz.w, i.nz.xyz);
}
ENDCG
	}
}
	Fallback Off
}
