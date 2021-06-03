#pragma once
#include "../IConstantBuffer.h"
#include "GLPreHead.h"

NS_BEGIN

class ITextureBase;

class IGLRenderContext;
class IGLConstantBuffer : public IConstantBuffer
{
public:
	IGLConstantBuffer();
	~IGLConstantBuffer();
	virtual void Cleanup() override;

	//virtual void UpdateDrawPass(ICommandList* cmd) override;
	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) override;
public:
	std::shared_ptr<GLSdk::GLBufferId>		mBuffer;
public:
	bool Init(IGLRenderContext* rc, const IConstantBufferDesc* desc);
};

NS_END