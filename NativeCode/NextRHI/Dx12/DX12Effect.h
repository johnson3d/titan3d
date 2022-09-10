#pragma once
#include "../NxEffect.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12ShaderEffect : public IShaderEffect
	{
	public:
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;

		AutoRef<ID3D12CommandSignature> GetIndirectDrawIndexCmdSig(ICommandList* cmdlist);
	public:
		int								mSrvTableSizeIndex;
		int								mSrvTableSize;
		int								mSamplerTableSizeIndex;
		int								mSamplerTableSize;
		AutoRef<ID3D12RootSignature>	mSignature;

		//AutoRef<ID3D12CommandSignature>	CmdSigForIndirectDrawIndex;
	};

	class DX12ComputeEffect : public IComputeEffect
	{
	public:
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;
	public:
		int								mSrvTableSizeIndex;
		int								mSrvTableSize;
		int								mSamplerTableSizeIndex;
		int								mSamplerTableSize;
		AutoRef<ID3D12RootSignature>	mSignature;
		AutoRef<ID3D12PipelineState>	mPipelineState;
	};
}

NS_END
