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

bool INullShaderResourceView::Init(INullRenderContext* rc, const IShaderResourceViewDesc* desc)
{
	mRenderContext.FromObject(rc);

	return true;
}

NS_END