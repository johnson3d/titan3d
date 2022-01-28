#include "CsValueList.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::CsValueList);
ENGINE_RTTI_IMPL(EngineNS::CsQueue);

CsValueList::CsValueList(int stride)
{
	mStride = stride;
}

CsValueList::~CsValueList()
{
	Clear(TRUE);
}

UINT CsValueList::GetCount()
{
	ASSERT(mStride != 0);
	ASSERT(mMemData.GetSize() % mStride == 0);
 	return mMemData.GetSize() / mStride;
}

void CsValueList::SetCapacity(int capacity)
{
	ASSERT(mStride != 0);
	mMemData.InstantArray(mMemData.GetSize(), capacity * mStride, FALSE);
}

void CsValueList::SetSize(int capacity)
{
	ASSERT(mStride != 0);
	mMemData.SetSize(capacity * mStride, capacity * mStride, FALSE);
}

void CsValueList::AddValue(BYTE* ptr)
{
	ASSERT(mStride != 0);

	for (UINT i = 0; i < mStride; i++)
	{
		mMemData.Add(ptr[i]);
	}
}

void CsValueList::Append(CsValueList* src)
{
	ASSERT(mStride != 0);

	mMemData.Append(src->mMemData);
}

void CsValueList::AppendArray(BYTE* src, int count)
{
	mMemData.Append(src, mStride * count);
}

void CsValueList::RemoveAt(UINT index)
{
	ASSERT(mStride != 0);

	UINT offset = index * mStride;
	for (UINT i = 0; i < mStride; i++)
	{
		mMemData.RemoveAt(offset);
	}
}

void CsValueList::Clear(vBOOL bFreeMemory)
{
	mMemData.RemoveAll(bFreeMemory);
}

BYTE* CsValueList::GetAddressAt(UINT index)
{
	ASSERT(mStride != 0);

	UINT offset = index * mStride;
	if ((int)offset >= mMemData.GetSize())
		return nullptr;
	return &mMemData[offset];
}

void CsValueList::SetDatas(BYTE* ptr, int countOfObj)
{
	ASSERT(mStride != 0);

	UINT size = countOfObj * mStride;
	mMemData.SetSize(size);
	memcpy(mMemData.GetData(), ptr, size);
}

void CsQueue::Enqueue(BYTE* p)
{
	for (UINT i = 0; i < mStride; i++)
	{
		mDatas.push(p[i]);
	}
}

void CsQueue::Dequeue()
{
	for (UINT i = 0; i < mStride; i++)
	{
		mDatas.pop();
	}
}

vBOOL CsQueue::Peek(BYTE* pData)
{
	for (UINT i = 0; i < mStride; i++)
	{
		if (mDatas.size() == 0)
			return FALSE;
		pData[i] = mDatas.front();
		mDatas.pop();
	}
	return TRUE;
}

void CsQueue::Clear()
{
	while (mDatas.size() > 0)
	{
		mDatas.pop();
	}
}

NS_END
