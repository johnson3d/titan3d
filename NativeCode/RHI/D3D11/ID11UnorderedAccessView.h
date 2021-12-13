#pragma once
#include "../IUnorderedAccessView.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11GpuBuffer;

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