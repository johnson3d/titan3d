#pragma once
#include "GLSdk.h"
#include "../ShaderReflector.h"
#include "../IDrawCall.h"
#include "../IBlendState.h"
#include "../ISamplerState.h"
#include "../IShaderResourceView.h"

NS_BEGIN

inline void FormatToGL(EPixelFormat fmt, GLint& internalFormat, GLint& format, GLenum& type)
{
	switch (fmt)
	{
	case PXF_R16_FLOAT:
		internalFormat = GL_R16F;
		format = GL_RED;
		type = GL_HALF_FLOAT;
		return;
	case PXF_R16_UINT:
		internalFormat = GL_R16UI;
		format = GL_RED_INTEGER;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16_SINT:
		internalFormat = GL_R16I;
		format = GL_RED_INTEGER;
		type = GL_SHORT;
		return;
	case PXF_R16_UNORM:
		internalFormat = GL_R16;
		format = GL_RED;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16_SNORM:
		internalFormat = GL_R16_SNORM;
		format = GL_RED;
		type = GL_SHORT;
		return;
	case PXF_R32_UINT:
		internalFormat = GL_R32UI;
		format = GL_RED;
		type = GL_UNSIGNED_INT;
		return;
	case PXF_R32_SINT:
		internalFormat = GL_R32I;
		format = GL_RED;
		type = GL_INT;
		return;
	case PXF_R32_FLOAT:
		internalFormat = GL_R32F;
		format = GL_RED;
		type = GL_FLOAT;
		return;
	case PXF_R8G8B8A8_SINT:
		internalFormat = GL_RGBA;
		format = GL_RGBA;
		type = GL_BYTE;
		return;
	case PXF_R8G8B8A8_UINT:
		internalFormat = GL_RGBA;
		format = GL_RGBA;
		type = GL_UNSIGNED_BYTE;
		return;
	case PXF_R8G8B8A8_UNORM:
		internalFormat = GL_RGBA;
		format = GL_RGBA;
		type = GL_UNSIGNED_BYTE;
		return;
	case PXF_R8G8B8A8_SNORM:
		internalFormat = GL_RGBA;
		format = GL_RGBA;
		type = GL_BYTE;
		return;
	case PXF_R16G16_UINT:
		internalFormat = GL_RG16UI;
		format = GL_RG16;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16G16_SINT:
		internalFormat = GL_RG16I;
		format = GL_RG16;
		type = GL_SHORT;
		return;
	case PXF_R16G16_FLOAT:
		internalFormat = GL_RG16F;
		format = GL_RG;
		type = GL_FLOAT;
		return;
	case PXF_R16G16_UNORM:
		internalFormat = GL_RG16UI;
		format = GL_RG16;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16G16_SNORM:
		internalFormat = GL_RG16I;
		format = GL_RG16;
		type = GL_SHORT;
		return;
	case PXF_R16G16B16A16_UINT:
		internalFormat = GL_RGBA16UI;
		format = GL_RGBA;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16G16B16A16_SINT:
		internalFormat = GL_RGBA16I;
		format = GL_RGBA;
		type = GL_SHORT;
		return;
	case PXF_R16G16B16A16_FLOAT:
		internalFormat = GL_RGBA16F;
		format = GL_RGBA;
		type = GL_HALF_FLOAT;
		return;
	case PXF_R16G16B16A16_UNORM:
		internalFormat = GL_RGBA16UI;
		format = GL_RGBA;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R16G16B16A16_SNORM:
		internalFormat = GL_RGBA16I;
		format = GL_RGBA;
		type = GL_SHORT;
		return;
	case PXF_R32G32B32A32_UINT:
		internalFormat = GL_RGBA32UI;
		format = GL_RGBA;
		type = GL_UNSIGNED_INT;
		return;
	case PXF_R32G32B32A32_SINT:
		internalFormat = GL_RGBA32I;
		format = GL_RGBA;
		type = GL_INT;
		return;
	case PXF_R32G32B32A32_FLOAT:
		internalFormat = GL_RGBA32F;
		format = GL_RGBA;
		type = GL_FLOAT;
		return;
	case PXF_R32G32B32_UINT:
		internalFormat = GL_RGB32UI;
		format = GL_RGB;
		type = GL_UNSIGNED_INT;
		return;
	case PXF_R32G32B32_SINT:
		internalFormat = GL_RGB32I;
		format = GL_RGB;
		type = GL_INT;
		return;
	case PXF_R32G32B32_FLOAT:
		internalFormat = GL_RGBA32F;
		format = GL_RGBA;
		type = GL_FLOAT;
		return;
	case PXF_R32G32_UINT:
		internalFormat = GL_RG32UI;
		format = GL_RG;
		type = GL_UNSIGNED_INT;
		return;
	case PXF_R32G32_SINT:
		internalFormat = GL_RG32I;
		format = GL_RG;
		type = GL_INT;
		return;
	case PXF_R32G32_FLOAT:
		internalFormat = GL_RG32F;
		format = GL_RG;
		type = GL_FLOAT;
		return;
	case PXF_D24_UNORM_S8_UINT:
		internalFormat = GL_RGBA;
		format = GL_RGBA;
		type = GL_UNSIGNED_BYTE;
		return;
	case PXF_D32_FLOAT:
		internalFormat = GL_R32F;
		format = GL_R;
		type = GL_FLOAT;
		return;
	case PXF_D32_FLOAT_S8X24_UINT:
		internalFormat = GL_RG32F;
		format = GL_RG;
		type = GL_FLOAT;
		return;
	case PXF_D16_UNORM:
		internalFormat = GL_DEPTH_COMPONENT16;
		format = GL_DEPTH_COMPONENT;
		type = GL_UNSIGNED_SHORT;
		return;
	case PXF_R11G11B10_FLOAT:
		internalFormat = GL_R11F_G11F_B10F;
		format = GL_RGB;
		type = GL_UNSIGNED_INT_10F_11F_11F_REV;
		return;
	case PXF_R8G8_UNORM:
		internalFormat = GL_RG8;
		format = GL_RG;
		type = GL_UNSIGNED_BYTE;
		return;
	case PXF_R8_UNORM:
		internalFormat = GL_R8;
		format = GL_RED;
		type = GL_UNSIGNED_BYTE;
		return;
	case PXF_R32_TYPELESS:
		internalFormat = GL_R32UI;
		format = GL_RED;
		type = GL_UNSIGNED_INT;
		return;
	default:
		break;
	}
}

inline UINT GLInternalFormatSize(GLint internalFormat)
{
	switch (internalFormat)
	{
		case GL_R16:
		case GL_R16_SNORM:
		case GL_R16F:
		case GL_R16I:
		case GL_R16UI:
			return 2;
		case GL_R32F:
		case GL_R32I:
		case GL_R32UI:
			return 4;
		case GL_RG:
			return 2;
		case GL_RG_INTEGER:
			return 4;
		case GL_RG8:
			return 2;
		case GL_RG16:
		case GL_RG16F:
		case GL_RG16I:
		case GL_RG16UI:
			return 4;
		case GL_RG32F:
		case GL_RG32I:
		case GL_RG32UI:
			return 8;
		case GL_R8:
		case GL_R8I:
		case GL_R8UI:
			return 1;
		case GL_RG8I:
		case GL_RG8UI:
			return 2;
		case GL_RGBA16UI:
		case GL_RGBA16I:
		case GL_RGBA16F:
			return 8;
		case GL_RGBA32UI:
		case GL_RGBA32I:
		case GL_RGBA32F:
			return 16;
		case GL_RGB32UI:
		case GL_RGB32I:
		case GL_RGB32F:
			return 12;
		case GL_RGBA:
			return 4;
		case GL_DEPTH24_STENCIL8:
			return 4;
		case GL_DEPTH_COMPONENT32F:
			return 4;
		case GL_DEPTH_COMPONENT16:
			return 2;
		default:
			ASSERT(false);
			return 0;
	}
}

// isIntegerVertexAttrib : in vertex shaders, the int, ivec2, ivec3, ivec4, uint, uvec2, uvec3, uvec4 types will use
// glVertexAttribIPointer()--one that doesn't automatically convert everything to floating point.
// otherwise, use glVertexAttribPointer().
inline void FormatToGLElement(EPixelFormat fmt, int& size, GLenum& type, GLboolean& normolized, bool &isIntegerVertexAttrib)
{
	isIntegerVertexAttrib = FALSE;
	switch (fmt)
	{
	case PXF_R16_FLOAT:
		size = 1;
		type = GL_HALF_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R16_UINT:
		size = 1;
		type = GL_UNSIGNED_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16_SINT:
		size = 1;
		type = GL_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16_UNORM:
		size = 1;
		type = GL_UNSIGNED_SHORT;
		normolized = TRUE;
		return;
	case PXF_R16_SNORM:
		size = 1;
		type = GL_SHORT;
		normolized = TRUE;
		return;
	case PXF_R32_UINT:
		size = 1;
		type = GL_UNSIGNED_INT;
		normolized = FALSE;
		return;
	case PXF_R32_SINT:
		size = 1;
		type = GL_INT;
		normolized = FALSE;
		return;
	case PXF_R32_FLOAT:
		size = 1;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R8G8B8A8_SINT:
		size = 4;
		type = GL_BYTE;
		normolized = FALSE;
		return;
	case PXF_R8G8B8A8_UINT:
		size = 4;
		type = GL_UNSIGNED_BYTE;
		normolized = FALSE;
		isIntegerVertexAttrib = TRUE;
		return;
	case PXF_R8G8B8A8_SNORM:
		size = 4;
		type = GL_BYTE;
		normolized = TRUE;
		return;
	case PXF_R8G8B8A8_UNORM:
		size = 4;
		type = GL_UNSIGNED_BYTE;
		normolized = TRUE;
		return;
	case PXF_R16G16_UINT:
		size = 2;
		type = GL_UNSIGNED_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16G16_SINT:
		size = 2;
		type = GL_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16G16_FLOAT:
		size = 2;
		type = GL_HALF_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R16G16_UNORM:
		size = 2;
		type = GL_UNSIGNED_SHORT;
		normolized = TRUE;
		return;
	case PXF_R16G16_SNORM:
		size = 2;
		type = GL_SHORT;
		normolized = TRUE;
		return;
	case PXF_R16G16B16A16_UINT:
		size = 4;
		type = GL_UNSIGNED_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16G16B16A16_SINT:
		size = 4;
		type = GL_SHORT;
		normolized = FALSE;
		return;
	case PXF_R16G16B16A16_FLOAT:
		size = 4;
		type = GL_HALF_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R16G16B16A16_UNORM:
		size = 4;
		type = GL_UNSIGNED_SHORT;
		normolized = TRUE;
		return;
	case PXF_R16G16B16A16_SNORM:
		size = 4;
		type = GL_SHORT;
		normolized = TRUE;
		return;
	case PXF_R32G32B32A32_UINT:
		size = 4;
		type = GL_UNSIGNED_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32B32A32_SINT:
		size = 4;
		type = GL_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32B32A32_FLOAT:
		size = 4;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R32G32B32_UINT:
		size = 3;
		type = GL_UNSIGNED_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32B32_SINT:
		size = 3;
		type = GL_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32B32_FLOAT:
		size = 3;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_R32G32_UINT:
		size = 2;
		type = GL_UNSIGNED_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32_SINT:
		size = 2;
		type = GL_INT;
		normolized = FALSE;
		return;
	case PXF_R32G32_FLOAT:
		size = 2;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_D24_UNORM_S8_UINT:
		size = 2;
		type = GL_UNSIGNED_INT;
		normolized = FALSE;
		return;
	case PXF_D32_FLOAT:
		size = 1;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_D32_FLOAT_S8X24_UINT:
		size = 2;
		type = GL_FLOAT;
		normolized = FALSE;
		return;
	case PXF_D16_UNORM:
		size = 1;
		type = GL_HALF_FLOAT;
		normolized = TRUE;
		return;
	default:
		break;
	}
}

inline GLenum AddressModeToGL(EAddressMode mode)
{
	switch (mode)
	{
	case ADM_WRAP:
		return GL_REPEAT;
	case ADM_MIRROR:
		return GL_MIRRORED_REPEAT;
	case ADM_CLAMP:
		return GL_CLAMP_TO_EDGE;
	case ADM_BORDER:
		return GL_CLAMP_TO_BORDER;
	case ADM_MIRROR_ONCE:
		return GL_MIRROR_CLAMP_TO_EDGE;
	default:
		return GL_REPEAT;
	}
}

inline void FilterToGL(ESamplerFilter filter, GLenum &minFilter, GLenum &magFilter)
{
	switch (filter)
	{
	case SPF_MIN_MAG_MIP_POINT:
		minFilter = GL_NEAREST_MIPMAP_NEAREST;
		magFilter = GL_NEAREST;
		break;
	case SPF_MIN_MAG_POINT_MIP_LINEAR:
		minFilter = GL_NEAREST_MIPMAP_LINEAR;
		magFilter = GL_NEAREST;
		break;
	case SPF_MIN_POINT_MAG_LINEAR_MIP_POINT:
		minFilter = GL_NEAREST_MIPMAP_NEAREST;
		magFilter = GL_LINEAR;
		break;
	case SPF_MIN_POINT_MAG_MIP_LINEAR:
		minFilter = GL_NEAREST_MIPMAP_LINEAR;
		magFilter = GL_LINEAR;
		break;
	case SPF_MIN_LINEAR_MAG_MIP_POINT:
		minFilter = GL_LINEAR_MIPMAP_NEAREST;
		magFilter = GL_NEAREST;
		break;
	case SPF_MIN_LINEAR_MAG_POINT_MIP_LINEAR:
		minFilter = GL_LINEAR_MIPMAP_LINEAR;
		magFilter = GL_NEAREST;
		break;
	case SPF_MIN_MAG_LINEAR_MIP_POINT:
		minFilter = GL_LINEAR_MIPMAP_NEAREST;
		magFilter = GL_LINEAR;
		break;
	case SPF_MIN_MAG_MIP_LINEAR:
		minFilter = GL_LINEAR_MIPMAP_LINEAR;
		magFilter = GL_LINEAR;
		break;
	case SPF_ANISOTROPIC:
	default:
		minFilter = GL_LINEAR_MIPMAP_LINEAR;
		magFilter = GL_LINEAR;
		break;
	}
}

inline EShaderVarType GLTypeToShaderVarType(GLuint type)
{
	switch (type)
	{
	case GL_FLOAT:
		return SVT_Float1;
	case GL_FLOAT_VEC2:
		return SVT_Float2;
	case GL_FLOAT_VEC3:
		return SVT_Float3;
	case GL_FLOAT_VEC4:
		return SVT_Float4;
	case GL_INT:
		return SVT_Int1;
	case GL_INT_VEC2:
		return SVT_Int2;
	case GL_INT_VEC3:
		return SVT_Int3;
	case GL_INT_VEC4:
		return SVT_Int4;
	case GL_FLOAT_MAT4:
		return SVT_Matrix4x4;
	case GL_FLOAT_MAT3:
		return SVT_Matrix3x3;
	}
	return SVT_Unknown;
}

inline UINT GLTypeToShaderVarSize(GLuint type)
{
	switch (type)
	{
	case GL_FLOAT:
		return 4;
	case GL_FLOAT_VEC2:
		return 8;
	case GL_FLOAT_VEC3:
		return 12;
	case GL_FLOAT_VEC4:
		return 16;
	case GL_INT:
		return 4;
	case GL_INT_VEC2:
		return 8;
	case GL_INT_VEC3:
		return 12;
	case GL_INT_VEC4:
		return 16;
	case GL_FLOAT_MAT4:
		return 64;
	case GL_FLOAT_MAT3:
		return 36;
	}
	return SVT_Unknown;
}

inline GLenum PrimitiveTypeToGL(EPrimitiveType type, UINT NumPrimitives, UINT* pCount)
{
	switch (type)
	{
	case EPT_PointList:
		break;
	case EPT_LineList:
		*pCount = NumPrimitives * 2;
		return GL_LINES;
	case EPT_LineStrip:
		*pCount = NumPrimitives + 1;
		return GL_LINE_LOOP;
	case EPT_TriangleList:
		*pCount = NumPrimitives * 3;
		return GL_TRIANGLES;
	case EPT_TriangleStrip:
		*pCount = NumPrimitives + 2;
		return GL_TRIANGLE_STRIP;
	case EPT_TriangleFan:
		*pCount = NumPrimitives + 2;
		return GL_TRIANGLE_STRIP;
	default:
		ASSERT(false);
	}
	return GL_TRIANGLES;
}

inline GLenum ComparisionModeToGL(EComparisionMode mode)
{
	switch (mode)
	{
	case CMP_NEVER:
		return GL_NEVER;
	case CMP_LESS:
		return GL_LESS;
	case CMP_EQUAL:
		return GL_EQUAL;
	case CMP_LESS_EQUAL:
		return GL_LEQUAL;
	case CMP_GREATER:
		return GL_GREATER;
	case CMP_NOT_EQUAL:
		return GL_NOTEQUAL;
	case CMP_GREATER_EQUAL:
		return GL_GEQUAL;
	case CMP_ALWAYS:
		return GL_ALWAYS;
	default:
		return GL_ALWAYS;
	}
}

inline GLenum BlendToGL(EBlend blend)
{
	switch (blend)
	{
	case BLD_ZERO:
		return GL_ZERO;
	case BLD_ONE:
		return GL_ONE;
	case BLD_SRC_COLOR:
		return GL_SRC_COLOR;
	case BLD_INV_SRC_COLOR:
		return GL_ONE_MINUS_SRC_COLOR;
	case BLD_SRC_ALPHA:
		return GL_SRC_ALPHA;
	case BLD_INV_SRC_ALPHA:
		return GL_ONE_MINUS_SRC_ALPHA;
	case BLD_DEST_ALPHA:
		return GL_DST_ALPHA;
	case BLD_INV_DEST_ALPHA:
		return GL_ONE_MINUS_DST_ALPHA;
	case BLD_DEST_COLOR:
		return GL_DST_COLOR;
	case BLD_INV_DEST_COLOR:
		return GL_ONE_MINUS_DST_COLOR;
	case BLD_SRC_ALPHA_SAT:
		return GL_SRC_ALPHA_SATURATE;
	case BLD_BLEND_FACTOR:
		return GL_ZERO;//????
	case BLD_INV_BLEND_FACTOR:
		return GL_ONE;//????
	case BLD_SRC1_COLOR:
		return GL_SRC1_COLOR;
	case BLD_INV_SRC1_COLOR:
		return GL_ONE_MINUS_SRC1_COLOR;
	case BLD_SRC1_ALPHA:
		return GL_SRC1_ALPHA;
	case BLD_INV_SRC1_ALPHA:
		return GL_ONE_MINUS_SRC1_ALPHA;
	default:
		return GL_ONE;
	}
}

inline GLenum BlendOPToGL(EBlendOp blendOp)
{
	switch (blendOp)
	{
	case BLDOP_ADD:
		return GL_FUNC_ADD;
	case BLDOP_SUBTRACT:
		return GL_FUNC_SUBTRACT;
	case BLDOP_REV_SUBTRACT:
		return GL_FUNC_REVERSE_SUBTRACT;
	case BLDOP_MIN:
		return GL_MIN;
	case BLDOP_MAX:
		return GL_MAX;
	default:
		return GL_FUNC_ADD;
	}
}



NS_END