#include "DX12Effect.h"
#include "DX12GpuDevice.h"
#include "DX12CommandList.h"
#include "../NxDrawcall.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<ID3D12RootSignature>>
	{
		static void Destroy(AutoRef<ID3D12RootSignature> obj, IGpuDevice* device1)
		{

		}
	};
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<ID3D12CommandSignature>>
	{
		static void Destroy(AutoRef<ID3D12CommandSignature> obj, IGpuDevice* device1)
		{

		}
	};
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<ID3D12PipelineState>>
	{
		static void Destroy(AutoRef<ID3D12PipelineState> obj, IGpuDevice* device1)
		{

		}
	};

	DX12GraphicsEffect::~DX12GraphicsEffect()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		mSignature = nullptr;
		mCmdSignature = nullptr;
	}
	void DX12GraphicsEffect::BuildState(IGpuDevice* device1)
	{
		mDeviceRef.FromObject(device1);
		auto device = ((DX12GpuDevice*)device1);

		for (int i = 0; i < FRootParameter::GraphicsNumber; i++)
		{
			mRootParameters[i].Reset();
		}
		
		for (auto& i : mBinders)
		{
			Push2RootParamters(i.second);
		}

		std::vector<D3D12_ROOT_PARAMETER>	dxRootParameters;
		
		{
			int HeapStartIndex = 0;
			{
				auto& rp = mRootParameters[FRootParameter::VS_Cbv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_VERTEX, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::VS_Srv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_VERTEX, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::VS_Uav];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_VERTEX, dxRootParameters);
			}
			//////////////////////////////////////////////////////////////////////////
			{
				auto& rp = mRootParameters[FRootParameter::PS_Cbv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_PIXEL, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::PS_Srv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_PIXEL, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::PS_Uav];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_PIXEL, dxRootParameters);
			}

			mCbvSrvUavNumber = HeapStartIndex;
		}

		{
			int HeapStartIndex = 0;
			mRootParameters[FRootParameter::VS_Sampler].IsSamplers = true;
			mRootParameters[FRootParameter::PS_Sampler].IsSamplers = true;

			{
				auto& rp = mRootParameters[FRootParameter::VS_Sampler];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_VERTEX, dxRootParameters);
			}

			{	
				auto& rp = mRootParameters[FRootParameter::PS_Sampler];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_PIXEL, dxRootParameters);
			}

			mSamplerNumber = HeapStartIndex;
		}

		D3D12_ROOT_SIGNATURE_DESC sigDesc{};
		sigDesc.NumParameters = (UINT)dxRootParameters.size();
		if (sigDesc.NumParameters > 0)
			sigDesc.pParameters = &dxRootParameters[0];
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
		hr = device->mDevice->CreateRootSignature(0,
			serializedRootSig->GetBufferPointer(),
			bfSize,
			IID_PPV_ARGS(mSignature.GetAddressOf()));

		ASSERT(hr == S_OK);
	}
	void DX12GraphicsEffect::Push2RootParamters(FEffectBinder* binder)
	{
		auto pVSBinder = (FShaderBinder*)binder->VSBinder;
		auto pPSBinder = (FShaderBinder*)binder->PSBinder;
		switch (binder->BindType)
		{
			case EShaderBindType::SBT_CBuffer:
			{
				mRootParameters[FRootParameter::VS_Cbv].PushShaderBinder(pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
				mRootParameters[FRootParameter::PS_Cbv].PushShaderBinder(pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
			}
			break;
			case EShaderBindType::SBT_SRV:
			{
				mRootParameters[FRootParameter::VS_Srv].PushShaderBinder(pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
				mRootParameters[FRootParameter::PS_Srv].PushShaderBinder(pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
			}
			break;
			case EShaderBindType::SBT_UAV:
			{
				mRootParameters[FRootParameter::VS_Uav].PushShaderBinder(pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
				mRootParameters[FRootParameter::PS_Uav].PushShaderBinder(pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
			}
			break;
			case EShaderBindType::SBT_Sampler:
			{
				mRootParameters[FRootParameter::VS_Sampler].PushShaderBinder(pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
				mRootParameters[FRootParameter::PS_Sampler].PushShaderBinder(pPSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
			}
			break;
		}
	}
	AutoRef<ID3D12CommandSignature> DX12GraphicsEffect::GetIndirectDrawCmdSig(DX12GpuDevice* device, ICommandList* cmdlist)
	{
		if (mCmdSignature != nullptr)
			return mCmdSignature;
		mCmdSignature = device->CmdSigForIndirectDraw;
		mIndirectOffset = offsetof(FIndirectDrawArgument, VertexCountPerInstance);
		return mCmdSignature;
	}
	
	AutoRef<ID3D12CommandSignature> DX12GraphicsEffect::GetIndirectDrawIndexCmdSig(DX12GpuDevice* device, ICommandList* cmdlist)
	{
		if (mCmdSignature != nullptr)
			return mCmdSignature;
		mCmdSignature = device->CmdSigForIndirectDrawIndex;
		mIndirectOffset = offsetof(FIndirectDrawIndexArgument, VertexCountPerInstance);
		/*if (mVSMutiDrawRootIndex == -1)
		{
			
		}
		else
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc[2]{};
			argDesc[0].Type = D3D12_INDIRECT_ARGUMENT_TYPE_CONSTANT;
			argDesc[0].Constant.RootParameterIndex = mVSMutiDrawRootIndex;
			argDesc[0].Constant.DestOffsetIn32BitValues = 0;
			argDesc[0].Constant.Num32BitValuesToSet = 1;
			argDesc[1].Type = D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED;
			desc.ByteStride = sizeof(FIndirectDrawArgument);
			desc.NumArgumentDescs = sizeof(argDesc) / sizeof(D3D12_INDIRECT_ARGUMENT_DESC);
			desc.pArgumentDescs = argDesc;
			auto hr = device->mDevice->CreateCommandSignature(&desc, mSignature, IID_PPV_ARGS(mCmdSignature.GetAddressOf()));
			ASSERT(hr == S_OK);
			mIndirectOffset = 0;
		}*/
		return mCmdSignature;
	}
	void DX12GraphicsEffect::Commit(ICommandList* cmdlist, IGraphicDraw* drawcall)
	{
		
	}

	/// Comput Effect
	void DX12ComputeEffect::Push2Root(FShaderBinder* binder)
	{
		switch (binder->Type)
		{
			case EShaderBindType::SBT_CBuffer:
			{
				mRootParameters[FRootParameter::CS_Cbv].PushShaderBinder(binder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
			}
			break;
			case EShaderBindType::SBT_SRV:
			{
				mRootParameters[FRootParameter::CS_Srv].PushShaderBinder(binder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
			}
			break;
			case EShaderBindType::SBT_UAV:
			{
				mRootParameters[FRootParameter::CS_Uav].PushShaderBinder(binder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
			}
			break;
			case EShaderBindType::SBT_Sampler:
			{
				mRootParameters[FRootParameter::CS_Sampler].PushShaderBinder(binder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
			}
			break;
		}
	}
	DX12ComputeEffect::~DX12ComputeEffect()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		mSignature = nullptr;
		mCmdSignature = nullptr;
	}
	void DX12ComputeEffect::BuildState(IGpuDevice* device1)
	{
		mDeviceRef.FromObject(device1);
		auto device = ((DX12GpuDevice*)device1);

		std::vector<D3D12_ROOT_PARAMETER>	dxRootParameters;
		for (auto& i : mComputeShader->Reflector->CBuffers)
		{
			mRootParameters[FRootParameter::CS_Cbv].PushShaderBinder(i, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
		}
		for (auto& i : mComputeShader->Reflector->Srvs)
		{
			mRootParameters[FRootParameter::CS_Srv].PushShaderBinder(i, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
		}
		for (auto& i : mComputeShader->Reflector->Uavs)
		{
			mRootParameters[FRootParameter::CS_Uav].PushShaderBinder(i, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
		}
		for (auto& i : mComputeShader->Reflector->Samplers)
		{
			mRootParameters[FRootParameter::CS_Sampler].PushShaderBinder(i, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
		}

		{
			int HeapStartIndex = 0;
			{
				auto& rp = mRootParameters[FRootParameter::CS_Cbv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_ALL, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::CS_Srv];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_ALL, dxRootParameters);
			}

			{
				auto& rp = mRootParameters[FRootParameter::CS_Uav];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_ALL, dxRootParameters);
			}

			mCbvSrvUavNumber = HeapStartIndex;
		}

		{
			int HeapStartIndex = 0;
			mRootParameters[FRootParameter::CS_Sampler].IsSamplers = true;

			{
				auto& rp = mRootParameters[FRootParameter::CS_Sampler];
				rp.BuildDX12RootParameters(HeapStartIndex, D3D12_SHADER_VISIBILITY_ALL, dxRootParameters);
			}

			mSamplerNumber = HeapStartIndex;
		}

		D3D12_ROOT_SIGNATURE_DESC sigDesc{};
		sigDesc.NumParameters = (UINT)dxRootParameters.size();
		if (sigDesc.NumParameters > 0)
			sigDesc.pParameters = &dxRootParameters[0];
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
		device->mDevice->CreateRootSignature(0,
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
		hr = device->mDevice->CreateComputePipelineState(&pipeDesc, IID_PPV_ARGS(mPipelineState.GetAddressOf()));
		ASSERT(hr == S_OK);
	}
	
	void DX12ComputeEffect::Commit(ICommandList* cmdlist)
	{
		/*auto dx12Cmd = (DX12CommandList*)cmdlist;
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
		}*/
	}

	AutoRef<ID3D12CommandSignature> DX12ComputeEffect::GetIndirectDispatchCmdSig(DX12GpuDevice* device, ICommandList* cmdlist)
	{
		if (mCmdSignature != nullptr)
			return mCmdSignature;
		mCmdSignature = device->CmdSigForIndirectDispatch;
		mIndirectOffset = offsetof(FIndirectDispatchArgument, X);
		/*if (mCSMutiDrawRootIndex == -1)
		{
			mCmdSignature = device->CmdSigForIndirectDispatch;
			mIndirectOffset = offsetof(FIndirectDispatchArgument, X);
		}
		else
		{
			D3D12_COMMAND_SIGNATURE_DESC desc{};
			D3D12_INDIRECT_ARGUMENT_DESC argDesc[2]{};
			argDesc[0].Type = D3D12_INDIRECT_ARGUMENT_TYPE_CONSTANT;
			argDesc[0].Constant.RootParameterIndex = mCSMutiDrawRootIndex;
			argDesc[0].Constant.DestOffsetIn32BitValues = 0;
			argDesc[0].Constant.Num32BitValuesToSet = 1;
			argDesc[1].Type = D3D12_INDIRECT_ARGUMENT_TYPE_DISPATCH;
			desc.ByteStride = sizeof(FIndirectDispatchArgument);
			desc.NumArgumentDescs = sizeof(argDesc) / sizeof(D3D12_INDIRECT_ARGUMENT_DESC);
			desc.pArgumentDescs = argDesc;
			auto hr = device->mDevice->CreateCommandSignature(&desc, mSignature, IID_PPV_ARGS(mCmdSignature.GetAddressOf()));
			ASSERT(hr == S_OK);
			mIndirectOffset = 0;
		}*/
		return mCmdSignature;
	}
}

NS_END