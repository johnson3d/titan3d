#include "DX12Effect.h"
#include "DX12GpuDevice.h"
#include "DX12CommandList.h"
#include "../NxDrawcall.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void FillRange(std::vector<D3D12_DESCRIPTOR_RANGE>* pOutRanges, FShaderBinder* pVSBinder, FShaderBinder* pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE type)
	{
		if (pVSBinder != nullptr && pPSBinder == nullptr)
		{
			D3D12_DESCRIPTOR_RANGE rangeVS{};
			rangeVS.RangeType = type;
			rangeVS.NumDescriptors = 1;
			rangeVS.BaseShaderRegister = pVSBinder->Slot;
			rangeVS.RegisterSpace = pVSBinder->Space;
			rangeVS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			pVSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pOutRanges->push_back(rangeVS);
		}
		else if (pVSBinder == nullptr && pPSBinder != nullptr)
		{
			D3D12_DESCRIPTOR_RANGE rangePS{};
			rangePS.RangeType = type;
			rangePS.NumDescriptors = 1;
			rangePS.BaseShaderRegister = pPSBinder->Slot;
			rangePS.RegisterSpace = pPSBinder->Space;
			rangePS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			pPSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pOutRanges->push_back(rangePS);
		}
		else if (pVSBinder->Slot == pPSBinder->Slot && pVSBinder->Space == pPSBinder->Space)
		{
			D3D12_DESCRIPTOR_RANGE rangeVSPS{};
			rangeVSPS.RangeType = type;
			rangeVSPS.NumDescriptors = 1;
			rangeVSPS.BaseShaderRegister = pVSBinder->Slot;
			rangeVSPS.RegisterSpace = pVSBinder->Space;
			rangeVSPS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			pVSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pPSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pOutRanges->push_back(rangeVSPS);
		}
		else
		{
			D3D12_DESCRIPTOR_RANGE rangeVS{};
			rangeVS.RangeType = type;
			rangeVS.NumDescriptors = 1;
			rangeVS.BaseShaderRegister = pVSBinder->Slot;
			rangeVS.RegisterSpace = pVSBinder->Space;
			rangeVS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			pVSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pOutRanges->push_back(rangeVS);

			D3D12_DESCRIPTOR_RANGE rangePS{};
			rangePS.RangeType = type;
			rangePS.NumDescriptors = 1;
			rangePS.BaseShaderRegister = pPSBinder->Slot;
			rangePS.RegisterSpace = pPSBinder->Space;
			rangePS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
			pPSBinder->DescriptorIndex = (UINT)pOutRanges->size();
			pOutRanges->push_back(rangePS);
		}
	}
	void FillRange(std::vector<D3D12_DESCRIPTOR_RANGE>* pOutRanges, FShaderBinder* pBinder, D3D12_DESCRIPTOR_RANGE_TYPE type)
	{
		if (pBinder == nullptr)
			return;
		D3D12_DESCRIPTOR_RANGE rangeVS{};
		rangeVS.RangeType = type;
		rangeVS.NumDescriptors = 1;
		rangeVS.BaseShaderRegister = pBinder->Slot;
		rangeVS.RegisterSpace = pBinder->Space;
		rangeVS.OffsetInDescriptorsFromTableStart = D3D12_DESCRIPTOR_RANGE_OFFSET_APPEND;
		pBinder->DescriptorIndex = (UINT)pOutRanges->size();
		pOutRanges->push_back(rangeVS);
	}
	void DX12ShaderEffect::BuildState(IGpuDevice* device)
	{
		std::vector<D3D12_ROOT_PARAMETER>	mRootParameters;
		std::vector<D3D12_DESCRIPTOR_RANGE>	usb;
		std::vector<D3D12_DESCRIPTOR_RANGE>	sampler;
		mRootParameters.clear();
		for (auto& i : mBinders)
		{
			if (i.second->BindType == EShaderBindType::SBT_CBuffer)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_SRV)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_UAV)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_Sampler)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&sampler, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
			}
		}

		for (auto& i : mBinders)
		{
			if (i.second->BindType == EShaderBindType::SBT_CBuffer)
			{
				auto pPSBinder = (FShaderBinder*)i.second->PSBinder;
				FillRange(&usb, pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_SRV)
			{
				auto pPSBinder = (FShaderBinder*)i.second->PSBinder;
				FillRange(&usb, pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_UAV)
			{
				auto pPSBinder = (FShaderBinder*)i.second->PSBinder;
				FillRange(&usb, pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_Sampler)
			{
				auto pPSBinder = (FShaderBinder*)i.second->PSBinder;
				FillRange(&sampler, pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
			}
		}

		D3D12_ROOT_PARAMETER tmp{};
		tmp.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
		
		tmp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;//D3D12_SHADER_VISIBILITY_VERTEX;
		tmp.DescriptorTable.NumDescriptorRanges = (UINT)usb.size();
		mSrvTableSize = tmp.DescriptorTable.NumDescriptorRanges;
		if (usb.size() > 0)
		{
			tmp.DescriptorTable.pDescriptorRanges = &usb[0];
			mSrvTableSizeIndex = (UINT)mRootParameters.size();
			mRootParameters.push_back(tmp);
		}
		else
		{
			mSrvTableSizeIndex = -1;
		}
		tmp.DescriptorTable.NumDescriptorRanges = (UINT)sampler.size();
		mSamplerTableSize = tmp.DescriptorTable.NumDescriptorRanges;
		if (sampler.size() > 0)
		{
			tmp.DescriptorTable.pDescriptorRanges = &sampler[0];
			mSamplerTableSizeIndex = (UINT)mRootParameters.size();
			mRootParameters.push_back(tmp);
		}
		else
		{
			mSamplerTableSizeIndex = -1;
		}
		D3D12_ROOT_SIGNATURE_DESC sigDesc{};
		sigDesc.NumParameters = (UINT)mRootParameters.size();
		sigDesc.pParameters = &mRootParameters[0];
		sigDesc.NumStaticSamplers = 0;
		sigDesc.pStaticSamplers = nullptr;
		sigDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

		ID3DBlob* serializedRootSig = nullptr;
		ID3DBlob* errorBlob = nullptr;
		HRESULT hr = D3D12SerializeRootSignature(&sigDesc, D3D_ROOT_SIGNATURE_VERSION_1, &serializedRootSig, &errorBlob);
		if (errorBlob != nullptr)
		{
			auto pError = (char*)errorBlob->GetBufferPointer();
			VFX_LTRACE(ELTT_Graphics, pError);
			ASSERT(false);
		}

		auto bfSize = serializedRootSig->GetBufferSize();
		ID3D12RootSignature* pRootSig = nullptr;
		((DX12GpuDevice*)device)->mDevice->CreateRootSignature(0,
			serializedRootSig->GetBufferPointer(),
			bfSize,
			IID_PPV_ARGS(&pRootSig));
		mSignature = pRootSig;
	}
	AutoRef<ID3D12CommandSignature> DX12ShaderEffect::GetIndirectDrawIndexCmdSig(ICommandList* cmdlist)
	{
		return nullptr;
		/*if (CmdSigForIndirectDrawIndex == nullptr)
		{
			auto device = (DX12GpuDevice*)cmdlist->mDevice.GetPtr();
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc{};
			argDesc.Type = D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED;
			desc.ByteStride = 20;
			desc.NumArgumentDescs = 1;
			desc.pArgumentDescs = &argDesc;
			device->mDevice->CreateCommandSignature(&desc, nullptr, IID_PPV_ARGS(CmdSigForIndirectDrawIndex.GetAddressOf()));
		}
		return CmdSigForIndirectDrawIndex;*/
	}
	void DX12ShaderEffect::Commit(ICommandList* cmdlist, IGraphicDraw* drawcall)
	{
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		ASSERT(dx12Cmd->mCurrentTableRecycle != nullptr);
		auto device = dx12Cmd->GetDX12Device();

		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		
		dx12Cmd->mContext->SetGraphicsRootSignature(mSignature);
		if (mSrvTableSize > 0)
		{
			dx12Cmd->mCurrentSrvTable = device->mSrvTableHeapManager->Alloc(device->mDevice, mSrvTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentSrvTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentSrvTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentSrvTable = nullptr;
		}

		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mCurrentSamplerTable = device->mSamplerTableHeapManager->Alloc(device->mDevice, mSamplerTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentSamplerTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentSamplerTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentSamplerTable = nullptr;
		}

		dx12Cmd->mContext->SetGraphicsRootSignature(mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);
		
		if (mSrvTableSize > 0)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(mSrvTableSizeIndex, dx12Cmd->mCurrentSrvTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}
		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(mSamplerTableSizeIndex, dx12Cmd->mCurrentSamplerTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}

		/*if (drawcall->IndirectDrawArgsBuffer != nullptr)
		{
			dx12Cmd->mCurrentIndirectDrawIndexSig = GetIndirectDrawIndexCmdSig(cmdlist);
		}
		else
		{
			dx12Cmd->mCurrentIndirectDrawIndexSig = nullptr;
		}*/
	}

	void DX12ComputeEffect::BuildState(IGpuDevice* device)
	{
		std::vector<D3D12_ROOT_PARAMETER>	mRootParameters;
		std::vector<D3D12_DESCRIPTOR_RANGE>	usb;
		std::vector<D3D12_DESCRIPTOR_RANGE>	sampler;
		mRootParameters.clear();
		auto pReflector = mComputeShader->GetReflector();
		for (auto& i : pReflector->CBuffers)
		{
			auto pBinder = i;
			FillRange(&usb, pBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
		}
		for (auto& i : pReflector->Uavs)
		{
			auto pBinder = i;
			FillRange(&usb, pBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
		}
		for (auto& i : pReflector->Srvs)
		{
			auto pBinder = i;
			FillRange(&usb, pBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
		}
		for (auto& i : pReflector->Samplers)
		{
			auto pBinder = i;
			FillRange(&sampler, pBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
		}

		D3D12_ROOT_PARAMETER tmp{};
		tmp.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;

		tmp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;//D3D12_SHADER_VISIBILITY_VERTEX;
		tmp.DescriptorTable.NumDescriptorRanges = (UINT)usb.size();
		mSrvTableSize = tmp.DescriptorTable.NumDescriptorRanges;
		if (usb.size() > 0)
		{
			tmp.DescriptorTable.pDescriptorRanges = &usb[0];
			mSrvTableSizeIndex = (UINT)mRootParameters.size();
			mRootParameters.push_back(tmp);
		}
		else
		{
			mSrvTableSizeIndex = -1;
		}
		tmp.DescriptorTable.NumDescriptorRanges = (UINT)sampler.size();
		mSamplerTableSize = tmp.DescriptorTable.NumDescriptorRanges;
		if (sampler.size() > 0)
		{
			tmp.DescriptorTable.pDescriptorRanges = &sampler[0];
			mSamplerTableSizeIndex = (UINT)mRootParameters.size();
			mRootParameters.push_back(tmp);
		}
		else
		{
			mSamplerTableSizeIndex = -1;
		}
		D3D12_ROOT_SIGNATURE_DESC sigDesc{};
		sigDesc.NumParameters = (UINT)mRootParameters.size();
		sigDesc.pParameters = &mRootParameters[0];
		sigDesc.NumStaticSamplers = 0;
		sigDesc.pStaticSamplers = nullptr;
		sigDesc.Flags = D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

		ID3DBlob* serializedRootSig = nullptr;
		ID3DBlob* errorBlob = nullptr;
		HRESULT hr = D3D12SerializeRootSignature(&sigDesc, D3D_ROOT_SIGNATURE_VERSION_1, &serializedRootSig, &errorBlob);
		if (errorBlob != nullptr)
		{
			auto pError = (char*)errorBlob->GetBufferPointer();
			VFX_LTRACE(ELTT_Graphics, pError);
			ASSERT(false);
		}

		auto bfSize = serializedRootSig->GetBufferSize();
		ID3D12RootSignature* pRootSig = nullptr;
		((DX12GpuDevice*)device)->mDevice->CreateRootSignature(0,
			serializedRootSig->GetBufferPointer(),
			bfSize,
			IID_PPV_ARGS(&pRootSig));
		mSignature = pRootSig;

		D3D12_COMPUTE_PIPELINE_STATE_DESC pipeDesc{};
		pipeDesc.pRootSignature = mSignature;
		pipeDesc.CS =
		{
			reinterpret_cast<BYTE*>(&mComputeShader->Desc->DxIL[0]),
			mComputeShader->Desc->DxIL.size()
		};
		pipeDesc.Flags = D3D12_PIPELINE_STATE_FLAG_NONE;
		((DX12GpuDevice*)device)->mDevice->CreateComputePipelineState(&pipeDesc, IID_PPV_ARGS(mPipelineState.GetAddressOf()));
	}
	
	void DX12ComputeEffect::Commit(ICommandList* cmdlist)
	{
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		ASSERT(dx12Cmd->mCurrentTableRecycle != nullptr);
		auto device = dx12Cmd->GetDX12Device();

		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;

		if (mSrvTableSize > 0)
		{
			dx12Cmd->mCurrentComputeSrvTable = device->mSrvTableHeapManager->Alloc(device->mDevice, mSrvTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentComputeSrvTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentComputeSrvTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentComputeSrvTable = nullptr;
		}

		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mCurrentComputeSamplerTable = device->mSamplerTableHeapManager->Alloc(device->mDevice, mSamplerTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentComputeSamplerTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentComputeSamplerTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentComputeSamplerTable = nullptr;
		}

		dx12Cmd->mContext->SetPipelineState(mPipelineState);
		dx12Cmd->mContext->SetComputeRootSignature(mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		if (mSrvTableSize > 0)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(mSrvTableSizeIndex, dx12Cmd->mCurrentComputeSrvTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}
		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(mSamplerTableSizeIndex, dx12Cmd->mCurrentComputeSamplerTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}
	}
}

NS_END