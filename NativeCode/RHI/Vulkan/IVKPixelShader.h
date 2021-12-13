#pragma once
#include "../IPixelShader.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKPixelShader : public IPixelShader
{
public:
	IVKPixelShader();
	~IVKPixelShader();
	bool Init(IVKRenderContext* rc, const IShaderDesc* desc);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkShaderModule						mShader;
};

NS_END