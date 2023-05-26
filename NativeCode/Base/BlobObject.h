#pragma once

#include "IUnknown.h"

NS_BEGIN

class XndAttribute;

struct TR_CLASS()
	IBlobObject : public IWeakReference
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
		if (data != nullptr)
			memcpy(&mDatas[oldSize], data, size);
	}
	template<typename T>
	void SetValueToOffset(UINT offset, T v)
	{
		memcpy(&mDatas[offset], &v, sizeof(T));
	}
};

class TR_CLASS()
	FBitset : public IWeakReference
{
	UINT mBitCount;
	std::vector<BYTE> mData;
public:
	FBitset()
	{
		mBitCount = 0;
	}
	UINT GetBitCount() {
		return mBitCount;
	}
	BYTE* GetDataPtr() {
		return &mData[0];
	}
	UINT GetDataByteSize() {
		return (UINT)mData.size();
	}
	void SetBitCount(UINT Count)
	{
		mBitCount = Count;
		if (Count % 8 == 0)
			mData.resize(Count / 8);
		else
			mData.resize(Count / 8 + 1);
	}
	void SetBit(UINT index)
	{
		if (index >= mBitCount)
			return;
		UINT a = index / 8;
		int b = (int)index % 8;
		mData[a] |= (BYTE)(1 << b);
	}
	void UnsetBit(UINT index)
	{
		if (index >= mBitCount)
			return;
		UINT a = index / 8;
		int b = (int)index % 8;
		BYTE v = ((BYTE)(1 << b));
		mData[a] &= (BYTE)(~v);
	}
	bool IsSet(UINT index)
	{
		if (index >= mBitCount)
			return false;
		UINT a = index / 8;
		int b = (int)index % 8;
		return (mData[a] & (UINT)(1 << b)) != 0;
	}
	void Clear()
	{
		for (int i = 0; i < mData.size(); i++)
		{
			mData[i] = 0;
		}
	}
};

NS_END

