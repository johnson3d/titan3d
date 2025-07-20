#include "DX12Effect.h"
#include "DX12GpuDevice.h"
#include "DX12CommandList.h"
#include "DX12Buffer.h"
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

	void DX12ShaderSignatureBuilder::Build(std::map<VNameString, AutoRef<FEffectBinder>>& binders)
	{
		mCbvSrvUavNumber = 0;
		mSamplerNumber = 0;

		mCbvSrvUavBinders.clear();
		mSamplerBinders.clear();

		auto SetBinder = [&](FShaderBinder* binder)
			{
				if (binder->Type == EShaderBindType::SBT_Sampler)
				{
					mSamplerBinders.push_back(binder);
				}
				else
				{
					mCbvSrvUavBinders.push_back(binder);
				}
			};

		for (auto& i : binders)
		{
			auto binder = (FShaderBinder*)i.second->GetShaderBinder();
			if (i.second->BindType == EShaderBindType::SBT_Sampler)
			{
				i.second->DescriptorIndex = mSamplerNumber;
				if (binder->IsBindless())
					mSamplerNumber += IBindless::MaxBindless;
				else
					mSamplerNumber += binder->BindCount;
			}
			else
			{
				i.second->DescriptorIndex = mCbvSrvUavNumber;
				if (binder->IsBindless())
					mCbvSrvUavNumber += IBindless::MaxBindless;
				else
					mCbvSrvUavNumber += binder->BindCount;
			}

			binder = (FShaderBinder*)i.second->ASBinder;
			if (binder)
			{
				SetBinder(binder);
				binder->DescriptorIndex = i.second->DescriptorIndex;
			}
			binder = (FShaderBinder*)i.second->MSBinder;
			if (binder)
			{
				SetBinder(binder);
				binder->DescriptorIndex = i.second->DescriptorIndex;
			}
			binder = (FShaderBinder*)i.second->VSBinder;
			if (binder)
			{
				SetBinder(binder);
				binder->DescriptorIndex = i.second->DescriptorIndex;
			}
			binder = (FShaderBinder*)i.second->PSBinder;
			if (binder)
			{
				SetBinder(binder);
				binder->DescriptorIndex = i.second->DescriptorIndex;
			}
		}

		//mCbvSrvUavNumber = (UINT)mCbvSrvUavBinders.size();
		//mSamplerNumber = (UINT)mSamplerBinders.size();
	}
	void DX12ShaderSignatureBuilder::Build(IShaderReflector* reflector)
	{
		//mCbvSrvUavNumber = (int)(reflector->CBuffers.size() + reflector->Srvs.size() + reflector->Uavs.size());
		//mSamplerNumber = (int)reflector->Samplers.size();

		mCbvSrvUavBinders.clear();
		mSamplerBinders.clear();

		mCbvSrvUavNumber = 0;
		mSamplerNumber = 0;
		for (auto& i : reflector->CBuffers)
		{
			auto binder = i.UnsafeConvertTo<FShaderBinder>();
			binder->DescriptorIndex = mCbvSrvUavNumber;
			if (binder->IsBindless())
				mCbvSrvUavNumber += IBindless::MaxBindless;
			else
				mCbvSrvUavNumber += binder->BindCount;
			mCbvSrvUavBinders.push_back(i);
		}
		for (auto& i : reflector->Srvs)
		{
			auto binder = i.UnsafeConvertTo<FShaderBinder>();
			binder->DescriptorIndex = mCbvSrvUavNumber;
			if (binder->IsBindless())
				mCbvSrvUavNumber += IBindless::MaxBindless;
			else
				mCbvSrvUavNumber += binder->BindCount;
			mCbvSrvUavBinders.push_back(i);
		}
		for (auto& i : reflector->Uavs)
		{
			auto binder = i.UnsafeConvertTo<FShaderBinder>();
			binder->DescriptorIndex = mCbvSrvUavNumber;
			if (binder->IsBindless())
				mCbvSrvUavNumber += IBindless::MaxBindless;
			else
				mCbvSrvUavNumber += binder->BindCount;
			mCbvSrvUavBinders.push_back(i);
		}
		for (auto& i : reflector->Samplers)
		{
			auto binder = i.UnsafeConvertTo<FShaderBinder>();
			binder->DescriptorIndex = mSamplerNumber;
			if (binder->IsBindless())
				mSamplerNumber += IBindless::MaxBindless;
			else
				mSamplerNumber += binder->BindCount;
			mSamplerBinders.push_back(i);
		}

		//mCbvSrvUavNumber = (UINT)mCbvSrvUavBinders.size();
		//mSamplerNumber = (UINT)mSamplerBinders.size();
	}
	bool DX12ShaderSignatureBuilder::CreateHeap(DX12GpuDevice* device, AutoRef<DX12HeapHolder>& OutCbvSrvUavHeap, AutoRef<DX12HeapHolder>& OutSamplerHeap)
	{
		//we must allocate a new heap for every frame
		//When gpu execute commandlist, CopyDescriptorHeap will parrallel set view to heap
		bool created = false;
		if (mCbvSrvUavNumber > 0)
		{
			//if (OutCbvSrvUavHeap == nullptr || OutCbvSrvUavHeap->NumOfDescriptor != mCbvSrvUavNumber)
			{
				OutCbvSrvUavHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device,
					mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV));

				created = true;
			}
		}
		else
		{
			OutCbvSrvUavHeap = nullptr;
		}
		if (mSamplerNumber > 0)
		{
			//if (OutSamplerHeap == nullptr || OutSamplerHeap->NumOfDescriptor != mSamplerNumber)
			{
				OutSamplerHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device,
					mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER));
				created = true;
			}
		}
		else
		{
			OutSamplerHeap = nullptr;
		}
		return created;
	}
	AutoRef<ID3D12RootSignature> DX12ShaderSignatureBuilder::CreateSignature(DX12GpuDevice* device, D3D12_ROOT_SIGNATURE_FLAGS flags,
		const std::vector<VNameString>* roots, IShaderReflector* pOutReflector,
		std::vector<FSignatureBinder>& OutCbvSrvUav, std::vector<FSignatureBinder>& OutSampler)
	{
		typedef D3D12_ROOT_PARAMETER1 RootParameterType;

		struct DiscriptorRangeType
		{
			D3D12_DESCRIPTOR_RANGE1 Range{};
			FShaderBinder* Binder = nullptr;
		};

		std::vector<RootParameterType>	dxRootParameters;
		std::vector<DiscriptorRangeType> dxCbvSrvUavRanges;
		std::vector<DiscriptorRangeType> dxSamplerRanges;

		std::vector<const FShaderBinder*>	CbvSrvUavBinders;
		std::vector<const FShaderBinder*>	SamplerBinders;

		OutCbvSrvUav.clear();
		OutSampler.clear();

		D3D12_DESCRIPTOR_RANGE_FLAGS rgFlags = D3D12_DESCRIPTOR_RANGE_FLAG_DESCRIPTORS_VOLATILE;

		auto SetBinder = [&](FShaderBinder* binder)->void
			{
				switch (binder->Type)
				{
					case EShaderBindType::SBT_Sampler:
					{
						SamplerBinders.push_back(binder);
						DiscriptorRangeType rg{};
						rg.Range.Flags = rgFlags;
						rg.Range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER;
						rg.Range.NumDescriptors = binder->IsBindless() ? IBindless::MaxBindless : 1;
						rg.Range.BaseShaderRegister = binder->Slot;
						rg.Range.RegisterSpace = binder->Space;
						rg.Range.OffsetInDescriptorsFromTableStart = 0;
						rg.Binder = binder;
						dxSamplerRanges.push_back(rg);

						if (pOutReflector)
							pOutReflector->Samplers.push_back(binder);
					}
					break;
					case EShaderBindType::SBT_CBV:
					{
						CbvSrvUavBinders.push_back(binder);
						DiscriptorRangeType rg{};
						rg.Range.Flags = rgFlags;
						rg.Range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_CBV;
						rg.Range.NumDescriptors = binder->IsBindless() ? IBindless::MaxBindless : 1;
						rg.Range.BaseShaderRegister = binder->Slot;
						rg.Range.RegisterSpace = binder->Space;
						rg.Range.OffsetInDescriptorsFromTableStart = 0;
						rg.Binder = binder;
						dxCbvSrvUavRanges.push_back(rg);

						if (pOutReflector)
							pOutReflector->CBuffers.push_back(binder);
					}
					break;
					case EShaderBindType::SBT_SRV:
					{
						CbvSrvUavBinders.push_back(binder);
						DiscriptorRangeType rg{};
						rg.Range.Flags = rgFlags;
						rg.Range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_SRV;
						rg.Range.NumDescriptors = binder->IsBindless() ? IBindless::MaxBindless : 1;
						rg.Range.BaseShaderRegister = binder->Slot;
						rg.Range.RegisterSpace = binder->Space;
						rg.Range.OffsetInDescriptorsFromTableStart = 0;
						rg.Binder = binder;
						dxCbvSrvUavRanges.push_back(rg);

						if (pOutReflector)
							pOutReflector->Srvs.push_back(binder);
					}
					break;
					case EShaderBindType::SBT_UAV:
					{
						CbvSrvUavBinders.push_back(binder);
						DiscriptorRangeType rg{};
						rg.Range.Flags = rgFlags;
						rg.Range.RangeType = D3D12_DESCRIPTOR_RANGE_TYPE_UAV;
						rg.Range.NumDescriptors = binder->IsBindless() ? IBindless::MaxBindless : 1;
						rg.Range.BaseShaderRegister = binder->Slot;
						rg.Range.RegisterSpace = binder->Space;
						rg.Range.OffsetInDescriptorsFromTableStart = 0;
						rg.Binder = binder;
						dxCbvSrvUavRanges.push_back(rg);

						if (pOutReflector)
							pOutReflector->Uavs.push_back(binder);
					}
					break;
					default:
						break;
				}
			};
		if (roots)
		{
			for (auto& i : *roots)
			{
				auto binder = (FShaderBinder*)FindBinder(i);
				if (binder == nullptr)
				{
					VFX_LTRACE(ELTT_Warning, "CreateSignature: Root[%s] not found\r\n", i.c_str());
					continue;
				}
				SetBinder(binder);
			}
		}
		else
		{
			for (auto& i : mCbvSrvUavBinders)
			{
				auto binder = i.UnsafeConvertTo<FShaderBinder>();
				SetBinder(binder);
			}
			for (auto& i : mSamplerBinders)
			{
				auto binder = i.UnsafeConvertTo<FShaderBinder>();
				SetBinder(binder);
			}
		}

		for (size_t i = 0; i < dxCbvSrvUavRanges.size(); i++)
		{
			RootParameterType rp{};
			rp.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
			switch (dxCbvSrvUavRanges[i].Binder->ShaderStage)
			{
				case EShaderType::SDT_VertexShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_VERTEX;
				}
				break;
				case EShaderType::SDT_PixelShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;
				}
				break;
				case EShaderType::SDT_ComputeShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
				}
				break;
				case EShaderType::SDT_AmplificationShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_AMPLIFICATION;
				}
				break;
				case EShaderType::SDT_MeshShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_MESH;
				}
				break;
				default:
					ASSERT(false);
					break;
			}
			//rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
			rp.DescriptorTable.NumDescriptorRanges = 1;
			rp.DescriptorTable.pDescriptorRanges = &dxCbvSrvUavRanges[i].Range;

			FSignatureBinder sb{};
			sb.Binder = (FShaderBinder*)CbvSrvUavBinders[i];
			sb.RootIndex = (int)dxRootParameters.size();
			sb.DescriptorStart = sb.Binder->DescriptorIndex;
			sb.DescriptorNum = sb.Binder->BindCount;
			OutCbvSrvUav.push_back(sb);
			dxRootParameters.push_back(rp);
		}
		for (size_t i = 0; i < dxSamplerRanges.size(); i++)
		{
			RootParameterType rp{};
			rp.ParameterType = D3D12_ROOT_PARAMETER_TYPE_DESCRIPTOR_TABLE;
			switch (dxSamplerRanges[i].Binder->ShaderStage)
			{
				case EShaderType::SDT_VertexShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_VERTEX;
				}
				break;
				case EShaderType::SDT_PixelShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;
				}
				break;
				case EShaderType::SDT_ComputeShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
				}
				break;
				case EShaderType::SDT_AmplificationShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_AMPLIFICATION;
				}
				break;
				case EShaderType::SDT_MeshShader:
				{
					rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_MESH;
				}
				break;
				default:
					ASSERT(false);
					break;
			}
			//rp.ShaderVisibility = D3D12_SHADER_VISIBILITY_ALL;
			rp.DescriptorTable.NumDescriptorRanges = 1;
			rp.DescriptorTable.pDescriptorRanges = &dxSamplerRanges[i].Range;

			FSignatureBinder sb{};
			sb.Binder = (FShaderBinder*)SamplerBinders[i];
			sb.RootIndex = (int)dxRootParameters.size();
			sb.DescriptorStart = sb.Binder->DescriptorIndex;
			sb.DescriptorNum = sb.Binder->BindCount;
			OutSampler.push_back(sb);
			dxRootParameters.push_back(rp);
		}

		if (true)
		{
			D3D12_VERSIONED_ROOT_SIGNATURE_DESC sigDesc{};
			sigDesc.Version = D3D_ROOT_SIGNATURE_VERSION_1_1;
			sigDesc.Desc_1_1.NumParameters = (UINT)dxRootParameters.size();
			if (sigDesc.Desc_1_1.NumParameters > 0)
				sigDesc.Desc_1_1.pParameters = &dxRootParameters[0];
			sigDesc.Desc_1_1.NumStaticSamplers = 0;
			sigDesc.Desc_1_1.pStaticSamplers = nullptr;
			sigDesc.Desc_1_1.Flags = flags;// D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

			ID3DBlob* serializedRootSig = nullptr;
			ID3DBlob* errorBlob = nullptr;
			HRESULT hr = D3D12SerializeVersionedRootSignature(&sigDesc, &serializedRootSig, &errorBlob);
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
			return MakeWeakRef(pRootSig);
		}
		else
		{
			D3D12_ROOT_SIGNATURE_DESC sigDesc{};
			sigDesc.NumParameters = (UINT)dxRootParameters.size();
			/*if (sigDesc.NumParameters > 0)
				sigDesc.pParameters = &dxRootParameters[0];*/
			sigDesc.NumStaticSamplers = 0;
			sigDesc.pStaticSamplers = nullptr;
			sigDesc.Flags = flags;// D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

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
			return MakeWeakRef(pRootSig);
		}
	}

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

		mSignatureBuilder.Build(mBinders);

		//D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT
		mSignature = mSignatureBuilder.CreateSignature(mDeviceRef.GetPtr(), D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT, nullptr, nullptr, mCbvSrvUavBinders, mSamplerBinders);
		
		ASSERT(mSignature);
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

	inline bool IsInludeBinder(std::vector<VNameString>* pFilters, FShaderBinder* pBinder)
	{
		if(pFilters == nullptr)
			return true;
		for (auto& i : *pFilters)
		{
			if (i == pBinder->Name)
				return true;
		}
		return false;
	}
	
	/// Compute Effect
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
		auto device = ((DX12GpuDevice*)device1);

		mDeviceRef.FromObject(device1);
		mSignatureBuilder.Build(mComputeShader->Reflector);
		mSignature = mSignatureBuilder.CreateSignature(device, D3D12_ROOT_SIGNATURE_FLAG_NONE, nullptr, nullptr, mCbvSrvUavBinders, mSamplerBinders);//D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT

		D3D12_COMPUTE_PIPELINE_STATE_DESC pipeDesc{};
		pipeDesc.pRootSignature = mSignature;
		pipeDesc.CS =
		{
			reinterpret_cast<BYTE*>(&mComputeShader->Desc->DxIL[0]),
			mComputeShader->Desc->DxIL.size()
		};
		pipeDesc.Flags = D3D12_PIPELINE_STATE_FLAG_NONE;
		auto hr = device->mDevice->CreateComputePipelineState(&pipeDesc, IID_PPV_ARGS(mPipelineState.GetAddressOf()));
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


	bool DX12RayTracingEffect::BuildEffect(IGpuDevice* device1)
	{
		DX12GpuDevice* device = (DX12GpuDevice*)device1;
		if (mStateObject != nullptr)
			return false;
		mStateObject = CreateDxrStateObject(device, this);
		
		ASSERT(mStateObject);
		if (mStateObject == nullptr)
			return false;
		mStateObject->QueryInterface(IID_PPV_ARGS(mStateObjectProperties.GetAddressOf()));
		ASSERT(mStateObjectProperties);

		auto rayGenShaderIdentifier = mStateObjectProperties->GetShaderIdentifier(StringHelper::strtowstr(mRayGenName).c_str());
		auto missShaderIdentifier = mStateObjectProperties->GetShaderIdentifier(StringHelper::strtowstr(mMissName).c_str());
		
		UINT shaderIdentifierSize = D3D12_SHADER_IDENTIFIER_SIZE_IN_BYTES;

		{
			FBufferDesc rayGenDesc{};
			rayGenDesc.SetDefault(false);
			rayGenDesc.Type = EBufferType::BFT_NONE;
			rayGenDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			rayGenDesc.Usage = EGpuUsage::USAGE_STAGING;
			rayGenDesc.Size = shaderIdentifierSize;
			rayGenDesc.RowPitch = rayGenDesc.Size;
			rayGenDesc.DepthPitch = rayGenDesc.Size;
			rayGenDesc.InitData = rayGenShaderIdentifier;
			mRayGenShaderTable = MakeWeakRef(new FUploadBuffer(MakeWeakRef(device->CreateBuffer(&rayGenDesc))));
		}
		{
			FBufferDesc missDesc{};
			missDesc.SetDefault(false);
			missDesc.Type = EBufferType::BFT_NONE;
			missDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			missDesc.Usage = EGpuUsage::USAGE_STAGING;
			missDesc.Size = shaderIdentifierSize;
			missDesc.RowPitch = missDesc.Size;
			missDesc.DepthPitch = missDesc.Size;
			missDesc.InitData = missShaderIdentifier;
			mMissShaderTable = MakeWeakRef(new FUploadBuffer(MakeWeakRef(device->CreateBuffer(&missDesc))));
		}

		return true;
	}

	AutoRef<ID3D12StateObject> DX12RayTracingEffect::CreateDxrStateObject(DX12GpuDevice* device, DX12RayTracingEffect* effect)
	{
		CD3DX12_STATE_OBJECT_DESC raytracingPipeline{ D3D12_STATE_OBJECT_TYPE_RAYTRACING_PIPELINE };
		auto lib = raytracingPipeline.CreateSubobject<CD3DX12_DXIL_LIBRARY_SUBOBJECT>();
		D3D12_SHADER_BYTECODE libdxil = CD3DX12_SHADER_BYTECODE(&effect->mShaderLibDesc->DxIL[0], effect->mShaderLibDesc->DxIL.size());
		lib->SetDXILLibrary(&libdxil);
		for (auto& i : this->mFunctions)
		{
			lib->DefineExport(StringHelper::strtowstr(i).c_str());
		}
		//lib->DefineExport(mShaderConfigName.c_str());
		//lib->DefineExport(mPipelineConfigName.c_str());

		if (true)
		{
			auto reflector = effect->GetShaderLibDesc()->DxILReflector;
			mGlobalReflector = MakeWeakRef(new IShaderReflector());

			mSignatureBuilder.Build(reflector);
			
			mGlobalSignature = mSignatureBuilder.CreateSignature(device, D3D12_ROOT_SIGNATURE_FLAG_NONE, &mGlobalSignatures,
				mGlobalReflector, mGlobalCbvSrvUavBinders, mGlobalSamplerBinders);
			if (mGlobalSignature == nullptr)
				return nullptr;
			auto globalRootSignature = raytracingPipeline.CreateSubobject<CD3DX12_GLOBAL_ROOT_SIGNATURE_SUBOBJECT>();
			globalRootSignature->SetRootSignature(mGlobalSignature);

			// Triangle hit group
			for (auto& i : mHitGroups)
			{
				auto group = i.UnsafeConvertTo<DX12HitGroup>();
				auto hitGroup = raytracingPipeline.CreateSubobject<CD3DX12_HIT_GROUP_SUBOBJECT>();
				if (group->AnyHit.AsStdString() != "")
					hitGroup->SetAnyHitShaderImport(StringHelper::strtowstr(group->AnyHit.GetString()).c_str());
				if (group->ClosestHit.AsStdString() != "")
					hitGroup->SetClosestHitShaderImport(StringHelper::strtowstr(group->ClosestHit.GetString()).c_str());
				if (group->Intersection.AsStdString() != "")
					hitGroup->SetIntersectionShaderImport(StringHelper::strtowstr(group->Intersection.GetString()).c_str());
				hitGroup->SetHitGroupExport(StringHelper::strtowstr(group->Name).c_str());
				hitGroup->SetHitGroupType(D3D12_HIT_GROUP_TYPE_TRIANGLES);

				group->LocalReflector = MakeWeakRef(new IShaderReflector());
				group->Dx12Signature = mSignatureBuilder.CreateSignature(device, D3D12_ROOT_SIGNATURE_FLAG_LOCAL_ROOT_SIGNATURE, 
					&group->LocalSignatures, group->LocalReflector,
					group->CbvSrvUavBinders, group->SamplerBinders);
				if (group->Dx12Signature == nullptr)
					return nullptr;
				
				auto localRootSignature = raytracingPipeline.CreateSubobject<CD3DX12_LOCAL_ROOT_SIGNATURE_SUBOBJECT>();
				localRootSignature->SetRootSignature(group->Dx12Signature);

				auto rootSignatureAssociation = raytracingPipeline.CreateSubobject<CD3DX12_SUBOBJECT_TO_EXPORTS_ASSOCIATION_SUBOBJECT>();
				rootSignatureAssociation->SetSubobjectToAssociate(*localRootSignature);
				rootSignatureAssociation->AddExport(StringHelper::strtowstr(group->Name).c_str());
			}
			
			//// Shader config
			//// Defines the maximum sizes in bytes for the ray payload and attribute structure.
			auto shaderConfig = raytracingPipeline.CreateSubobject<CD3DX12_RAYTRACING_SHADER_CONFIG_SUBOBJECT>();
			shaderConfig->Config(mPayloadSize, mAttributeSize);

			auto pipelineConfig = raytracingPipeline.CreateSubobject<CD3DX12_RAYTRACING_PIPELINE_CONFIG_SUBOBJECT>();

			pipelineConfig->Config(mMaxRecursionDepth);
		}

		AutoRef<ID3D12StateObject> result;
		device->mLastDevice->CreateStateObject(raytracingPipeline, IID_PPV_ARGS(result.GetAddressOf()));
		return result;
	}

	bool DX12RayTracingEffect::BuildHitGroup(FHitGroup* group)
	{
		return true;
	}
}

NS_END