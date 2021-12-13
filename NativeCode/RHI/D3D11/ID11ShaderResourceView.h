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
	AutoRef<ID3D11ShaderResourceView>	m_pDX11SRV;
	TObjectHandle<ID11RenderContext>	mRenderContext;

	virtual bool UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer) override;
	virtual vBOOL Save2Memory(IRenderContext* rc, IBlobObject* data, int Type) override;
	virtual vBOOL GetTexture2DData(IRenderContext* rc, IBlobObject* data, int level, int RectWidth, int RectHeight) override;
	virtual void RefreshResource() override;
	virtual void* GetAPIObject() override {
		return m_pDX11SRV;
	}
public:
	bool Init(ID11RenderContext* rc, const IShaderResourceViewDesc* desc);
	
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
};

NS_END