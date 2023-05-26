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
	ASSERT(mMemData.size() % mStride == 0);
 	return (UINT)mMemData.size() / mStride;
}

void CsValueList::SetCapacity(int capacity)
{
	ASSERT(mStride != 0);
	mMemData.reserve(capacity * mStride);
}

int CsValueList::GetCapacity()
{
	ASSERT(mStride != 0);
	return (int)(mMemData.capacity() / mStride);
}

void CsValueList::SetSize(int size)
{
	ASSERT(mStride != 0);
	mMemData.resize(size * mStride);
}

void CsValueList::AddValue(BYTE* ptr)
{
	ASSERT(mStride != 0);

	auto index = mMemData.size();
	mMemData.resize(mMemData.size() + mStride);
	memcpy(&mMemData[index], ptr, mStride);
	/*for (UINT i = 0; i < mStride; i++)
	{
		mMemData.Add(ptr[i]);
	}*/
}

void CsValueList::Append(CsValueList* src)
{
	ASSERT(mStride != 0);

	mMemData.insert(mMemData.end(), src->mMemData.begin(), src->mMemData.end());
	//mMemData.Append(src->mMemData);
}

void CsValueList::AppendArray(BYTE* src, int count)
{
	auto index = mMemData.size();
	mMemData.resize(mMemData.size() + mStride * count);
	memcpy(&mMemData[index], src, mStride * count);
	//mMemData.Append(src, mStride * count);
}

void CsValueList::RemoveAt(UINT index)
{
	ASSERT(mStride != 0);
	UINT offset = index * mStride;
	auto s = mMemData.begin() + offset;
	
	mMemData.erase(s, s + mStride);
	/*for (UINT i = 0; i < mStride; i++)
	{
		mMemData.RemoveAt(offset);
	}*/
}

void CsValueList::InsertAt(UINT index, BYTE* ptr)
{
	ASSERT(mStride != 0);
	UINT offset = index * mStride;
	auto oldSize = mMemData.size();	
	mMemData.resize(oldSize + mStride);
	
	memmove(&mMemData[offset + mStride], &mMemData[offset], oldSize - offset);
	memcpy(&mMemData[offset], ptr, mStride);
}

void CsValueList::Clear(vBOOL bFreeMemory)
{
	//mMemData.RemoveAll(bFreeMemory);
	if (bFreeMemory)
	{
		mMemData = std::vector<BYTE>();
	}
	else
	{
		mMemData.clear();
	}
}

BYTE* CsValueList::GetAddressAt(UINT index)
{
	ASSERT(mStride != 0);

	size_t offset = index * mStride;
	if (offset >= mMemData.size())
		return nullptr;
	return &mMemData[offset];
}

void CsValueList::SetDatas(BYTE* ptr, int countOfObj)
{
	ASSERT(mStride != 0);

	UINT size = countOfObj * mStride;
	mMemData.resize(size);
	memcpy(&mMemData[0], ptr, size);
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
