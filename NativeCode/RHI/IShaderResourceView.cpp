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
	mTexture2D.Clear();
}


EPixelFormat IShaderResourceView::GetTextureFormat()
{
	if (mTexture2D == nullptr)
	{
		return PXF_UNKNOWN;
	}

	return mTexture2D->mDesc.Format;
}

NS_END
