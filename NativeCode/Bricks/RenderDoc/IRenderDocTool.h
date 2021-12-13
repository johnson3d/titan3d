#pragma once
#include "renderdoc_app.h"
#include "../../../NativeCode/Base/IUnknown.h"

NS_BEGIN

class IRenderContext;
class TR_CLASS()
	IRenderDocTool : public VIUnknown
{
	RENDERDOC_API_1_4_0* mApi;
	TObjectHandle<IRenderContext>	mRenderContext;
	void*							mHWDevice;
public:
	static IRenderDocTool* GetInstance();
	IRenderDocTool();
	void InitTool(IRenderContext * rc);

	void SetActiveWindow(void* wndHandle);
	void TriggerCapture();
	void StartFrameCapture(void* wndHandle);
	bool IsFrameCapturing();
	UINT EndFrameCapture(void* wndHandle);
	UINT DiscardFrameCapture(void* wndHandle);
};

NS_END