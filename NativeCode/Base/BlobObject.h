#pragma once

#include "IUnknown.h"

NS_BEGIN

class XndAttribute;

struct TR_CLASS()
	IBlobObject : public VIUnknown
{
	ENGINE_RTTI(IBlobObject);
	std::vector<BYTE>		mDatas;

	IBlobObject()
	{

	}

	UINT GetSize() const {
		return (UINT)mDatas.size();
	}
	void* GetData() {
		if (mDatas.size() == 0)
			return nullptr;
		return & mDatas[0];
	}
	void ReadFromXnd(XndAttribute * attr);
	void Write2Xnd(XndAttribute * attr);

	void ReSize(UINT size)
	{
		mDatas.resize(size);
	}
	void PushData(const void* data, UINT size)
	{
		auto oldSize = mDatas.size();
		mDatas.resize(oldSize + size);
		memcpy(&mDatas[oldSize], data, size);
	}
};

NS_END

