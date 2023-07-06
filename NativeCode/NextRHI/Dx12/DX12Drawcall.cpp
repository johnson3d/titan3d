#include "DX12Drawcall.h"
#include "DX12CommandList.h"
#include "DX12Buffer.h"
#include "DX12GpuState.h"
#include "DX12FrameBuffers.h"
#include "DX12GpuDevice.h"
#include "../NxGeomMesh.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	
	DX12GraphicDraw::DX12GraphicDraw()
	{

	}
	DX12GraphicDraw::~DX12GraphicDraw()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (mCbvSrvUavHeap != nullptr)
		{
			device->DelayDestroy(mCbvSrvUavHeap);
			mCbvSrvUavHeap = nullptr;
		}
		if (mSamplerHeap != nullptr)
		{
			device->DelayDestroy(mSamplerHeap);
			mSamplerHeap = nullptr;
		}
	}
	void DX12GraphicDraw::OnGpuDrawStateUpdated()
	{
		IsDirty = true;

	}
	void DX12GraphicDraw::OnBindResource(const FEffectBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void DX12GraphicDraw::BindResourceToDescriptSets(DX12GpuDevice* device, const FEffectBinder* binder, IGpuResource* resource)
	{
		DX12DescriptorSetPagedObject* handle = nullptr;
		if (resource != nullptr)
			handle = (DX12DescriptorSetPagedObject*)resource->GetHWBuffer();

		if (handle == nullptr)
			return;

		if (binder->BindType == EShaderBindType::SBT_Sampler)
		{
			ASSERT(mSamplerHeap != nullptr);
			if (binder->VSBinder != nullptr)
			{
				device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			}
			if (binder->PSBinder != nullptr)
			{
				device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->GetCpuAddress(binder->PSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			}
		}
		else
		{
			ASSERT(mCbvSrvUavHeap != nullptr);
			if (binder->VSBinder != nullptr)
			{
				if (binder->Name == "cbPerGrassType")
				{
					int xxx = 0;
				}
				/*device->mDevice->CopyDescriptorsSimple(1, mSrvTable->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					device->mNullCBV_SRV_UAV->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);*/
				device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
			if (binder->PSBinder != nullptr)
			{
				device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->GetCpuAddress(binder->PSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
		}
	}
	void DX12GraphicDraw::BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12GraphicsEffect* effect)
	{
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap != nullptr)
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap->RealObject;
		if (mSamplerHeap != nullptr)
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap->RealObject;

		dx12Cmd->mContext->SetGraphicsRootSignature(effect->mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		for (int i = 0; i < FRootParameter::GraphicsNumber; i++)
		{
			if (effect->mRootParameters[i].IsValidRoot())
			{
				if(effect->mRootParameters[i].IsSamplers)
					dx12Cmd->mContext->SetGraphicsRootDescriptorTable(effect->mRootParameters[i].RootIndex, mSamplerHeap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				else
					dx12Cmd->mContext->SetGraphicsRootDescriptorTable(effect->mRootParameters[i].RootIndex, mCbvSrvUavHeap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
			}
		}
	}
	void DX12GraphicDraw::RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		auto effect = this->ShaderEffect.UnsafeConvertTo<DX12GraphicsEffect>();
		if (IsDirty == false)
		{
			UINT finger = 0;
			for (auto& i : BindResources)
			{
				if (i.second != nullptr)
					finger += i.second->GetFingerPrint();
			}
			if (finger == FingerPrient)
			{
				BindDescriptors(device, dx12Cmd, effect);

				/*for (auto& i : BindResources)
				{
					BindResourceToDescriptSets(device, i.first, i.second);
				}*/

				return;
			}

			FingerPrient = finger;
		}

		IsDirty = false;

		if (effect->mCbvSrvUavNumber > 0)
		{
			if (mCbvSrvUavHeap != nullptr)
				device->DelayDestroy(mCbvSrvUavHeap);
			mCbvSrvUavHeap = device->mDescriptorSetAllocator->AllocDescriptorSet(device, effect->mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		if (effect->mSamplerNumber > 0)
		{
			if (mSamplerHeap != nullptr)
				device->DelayDestroy(mSamplerHeap);
			mSamplerHeap = device->mDescriptorSetAllocator->AllocDescriptorSet(device, effect->mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}

		auto multiDrawIndex = this->ShaderEffect.UnsafeConvertTo<DX12GraphicsEffect>()->mVSMutiDrawRootIndex;
		if (multiDrawIndex != -1)
		{
			static VNameString multiDrawName = VNameString("cbForMultiDraw");
			for (auto& i : BindResources)
			{
				if (i.first->Name == multiDrawName)
				{
					dx12Cmd->mContext->SetGraphicsRootConstantBufferView(multiDrawIndex, i.second.UnsafeConvertTo<DX12CbView>()->GetBufferVirtualAddress());
				}
				else
				{
					BindResourceToDescriptSets(device, i.first, i.second);
				}
			}
		}
		else
		{
			for (auto& i : BindResources)
			{
				BindResourceToDescriptSets(device, i.first, i.second);
			}
		}

		BindDescriptors(device, dx12Cmd, effect);
	}
	void DX12GraphicDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		auto dx12Cmd = (DX12CommandList*)cmdlist;
		//Mesh->Commit(cmdlist);
		D3D12_VERTEX_BUFFER_VIEW dxVBs[VST_Number]{};
		for (int i = 0; i < VST_Number; i++)
		{
			auto vbv = Mesh->VertexArray->VertexBuffers[i];
			if (vbv != nullptr)
			{
				auto vb = vbv->Buffer.UnsafeConvertTo<DX12Buffer>();
				dxVBs[i].BufferLocation = vb->GetGPUVirtualAddress();
				dxVBs[i].StrideInBytes = vb->Desc.StructureStride;
				dxVBs[i].SizeInBytes = vb->Desc.Size;
			}
		}
		dx12Cmd->mContext->IASetVertexBuffers(0, VST_Number, dxVBs);
		dx12Cmd->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);

		if (AttachVB != nullptr)
		{
			AttachVB->Commit(cmdlist);
		}

		auto device = (DX12GpuDevice*)cmdlist->GetGpuDevice();
		UpdateGpuDrawState(device, cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
		/*if (GpuDrawState == nullptr)
		{
			UpdateGpuDrawState(cmdlist->mDevice, cmdlist, );
			if (GpuDrawState == nullptr)
				return;
		}*/
		cmdlist->SetGraphicsPipeline(GpuDrawState);
		
		auto effect = (DX12GraphicsEffect*)GetGraphicsEffect();
		
		//effect->Commit(cmdlist, this);
		{
			RebuildDescriptorSets(device, dx12Cmd);
		}
		
		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					effect->BindCBV(cmdlist, i.first, (ICbView*)t);
					if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->mRefBuffers.push_back(t);
					}
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
					if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->mRefBuffers.push_back(t);
					}
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second;
					effect->BindUav(cmdlist, i.first, (IUaView*)t);
					if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->mRefBuffers.push_back(t);
					}
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second;
					effect->BindSampler(cmdlist, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		ASSERT(pDrawDesc);
		if (IndirectDrawArgsBuffer)
		{
			auto effect = this->ShaderEffect.UnsafeConvertTo<DX12GraphicsEffect>();
			dx12Cmd->mCurrentCmdSig = effect->GetIndirectDrawIndexCmdSig(device, dx12Cmd);
			dx12Cmd->mCurrentIndirectOffset = effect->mIndirectOffset;
			cmdlist->IndirectDrawIndexed(pDrawDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
			dx12Cmd->mCurrentCmdSig = nullptr;
			dx12Cmd->mCurrentIndirectOffset = 0;
		}
		else
		{
			if (pDrawDesc->IsIndexDraw())
			{
				cmdlist->DrawIndexed(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->StartIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
			else
			{
				cmdlist->Draw(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
		}
	}

	DX12ComputeDraw::DX12ComputeDraw()
	{

	}
	DX12ComputeDraw::~DX12ComputeDraw()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (mCbvSrvUavHeap != nullptr)
		{
			device->DelayDestroy(mCbvSrvUavHeap);
			mCbvSrvUavHeap = nullptr;
		}
		if (mSamplerHeap != nullptr)
		{
			device->DelayDestroy(mSamplerHeap);
			mSamplerHeap = nullptr;
		}
	}
	void DX12ComputeDraw::OnBindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void DX12ComputeDraw::BindResourceToDescriptSets(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource)
	{
		auto handle = (DX12DescriptorSetPagedObject*)resource->GetHWBuffer();
		if (binder->Type == EShaderBindType::SBT_Sampler)
		{
			ASSERT(mSamplerHeap != nullptr);
			device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->GetCpuAddress(binder->DescriptorIndex),
				handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}
		else
		{
			ASSERT(mCbvSrvUavHeap != nullptr);
			device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->GetCpuAddress(binder->DescriptorIndex),
				handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
	}
	void DX12ComputeDraw::BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12ComputeEffect* effect)
	{
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap != nullptr)
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap->RealObject;
		if (mSamplerHeap != nullptr)
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap->RealObject;

		dx12Cmd->mContext->SetComputeRootSignature(effect->mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		for (int i = 0; i < FRootParameter::ComputeNumber; i++)
		{
			if (effect->mRootParameters[i].IsValidRoot())
			{
				if (effect->mRootParameters[i].IsSamplers)
				{
					ASSERT(mSamplerHeap);
					dx12Cmd->mContext->SetComputeRootDescriptorTable(effect->mRootParameters[i].RootIndex, mSamplerHeap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				}
				else
				{
					ASSERT(mCbvSrvUavHeap);
					dx12Cmd->mContext->SetComputeRootDescriptorTable(effect->mRootParameters[i].RootIndex, mCbvSrvUavHeap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				}
			}
		}
	}
	void DX12ComputeDraw::RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		auto effect = this->mEffect.UnsafeConvertTo<DX12ComputeEffect>();
		if (IsDirty == false)
		{
			UINT finger = 0;
			for (auto& i : BindResources)
			{
				finger += i.second->GetFingerPrint();
			}
			if (finger == FingerPrient)
			{
				BindDescriptors(device, dx12Cmd, effect);

				/*for (auto& i : BindResources)
				{
					BindResourceToDescriptSets(device, i.first, i.second);
				}*/

				return;
			}

			FingerPrient = finger;
		}

		IsDirty = false;

		if (effect->mCbvSrvUavNumber > 0)
		{
			if (mCbvSrvUavHeap != nullptr)
				device->DelayDestroy(mCbvSrvUavHeap);
			mCbvSrvUavHeap = device->mDescriptorSetAllocator->AllocDescriptorSet(device, effect->mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			ASSERT(mCbvSrvUavHeap);
		}
		if (effect->mSamplerNumber > 0)
		{
			if (mSamplerHeap != nullptr)
				device->DelayDestroy(mSamplerHeap);
			mSamplerHeap = device->mDescriptorSetAllocator->AllocDescriptorSet(device, effect->mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			ASSERT(mSamplerHeap);
		}

		BindDescriptors(device, dx12Cmd, effect);

		auto multiDrawIndex = ((DX12ComputeEffect*)GetComputeEffect())->mCSMutiDrawRootIndex;
		if (multiDrawIndex != -1)
		{
			static VNameString multiDrawName = VNameString("cbForMultiDraw");
			for (auto& i : BindResources)
			{
				if (i.first->Name == multiDrawName)
				{
					dx12Cmd->mContext->SetGraphicsRootConstantBufferView(multiDrawIndex, i.second.UnsafeConvertTo<DX12CbView>()->GetBufferVirtualAddress());
				}
				else
				{
					BindResourceToDescriptSets(device, i.first, i.second);
				}
			}
		}
		else
		{
			for (auto& i : BindResources)
			{
				BindResourceToDescriptSets(device, i.first, i.second);
			}
		}
	}
	void DX12ComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (mEffect == nullptr)
			return;
		auto device = (DX12GpuDevice*)cmdlist->GetGpuDevice();
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		auto effect = mEffect.UnsafeConvertTo<DX12ComputeEffect>();

		dx12Cmd->mContext->SetPipelineState(effect->mPipelineState);
		dx12Cmd->mContext->SetComputeRootSignature(effect->mSignature);
		//effect->Commit(cmdlist, this);
		{
			RebuildDescriptorSets(device, dx12Cmd);
		}
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
					if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->mRefBuffers.push_back(t);
					}
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
					if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->mRefBuffers.push_back(t);
					}
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetUav(EShaderType::SDT_ComputeShader, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSampler(EShaderType::SDT_ComputeShader, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		if (IndirectDispatchArgsBuffer != nullptr)
		{
			auto effect = this->mEffect.UnsafeConvertTo<DX12ComputeEffect>();
			dx12Cmd->mCurrentCmdSig = effect->GetIndirectDispatchCmdSig(device, dx12Cmd);
			dx12Cmd->mCurrentIndirectOffset = effect->mIndirectOffset;
			cmdlist->IndirectDispatch(IndirectDispatchArgsBuffer, 0);
			dx12Cmd->mCurrentCmdSig = nullptr;
			dx12Cmd->mCurrentIndirectOffset = 0;
		}
		else
		{
			cmdlist->Dispatch(mDispatchX, mDispatchY, mDispatchZ);
		}
	}
}

NS_END
