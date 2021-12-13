#include "IVKRenderTargetView.h"
#include "IVKRenderContext.h"
#include "IVKTexture2D.h"
#include "IVKShaderResourceView.h"

#define new VNEW

NS_BEGIN

IVKRenderTargetView::IVKRenderTargetView()
{
}

IVKRenderTargetView::~IVKRenderTargetView()
{
}

bool IVKRenderTargetView::Init(IVKRenderContext* rc, const IRenderTargetViewDesc* desc)
{
	IShaderResourceViewDesc srvDesc;
	if (desc->Type == RTV_Texture2D)
	{
		srvDesc.SetTexture2D();
		srvDesc.mGpuBuffer = desc->mGpuBuffer;
		srvDesc.Format = desc->Format;
		switch (desc->ViewDimension)
		{
		case EngineNS::RTV_DIMENSION_UNKNOWN:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_UNKNOWN;
			break;
		case EngineNS::RTV_DIMENSION_BUFFER:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_BUFFER;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE1D:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE1D;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE1DARRAY:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE1DARRAY;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE2D:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE2D;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE2DARRAY:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE2DARRAY;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE2DMS:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE2DMS;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE2DMSARRAY:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE2DMSARRAY;
			break;
		case EngineNS::RTV_DIMENSION_TEXTURE3D:
			srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE3D;
			break;
		default:
			break;
		}		

		srvDesc.Texture2D.MipLevels = 1;
		//srvDesc.Texture2D.MipLevels = desc->Texture2D.MipSlice;
	}
	auto pSRV = new IVKShaderResourceView();
	pSRV->Init(rc, &srvDesc);
	mTextureSRV.WeakRef(pSRV);
	return true;
}

NS_END