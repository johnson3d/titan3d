#pragma once
#include "NxGpuDevice.h"
#include "NxShader.h"

NS_BEGIN

namespace NxRHI
{
	class IBuffer;
	class ISrView;
	class IUaView;
	class ISampler;
	class IInputLayout;
	class ICommandList;
	class IRenderPass;
	class TR_CLASS()
		FEffectBinder : public VIUnknownBase
	{
	public:
		VNameString				Name;
		EShaderBindType			BindType;
		const FShaderBinder*	VSBinder = nullptr;
		const FShaderBinder*	PSBinder = nullptr;
		const FShaderVarDesc* FindField(const char* name) const;
		UINT GetBindResourceSize() const;
		const FShaderBinder* GetShaderBinder(EShaderType type = EShaderType::SDT_Unknown) const{
			switch (type)
			{
			case SDT_Unknown:
				break;
			case SDT_VertexShader:
				return VSBinder;
			case SDT_PixelShader:
				return PSBinder;
			case SDT_ComputeShader:
				break;
			default:
				break;
			}
			if (VSBinder != nullptr)
				return VSBinder;
			else if (PSBinder != nullptr)
				return PSBinder;
			return nullptr;
		}
	};

	class TR_CLASS()
		IGpuEffect : public VIUnknownBase
	{

	};
	class TR_CLASS()
		IGraphicsEffect : public IGpuEffect
	{
	public:
		IGraphicsEffect();
		~IGraphicsEffect();
		virtual void BuildState(IGpuDevice * device) {

		}
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) {

		}
		void BindInputLayout(IInputLayout * layout);
		void BindVS(IShader* shader) {
			mVertexShader = shader;
		}
		void BindPS(IShader* shader) {
			mPixelShader = shader;
		}
		IShader* GetVS() {
			return mVertexShader;
		}
		IShader* GetPS() {
			return mPixelShader;
		}
		void LinkShaders();
		const FEffectBinder* FindBinder(const char* name) const{
			return FindBinder(VNameString(name));
		}
		const FEffectBinder* FindBinder(VNameString name) const{
			auto iter = mBinders.find(name);
			if (iter == mBinders.end())
			{
				return nullptr;
			}
			return iter->second;
		}
		void BindCBV(ICommandList* cmdlist, const FEffectBinder* binder, ICbView* buffer);
		void BindSrv(ICommandList* cmdlist, const FEffectBinder* binder, ISrView* srv);
		void BindUav(ICommandList* cmdlist, const FEffectBinder* binder, IUaView* uav);
		void BindSampler(ICommandList* cmdlist, const FEffectBinder* binder, ISampler* sampler);
	private:
		void PushBinder(EShaderType shaderType, IShaderReflector* pReflector);
		void PushBinder(EShaderType shaderType, VNameString name, AutoRef<FShaderBinder>& binder);
	public:
		AutoRef<IInputLayout>	mInputLayout;
		AutoRef<IShader>		mVertexShader;
		AutoRef<IShader>		mPixelShader;

		std::map<VNameString, AutoRef<FEffectBinder>>	mBinders;
	};

	class TR_CLASS()
		IComputeEffect : public IGpuEffect
	{
	public:
		void BindCS(IShader * shader);
		IShader* GetCS() {
			return mComputeShader;
		}
		void BindCBV(ICommandList * cmdlist, const FShaderBinder* binder, ICbView * buffer);
		void BindSrv(ICommandList * cmdlist, const FShaderBinder* binder, ISrView * srv);
		void BindUav(ICommandList * cmdlist, const FShaderBinder* binder, IUaView * uav);
		void BindSampler(ICommandList * cmdlist, const FShaderBinder* binder, ISampler * sampler);
		virtual void BuildState(IGpuDevice* device) {

		}
		virtual void Commit(ICommandList* cmdlist) {

		}
		const FShaderBinder* FindBinder(const char* name) const {
			return FindBinder(VNameString(name));
		}
		const FShaderBinder* FindBinder(VNameString name) const;
		const FShaderBinder* FindBinder(EShaderBindType type, VNameString name) const;
	public:
		AutoRef<IShader>		mComputeShader;
	};
}

NS_END