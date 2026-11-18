#include "IRenderDocTool.h"
#include "../../../NativeCode/NextRHI/Dx11/DX11GpuDevice.h"
#include "../../../NativeCode/NextRHI/Dx12/DX12GpuDevice.h"
#include "../../../NativeCode/NextRHI/Vulkan/VKGpuDevice.h"
#include "../../../NativeCode/Base/csharp/CsBinder.h"
#include "../../../3rd/native/renderdoc/renderdoc_app.h"

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

static bool PrepareRenderDocLayerPath(const std::string& directory)
{
	const auto manifest = directory + "/renderdoc.json";
	const auto attributes = GetFileAttributesA(manifest.c_str());
	if (attributes == INVALID_FILE_ATTRIBUTES || (attributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
	{
		VFX_LTRACE(ELTT_Warning, "RenderDoc: missing Vulkan layer manifest %s; D3D capture remains available, Vulkan capture requires a matching layer\r\n", manifest.c_str());
		return false;
	}

	auto readEnvironment = [](const char* name)
	{
		const auto size = GetEnvironmentVariableA(name, nullptr, 0);
		std::string value(size, '\0');
		if (size != 0)
		{
			const auto length = GetEnvironmentVariableA(name, &value[0], size);
			if (length < size)
				value.resize(length);
			else
				value.clear();
		}
		return value;
	};

	// 显式覆盖存在时 ADD 路径会被 loader 忽略; 保留原路径并在最前面加入同 DLL 的 manifest 目录。
	const auto overridePath = readEnvironment("VK_IMPLICIT_LAYER_PATH");
	const char* variable = overridePath.empty() ? "VK_ADD_IMPLICIT_LAYER_PATH" : "VK_IMPLICIT_LAYER_PATH";
	const auto previous = overridePath.empty() ? readEnvironment(variable) : overridePath;
	const auto searchPath = directory + (previous.empty() ? "" : ";" + previous);
	if (!SetEnvironmentVariableA(variable, searchPath.c_str()) ||
		!SetEnvironmentVariableA("ENABLE_VULKAN_RENDERDOC_CAPTURE", "1"))
	{
		VFX_LTRACE(ELTT_Error, "RenderDoc: could not configure process-local Vulkan layer path, error %lu\r\n", GetLastError());
		return false;
	}
	VFX_LTRACE(ELTT_info, "RenderDoc: %s=%s\r\n", variable, searchPath.c_str());
	return true;
}

void IRenderDocTool::InitRenderDoc(const char* path)
{
	// At init, on windows
	if (mApi != nullptr)
		return;

	// 外部工具注入时复用已有模块; 普通启动使用调用方选定的 DLL, 两者都让 layer 与 API 指向同一份文件。
	HMODULE mod = GetModuleHandleA("renderdoc.dll");
	char modulePath[32768]{};
	const auto length = mod != nullptr ? GetModuleFileNameA(mod, modulePath, sizeof(modulePath)) :
		(path != nullptr ? GetFullPathNameA(path, sizeof(modulePath), modulePath, nullptr) : 0);
	if (length == 0 || length >= sizeof(modulePath))
	{
		VFX_LTRACE(ELTT_Error, "RenderDoc: invalid DLL path\r\n");
		return;
	}
	if (mod == nullptr)
		mod = LoadLibraryA(modulePath);
	if (mod == nullptr)
	{
		VFX_LTRACE(ELTT_Error, "RenderDoc: failed to load %s, error %lu\r\n", modulePath, GetLastError());
		return;
	}
	{
		pRENDERDOC_GetAPI RENDERDOC_GetAPI =
			(pRENDERDOC_GetAPI)GetProcAddress(mod, "RENDERDOC_GetAPI");
		RENDERDOC_API_1_6_0* api = nullptr;
		if (RENDERDOC_GetAPI == nullptr || RENDERDOC_GetAPI(eRENDERDOC_API_Version_1_6_0, (void**)&api) != 1 || api == nullptr)
		{
			VFX_LTRACE(ELTT_Error, "RenderDoc: could not obtain API 1.6.0 from %s\r\n", modulePath);
			return;
		}
		mApi = api;
		VFX_LTRACE(ELTT_info, "RenderDoc: API module %s\r\n", modulePath);
		const std::string fullPath(modulePath);
		const auto separator = fullPath.find_last_of("/\\");
		// Vulkan instance 尚未创建; manifest 不存在只影响 Vulkan, 不阻断已注入的 D3D API。
		if (separator != std::string::npos)
			PrepareRenderDocLayerPath(fullPath.substr(0, separator));
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_CaptureAllCmdLists, 1);

		auto opt = FGlobalConfig::GetInstance()->GetConfigValueI32("RenderDocCallStacks");
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_CaptureCallstacks, opt);

		opt = FGlobalConfig::GetInstance()->GetConfigValueI32("RenderDocSaveAllInitials");
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_SaveAllInitials, opt);
		
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_AllowUnsupportedVendorExtensions, 1);
		mApi->SetCaptureOptionU32(RENDERDOC_CaptureOption::eRENDERDOC_Option_AllowVSync, 0); // 禁用垂直同步


		mApi->MaskOverlayBits(RENDERDOC_OverlayBits::eRENDERDOC_Overlay_None, RENDERDOC_OverlayBits::eRENDERDOC_Overlay_None);
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
	{//GetCapture return utf8
		return mTempLogFile;
	}
	else
	{
		return "";
	}
}

UINT IRenderDocTool::OpenFile(const char* capturedFile)
{
	if (mApi == nullptr)
		return 0;
	const auto result = mApi->LaunchReplayUI(1, capturedFile);
	if (result == 0)
		VFX_LTRACE(ELTT_Warning, "RenderDoc: capture saved, but replay UI could not be opened; open %s in RenderDoc manually\r\n", capturedFile);
	return result;
}

NS_END
