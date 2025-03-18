#include "NxEffect.h"
#include "NxCommandList.h"
#include "NxShader.h"
#include "NxInputAssembly.h"
#include "NxFrameBuffers.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	const FShaderVarDesc* FEffectBinder::FindField(const char* name) const
	{
		if (VSBinder != nullptr)
		{
			return VSBinder->FindField(name);
		}
		if (PSBinder != nullptr)
		{
			return PSBinder->FindField(name);
		}
		if (ASBinder != nullptr)
		{
			return ASBinder->FindField(name);
		}
		if (MSBinder != nullptr)
		{
			return MSBinder->FindField(name);
		}
		return nullptr;
	}
	UINT FEffectBinder::GetBindResourceSize() const
	{
		if (VSBinder != nullptr)
		{
			return VSBinder->Size;
		}
		else if (PSBinder != nullptr)
		{
			return PSBinder->Size;
		}
		else if (ASBinder != nullptr)
		{
			return ASBinder->Size;
		}
		else if (MSBinder != nullptr)
		{
			return MSBinder->Size;
		}
		return 0;
	}
	IGraphicsEffect::IGraphicsEffect()
	{

	}
	IGraphicsEffect::~IGraphicsEffect()
	{

	}
	void IGraphicsEffect::BindInputLayout(IInputLayout* va)
	{
		mInputLayout = va;
	}
	void IGraphicsEffect::LinkShaders()
	{
		mBinders.clear();

		if (mAmplificationShader != nullptr)
			PushBinder(EShaderType::SDT_MeshShader, mAmplificationShader->Reflector);
		if (mMeshShader != nullptr)
			PushBinder(EShaderType::SDT_MeshShader, mMeshShader->Reflector);
		if (mVertexShader != nullptr)
			PushBinder(EShaderType::SDT_VertexShader, mVertexShader->Reflector);
		if (mPixelShader != nullptr)
			PushBinder(EShaderType::SDT_PixelShader, mPixelShader->Reflector);
	}
	void IGraphicsEffect::PushBinder(EShaderType shaderType, IShaderReflector* pReflector)
	{
		if (pReflector == nullptr)
			return;
		for (auto& i : pReflector->CBuffers)
		{
			PushBinder(shaderType, i->Name, i);
		}
		for (auto& i : pReflector->Uavs)
		{
			PushBinder(shaderType, i->Name, i);
		}
		for (auto& i : pReflector->Srvs)
		{
			PushBinder(shaderType, i->Name, i);
		}
		for (auto& i : pReflector->Samplers)
		{
			PushBinder(shaderType, i->Name, i);
		}
	}
	void IGraphicsEffect::PushBinder(EShaderType shaderType, VNameString name, AutoRef<FShaderBinder>& binder)
	{
		AutoRef<FEffectBinder> eb;
		auto iter = mBinders.find(name);
		if (iter == mBinders.end())
		{
			eb = MakeWeakRef(new FEffectBinder());
			eb->Name = name;
			eb->BindType = binder->Type;
			mBinders.insert(std::make_pair(name, eb));
		}
		else
		{
			eb = iter->second;
		}
		ASSERT(eb->BindType == binder->Type);

		switch (shaderType)
		{
			case SDT_VertexShader:
				eb->VSBinder = binder;
				break;
			case SDT_PixelShader:
				eb->PSBinder = binder;
				break;
			case SDT_ComputeShader:
				break;
			case SDT_AmplificationShader:
				eb->ASBinder = binder;
				break;
			case SDT_MeshShader:
				eb->MSBinder = binder;
				break;
			default:
				break;
		}
	}
	void IGraphicsEffect::BindCBV(ICommandList* cmdlist, const FEffectBinder* binder, ICbView* buffer)
	{
		if (binder->VSBinder != nullptr)
		{
			cmdlist->SetCBV(EShaderType::SDT_VertexShader, binder->VSBinder, buffer);
		}
		if (binder->PSBinder != nullptr)
		{
			cmdlist->SetCBV(EShaderType::SDT_PixelShader, binder->PSBinder, buffer);
		}
		if (binder->ASBinder != nullptr)
		{
			cmdlist->SetCBV(EShaderType::SDT_AmplificationShader, binder->ASBinder, buffer);
		}
		if (binder->MSBinder != nullptr)
		{
			cmdlist->SetCBV(EShaderType::SDT_MeshShader, binder->MSBinder, buffer);
		}
	}
	void IGraphicsEffect::BindSrv(ICommandList* cmdlist, const FEffectBinder* binder, ISrView* srv)
	{
		if (binder->VSBinder != nullptr)
		{
			cmdlist->SetSrv(EShaderType::SDT_VertexShader, binder->VSBinder, srv);
		}
		if (binder->PSBinder != nullptr)
		{
			cmdlist->SetSrv(EShaderType::SDT_PixelShader, binder->PSBinder, srv);
		}
		if (binder->ASBinder != nullptr)
		{
			cmdlist->SetSrv(EShaderType::SDT_AmplificationShader, binder->ASBinder, srv);
		}
		if (binder->MSBinder != nullptr)
		{
			cmdlist->SetSrv(EShaderType::SDT_MeshShader, binder->MSBinder, srv);
		}
	}
	void IGraphicsEffect::BindUav(ICommandList* cmdlist, const FEffectBinder* binder, IUaView* uav)
	{
		if (binder->VSBinder != nullptr)
		{
			cmdlist->SetUav(EShaderType::SDT_VertexShader, binder->VSBinder, uav);
		}
		if (binder->PSBinder != nullptr)
		{
			cmdlist->SetUav(EShaderType::SDT_PixelShader, binder->PSBinder, uav);
		}
		if (binder->ASBinder != nullptr)
		{
			cmdlist->SetUav(EShaderType::SDT_AmplificationShader, binder->ASBinder, uav);
		}
		if (binder->MSBinder != nullptr)
		{
			cmdlist->SetUav(EShaderType::SDT_MeshShader, binder->MSBinder, uav);
		}
	}
	void IGraphicsEffect::BindSampler(ICommandList* cmdlist, const FEffectBinder* binder, ISampler* sampler)
	{
		if (binder->VSBinder != nullptr)
		{
			cmdlist->SetSampler(EShaderType::SDT_VertexShader, binder->VSBinder, sampler);
		}
		if (binder->PSBinder != nullptr)
		{
			cmdlist->SetSampler(EShaderType::SDT_PixelShader, binder->PSBinder, sampler);
		}
		if (binder->ASBinder != nullptr)
		{
			cmdlist->SetSampler(EShaderType::SDT_AmplificationShader, binder->ASBinder, sampler);
		}
		if (binder->MSBinder != nullptr)
		{
			cmdlist->SetSampler(EShaderType::SDT_MeshShader, binder->MSBinder, sampler);
		}
	}

	void IComputeEffect::BindCS(IShader* shader) 
	{
		mComputeShader = shader;
	}
	const FShaderBinder* IComputeEffect::FindBinder(VNameString name) const 
	{
		auto pReflector = mComputeShader->GetReflector();
		auto pBinder = pReflector->FindBinder(EShaderBindType::SBT_CBuffer, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_SRV, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_UAV, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_Sampler, name);
		if (pBinder != nullptr)
			return pBinder;
		return nullptr;
	}
	const FShaderBinder* IComputeEffect::FindBinder(EShaderBindType type, VNameString name) const
	{
		auto pReflector = mComputeShader->GetReflector();
		return pReflector->FindBinder(type, name);
	}
	void IComputeEffect::BindCBV(ICommandList* cmdlist, const FShaderBinder* binder, ICbView* buffer)
	{
		cmdlist->SetCBV(EShaderType::SDT_ComputeShader, binder, buffer);
	}
	void IComputeEffect::BindSrv(ICommandList* cmdlist, const FShaderBinder* binder, ISrView* srv)
	{
		cmdlist->SetSrv(EShaderType::SDT_ComputeShader, binder, srv);
	}
	void IComputeEffect::BindUav(ICommandList* cmdlist, const FShaderBinder* binder, IUaView* uav)
	{
		cmdlist->SetUav(EShaderType::SDT_ComputeShader, binder, uav);
	}
	void IComputeEffect::BindSampler(ICommandList* cmdlist, const FShaderBinder* binder, ISampler* sampler)
	{
		cmdlist->SetSampler(EShaderType::SDT_ComputeShader, binder, sampler);
	}

	void FHitGroup::CountShaderBufferSize(IShaderReflector * pReflector)
	{
		ShaderBufferSize = 0;
		for (auto& i : LocalSignatures)
		{
			auto binder = pReflector->FindBinder(EShaderBindType::SBT_CBuffer, i);
			if (binder != nullptr)
			{
				ShaderBufferSize += binder->Size;
			}
		}
	}
	FHitGroup* IRayTracingEffect::CreateHitGroup()
	{
		return new FHitGroup();
	}
	bool IRayTracingEffect::AddHitGroup(VNameString name, VNameString anyHit, VNameString closestHit, VNameString intersection, VNameString* sigs, int count)
	{
		if (FindHitGroup(name) != nullptr)
			return false;
		auto group = MakeWeakRef(CreateHitGroup());
		group->HitGroupIndex = (int)mHitGroups.size();
		group->Name = name;
		group->AnyHit = anyHit;
		group->ClosestHit = closestHit;
		group->Intersection = intersection;
		for(int i=0; i<count; i++)
		{
			group->LocalSignatures.push_back(sigs[i]);
		}
		/*if (this->BuildHitGroup(group) == false)
			return false;*/
		mHitGroups.push_back(group);
		return true;
	}
	void IRayTracingEffect::SaveGlobalAndHitGroups(XndAttribute* attr)
	{
		attr->mVersion = 0;
		attr->BeginWrite();
		UINT count = (UINT)mGlobalSignatures.size();
		attr->Write(count);
		for (UINT i = 0; i < count; i++)
		{
			attr->Write(mGlobalSignatures[i]);
		}

		count = (UINT)mHitGroups.size();
		attr->Write(count);
		for (auto& i : mHitGroups)
		{
			attr->Write(i->Name);
			attr->Write(i->AnyHit);
			attr->Write(i->ClosestHit);
			attr->Write(i->Intersection);
			count = (UINT)i->LocalSignatures.size();
			attr->Write(count);
			for (UINT j = 0; j < count; j++)
			{
				attr->Write(i->LocalSignatures[j]);
			}
		}
		attr->EndWrite();
	}
	void IRayTracingEffect::LoadGlobalAndHitGroups(XndAttribute* attr)
	{
		attr->BeginRead();
		mGlobalSignatures.clear();
		UINT count;
		attr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			VNameString t;
			attr->Read(t);
			mGlobalSignatures.push_back(t);
		}

		mHitGroups.clear();
		attr->Read(count);
		for (UINT i = 0; i < count; i++)
		{
			VNameString t;
			auto e = MakeWeakRef(this->CreateHitGroup());
			attr->Read(t);
			e->Name = t;
			attr->Read(t);
			e->AnyHit = t;
			attr->Read(t);
			e->ClosestHit = t;
			attr->Read(t);
			e->Intersection = t;
			UINT count1;
			attr->Read(count1);
			for (UINT j = 0; j < count1; j++)
			{
				e->AnyHit = t;
				e->LocalSignatures.push_back(t);
			}
			this->mHitGroups.push_back(e);
		}
		attr->EndRead();
	}
	const FShaderBinder* IRayTracingEffect::FindBinder(VNameString name) const
	{
		auto pReflector = GetReflector();
		auto pBinder = pReflector->FindBinder(EShaderBindType::SBT_CBuffer, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_SRV, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_UAV, name);
		if (pBinder != nullptr)
			return pBinder;
		pBinder = pReflector->FindBinder(EShaderBindType::SBT_Sampler, name);
		if (pBinder != nullptr)
			return pBinder;
		return nullptr;
	}
	const FShaderBinder* IRayTracingEffect::FindBinder(EShaderBindType type, VNameString name) const
	{
		auto pReflector = GetReflector();
		return pReflector->FindBinder(type, name);
	}
}

NS_END