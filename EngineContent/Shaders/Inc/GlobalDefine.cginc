#ifndef	_GobalDefine_shadderinc_
#define _GobalDefine_shadderinc_

//#define half min16float
//#define half2 min16float2
//#define half3 min16float3
//#define half4 min16float4

//Textures
Texture2D PreFrameBuffer;
SamplerState SampPreFrameBuffer;
Texture2D PreFrameDepth;
SamplerState SampPreFrameDepth;

//Structs
struct PointLight
{
	float4		Diffuse;
	float4		Specular;
	float3		Position;
	float		Shiness;
};

//CBuffers
#include "../CBuffer/VarBase_PerCamera.cginc"
#include "../CBuffer/VarBase_PerFrame.cginc"
#include "../CBuffer/VarBase_PerInstance.cginc"
#include "../CBuffer/VarBase_PerViewport.cginc"
#include "../CBuffer/VarBase_PerShadingEnv.cginc"
#include "../CBuffer/VarBase_PerMesh.cginc"

//Functions
#include "../Common.function"

#endif