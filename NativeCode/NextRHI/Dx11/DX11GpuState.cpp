#include "DX11GpuState.h"
#include "DX11GpuDevice.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	inline D3D11_FILTER FilterToDX(ESamplerFilter fiter)
	{
		return (D3D11_FILTER)fiter;
	}
	inline D3D11_COMPARISON_FUNC CmpModeToDX(EComparisionMode cmp)
	{
		switch (cmp)
		{
		case CMP_NEVER:
			return D3D11_COMPARISON_NEVER;
		case CMP_LESS:
			return D3D11_COMPARISON_LESS;
		case CMP_EQUAL:
			return D3D11_COMPARISON_EQUAL;
		case CMP_LESS_EQUAL:
			return D3D11_COMPARISON_LESS_EQUAL;
		case CMP_GREATER:
			return D3D11_COMPARISON_GREATER;
		case CMP_NOT_EQUAL:
			return D3D11_COMPARISON_NOT_EQUAL;
		case CMP_GREATER_EQUAL:
			return D3D11_COMPARISON_GREATER_EQUAL;
		case CMP_ALWAYS:
			return D3D11_COMPARISON_ALWAYS;
		}
		return D3D11_COMPARISON_NEVER;
	}
	inline D3D11_TEXTURE_ADDRESS_MODE AddressModeToDX(EAddressMode mode)
	{
		switch (mode)
		{
		case ADM_WRAP:
			return D3D11_TEXTURE_ADDRESS_WRAP;
		case ADM_MIRROR:
			return D3D11_TEXTURE_ADDRESS_MIRROR;
		case ADM_CLAMP:
			return D3D11_TEXTURE_ADDRESS_CLAMP;
		case ADM_BORDER:
			return D3D11_TEXTURE_ADDRESS_BORDER;
		case ADM_MIRROR_ONCE:
			return D3D11_TEXTURE_ADDRESS_MIRROR_ONCE;
		}
		return D3D11_TEXTURE_ADDRESS_WRAP;
	}
	DX11Sampler::DX11Sampler()
	{
		mState = nullptr;
	}
	DX11Sampler::~DX11Sampler()
	{
		Safe_Release(mState);
	}
	bool DX11Sampler::Init(DX11GpuDevice* device, const FSamplerDesc& desc)
	{
		Desc = desc;

		D3D11_SAMPLER_DESC				mD11Desc;
		mD11Desc.Filter = FilterToDX(desc.Filter);
		mD11Desc.AddressU = AddressModeToDX(desc.AddressU);
		mD11Desc.AddressV = AddressModeToDX(desc.AddressV);
		mD11Desc.AddressW = AddressModeToDX(desc.AddressW);
		mD11Desc.ComparisonFunc = CmpModeToDX(desc.CmpMode);
		mD11Desc.MinLOD = desc.MinLOD;
		mD11Desc.MaxLOD = desc.MaxLOD;
		memcpy(mD11Desc.BorderColor, desc.BorderColor, sizeof(desc.BorderColor));
		mD11Desc.MaxAnisotropy = desc.MaxAnisotropy;
		mD11Desc.MipLODBias = desc.MipLODBias;

		auto hr = device->mDevice->CreateSamplerState(&mD11Desc, &mState);
		if (FAILED(hr))
			return false;

		return true;
	}

	DX11GpuPipeline::DX11GpuPipeline()
	{

	}
	DX11GpuPipeline::~DX11GpuPipeline()
	{
		Safe_Release(mRasterState);
		Safe_Release(mDepthStencilState);
		Safe_Release(mBlendState);
	}
	bool DX11GpuPipeline::Init(DX11GpuDevice* device, const FGpuPipelineDesc& desc)
	{
		Desc = desc;
		
		D3D11_RASTERIZER_DESC			mD11Desc;
		mD11Desc.FillMode = (D3D11_FILL_MODE)Desc.Rasterizer.FillMode;
		mD11Desc.CullMode = (D3D11_CULL_MODE)Desc.Rasterizer.CullMode;
		mD11Desc.FrontCounterClockwise = Desc.Rasterizer.FrontCounterClockwise;
		mD11Desc.DepthBias = Desc.Rasterizer.DepthBias;
		mD11Desc.DepthBiasClamp = Desc.Rasterizer.DepthBiasClamp;
		mD11Desc.SlopeScaledDepthBias = Desc.Rasterizer.SlopeScaledDepthBias;
		mD11Desc.DepthClipEnable = Desc.Rasterizer.DepthClipEnable;
		mD11Desc.ScissorEnable = Desc.Rasterizer.ScissorEnable;
		mD11Desc.MultisampleEnable = Desc.Rasterizer.MultisampleEnable;
		mD11Desc.AntialiasedLineEnable = Desc.Rasterizer.AntialiasedLineEnable;

		if (device->mDevice->CreateRasterizerState(&mD11Desc, &mRasterState) != S_OK)
			return false;

		D3D11_BLEND_DESC dxDesc;
		dxDesc.AlphaToCoverageEnable = Desc.Blend.AlphaToCoverageEnable;
		dxDesc.IndependentBlendEnable = Desc.Blend.IndependentBlendEnable;
		for (int i = 0; i < 8; i++)
		{
			memcpy(&dxDesc.RenderTarget[i], &Desc.Blend.RenderTarget[i], sizeof(FRenderTargetBlendDesc));
		}
		if (device->mDevice->CreateBlendState(&dxDesc, &mBlendState) != S_OK)
			return false;

		D3D11_DEPTH_STENCIL_DESC dxDSDesc;
		memcpy(&dxDSDesc, &Desc.DepthStencil, sizeof(D3D11_DEPTH_STENCIL_DESC));
		if (device->mDevice->CreateDepthStencilState(&dxDSDesc, &mDepthStencilState) != S_OK)
			return false;

		return true;
	}
}

NS_END