#pragma once
#include "../IUnorderedAccessView.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKGpuBuffer;

class IVKUnorderedAccessView : public IUnorderedAccessView
{
public:
	IVKUnorderedAccessView();
	~IVKUnorderedAccessView();
	bool Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc);
public:
	AutoRef<IVKGpuBuffer>			mBuffer;
};

NS_END