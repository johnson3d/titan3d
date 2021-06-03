#pragma once
#include "IRenderResource.h"

NS_BEGIN

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EIndexBufferType
{
	IBT_Int16,
	IBT_Int32,
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IIndexBufferDesc
{
	TR_FUNCTION()
	void SetDefault()
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
class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IIndexBuffer : public IRenderResource
{
public:
	TR_MEMBER()
	IIndexBufferDesc	mDesc;
	//bool				mHasPushed;
	//UINT				GpuBufferSize;
	//std::vector<BYTE>	mIndexes;

	IIndexBuffer();
	~IIndexBuffer();

	TR_FUNCTION()
	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) = 0;
	TR_FUNCTION()
	virtual void SetDebugInfo(const char* info)
	{

	}
	TR_FUNCTION()
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) = 0;

	virtual void DoSwap(IRenderContext* rc) override;
};

NS_END