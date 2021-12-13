#include "IRenderDocTool.h"
#include "../../../NativeCode/RHI/D3D11/ID11RenderContext.h"
#include "../../../NativeCode/RHI/Vulkan/IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IRenderDocTool* IRenderDocTool::GetInstance()
{
	static IRenderDocTool obj;
	return &obj;
}

IRenderDocTool::IRenderDocTool()
{
	mApi = nullptr;
	mHWDevice = nullptr;
}

void IRenderDocTool::InitTool(IRenderContext* rc)
{
	if (mApi != nullptr)
		return;
	mRenderContext.FromObject(rc);

	switch (rc->GetRHIType())
	{
		case RHT_D3D11:
		{
			mHWDevice = ((ID11RenderContext*)rc)->mDevice;
		}
		break;
		case RHT_VULKAN:
		{
			mHWDevice = ((IVKRenderContext*)rc)->mPhysicalDevice;
		}
		break;
		default:
			break;
	}
	
	// At init, on windows
	auto rdModule = LoadLibraryA("renderdoc.dll");
	if (HMODULE mod = GetModuleHandleA("renderdoc.dll"))
	{
		pRENDERDOC_GetAPI RENDERDOC_GetAPI =
			(pRENDERDOC_GetAPI)GetProcAddress(mod, "RENDERDOC_GetAPI");
		int ret = RENDERDOC_GetAPI(eRENDERDOC_API_Version_1_4_0, (void**)&mApi);
		assert(ret == 1);
	}
}

void IRenderDocTool::SetActiveWindow(void* wndHandle)
{
	if (mApi == nullptr)
		return;
	mApi->SetActiveWindow(mHWDevice, wndHandle);
}

void IRenderDocTool::TriggerCapture()
{
	if (mApi == nullptr)
		return;
	mApi->TriggerCapture();
}

void IRenderDocTool::StartFrameCapture(void* wndHandle)
{
	if (mApi == nullptr)
		return;
	mApi->StartFrameCapture(mHWDevice, wndHandle);
}

bool IRenderDocTool::IsFrameCapturing()
{
	if (mApi == nullptr)
		return false;
	return mApi->IsFrameCapturing() ? true : false;
}

UINT IRenderDocTool::EndFrameCapture(void* wndHandle)
{
	if (mApi == nullptr)
		return 0;
	return mApi->EndFrameCapture(mHWDevice, wndHandle);
}

UINT IRenderDocTool::DiscardFrameCapture(void* wndHandle)
{
	if (mApi == nullptr)
		return 0;
	return mApi->DiscardFrameCapture(mHWDevice, wndHandle);
}

NS_END
