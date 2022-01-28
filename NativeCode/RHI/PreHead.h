#pragma once

#include "../Base/BaseHead.h"
#include "../Base/CoreRtti.h"
#include "../Base/IUnknown.h"
#include "../Base/debug/vfxdebug.h"
#include "../Base/string/vfxstring.h"
#include "../Base/TypeUtility.h"

#if defined(PLATFORM_WIN)
#define USE_VirtualDevice
#define USE_D11
#define USE_GL
#define USE_VK
#elif defined(PLATFORM_DROID)
#define USE_GL
#elif defined(PLATFORM_IOS)
#define USE_METAL
#endif

NS_BEGIN

const int MaxCB = 16;
const UINT32 MaxTexOrSamplerSlot = 16;

enum TR_ENUM(SV_EnumNoFlags)
ERHIType
{
	RHT_VirtualDevice,
	RHT_D3D11,
	RHT_VULKAN,
	RHT_OGL,
	RHIType_Metal,
};

enum TR_ENUM(SV_EnumNoFlags = true)
EColorSpace{
	COLOR_SPACE_SRGB_NONLINEAR = 0,
	COLOR_SPACE_EXTENDED_SRGB_LINEAR,
};

enum TR_ENUM(SV_EnumNoFlags = true)
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
	case PXF_R8G8B8A8_UINT:
	case PXF_R8G8B8A8_UNORM:
	case PXF_R8G8B8A8_SNORM:
	case PXF_R8G8B8A8_TYPELESS:
	case PXF_R8G8B8A8_UNORM_SRGB:
		return 4;
	case PXF_B8G8R8A8_UNORM:
	case PXF_B8G8R8A8_TYPELESS:
	case PXF_B8G8R8A8_UNORM_SRGB:
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
		ASSERT(false);
		return 0;
	}
}

enum TR_ENUM()
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

enum TR_ENUM()
ECpuAccess
{
	CAS_WRITE = 0x10000,
	CAS_READ = 0x20000,
};

struct IBlobObject;
struct TR_CLASS(SV_LayoutStruct = 8)
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

struct TR_CLASS()
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

enum TR_ENUM(SV_EnumNoFlags = true)
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

enum TR_ENUM(SV_EnumNoFlags = true)
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

enum TR_ENUM(SV_EnumNoFlags = true)
EAddressMode
{
	ADM_WRAP = 1,
	ADM_MIRROR,
	ADM_CLAMP,
	ADM_BORDER,
	ADM_MIRROR_ONCE,
};

enum TR_ENUM(SV_EnumNoFlags = true)
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

enum TR_ENUM(SV_EnumNoFlags = true)
EFillMode
{
	FMD_WIREFRAME = 2,
	FMD_SOLID = 3
};

enum TR_ENUM(SV_EnumNoFlags = true)
ECullMode
{
	CMD_NONE = 1,
	CMD_FRONT = 2,
	CMD_BACK = 3
};

enum TR_ENUM(SV_EnumNoFlags = true)
EDepthWriteMask
{
	DSWM_ZERO = 0,
	DSWM_ALL = 1
};

enum TR_ENUM(SV_EnumNoFlags = true)
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

enum TR_ENUM(SV_EnumNoFlags = true)
	EBlend
{
	BLD_ZERO = 1,
	BLD_ONE = 2,
	BLD_SRC_COLOR = 3,
	BLD_INV_SRC_COLOR = 4,
	BLD_SRC_ALPHA = 5,
	BLD_INV_SRC_ALPHA = 6,
	BLD_DEST_ALPHA = 7,
	BLD_INV_DEST_ALPHA = 8,
	BLD_DEST_COLOR = 9,
	BLD_INV_DEST_COLOR = 10,
	BLD_SRC_ALPHA_SAT = 11,
	BLD_BLEND_FACTOR = 14,
	BLD_INV_BLEND_FACTOR = 15,
	BLD_SRC1_COLOR = 16,
	BLD_INV_SRC1_COLOR = 17,
	BLD_SRC1_ALPHA = 18,
	BLD_INV_SRC1_ALPHA = 19
};

enum TR_ENUM(SV_EnumNoFlags = true)
	EBlendOp
{
	BLDOP_ADD = 1,
	BLDOP_SUBTRACT = 2,
	BLDOP_REV_SUBTRACT = 3,
	BLDOP_MIN = 4,
	BLDOP_MAX = 5
};

enum TR_ENUM()
	FrameBufferLoadAction
{
	LoadActionDontCare = 0,
	LoadActionLoad = 1,
	LoadActionClear = 2
};

enum TR_ENUM()
	FrameBufferStoreAction
{
	StoreActionDontCare = 0,
	StoreActionStore = 1,
	StoreActionMultisampleResolve = 2,
	StoreActionStoreAndMultisampleResolve = 3,
	StoreActionUnknown = 4
};

enum TR_ENUM()
	EPrimitiveType
{
	EPT_PointList = 1,
		EPT_LineList = 2,
		EPT_LineStrip = 3,
		EPT_TriangleList = 4,
		EPT_TriangleStrip = 5,
		EPT_TriangleFan = 6,
};

inline UINT HLSLBinding2KhronosBindingLocation(const VNameString& sematic, UINT index)
{
	if (sematic == "POSITION")
	{
		if (index == 0)
			return 0;
	}
	else if (sematic == "NORMAL")
	{
		if (index == 0)
			return 1;
	}
	else if (sematic == "COLOR")
	{
		if (index == 0)
			return 3;
	}
	else if (sematic == "TEXCOORD")
	{
		switch (index)
		{
			case 0:
				return 2;
			case 1:
				return 4;
			case 2:
				return 5;
			case 3:
				return 6;
			case 4:
				return 7;
			case 5:
				return 8;
			case 6:
				return 9;
			case 7:
				return 10;
			case 8:
				return 11;
			case 9:
				return 12;
			case 10:
				return 13;
			case 11:
				return 14;
			case 12:
				return 15;
		default:
			break;
		}
	}
	else if (sematic == "SV_VertexID")
	{
		if (index == 0)
			return 16;
	}
	else if (sematic == "SV_InstanceID")
	{
		if (index == 0)
			return 17;
	}
	return 0xFFFFFFFF;
}

enum TR_ENUM()
	EShaderType
{
	EST_UnknownShader,
	EST_VertexShader,
	EST_PixelShader,
	EST_ComputeShader,
};

enum TR_ENUM()
	EGpuBufferType
{
	GBT_Unknown = 0,
	GBT_VertexBuffer = 1,
	GBT_IndexBuffer = (1 << 1),
	GBT_UavBuffer = (1 << 2),
	GBT_TBufferBuffer = (1 << 3),
	GBT_IndirectBuffer = (1 << 4),
	GBT_CBuffer = (1 << 5),	
};

enum TR_ENUM()
	EGpuBufferViewType
{
	GBVT_Srv = 1,
	GBVT_Rtv = (1 << 1),
	GBVT_Dsv = (1 << 2),
	GBVT_Uav = (1 << 3),
	GBVT_CBuffer = (1 << 4),
	GBVT_VertexBuffer = (1 << 5),
	GBVT_IndexBuffer = (1 << 6),
	GBVT_IndirectBuffer = (1 << 7),
};

enum TR_ENUM()
	EShaderBindType
{
	SBT_CBuffer,
	SBT_Uav,
	SBT_Srv,
	SBT_Sampler,
};

struct TR_CLASS(SV_LayoutStruct = 8)
	IShaderBinder
{
	IShaderBinder()
	{
		SetDefault();
	}
	void SetDefault()
	{
		BindType = EShaderBindType::SBT_CBuffer;
		VSBindPoint = -1;
		PSBindPoint = -1;
		CSBindPoint = -1;
		BindCount = 0;
		DescriptorSet = -1;
		BufferType = EGpuBufferType::GBT_CBuffer;
	}
	EShaderBindType	BindType;
	EGpuBufferType	BufferType;
	UINT			VSBindPoint;
	UINT			PSBindPoint;
	UINT			CSBindPoint;
	UINT			BindCount;
	UINT			DescriptorSet;
	VNameString		Name;
};

NS_END

