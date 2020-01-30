#pragma once
#include "../IIndexBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKIndexBuffer : public IIndexBuffer
{
public:
	IVKIndexBuffer();
	~IVKIndexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkBuffer							mBuffer;
	VkDeviceMemory						mMemory;

	bool Init(IVKRenderContext* rc, const IIndexBufferDesc* desc);
};

NS_END