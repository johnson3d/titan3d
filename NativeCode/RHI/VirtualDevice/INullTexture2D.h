#pragma once
#include "../ITextureBase.h"
#include "NullPreHead.h"

NS_BEGIN

class INullTexture2D : public ITexture2D
{
public:
	INullTexture2D();
	~INullTexture2D();
public:
	virtual vBOOL Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) override;
	virtual void Unmap(ICommandList* cmd, int MipLevel) override;
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) override;
};

NS_END