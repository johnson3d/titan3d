#include "IDepthStencilView.h"
#include "IShaderResourceView.h"

#define new VNEW

NS_BEGIN

IDepthStencilView::IDepthStencilView()
{
}


IDepthStencilView::~IDepthStencilView()
{
	m_refTexture2D.Clear();
}

NS_END
