#include "ID11FrameBuffers.h"
#include "ID11RenderTargetView.h"
#include "ID11DepthStencilView.h"

#define new VNEW

NS_BEGIN

ID11FrameBuffers::ID11FrameBuffers()
{
}

ID11FrameBuffers::~ID11FrameBuffers()
{
	
}

bool ID11FrameBuffers::Init(ID11RenderContext* rc, const IFrameBuffersDesc* desc)
{
	mDesc = *desc;
	return true;
}

NS_END