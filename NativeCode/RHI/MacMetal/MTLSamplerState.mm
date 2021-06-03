#include "MTLSamplerState.h"

#define new VNEW

NS_BEGIN

MTLSamplerMinMagFilter TranslateMinMagFilter_RHI2Mtl(ESamplerFilter samp_filter)
{
	if (samp_filter == ESamplerFilter::SPF_MIN_MAG_MIP_POINT || samp_filter == ESamplerFilter::SPF_MIN_MAG_POINT_MIP_LINEAR)
	{
		return MTLSamplerMinMagFilterNearest;
	}
	else
	{
		return MTLSamplerMinMagFilterLinear;
	}
}

MTLSamplerMipFilter TranslateMipFilter_RHI2Mtl(ESamplerFilter samp_filter)
{
	if (samp_filter == ESamplerFilter::SPF_MIN_MAG_MIP_POINT || samp_filter == ESamplerFilter::SPF_MIN_MAG_LINEAR_MIP_POINT)
	{
		return MTLSamplerMipFilterNearest;
	}
	else
	{
		return MTLSamplerMipFilterLinear;
	}
}

MTLCompareFunction TranslateSamplerCompareFunc_RHI2Mtl(EComparisionMode CM)
{
	static const MTLCompareFunction MtlCompareFuncArray[8] =
	{
		MTLCompareFunctionNever,
		MTLCompareFunctionLess,
		MTLCompareFunctionEqual,
		MTLCompareFunctionLessEqual,
		MTLCompareFunctionGreater,
		MTLCompareFunctionNotEqual,
		MTLCompareFunctionGreaterEqual,
		MTLCompareFunctionAlways
	};
	UINT32 idx = (UINT32)CM - 1;
	return MtlCompareFuncArray[idx];
}

MTLSamplerAddressMode TranslateAddressMode_RHI2Mtl(EAddressMode address_mode)
{
	static const MTLSamplerAddressMode MtlSamplerAddressModeArray[5] =
	{
		MTLSamplerAddressModeRepeat,
		MTLSamplerAddressModeMirrorRepeat,
		MTLSamplerAddressModeClampToEdge,
		MTLSamplerAddressModeClampToEdge, //use clamp to instead;
		MTLSamplerAddressModeClampToEdge
		//MTLSamplerAddressModeClampToBorderColor, //not supported for ios;
		//MTLSamplerAddressModeMirrorClampToEdge, // not supported for ios;
	};
	UINT32 idx = (UINT32)address_mode - 1;
	return MtlSamplerAddressModeArray[idx];
}

//MTLSamplerBorderColor TranslateBorderColor_RHI2Mtl(const ISamplerStateDesc* pDesc)
//{
//	if (pDesc->BorderColor[0] != 0)
//	{
//		return MTLSamplerBorderColorOpaqueWhite;
//	}
//	else if (pDesc->BorderColor[3] != 0)
//	{
//		return MTLSamplerBorderColorOpaqueBlack;
//	}
//	else
//	{
//		return MTLSamplerBorderColorTransparentBlack;
//	}
//}



MtlSamplerState::MtlSamplerState()
{
	m_pSamplerState = nil;
}

MtlSamplerState::~MtlSamplerState()
{
}

bool MtlSamplerState::Init(MtlContext* pCtx, const ISamplerStateDesc* pDesc)
{
	MTLSamplerDescriptor* pMtlSampDesc = [[MTLSamplerDescriptor alloc] init];
	pMtlSampDesc.minFilter = TranslateMinMagFilter_RHI2Mtl(pDesc->Filter);
	pMtlSampDesc.magFilter = TranslateMinMagFilter_RHI2Mtl(pDesc->Filter);
	pMtlSampDesc.mipFilter = TranslateMipFilter_RHI2Mtl(pDesc->Filter);
	pMtlSampDesc.sAddressMode = TranslateAddressMode_RHI2Mtl(pDesc->AddressU);
	pMtlSampDesc.tAddressMode = TranslateAddressMode_RHI2Mtl(pDesc->AddressV);
	pMtlSampDesc.rAddressMode = TranslateAddressMode_RHI2Mtl(pDesc->AddressW);
	pMtlSampDesc.maxAnisotropy = 1; //we dont want anisotropy for ios for now;
	m_pSamplerState = [pCtx->m_pDevice newSamplerStateWithDescriptor : pMtlSampDesc];

	[pMtlSampDesc release];
	pMtlSampDesc = nil;
	return true;
}

NS_END