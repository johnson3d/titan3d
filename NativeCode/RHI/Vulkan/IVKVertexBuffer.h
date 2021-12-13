#pragma once
#include "../IVertexBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKGpuBuffer;
class IVKVertexBuffer : public IVertexBuffer
{
public:
	IVKVertexBuffer();
	~IVKVertexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;

public:
	AutoRef<IVKGpuBuffer>				mBuffer;

	bool Init(IVKRenderContext* rc, const IVertexBufferDesc* desc);
	bool Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer);
};

NS_END