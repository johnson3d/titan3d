#pragma once
#include "../ITextureBase.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlTexture2D : public ITexture2D
{
public:
	MtlTexture2D();
	~MtlTexture2D();

public:
	bool Init(MtlContext* pCtx, const ITexture2DDesc* pDesc);

public:
	id<MTLTexture> m_pMtlTexture2D;
};

NS_END