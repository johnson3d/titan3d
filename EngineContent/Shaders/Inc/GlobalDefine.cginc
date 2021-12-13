#ifndef	_GobalDefine_shadderinc_
#define _GobalDefine_shadderinc_

#ifndef ShaderModel
#define ShaderModel 5
#endif

#define RHI_DX11 1
#define RHI_DX12 1
#define RHI_GL 2
#define RHI_MTL 3
#define RHI_VK 4

#define VSStage 0
#define PSStage 1
#define CSStage 2

#define Combine3(a,b,c) a##b##c
#define Combine2(a,b) Combine3(a,b,)

#if RHI_TYPE == RHI_DX11
	#define DX_NOBIND
	#define DX_BIND_B(n) : register(b##n)
	#define DX_BIND_T(n) : register(t##n)
	#define DX_BIND_S(s) : register(s##n)
	#define DX_BIND_U(n) : register(u##n)
#elif RHI_TYPE == RHI_DX12
	#define DX_NOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#elif RHI_TYPE == RHI_VK
	#define DX_NOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#elif RHI_TYPE == RHI_MTL
	#define DX_NOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#else
	#define DX_NOBIND 
	#define DX_BIND_B(n) 
	#define DX_BIND_T(n) 
	#define DX_BIND_S(n) 
	#define DX_BIND_U(n) 
#endif

#if RHI_TYPE == RHI_DX11
	#define VK_BIND(n) 
	#define VK_LOCATION(n) 
	#define VK_OFFSET(n) 
#else
	#define VK_BIND(n) [[vk::binding(n, ShaderStage)]]
	#define VK_LOCATION(n) [[vk::location(n)]]
	#define VK_OFFSET(n) [[vk::offset(n)]]
#endif

//#define half min16float
//#define half2 min16float2
//#define half3 min16float3
//#define half4 min16float4

//CBuffers
#include "../CBuffer/VarBase_PerCamera.cginc"
#include "../CBuffer/VarBase_PerFrame.cginc"
#include "../CBuffer/VarBase_PerMaterial.cginc"
#include "../CBuffer/VarBase_PerViewport.cginc"
#include "../CBuffer/VarBase_PerMesh.cginc"

//Functions
#include "SysFunction.cginc"

#endif