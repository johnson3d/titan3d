#pragma once
#include "../IRenderSystem.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderSystem : public IRenderSystem
{
public:
	INullRenderSystem();
	~INullRenderSystem();

	virtual bool Init(const IRenderSystemDesc* desc) override;

	virtual UINT32 GetContextNumber() override;
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) override;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) override;


public:
};

NS_END