#include "NvAftermath.h"
#include "../../NextRHI/Dx11/DX11GpuDevice.h"
#include "../../NextRHI/Dx12/DX12GpuDevice.h"

#define VULKAN_HPP_NO_TO_STRING
#include "NsightDumpVK/NsightAftermathGpuCrashTracker.h"

#pragma comment(lib, "GFSDK_Aftermath_Lib.x64.lib")

#define new VNEW

NS_BEGIN

namespace GpuDump
{
	VKGpuCrashTracker::MarkerMap markerMap;
	VKGpuCrashTracker gNvGpuCrashTracker(markerMap);
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

	static NxRHI::ERhiType NvAftermath_RHIType = NxRHI::ERhiType::RHI_VirtualDevice;
	NxRHI::ERhiType NvAftermath::GetAfterMathRhiType()
	{
		return NvAftermath_RHIType;
	}
	
	void NvAftermath::InitDump(NxRHI::ERhiType type)
	{
		NvAftermath_RHIType = type;
		VKShaderDatabase::IsSpirV = false;
		GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags apiType{};
		switch (type)
		{
		case EngineNS::NxRHI::RHI_D3D11:
			{
				apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_DX;
				gNvGpuCrashTracker.Initialize(apiType);
			}
			break;
		case EngineNS::NxRHI::RHI_D3D12:
			{
				apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_DX;
				gNvGpuCrashTracker.Initialize(apiType);
			}
			break;
		case EngineNS::NxRHI::RHI_VK:
			VKShaderDatabase::IsSpirV = true;
			apiType = GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags::GFSDK_Aftermath_GpuCrashDumpWatchedApiFlags_Vulkan;
			gNvGpuCrashTracker.Initialize(apiType);
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
	
	void NvAftermath::DeviceCreated(NxRHI::ERhiType type, NxRHI::IGpuDevice* device)
	{
		switch (type)
		{
		case EngineNS::NxRHI::RHI_D3D11:
			{
				const uint32_t aftermathFlags =
					GFSDK_Aftermath_FeatureFlags_EnableMarkers |             // Enable event marker tracking. Only effective in combination with the Nsight Aftermath Crash Dump Monitor.
					GFSDK_Aftermath_FeatureFlags_EnableResourceTracking |    // Enable tracking of resources.
					GFSDK_Aftermath_FeatureFlags_CallStackCapturing |        // Capture call stacks for all draw calls, compute dispatches, and resource copies.
					GFSDK_Aftermath_FeatureFlags_GenerateShaderDebugInfo;    // Generate debug information for shaders.

				auto dx11Device = ((NxRHI::DX11GpuDevice*)device)->mDevice;
				GFSDK_Aftermath_Result _result = GFSDK_Aftermath_DX11_Initialize(
					GFSDK_Aftermath_Version_API,
					aftermathFlags,
					dx11Device);
				ASSERT(_result == GFSDK_Aftermath_Result_Success);
			}
			break;
		case EngineNS::NxRHI::RHI_D3D12:
			{
				const uint32_t aftermathFlags =
					GFSDK_Aftermath_FeatureFlags_EnableMarkers |             // Enable event marker tracking. Only effective in combination with the Nsight Aftermath Crash Dump Monitor.
					GFSDK_Aftermath_FeatureFlags_EnableResourceTracking |    // Enable tracking of resources.
					GFSDK_Aftermath_FeatureFlags_CallStackCapturing |        // Capture call stacks for all draw calls, compute dispatches, and resource copies.
					GFSDK_Aftermath_FeatureFlags_GenerateShaderDebugInfo;    // Generate debug information for shaders.

				auto dx12Device = ((NxRHI::DX12GpuDevice*)device)->mDevice;
				GFSDK_Aftermath_Result _result = GFSDK_Aftermath_DX12_Initialize(
					GFSDK_Aftermath_Version_API,
					aftermathFlags,
					dx12Device.GetPtr());
				ASSERT(_result == GFSDK_Aftermath_Result_Success);
				/*switch (_result)
				{
				case GFSDK_Aftermath_Result_Success:
					break;
				case GFSDK_Aftermath_Result_NotAvailable:
					break;
				case GFSDK_Aftermath_Result_Fail:
					break;
				case GFSDK_Aftermath_Result_FAIL_VersionMismatch:
					break;
				case GFSDK_Aftermath_Result_FAIL_NotInitialized:
					break;
				case GFSDK_Aftermath_Result_FAIL_InvalidAdapter:
					break;
				case GFSDK_Aftermath_Result_FAIL_InvalidParameter:
					break;
				case GFSDK_Aftermath_Result_FAIL_Unknown:
					break;
				case GFSDK_Aftermath_Result_FAIL_ApiError:
					break;
				case GFSDK_Aftermath_Result_FAIL_NvApiIncompatible:
					break;
				case GFSDK_Aftermath_Result_FAIL_GettingContextDataWithNewCommandList:
					break;
				case GFSDK_Aftermath_Result_FAIL_AlreadyInitialized:
					break;
				case GFSDK_Aftermath_Result_FAIL_D3DDebugLayerNotCompatible:
					break;
				case GFSDK_Aftermath_Result_FAIL_DriverInitFailed:
					break;
				case GFSDK_Aftermath_Result_FAIL_DriverVersionNotSupported:
					break;
				case GFSDK_Aftermath_Result_FAIL_OutOfMemory:
					break;
				case GFSDK_Aftermath_Result_FAIL_GetDataOnBundle:
					break;
				case GFSDK_Aftermath_Result_FAIL_GetDataOnDeferredContext:
					break;
				case GFSDK_Aftermath_Result_FAIL_FeatureNotEnabled:
					break;
				case GFSDK_Aftermath_Result_FAIL_NoResourcesRegistered:
					break;
				case GFSDK_Aftermath_Result_FAIL_ThisResourceNeverRegistered:
					break;
				case GFSDK_Aftermath_Result_FAIL_NotSupportedInUWP:
					break;
				case GFSDK_Aftermath_Result_FAIL_D3dDllNotSupported:
					break;
				case GFSDK_Aftermath_Result_FAIL_D3dDllInterceptionNotSupported:
					break;
				case GFSDK_Aftermath_Result_FAIL_Disabled:
					break;
				default:
					break;
				}*/
			}
			break;
		case EngineNS::NxRHI::RHI_VK:
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
	}

	void NvAftermath::RegByteCode(const char* name, void* pCode, UINT size)
	{
		if (gNvGpuCrashTracker.IsInitialized() == false)
			return;
		std::vector<uint8_t> data, debugData;
		data.resize(size);
		memcpy(&data[0], pCode, size);
		gNvGpuCrashTracker.GetShaderDatabase()->AddShaderBinary(name, data);
		/*data.resize(size);
		memcpy(&data[0], pCode, size);*/
		debugData.resize(size);
		memcpy(&debugData[0], pCode, size);
		gNvGpuCrashTracker.GetShaderDatabase()->AddShaderBinaryWithDebugInfo(name, debugData, data);
	}
};

NS_END