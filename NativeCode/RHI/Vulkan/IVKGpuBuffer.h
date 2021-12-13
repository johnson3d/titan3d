#pragma once
#include "../IUnorderedAccessView.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;

class IVKGpuBuffer : public IGpuBuffer
{
public:
	IVKGpuBuffer();
	~IVKGpuBuffer();

	virtual void* GetHWBuffer() const override {
		return mBuffer;
	}

	virtual vBOOL GetBufferData(IRenderContext* rc, IBlobObject* blob);

	virtual vBOOL Map(IRenderContext* rc, 
		UINT Subresource,
		EGpuMAP MapType,
		UINT MapFlags,
		IMappedSubResource* mapRes);
	virtual void Unmap(IRenderContext* rc, UINT Subresource);

	virtual vBOOL UpdateBufferData(ICommandList* cmd, UINT offset, void* data, UINT size);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkBuffer							mBuffer;
	VKGpuMemory*						mMemory;
	//VkDescriptorSetLayout				mLayout;

	bool Init(IVKRenderContext* rc, const IGpuBufferDesc* desc);
};

NS_END