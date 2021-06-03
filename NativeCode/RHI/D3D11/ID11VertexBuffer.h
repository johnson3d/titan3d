#pragma once
#include "../IVertexBuffer.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class IGpuBuffer;
class ID11VertexBuffer : public IVertexBuffer
{
public:
	ID11VertexBuffer();
	~ID11VertexBuffer();
	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) override;
	
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) override;
	virtual void SetDebugInfo(const char* info) override;
public:
	std::string				mDebugInfo;
	ID3D11Buffer*			mBuffer;
public:
	bool Init(ID11RenderContext* rc, const IVertexBufferDesc* desc);
	bool Init(ID11RenderContext* rc, const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer);
};

NS_END