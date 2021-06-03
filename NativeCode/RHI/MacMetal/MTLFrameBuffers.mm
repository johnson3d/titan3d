#include "MTLFrameBuffers.h"
#include "MTLRenderTargetView.h"
#include "MTLDepthStencilView.h"

#define new VNEW

NS_BEGIN

MtlFrameBuffer::MtlFrameBuffer()
{
	
}

MtlFrameBuffer::~MtlFrameBuffer()
{
	
}

bool MtlFrameBuffer::Init(MtlContext* pCtx, const IFrameBuffersDesc* pDesc)
{
	mDesc = *pDesc;
	return true;
}

NS_END