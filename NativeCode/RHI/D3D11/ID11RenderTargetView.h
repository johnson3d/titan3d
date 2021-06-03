#pragma once
#include "../IRenderTargetView.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11RenderTargetView : public IRenderTargetView
{
public:
	ID11RenderTargetView();
	~ID11RenderTargetView();

public:
	ID3D11RenderTargetView*			m_pDX11RTV;
public:
	bool Init(ID11RenderContext* rc, const IRenderTargetViewDesc* desc);
};

NS_END