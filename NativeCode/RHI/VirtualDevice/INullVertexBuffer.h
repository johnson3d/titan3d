#pragma once
#include "../IVertexBuffer.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderContext;
class INullVertexBuffer : public IVertexBuffer
{
public:
	INullVertexBuffer();
	~INullVertexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;

public:
	TObjectHandle<INullRenderContext>		mRenderContext;
	
	bool Init(INullRenderContext* rc, const IVertexBufferDesc* desc);
	std::vector<BYTE>						mVertexBuffer;
};

NS_END