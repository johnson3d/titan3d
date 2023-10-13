#pragma once

#include "../NxRHIDefine.h"
#include "../NxGpuDevice.h"
#include "../NxBuffer.h"

#include "../../Base/allocator/PagedAllocator.h"

#pragma warning(push)
#pragma warning(disable:4005)
#include <d3d12.h>
#include <d3d12Shader.h>
#include <d3dcompiler.h>
#include <dxgi1_4.h>
#pragma warning(pop)

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12CmdRecorder;

	inline DXGI_FORMAT FormatToDX12Format(EPixelFormat pixel_fmt)
	{
		switch (pixel_fmt)
		{
		case PXF_UNKNOWN:
			return DXGI_FORMAT_UNKNOWN;
		case PXF_R16_FLOAT:
			return DXGI_FORMAT_R16_FLOAT;
		case PXF_R16_UINT:
			return DXGI_FORMAT_R16_UINT;
		case PXF_R16_SINT:
			return DXGI_FORMAT_R16_SINT;
		case PXF_R16_UNORM:
			return DXGI_FORMAT_R16_UNORM;
		case PXF_R16_SNORM:
			return DXGI_FORMAT_R16_SNORM;
		case PXF_R32_UINT:
			return DXGI_FORMAT_R32_UINT;
		case PXF_R32_SINT:
			return DXGI_FORMAT_R32_SINT;
		case PXF_R32_FLOAT:
			return DXGI_FORMAT_R32_FLOAT;
		case PXF_R8G8B8A8_SINT:
			return DXGI_FORMAT_R8G8B8A8_SINT;
		case PXF_R8G8B8A8_UINT:
			return DXGI_FORMAT_R8G8B8A8_UINT;
		case PXF_R8G8B8A8_UNORM:
			return DXGI_FORMAT_R8G8B8A8_UNORM;
		case PXF_R8G8B8A8_SNORM:
			return DXGI_FORMAT_R8G8B8A8_SNORM;
		case PXF_R16G16_UINT:
			return DXGI_FORMAT_R16G16_UINT;
		case PXF_R16G16_SINT:
			return DXGI_FORMAT_R16G16_SINT;
		case PXF_R16G16_FLOAT:
			return DXGI_FORMAT_R16G16_FLOAT;
		case PXF_R16G16_UNORM:
			return DXGI_FORMAT_R16G16_UNORM;
		case PXF_R16G16_SNORM:
			return DXGI_FORMAT_R16G16_SNORM;
		case PXF_R16G16B16A16_UINT:
			return DXGI_FORMAT_R16G16B16A16_UINT;
		case PXF_R16G16B16A16_SINT:
			return DXGI_FORMAT_R16G16B16A16_SINT;
		case PXF_R16G16B16A16_FLOAT:
			return DXGI_FORMAT_R16G16B16A16_FLOAT;
		case PXF_R16G16B16A16_UNORM:
			return DXGI_FORMAT_R16G16B16A16_UNORM;
		case PXF_R16G16B16A16_SNORM:
			return DXGI_FORMAT_R16G16B16A16_SNORM;
		case PXF_R32G32B32A32_UINT:
			return DXGI_FORMAT_R32G32B32A32_UINT;
		case PXF_R32G32B32A32_SINT:
			return DXGI_FORMAT_R32G32B32A32_SINT;
		case PXF_R32G32B32A32_FLOAT:
			return DXGI_FORMAT_R32G32B32A32_FLOAT;
		case PXF_R32G32B32_UINT:
			return DXGI_FORMAT_R32G32B32_UINT;
		case PXF_R32G32B32_SINT:
			return DXGI_FORMAT_R32G32B32_SINT;
		case PXF_R32G32B32_FLOAT:
			return DXGI_FORMAT_R32G32B32_FLOAT;
		case PXF_R32G32_UINT:
			return DXGI_FORMAT_R32G32_UINT;
		case PXF_R32G32_SINT:
			return DXGI_FORMAT_R32G32_SINT;
		case PXF_R32G32_FLOAT:
			return DXGI_FORMAT_R32G32_FLOAT;
		case PXF_D24_UNORM_S8_UINT:
			return DXGI_FORMAT_D24_UNORM_S8_UINT;
		case PXF_D32_FLOAT:
			return DXGI_FORMAT_D32_FLOAT;
		case PXF_D32_FLOAT_S8X24_UINT:
			return DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
		case PXF_D16_UNORM:
			return DXGI_FORMAT_D16_UNORM;
		case PXF_B8G8R8A8_UNORM:
			return DXGI_FORMAT_B8G8R8A8_UNORM;
		case PXF_R11G11B10_FLOAT:
			return DXGI_FORMAT_R11G11B10_FLOAT;
		case PXF_R8G8_UNORM:
			return DXGI_FORMAT_R8G8_UNORM;
		case PXF_R8_UNORM:
			return DXGI_FORMAT_R8_UNORM;
		case PXF_R32_TYPELESS:
			return DXGI_FORMAT_R32_TYPELESS;
		case PXF_R32G32B32A32_TYPELESS:
			return DXGI_FORMAT_R32G32B32A32_TYPELESS;
		case PXF_R32G32B32_TYPELESS:
			return DXGI_FORMAT_R32G32B32_TYPELESS;
		case PXF_R16G16B16A16_TYPELESS:
			return DXGI_FORMAT_R16G16B16A16_TYPELESS;
		case PXF_R32G32_TYPELESS:
			return DXGI_FORMAT_R32G32_TYPELESS;
		case PXF_R32G8X24_TYPELESS:
			return DXGI_FORMAT_R32G8X24_TYPELESS;
		case PXF_R10G10B10A2_TYPELESS:
			return DXGI_FORMAT_R10G10B10A2_TYPELESS;
		case PXF_R10G10B10A2_UNORM:
			return DXGI_FORMAT_R10G10B10A2_UNORM;
		case PXF_R10G10B10A2_UINT:
			return DXGI_FORMAT_R10G10B10A2_UINT;
		case PXF_R8G8B8A8_TYPELESS:
			return DXGI_FORMAT_R8G8B8A8_TYPELESS;
		case PXF_R8G8B8A8_UNORM_SRGB:
			return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
		case PXF_R16G16_TYPELESS:
			return DXGI_FORMAT_R16G16_TYPELESS;
		case PXF_R24G8_TYPELESS:
			return DXGI_FORMAT_R24G8_TYPELESS;
		case PXF_R24_UNORM_X8_TYPELESS:
			return DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		case PXF_X24_TYPELESS_G8_UINT:
			return DXGI_FORMAT_X24_TYPELESS_G8_UINT;
		case PXF_R8G8_TYPELESS:
			return DXGI_FORMAT_R8G8_TYPELESS;
		case PXF_R8G8_UINT:
			return DXGI_FORMAT_R8G8_UINT;
		case PXF_R8G8_SNORM:
			return DXGI_FORMAT_R8G8_SNORM;
		case PXF_R8G8_SINT:
			return DXGI_FORMAT_R8G8_SINT;
		case PXF_R16_TYPELESS:
			return DXGI_FORMAT_R16_TYPELESS;
		case PXF_R8_TYPELESS:
			return DXGI_FORMAT_R8_TYPELESS;
		case PXF_R8_UINT:
			return DXGI_FORMAT_R8_UINT;
		case PXF_R8_SNORM:
			return DXGI_FORMAT_R8_SNORM;
		case PXF_R8_SINT:
			return DXGI_FORMAT_R8_SINT;
		case PXF_A8_UNORM:
			return DXGI_FORMAT_A8_UNORM;
		case PXF_B8G8R8X8_UNORM:
			return DXGI_FORMAT_B8G8R8X8_UNORM;
		case PXF_B8G8R8A8_TYPELESS:
			return DXGI_FORMAT_B8G8R8A8_TYPELESS;
		case PXF_B8G8R8A8_UNORM_SRGB:
			return DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
		case PXF_B8G8R8X8_TYPELESS:
			return DXGI_FORMAT_B8G8R8X8_TYPELESS;
		case PXF_B8G8R8X8_UNORM_SRGB:
			return DXGI_FORMAT_B8G8R8X8_UNORM_SRGB;
		case PXF_B5G6R5_UNORM:
			return DXGI_FORMAT_B5G6R5_UNORM;
		case PXF_B4G4R4A4_UNORM:
			return DXGI_FORMAT_B4G4R4A4_UNORM;
		case PXF_BC1_TYPELESS:
			return DXGI_FORMAT_BC1_TYPELESS;
		case PXF_BC1_UNORM:
			return DXGI_FORMAT_BC1_UNORM;
		case PXF_BC1_UNORM_SRGB:
			return DXGI_FORMAT_BC1_UNORM_SRGB;
		case PXF_BC2_TYPELESS:
			return DXGI_FORMAT_BC2_TYPELESS;
		case PXF_BC2_UNORM:
			return DXGI_FORMAT_BC2_UNORM;
		case PXF_BC2_UNORM_SRGB:
			return DXGI_FORMAT_BC2_UNORM_SRGB;
		case PXF_BC3_TYPELESS:
			return DXGI_FORMAT_BC3_TYPELESS;
		case PXF_BC3_UNORM:
			return DXGI_FORMAT_BC3_UNORM;
		case PXF_BC3_UNORM_SRGB:
			return DXGI_FORMAT_BC3_UNORM_SRGB;
		case PXF_BC4_TYPELESS:
			return DXGI_FORMAT_BC4_TYPELESS;
		case PXF_BC4_UNORM:
			return DXGI_FORMAT_BC4_UNORM;
		case PXF_BC4_SNORM:
			return DXGI_FORMAT_BC4_SNORM;
		case PXF_BC5_TYPELESS:
			return DXGI_FORMAT_BC5_TYPELESS;
		case PXF_BC5_UNORM:
			return DXGI_FORMAT_BC5_UNORM;
		case PXF_BC5_SNORM:
			return DXGI_FORMAT_BC5_SNORM;
		case PXF_BC6H_TYPELESS:
			return DXGI_FORMAT_BC6H_TYPELESS;
		case PXF_BC6H_UF16:
			return DXGI_FORMAT_BC6H_UF16;
		case PXF_BC6H_SF16:
			return DXGI_FORMAT_BC6H_SF16;
		case PXF_BC7_TYPELESS:
			return DXGI_FORMAT_BC7_TYPELESS;
		case PXF_BC7_UNORM:
			return DXGI_FORMAT_BC7_UNORM;
		case PXF_BC7_UNORM_SRGB:
			return DXGI_FORMAT_BC7_UNORM_SRGB;
		case PXF_ETC2_RGB8:
		case PXF_ETC2_SRGB8:
		case PXF_ETC2_RGBA8:
		case PXF_ETC2_SRGBA8:
		case PXF_ETC2_R11:
		case PXF_ETC2_SIGNED_R11:
		case PXF_ETC2_RG11:
		case PXF_ETC2_SIGNED_RG11:
		case PXF_ETC2_RGBA1:
		case PXF_ETC2_SRGBA1:
		default:
			break;
		}
		return DXGI_FORMAT_UNKNOWN;
	}
	inline EPixelFormat DX12FormatToFormat(DXGI_FORMAT fmt)
	{
		switch (fmt)
		{
		case DXGI_FORMAT_UNKNOWN:
			return PXF_UNKNOWN;
		case DXGI_FORMAT_R32G32B32A32_TYPELESS:
			return PXF_R32G32B32A32_TYPELESS;
		case DXGI_FORMAT_R32G32B32A32_FLOAT:
			return PXF_R32G32B32A32_FLOAT;
		case DXGI_FORMAT_R32G32B32A32_UINT:
			return PXF_R32G32B32A32_UINT;
		case DXGI_FORMAT_R32G32B32A32_SINT:
			return PXF_R32G32B32A32_SINT;
		case DXGI_FORMAT_R32G32B32_TYPELESS:
			return PXF_R32G32B32_TYPELESS;
		case DXGI_FORMAT_R32G32B32_FLOAT:
			return PXF_R32G32B32_FLOAT;
		case DXGI_FORMAT_R32G32B32_UINT:
			return PXF_R32G32B32_UINT;
		case DXGI_FORMAT_R32G32B32_SINT:
			return PXF_R32G32B32_SINT;
		case DXGI_FORMAT_R16G16B16A16_TYPELESS:
			return PXF_R16G16B16A16_TYPELESS;
		case DXGI_FORMAT_R16G16B16A16_FLOAT:
			return PXF_R16G16B16A16_FLOAT;
		case DXGI_FORMAT_R16G16B16A16_UNORM:
			return PXF_R16G16B16A16_UNORM;
		case DXGI_FORMAT_R16G16B16A16_UINT:
			return PXF_R16G16B16A16_UINT;
		case DXGI_FORMAT_R16G16B16A16_SNORM:
			return PXF_R16G16B16A16_SNORM;
		case DXGI_FORMAT_R16G16B16A16_SINT:
			return PXF_R16G16B16A16_SINT;
		case DXGI_FORMAT_R32G32_TYPELESS:
			return PXF_R32G32_TYPELESS;
		case DXGI_FORMAT_R32G32_FLOAT:
			return PXF_R32G32_FLOAT;
		case DXGI_FORMAT_R32G32_UINT:
			return PXF_R32G32_UINT;
		case DXGI_FORMAT_R32G32_SINT:
			return PXF_R32G32_SINT;
		case DXGI_FORMAT_R32G8X24_TYPELESS:
			return PXF_R32G8X24_TYPELESS;
		case DXGI_FORMAT_D32_FLOAT_S8X24_UINT:
			return PXF_D32_FLOAT_S8X24_UINT;
		case DXGI_FORMAT_R32_FLOAT_X8X24_TYPELESS:
			break;
		case DXGI_FORMAT_X32_TYPELESS_G8X24_UINT:
			break;
		case DXGI_FORMAT_R10G10B10A2_TYPELESS:
			return PXF_R10G10B10A2_TYPELESS;
		case DXGI_FORMAT_R10G10B10A2_UNORM:
			return PXF_R10G10B10A2_UNORM;
		case DXGI_FORMAT_R10G10B10A2_UINT:
			return PXF_R10G10B10A2_UINT;
		case DXGI_FORMAT_R11G11B10_FLOAT:
			return PXF_R11G11B10_FLOAT;
		case DXGI_FORMAT_R8G8B8A8_TYPELESS:
			return PXF_R8G8B8A8_TYPELESS;
		case DXGI_FORMAT_R8G8B8A8_UNORM:
			return PXF_R8G8B8A8_UNORM;
		case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB:
			return PXF_R8G8B8A8_UNORM_SRGB;
		case DXGI_FORMAT_R8G8B8A8_UINT:
			return PXF_R8G8B8A8_UINT;
		case DXGI_FORMAT_R8G8B8A8_SNORM:
			return PXF_R8G8B8A8_SNORM;
		case DXGI_FORMAT_R8G8B8A8_SINT:
			return PXF_R8G8B8A8_SINT;
		case DXGI_FORMAT_R16G16_TYPELESS:
			return PXF_R16G16_TYPELESS;
		case DXGI_FORMAT_R16G16_FLOAT:
			return PXF_R16G16_FLOAT;
		case DXGI_FORMAT_R16G16_UNORM:
			return PXF_R16G16_UNORM;
		case DXGI_FORMAT_R16G16_UINT:
			return PXF_R16G16_UINT;
		case DXGI_FORMAT_R16G16_SNORM:
			return PXF_R16G16_SNORM;
		case DXGI_FORMAT_R16G16_SINT:
			return PXF_R16G16_SINT;
		case DXGI_FORMAT_R32_TYPELESS:
			return PXF_R32_TYPELESS;
		case DXGI_FORMAT_D32_FLOAT:
			return PXF_D32_FLOAT;
		case DXGI_FORMAT_R32_FLOAT:
			return PXF_R32_FLOAT;
		case DXGI_FORMAT_R32_UINT:
			return PXF_R32_UINT;
		case DXGI_FORMAT_R32_SINT:
			return PXF_R32_SINT;
		case DXGI_FORMAT_R24G8_TYPELESS:
			return PXF_R24G8_TYPELESS;
		case DXGI_FORMAT_D24_UNORM_S8_UINT:
			return PXF_D24_UNORM_S8_UINT;
		case DXGI_FORMAT_R24_UNORM_X8_TYPELESS:
			return PXF_R24_UNORM_X8_TYPELESS;
		case DXGI_FORMAT_X24_TYPELESS_G8_UINT:
			return PXF_X24_TYPELESS_G8_UINT;
		case DXGI_FORMAT_R8G8_TYPELESS:
			return PXF_R8G8_TYPELESS;
		case DXGI_FORMAT_R8G8_UNORM:
			return PXF_R8G8_UNORM;
		case DXGI_FORMAT_R8G8_UINT:
			return PXF_R8G8_UINT;
		case DXGI_FORMAT_R8G8_SNORM:
			return PXF_R8G8_SNORM;
		case DXGI_FORMAT_R8G8_SINT:
			return PXF_R8G8_SINT;
		case DXGI_FORMAT_R16_TYPELESS:
			return PXF_R16_TYPELESS;
		case DXGI_FORMAT_R16_FLOAT:
			return PXF_R16_FLOAT;
		case DXGI_FORMAT_D16_UNORM:
			return PXF_D16_UNORM;
		case DXGI_FORMAT_R16_UNORM:
			return PXF_R16_UNORM;
		case DXGI_FORMAT_R16_UINT:
			return PXF_R16_UINT;
		case DXGI_FORMAT_R16_SNORM:
			return PXF_R16_SNORM;
		case DXGI_FORMAT_R16_SINT:
			return PXF_R16_SINT;
		case DXGI_FORMAT_R8_TYPELESS:
			return PXF_R8_TYPELESS;
		case DXGI_FORMAT_R8_UNORM:
			return PXF_R8_UNORM;
		case DXGI_FORMAT_R8_UINT:
			return PXF_R8_UINT;
		case DXGI_FORMAT_R8_SNORM:
			return PXF_R8_SNORM;
		case DXGI_FORMAT_R8_SINT:
			return PXF_R8_SINT;
		case DXGI_FORMAT_A8_UNORM:
			return PXF_A8_UNORM;
		case DXGI_FORMAT_R1_UNORM:
			break;
		case DXGI_FORMAT_R9G9B9E5_SHAREDEXP:
			break;
		case DXGI_FORMAT_R8G8_B8G8_UNORM:
			break;
		case DXGI_FORMAT_G8R8_G8B8_UNORM:
			break;
		case DXGI_FORMAT_BC1_TYPELESS:
			return PXF_BC1_TYPELESS;
		case DXGI_FORMAT_BC1_UNORM:
			return PXF_BC1_UNORM;
		case DXGI_FORMAT_BC1_UNORM_SRGB:
			return PXF_BC1_UNORM_SRGB;
		case DXGI_FORMAT_BC2_TYPELESS:
			return PXF_BC2_TYPELESS;
		case DXGI_FORMAT_BC2_UNORM:
			return PXF_BC2_UNORM;
		case DXGI_FORMAT_BC2_UNORM_SRGB:
			return PXF_BC2_UNORM_SRGB;
		case DXGI_FORMAT_BC3_TYPELESS:
			return PXF_BC3_TYPELESS;
		case DXGI_FORMAT_BC3_UNORM:
			return PXF_BC3_UNORM;
		case DXGI_FORMAT_BC3_UNORM_SRGB:
			return PXF_BC3_UNORM_SRGB;
		case DXGI_FORMAT_BC4_TYPELESS:
			return PXF_BC4_TYPELESS;
		case DXGI_FORMAT_BC4_UNORM:
			return PXF_BC4_UNORM;
		case DXGI_FORMAT_BC4_SNORM:
			return PXF_BC4_SNORM;
		case DXGI_FORMAT_BC5_TYPELESS:
			return PXF_BC5_TYPELESS;
		case DXGI_FORMAT_BC5_UNORM:
			return PXF_BC5_UNORM;
		case DXGI_FORMAT_BC5_SNORM:
			return PXF_BC5_SNORM;
		case DXGI_FORMAT_B5G6R5_UNORM:
			return PXF_B5G6R5_UNORM;
		case DXGI_FORMAT_B5G5R5A1_UNORM:
			break;
		case DXGI_FORMAT_B8G8R8A8_UNORM:
			return PXF_B8G8R8A8_UNORM;
		case DXGI_FORMAT_B8G8R8X8_UNORM:
			return PXF_B8G8R8X8_UNORM;
		case DXGI_FORMAT_R10G10B10_XR_BIAS_A2_UNORM:
			break;
		case DXGI_FORMAT_B8G8R8A8_TYPELESS:
			return PXF_B8G8R8A8_TYPELESS;
		case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB:
			return PXF_B8G8R8A8_UNORM_SRGB;
		case DXGI_FORMAT_B8G8R8X8_TYPELESS:
			return PXF_B8G8R8X8_TYPELESS;
		case DXGI_FORMAT_B8G8R8X8_UNORM_SRGB:
			return PXF_B8G8R8X8_UNORM_SRGB;
		case DXGI_FORMAT_BC6H_TYPELESS:
			return PXF_BC6H_TYPELESS;
		case DXGI_FORMAT_BC6H_UF16:
			return PXF_BC6H_UF16;
		case DXGI_FORMAT_BC6H_SF16:
			return PXF_BC6H_SF16;
		case DXGI_FORMAT_BC7_TYPELESS:
			return PXF_BC7_TYPELESS;
		case DXGI_FORMAT_BC7_UNORM:
			return PXF_BC7_UNORM;
		case DXGI_FORMAT_BC7_UNORM_SRGB:
			return PXF_BC7_UNORM_SRGB;
		case DXGI_FORMAT_AYUV:
			break;
		case DXGI_FORMAT_Y410:
			break;
		case DXGI_FORMAT_Y416:
			break;
		case DXGI_FORMAT_NV12:
			break;
		case DXGI_FORMAT_P010:
			break;
		case DXGI_FORMAT_P016:
			break;
		case DXGI_FORMAT_420_OPAQUE:
			break;
		case DXGI_FORMAT_YUY2:
			break;
		case DXGI_FORMAT_Y210:
			break;
		case DXGI_FORMAT_Y216:
			break;
		case DXGI_FORMAT_NV11:
			break;
		case DXGI_FORMAT_AI44:
			break;
		case DXGI_FORMAT_IA44:
			break;
		case DXGI_FORMAT_P8:
			break;
		case DXGI_FORMAT_A8P8:
			break;
		case DXGI_FORMAT_B4G4R4A4_UNORM:
			break;
		case DXGI_FORMAT_P208:
			break;
		case DXGI_FORMAT_V208:
			break;
		case DXGI_FORMAT_V408:
			break;
		case DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE:
			break;
		case DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE:
			break;
		case DXGI_FORMAT_FORCE_UINT:
			break;
		default:
			break;
		}

		return EPixelFormat::PXF_UNKNOWN;
	}
	class DX12CommandAllocatorManager : public VIUnknown
	{
	public:
		AutoRef<DX12CmdRecorder> Alloc(ID3D12Device* device);
		void Free(const AutoRef<DX12CmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence);
		void TickRecycle();
		void Finalize();
		void UnsafeDirectFree(const AutoRef<DX12CmdRecorder>& allocator);
	public:
		VSLLock				mLocker;
		std::queue<AutoRef<DX12CmdRecorder>>		CmdAllocators;
		struct FWaitRecycle
		{
			UINT64							WaitValue = 0;
			AutoRef<IFence>					Fence;
			AutoRef<DX12CmdRecorder>		Allocator;
		};
		std::vector<FWaitRecycle>	Recycles;
	};

	struct DX12GpuHeap : public IGpuHeap
	{
		//AutoRef<ID3D12Heap>			mDxHeap;
		AutoRef<ID3D12Resource>		mGpuResource;
		virtual UINT64 GetGPUVirtualAddress() override
		{
			auto result = mGpuResource->GetGPUVirtualAddress();
			ASSERT(result != 0);
			return result;
		}
		virtual void* GetHWBuffer() override {
			return mGpuResource;
		}
		void SetDebugName(const char* name);
	};
	struct FDX12DefaultGpuMemory : public FGpuMemory
	{
		~FDX12DefaultGpuMemory();
		DX12GpuHeap* GetDX12GpuHeap() {
			return (DX12GpuHeap*)GpuHeap;
		}
		virtual void FreeMemory();
	};

	class DX12DefaultGpuMemAllocator : public IGpuMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		static AutoRef<FGpuMemory> Alloc(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const char* debugName);
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size, const char* name);
		virtual void Free(FGpuMemory* memory);

		static FGpuMemHolder* AllocGpuMem(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const char* debugName) {
			auto result = new FGpuMemHolder();
			result->GpuMem = Alloc(device, resDesc, heapDesc, resState, debugName);
			return result;
		}
	};

	class DX12PagedGpuMemAllocator : public IPagedGpuMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;
		virtual UINT GetBatchCount(UINT64 size) {
			return (UINT)(mResDesc.Width / size);
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count, const char* name) override;
	};

	///-----------------------------------------------------------
	///DX12DescriptorSetPagedObject
	struct DX12PagedHeap : public MemAlloc::FPagedObject<AutoRef<ID3D12DescriptorHeap>>
	{
		//DX12ShaderEffect*		ShaderEffect = nullptr;
		D3D12_GPU_DESCRIPTOR_HANDLE	GetGpuAddress(int index = 0);
		D3D12_CPU_DESCRIPTOR_HANDLE	GetCpuAddress(int index);
		void BindToHeap(DX12GpuDevice* device, DX12PagedHeap* dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType);
		//D3D12_DESCRIPTOR_HEAP_TYPE	HeapType = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		SIZE_T						OffsetInPage = 0;

		std::vector<AutoRef<IGpuResource>>	RefResources;
	};
	struct DX12HeapHolder : public IGpuResource
	{
		AutoRef<DX12PagedHeap>	Heap;
		~DX12HeapHolder()
		{
			if (Heap != nullptr)
			{
				Heap->Free();
				for (auto& i : Heap->RefResources)
				{
					i = nullptr;
				}

				Heap = nullptr;
			}
		}
		D3D12_GPU_DESCRIPTOR_HANDLE	GetGpuAddress(int index = 0)
		{
			return Heap->GetGpuAddress(index);
		}
		D3D12_CPU_DESCRIPTOR_HANDLE	GetCpuAddress(int index)
		{
			return Heap->GetCpuAddress(index);
		}
		void BindToHeap(DX12GpuDevice* device, DX12PagedHeap* dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType)
		{
			return Heap->BindToHeap(device, dest, destIndex, srcIndex, HeapType);
		}
	};

	struct DX12HeapCreator
	{
		struct DX12DescriptorSetPage : public MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>
		{
			AutoRef<ID3D12DescriptorHeap>		mGpuHeap;
		};

		DX12HeapCreator()
		{
			
		}
		TWeakRefHandle<DX12GpuDevice>		mDeviceRef;

		enum EDescriptorType
		{
			Graphics,
			Compute,
		};
		EDescriptorType Type = EDescriptorType::Graphics;
		bool			IsDescriptorSet = false;
		
		D3D12_DESCRIPTOR_HEAP_TYPE			HeapType = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		D3D12_DESCRIPTOR_HEAP_DESC			mDesc{};
		//bool								IsSampler = false;
		std::wstring						DebugName;
		UINT								SerialId = 0;
		using ObjectType = AutoRef<ID3D12DescriptorHeap>;
		using PagedObjectType = MemAlloc::FPagedObject<ObjectType>;
		using PageType = DX12DescriptorSetPage;// MemAlloc::FPage<ObjectType>;
		using AllocatorType = MemAlloc::FAllocatorBase<ObjectType>;

		UINT GetPageSize() const{
			return PageSize;
		}
		UINT PageSize = 128;

		PageType* CreatePage(UINT pageSize);
		PagedObjectType* CreatePagedObject(PageType* page, UINT index);
		void OnAlloc(AllocatorType* pAllocator, PagedObjectType* obj);
		void OnFree(AllocatorType* pAllocator, PagedObjectType* obj);
		void FinalCleanup(MemAlloc::FPage<ObjectType>* page);
	};

	struct DX12HeapAllocator : public MemAlloc::FPagedObjectAllocator<DX12HeapCreator::ObjectType, DX12HeapCreator>
	{
		UINT			mDescriptorStride = 0;
		DX12HeapHolder* AllocDX12Heap()
		{
			auto result = new DX12HeapHolder();
			result->Heap = this->Alloc<DX12PagedHeap>();
			return result;
		}
	};	
	
	class DX12HeapAllocatorManager : public IWeakReference
	{
		std::map<UINT64, AutoRef<DX12HeapAllocator>>		mAllocators;
	public:
		DX12HeapHolder* AllocDX12Heap(DX12GpuDevice* pDevice, UINT numOfDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type);
	};
	///-----------------------------------------------------------

	class DX12ResourceDebugMapper
	{
	public:
		struct DX12ResourceDebugInfo : public VIUnknown
		{
			std::string Name;
		};
		std::map<ID3D12Resource*, AutoRef<DX12ResourceDebugInfo>>	mMapper;
		VSLLock									mLocker;
	public:
		static DX12ResourceDebugMapper* Get();
		void SetDebugMapper(ID3D12Resource* res, const char* name);

		DX12ResourceDebugInfo* FindDebugInfo(ID3D12Resource* res) {
			auto iter = mMapper.find(res);
			if (iter == mMapper.end())
			{
				return nullptr;
			}
			else
			{
				return iter->second;
			}
		}
	};
}

NS_END