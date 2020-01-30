#pragma once
#include "../IVertexBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKVertexBuffer : public IVertexBuffer
{
public:
	IVKVertexBuffer();
	~IVKVertexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkBuffer							mBuffer;
	VkDeviceMemory						mMemory;

	bool Init(IVKRenderContext* rc, const IVertexBufferDesc* desc);
};

NS_END