#pragma once
#include "../ITextureBase.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11Texture2D : public ITexture2D
{
public:
	ID11Texture2D();
	~ID11Texture2D();
public:
	ID3D11Texture2D*		m_pDX11Texture2D;
public:
	bool Init(ID11RenderContext* rc, const ITexture2DDesc* desc);
	bool InitD11Texture2D(ID3D11Texture2D* texture);

	virtual vBOOL Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) override;
	virtual void Unmap(ICommandList* cmd, int MipLevel) override;
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) override;
};

NS_END