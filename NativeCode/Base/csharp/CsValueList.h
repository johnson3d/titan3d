#pragma once

#include "../IUnknown.h"
#include "../debug/vfxdebug.h"
#include "../generic/vfxarray.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
CsValueList : public VIUnknown
{
public:
	RTTI_DEF(CsValueList, 0xcadd61c75cb69de9, true);
	
	TR_MEMBER()
	UINT					mStride;
	VArray<BYTE, BYTE>		mMemData;

	TR_CONSTRUCTOR()
	CsValueList()
	{
		mStride = 0;
	}
	TR_CONSTRUCTOR()
	CsValueList(int stride);
	~CsValueList();
	TR_FUNCTION()
	void SetCapacity(int capacity);
	TR_FUNCTION()
	UINT GetCount();
	TR_FUNCTION(SV_NoStarToRef = ptr)
	void AddValue(BYTE* ptr);
	TR_FUNCTION(SV_NoStarToRef = src)
	void Append(CsValueList* src);
	TR_FUNCTION(SV_NoStarToRef = src)
	void AppendArray(BYTE* src, int count);
	TR_FUNCTION()
	void RemoveAt(UINT index);
	TR_FUNCTION()
	void Clear(vBOOL bFreeMemory);
	TR_FUNCTION()
	BYTE* GetAddressAt(UINT index);
	TR_FUNCTION(SV_NoStarToRef = ptr)
	void SetDatas(BYTE* ptr, int countOfObj);
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
CsQueue : public VIUnknown
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
	RTTI_DEF(CsQueue, 0xc7506ea55cddfde3, true);
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
