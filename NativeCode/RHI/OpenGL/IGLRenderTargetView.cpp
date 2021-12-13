#include "IGLRenderTargetView.h"
#include "IGLRenderContext.h"
#include "IGLTexture2D.h"
#include "IGLShaderResourceView.h"
#include "IGLUnorderedAccessView.h"

#define new VNEW

NS_BEGIN

IGLRenderTargetView::IGLRenderTargetView()
{
}

IGLRenderTargetView::~IGLRenderTargetView()
{
}

void IGLRenderTargetView::Cleanup()
{
}

bool IGLRenderTargetView::Init(IGLRenderContext* rc, const IRenderTargetViewDesc* pDesc)
{
	RefGpuBuffer.StrongRef(pDesc->mGpuBuffer);

	return true;
}

bool IGLRenderTargetView::Init(IGLRenderContext* rc, IGLGpuBuffer* pBuffer, const IRenderTargetViewDesc* desc)
{
	RefGpuBuffer.StrongRef(pBuffer);

	return true;
}

NS_END