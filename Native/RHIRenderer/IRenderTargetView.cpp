#include "IRenderTargetView.h"
#include "ITextureBase.h"
#include "IShaderResourceView.h"

#define new VNEW

NS_BEGIN

IRenderTargetView::IRenderTargetView()
{
}


IRenderTargetView::~IRenderTargetView()
{	
	m_refTexture2D.Clear();
}

NS_END

using namespace EngineNS;
extern "C"
{
	CSharpReturnAPI0(ITexture2D*, EngineNS, IRenderTargetView, GetTexture2D);
}