#pragma once
#include "../IVertexShader.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKVertexShader : public IVertexShader
{
public:
	IVKVertexShader();
	~IVKVertexShader();

	bool Init(IVKRenderContext* rc, const IShaderDesc* desc);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkShaderModule						mShader;
};

NS_END