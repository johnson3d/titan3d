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
	static void ResetHeapToNullByReflector(DX12GpuDevice* device, const IShaderReflector* pReflector,
		AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder> mSamplerHeap)
	{
		if (mCbvSrvUavHeap)
		{
			for (auto& b : pReflector->CBuffers)
			{
				auto handle = device->mNullCBV->mView;
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
			for (auto& b : pReflector->Srvs)
			{
				auto handle = device->mNullSRV->mView;
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
			for (auto& b : pReflector->Uavs)
			{
				auto handle = device->mNullUAV->mView;
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
		}
		if (mSamplerHeap)
		{
			for (auto& b : pReflector->Samplers)
			{
				auto handle = device->mNullSampler->mView;
				handle->BindToHeap(device, mSamplerHeap->Heap, b->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			}
		}
	}

	static void ResetHeapToNullByEffect(DX12GpuDevice* device, DX12GraphicsEffect* effect,
		AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder> mSamplerHeap)
	{
		auto SetBinder = [&](const FShaderBinder* binder)
		{
			switch (binder->Type)
			{
				case EShaderBindType::SBT_Sampler:
				{
					auto handle = device->mNullSampler->mView;
					handle->BindToHeap(device, mSamplerHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				}
				break;
				case EShaderBindType::SBT_CBV:
				{
					auto handle = device->mNullCBV->mView;
					handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				case EShaderBindType::SBT_SRV:
				{
					auto handle = device->mNullSRV->mView;
					handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				case EShaderBindType::SBT_UAV:
				{
					auto handle = device->mNullUAV->mView;
					handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				}
				break;
				default:
					break;
			}
		};
		for (auto& b : effect->mBinders)
		{
			auto binder = b.second;
			if (binder->ASBinder) 
			{
				SetBinder(binder->ASBinder);
			}
			if (binder->MSBinder)
			{
				SetBinder(binder->MSBinder);
			}
			if (binder->VSBinder)
			{
				SetBinder(binder->VSBinder);
			}
			if (binder->PSBinder)
			{
				SetBinder(binder->PSBinder);
			}
		}
	}
	
	static void Bind2Heap(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource,
		AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder>& mSamplerHeap)
	{
		if (resource == nullptr)
			return;
		if (binder->IsBindless())
		{
			if (binder->Type == EShaderBindType::SBT_Sampler)
			{

			}
			else
			{
				/*auto bd = (IBindlessBase*)resource;
				//device->mDevice->CopyDescriptors()
				for (size_t i = 0; i < bd->mResources.size(); i++)
				{
					if (bd->mResources[i] == nullptr)
						continue;
					auto handle = (DX12PagedHeap*)bd->mResources[i]->GetHWBuffer();
					handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex + i, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				}*/
			}
		}
		else
		{
			auto handle = (DX12PagedHeap*)resource->GetHWBuffer();
			if (binder->Type == EShaderBindType::SBT_Sampler)
			{
				ASSERT(mSamplerHeap != nullptr);
				handle->BindToHeap(device, mSamplerHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
			}
			else
			{
				ASSERT(mCbvSrvUavHeap != nullptr);
				handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
			}
		}
	}

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
	void DX12GraphicDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		auto effect = this->ShaderEffect.UnsafeConvertTo<DX12GraphicsEffect>();
		if (effect->mSignatureBuilder.mCbvSrvUavNumber > 0)
		{
			if (mCbvSrvUavHeap == nullptr)
				mCbvSrvUavHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSignatureBuilder.mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV));
		}
		if (effect->mSignatureBuilder.mSamplerNumber > 0)
		{
			if (mSamplerHeap == nullptr)
				mSamplerHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSignatureBuilder.mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER));
		}

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

		for (auto& i : effect->mCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mSamplerBinders)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(i.RootIndex,
				mSamplerHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}

		UINT finger = 0;
		for (auto& i : BindResources)
		{
			if (i.second != nullptr)
				finger += i.second->GetFingerPrint();
		}
		if (finger != FingerPrient)
		{
			FingerPrient = finger;
			IsDirty = true;
		}

		if (IsDirty)
		{
			ResetHeapToNullByEffect(device, effect, mCbvSrvUavHeap, mSamplerHeap);
			for (auto& i : BindResources)
			{
				BindResourceToHeap(device, i.first, i.second);
			}
			IsDirty = false;
		}
	}
	void DX12GraphicDraw::BindResourceToHeap(DX12GpuDevice* device, const FEffectBinder* binder, IGpuResource* resource)
	{
		if (resource == nullptr)
			return;
		auto handle = (DX12PagedHeap*)resource->GetHWBuffer();
		if (binder->ASBinder)
		{
			Bind2Heap(device, binder->ASBinder, resource, mCbvSrvUavHeap, mSamplerHeap);
		}
		if (binder->MSBinder)
		{
			Bind2Heap(device, binder->MSBinder, resource, mCbvSrvUavHeap, mSamplerHeap);
		}
		if (binder->VSBinder)
		{
			Bind2Heap(device, binder->VSBinder, resource, mCbvSrvUavHeap, mSamplerHeap);
		}
		if (binder->PSBinder)
		{
			Bind2Heap(device, binder->PSBinder, resource, mCbvSrvUavHeap, mSamplerHeap);
		}
	}
	IBindless* DX12GraphicDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
		result->mHeap = binder->BindType == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
		if (result->mHeap == nullptr)
		{
			result->Release();
			return nullptr;
		}
		result->mResources.resize(IBindless::MaxBindless);
		result->mBindType = binder->BindType;
		result->mStartIndex = binder->DescriptorIndex;
		((DX12GraphicDraw*)this)->BindResource(binder, result);
		return result;
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
				if (i.second == nullptr)
					continue;
				switch (i.first->BindType)
				{
					case SBT_CBV:
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
		
		if (ViewInstanceMask != 0)
		{
			//cmdlist->SetViewInstanceMask(ViewInstanceMask);
		}

		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.Draw");
			auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
			ASSERT(pDrawDesc);
			if (IndirectDrawArgsBuffer)
			{
				IndirectDrawArgsBuffer->TransitionTo(cmdlist, GRS_UavIndirect);
				if (pDrawDesc->IsDispatchMesh())
				{
					ASSERT(false);
				}
				else if (pDrawDesc->IsIndexDraw())
				{
					auto effect = this->ShaderEffect.UnsafeConvertTo<DX12GraphicsEffect>();
					dx12Cmd->mCurrentCmdSig = effect->GetIndirectDrawIndexCmdSig(device, dx12Cmd);
					dx12Cmd->mCurrentIndirectOffset = effect->mIndirectOffset;
					cmdlist->IndirectDrawIndexed(pDrawDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
					dx12Cmd->mCurrentCmdSig = nullptr;
					dx12Cmd->mCurrentIndirectOffset = 0;
				}
			}
			else
			{
				if (pDrawDesc->IsDispatchMesh())
				{
					cmdlist->DispatchMesh(pDrawDesc->DispatchMeshX, pDrawDesc->DispatchMeshY, pDrawDesc->DispatchMeshZ);
				}
				else if (pDrawDesc->IsIndexDraw())
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
	IBindless* DX12ComputeDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
		result->mHeap = binder->Type == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
		if (result->mHeap == nullptr)
		{
			result->Release();
			return nullptr;
		}
		result->mResources.resize(IBindless::MaxBindless);
		result->mBindType = binder->Type;
		result->mStartIndex = binder->DescriptorIndex;
		((DX12ComputeDraw*)this)->BindResource(binder, result);
		return result;
	}
	void DX12ComputeDraw::OnBindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	
	void DX12ComputeDraw::BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource)
	{
		Bind2Heap(device, binder, resource, mCbvSrvUavHeap, mSamplerHeap);
	}
	void DX12ComputeDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		auto effect = this->mEffect.UnsafeConvertTo<DX12ComputeEffect>();
		if (effect->mSignatureBuilder.mCbvSrvUavNumber > 0)
		{
			if (mCbvSrvUavHeap == nullptr)
				mCbvSrvUavHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSignatureBuilder.mCbvSrvUavNumber, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV));
		}
		if (effect->mSignatureBuilder.mSamplerNumber > 0)
		{
			if (mSamplerHeap == nullptr)
				mSamplerHeap = MakeWeakRef(device->mDescriptorSetAllocator->AllocDX12Heap(device, effect->mSignatureBuilder.mSamplerNumber, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER));
		}
		
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

		for (auto& i : effect->mCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mSamplerBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mSamplerHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}

		UINT finger = 0;
		for (auto& i : BindResources)
		{
			if (i.second != nullptr)
				finger += i.second->GetFingerPrint();
		}
		if (finger != FingerPrient)
		{
			FingerPrient = finger;
			IsDirty = true;
		}

		if (IsDirty)
		{
			ResetHeapToNullByReflector(device, effect->mComputeShader->Reflector, mCbvSrvUavHeap, mSamplerHeap);
			for (auto& i : BindResources)
			{
				BindResourceToHeap(device, i.first, i.second);
			}
			IsDirty = false;
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
		//effect->Commit(cmdlist, this);
		{
			BindDescriptorHeaps(device, dx12Cmd);
		}
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
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

	void DX12RayTracingDraw::OnBindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void DX12RayTracingDraw::BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12RayTracingEffect* effect)
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

		dx12Cmd->mContext->SetComputeRootSignature(effect->mGlobalSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		for (auto& i : effect->mGlobalCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mGlobalSamplerBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mSamplerHeap->Heap->GetGpuAddress(i.DescriptorStart));
		}
	}
	IBindless* DX12RayTracingDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
		result->mHeap = binder->Type == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
		if (result->mHeap == nullptr)
		{
			result->Release();
			return nullptr;
		}
		result->mResources.resize(IBindless::MaxBindless);
		result->mBindType = binder->Type;
		result->mStartIndex = binder->DescriptorIndex;

		((DX12RayTracingDraw*)this)->BindResource(binder, result);
		return result;
	}
	void DX12RayTracingDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		if (dx12Cmd->mLastContext == nullptr)
			return;

		auto device = dx12Cmd->GetDX12Device();
		auto effect = this->ShaderEffect.UnsafeConvertTo<DX12RayTracingEffect>();
		//effect->BuildState(dx12Cmd->GetDX12Device());

		effect->mSignatureBuilder.CreateHeap(device, mCbvSrvUavHeap, mSamplerHeap);

		if (mHitGroupShaderBindTable == nullptr)
		{
			FBufferDesc hitGroupDesc{};
			hitGroupDesc.SetDefault(false);
			hitGroupDesc.Type = EBufferType::BFT_NONE;
			hitGroupDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			hitGroupDesc.Usage = EGpuUsage::USAGE_STAGING;
			IBlobObject blob;
			const UINT shaderIdentifierSize = D3D12_SHADER_IDENTIFIER_SIZE_IN_BYTES;
			ShaderBindTables.clear();
			for (auto& i : effect->mHitGroups)
			{
				FHitGroupShaderBindTable sbt{};
				int PushSize = 0;
				auto group = i.UnsafeConvertTo<DX12RayTracingEffect::DX12HitGroup>();
				sbt.HitGroup = group;
				auto hitGroupShaderIdentifier = effect->mStateObjectProperties->GetShaderIdentifier(StringHelper::strtowstr(group->Name).c_str());
				blob.PushData(hitGroupShaderIdentifier, shaderIdentifierSize);
				PushSize += shaderIdentifierSize;
				for (auto& i : group->CbvSrvUavBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mCbvSrvUavHeap->GetGpuAddress(i.DescriptorStart);
					blob.PushData(&value, sizeof(D3D12_GPU_DESCRIPTOR_HANDLE));
					PushSize += sizeof(D3D12_GPU_DESCRIPTOR_HANDLE);
				}
				for (auto& i : group->SamplerBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mSamplerHeap->GetGpuAddress(i.DescriptorStart);
					blob.PushData(&value, sizeof(D3D12_GPU_DESCRIPTOR_HANDLE));
					PushSize += sizeof(D3D12_GPU_DESCRIPTOR_HANDLE);
				}
				auto alignSize = Align(PushSize, D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT);
				if (alignSize - PushSize)
				{
					blob.PushData(nullptr, alignSize - PushSize);
				}
				ShaderBindTables.push_back(sbt);
			}
			hitGroupDesc.Size = blob.GetSize();
			hitGroupDesc.RowPitch = hitGroupDesc.Size;
			hitGroupDesc.DepthPitch = hitGroupDesc.Size;
			hitGroupDesc.InitData = blob.GetData();
			mHitGroupShaderBindTable = MakeWeakRef(new FUploadBuffer(MakeWeakRef(device->CreateBuffer(&hitGroupDesc))));
		}

		BindDescriptors(device, dx12Cmd, effect);

		if (IsDirty)
		{
			ResetHeapToNullByReflector(device, effect->GetShaderLibDesc()->DxILReflector, mCbvSrvUavHeap, mSamplerHeap);

			for (auto& i : this->BindResources)
			{
				if (i.second == nullptr)
					continue;
				auto binder = i.first;

				Bind2Heap(device, binder, i.second, mCbvSrvUavHeap, mSamplerHeap);
			}
			IsDirty = false;
		}

		for (auto& i : BindResources)
		{
			if (i.second == nullptr)
				continue;
			switch (i.first->Type)
			{
			case SBT_CBV:
			{
				IGpuResource* t = i.second;
				effect->BindCBV(cmdlist, i.first, (ICbView*)t);
			}
			break;
			case SBT_SRV:
			{
				auto t = (DX12SrView*)i.second;
				effect->BindSrv(cmdlist, i.first, t);
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

		D3D12_DISPATCH_RAYS_DESC dispatchDesc = {};
		dispatchDesc.Width = this->Width;
		dispatchDesc.Height = this->Height;
		dispatchDesc.Depth = this->Depth;
		auto dxBuffer = (DX12Buffer*)effect->mRayGenShaderTable->GetBuffer();
		dispatchDesc.RayGenerationShaderRecord.StartAddress = dxBuffer->GetGPUVirtualAddress();
		dispatchDesc.RayGenerationShaderRecord.SizeInBytes = dxBuffer->Desc.Size;
		dxBuffer = (DX12Buffer*)effect->mMissShaderTable->GetBuffer();
		dispatchDesc.MissShaderTable.StartAddress = dxBuffer->GetGPUVirtualAddress();
		dispatchDesc.MissShaderTable.SizeInBytes = dxBuffer->Desc.Size;
		dispatchDesc.MissShaderTable.StrideInBytes = dxBuffer->Desc.Size;
		dxBuffer = (DX12Buffer*)mHitGroupShaderBindTable->GetBuffer();
		dispatchDesc.HitGroupTable.StartAddress = dxBuffer->GetGPUVirtualAddress();
		dispatchDesc.HitGroupTable.SizeInBytes = dxBuffer->Desc.Size;
		dispatchDesc.HitGroupTable.StrideInBytes = dxBuffer->Desc.Size;

		dx12Cmd->mLastContext->SetPipelineState1(effect->mStateObject);
		dx12Cmd->mLastContext->DispatchRays(&dispatchDesc);
	}
}

NS_END
