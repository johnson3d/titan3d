#pragma once
#include "../IShaderResourceView.h"
#include "GLPreHead.h"
#include "IGLTexture2D.h"

struct XImageBuffer;

NS_BEGIN

inline int FormatToGL_ETC(ETCFormat fmt, GLint& internalFormat, GLint& format, GLenum& type)
{
	/*
		GL_COMPRESSED_R11_EAC	ceil(width / 4) * ceil(height / 4) * 8
		GL_COMPRESSED_SIGNED_R11_EAC	ceil(width / 4) * ceil(height / 4) * 8
		GL_COMPRESSED_RG11_EAC	ceil(width / 4) * ceil(height / 4) * 16
		GL_COMPRESSED_SIGNED_RG11_EAC	ceil(width / 4) * ceil(height / 4) * 16
		GL_COMPRESSED_RGB8_ETC2	ceil(width / 4) * ceil(height / 4) * 8
		GL_COMPRESSED_SRGB8_ETC2	ceil(width / 4) * ceil(height / 4) * 8
		GL_COMPRESSED_RGBA8_ETC2_EAC	ceil(width / 4) * ceil(height / 4) * 16
		GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC	ceil(width / 4) * ceil(height / 4) * 16
		GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2	ceil(width / 4) * ceil(height / 4) * 8
		GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2	ceil(width / 4) * ceil(height / 4) * 8
	*/
	switch (fmt)
	{
	case ETCFormat::ETC1:
	{
		format = GL_RGB;
		internalFormat = 0x00008d64;// GL_ETC1_RGB8_OES;
		return 8;
	}
	break;
	case ETCFormat::RGB8:
	{
		format = GL_RGB;
		internalFormat = GL_COMPRESSED_RGB8_ETC2;
		return 8;
	}
	break;
	case ETCFormat::SRGB8:
	{
		format = GL_RGB;
		internalFormat = GL_COMPRESSED_SRGB8_ETC2;
		return 8;
	}
	break;
	case ETCFormat::RGBA8:
	{
		format = GL_RGBA;
		internalFormat = GL_COMPRESSED_RGBA8_ETC2_EAC;
		return 16;
	}
	break;
	case ETCFormat::SRGBA8:
	{
		format = GL_RGBA;
		internalFormat = GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC;
		return 16;
	}
	break;
	case ETCFormat::R11:
	{
		format = GL_R;
		internalFormat = GL_COMPRESSED_R11_EAC;
		return 8;
	}
	case ETCFormat::SIGNED_R11:
	{
		format = GL_R;
		internalFormat = GL_COMPRESSED_SIGNED_R11_EAC;
		return 8;
	}
	case ETCFormat::RG11:
	{
		format = GL_RG;
		internalFormat = GL_COMPRESSED_RG11_EAC;
		return 16;
	}
	case ETCFormat::SIGNED_RG11:
	{
		format = GL_RG;
		internalFormat = GL_COMPRESSED_SIGNED_RG11_EAC;
		return 16;
	}
	case ETCFormat::RGB8A1:
	{
		format = GL_RGBA;
		internalFormat = GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2;
		return 8;
	}
	case ETCFormat::SRGB8A1:
	{
		format = GL_RGBA;
		internalFormat = GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2;
		return 8;
	}
	default:
		return 0;
	}
}

class IGLGpuBuffer;
class IGLRenderContext;
class IGLShaderResourceView : public IShaderResourceView
{
public:
	IGLShaderResourceView();
	~IGLShaderResourceView();

	virtual void Cleanup() override;

public:
	virtual bool UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer) override;
	bool Init(IGLRenderContext* rc, const IShaderResourceViewDesc* desc);
	
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;

	virtual vBOOL Save2Memory(IRenderContext* rc, IBlobObject* data, int Type) override;

	virtual vBOOL GetTexture2DData(IRenderContext* rc, IBlobObject* data, int level, int RectWidth, int RectHeight) override;

	std::shared_ptr<GLSdk::GLBufferId> GetGLBufferId();

	virtual void* GetAPIObject() override {
		return this;
	}
public:
	TObjectHandle<IGLRenderContext>	mRenderContext;
	std::string			mResourceFile;
	ITexture2DDesc		mTx2dDesc;
	XImageBuffer*		mImage;
};

NS_END