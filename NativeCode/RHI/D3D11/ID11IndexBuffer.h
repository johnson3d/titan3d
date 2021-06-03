#pragma once
#include "../IIndexBuffer.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class IGpuBuffer;
class ID11IndexBuffer : public IIndexBuffer
{
public:
	ID11IndexBuffer();
	~ID11IndexBuffer();

	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	virtual void SetDebugInfo(const char* info) override;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;
public:
	std::string			mDebugInfo;
	ID3D11Buffer*		mBuffer;
public:
	bool Init(ID11RenderContext* rc, const IIndexBufferDesc* desc);
	bool Init(ID11RenderContext* rc, const IIndexBufferDesc* desc, const IGpuBuffer* pBuffer);
};

NS_END