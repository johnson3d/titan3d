#pragma once
#include "../IDrawCall.h"
#include "VKPreHead.h"
#include "IVKShaderProgram.h"
#include "IVKGpuBuffer.h"
#include "IVKShaderResourceView.h"

NS_BEGIN

class IVKCommandList;
class IVKGpuBuffer;
class IVKShaderResourceView;
class IVKDrawCall : public IDrawCall
{
public:
	IVKDrawCall();
	~IVKDrawCall();

	virtual void BuildPass(ICommandList* cmd, vBOOL bImmCBuffer) override;

	virtual void SetViewport(ICommandList* cmd, IViewPort* vp) override;
	virtual void SetScissorRect(ICommandList* cmd, IScissorRect* sr) override;

	virtual void SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline, EPrimitiveType dpType) override;

	virtual void SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) override;
	virtual void SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer) override;

	virtual void VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) override;
	virtual void PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer) override;
	virtual void VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) override;
	virtual void PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture) override;
	virtual void VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) override;
	virtual void PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler) override;
	
	virtual void DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;
	virtual void DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;
	virtual void DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) override;

	virtual vBOOL ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer) override;

	bool Init(IVKRenderContext* rc, const IDrawCallDesc* desc);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	enum ESetStage
	{
		STS_VS = 0,
		STS_PS = 1,
		//STS_CS = 2,
		
		STS_NUM,
	};
	struct VDescriptorSets
	{
		VDescriptorSets()
		{
			mDescriptorSet[STS_VS] = nullptr;
			mDescriptorSet[STS_PS] = nullptr;
		}
		void Init(IVKRenderContext* rc, IVKDrawCall* dr);
		void Cleanup(IVKRenderContext* rc, IVKShaderProgram* program);
		VkDescriptorSet						mDescriptorSet[STS_NUM];
		UINT								mCurrentFrame;
	};
	struct VDescriptorSetsManager : public VIUnknownBase
	{
		std::map<UINT64, VDescriptorSets*>	mCachedStates;
	};
	
	AutoRef<VDescriptorSetsManager>		mStateManager;
	Hash64									mCurVkSetStateHash;
	VDescriptorSets*					mCurVkSetState;
private:
	void UpdateLayoutSet(IVKRenderContext* rc);
};

class IVKComputeDrawcall : public IComputeDrawcall
{
public:
	virtual void BuildPass(ICommandList* cmd) override;
	bool Init(IVKRenderContext* rc);
};

class IVKCopyDrawcall : public ICopyDrawcall
{

};

NS_END