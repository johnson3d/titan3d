#include "INullShaderResourceView.h"
#include "INullRenderContext.h"

#define new VNEW

NS_BEGIN

INullShaderResourceView::INullShaderResourceView()
{
	
}


INullShaderResourceView::~INullShaderResourceView()
{
	
}

bool INullShaderResourceView::UpdateTexture2D(IRenderContext* rc, const ITexture2D* pTexture2D)
{
	return true;
}

bool INullShaderResourceView::Init(INullRenderContext* rc, const IShaderResourceViewDesc* desc)
{
	mRenderContext.FromObject(rc);

	return true;
}

NS_END