#pragma once
#include "../IConstantBuffer.h"
#include "D11PreHead.h"

NS_BEGIN

class ITextureBase;
class ID11RenderContext;

class ID11ConstantBuffer : public IConstantBuffer
{
public:
	ID11ConstantBuffer();
	~ID11ConstantBuffer();

	//virtual void UpdateDrawPass(ICommandList* cmd) override;
	virtual bool UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size) override;
public:
	ID3D11Buffer*			mBuffer;
public:
	bool Init(ID11RenderContext* rc, const IConstantBufferDesc* desc);
};

NS_END