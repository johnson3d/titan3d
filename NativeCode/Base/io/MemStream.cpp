#include "MemStream.h"

#define new VNEW

NS_BEGIN

MemStreamWriter::MemStreamWriter()
{
	mDataStream = nullptr;
	mBufferSize = 0;
	mPosition = 0;
}

MemStreamWriter::~MemStreamWriter()
{
	ResetStream(0);
}

void MemStreamWriter::ResetStream(UINT64 size)
{
	Safe_DeleteArray(mDataStream);
	if (size > 0)
	{
		mBufferSize = size;
		mDataStream = new BYTE[size];
	}
	mPosition = 0;
}

bool MemStreamWriter::Seek(UINT64 offset)
{
	if (mBufferSize <= offset)
	{
		return false;
	}
	mPosition = offset;
	return true;
}

void MemStreamWriter::Write(const void* pSrc, UINT t)
{
	if (mBufferSize < mPosition + t)
	{
		auto sz = (mBufferSize + t) * 2;
		BYTE* nBuffer = new BYTE[sz];
		if (mPosition > 0)
		{
			memcpy(nBuffer, mDataStream, mPosition);
		}
		Safe_DeleteArray(mDataStream);
		mDataStream = nBuffer;
		mBufferSize = sz;
	}
	memcpy(&mDataStream[mPosition], pSrc, (size_t)t);
	mPosition += t;
}

UINT MemStreamReader::Read(void* pSrc, UINT t)
{
	if (mLength < mPosition + t)
	{
		return 0;
	}
	memcpy(pSrc, &mProxyPointer[mPosition], (size_t)t);
	mPosition += t;
	return t;
}

NS_END