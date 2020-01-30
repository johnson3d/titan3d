#pragma once
#include "../IRenderSystem.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderSystem : public IRenderSystem
{
public:
	ID11RenderSystem();
	~ID11RenderSystem();

	virtual bool Init(const IRenderSystemDesc* desc) override;

	virtual UINT32 GetContextNumber() override;
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) override;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) override;
public:
	IDXGIFactory*					m_pDXGIFactory;
	UINT32							mDeviceNumber;
public:
	
	void SafeCreateDXGIFactory(IDXGIFactory** ppDXGIFactory);
};

NS_END