#pragma once
#include "../ITextureBase.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLTexture2D : public ITexture2D
{
public:
	IGLTexture2D();
	~IGLTexture2D();
	virtual void Cleanup() override;
public:
	std::shared_ptr<GLSdk::GLBufferId>	mGlesTexture2D;
	bool				mIsReadable;
public:
	bool Init(IGLRenderContext* rc, const ITexture2DDesc* desc);
	static GLuint LoadDDS(IGLRenderContext* rc, const char * imagepath);

	virtual vBOOL Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) override;
	virtual void Unmap(ICommandList* cmd, int MipLevel) override;
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) override;
};

NS_END