#pragma once

#include "../IUnknown.h"
#include "../debug/vfxdebug.h"
#include "../generic/vfxarray.h"

NS_BEGIN

class TR_CLASS()
CsValueList : public IWeakReference
{
public:
	ENGINE_RTTI(CsValueList);
	
	UINT					mStride;
	std::vector<BYTE>		mMemData;
	
	CsValueList()
	{
		mStride = 0;
	}
	CsValueList(int stride);
	~CsValueList();
	void SetCapacity(int capacity);
	int GetCapacity();
	void SetSize(int size);
	UINT GetCount();
	TR_FUNCTION(SV_NoStarToRef = ptr)
	void AddValue(BYTE* ptr);
	TR_FUNCTION(SV_NoStarToRef = src)
	void Append(CsValueList* src);
	TR_FUNCTION(SV_NoStarToRef = src)
	void AppendArray(BYTE* src, int count);
	void RemoveAt(UINT index);
	void InsertAt(UINT index, BYTE* ptr);
	void Clear(vBOOL bFreeMemory);
	BYTE* GetAddressAt(UINT index);
	TR_FUNCTION(SV_NoStarToRef = ptr)
	void SetDatas(BYTE* ptr, int countOfObj);
};

template<typename _Type>
struct CSList
{
	CsValueList			mImpl;
	CSList()
		: mImpl(sizeof(_Type))
	{

	}
	UINT GetCount() {
		return mImpl.GetCount();
	}
	void SetCapacity(int capacity)
	{
		mImpl.SetCapacity(capacity);
	}
	void AddValue(const _Type& v)
	{
		mImpl.AddValue((BYTE*)&v);
	}
	void Append(CSList<_Type>& src)
	{
		mImpl.Append(&src.mImpl);
	}
	void AppendArray(_Type* src, int count)
	{
		mImpl.AppendArray((BYTE*)src, count);
	}
	void RemoveAt(UINT index)
	{
		mImpl.RemoveAt(index);
	}
	void Clear(vBOOL bFreeMemory)
	{
		mImpl.Clear(bFreeMemory);
	}
};

class TR_CLASS()
	CsQueue : public IWeakReference
{
protected:
	TR_MEMBER()
	UINT					mStride;
	std::queue<BYTE>		mDatas;
public:
	TR_CONSTRUCTOR()
	CsQueue()
	{
		mStride = 0;
	}
	ENGINE_RTTI(CsQueue);
	TR_FUNCTION()
	UINT GetCount() {
		return (UINT)mDatas.size() / mStride;
	}
	TR_FUNCTION(SV_NoStarToRef = p)
	void Enqueue(BYTE* p);
	TR_FUNCTION()
	void Dequeue();
	TR_FUNCTION(SV_NoStarToRef = pData)
	vBOOL Peek(BYTE* pData);
	TR_FUNCTION()
	void Clear();
};

NS_END
