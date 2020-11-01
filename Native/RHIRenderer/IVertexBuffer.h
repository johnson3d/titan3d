#pragma once
#include "IRenderResource.h"

NS_BEGIN

struct IVertexBufferDesc
{
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

TR_CLASS(AAA = "", BBB = 1, SV_UsingNS = EngineNS&Titan3D)
class IVertexBuffer : public IRenderResource
{
public:
	TR_CLASS(AAA = "", BBB = 2, SV_NameSpace = EngineNS.IVertexBuffer)
	class ClassInIVertexBuffer
	{
		TR_MEMBER()
			int Member0;
		TR_MEMBER()
			volatile IRenderContext* Member1;
	} ;

	TR_CONSTRUCTOR()
	IVertexBuffer();
	~IVertexBuffer();

	TR_MEMBER()
	int Member0;
	TR_MEMBER()
	volatile IRenderContext* Member1;

	TR_FUNCTION()
	virtual void GetBufferData(IRenderContext* rc, IBlobObject* data) = 0;
	TR_FUNCTION()
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