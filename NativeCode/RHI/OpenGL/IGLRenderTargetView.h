#pragma once
#include "../IRenderTargetView.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLTexture2D;
class IGLGpuBuffer;

class IGLRenderTargetView : public IRenderTargetView
{
public:
	IGLRenderTargetView();
	~IGLRenderTargetView();

	virtual void Cleanup() override;
public:
	bool Init(IGLRenderContext* rc, const IRenderTargetViewDesc* desc);
	bool Init(IGLRenderContext* rc, IGLGpuBuffer* pBuffer, const IRenderTargetViewDesc* desc);
};

NS_END