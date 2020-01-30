#pragma once
#include "../IShaderResourceView.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11Texture2D;
class ID11GpuBuffer;
class ID11ShaderResourceView : public IShaderResourceView
{
public:
	ID11ShaderResourceView();
	~ID11ShaderResourceView();
public:
	D3D11_SHADER_RESOURCE_VIEW_DESC		mDX11SRVDesc;
	AutoRef<ID3D11ShaderResourceView>	m_pDX11SRV;
	std::string							mResourceFile;
	TObjectHandle<ID11RenderContext>	mRenderContext;

	virtual vBOOL Save2Memory(IRenderContext* rc, IBlobObject* data, int Type) override;
	virtual vBOOL GetTexture2DData(IRenderContext* rc, IBlobObject* data, int level, int RectWidth, int RectHeight) override;
	virtual void RefreshResource() override;
public:
	bool Init(ID11RenderContext* rc, const IShaderResourceViewDesc* desc);
	bool Init(ID11RenderContext* rc, ID11GpuBuffer* pBuffer, const ISRVDesc* desc);
	//bool SetDepthStencilTexture(ID11Texture2D* );
	bool InitD11View(ID11RenderContext* rc, const char* file, bool isLoad);

	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
};

NS_END