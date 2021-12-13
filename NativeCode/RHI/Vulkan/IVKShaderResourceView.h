#pragma once
#include "../IShaderResourceView.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;

class IVKShaderResourceView : public IShaderResourceView
{
public:
	IVKShaderResourceView();
	~IVKShaderResourceView();

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	union
	{
		VkImageView						mImageView;
		VkBufferView					mBufferView;
	};
	bool Init(IVKRenderContext* rc, const IShaderResourceViewDesc* desc);

	virtual bool UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer) override;
	virtual void* GetAPIObject() override;
};

NS_END