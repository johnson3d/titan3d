#pragma once
#include "../IConstantBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

class ITextureBase;
class IVKRenderContext;
class IVKConstantBuffer : public IConstantBuffer
{
public:
	IVKConstantBuffer();
	~IVKConstantBuffer();

	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) override;

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkBuffer							mBuffer;
	VKGpuMemory*						mMemory;

	bool Init(IVKRenderContext* rc, const IConstantBufferDesc* desc);
};

NS_END