#pragma once
#include "../IIndexBuffer.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderContext;
class INullIndexBuffer : public IIndexBuffer
{
public:
	INullIndexBuffer();
	~INullIndexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;
	
	bool Init(INullRenderContext* rc, const IIndexBufferDesc* desc);

	std::vector<BYTE>						mIndexBuffer;
};

NS_END