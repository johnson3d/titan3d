#pragma once
#include "../IIndexBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKGpuBuffer;
class IVKIndexBuffer : public IIndexBuffer
{
public:
	IVKIndexBuffer();
	~IVKIndexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;

public:
	AutoRef<IVKGpuBuffer>				mBuffer;

	bool Init(IVKRenderContext* rc, const IIndexBufferDesc* desc);
	bool Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer);
};

NS_END