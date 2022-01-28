#pragma once
#include "PreHead.h"
#include "../Base/thread/vfxcritical.h"
#include "../Base/generic/vfxarray.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8, SV_Manual)
	BUFFER_SRV
{
	union
	{
		UINT FirstElement;
		UINT ElementOffset;
	};
	union
	{
		UINT NumElements;
		UINT ElementWidth;
	};
};

class XndAttribute;
class IRenderContext;

struct TR_CLASS()
	IBlobObject : public VIUnknown
{
	friend class TTASTSD;
	ENGINE_RTTI(IBlobObject);
	VArray<BYTE, BYTE>		mDatas;

	IBlobObject()
	{

	}

	UINT GetSize() const {
		return (UINT)mDatas.GetSize();
	}
	void* GetData() {
		if (mDatas.GetSize() == 0)
			return nullptr;
		return mDatas.GetData();
	}
	void ReadFromXnd(XndAttribute * attr);
	void Write2Xnd(XndAttribute * attr);

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

class TR_CLASS()
	IRenderResource : public VIUnknownBase
{
public:
	IRenderResource();
	~IRenderResource();
	virtual void Cleanup();

	virtual void OnFrameEnd(IRenderContext * rc) {}

	virtual void SetDebugName(const char* name) {}

	virtual IResourceState* GetResourceState() {
		return nullptr;
	}
	virtual void InvalidateResource() {
		return;
	}
	virtual vBOOL RestoreResource() {
		return TRUE;
	}
};

NS_END