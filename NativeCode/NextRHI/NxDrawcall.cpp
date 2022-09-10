#include "NxDrawcall.h"
#include "NxBuffer.h"
#include "NxGpuState.h"
#include "NxGeomMesh.h"
#include "NxEffect.h"
#include "NxCommandList.h"
#include "NxFrameBuffers.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void IGraphicDraw::UpdateGpuDrawState(IGpuDevice* device, ICommandList* cmdlist, IRenderPass* rpass)
	{
		auto topo = EPrimitiveType::EPT_TriangleList;
		if (Mesh != nullptr)
		{
			topo = Mesh->GetAtomDesc(MeshAtom, MeshLOD)->PrimitiveType;
		}
		if (ShaderEffect == nullptr)
			return;

		auto pPipe = Pipeline;
		if (pPipe == nullptr)
		{
			pPipe = cmdlist->GetDefaultPipeline();
		}
		if (GpuDrawState != nullptr &&
			GpuDrawState->RenderPass == rpass &&
			GpuDrawState->ShaderEffect == ShaderEffect &&
			GpuDrawState->Pipeline == pPipe &&
			GpuDrawState->TopologyType == topo)
		{
			return;
		}
		else
		{
			GpuDrawState = device->GetGpuPipelineManager()->GetOrCreate(device, rpass, ShaderEffect, pPipe, topo);
			OnGpuDrawStateUpdated();
		}
	}
	void IGraphicDraw::BindShaderEffect(IGpuDevice* device, IShaderEffect* effect)
	{
		ShaderEffect = effect;
	}
	void IGraphicDraw::BindGeomMesh(IGpuDevice* device, FGeomMesh* pMesh)
	{
		Mesh = pMesh;
	}
	void IGraphicDraw::BindPipeline(IGpuDevice* device, IGpuPipeline* pipe)
	{
		Pipeline = pipe;
	}
	IShaderEffect* IGraphicDraw::GetShaderEffect() 
	{
		return ShaderEffect;
	}
	IGpuPipeline* IGraphicDraw::GetPipeline() 
	{
		return Pipeline;
	}
	const FEffectBinder* IGraphicDraw::FindBinder(const char* name) const
	{
		return ShaderEffect->FindBinder(name);
	}
	IGpuResource* IGraphicDraw::FindGpuResource(VNameString name)
	{
		auto binder = GetShaderEffect()->FindBinder(name);
		if (binder == nullptr)
			return nullptr;

		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			return iter->second;
		}
		return nullptr;
	}
	bool IGraphicDraw::BindResource(VNameString name, IGpuResource* resource)
	{
		auto binder = GetShaderEffect()->FindBinder(name);
		if (binder == nullptr)
			return false;

		BindResource(binder, resource);
		
		return true;
	}
	void IGraphicDraw::BindResource(const FEffectBinder* binder, IGpuResource* resource)
	{
		AutoRef<IGpuResource> tmp(resource);
		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			if (iter->second == resource)
			{
				return;
			}
			else
			{
				BindResources[binder] = tmp;
				OnBindResource(binder, resource);
			}
		}
		else
		{
			BindResources[binder] = tmp;
			OnBindResource(binder, resource);
		}
	}
	void IGraphicDraw::BindIndirectDrawArgsBuffer(IBuffer* buffer, UINT offset)
	{
		IndirectDrawArgsBuffer = buffer;
		IndirectDrawOffsetForArgs = offset;
		OnBindResource(nullptr, buffer);
	}
	void IGraphicDraw::Commit(ICommandList* cmdlist)
	{
		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		Mesh->Commit(cmdlist);

		if (AttachVB != nullptr)
		{
			AttachVB->Commit(cmdlist);
		}

		UpdateGpuDrawState(cmdlist->GetGpuDevice(), cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
		/*if (GpuDrawState == nullptr)
		{
			UpdateGpuDrawState(cmdlist->mDevice, cmdlist, );
			if (GpuDrawState == nullptr)
				return;
		}*/
		cmdlist->SetGraphicsPipeline(GpuDrawState);
		
		auto effect = GetShaderEffect();

		effect->Commit(cmdlist, this);
		
		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					effect->BindCBuffer(cmdlist, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
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
		
		auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		ASSERT(pDrawDesc);
		if (IndirectDrawArgsBuffer)
		{
			cmdlist->IndirectDrawIndexed(pDrawDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
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

	const FShaderBinder* IComputeDraw::FindBinder(EShaderBindType type, const char* name) const
	{
		return mEffect->FindBinder(type, name);
	}
	bool IComputeDraw::BindResource(EShaderBindType type, VNameString name, IGpuResource* resource)
	{
		auto binder = mEffect->FindBinder(type, name);
		if (binder == nullptr)
			return false;

		BindResource(binder, resource);
		return true;
	}
	void IComputeDraw::BindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		AutoRef<IGpuResource> tmp(resource);
		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			if (iter->second == resource)
			{
				return;
			}
			else
			{
				BindResources[binder] = tmp;
				OnBindResource(binder, resource);
			}
		}
		else
		{
			BindResources[binder] = tmp;
			OnBindResource(binder, resource);
		}
	}
	IGpuResource* IComputeDraw::FindGpuResource(EShaderBindType type, VNameString name)
	{
		auto binder = mEffect->FindBinder(type, name);
		if (binder == nullptr)
			return nullptr;

		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			return iter->second;
		}
		return nullptr;
	}

	void IComputeDraw::Commit(ICommandList* cmdlist)
	{
		if (mEffect == nullptr)
			return;

		cmdlist->SetShader(mEffect->mComputeShader);
		mEffect->Commit(cmdlist);
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBuffer:
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
			cmdlist->IndirectDispatch(IndirectDispatchArgsBuffer, 0);
		}
		else
		{
			cmdlist->Dispatch(mDispatchX, mDispatchY, mDispatchZ);
		}
	}

	void ICopyDraw::BindSrc(IGpuBufferData* res)
	{
		mSrc = res;
	}
	void ICopyDraw::BindDest(IGpuBufferData* res)
	{
		mDest = res;
	}
	void ICopyDraw::Commit(ICommandList* cmdlist)
	{
		//cmdlist->CopyBuffer(mSrc, mDest);
	}
}

NS_END