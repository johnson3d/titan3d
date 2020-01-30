#pragma once
#include "../IRenderSystem.h"

NS_BEGIN

class MtlSystem : public IRenderSystem
{
public:
	MtlSystem();
	~MtlSystem();

	virtual bool Init(const IRenderSystemDesc* desc) override;

	virtual UINT32 GetContextNumber() override;
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* pDesc) override;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* pDesc) override;
};

NS_END