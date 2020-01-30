#include "MTLBlendState.h"

#define new VNEW

NS_BEGIN


MtlBlendState::MtlBlendState()
{

}


MtlBlendState::~MtlBlendState()
{
}


MTLBlendOperation  TranslateBlendOperation_RHI2Mtl(EBlendOp blend_op)
{
	static const MTLBlendOperation MtlBlendOpArray[5] = 
	{
		MTLBlendOperationAdd,
		MTLBlendOperationSubtract,
		MTLBlendOperationReverseSubtract,
		MTLBlendOperationMin,
		MTLBlendOperationMax
	};
	UINT32 idx = (UINT32)blend_op - 1;
	return MtlBlendOpArray[idx];
}

MTLBlendFactor TranslateBlendFactor_RHI2Mtl(EBlend blend_factor)
{
	switch (blend_factor)
	{
	case BLD_ZERO:
		return MTLBlendFactorZero;
		break;
	case BLD_ONE:
		return MTLBlendFactorOne;
		break;
	case BLD_SRC_COLOR:
		return MTLBlendFactorSourceColor;
		break;
	case BLD_INV_SRC_COLOR:
		return MTLBlendFactorOneMinusSourceColor;
		break;
	case BLD_SRC_ALPHA:
		return MTLBlendFactorSourceAlpha;
		break;
	case BLD_INV_SRC_ALPHA:
		return MTLBlendFactorOneMinusSourceAlpha;
		break;
	case BLD_DEST_ALPHA:
		return MTLBlendFactorDestinationAlpha;
		break;
	case BLD_INV_DEST_ALPHA:
		return MTLBlendFactorOneMinusDestinationAlpha;
		break;
	case BLD_DEST_COLOR:
		return MTLBlendFactorDestinationColor;
		break;
	case BLD_INV_DEST_COLOR:
		return MTLBlendFactorOneMinusDestinationColor;
		break;
	case BLD_SRC_ALPHA_SAT:
		return MTLBlendFactorSourceAlphaSaturated;
		break;
	default:
		return MTLBlendFactorZero;
		break;
	}
}

bool MtlBlendState::Init(MtlContext* pCtx, const IBlendStateDesc* pDesc)
{
	for (UINT32 idx = 0; idx < MAX_MRT_NUM; idx++)
	{
		mMtlBlendStateDescArray[idx].mEnable = (bool)pDesc->RenderTarget[idx].BlendEnable;
		mMtlBlendStateDescArray[idx].mBlendOperationRGB = TranslateBlendOperation_RHI2Mtl(pDesc->RenderTarget[idx].BlendOp);
		mMtlBlendStateDescArray[idx].mBlendOperationAlpha = TranslateBlendOperation_RHI2Mtl(pDesc->RenderTarget[idx].BlendOpAlpha);
		mMtlBlendStateDescArray[idx].mBlendFactorSrcRGB = TranslateBlendFactor_RHI2Mtl(pDesc->RenderTarget[idx].SrcBlend);
		mMtlBlendStateDescArray[idx].mBlendFactorSrcAlpha = TranslateBlendFactor_RHI2Mtl(pDesc->RenderTarget[idx].SrcBlendAlpha);
		mMtlBlendStateDescArray[idx].mBlendFactorDstRGB = TranslateBlendFactor_RHI2Mtl(pDesc->RenderTarget[idx].DestBlend);
		mMtlBlendStateDescArray[idx].mBlendFactorDstAlpha = TranslateBlendFactor_RHI2Mtl(pDesc->RenderTarget[idx].DestBlendAlpha);
		mMtlBlendStateDescArray[idx].mWriteMask = (MTLColorWriteMask)pDesc->RenderTarget[idx].RenderTargetWriteMask;
	}

	return true;
}

NS_END