#pragma once
#include "renderdoc_app.h"
#include "../../../NativeCode/Base/IUnknown.h"

NS_BEGIN

namespace NxRHI
{
	class IGpuDevice;
}
class IRenderContext;
class TR_CLASS()
	IRenderDocTool : public IWeakReference
{
	RENDERDOC_API_1_4_0* mApi;
	NxRHI::IGpuDevice*					mRenderContext = nullptr;
	void*								mHWDevice = nullptr;
	char								mTempLogFile[512];
	void*								mActiveWndHandle = nullptr;
public:
	static IRenderDocTool* GetInstance();
	IRenderDocTool();
	void InitRenderDoc(const char* path);
	void SetGpuDevice(NxRHI::IGpuDevice* rc);

	void SetActiveWindow(void* wndHandle);
	void TriggerCapture();
	void StartFrameCapture();
	bool IsFrameCapturing();
	UINT EndFrameCapture();
	UINT DiscardFrameCapture();

	UINT GetNumCaptures();
	const char* GetCapture(UINT idx, UINT64* timestamp);
	UINT OpenFile(const char* capturedFile);
};

NS_END