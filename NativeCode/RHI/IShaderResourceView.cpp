#include "IShaderResourceView.h"
#include "ITextureBase.h"
#include "Utility/GfxTextureStreaming.h"
#include "IUnorderedAccessView.h"

#define new VNEW

NS_BEGIN

IShaderResourceView::IShaderResourceView()
{
	
}

IShaderResourceView::~IShaderResourceView()
{
	mBuffer.Clear();
}


EPixelFormat IShaderResourceView::GetFormat()
{
	return mSrvDesc.Format;
}

NS_END
