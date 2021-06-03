#pragma once
#include "../IInputLayout.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlInputLayout : public IInputLayout
{
public:
	MtlInputLayout();
	~MtlInputLayout();

public:
	bool Init(MtlContext* pCtx, const IInputLayoutDesc* pDesc);

public:
	MTLVertexDescriptor* m_pVtxDesc;
};

NS_END