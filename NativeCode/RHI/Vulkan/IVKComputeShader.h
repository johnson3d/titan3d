#pragma once
#include "../IComputeShader.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKComputeShader : public IComputeShader
{
public:
	IVKComputeShader();
	~IVKComputeShader();
	bool Init(IVKRenderContext* rc, const IShaderDesc* desc);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkShaderModule						mShader;
};

NS_END