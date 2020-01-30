#pragma once
#include "../IShaderResourceView.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderContext;

class INullShaderResourceView : public IShaderResourceView
{
public:
	INullShaderResourceView();
	~INullShaderResourceView();

public:
	TObjectHandle<INullRenderContext>		mRenderContext;
	
	bool Init(INullRenderContext* rc, const IShaderResourceViewDesc* desc);
};

NS_END