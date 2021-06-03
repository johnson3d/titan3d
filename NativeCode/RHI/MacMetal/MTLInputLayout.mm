#include "MTLInputLayout.h"
#include"./../IConstantBuffer.h"

#define new VNEW

NS_BEGIN

MTLVertexFormat TranslateVtxFmt_RHI2Mtl(EPixelFormat vtx_fmt)
{
	switch (vtx_fmt)
	{
	case EPixelFormat::PXF_R32G32B32_FLOAT:
		return MTLVertexFormatFloat3;
		break;
	case EPixelFormat::PXF_R32G32B32A32_FLOAT:
		return MTLVertexFormatFloat4;
		break;
	case EPixelFormat::PXF_R8G8B8A8_UNORM:
		return MTLVertexFormatUChar4Normalized;
		break;
	case EPixelFormat::PXF_R8G8B8A8_UINT:
		return MTLVertexFormatUChar4;
		break;
	case EPixelFormat::PXF_R32G32_FLOAT:
		return MTLVertexFormatFloat2;
		break;
	case EPixelFormat::PXF_UNKNOWN:
		return MTLVertexFormatInvalid;
		break;
	default:
		return MTLVertexFormatInvalid;
		break;
	}
}

UINT32 GetMtlVtxFmtSize_Byte(MTLVertexFormat VF)
{
	switch (VF)
	{
	case MTLVertexFormatFloat3:
		return 12;
		break;
	case MTLVertexFormatFloat4:
		return 16;
		break;
	case MTLVertexFormatUChar4Normalized:
		return 4;
		break;
	case MTLVertexFormatUChar4:
		return 4;
		break;
	case MTLVertexFormatFloat2:
		return 8;
		break;
	case MTLVertexFormatInvalid:
		return 0;
		break;
	default:
		return 0;
		break;
	}
}

MtlInputLayout::MtlInputLayout()
{
	m_pVtxDesc = nil;
}

MtlInputLayout::~MtlInputLayout()
{
}

bool MtlInputLayout::Init(MtlContext* pCtx, const IInputLayoutDesc* pDesc)
{
	m_pVtxDesc = [MTLVertexDescriptor vertexDescriptor];
	UINT32 LayoutIdx = 0;
	for (UINT32 idx = 0; idx < pDesc->Layouts.size(); idx++)
	{
		m_pVtxDesc.attributes[idx].format = TranslateVtxFmt_RHI2Mtl(pDesc->Layouts[idx].Format);
		m_pVtxDesc.attributes[idx].bufferIndex = pDesc->Layouts[idx].InputSlot + MaxConstBufferNum; //[[stage_in]]
		m_pVtxDesc.attributes[idx].offset = pDesc->Layouts[idx].AlignedByteOffset;

		LayoutIdx = (UINT32)m_pVtxDesc.attributes[idx].bufferIndex;
		m_pVtxDesc.layouts[LayoutIdx].stepFunction = pDesc->Layouts[idx].IsInstanceData == 0 ? MTLVertexStepFunctionPerVertex : MTLVertexStepFunctionPerInstance;
		m_pVtxDesc.layouts[LayoutIdx].stepRate = 1;
		m_pVtxDesc.layouts[LayoutIdx].stride = GetMtlVtxFmtSize_Byte(m_pVtxDesc.attributes[idx].format);
	}

	return true;
}


NS_END