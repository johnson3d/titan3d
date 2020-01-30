#pragma once
#include "IRenderResource.h"

NS_BEGIN

enum EIndexBufferType
{
	IBT_Int16,
	IBT_Int32,
};

struct IIndexBufferDesc
{
	IIndexBufferDesc()
	{
		CPUAccess = 0;
		ByteWidth = 0;
		InitData = nullptr;
		Type = IBT_Int16;
	}
	UINT		CPUAccess;
	UINT		ByteWidth;
	EIndexBufferType	Type;
	void*		InitData;
};

class IRenderContext;
class ICommandList;
class IIndexBuffer : public IRenderResource
{
public:
	IIndexBufferDesc	mDesc;
	//bool				mHasPushed;
	//UINT				GpuBufferSize;
	//std::vector<BYTE>	mIndexes;

	IIndexBuffer();
	~IIndexBuffer();

	void GetDesc(IIndexBufferDesc* desc) const {
		*desc = mDesc;
	}
	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) = 0;
	virtual void SetDebugInfo(const char* info)
	{

	}
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) = 0;

	virtual void DoSwap(IRenderContext* rc) override;
};

NS_END