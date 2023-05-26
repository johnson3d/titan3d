#include "IRenderDocTool.h"
#include "../../../NativeCode/NextRHI/Dx11/DX11GpuDevice.h"
#include "../../../NativeCode/NextRHI/Dx12/DX12GpuDevice.h"
#include "../../../NativeCode/NextRHI/Vulkan/VKGpuDevice.h"
#include "../../../NativeCode/Base/csharp/CsBinder.h"

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

void IRenderDocTool::InitRenderDoc(const char* path)
{
	// At init, on windows
	auto rdModule = LoadLibraryA(path);
	if (HMODULE mod = GetModuleHandleA(path))
	{
		pRENDERDOC_GetAPI RENDERDOC_GetAPI =
			(pRENDERDOC_GetAPI)GetProcAddress(mod, "RENDERDOC_GetAPI");
		int ret = RENDERDOC_GetAPI(eRENDERDOC_API_Version_1_4_0, (void**)&mApi);
		assert(ret == 1);
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_CaptureAllCmdLists, 1);

		auto opt = FGlobalConfig::GetInstance()->GetConfigValueI32("RenderDocCallStacks");
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_CaptureCallstacks, opt);

		opt = FGlobalConfig::GetInstance()->GetConfigValueI32("RenderDocSaveAllInitials");
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_SaveAllInitials, opt);
	}
}

void IRenderDocTool::SetGpuDevice(NxRHI::IGpuDevice* rc)
{
	mRenderContext = rc;

	switch (rc->Desc.RhiType)
	{
		case NxRHI::ERhiType::RHI_D3D11:
		{
			mHWDevice = ((NxRHI::DX11GpuDevice*)rc)->mDevice;
		}
		break;
		case NxRHI::ERhiType::RHI_D3D12:
		{
			mHWDevice = ((NxRHI::DX12GpuDevice*)rc)->mDevice;
		}
		break;
		case NxRHI::ERhiType::RHI_VK:
		{
			mHWDevice = RENDERDOC_DEVICEPOINTER_FROM_VKINSTANCE(((NxRHI::VKGpuDevice*)rc)->GetVkInstance());
		}
		break;
		default:
			break;
	}
}

void IRenderDocTool::SetActiveWindow(void* wndHandle)
{
	if (mApi == nullptr)
		return;
	mActiveWndHandle = wndHandle;
	mApi->SetActiveWindow(mHWDevice, wndHandle);
}

void IRenderDocTool::TriggerCapture()
{
	if (mApi == nullptr)
		return;
	mApi->TriggerCapture();
}

void IRenderDocTool::StartFrameCapture()
{
	if (mApi == nullptr)
		return;
	mApi->StartFrameCapture(mHWDevice, mActiveWndHandle);
}

bool IRenderDocTool::IsFrameCapturing()
{
	if (mApi == nullptr)
		return false;
	return mApi->IsFrameCapturing() ? true : false;
}

UINT IRenderDocTool::EndFrameCapture()
{
	if (mApi == nullptr)
		return 0;
	return mApi->EndFrameCapture(mHWDevice, mActiveWndHandle);
}

UINT IRenderDocTool::DiscardFrameCapture()
{
	if (mApi == nullptr)
		return 0;
	return mApi->DiscardFrameCapture(mHWDevice, mActiveWndHandle);
}

UINT IRenderDocTool::GetNumCaptures()
{
	if (mApi == nullptr)
		return 0;

	return mApi->GetNumCaptures();
}

const char* IRenderDocTool::GetCapture(UINT idx, UINT64* timestamp)
{
	if (mApi == nullptr)
		return 0;

	uint32_t LogPathLength = 512;
	memset(mTempLogFile, 0, sizeof(mTempLogFile));
	if (mApi->GetCapture(idx, mTempLogFile, &LogPathLength, timestamp))
	{
		return mTempLogFile;
	}
	else
	{
		return "";
	}
}

NS_END
