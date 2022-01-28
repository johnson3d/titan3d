#pragma once
#include "../IDrawCall.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLCommandList;

class IGLGeometryMesh : public IGeometryMesh
{
public:
	~IGLGeometryMesh()
	{
		mVertexArray.reset();
	}
	std::shared_ptr<GLSdk::GLBufferId>				mVertexArray;
	virtual vBOOL ApplyGeometry(ICommandList* cmd, IDrawCall* pass, vBOOL bImm) override;
};

class IGLDrawCall : public IDrawCall
{
public:
	IGLDrawCall();
	~IGLDrawCall();

	virtual void Cleanup() override;

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
public:
	bool Init(IRenderContext* rc, const IDrawCallDesc* desc);
};

class IGLComputeDrawcall : public IComputeDrawcall
{
public:
	virtual void BuildPass(ICommandList* cmd) override;
	bool Init(IRenderContext* rc);
};

class IGLCopyDrawcall : public ICopyDrawcall
{
};

NS_END