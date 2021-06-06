#pragma once
#include "PreHead.h"

NS_BEGIN

struct ISwapChainDesc;
class IRenderContext;
struct IRenderContextDesc;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IRenderSystemDesc
{
	IRenderSystemDesc()
	{
		WindowHandle = nullptr;
		CreateDebugLayer = TRUE;
	}
	vBOOL		CreateDebugLayer;
	void*		WindowHandle;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IRenderSystem : public VIUnknown
{
public:
	IRenderSystem();
	~IRenderSystem();

	TR_FUNCTION()
	static IRenderSystem* CreateRenderSystem(ERHIType type, const IRenderSystemDesc* pDesc);

	virtual bool Init(const IRenderSystemDesc* desc) = 0;

	TR_FUNCTION()
	virtual UINT32 GetContextNumber() = 0;
	TR_FUNCTION()
	virtual vBOOL GetContextDesc(UINT32 index, IRenderContextDesc* desc) = 0;
	TR_FUNCTION()
	virtual IRenderContext* CreateContext(const IRenderContextDesc* desc) = 0;
public:
	static IRenderSystem*	Instance;

	ERHIType				mRHIType;
};

NS_END