#include "DX12Drawcall.h"
#include "DX12CommandList.h"
#include "DX12Buffer.h"
#include "DX12GpuState.h"
#include "DX12FrameBuffers.h"
#include "DX12GpuDevice.h"
#include "DX12GeomMesh.h"
#include "../NxGeomMesh.h"
#include "../NxEffect.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	
	DX12GraphicDraw::DX12GraphicDraw()
	{

	}
	DX12GraphicDraw::~DX12GraphicDraw()
	{
		mCbvSrvUavHeap = nullptr;
		mSamplerHeap = nullptr;
	}
	void DX12GraphicDraw::OnGpuDrawStateUpdated()
	{
		IsDirty = true;

	}
	void DX12GraphicDraw::OnBindResource(const FEffectBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void DX12GraphicDraw::BindResourceToHeap(DX12GpuDevice* device, const FEffectBinder* binder, IGpuResource* resource)
	{
		DX12PagedHeap* handle = nullptr;
		if (resource != nullptr)
			handle = (DX12PagedHeap*)resource->GetHWBuffer();

		if (handle == nullptr)
			return;

		if (binder->BindType == EShaderBindType::SBT_Sampler)
		{
			ASSERT(mSamplerHeap != nullptr);
			if (binder->VSBinder != nullptr)
			{
				handle->BindToHeap(device, mSamplerHeap->Heap, binder->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				/*device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->Heap->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);*/
			}
			if (binder->PSBinder != nullptr)
			{
				handle->BindToHeap(device, mSamplerHeap->Heap, binder->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				/*device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->Heap->GetCpuAddress(binder->PSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);*/
			}
		}
		else
		{
			ASSERT(mCbvSrvUavHeap != nullptr);
			if (binder->VSBinder != nullptr)
			{
				/*device->mDevice->CopyDescriptorsSimple(1, mSrvTable->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					device->mNullCBV_SRV_UAV->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);*/
				
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);

				/*device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->Heap->GetCpuAddress(binder->VSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);*/
			}
			if (binder->PSBinder != nullptr)
			{
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				/*device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->Heap->GetCpuAddress(binder->PSBinder->DescriptorIndex),
					handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);*/
			}
		}
	}
	void DX12GraphicDraw::BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12GraphicsEffect* effect)
	{
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap != nullptr)
		{
			dx12Cmd->GetCmdRecorder()->UseResource(mCbvSrvUavHeap);
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap->Heap->RealObject;
		}	
		if (mSamplerHeap != nullptr)
		{
			dx12Cmd->GetCmdRecorder()->UseResource(mSamplerHeap);
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap->Heap->RealObject;
		}

		dx12Cmd->mContext->SetGraphicsRootSignature(effect->mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		for (int i = 0; i < FRootParameter::GraphicsNumber; i++)
		{
			if (effect->mRootParameters[i].IsValidRoot())
			{
				if(effect->mRootParameters[i].IsSamplers)
					dx12Cmd->mContext->SetGraphicsRootDescriptorTable(effect->mRootParameters[i].RootIndex, mSamplerHeap->Heap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				else
					dx12Cmd->mContext->SetGraphicsRootDescriptorTable(effect->mRootParameters[i].RootIndex, mCbvSrvUavHeap->Heap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
			}
		}
	}
	void DX12GraphicDraw::ResetHeap(DX12GpuDevice* device, DX12GraphicsEffect* effect)
	{
		for (auto& b : effect->mBinders)
		{
			switch (b.second->BindType)
			{
				case EShaderBindType::SBT_Sampler:
				{
					auto handle = device->mNullSampler->mView;
					if (b.second->VSBinder != nullptr)
						handle->BindToHeap(device, mSamplerHeap->Heap, b.second->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
					if (b.second->PSBinder != nullptr)
						handle->BindToHeap(device, mSamplerHeap->Heap, b.second->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				}
				break;
				case EShaderBindType::SBT_CBuffer:
				{
					auto handle = device->mNullCBV->mView;
					if (b.second->VSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					if (b.second->PSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				case EShaderBindType::SBT_SRV:
				{
					auto handle = device->mNullSRV->mView;
					if (b.second->VSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					if (b.second->PSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				case EShaderBindType::SBT_UAV:
				{
					auto handle = device->mNullUAV->mView;
					if (b.second->VSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->VSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
					if (b.second->PSBinder != nullptr)
						handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b.second->PSBinder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				default:
					break;
			}
		}
	}
	void DX12GraphicDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
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
			mCbvSrvUavHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV));
		}
		else
		{
			mCbvSrvUavHeap = nullptr;
		}
		if (effect->mSamplerNumber > 0)
		{
			mSamplerHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER));
		}
		else
		{
			mSamplerHeap = nullptr;
		}
		ResetHeap(device, effect);
		
		for (auto& i : BindResources)
		{
			BindResourceToHeap(device, i.first, i.second);
		}

		BindDescriptors(device, dx12Cmd, effect);
	}
	void DX12GraphicDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		AUTO_SAMP("NxRHI.GraphicDraw.Commit");
		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		if (DebugName.empty() == false)
		{
			cmdlist->BeginEvent(DebugName.c_str());
		}

		auto device = (DX12GpuDevice*)cmdlist->GetGpuDevice();
		device->CheckDeviceThread();
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.Geom");
			DX12VertexArray* VAs[2] = { Mesh->VertexArray , AttachVB };
			DX12VertexArray::Commit(dx12Cmd, 2, VAs);

			dx12Cmd->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);
		}
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.UpdateDrawState");
			UpdateGpuDrawState(device, cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
			cmdlist->SetGraphicsPipeline(GpuDrawState);
		}
		
		auto effect = (DX12GraphicsEffect*)GetGraphicsEffect();
		
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.BindResouces");
			BindDescriptorHeaps(device, dx12Cmd);
		
			for (auto& i : BindResources)
			{
				switch (i.first->BindType)
				{
					case SBT_CBuffer:
					{
						IGpuResource* t = i.second;
						effect->BindCBV(cmdlist, i.first, (ICbView*)t);
					}
					break;
					case SBT_SRV:
					{
						auto t = (DX12SrView*)i.second;
						effect->BindSrv(cmdlist, i.first, t);
						//cmdlist->GetCmdRecorder()->UseResource(t->Buffer);
						/*if (bRefResource)
						{
							cmdlist->GetCmdRecorder()->UseResource(t);
						}*/
					}
					break;
					case SBT_UAV:
					{
						IGpuResource* t = i.second;
						effect->BindUav(cmdlist, i.first, (IUaView*)t);
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
		}
		
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.Draw");
			auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
			ASSERT(pDrawDesc);
			if (IndirectDrawArgsBuffer)
			{
				IndirectDrawArgsBuffer->TransitionTo(cmdlist, GRS_UavIndirect);
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
		if (DebugName.empty() == false)
		{
			cmdlist->EndEvent();
		}
	}

	DX12ComputeDraw::DX12ComputeDraw()
	{

	}
	DX12ComputeDraw::~DX12ComputeDraw()
	{
		mCbvSrvUavHeap = nullptr;
		mSamplerHeap = nullptr;
	}
	void DX12ComputeDraw::OnBindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	
	void DX12ComputeDraw::BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource)
	{
		auto handle = (DX12PagedHeap*)resource->GetHWBuffer();
		if (binder->Type == EShaderBindType::SBT_Sampler)
		{
			ASSERT(mSamplerHeap != nullptr);
			handle->BindToHeap(device, mSamplerHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			/*device->mDevice->CopyDescriptorsSimple(1, mSamplerHeap->Heap->GetCpuAddress(binder->DescriptorIndex),
				handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);*/
		}
		else
		{
			ASSERT(mCbvSrvUavHeap != nullptr);
			handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			/*device->mDevice->CopyDescriptorsSimple(1, mCbvSrvUavHeap->Heap->GetCpuAddress(binder->DescriptorIndex),
				handle->GetCpuAddress(0), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);*/
		}
	}
	void DX12ComputeDraw::BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12ComputeEffect* effect)
	{
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap != nullptr)
		{
			dx12Cmd->GetCmdRecorder()->UseResource(mCbvSrvUavHeap);
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap->Heap->RealObject;
		}
			
		if (mSamplerHeap != nullptr)
		{
			dx12Cmd->GetCmdRecorder()->UseResource(mSamplerHeap);
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap->Heap->RealObject;
		}

		dx12Cmd->mContext->SetComputeRootSignature(effect->mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		for (int i = 0; i < FRootParameter::ComputeNumber; i++)
		{
			if (effect->mRootParameters[i].IsValidRoot())
			{
				if (effect->mRootParameters[i].IsSamplers)
				{
					ASSERT(mSamplerHeap);
					dx12Cmd->mContext->SetComputeRootDescriptorTable(effect->mRootParameters[i].RootIndex, mSamplerHeap->Heap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				}
				else
				{
					ASSERT(mCbvSrvUavHeap);
					dx12Cmd->mContext->SetComputeRootDescriptorTable(effect->mRootParameters[i].RootIndex, mCbvSrvUavHeap->Heap->GetGpuAddress(effect->mRootParameters[i].HeapStartIndex));
				}
			}
		}
	}
	void DX12ComputeDraw::ResetHeap(DX12GpuDevice* device, DX12ComputeEffect* effect)
	{
		for (auto& b : effect->mComputeShader->Reflector->CBuffers)
		{
			auto handle = device->mNullCBV->mView;
			handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		for (auto& b : effect->mComputeShader->Reflector->Srvs)
		{
			auto handle = device->mNullSRV->mView;
			handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		for (auto& b : effect->mComputeShader->Reflector->Uavs)
		{
			auto handle = device->mNullUAV->mView;
			handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		for (auto& b : effect->mComputeShader->Reflector->Samplers)
		{
			auto handle = device->mNullSampler->mView;
			handle->BindToHeap(device, mSamplerHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
	}
	void DX12ComputeDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
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
			mCbvSrvUavHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV));
			ASSERT(mCbvSrvUavHeap);
		}
		else
		{
			mCbvSrvUavHeap = nullptr;
		}
		if (effect->mSamplerNumber > 0)
		{
			mSamplerHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER));
			ASSERT(mSamplerHeap);
		}
		else
		{
			mSamplerHeap = nullptr;
		}
		
		ResetHeap(device, effect);
		BindDescriptors(device, dx12Cmd, effect);

		for (auto& i : BindResources)
		{
			BindResourceToHeap(device, i.first, i.second);
		}
	}
	void DX12ComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (mEffect == nullptr)
			return;
		auto device = (DX12GpuDevice*)cmdlist->GetGpuDevice();
		device->CheckDeviceThread();

		auto dx12Cmd = (DX12CommandList*)cmdlist;
		auto effect = mEffect.UnsafeConvertTo<DX12ComputeEffect>();

		if (DebugName.empty() == false)
		{
			cmdlist->BeginEvent(DebugName.c_str());
		}

		dx12Cmd->mContext->SetPipelineState(effect->mPipelineState);
		dx12Cmd->mContext->SetComputeRootSignature(effect->mSignature);
		//effect->Commit(cmdlist, this);
		{
			BindDescriptorHeaps(device, dx12Cmd);
		}
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
					/*if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->UseResource(t);
					}*/
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
					/*if (bRefResource)
					{
						cmdlist->GetCmdRecorder()->UseResource(t);
					}*/
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
			IndirectDispatchArgsBuffer->TransitionTo(cmdlist, GRS_UavIndirect);
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

		if (DebugName.empty() == false)
		{
			cmdlist->EndEvent();
		}
	}
}

NS_END
