#include "MTLRasterizerState.h"

#define new VNEW

NS_BEGIN

MTLTriangleFillMode TranslateFillMode_RHI2Mtl(EFillMode fill_mode)
{
	switch (fill_mode)
	{
	case EFillMode::FMD_WIREFRAME:
		return MTLTriangleFillModeLines;
		break;
	case EFillMode::FMD_SOLID:
		return MTLTriangleFillModeFill;
			break;
	default:
		return MTLTriangleFillModeFill;
		break;
	}
}

MTLCullMode TranslateCullMode_RHI2Mtl(ECullMode cull_mode)
{
	static const MTLCullMode CullModes[3] = { MTLCullModeNone, MTLCullModeFront, MTLCullModeBack };
	UINT32 idx = (UINT32)cull_mode - 1;
	return CullModes[idx];
}

MTLWinding TranslateFrontFaceWinding_RHI2Mtl(vBOOL FrontCounterClockwise)
{
	return 	FrontCounterClockwise == FALSE ? MTLWindingClockwise : MTLWindingCounterClockwise;
}

MtlRasterState::MtlRasterState()
{
	mFillMode = MTLTriangleFillModeFill;
	mCullMode = MTLCullModeBack;
	mFrontFaceWinding = MTLWindingClockwise;
	mDepthBias = 0.0f;
	mDepthBiasClamp = 0.0f;
	mSlopeScaledDepthBias = 0.0f;
}


MtlRasterState::~MtlRasterState()
{
}

bool MtlRasterState::Init(MtlContext* pCtx, const IRasterizerStateDesc* pDesc)
{
	mFillMode = TranslateFillMode_RHI2Mtl(pDesc->FillMode);
	mCullMode = TranslateCullMode_RHI2Mtl(pDesc->CullMode);
	mFrontFaceWinding = TranslateFrontFaceWinding_RHI2Mtl(pDesc->FrontCounterClockwise);
	mDepthBias = pDesc->DepthBias;
	mDepthBiasClamp = pDesc->DepthBiasClamp;
	mSlopeScaledDepthBias = pDesc->SlopeScaledDepthBias;
	return true;
}

NS_END