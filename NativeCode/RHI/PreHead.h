#pragma once

#include "../Base/BaseHead.h"
#include "../Base/CoreRtti.h"
#include "../Base/IUnknown.h"
#include "../Base/debug/vfxdebug.h"
#include "../Base/TypeUtility.h"

#define RHI_ASSERT assert

#define RHI_Trace _vfxTraceA

#if _DEBUG
#define AssertRHI(bool_expr)                       assert(bool_expr)
#else
#define AssertRHI(bool_expr)                    do { if (false) (void)(bool_expr); } while(0)
#endif

NS_BEGIN

const int MaxCB = 16;
const UINT32 MaxTexOrSamplerSlot = 16;

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
ERHIType
{
	RHT_VirtualDevice,
	RHT_D3D11,
	RHT_VULKAN,
	RHT_OGL,
	RHIType_Metal,
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EColorSpace{
	COLOR_SPACE_SRGB_NONLINEAR = 0,
	COLOR_SPACE_EXTENDED_SRGB_LINEAR,
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EPixelFormat
{
	PXF_UNKNOWN = 0,
	PXF_R16_FLOAT =1,
	PXF_R16_UINT =2,
	PXF_R16_SINT =3,
	PXF_R16_UNORM =4,
	PXF_R16_SNORM =5,
	PXF_R32_UINT =6,
	PXF_R32_SINT =7,
	PXF_R32_FLOAT =8,
	PXF_R8G8B8A8_SINT =9,
	PXF_R8G8B8A8_UINT =10,
	PXF_R8G8B8A8_UNORM =11,
	PXF_R8G8B8A8_SNORM =12,
	PXF_R16G16_UINT =13,
	PXF_R16G16_SINT =14,
	PXF_R16G16_FLOAT =15,
	PXF_R16G16_UNORM =16,
	PXF_R16G16_SNORM =17,
	PXF_R16G16B16A16_UINT =18,
	PXF_R16G16B16A16_SINT =19,
	PXF_R16G16B16A16_FLOAT =20,
	PXF_R16G16B16A16_UNORM =21,
	PXF_R16G16B16A16_SNORM =22,
	PXF_R32G32B32A32_UINT =23,
	PXF_R32G32B32A32_SINT =24,
	PXF_R32G32B32A32_FLOAT =25,
	PXF_R32G32B32_UINT = 26,
	PXF_R32G32B32_SINT = 27,
	PXF_R32G32B32_FLOAT = 28,
	PXF_R32G32_UINT = 29,
	PXF_R32G32_SINT = 30,
	PXF_R32G32_FLOAT = 31,
	PXF_D24_UNORM_S8_UINT =32,
	PXF_D32_FLOAT =33,
	PXF_D32_FLOAT_S8X24_UINT =34,
	PXF_D16_UNORM =35,
	PXF_B8G8R8A8_UNORM = 36,
	PXF_R11G11B10_FLOAT = 37,
	PXF_R8G8_UNORM = 38,
	PXF_R8_UNORM = 39,
	PXF_R32_TYPELESS = 40,
	PXF_R32G32B32A32_TYPELESS = 41,
	PXF_R32G32B32_TYPELESS ,
	PXF_R16G16B16A16_TYPELESS,
	PXF_R32G32_TYPELESS,
	PXF_R32G8X24_TYPELESS,
	PXF_R10G10B10A2_TYPELESS,
	PXF_R10G10B10A2_UNORM,
	PXF_R10G10B10A2_UINT,
	PXF_R8G8B8A8_TYPELESS,
	PXF_R8G8B8A8_UNORM_SRGB,
	PXF_R16G16_TYPELESS,
	PXF_R24G8_TYPELESS,
	PXF_R24_UNORM_X8_TYPELESS,
	PXF_X24_TYPELESS_G8_UINT,
	PXF_R8G8_TYPELESS,
	PXF_R8G8_UINT,
	PXF_R8G8_SNORM,
	PXF_R8G8_SINT,
	PXF_R16_TYPELESS,
	PXF_R8_TYPELESS,
	PXF_R8_UINT,
	PXF_R8_SNORM,
	PXF_R8_SINT,
	PXF_A8_UNORM,
	PXF_B8G8R8X8_UNORM,
	PXF_B8G8R8A8_TYPELESS,
	PXF_B8G8R8A8_UNORM_SRGB,
	PXF_B8G8R8X8_TYPELESS,
	PXF_B8G8R8X8_UNORM_SRGB,
	PXF_B5G6R5_UNORM,
	PXF_B4G4R4A4_UNORM,
};

inline UINT GetPixelByteWidth(EPixelFormat fmt)
{
	switch (fmt)
	{
	case PXF_R16_FLOAT:
		return 2;
	case PXF_R16_UINT:
		return 2;
	case PXF_R16_SINT:
		return 2;
	case PXF_R16_UNORM:
		return 2;
	case PXF_R16_SNORM:
		return 2;
	case PXF_R32_UINT:
		return 4;
	case PXF_R32_SINT:
		return 4;
	case PXF_R32_FLOAT:
		return 4;
	case PXF_R8G8B8A8_SINT:
		return 4;
	case PXF_R8G8B8A8_UINT:
		return 4;
	case PXF_R8G8B8A8_UNORM:
		return 4;
	case PXF_R8G8B8A8_SNORM:
		return 4;
	case PXF_R16G16_UINT:
		return 4;
	case PXF_R16G16_SINT:
		return 4;
	case PXF_R16G16_FLOAT:
		return 4;
	case PXF_R16G16_UNORM:
		return 4;
	case PXF_R16G16_SNORM:
		return 4;
	case PXF_R16G16B16A16_UINT:
		return 8;
	case PXF_R16G16B16A16_SINT:
		return 8;
	case PXF_R16G16B16A16_FLOAT:
		return 8;
	case PXF_R16G16B16A16_UNORM:
		return 8;
	case PXF_R16G16B16A16_SNORM:
		return 8;
	case PXF_R32G32B32A32_UINT:
		return 16;
	case PXF_R32G32B32A32_SINT:
		return 16;
	case PXF_R32G32B32A32_FLOAT:
		return 16;
	case PXF_R32G32B32_UINT:
		return 12;
	case PXF_R32G32B32_SINT:
		return 12;
	case PXF_R32G32B32_FLOAT:
		return 12;
	case PXF_R32G32_UINT:
		return 8;
	case PXF_R32G32_SINT:
		return 8;
	case PXF_R32G32_FLOAT:
		return 8;
	case PXF_D24_UNORM_S8_UINT:
		return 4;
	case PXF_D32_FLOAT:
		return 4;
	case PXF_D32_FLOAT_S8X24_UINT:
		return 8;
	case PXF_D16_UNORM:
		return 2;
	default:
		return 0;
	}
}

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EBindFlags
{
	BF_VB = 0x1,
	BF_IB = 0x2,
	BF_CB = 0x4,
	BF_SHADER_RES = 0x8,
	BF_STREAM_OUTPUT = 0x10,
	BF_RENDER_TARGET = 0x20,
	BF_DEPTH_STENCIL = 0x40,
	BF_UNORDERED_ACCESS = 0x80
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
ECpuAccess
{
	CAS_WRITE = 0x10000,
	CAS_READ = 0x20000,
};

struct IBlobObject;
struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
ImageInitData
{
	ImageInitData()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		pSysMem = nullptr;
		SysMemPitch = 0;
		SysMemSlicePitch = 1;
	}
	const void* pSysMem;
	UINT SysMemPitch;
	UINT SysMemSlicePitch;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
MacroDefine
{
	MacroDefine(const char* name, const char* definition)
	{
		Name = name;
		Definition = definition;
	}
	std::string Name;
	std::string Definition;
	TR_FUNCTION()
	const char* GetName() const {
		return Name.c_str();
	}
	TR_FUNCTION()
		const char* GetDefinition() const {
		return Definition.c_str();
	}
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EShaderVarType
{
	SVT_Float1,
	SVT_Float2,
	SVT_Float3,
	SVT_Float4,

	SVT_Int1,
	SVT_Int2,
	SVT_Int3,
	SVT_Int4,
	
	SVT_Matrix4x4,
	SVT_Matrix3x3,

	SVT_Texture,
	SVT_Sampler,
	SVT_Struct,
	SVT_Unknown,
};

inline int GetShaderVarTypeSize(EShaderVarType type)
{
	switch (type)
	{
	case SVT_Float1:
		return 4;
	case SVT_Float2:
		return 8;
	case SVT_Float3:
		return 12;
	case SVT_Float4:
		return 16;
	case SVT_Int1:
		return 4;
	case SVT_Int2:
		return 8;
	case SVT_Int3:
		return 12;
	case SVT_Int4:
		return 16;
	case SVT_Matrix4x4:
		return 64;
	case SVT_Matrix3x3:
		return 36;
	default:
		return -1;
	}
}

inline void RgbaToColor4(UINT rgba, float color[4])
{
	color[0] = (float)(rgba & 0xFF) / 255.0f;
	color[1] = (float)((rgba >> 8) & 0xFF) / 255.0f;
	color[2] = (float)((rgba >> 16) & 0xFF) / 255.0f;
	color[3] = (float)((rgba >> 24) & 0xFF) / 255.0f;
}

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
ESamplerFilter
{
	SPF_MIN_MAG_MIP_POINT = 0,
	SPF_MIN_MAG_POINT_MIP_LINEAR = 0x1,
	SPF_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x4,
	SPF_MIN_POINT_MAG_MIP_LINEAR = 0x5,
	SPF_MIN_LINEAR_MAG_MIP_POINT = 0x10,
	SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x11,
	SPF_MIN_MAG_LINEAR_MIP_POINT = 0x14,
	SPF_MIN_MAG_MIP_LINEAR = 0x15,
	SPF_ANISOTROPIC = 0x55,
	SPF_COMPARISON_MIN_MAG_MIP_POINT = 0x80,
	SPF_COMPARISON_MIN_MAG_POINT_MIP_LINEAR = 0x81,
	SPF_COMPARISON_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x84,
	SPF_COMPARISON_MIN_POINT_MAG_MIP_LINEAR = 0x85,
	SPF_COMPARISON_MIN_LINEAR_MAG_MIP_POINT = 0x90,
	SPF_COMPARISON_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x91,
	SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT = 0x94,
	SPF_COMPARISON_MIN_MAG_MIP_LINEAR = 0x95,
	SPF_COMPARISON_ANISOTROPIC = 0xd5,
	SPF_MINIMUM_MIN_MAG_MIP_POINT = 0x100,
	SPF_MINIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x101,
	SPF_MINIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x104,
	SPF_MINIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x105,
	SPF_MINIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x110,
	SPF_MINIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x111,
	SPF_MINIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x114,
	SPF_MINIMUM_MIN_MAG_MIP_LINEAR = 0x115,
	SPF_MINIMUM_ANISOTROPIC = 0x155,
	SPF_MAXIMUM_MIN_MAG_MIP_POINT = 0x180,
	SPF_MAXIMUM_MIN_MAG_POINT_MIP_LINEAR = 0x181,
	SPF_MAXIMUM_MIN_POINT_MAG_LINEAR_MIP_POINT = 0x184,
	SPF_MAXIMUM_MIN_POINT_MAG_MIP_LINEAR = 0x185,
	SPF_MAXIMUM_MIN_LINEAR_MAG_MIP_POINT = 0x190,
	SPF_MAXIMUM_MIN_LINEAR_MAG_POINT_MIP_LINEAR = 0x191,
	SPF_MAXIMUM_MIN_MAG_LINEAR_MIP_POINT = 0x194,
	SPF_MAXIMUM_MIN_MAG_MIP_LINEAR = 0x195,
	SPF_MAXIMUM_ANISOTROPIC = 0x1d5
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EAddressMode
{
	ADM_WRAP = 1,
	ADM_MIRROR,
	ADM_CLAMP,
	ADM_BORDER,
	ADM_MIRROR_ONCE,
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EComparisionMode
{
	CMP_NEVER = 1,
	CMP_LESS = 2,
	CMP_EQUAL = 3,
	CMP_LESS_EQUAL = 4,
	CMP_GREATER = 5,
	CMP_NOT_EQUAL = 6,
	CMP_GREATER_EQUAL = 7,
	CMP_ALWAYS = 8
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EFillMode
{
	FMD_WIREFRAME = 2,
	FMD_SOLID = 3
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
ECullMode
{
	CMD_NONE = 1,
	CMD_FRONT = 2,
	CMD_BACK = 3
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EDepthWriteMask
{
	DSWM_ZERO = 0,
	DSWM_ALL = 1
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_EnumNoFlags)
EStencilOp
{
	STOP_KEEP = 1,
	STOP_ZERO = 2,
	STOP_REPLACE = 3,
	STOP_INCR_SAT = 4,
	STOP_DECR_SAT = 5,
	STOP_INVERT = 6,
	STOP_INCR = 7,
	STOP_DECR = 8
};

NS_END

