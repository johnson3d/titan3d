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
	
	virtual bool UpdateBuffer(IRenderContext* rc, const IGpuBuffer* buffer) override;
	virtual void* GetAPIObject() override {
		return this;
	}
	bool Init(INullRenderContext* rc, const IShaderResourceViewDesc* desc);
};

NS_END