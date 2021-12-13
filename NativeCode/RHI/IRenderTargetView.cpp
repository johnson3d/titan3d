#include "IRenderTargetView.h"
#include "ITextureBase.h"
#include "IShaderResourceView.h"

#define new VNEW

NS_BEGIN

IRenderTargetView::IRenderTargetView()
{
	memset(&Desc, 0 , sizeof(Desc));
}


IRenderTargetView::~IRenderTargetView()
{	
	RefGpuBuffer.Clear();
}

NS_END
