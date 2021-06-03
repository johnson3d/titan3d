#pragma once
#include "../IFrameBuffers.h"

NS_BEGIN

class MtlContext;

class MtlFrameBuffer : public IFrameBuffers
{
public:
	MtlFrameBuffer();
	~MtlFrameBuffer();

public:
	bool Init(MtlContext* pCtx, const IFrameBuffersDesc* pDesc);
};

NS_END