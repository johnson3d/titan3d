#ifndef	_GobalDefine_shadderinc_
#define _GobalDefine_shadderinc_


//RHI_TYPE
#define RHI_DX11 1
#define RHI_DX12 2
#define RHI_GL 3
#define RHI_MTL 4
#define RHI_VK 5

//ShaderStage
#define ShaderStage_VS 0
#define ShaderStage_PS 1
#define ShaderStage_CS 2

//MTL_NORMAL_MODE
#define MTL_NORMAL 1
#define MTL_NORMALMAP 2
#define MTL_NORMALNONE 3

#define Combine3(a,b,c) a##b##c
#define Combine2(a,b) Combine3(a,b,)

#if RHI_TYPE == RHI_DX11
	#define DX_AUTOBIND
	#define DX_BIND_B(n) : register(b##n)
	#define DX_BIND_T(n) : register(t##n)
	#define DX_BIND_S(s) : register(s##n)
	#define DX_BIND_U(n) : register(u##n)
#elif RHI_TYPE == RHI_DX12
	#define DX_AUTOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#elif RHI_TYPE == RHI_VK
	#define DX_AUTOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#elif RHI_TYPE == RHI_MTL
	#define DX_AUTOBIND : register(Combine2(space,ShaderStage))
	#define DX_BIND_B(n) : register(b##n, Combine2(space,ShaderStage))
	#define DX_BIND_T(n) : register(t##n, Combine2(space,ShaderStage))
	#define DX_BIND_S(n) : register(s##n, Combine2(space,ShaderStage))
	#define DX_BIND_U(n) : register(u##n, Combine2(space,ShaderStage))
#else
	#define DX_AUTOBIND 
	#define DX_BIND_B(n) 
	#define DX_BIND_T(n) 
	#define DX_BIND_S(n) 
	#define DX_BIND_U(n) 
#endif

#if RHI_TYPE == RHI_DX11
	#define VK_BIND(n) 
	#define VK_LOCATION(n) 
	#define VK_OFFSET(n) 
#elif RHI_TYPE == RHI_DX12
	#define VK_BIND(n) 
	#define VK_LOCATION(n) 
	#define VK_OFFSET(n) 
#else
	#define VK_BIND(n) [[vk::binding(n, ShaderStage)]]
	#define VK_LOCATION(n) [[vk::location(n)]]
	#define VK_OFFSET(n) [[vk::offset(n)]]
#endif

// Works around bug in the spirv for the missing implementation of the and() and or() intrinsics.
bool  and_internal(bool  a, bool  b) { return bool(a && b); }
bool2 and_internal(bool2 a, bool2 b) { return bool2(a.x && b.x, a.y && b.y); }
bool3 and_internal(bool3 a, bool3 b) { return bool3(a.x && b.x, a.y && b.y, a.z && b.z); }
bool4 and_internal(bool4 a, bool4 b) { return bool4(a.x && b.x, a.y && b.y, a.z && b.z, a.w && b.w); }

bool  or_internal(bool  a, bool  b) { return bool(a || b); }
bool2 or_internal(bool2 a, bool2 b) { return bool2(a.x || b.x, a.y || b.y); }
bool3 or_internal(bool3 a, bool3 b) { return bool3(a.x || b.x, a.y || b.y, a.z || b.z); }
bool4 or_internal(bool4 a, bool4 b) { return bool4(a.x || b.x, a.y || b.y, a.z || b.z, a.w || b.w); }

#define and(a, b) and_internal(a, b)
#define or(a, b) or_internal(a, b)

//#define half min16float
//#define half2 min16float2
//#define half3 min16float3
//#define half4 min16float4

void SetIndirectDispatchArg(RWByteAddressBuffer buffer, int offset, uint3 dispatchArg, uint drawId)
{
#if RHI_TYPE == RHI_DX12
	buffer.Store(offset + 0 * 4, drawId);
#endif
	buffer.Store(offset + 1 * 4, dispatchArg.x);//dispatchx
	buffer.Store(offset + 2 * 4, dispatchArg.y);//dispatchy
	buffer.Store(offset + 3 * 4, dispatchArg.z);//dispatchz
}

void SetIndirectDrawIndexArg(RWByteAddressBuffer buffer, int offset, 
	uint IndexCountPerInstance, uint InstanceCount, uint StartIndexLocation, 
	uint BaseVertexLocation, uint StartInstanceLocation, uint drawId)
{
#if RHI_TYPE == RHI_DX11
	buffer.Store(offset + 0 * 4, drawId);
#endif
	buffer.Store(offset + 1 * 4, IndexCountPerInstance);
	buffer.Store(offset + 2 * 4, InstanceCount);
	buffer.Store(offset + 3 * 4, StartIndexLocation);
	buffer.Store(offset + 4 * 4, BaseVertexLocation);
	buffer.Store(offset + 5 * 4, StartInstanceLocation);
}

#endif