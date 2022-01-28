#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct=8)
IVertexBufferDesc
{
	TR_FUNCTION()
	void SetDefault()
	{
		CPUAccess = 0;
		ByteWidth = 0;
		Stride = 0;
		InitData = nullptr;
	}
	UINT			CPUAccess;
	UINT			ByteWidth;
	UINT			Stride;
	void*			InitData;
};

class IRenderContext;
class ICommandList;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IVertexBuffer : public IRenderResource
{
public:
	TR_DECL(IVertexBuffer);
public:
	IVertexBuffer();
	~IVertexBuffer();

	TR_FUNCTION()
	virtual void GetBufferData(EngineNS::IRenderContext* rc, IBlobObject* data) = 0;
	TR_FUNCTION()
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) = 0;

	virtual void SetDebugInfo(const char* info)
	{
	}
public:
	TR_MEMBER()
	IVertexBufferDesc		mDesc;
	/*std::vector<BYTE>		mVertices;
	bool					mHasPushed;
	UINT					GpuBufferSize;*/
};

NS_END