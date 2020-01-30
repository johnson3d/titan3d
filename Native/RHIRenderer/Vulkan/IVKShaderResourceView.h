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
	VkImageView							mImageView;
	VkImage								mImage;

	bool Init(IVKRenderContext* rc, const IShaderResourceViewDesc* desc);
	bool Init(IVKRenderContext* rc, VkImage pBuffer, const ISRVDesc* desc);
};

NS_END