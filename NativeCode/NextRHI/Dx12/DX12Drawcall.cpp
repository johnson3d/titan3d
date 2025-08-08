#include "DX12Drawcall.h"
#include "DX12CommandList.h"
#include "DX12Buffer.h"
#include "DX12GpuState.h"
#include "DX12FrameBuffers.h"
#include "DX12GpuDevice.h"
#include "DX12GeomMesh.h"
#include "DX12DescriptorSet.h"
#include "../NxGeomMesh.h"
#include "../NxEffect.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	static void Bind2Heap(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource,
		FDX12DescriptorHeap& mCbvSrvUavHeap, FDX12DescriptorHeap& mSamplerHeap, FCopyDescriptors& cbvsrvuavDescriptors, FCopyDescriptors& samplerDescriptors)
	{
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
			DX12PagedHeap* handle;
			if (resource != nullptr)
			{
				handle = (DX12PagedHeap*)resource->GetHWBuffer();
			}
			else
			{
				handle = DX12PagedHeap::GetNullHeap(device, binder->Type);
			}
			if (binder->Type == EShaderBindType::SBT_Sampler)
			{
				ASSERT(mSamplerHeap.Num != 0);
				//handle->BindToHeap(device, mSamplerHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
				if (mSamplerHeap.IsSet(binder->DescriptorIndex) == false)
					handle->PushDescriptorCopy(samplerDescriptors, mSamplerHeap, binder->DescriptorIndex);
			}
			else
			{
				ASSERT(mCbvSrvUavHeap.Num != 0);
				//handle->BindToHeap(device, mCbvSrvUavHeap->Heap, binder->DescriptorIndex, 0, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
				if (mCbvSrvUavHeap.IsSet(binder->DescriptorIndex) == false)
					handle->PushDescriptorCopy(cbvsrvuavDescriptors, mCbvSrvUavHeap, binder->DescriptorIndex);
			}
		}
	}

	static void CommitResource(DX12CommandList* cmdlist, EShaderType shaderType, const FShaderBinder* binder, IGpuResource* resource)
	{
		if (resource == nullptr)
			return;
		//1 Sured TransitionTo Layout
		//2 Update AccessTime for Texture SRV(streaming need)
		bool bBindless = binder->IsBindless();
		if (bBindless)
		{
			auto bl = (IBindless*)resource;
			switch (binder->Type)
			{
				case SBT_SRV:
				{
					for (auto& i : bl->mResources)
					{
						if (i.Resource == nullptr)
							continue;

						cmdlist->SetSrv(shaderType, binder, i.Resource.UnsafeConvertTo<DX12SrView>());
					}
				}
				break;
				case SBT_UAV:
				{
					for (auto& i : bl->mResources)
					{
						if (i.Resource == nullptr)
							continue;

						cmdlist->SetUav(shaderType, binder, i.Resource.UnsafeConvertTo<DX12UaView>());
					}
				}
				break;
			}
			return;
		}
		
		switch (binder->Type)
		{
			case SBT_CBV:
			{
				cmdlist->SetCBV(shaderType, binder, (ICbView*)resource);
			}
			break;
			case SBT_SRV:
			{
				cmdlist->SetSrv(shaderType, binder, (DX12SrView*)resource);
			}
			break;
			case SBT_UAV:
			{
				cmdlist->SetUav(shaderType, binder, (IUaView*)resource);
			}
			break;
			case SBT_Sampler:
			{
				cmdlist->SetSampler(shaderType, binder, (ISampler*)resource);
			}
			break;
			default:
				break;
		}
	}

	static bool GDX12ForceCopyDiscriptor = true;
	DX12GraphicDraw::DX12GraphicDraw()
	{

	}
	DX12GraphicDraw::~DX12GraphicDraw()
	{
	}
	void DX12GraphicDraw::OnGpuDrawStateUpdated()
	{
		
	}
	void DX12GraphicDraw::ResetResources()
	{
		IGraphicDraw::ResetResources();
	}
	void DX12GraphicDraw::OnBindResource(const FEffectBinder* binder, FBindResource& resource)
	{
		if (binder == nullptr || binder->IsBindless())
			return;
		
		//BindResourceToHeap(device, binder, resource);
		resource.FingerPrint = -1;
	}
	void DX12GraphicDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		auto effect = (DX12GraphicsEffect*)this->ShaderEffect.GetPtr();

		FDX12DescriptorHeap			mCbvSrvUavHeap;
		FDX12DescriptorHeap			mSamplerHeap;
		effect->mSignatureBuilder.CreateHeap(device, mCbvSrvUavHeap, mSamplerHeap);
		//ResetHeapToNullByEffect(device, effect, mCbvSrvUavHeap, mSamplerHeap);

		ID3D12DescriptorHeap* descriptorHeaps[2] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap.Heap;
		}

		if (mSamplerHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap.Heap;
		}

		if (dx12Cmd->mCurrentGraphicsRootSignature != effect->mSignature)
		{
			dx12Cmd->mCurrentGraphicsRootSignature = effect->mSignature;
			dx12Cmd->mContext->SetGraphicsRootSignature(effect->mSignature);
		}
		if (dx12Cmd->mCurrentNumOfHeaps != NumOfHeaps || dx12Cmd->mCurrentBindHeap[0]!= descriptorHeaps[0] || dx12Cmd->mCurrentBindHeap[1] != descriptorHeaps[1])
		{
			dx12Cmd->mCurrentNumOfHeaps = NumOfHeaps;
			dx12Cmd->mCurrentBindHeap[0] = descriptorHeaps[0];
			dx12Cmd->mCurrentBindHeap[1] = descriptorHeaps[1];
			dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);
		}

		for (auto& i : effect->mCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap.GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mSamplerBinders)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(i.RootIndex,
				mSamplerHeap.GetGpuAddress(i.DescriptorStart));
		}
		thread_local static FCopyDescriptors cbvsrvuavDescriptors;
		thread_local static FCopyDescriptors samplerDescriptors;
		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
		for (auto& i : BindResources)
		{
			auto binder = i.first->GetShaderBinder();
			if (binder->IsBindless())
			{
				auto bl = (DX12Bindless*)i.second.Resource;
				if (bl)
				{
					bl->mHeap = bl->mBindType == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
					bl->BindResources();
					bl->mHeap.Num = 0;
				}
			}	
			else
			{
				BindResourceToHeap(device, i.first, i.second, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			}
		}
		
		if (cbvsrvuavDescriptors.Dest.size() > 0)
		{
			mCbvSrvUavHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)cbvsrvuavDescriptors.Dest.size(), cbvsrvuavDescriptors.Dest.data(), cbvsrvuavDescriptors.Sizes.data(),
				(UINT)cbvsrvuavDescriptors.Src.size(), cbvsrvuavDescriptors.Src.data(), cbvsrvuavDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		if (samplerDescriptors.Dest.size() > 0)
		{
			mSamplerHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)samplerDescriptors.Dest.size(), samplerDescriptors.Dest.data(), samplerDescriptors.Sizes.data(),
				(UINT)samplerDescriptors.Src.size(), samplerDescriptors.Src.data(), samplerDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}

		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
	}
	void DX12GraphicDraw::BindResourceToHeap(DX12GpuDevice* device, const FEffectBinder* binder, FBindResource& resource,
		FDX12DescriptorHeap& mCbvSrvUavHeap, FDX12DescriptorHeap& mSamplerHeap, FCopyDescriptors& cbvsrvuavDescriptors, FCopyDescriptors& samplerDescriptors)
	{
		//Every FShaderBinder's DescriptorIndex is same with FEffectBinder's DescriptorIndex
		if (binder->ASBinder)
		{
			Bind2Heap(device, binder->ASBinder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			return;
		}
		if (binder->MSBinder)
		{
			Bind2Heap(device, binder->MSBinder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			return;
		}
		if (binder->VSBinder)
		{
			Bind2Heap(device, binder->VSBinder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			return;
		}
		if (binder->PSBinder)
		{
			Bind2Heap(device, binder->PSBinder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			return;
		}
		/*if (resource.Resource)
			resource.FingerPrint = resource.Resource->GetFingerPrint();
		else
			resource.FingerPrint = 0;*/
	}
	IBindless* DX12GraphicDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
		result->mResources.resize(IBindless::MaxBindless);
		result->mBindType = binder->BindType;
		result->mStartIndex = binder->DescriptorIndex;
		((DX12GraphicDraw*)this)->BindResource(binder, result);
		return result;
	}
	void DX12GraphicDraw::BuildDrawcall(ICommandList* cmdlist)
	{
		for (auto& i : BindResources)
		{
			CommitResource((DX12CommandList*)cmdlist, EShaderType::SDT_Unknown, i.first->GetValidShaderBinder(), i.second.Resource);
		}
		if (IndirectDrawArgsBuffer)
			FTransitionScope::Transition(cmdlist, IndirectDrawArgsBuffer, GRS_UavIndirect, true);
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
			if (dx12Cmd->mCurrentGeomMesh != Mesh || AttachVB != dx12Cmd->mCurrentAttachVA)
			{
				dx12Cmd->mCurrentGeomMesh = Mesh;
				dx12Cmd->mCurrentAttachVA = AttachVB;
				DX12VertexArray* VAs[2] = { Mesh->VertexArray , AttachVB };
				DX12VertexArray::Commit(dx12Cmd, 2, VAs);
				dx12Cmd->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);
			}
		}
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.UpdateDrawState");
			UpdateGpuDrawState(device, cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
			cmdlist->SetGraphicsPipeline(GpuDrawState);
		}
		
		auto effect = (DX12GraphicsEffect*)GetGraphicsEffect();
		
		{
			AUTO_SAMP("NxRHI.GraphicDraw.Commit.BindResouces");
			//todo: move copy descriptor heap to BuildDrawcall,and parallel build them
			BindDescriptorHeaps(device, dx12Cmd);
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
		
	}
	IBindless* DX12ComputeDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
		result->mResources.resize(IBindless::MaxBindless);
		result->mBindType = binder->Type;
		result->mStartIndex = binder->DescriptorIndex;
		((DX12ComputeDraw*)this)->BindResource(binder, result);
		return result;
	}
	void DX12ComputeDraw::ResetResources() 
	{
		IComputeDraw::ResetResources();
	}
	void DX12ComputeDraw::OnBindResource(const FShaderBinder* binder, FBindResource& resource)
	{
		if (binder->IsBindless())
			return;

		resource.FingerPrint = -1;
		//IsDirty = true;
	}
	void DX12ComputeDraw::BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, FBindResource& resource,
		FDX12DescriptorHeap& mCbvSrvUavHeap, FDX12DescriptorHeap& mSamplerHeap, FCopyDescriptors& cbvsrvuavDescriptors, FCopyDescriptors& samplerDescriptors)
	{
		Bind2Heap(device, binder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
		/*if (resource.Resource)
			resource.FingerPrint = resource.Resource->GetFingerPrint();
		else
			resource.FingerPrint = 0;*/
	}
	void DX12ComputeDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd)
	{
		FDX12DescriptorHeap			mCbvSrvUavHeap;
		FDX12DescriptorHeap			mSamplerHeap;
		auto effect = this->mEffect.UnsafeConvertTo<DX12ComputeEffect>();
		effect->mSignatureBuilder.CreateHeap(device, mCbvSrvUavHeap, mSamplerHeap);
		
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap.Heap;
		}

		if (mSamplerHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap.Heap;
		}

		dx12Cmd->mContext->SetComputeRootSignature(effect->mSignature);
		if (dx12Cmd->mCurrentNumOfHeaps != NumOfHeaps || dx12Cmd->mCurrentBindHeap[0] != descriptorHeaps[0] || dx12Cmd->mCurrentBindHeap[1] != descriptorHeaps[1])
		{
			dx12Cmd->mCurrentNumOfHeaps = NumOfHeaps;
			dx12Cmd->mCurrentBindHeap[0] = descriptorHeaps[0];
			dx12Cmd->mCurrentBindHeap[1] = descriptorHeaps[1];
			dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);
		}

		for (auto& i : effect->mCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap.GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mSamplerBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mSamplerHeap.GetGpuAddress(i.DescriptorStart));
		}

		thread_local static FCopyDescriptors cbvsrvuavDescriptors;
		thread_local static FCopyDescriptors samplerDescriptors;
		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			if (binder->IsBindless())
			{
				auto bl = (DX12Bindless*)i.second.Resource;
				if (bl)
				{
					bl->mHeap = bl->mBindType == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
					bl->BindResources();
					bl->mHeap.Num = 0;
				}
			}
			else
			{
				BindResourceToHeap(device, i.first, i.second, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			}
			/*if (i.second.Resource)
			{
				if (GDX12ForceCopyDiscriptor || i.second.Resource->GetFingerPrint() != i.second.FingerPrint)
				{
					BindResourceToHeap(device, i.first, i.second);
				}
			}*/
		}
		
		if (cbvsrvuavDescriptors.Dest.size() > 0)
		{
			mCbvSrvUavHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)cbvsrvuavDescriptors.Dest.size(), cbvsrvuavDescriptors.Dest.data(), cbvsrvuavDescriptors.Sizes.data(),
				(UINT)cbvsrvuavDescriptors.Src.size(), cbvsrvuavDescriptors.Src.data(), cbvsrvuavDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		if (samplerDescriptors.Dest.size() > 0)
		{
			mSamplerHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)samplerDescriptors.Dest.size(), samplerDescriptors.Dest.data(), samplerDescriptors.Sizes.data(),
				(UINT)samplerDescriptors.Src.size(), samplerDescriptors.Src.data(), samplerDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}

		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
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
		
		BindDescriptorHeaps(device, dx12Cmd);

		for (auto& i : BindResources)
		{
			CommitResource((DX12CommandList*)cmdlist, EShaderType::SDT_ComputeShader, i.first, i.second.Resource);
		}

		if (IndirectDispatchArgsBuffer != nullptr)
		{
			FTransitionScope::Transition(cmdlist, IndirectDispatchArgsBuffer, GRS_UavIndirect, true);
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

	void DX12RayTracingDraw::OnBindResource(const FShaderBinder* binder, FBindResource& resource)
	{
		/*if (binder->IsBindless())
			return;*/

		resource.FingerPrint = -1;
	}
	void DX12RayTracingDraw::BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, FBindResource& resource,
		FDX12DescriptorHeap& mCbvSrvUavHeap, FDX12DescriptorHeap& mSamplerHeap, FCopyDescriptors& cbvsrvuavDescriptors, FCopyDescriptors& samplerDescriptors)
	{
		Bind2Heap(device, binder, resource.Resource, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
		if (resource.Resource)
			resource.FingerPrint = resource.Resource->GetFingerPrint();
		else
			resource.FingerPrint = 0;
	}
	void DX12RayTracingDraw::BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12RayTracingEffect* effect,
		FDX12DescriptorHeap& mCbvSrvUavHeap, FDX12DescriptorHeap& mSamplerHeap)
	{
		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		if (mCbvSrvUavHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mCbvSrvUavHeap.Heap;
		}

		if (mSamplerHeap.Num != 0)
		{
			descriptorHeaps[NumOfHeaps++] = mSamplerHeap.Heap;
		}

		dx12Cmd->mContext->SetComputeRootSignature(effect->mGlobalSignature);
		if (dx12Cmd->mCurrentNumOfHeaps != NumOfHeaps || dx12Cmd->mCurrentBindHeap[0] != descriptorHeaps[0] || dx12Cmd->mCurrentBindHeap[1] != descriptorHeaps[1])
		{
			dx12Cmd->mCurrentNumOfHeaps = NumOfHeaps;
			dx12Cmd->mCurrentBindHeap[0] = descriptorHeaps[0];
			dx12Cmd->mCurrentBindHeap[1] = descriptorHeaps[1];
			dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);
		}

		for (auto& i : effect->mGlobalCbvSrvUavBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mCbvSrvUavHeap.GetGpuAddress(i.DescriptorStart));
		}
		for (auto& i : effect->mGlobalSamplerBinders)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(i.RootIndex,
				mSamplerHeap.GetGpuAddress(i.DescriptorStart));
		}

		thread_local static FCopyDescriptors cbvsrvuavDescriptors;
		thread_local static FCopyDescriptors samplerDescriptors;
		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			if (binder->IsBindless())
			{
				auto bl = (DX12Bindless*)i.second.Resource;
				if (bl)
				{
					bl->mHeap = bl->mBindType == EShaderBindType::SBT_Sampler ? mSamplerHeap : mCbvSrvUavHeap;
					bl->BindResources();
					bl->mHeap.Num = 0;
				}
			}
			else
			{
				BindResourceToHeap(device, i.first, i.second, mCbvSrvUavHeap, mSamplerHeap, cbvsrvuavDescriptors, samplerDescriptors);
			}
			/*if (i.second.Resource)
			{
				if (GDX12ForceCopyDiscriptor || i.second.Resource->GetFingerPrint() != i.second.FingerPrint)
				{
					BindResourceToHeap(device, i.first, i.second, mCbvSrvUavHeap, mSamplerHeap);
				}
			}*/
		}

		if (cbvsrvuavDescriptors.Dest.size() > 0)
		{
			mCbvSrvUavHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)cbvsrvuavDescriptors.Dest.size(), cbvsrvuavDescriptors.Dest.data(), cbvsrvuavDescriptors.Sizes.data(),
				(UINT)cbvsrvuavDescriptors.Src.size(), cbvsrvuavDescriptors.Src.data(), cbvsrvuavDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		if (samplerDescriptors.Dest.size() > 0)
		{
			mSamplerHeap.CheckCompletion();
			device->mDevice->CopyDescriptors((UINT)samplerDescriptors.Dest.size(), samplerDescriptors.Dest.data(), samplerDescriptors.Sizes.data(),
				(UINT)samplerDescriptors.Src.size(), samplerDescriptors.Src.data(), samplerDescriptors.Sizes.data(), D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}

		cbvsrvuavDescriptors.Reset();
		samplerDescriptors.Reset();
	}
	IBindless* DX12RayTracingDraw::CreateBindless(const char* name) const
	{
		auto binder = this->FindBinder(name);
		if (binder == nullptr)
			return nullptr;
		auto result = new DX12Bindless();
		result->mDeviceRef.FromObject(mDeviceRef.GetPtr());
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

		FDX12DescriptorHeap			mCbvSrvUavHeap;
		FDX12DescriptorHeap			mSamplerHeap;
		effect->mSignatureBuilder.CreateHeap(device, mCbvSrvUavHeap, mSamplerHeap);

		const UINT shaderIdentifierSize = D3D12_SHADER_IDENTIFIER_SIZE_IN_BYTES;
		if (mHitGroupShaderBindTable == nullptr)
		{
			FBufferDesc hitGroupDesc{};
			hitGroupDesc.SetDefault(false);
			hitGroupDesc.Type = EBufferType::BFT_NONE;
			hitGroupDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			hitGroupDesc.Usage = EGpuUsage::USAGE_STAGING;
			IBlobObject blob;
			ShaderBindTables.clear();
			for (auto& i : effect->mHitGroups)
			{
				FHitGroupShaderBindTable sbt{};
				int PushSize = 0;
				auto group = i.UnsafeConvertTo<DX12RayTracingEffect::DX12HitGroup>();
				sbt.HitGroup = group;
				auto hitGroupShaderIdentifier = effect->mStateObjectProperties->GetShaderIdentifier(StringHelper::strtowstr(group->Name).c_str());
				sbt.HitGroup->HitGroupShaderIdentifier = hitGroupShaderIdentifier;

				blob.PushData(hitGroupShaderIdentifier, shaderIdentifierSize);
				PushSize += shaderIdentifierSize;
				for (auto& i : group->CbvSrvUavBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mCbvSrvUavHeap.GetGpuAddress(i.DescriptorStart);
					blob.PushData(&value, sizeof(D3D12_GPU_DESCRIPTOR_HANDLE));
					PushSize += sizeof(D3D12_GPU_DESCRIPTOR_HANDLE);
				}
				for (auto& i : group->SamplerBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mSamplerHeap.GetGpuAddress(i.DescriptorStart);
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
		else
		{
			auto pData = (BYTE*)mHitGroupShaderBindTable->GetPtr();
			ProxyMemStreamWriter blob(pData, mHitGroupShaderBindTable->GetSize());
			
			for (auto& sbt : ShaderBindTables)
			{	
				int PushSize = 0;
				blob.Write(sbt.HitGroup->HitGroupShaderIdentifier, shaderIdentifierSize);
				PushSize += shaderIdentifierSize;
				for (auto& i : sbt.HitGroup->CbvSrvUavBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mCbvSrvUavHeap.GetGpuAddress(i.DescriptorStart);
					blob.Write(&value, sizeof(D3D12_GPU_DESCRIPTOR_HANDLE));
					PushSize += sizeof(D3D12_GPU_DESCRIPTOR_HANDLE);
				}
				for (auto& i : sbt.HitGroup->SamplerBinders)
				{
					D3D12_GPU_DESCRIPTOR_HANDLE value = mSamplerHeap.GetGpuAddress(i.DescriptorStart);
					blob.Write(&value, sizeof(D3D12_GPU_DESCRIPTOR_HANDLE));
					PushSize += sizeof(D3D12_GPU_DESCRIPTOR_HANDLE);
				}
				auto alignSize = Align(PushSize, D3D12_RAYTRACING_SHADER_RECORD_BYTE_ALIGNMENT);
				if (alignSize - PushSize)
				{
					blob.Write(nullptr, alignSize - PushSize);
				}
			}
		}

		BindDescriptorHeaps(device, dx12Cmd, effect, mCbvSrvUavHeap, mSamplerHeap);

		for (auto& i : BindResources)
		{
			CommitResource((DX12CommandList*)cmdlist, EShaderType::SDT_RayTracing, i.first, i.second.Resource);
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
