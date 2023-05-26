#pragma once
#include "../NxGpuState.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	struct DX12DescriptorSetPagedObject;

	class DX12Sampler : public ISampler
	{
	public:
		DX12Sampler();
		~DX12Sampler();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, const FSamplerDesc& desc);
	public:
		TWeakRefHandle<DX12GpuDevice>			mDeviceRef;
		AutoRef<DX12DescriptorSetPagedObject>	mView;
	};

	class DX12GpuPipeline : public IGpuPipeline
	{
	public:
		DX12GpuPipeline();
		~DX12GpuPipeline();
		bool Init(DX12GpuDevice* device, const FGpuPipelineDesc& desc);
	public:
		D3D12_RASTERIZER_DESC			mRasterState{};
		D3D12_BLEND_DESC				mBlendState{};
		D3D12_DEPTH_STENCIL_DESC		mDepthStencilState{};
	};

	class DX12GpuDrawState : public IGpuDrawState
	{
	public:
		virtual bool BuildState(IGpuDevice* device) override;
	public:
		AutoRef<ID3D12PipelineState>	mDxState;
	};
}

NS_END