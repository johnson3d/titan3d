#pragma once
#include "../IConstantBuffer.h"
#include "NullPreHead.h"

NS_BEGIN

class ITextureBase;
class INullRenderContext;
class INullConstantBuffer : public IConstantBuffer
{
public:
	INullConstantBuffer();
	~INullConstantBuffer();

	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) override;
};

NS_END