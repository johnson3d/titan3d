#include "NvAftermath.h"
#include "../../NextRHI/Dx11/DX11GpuDevice.h"
#include "../../NextRHI/Dx12/DX12GpuDevice.h"
#include "../../Base/io/vfxfile.h"

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

	std::string GetOpStr(D3D12_AUTO_BREADCRUMB_OP op)
	{
		std::string opStr;
		switch (op)
		{
		case D3D12_AUTO_BREADCRUMB_OP_SETMARKER:
			opStr = "SETMARKER";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_BEGINEVENT:
			opStr = "BEGINEVENT";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_ENDEVENT:
			opStr = "ENDEVENT";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DRAWINSTANCED:
			opStr = "DRAWINSTANCED";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DRAWINDEXEDINSTANCED:
			opStr = "DRAWINDEXEDINSTANCED";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_EXECUTEINDIRECT:
			opStr = "EXECUTEINDIRECT";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DISPATCH:
			opStr = "DISPATCH";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_COPYBUFFERREGION:
			opStr = "COPYBUFFERREGION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_COPYTEXTUREREGION:
			opStr = "COPYTEXTUREREGION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_COPYRESOURCE:
			opStr = "COPYRESOURCE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_COPYTILES:
			opStr = "COPYTILES";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_RESOLVESUBRESOURCE:
			opStr = "RESOLVESUBRESOURCE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_CLEARRENDERTARGETVIEW:
			opStr = "CLEARRENDERTARGETVIEW";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_CLEARUNORDEREDACCESSVIEW:
			opStr = "CLEARUNORDEREDACCESSVIEW";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_CLEARDEPTHSTENCILVIEW:
			opStr = "CLEARDEPTHSTENCILVIEW";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_RESOURCEBARRIER:
			opStr = "RESOURCEBARRIER";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_EXECUTEBUNDLE:
			opStr = "EXECUTEBUNDLE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_PRESENT:
			opStr = "PRESENT";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_RESOLVEQUERYDATA:
			opStr = "RESOLVEQUERYDATA";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_BEGINSUBMISSION:
			opStr = "BEGINSUBMISSION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_ENDSUBMISSION:
			opStr = "ENDSUBMISSION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME:
			opStr = "DECODEFRAME";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_PROCESSFRAMES:
			opStr = "PROCESSFRAMES";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_ATOMICCOPYBUFFERUINT:
			opStr = "ATOMICCOPYBUFFERUINT";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_ATOMICCOPYBUFFERUINT64:
			opStr = "ATOMICCOPYBUFFERUINT64";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_RESOLVESUBRESOURCEREGION:
			opStr = "RESOLVESUBRESOURCEREGION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_WRITEBUFFERIMMEDIATE:
			opStr = "WRITEBUFFERIMMEDIATE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME1:
			opStr = "DECODEFRAME1";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_SETPROTECTEDRESOURCESESSION:
			opStr = "SETPROTECTEDRESOURCESESSION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DECODEFRAME2:
			opStr = "DECODEFRAME2";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_PROCESSFRAMES1:
			opStr = "PROCESSFRAMES1";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_BUILDRAYTRACINGACCELERATIONSTRUCTURE:
			opStr = "BUILDRAYTRACINGACCELERATIONSTRUCTURE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_EMITRAYTRACINGACCELERATIONSTRUCTUREPOSTBUILDINFO:
			opStr = "EMITRAYTRACINGACCELERATIONSTRUCTUREPOSTBUILDINFO";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_COPYRAYTRACINGACCELERATIONSTRUCTURE:
			opStr = "COPYRAYTRACINGACCELERATIONSTRUCTURE";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DISPATCHRAYS:
			opStr = "DISPATCHRAYS";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_INITIALIZEMETACOMMAND:
			opStr = "INITIALIZEMETACOMMAND";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_EXECUTEMETACOMMAND:
			opStr = "EXECUTEMETACOMMAND";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_ESTIMATEMOTION:
			opStr = "ESTIMATEMOTION";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_RESOLVEMOTIONVECTORHEAP:
			opStr = "RESOLVEMOTIONVECTORHEAP";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_SETPIPELINESTATE1:
			opStr = "SETPIPELINESTATE1";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_INITIALIZEEXTENSIONCOMMAND:
			opStr = "INITIALIZEEXTENSIONCOMMAND";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_EXECUTEEXTENSIONCOMMAND:
			opStr = "EXECUTEEXTENSIONCOMMAND";
			break;
		case D3D12_AUTO_BREADCRUMB_OP_DISPATCHMESH:
			opStr = "DISPATCHMESH";
			break;
		default:
			break;
		}
		return opStr;
	}
	void DX12_OnDredDump(ID3D12Device* mDevice, ID3D12DeviceRemovedExtendedDataSettings* mDredSettings, const char* GDredDir)
	{
		auto hr = mDevice->GetDeviceRemovedReason();
		if (mDredSettings != nullptr)
		{
			AutoRef<ID3D12DeviceRemovedExtendedData> pDred;
			mDevice->QueryInterface(IID_PPV_ARGS(pDred.GetAddressOf()));

			D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT DredAutoBreadcrumbsOutput;
			D3D12_DRED_PAGE_FAULT_OUTPUT DredPageFaultOutput;
			pDred->GetAutoBreadcrumbsOutput(&DredAutoBreadcrumbsOutput);
			pDred->GetPageFaultAllocationOutput(&DredPageFaultOutput);

#define AddCodeLine(txt, ...) {code.AddLine(VStringA_FormatV(txt, __VA_ARGS__).c_str());}
			{
				auto curNode = DredAutoBreadcrumbsOutput.pHeadAutoBreadcrumbNode;
				int nodeIndex = 0;
				while (curNode != nullptr)
				{
					FCodeWriter code;
					std::string n;
					std::string q;
					if (curNode->pCommandListDebugNameA != nullptr)
						n = curNode->pCommandListDebugNameA;
					else if (curNode->pCommandListDebugNameW != nullptr)
						n = StringHelper::wstrtostr(curNode->pCommandListDebugNameW);

					if (curNode->pCommandQueueDebugNameA != nullptr)
						q = curNode->pCommandQueueDebugNameA;
					else if (curNode->pCommandQueueDebugNameW != nullptr)
						q = StringHelper::wstrtostr(curNode->pCommandQueueDebugNameW);
					
					if (n.length() != 0)
						AddCodeLine("Dred Begin HistoryCmdList {%s} in Queue[%s]\r\n", n.c_str(), q.c_str());
					curNode->pLastBreadcrumbValue;
					for (UINT i = 0; i < curNode->BreadcrumbCount; i++)
					{
						D3D12_AUTO_BREADCRUMB_OP op = curNode->pCommandHistory[i];
						std::string opStr = GetOpStr(op);
						AddCodeLine("Dred HistoryOp[%d] {%s}\r\n", i, opStr.c_str());
					}

					std::string crashed = "";
					if (curNode->pLastBreadcrumbValue != nullptr)
					{
						auto lastIndex = *curNode->pLastBreadcrumbValue;
						if (curNode->BreadcrumbCount > lastIndex)
						{
							crashed = VStringA_FormatV("_crash_%d", *curNode->pLastBreadcrumbValue);
						}
						auto lastOp = curNode->pCommandHistory[*curNode->pLastBreadcrumbValue];
						AddCodeLine("Dred {%s} LastOp = %s[%d]\r\n", n.c_str(), GetOpStr(lastOp).c_str(), *curNode->pLastBreadcrumbValue);
					}


					if (n.length() != 0)
						AddCodeLine("Dred End HistoryCmdList {%s}\r\n", n.c_str());

					auto file = GDredDir + VStringA_FormatV("%d_%s%s.bread", nodeIndex, n.c_str(), crashed.c_str());
					VFile io;
					if (io.Open(file.c_str(), VFile::modeWrite | VFile::modeCreate))
					{
						io.Write(code.ClassCode.c_str(), code.ClassCode.length());
						io.Close();
					}

					curNode = curNode->pNext;
					nodeIndex++;
				}
			}
			{
				FCodeWriter code;
				auto curNode = DredPageFaultOutput.pHeadRecentFreedAllocationNode;
				while (curNode != nullptr)
				{
					std::string n;
					if (curNode->ObjectNameA != nullptr)
						n = curNode->ObjectNameA;
					else if (curNode->ObjectNameW != nullptr)
						n = StringHelper::wstrtostr(curNode->ObjectNameW);
					auto atype = curNode->AllocationType;
					std::string opStr;
					switch (atype)
					{
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_QUEUE:
						opStr = "COMMAND_QUEUE";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_ALLOCATOR:
						opStr = "COMMAND_ALLOCATOR";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_STATE:
						opStr = "PIPELINE_STATE";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_LIST:
						opStr = "COMMAND_LIST";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_FENCE:
						opStr = "FENCE";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_DESCRIPTOR_HEAP:
						opStr = "DESCRIPTOR_HEAP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_HEAP:
						opStr = "HEAP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_QUERY_HEAP:
						opStr = "QUERY_HEAP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_SIGNATURE:
						opStr = "COMMAND_SIGNATURE";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_PIPELINE_LIBRARY:
						opStr = "PIPELINE_LIBRARY";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER:
						opStr = "VIDEO_DECODER";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_PROCESSOR:
						opStr = "VIDEO_PROCESSOR";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_RESOURCE:
						opStr = "RESOURCE";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_PASS:
						opStr = "PASS";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSION:
						opStr = "CRYPTOSESSION";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_CRYPTOSESSIONPOLICY:
						opStr = "CRYPTOSESSIONPOLICY";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_PROTECTEDRESOURCESESSION:
						opStr = "PROTECTEDRESOURCESESSION";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_DECODER_HEAP:
						opStr = "VIDEO_DECODER_HEAP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_POOL:
						opStr = "COMMAND_POOL";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_COMMAND_RECORDER:
						opStr = "COMMAND_RECORDER";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_STATE_OBJECT:
						opStr = "STATE_OBJECT";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_METACOMMAND:
						opStr = "METACOMMAND";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_SCHEDULINGGROUP:
						opStr = "SCHEDULINGGROUP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_ESTIMATOR:
						opStr = "VIDEO_MOTION_ESTIMATOR";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_MOTION_VECTOR_HEAP:
						opStr = "VIDEO_MOTION_VECTOR_HEAP";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_VIDEO_EXTENSION_COMMAND:
						opStr = "VIDEO_EXTENSION_COMMAND";
						break;
					case D3D12_DRED_ALLOCATION_TYPE_INVALID:
						opStr = "INVALID";
						break;
					default:
						break;
					}
					AddCodeLine("Dred RecentFree {%s} = %s\r\n", n.c_str(), opStr.c_str());
					curNode = curNode->pNext;
				}
				auto file = GDredDir + VStringA_FormatV("RecentFreedAllocationNode.rfa");
				VFile io;
				if (io.Open(file.c_str(), VFile::modeWrite | VFile::modeCreate))
				{
					io.Write(code.ClassCode.c_str(), code.ClassCode.length());
					io.Close();
				}
			}
			{
				FCodeWriter code;
				auto curNode = DredPageFaultOutput.pHeadExistingAllocationNode;
				while (curNode != nullptr)
				{
					std::string n;
					if (curNode->ObjectNameA != nullptr)
						n = curNode->ObjectNameA;
					else if (curNode->ObjectNameW != nullptr)
						n = StringHelper::wstrtostr(curNode->ObjectNameW);
					AddCodeLine("Dred Existing {%s}\r\n", n.c_str());
					curNode = curNode->pNext;
				}
				auto file = GDredDir + VStringA_FormatV("ExistingAllocationNode.ea");
				VFile io;
				if (io.Open(file.c_str(), VFile::modeWrite | VFile::modeCreate))
				{
					io.Write(code.ClassCode.c_str(), code.ClassCode.length());
					io.Close();
				}
			}
		}
	}
	void NvAftermath::OnDredDump(NxRHI::IGpuDevice* device, const char* GDredDir)
	{
		switch (device->Desc.RhiType)
		{
			case EngineNS::NxRHI::RHI_D3D12:
			{
				DX12_OnDredDump(((NxRHI::DX12GpuDevice*)device)->mDevice, ((NxRHI::DX12GpuDevice*)device)->mDredSettings, GDredDir);
			}
			break;
			default:
				break;
		}
	}
};

NS_END