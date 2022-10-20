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
		else if (PSBinder != nullptr)
		{
			return PSBinder->FindField(name);
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

		PushBinder(EShaderType::SDT_VertexShader, mVertexShader->Reflector);
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
}

NS_END