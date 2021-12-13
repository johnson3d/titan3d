#pragma once
#include "../IUnorderedAccessView.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;

class IGLGpuBuffer : public IGpuBuffer
{
public:
	std::shared_ptr<GLSdk::GLBufferId>		mBufferId;
	GLenum					mTarget;
	IGLGpuBuffer();
	~IGLGpuBuffer();

	virtual void Cleanup() override;

	virtual void* GetHWBuffer() const override {
		return (void*)&mBufferId;
	}

	bool Init(IGLRenderContext* rc, const IGpuBufferDesc* desc, void* data);

	bool Init(IGLRenderContext* rc, GLenum target, GLenum usage, void* data, UINT length);

	virtual vBOOL GetBufferData(IRenderContext* rc, IBlobObject* blob) override;

	virtual vBOOL Map(IRenderContext* rc, 
		UINT Subresource,
		EGpuMAP MapType,
		UINT MapFlags,
		IMappedSubResource* mapRes) override;
	virtual void Unmap(IRenderContext* rc, UINT Subresource) override;

	virtual vBOOL UpdateBufferData(ICommandList* cmd, UINT offset, void* data, UINT size) override;
};

class IGLUnorderedAccessView : public IUnorderedAccessView
{
public:
	IGLUnorderedAccessView();
	~IGLUnorderedAccessView();
public:
	//ID3D11UnorderedAccessView*			mView;
	AutoRef<IGLGpuBuffer>			mBuffer;

	bool Init(IGLRenderContext* rc, IGLGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc);
};

NS_END