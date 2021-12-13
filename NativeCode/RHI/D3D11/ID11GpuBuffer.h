#pragma once
#include "../IUnorderedAccessView.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11GpuBuffer : public IGpuBuffer
{
public:
	D3D11_BUFFER_DESC	mDesc;
	ID3D11Buffer*		mBuffer;
	ID3D11Buffer*		mCopyBuffer;

	ID11GpuBuffer();
	~ID11GpuBuffer();

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
};

NS_END