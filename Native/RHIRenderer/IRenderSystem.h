#pragma once
#include "PreHead.h"

NS_BEGIN

struct ISwapChainDesc;
class IRenderContext;
struct IRenderContextDesc;

struct IRenderSystemDesc
{
	IRenderSystemDesc()
	{
		WindowHandle = nullptr;
		CreateDebugLayer = TRUE;
	}
	vBOOL		CreateDebugLayer;
	void*		WindowHandle;
};

class IRenderSystem : public RHIUnknown
{
public:
	IRenderSystem();
	~IRenderSystem();

	virtual bool Init(const IRenderSystemDesc* desc) = 0;

	virtual UINT32 GetContextNumber() = 0;
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) = 0;
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) = 0;

public:
	static IRenderSystem*	Instance;

	ERHIType				mRHIType;
};

NS_END