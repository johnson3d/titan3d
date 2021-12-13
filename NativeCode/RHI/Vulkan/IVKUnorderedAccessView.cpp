#include "IVKUnorderedAccessView.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"
#include "IVKGpuBuffer.h"

#define new VNEW

NS_BEGIN

IVKUnorderedAccessView::IVKUnorderedAccessView()
{
	
}

IVKUnorderedAccessView::~IVKUnorderedAccessView()
{
	
}

bool IVKUnorderedAccessView::Init(IVKRenderContext* rc, IVKGpuBuffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	mDesc = *desc;
	mBuffer.StrongRef(pBuffer);
	return true;
}

NS_END

