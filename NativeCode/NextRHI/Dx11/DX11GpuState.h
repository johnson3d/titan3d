#pragma once
#include "../NxGpuState.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11Sampler : public ISampler
	{
	public:
		DX11Sampler();
		~DX11Sampler();
		bool Init(DX11GpuDevice* device, const FSamplerDesc& desc);
	public:
		ID3D11SamplerState* mState = nullptr;
	};

	class DX11GpuPipeline : public IGpuPipeline
	{
	public:
		DX11GpuPipeline();
		~DX11GpuPipeline();
		bool Init(DX11GpuDevice* device, const FGpuPipelineDesc& desc);
	public:
		ID3D11RasterizerState* mRasterState = nullptr;
		ID3D11DepthStencilState* mDepthStencilState = nullptr;
		ID3D11BlendState* mBlendState = nullptr;
	};

	class DX11GpuDrawState : public IGpuDrawState
	{

	};
}

NS_END