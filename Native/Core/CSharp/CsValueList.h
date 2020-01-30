#pragma once

#include "../../BaseHead.h"
#include "../debug/vfxdebug.h"
#include "../generic/vfxarray.h"

NS_BEGIN

class CsValueList : public VIUnknown
{
public:
	RTTI_DEF(CsValueList, 0xcadd61c75cb69de9, true);

	UINT					mStride;
	VArray<BYTE, BYTE>		mMemData;

	VDef_ReadWrite(UINT, Stride, m);

	CsValueList();
	~CsValueList();
	void SetCapacity(int capacity);
	UINT GetCount();
	void AddValue(BYTE* ptr);
	void Append(CsValueList* src);
	
	void RemoveAt(UINT index);
	void Clear(UINT size = 0);
	BYTE* GetAddressAt(UINT index);
	void SetDatas(BYTE* ptr, int countOfObj);
};

class CsStreamWriter : public VIUnknown
{
public:
	RTTI_DEF(CsStreamWriter, 0x7eebcfce5cd90c29, true);
	VArray<BYTE, BYTE>		mDatas;

	CsStreamWriter();
	~CsStreamWriter();
	virtual void Cleanup() override;
	void Reserve(UINT size);
	void PushData(BYTE* p, UINT size);
	void Clear();
	BYTE* GetPointer();
	UINT GetSize();
};

class CsStreamReader : public VIUnknown
{
public:
	RTTI_DEF(CsStreamReader, 0x2852973c5cd90d42, true);
	VArray<BYTE, BYTE>		mDatas;
	UINT					mPos;

	CsStreamReader();
	~CsStreamReader();
	virtual void Cleanup() override;
	void InitData(BYTE* p, UINT size);
	BYTE* GetPointer();
	UINT GetSize();
	vBOOL Read(BYTE* p, UINT size);
	void ResetReader();
	UINT GetPosition();
};

class CsQueue : public VIUnknown
{
protected:
	UINT					mStride;
	std::queue<BYTE>		mDatas;
public:
	CsQueue()
	{
		mStride = 0;
	}
	RTTI_DEF(CsQueue, 0xc7506ea55cddfde3, true);
	VDef_ReadWrite(UINT, Stride, m);
	UINT GetCount() {
		return (UINT)mDatas.size() / mStride;
	}
	void Enqueue(BYTE* p);
	void Dequeue();
	vBOOL Peek(BYTE* pData);
	void Clear();
};

NS_END

