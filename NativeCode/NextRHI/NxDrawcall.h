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
		IGpuDraw : public VIUnknownBase
	{
	public:
		ENGINE_RTTI(IGpuDraw);
		virtual void Commit(ICommandList * cmdlist) = 0;
	};
	class TR_CLASS()
		IGraphicDraw : public IGpuDraw
	{
	public:
		ENGINE_RTTI(IGraphicDraw);
		const FEffectBinder* FindBinder(const char* name) const;
		bool BindResource(VNameString name, IGpuResource* resource);
		void BindResource(const FEffectBinder* binder, IGpuResource * resource);
		IGpuResource* FindGpuResource(VNameString name);
		virtual void Commit(ICommandList * cmdlist) override;
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
	public:
		std::map<const FEffectBinder*, AutoRef<IGpuResource>>	BindResources;
		AutoRef<IGraphicsEffect>		ShaderEffect;
		AutoRef<IGpuPipeline>		Pipeline;
		AutoRef<FGeomMesh>			Mesh;
		AutoRef<FVertexArray>		AttachVB;
		AutoRef<IBuffer>			IndirectDrawArgsBuffer;
		
		UINT						IndirectDrawOffsetForArgs = 0;
		USHORT						DrawInstance = 1;
		BYTE						MeshAtom = 0;//Maybe: bit12
		BYTE						MeshLOD = 0;//Maybe: bit6

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
		virtual void Commit(ICommandList * cmdlist) override;
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
	public:
		UINT					mDispatchX = 0;
		UINT					mDispatchY = 0;
		UINT					mDispatchZ = 0;
		AutoRef<IComputeEffect>	mEffect;
		AutoRef<IBuffer>		IndirectDispatchArgsBuffer;
		std::map<const FShaderBinder*, AutoRef<IGpuResource>>	BindResources;
	protected:
		virtual void OnBindResource(const FShaderBinder* binder, IGpuResource* resource) {}
	};
	class TR_CLASS()
		ICopyDraw : public IGpuDraw
	{
	public:
		ENGINE_RTTI(ICopyDraw);
		virtual void Commit(ICommandList * cmdlist) override;
		void BindSrc(IGpuBufferData * res);
		void BindDest(IGpuBufferData * res);
	public:
		AutoRef<IGpuBufferData>	mSrc;
		AutoRef<IGpuBufferData>	mDest;
	};
}

NS_END