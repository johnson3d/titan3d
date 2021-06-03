#pragma once
#include "PreHead.h"
#include "../Base/thread/vfxcritical.h"
#include "../Base/generic/vfxarray.h"

NS_BEGIN

class XndAttribute;
class IRenderContext;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IBlobObject : public VIUnknown
{
	RTTI_DEF(IBlobObject, 0x90eb57e75ba44773, true);
	VArray<BYTE, BYTE>		mDatas;
	
	TR_CONSTRUCTOR()
	IBlobObject()
	{

	}

	TR_FUNCTION()
	UINT GetSize() const {
		return (UINT)mDatas.GetSize();
	}
	TR_FUNCTION()
	void* GetData() {
		if (mDatas.GetSize() == 0)
			return nullptr;
		return mDatas.GetData();
	}
	TR_FUNCTION()
	void ReadFromXnd(XndAttribute* attr);
	TR_FUNCTION()
	void Write2Xnd(XndAttribute* attr);

	TR_FUNCTION()
	void ReSize(UINT size)
	{
		mDatas.SetSize(size);
	}
	TR_FUNCTION()
	void PushData(const void* data, UINT size)
	{
		int oldSize = mDatas.GetSize();
		mDatas.SetSize(oldSize + size);
		memcpy(&mDatas[oldSize], data, size);
	}
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IRenderResource : public VIUnknown
{
public:
	IRenderResource();
	~IRenderResource();

	virtual void DeleteThis() override;
	virtual void DoSwap(IRenderContext* rc) {}

	TR_FUNCTION()
	virtual void SetDebugName(const char* name) {}
};

class RResourceSwapChain
{
protected:
	std::queue<IRenderResource*>	mResources;
	VCritical						mLocker;
	static RResourceSwapChain		Instance;
public:
	static RResourceSwapChain* GetInstance() {
		return &Instance;
	}
	void Cleanup();
	void PushResource(IRenderResource* res);
	void TickSwap(IRenderContext* rc);
};

NS_END