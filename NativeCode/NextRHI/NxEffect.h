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
		FEffectBinder : public VIUnknown
	{
	public:
		VNameString				Name;
		EShaderBindType			BindType;
		const FShaderBinder*	ASBinder = nullptr;
		const FShaderBinder*	MSBinder = nullptr;
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
			case SDT_AmplificationShader:
				return ASBinder;
			case SDT_MeshShader:
				return MSBinder;
			default:
				break;
			}
			if (VSBinder != nullptr)
				return VSBinder;
			else if (PSBinder != nullptr)
				return PSBinder;
			else if (ASBinder != nullptr)
				return ASBinder;
			else if (MSBinder != nullptr)
				return MSBinder;
			return nullptr;
		}
	};

	class TR_CLASS()
		IGpuEffect : public IGpuResource
	{
	public:
		std::string		mDebugName;
		const char* GetDebugName() {
			return mDebugName.c_str();
		}
		virtual void SetDebugName(const char* n) override{
			mDebugName = n;
		}
	};
	class TR_CLASS()
		IGraphicsEffect : public IGpuEffect
	{
	public:
		IGraphicsEffect();
		~IGraphicsEffect();
		void SetIdentifier(const char* id)
		{
			Identifier = id;
		}
		virtual void BuildState(IGpuDevice * device) {

		}
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) {

		}
		void BindInputLayout(IInputLayout * layout);
		void BindAS(IShader* shader) {
			mAmplificationShader = shader;
		}
		void BindMS(IShader* shader) {
			mMeshShader = shader;
		}
		void BindVS(IShader* shader) {
			mVertexShader = shader;
		}
		void BindPS(IShader* shader) {
			mPixelShader = shader;
		}
		IShader* GetAS() {
			return mAmplificationShader;
		}
		IShader* GetMS() {
			return mMeshShader;
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
		std::string				Identifier;
		AutoRef<IInputLayout>	mInputLayout;
		AutoRef<IShader>		mAmplificationShader;
		AutoRef<IShader>		mMeshShader;
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

	class TR_CLASS()
		FHitGroup : public VIUnknown
	{
	public:
		int					HitGroupIndex = -1;
		VNameString			Name;
		VNameString			AnyHit;
		VNameString			ClosestHit;
		VNameString			Intersection;
		
		UINT				ShaderBufferSize = 0;	
		AutoRef<IBuffer>	ShaderRecord;

		std::vector<VNameString> LocalSignatures;
		AutoRef<IShaderReflector> LocalReflector;

		bool IsLocalShaderBinder(const FShaderBinder* binder) const{
			auto r = LocalReflector->FindBinder(binder->Type, binder->Name);
			if (r == nullptr)
				return false;
			return true;
		}

		void CountShaderBufferSize(IShaderReflector* pReflector);
	};

	class TR_CLASS()
		IRayTracingEffect : public IGpuEffect
	{
	public:
		AutoRef<FShaderDesc>	mShaderLibDesc;

		std::vector<VNameString> mFunctions;
		std::vector<VNameString> mGlobalSignatures;
		std::vector<AutoRef<FHitGroup>> mHitGroups;
		AutoRef<IShaderReflector> mGlobalReflector;

		VNameString				mRayGenName;
		VNameString				mMissName;

		UINT					mMaxRecursionDepth = 1;
		UINT					mPayloadSize = 4 * sizeof(float);
		UINT					mAttributeSize = 2 * sizeof(float);

		bool IsGlobalShaderBinder(const FShaderBinder* binder) const {
			auto r = mGlobalReflector->FindBinder(binder->Type, binder->Name);
			if (r == nullptr)
				return false;
			return true;
		}

		void SetRayGenShader(VNameString name) {
			mRayGenName = name;
		}
		void SetMissShader(VNameString name) {
			mMissName = name;
		}
		void AddFunctions(VNameString name) {
			mFunctions.push_back(name);
		}
		void AddGlobalSignature(VNameString name) {
			mGlobalSignatures.push_back(name);
		}

		FHitGroup* FindHitGroup(VNameString name) {
			for (auto& i : mHitGroups)
			{
				if (i->Name == name)
				{
					return i;
				}
			}
			return nullptr;
		}
		bool AddHitGroup(VNameString name, VNameString anyHit, VNameString closestHit, VNameString intersection, VNameString* sigs, int count);

		void SaveGlobalAndHitGroups(XndAttribute* attr);
		void LoadGlobalAndHitGroups(XndAttribute* attr);

		FShaderDesc* GetShaderLibDesc() {
			return mShaderLibDesc;
		}
		void SetShaderLibDesc(FShaderDesc* desc) {
			mShaderLibDesc = desc;
		}
		virtual void BuildState(IGpuDevice* device)
		{
		}
		virtual bool BuildHitGroup(FHitGroup* group)
		{
			return false;
		}
		virtual FHitGroup* CreateHitGroup();

		virtual const IShaderReflector* GetReflector() const { return nullptr; }
		
		const FShaderBinder* FindBinder(const char* name) const {
			return FindBinder(VNameString(name));
		}
		const FShaderBinder* FindBinder(VNameString name) const;
		const FShaderBinder* FindBinder(EShaderBindType type, VNameString name) const;
	};
}

NS_END