#pragma once
#include "PreHead.h"
#include "../Core/thread/vfxcritical.h"
#include "../Core/generic/vfxarray.h"

NS_BEGIN

class XNDAttrib;
class IRenderContext;

struct IBlobObject : public VIUnknown
{
	RTTI_DEF(IBlobObject, 0x90eb57e75ba44773, true);
	VArray<BYTE, BYTE>		mDatas;
	
	UINT GetSize() const {
		return (UINT)mDatas.GetSize();
	}
	void* GetData() {
		return mDatas.GetData();
	}
	void ReadFromXnd(XNDAttrib* attr);
	void Write2Xnd(XNDAttrib* attr);

	void ReSize(UINT size)
	{
		mDatas.SetSize(size);
	}
	void PushData(const void* data, UINT size)
	{
		int oldSize = mDatas.GetSize();
		mDatas.SetSize(oldSize + size);
		memcpy(&mDatas[oldSize], data, size);
	}
};

class IRenderResource : public RHIUnknown
{
public:
	IRenderResource();
	~IRenderResource();

	virtual void DeleteThis() override;
	virtual void DoSwap(IRenderContext* rc) {}

	virtual void* GetBuffer() { 
		return nullptr; 
	}
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
	void PushResource(IRenderResource* res);
	void TickSwap(IRenderContext* rc);
};

NS_END