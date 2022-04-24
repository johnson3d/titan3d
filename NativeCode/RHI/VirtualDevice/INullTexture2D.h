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
	virtual void* GetHWBuffer() const override {
		return nullptr;
	}
	virtual vBOOL MapMipmap(ICommandList* cmd, UINT ArraySlice, UINT MipSlice, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) override;
	virtual void UnmapMipmap(ICommandList* cmd, UINT ArraySlice, UINT MipSlice) override;
	virtual void UpdateMipData(ICommandList* cmd, UINT ArraySlice, UINT MipSlice, void* pData, UINT width, UINT height, UINT Pitch) override;
};

NS_END