#pragma once
#include "NxGpuDevice.h"
#include "NxShader.h"
#include "NxEffect.h"
#include "NxBuffer.h"
#include "NxGeomMesh.h"
#include "NxRHIDefine.h"

NS_BEGIN

namespace NxRHI
{
	class FVertexArray;
	class FGeomMesh;
	class IGpuPipeline;
	class IGraphicsEffect;
	class FEffectBinder;
	class IGpuResource;
	class ICommandList;
	class IGpuDrawState;

	class TR_CLASS()
		IGpuDraw : public VIUnknown
	{
	public:
		ENGINE_RTTI(IGpuDraw);
		virtual void Commit(ICommandList * cmdlist, bool bRefResource) = 0;
		virtual UINT GetPrimitiveNum() = 0;

		typedef bool FOnVisit(EShaderBindType type, IGpuResource* resource);
		virtual void ForeachGpuResource(const std::function<FOnVisit>&fun) {

		}
		std::string					DebugName;
		inline void SetDebugName(const char* name) {
			DebugName = name;
		}
		const char* GetDebugName() {
			return DebugName.c_str();
		}
	};
	class TR_CLASS()
		IGraphicDraw : public IGpuDraw
	{
	public:
		ENGINE_RTTI(IGraphicDraw);
		IGraphicDraw()
		{
			NumOfInstance++;
		}
		~IGraphicDraw();
		const FEffectBinder* FindBinder(const char* name) const;
		bool BindResource(VNameString name, IGpuResource* resource);
		void BindResource(const FEffectBinder* binder, IGpuResource * resource);
		IGpuResource* FindGpuResource(VNameString name);
		virtual void Commit(ICommandList * cmdlist, bool bRefResource) override;
		void BindShaderEffect(IGpuDevice* device, IGraphicsEffect * effect);
		void BindPipeline(IGpuDevice * device, IGpuPipeline * pipe);
		IGraphicsEffect* GetGraphicsEffect();
		IGpuPipeline* GetPipeline();
		FGeomMesh* GetGeomMesh() {
			return Mesh;
		}
		void BindGeomMesh(IGpuDevice* device, FGeomMesh* pMesh);
		FVertexArray* GetAttachVertexArray() {
			return AttachVB;
		}
		void BindAttachVertexArray(FVertexArray* va)
		{
			AttachVB = va;
		}
		IBuffer* GetIndirectDrawArgsBuffer() {
			return IndirectDrawArgsBuffer;
		}
		void BindIndirectDrawArgsBuffer(IBuffer* buffer, UINT offset);
		const FMeshAtomDesc* GetMeshAtomDesc() {
			return Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		}
		virtual UINT GetPrimitiveNum() override{
			auto desc = GetMeshAtomDesc();
			return desc->NumPrimitives * desc->NumInstances;
		}
		static int GetNumOfInstance() {
			return NumOfInstance;
		}
		virtual void ForeachGpuResource(const std::function<FOnVisit>& fun) override
		{
			for (auto& i : BindResources)
			{
				if (fun(i.first->BindType, i.second) == false)
					return;
			}
		}
	public:
		static std::atomic<int>		NumOfInstance;
		std::map<const FEffectBinder*, AutoRef<IGpuResource>>	BindResources;
		AutoRef<IGraphicsEffect>	ShaderEffect;
		AutoRef<IGpuPipeline>		Pipeline;
		AutoRef<FGeomMesh>			Mesh;
		AutoRef<FVertexArray>		AttachVB;
		AutoRef<IBuffer>			IndirectDrawArgsBuffer;
		
		UINT						IndirectDrawOffsetForArgs = 0;
		USHORT						DrawInstance = 1;
		BYTE						MeshAtom = 0;//Maybe: bit12
		BYTE						MeshLOD = 0;//Maybe: bit6
		UINT						ViewInstanceMask = 0;

		const IGpuDrawState*		GpuDrawState = nullptr;
	protected:
		void UpdateGpuDrawState(IGpuDevice* device, ICommandList* cmdlist, IRenderPass* rpass);
		virtual void OnGpuDrawStateUpdated() {}
		virtual void OnBindResource(const FEffectBinder* binder, IGpuResource* resource) {}
	};
	class TR_CLASS()
		IComputeDraw : public IGpuDraw
	{
	public:
		ENGINE_RTTI(IComputeDraw);
		IComputeDraw()
		{
			NumOfInstance++;
		}
		~IComputeDraw()
		{
			NumOfInstance--;
		}
		virtual void Commit(ICommandList * cmdlist, bool bRefResource) override;
		void SetComputeEffect(IComputeEffect * effect) {
			mEffect = effect;
		}
		IComputeEffect* GetComputeEffect() {
			return mEffect;
		}
		void SetDispatch(UINT x, UINT y, UINT z) {
			mDispatchX = x;
			mDispatchY = y;
			mDispatchZ = z;
		}
		void BindIndirectDispatchArgsBuffer(IBuffer* buffer) {
			IndirectDispatchArgsBuffer = buffer;
		}
		const FShaderBinder* FindBinder(EShaderBindType type, const char* name) const;
		bool BindResource(EShaderBindType type, VNameString name, IGpuResource* resource);
		void BindResource(const FShaderBinder* binder, IGpuResource* resource);
		IGpuResource* FindGpuResource(EShaderBindType type, VNameString name);

		virtual UINT GetPrimitiveNum() override{
			return mDispatchX * mDispatchY * mDispatchZ;
		}
		virtual void ForeachGpuResource(const std::function<FOnVisit>& fun) override
		{
			for (auto& i : BindResources)
			{
				if (fun(i.first->Type, i.second) == false)
					return;
			}
		}
	public:
		static int GetNumOfInstance() {
			return NumOfInstance;
		}
		static std::atomic<int>		NumOfInstance;
		UINT					mDispatchX = 0;
		UINT					mDispatchY = 0;
		UINT					mDispatchZ = 0;
		AutoRef<IComputeEffect>	mEffect;
		AutoRef<IBuffer>		IndirectDispatchArgsBuffer;
		std::map<const FShaderBinder*, AutoRef<IGpuResource>>	BindResources;
	protected:
		virtual void OnBindResource(const FShaderBinder* binder, IGpuResource* resource) {}
	};
	enum TR_ENUM()
		ECopyDrawMode
	{
		CDM_Buffer2Buffer,
			CDM_Texture2Texture,
			CDM_Buffer2Texture,
			CDM_Texture2Buffer,
	};
	class TR_CLASS()
		ICopyDraw : public IGpuDraw
	{
	public:
		ENGINE_RTTI(ICopyDraw);
		ICopyDraw()
		{
			NumOfInstance++;
		}
		~ICopyDraw()
		{
			NumOfInstance--;
		}
		virtual void Commit(ICommandList * cmdlist, bool bRefResource) override;
		void BindBufferSrc(IBuffer* res);
		void BindBufferDest(IBuffer* res);
		void BindTextureSrc(ITexture* res);
		void BindTextureDest(ITexture* res);
		
		virtual UINT GetPrimitiveNum() override{
			return 1;
		}
		FSubResourceFootPrint* GetFootPrint() {
			return &FootPrint;
		}
	public:
		static int GetNumOfInstance() {
			return NumOfInstance;
		}
		static std::atomic<int>		NumOfInstance;
		ECopyDrawMode Mode = ECopyDrawMode::CDM_Buffer2Buffer;
		AutoRef<IGpuBufferData>	mSrc;
		AutoRef<IGpuBufferData>	mDest;
		UINT SrcSubResource = 0;
		UINT DestSubResource = 0;

		UINT DstX = 0;
		UINT DstY = 0;
		UINT DstZ = 0;
		
		FSubResourceFootPrint FootPrint{};
	};

	class TR_CLASS()
		IActionDraw : public IGpuDraw
	{
	public:
		TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
		typedef void(*FOnActionDraw)(ICommandList* cmdlist, void* arg);
		FOnActionDraw OnActionDraw = nullptr;
		void* Arg = nullptr;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override
		{
			if (OnActionDraw != nullptr)
			{
				OnActionDraw(cmdlist, Arg);
			}
		}
		virtual UINT GetPrimitiveNum() override { return 0; }
	};
}

NS_END