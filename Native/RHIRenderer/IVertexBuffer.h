#pragma once
#include "IRenderResource.h"

NS_BEGIN

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct=8)
struct IVertexBufferDesc
{
	TR_DECL(IVertexBufferDesc);
	IVertexBufferDesc()
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

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
class IVertexBuffer : public IRenderResource
{
public:
	TR_DECL(IVertexBuffer);
public:
	IVertexBuffer();
	~IVertexBuffer();

	virtual void GetBufferData(EngineNS::IRenderContext* rc, IBlobObject* data) = 0;
	virtual void UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size) = 0;

	void UpdateDrawPass(ICommandList* cmd, vBOOL bImm);
	virtual void DoSwap(IRenderContext* rc) override;
	virtual void SetDebugInfo(const char* info)
	{
	}
public:
	IVertexBufferDesc		mDesc;
	/*std::vector<BYTE>		mVertices;
	bool					mHasPushed;
	UINT					GpuBufferSize;*/
};

NS_END