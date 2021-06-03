#pragma once
#include "../IIndexBuffer.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLGpuBuffer;
class IGLIndexBuffer : public IIndexBuffer
{
public:
	IGLIndexBuffer();
	~IGLIndexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void Cleanup() override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;
public:
	AutoRef<IGLGpuBuffer>	mGpuBuffer;
public:
	bool Init(IGLRenderContext* rc, const IIndexBufferDesc* desc);
	bool Init(IGLRenderContext* rc, const IIndexBufferDesc* desc, const IGLGpuBuffer* pBuffer);
};

NS_END