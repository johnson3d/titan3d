#include "CsValueList.h"
#include "../../CSharpAPI.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::CsValueList, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::CsQueue, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::CsStreamWriter, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::CsStreamReader, EngineNS::VIUnknown);

CsValueList::CsValueList()
{
	mStride = 0;
}

CsValueList::~CsValueList()
{
	Clear();
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
	mMemData.InstantArray(mMemData.GetSize(), capacity);
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

void CsValueList::RemoveAt(UINT index)
{
	ASSERT(mStride != 0);

	UINT offset = index * mStride;
	for (UINT i = 0; i < mStride; i++)
	{
		mMemData.RemoveAt(offset);
	}
}

void CsValueList::Clear(UINT size)
{
	mMemData.SetSize(0);
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

//////////////////////////////////////////////////////////////////////////

CsStreamWriter::CsStreamWriter()
{

}

CsStreamWriter::~CsStreamWriter()
{
	Clear();
}

void CsStreamWriter::Cleanup()
{
	Clear();
}

void CsStreamWriter::Reserve(UINT size)
{
	mDatas.InstantArray(mDatas.GetSize(), size);
}

void CsStreamWriter::PushData(BYTE* p, UINT size)
{
	auto old = mDatas.GetSize();
	mDatas.SetSize(old + size);
	memcpy(&mDatas[old], p, size);
}

void CsStreamWriter::Clear()
{
	mDatas.SetSize(0, 0);
}

BYTE* CsStreamWriter::GetPointer()
{
	return &mDatas[0];
}

UINT CsStreamWriter::GetSize()
{
	return (UINT)mDatas.GetSize();
}

CsStreamReader::CsStreamReader()
{
	mPos = 0;
}

CsStreamReader::~CsStreamReader()
{

}

void CsStreamReader::Cleanup()
{
	mDatas.SetSize(0, 0);
	mPos = 0;
}

void CsStreamReader::InitData(BYTE* p, UINT size)
{
	mDatas.SetSize(size, 0);
	if (size > 0)
	{
		memcpy(&mDatas[0], p, size);
	}
	mPos = 0;
}

BYTE* CsStreamReader::GetPointer()
{
	return &mDatas[0];
}

UINT CsStreamReader::GetSize()
{
	return (UINT)mDatas.GetSize();
}

vBOOL CsStreamReader::Read(BYTE* p, UINT size)
{
	if (mPos + size > GetSize())
		return FALSE;

	memcpy(p, &mDatas[mPos], size);
	mPos += size;

	return TRUE;
}

void CsStreamReader::ResetReader()
{
	mPos = 0;
}

UINT CsStreamReader::GetPosition()
{
	return mPos;
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

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, CsValueList, GetStride);
	Cpp2CS1(EngineNS, CsValueList, SetCapacity);
	Cpp2CS1(EngineNS, CsValueList, SetStride);
	Cpp2CS0(EngineNS, CsValueList, GetCount);
	Cpp2CS1(EngineNS, CsValueList, AddValue);
	Cpp2CS1(EngineNS, CsValueList, Append);
	Cpp2CS1(EngineNS, CsValueList, RemoveAt);
	Cpp2CS1(EngineNS, CsValueList, Clear);
	Cpp2CS1(EngineNS, CsValueList, GetAddressAt);
	Cpp2CS2(EngineNS, CsValueList, SetDatas);

	Cpp2CS0(EngineNS, CsQueue, GetStride);
	Cpp2CS1(EngineNS, CsQueue, SetStride);
	Cpp2CS0(EngineNS, CsQueue, GetCount);
	Cpp2CS1(EngineNS, CsQueue, Enqueue);
	Cpp2CS0(EngineNS, CsQueue, Dequeue);
	Cpp2CS1(EngineNS, CsQueue, Peek);
	Cpp2CS0(EngineNS, CsQueue, Clear);

	VFX_API CsStreamWriter* SDK_CsStreamWriter_New()
	{
		return new CsStreamWriter();
	}

	Cpp2CS1(EngineNS, CsStreamWriter, Reserve);
	Cpp2CS2(EngineNS, CsStreamWriter, PushData);
	Cpp2CS0(EngineNS, CsStreamWriter, Clear);
	Cpp2CS0(EngineNS, CsStreamWriter, GetPointer);
	Cpp2CS0(EngineNS, CsStreamWriter, GetSize);

	VFX_API CsStreamReader* SDK_CsStreamReader_New()
	{
		return new CsStreamReader();
	}

	Cpp2CS2(EngineNS, CsStreamReader, InitData);
	Cpp2CS0(EngineNS, CsStreamReader, GetPointer);
	Cpp2CS0(EngineNS, CsStreamReader, GetSize);
	Cpp2CS2(EngineNS, CsStreamReader, Read);
	Cpp2CS0(EngineNS, CsStreamReader, ResetReader);
	Cpp2CS0(EngineNS, CsStreamReader, GetPosition);
}