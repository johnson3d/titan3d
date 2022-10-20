#include "NvAftermath.h"

#define VULKAN_HPP_NO_TO_STRING
#include "NsightDumpVK/NsightAftermathGpuCrashTracker.h"

#pragma comment(lib, "GFSDK_Aftermath_Lib.x64.lib")

#define new VNEW

NS_BEGIN

namespace GpuDump
{
	GpuCrashTracker::MarkerMap markerMapVK;
	GpuCrashTracker gNvGpuCrashTrackerVK(markerMapVK);
	/*void GFSDK_AFTERMATH_CALL Aftermath_GpuCrashDumpCb(const void* pGpuCrashDump, const uint32_t gpuCrashDumpSize, void* pUserData)
	{

	}
	void GFSDK_AFTERMATH_CALL Aftermath_ShaderDebugInfoCb(const void* pShaderDebugInfo, const uint32_t shaderDebugInfoSize, void* pUserData)
	{

	}
	void GFSDK_AFTERMATH_CALL Aftermath_GpuCrashDumpDescriptionCb(PFN_GFSDK_Aftermath_AddGpuCrashDumpDescription addValue, void* pUserData)
	{

	}
	void GFSDK_AFTERMATH_CALL Aftermath_ResolveMarkerCb(const void* pMarker, void* pUserData, void** resolvedMarkerData, uint32_t* markerSize)
	{

	}*/
	void NvAftermath::InitDump(NxRHI::ERhiType type)
	{
		GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags apiType{};
		switch (type)
		{
		case EngineNS::NxRHI::RHI_D3D11:
			apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_DX;
			break;
		case EngineNS::NxRHI::RHI_D3D12:
			apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_DX;
			break;
		case EngineNS::NxRHI::RHI_VK:
			apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_Vulkan;
			gNvGpuCrashTrackerVK.Initialize(apiType);
			break;
		case EngineNS::NxRHI::RHI_GL:
			break;
		case EngineNS::NxRHI::RHI_Metal:
			break;
		case EngineNS::NxRHI::RHI_VirtualDevice:
			break;
		default:
			break;
		}
		/*GFSDK_Aftermath_EnableGpuCrashDumps(GFSDK_Aftermath_Version_API, 
			apiType,
			GFSDK_Aftermath_GpuCrashDumpFeatureFlags::GFSDK_Aftermath_GpuCrashDumpFeatureFlags_DeferDebugInfoCallbacks,
			&Aftermath_GpuCrashDumpCb, 
			&Aftermath_ShaderDebugInfoCb,
			&Aftermath_GpuCrashDumpDescriptionCb,
			&Aftermath_ResolveMarkerCb,
			nullptr);*/
	}
	
	void NvAftermath::RegSpirv(void* pCode, UINT size)
	{
		if (gNvGpuCrashTrackerVK.IsInitialized() == false)
			return;
		std::vector<uint8_t> data, debugData;
		data.resize(size);
		memcpy(&data[0], pCode, size);
		gNvGpuCrashTrackerVK.GetShaderDatabase()->AddShaderBinary(data);
		/*data.resize(size);
		memcpy(&data[0], pCode, size);*/
		debugData.resize(size);
		memcpy(&debugData[0], pCode, size);
		gNvGpuCrashTrackerVK.GetShaderDatabase()->AddShaderBinaryWithDebugInfo(debugData, data);
	}
};

NS_END