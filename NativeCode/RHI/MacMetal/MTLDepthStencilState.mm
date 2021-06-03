#include "MTLDepthStencilState.h"

NS_BEGIN

	MTLCompareFunction TranslateCompareFunc_RHI2Mtl(EComparisionMode CM)
	{
		switch (CM)
		{
		case EComparisionMode::CMP_NEVER:
			return MTLCompareFunctionNever;
			break;
		case EComparisionMode::CMP_LESS:
			return MTLCompareFunctionLess;
			break;
		case EComparisionMode::CMP_EQUAL:
			return MTLCompareFunctionEqual;
			break;
		case EComparisionMode::CMP_LESS_EQUAL:
			return MTLCompareFunctionLessEqual;
			break;
		case EComparisionMode::CMP_GREATER:
			return MTLCompareFunctionGreater;
			break;
		case EComparisionMode::CMP_NOT_EQUAL:
			return MTLCompareFunctionNotEqual;
			break;
		case EComparisionMode::CMP_GREATER_EQUAL:
			return MTLCompareFunctionGreaterEqual;
			break;
		case EComparisionMode::CMP_ALWAYS:
			return MTLCompareFunctionAlways;
			break;
		default:
			return MTLCompareFunctionLess;
			break;
		}
	}

	MTLStencilOperation TranslateStencilOperation_RHI2Mtl(EStencilOp SO)
	{
		switch (SO)
		{
		case EStencilOp::STOP_KEEP:
			return MTLStencilOperationKeep;
			break;
		case EStencilOp::STOP_ZERO:
			return MTLStencilOperationZero;
			break;
		case EStencilOp::STOP_REPLACE:
			return MTLStencilOperationReplace;
			break;
		case EStencilOp::STOP_INCR_SAT:
			return MTLStencilOperationIncrementClamp;
			break;
		case EStencilOp::STOP_DECR_SAT:
			return MTLStencilOperationDecrementClamp;
			break;
		case EStencilOp::STOP_INVERT:
			return MTLStencilOperationInvert;
			break;
		case EStencilOp::STOP_INCR:
			return MTLStencilOperationIncrementWrap;
			break;
		case EStencilOp::STOP_DECR:
			return MTLStencilOperationDecrementWrap;
			break;
		default:
			return MTLStencilOperationKeep;
			break;
		}
	}


MtlDepthStencilState::MtlDepthStencilState()
{
	m_pDepthStencilState = nil;
	mStencilRefValue = 0;
}

MtlDepthStencilState::~MtlDepthStencilState()
{
	
}

bool MtlDepthStencilState::Init(MtlContext* pCtx, const IDepthStencilStateDesc* pDesc)
{
	MTLDepthStencilDescriptor* pMtlDSDesc = [MTLDepthStencilDescriptor new];
	pMtlDSDesc.depthWriteEnabled = pDesc->DepthEnable;
	pMtlDSDesc.depthCompareFunction = TranslateCompareFunc_RHI2Mtl(pDesc->DepthFunc);

	MTLStencilDescriptor* pMtlFrontFaceStencilDesc = nil;
	MTLStencilDescriptor* pMtlBackFaceStencilDesc = nil;

	if (pDesc->StencilEnable == TRUE)
	{
		pMtlFrontFaceStencilDesc = [MTLStencilDescriptor new];
		pMtlFrontFaceStencilDesc.stencilCompareFunction = TranslateCompareFunc_RHI2Mtl(pDesc->FrontFace.StencilFunc);
		pMtlFrontFaceStencilDesc.stencilFailureOperation = TranslateStencilOperation_RHI2Mtl(pDesc->FrontFace.StencilFailOp);
		pMtlFrontFaceStencilDesc.depthFailureOperation = TranslateStencilOperation_RHI2Mtl(pDesc->FrontFace.StencilDepthFailOp);
		pMtlFrontFaceStencilDesc.depthStencilPassOperation = TranslateStencilOperation_RHI2Mtl(pDesc->FrontFace.StencilPassOp);
		pMtlFrontFaceStencilDesc.readMask = pDesc->StencilReadMask;
		pMtlFrontFaceStencilDesc.writeMask = pDesc->StencilWriteMask;

		pMtlBackFaceStencilDesc = [MTLStencilDescriptor new];
		pMtlBackFaceStencilDesc.stencilCompareFunction = TranslateCompareFunc_RHI2Mtl(pDesc->BackFace.StencilFunc);
		pMtlBackFaceStencilDesc.stencilFailureOperation = TranslateStencilOperation_RHI2Mtl(pDesc->BackFace.StencilFailOp);
		pMtlBackFaceStencilDesc.depthFailureOperation = TranslateStencilOperation_RHI2Mtl(pDesc->BackFace.StencilDepthFailOp);
		pMtlBackFaceStencilDesc.depthStencilPassOperation = TranslateStencilOperation_RHI2Mtl(pDesc->BackFace.StencilPassOp);
		pMtlBackFaceStencilDesc.readMask = pDesc->StencilReadMask;
		pMtlBackFaceStencilDesc.writeMask = pDesc->StencilWriteMask;

		mStencilRefValue = pDesc->StencilRef;
	}

	pMtlDSDesc.frontFaceStencil = pMtlFrontFaceStencilDesc;
	pMtlDSDesc.backFaceStencil = pMtlBackFaceStencilDesc;
	pMtlDSDesc.label = [NSString stringWithFormat : @"MetalDepthStecilState"];

	m_pDepthStencilState = [pCtx->m_pDevice newDepthStencilStateWithDescriptor: pMtlDSDesc];
	
	[pMtlDSDesc release];
	pMtlDSDesc = nil;

	if (pDesc->StencilEnable == TRUE)
	{
		[pMtlFrontFaceStencilDesc release];
		pMtlFrontFaceStencilDesc = nil;
		[pMtlBackFaceStencilDesc release];
		pMtlBackFaceStencilDesc = nil;
	}

	return true;
}

NS_END

