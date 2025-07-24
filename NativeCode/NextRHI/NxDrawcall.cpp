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
	std::atomic<int>		IGraphicDraw::NumOfInstance;
	std::atomic<int>		IComputeDraw::NumOfInstance;
	std::atomic<int>		ICopyDraw::NumOfInstance;

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
	void IGraphicDraw::BindShaderEffect(IGpuDevice* device, IGraphicsEffect* effect)
	{
		if (ShaderEffect == effect)
			return;
		ShaderEffect = effect;
		BindResources.clear();
		for (auto& i : effect->mBinders)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i.second] = temp;
		}
	}
	void IGraphicDraw::BindGeomMesh(IGpuDevice* device, FGeomMesh* pMesh)
	{
		Mesh = pMesh;
	}
	void IGraphicDraw::BindPipeline(IGpuDevice* device, IGpuPipeline* pipe)
	{
		Pipeline = pipe;
	}
	IGraphicsEffect* IGraphicDraw::GetGraphicsEffect() 
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
		auto binder = GetGraphicsEffect()->FindBinder(name);
		if (binder == nullptr)
			return nullptr;

		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			return iter->second.Resource;
		}
		return nullptr;
	}
	bool IGraphicDraw::BindResource(VNameString name, IGpuResource* resource)
	{
		auto binder = GetGraphicsEffect()->FindBinder(name);
		if (binder == nullptr)
			return false;

		BindResource(binder, resource);
		
		return true;
	}
	IGraphicDraw::IGraphicDraw()
	{
		NumOfInstance++;
	}
	IGraphicDraw::~IGraphicDraw()
	{
		NumOfInstance--;
	}
	void IGraphicDraw::BindResource(const FEffectBinder* binder, IGpuResource* resource)
	{
		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			if (iter->second.Resource == resource)
			{
				return;
			}
			else
			{
				auto& bs = BindResources[binder];
				bs.SetResource(resource);
				OnBindResource(binder, bs);
			}
		}
		else
		{
			FBindResource bs{};
			bs.Resource = resource;
			bs.FingerPrint = resource->GetFingerPrint();
			BindResources.insert(std::make_pair(binder, bs));
			OnBindResource(binder, bs);
		}
	}
	void IGraphicDraw::BindIndirectDrawArgsBuffer(IBuffer* buffer, UINT offset)
	{
		IndirectDrawArgsBuffer = buffer;
		IndirectDrawOffsetForArgs = offset;
		FBindResource bs{};
		bs.SetResource(buffer);
		OnBindResource(nullptr, bs);
	}
	void IGraphicDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		auto device = cmdlist->GetGpuDevice();
		device->CheckDeviceThread();
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
		
		auto effect = GetGraphicsEffect();

		effect->Commit(cmdlist, this);
		
		for (auto& i : BindResources)
		{
			IGpuResource* t = i.second.Resource;
			if (t == nullptr)
				continue;
			switch (i.first->BindType)
			{
				case SBT_CBV:
				{
					effect->BindCBV(cmdlist, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					effect->BindUav(cmdlist, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
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

	void IComputeDraw::BindShaderEffect(IComputeEffect* effect)
	{
		if (mEffect == effect)
			return;
		mEffect = effect;
		BindResources.clear();
		auto reflector = effect->mComputeShader->Reflector;
		for (auto& i : reflector->CBuffers)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Srvs)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Uavs)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Samplers)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
	}
	const FShaderBinder* IComputeDraw::FindBinder(EShaderBindType type, const char* name) const
	{
		return mEffect->FindBinder(type, name);
	}
	const FShaderBinder* IComputeDraw::FindBinder(const char* name) const
	{
		return mEffect->FindBinder(name);
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
		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			if (iter->second.Resource == resource)
			{
				return;
			}
			else
			{
				auto& bs = BindResources[binder];
				bs.SetResource(resource);
				OnBindResource(binder, bs);
			}
		}
		else
		{
			FBindResource bs{};
			bs.Resource = resource;
			bs.FingerPrint = resource->GetFingerPrint();
			BindResources.insert(std::make_pair(binder, bs));
			OnBindResource(binder, bs);
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
			return iter->second.Resource;
		}
		return nullptr;
	}

	void IComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		auto device = cmdlist->GetGpuDevice();
		device->CheckDeviceThread();
		if (mEffect == nullptr)
			return;

		cmdlist->SetShader(mEffect->mComputeShader);
		mEffect->Commit(cmdlist);
		for (auto& i : BindResources)
		{
			IGpuResource* t = i.second.Resource;
			if (t == nullptr)
				continue;
			switch (i.first->Type)
			{
				case SBT_CBV:
				{	
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					cmdlist->SetUav(EShaderType::SDT_ComputeShader, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
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

		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_UAV:
				{
					cmdlist->SetUav(EShaderType::SDT_ComputeShader, i.first, nullptr);
				}
				break;
				default:
					break;
			}
		}
	}

	void ICopyDraw::BindBufferSrc(IBuffer* res)
	{
		ASSERT(res->GpuState != EGpuResourceState::GRS_Undefine);
		mSrc = res;
	}
	void ICopyDraw::BindBufferDest(IBuffer* res)
	{
		mDest = res;
	}
	void ICopyDraw::BindTextureSrc(ITexture* res)
	{
		mSrc = res;
	}
	void ICopyDraw::BindTextureDest(ITexture* res)
	{
		mDest = res;
	}
	void ICopyDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		auto device = cmdlist->GetGpuDevice();
		device->CheckDeviceThread();
		
		FTransitionScope targetTransition(cmdlist, mDest, EGpuResourceState::GRS_CopyDst);
		FTransitionScope sourceTransition(cmdlist, mSrc, EGpuResourceState::GRS_CopySrc);

		cmdlist->BeginEvent("CopyDraw");
		switch (Mode)
		{
			case EngineNS::NxRHI::CDM_Buffer2Buffer:
			{
				cmdlist->CopyBufferRegion(mDest.UnsafeConvertTo<IBuffer>(), DstX, mSrc.UnsafeConvertTo<IBuffer>(), FootPrint.X, FootPrint.Width);
			}
			break;
			case EngineNS::NxRHI::CDM_Texture2Texture:
				{
					if (FootPrint.Width == 0 && FootPrint.Height == 0 && FootPrint.Depth == 0)
					{
						cmdlist->CopyTextureRegion(mDest.UnsafeConvertTo<ITexture>(), DestSubResource, 0,
							0, 0, mSrc.UnsafeConvertTo<ITexture>(), SrcSubResource, nullptr);
					}
					else
					{
						FSubresourceBox box;
						box.SetDefault();
						box.Left = FootPrint.X;
						box.Top = FootPrint.Y;
						box.Front = FootPrint.Z;
						box.Right = box.Left + FootPrint.Width;
						box.Bottom = box.Top + FootPrint.Height;
						box.Back = box.Front + FootPrint.Depth;
						cmdlist->CopyTextureRegion(mDest.UnsafeConvertTo<ITexture>(), DestSubResource, DstX,
							DstY, DstZ, mSrc.UnsafeConvertTo<ITexture>(), SrcSubResource, &box);
					}
				}
				break;
			case EngineNS::NxRHI::CDM_Buffer2Texture:
				cmdlist->CopyBufferToTexture(mDest.UnsafeConvertTo<ITexture>(), DestSubResource, 
					mSrc.UnsafeConvertTo<IBuffer>(), &FootPrint);
				break;
			case EngineNS::NxRHI::CDM_Texture2Buffer:
				cmdlist->CopyTextureToBuffer(mDest.UnsafeConvertTo<IBuffer>(), &FootPrint,
					mSrc.UnsafeConvertTo<ITexture>(), SrcSubResource);
				break;
			default:
				ASSERT(false);
				break;
		}
		cmdlist->EndEvent();

		/*mDest->TransitionTo(cmdlist, saveDst);
		mSrc->TransitionTo(cmdlist, saveSrc);*/
	}

	void IRayTracingDraw::BindShaderEffect(IRayTracingEffect* effect) 
	{
		if (ShaderEffect == effect)
			return;
		ShaderEffect = effect;
		BindResources.clear();
		auto reflector = effect->GetReflector();
		for (auto& i : reflector->CBuffers)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Srvs)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Uavs)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
		for (auto& i : reflector->Samplers)
		{
			FBindResource temp;
			temp.SetResource(nullptr);
			BindResources[i] = temp;
		}
	}
	const FShaderBinder* IRayTracingDraw::FindBinder(EShaderBindType type, const char* name) const
	{
		return ShaderEffect->FindBinder(type, name);
	}
	const FShaderBinder* IRayTracingDraw::FindBinder(const char* name) const
	{
		return ShaderEffect->FindBinder(name);
	}
	bool IRayTracingDraw::BindResource(EShaderBindType type, VNameString name, IGpuResource* resource)
	{
		auto binder = ShaderEffect->FindBinder(type, name);
		if (binder == nullptr)
			return false;

		BindResource(binder, resource);
		return true;
	}
	void IRayTracingDraw::BindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		auto iter = BindResources.find(binder);
		if (iter != BindResources.end())
		{
			if (iter->second.Resource == resource)
			{
				return;
			}
			else
			{
				auto& bs = BindResources[binder];
				bs.SetResource(resource);
				OnBindResource(binder, bs);
			}
		}
		else
		{
			FBindResource bs{};
			bs.Resource = resource;
			bs.FingerPrint = resource->GetFingerPrint();
			BindResources.insert(std::make_pair(binder, bs));
			OnBindResource(binder, bs);
		}
	}
	void IBarriersDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		for (UINT i = 0; i < (UINT)Barriers.size(); i++)
		{
			Barriers[i].Buffer->TransitionTo(cmdlist, Barriers[i].ToState);
		}
		Barriers.clear();
	}
	void IRenderPassCopyDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		for (UINT i = 0; i < (UINT)CopyDraws.size(); i++)
		{
			CopyDraws[i]->Commit(cmdlist, bRefResource);
		}
		CopyDraws.clear();
	}
}

NS_END