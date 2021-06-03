#pragma once
#include "../IVertexBuffer.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLGpuBuffer;
class IGLVertexBuffer : public IVertexBuffer
{
public:
	IGLVertexBuffer();
	~IGLVertexBuffer();
	virtual void Cleanup() override;
	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;
public:
	AutoRef<IGLGpuBuffer>	mGpuBuffer;
public:
	bool Init(IGLRenderContext* rc, const IVertexBufferDesc* desc);
	bool Init(IGLRenderContext* rc, const IVertexBufferDesc* desc, const IGLGpuBuffer* pBuffer);
};

NS_END