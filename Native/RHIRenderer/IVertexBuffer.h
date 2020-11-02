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

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS&Titan3D, AAA = "", BBB = 1)
class IVertexBuffer : public IRenderResource
{
public:
	TR_CLASS(SV_NameSpace = EngineNS.IVertexBuffer, SV_UsingNS = EngineNS, SV_ReflectAll, AAA = "", BBB = 2)
	class ClassInIVertexBuffer
	{
		const static int ttt = 9;
		volatile IRenderContext* Member1; 
		void UpdateDrawPass(ICommandList* cmd, vBOOL bImm) = 0;
		public:
		TR_MEMBER()
			float Member0;
		TR_MEMBER()
			int Member1;
		protected:
			int Member2;
		public:
			int Member3;
	} ;

	TR_CONSTRUCTOR()
	IVertexBuffer();
	~IVertexBuffer();

	TR_MEMBER()
	int Member0;
	TR_MEMBER()
	volatile IRenderContext* Member1; 

	TR_FUNCTION()
	virtual void GetBufferData(EngineNS::IRenderContext* rc, IBlobObject* data) = 0;
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