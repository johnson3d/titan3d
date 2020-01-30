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

	virtual vBOOL GetBufferData(IRenderContext* rc, IBlobObject* blob);

	virtual vBOOL Map(IRenderContext* rc, 
		UINT Subresource,
		EGpuMAP MapType,
		UINT MapFlags,
		IMappedSubResource* mapRes);
	virtual void Unmap(IRenderContext* rc, UINT Subresource);

	virtual vBOOL UpdateBufferData(ICommandList* cmd, void* data, UINT size);
};

class ID11UnorderedAccessView : public IUnorderedAccessView
{
public:
	ID11UnorderedAccessView();
	~ID11UnorderedAccessView();
public:
	ID3D11UnorderedAccessView*			mView;

	bool Init(ID11RenderContext* rc, ID3D11Buffer* pBuffer, const IUnorderedAccessViewDesc* desc);
};

NS_END