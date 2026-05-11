#include <stdio.h>
#include "file_2_memory.h"
#include <filesystem>

//#include "../xnd/vfxxnd.h"

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnull-arithmetic"
#endif

NS_BEGIN

ENGINE_RTTI_IMPL(VFile2Memory);

FResPointerGuard::FResPointerGuard(VRes2Memory* res, UINT64 offset, UINT64 size, bool tryClearCache)
{
	Resource = res;
	Offset = offset;
	Size = size;
	TryClearCache = tryClearCache;
	Pointer = Resource->Ptr(offset, size);
}

FResPointerGuard::~FResPointerGuard()
{
	Resource->Free(TryClearCache);
	Pointer = nullptr;
	Resource = nullptr;
	Offset = 0;
	Size = 0;
}

//------------------------------------------------------------------------
VFile2Memory::VFile2Memory()
	: mPtrRef(0)
{
	mCachedStarter = 0;
}

VFile2Memory::~VFile2Memory()
{
	Close();
}

std::atomic<long> GPtrRef(0);
VResPtr VFile2Memory::Ptr(UINT64 offset , UINT64 size)
{
	mLocker.Lock();
	GPtrRef++;
	if (mPtrRef >= 1)
    {
        VFX_LTRACE(ELTT_Error,"%s(%d):F2M(%s) Ptr Refcount is not zero\r\n", __FILE__, __LINE__, mName.c_str());
    }
	
	++mPtrRef;

	if (size == 0)
	{
		size = (INT_PTR)mFile.GetLength() - offset;
	}
	if (mCachedStarter > offset || mCachedStarter + mCachedBuffer.size() < offset + size)
	{
		mCachedBuffer.resize(size);
		mCachedStarter = offset;

		if (mFile.IsFileOpened() == false)
		{
			if (false == mFile.Open(mName.c_str(), VFile::modeRead))
			{
				VFX_LTRACE(ELTT_Error, "F2M Ptr [%s] open faile, someone deleted this file in runtime\r\n", mName.c_str());
				return NULL;
			}
		}
		mFile.Seek(offset, VFile::begin);
		auto readCount = mFile.Read(&mCachedBuffer[0], size);
		if (readCount != size)
		{
			ASSERT(false);
			VFX_LTRACE(ELTT_Error, "VFile2Memory::Ptr readCount[%d]!=size[%d]", (int)readCount, (int)size);
		}

		return &mCachedBuffer[0];
	}
	else
	{
		return &mCachedBuffer[offset - mCachedStarter];
	}
}

vBOOL VFile2Memory::Free(bool bTryClearCache)
{
	//ClearCache();
	GPtrRef--;

	ASSERT(mPtrRef > 0);
	--mPtrRef;
	if (bTryClearCache && mPtrRef == 0)
	{
		ClearCache();
	}
	mLocker.Unlock();
	
	return TRUE;
}

void VFile2Memory::ClearCache()
{
	mCachedStarter = 0;
	mCachedBuffer = std::vector<BYTE>();
}

bool VFile2Memory::TryReleaseHolder()
{
	if (mLocker.TryLock())
	{
		Close();
		mLocker.Unlock();
		return true;
	}
	else
	{
		return false;
	}
}

UINT64 VFile2Memory::Length() const
{
	if (mFile.GetLength() == 0 && mFile.IsFileOpened() == false)
	{
		if (false == ((ViseFile*)&mFile)->Open(mName.c_str(), VFile::modeRead))
		{
			VFX_LTRACE(ELTT_Error, "F2M Ptr [%s] open faile, someone deleted this file in runtime\r\n", mName.c_str());
			return 0;
		}
		else
		{
			auto ret = mFile.GetLength();
			((ViseFile*)&mFile)->Close();
			return ret;
		}
	}

	return mFile.GetLength();
}

LPCSTR VFile2Memory::Name() const
{
	return mName.c_str();
}

vBOOL VFile2Memory::Create(LPCSTR pszFile,vBOOL bShareFile)
{
	Close();

	mName = pszFile;

	return std::filesystem::exists(mName) ? TRUE : FALSE;
}

void VFile2Memory::Close()
{
	VAutoLock(mLocker);
	
	if (mFile.IsFileOpened())
	{
		mFile.Close();
	}
	if (mPtrRef == 0)
	{
		ClearCache();
	}
}

void VFile2Memory::OnBeforeWriteFile()
{
	if (mPtrRef != 0)
	{
		ASSERT(false);
	}
	Close();
	//mFile.UpdateFileLength();
}

void VFile2Memory::OnAfterWriteFile()
{
	if (mPtrRef != 0)
	{
		ASSERT(false);
	}
	Close();
	mFile.UpdateFileLength();
}
//--------------------------------------------------------------------------------------------------

VMemoryResPtr::VMemoryResPtr()
	: m_ptrBase(NULL)
	, m_dwLength(0)
{
}

VMemoryResPtr::~VMemoryResPtr()
{
	delete (char *)m_ptrBase;
}

vBOOL VMemoryResPtr::Create(LPCSTR pszName,VResPtr pPtr,DWORD dwLength)
{
	delete (char *)m_ptrBase;
	m_ptrBase = new char[dwLength];
	m_dwLength = dwLength;
	memcpy((void *)m_ptrBase,pPtr,dwLength);
	m_strName = pszName;
	VStringA_MakeLower(m_strName);

	return TRUE;
}

VResPtr VMemoryResPtr::Ptr(UINT64 offset , UINT64 size)
{
	return (const BYTE*)m_ptrBase+offset;
}

vBOOL VMemoryResPtr::Free(bool bTryClearCache)
{
	return TRUE;
}

UINT64 VMemoryResPtr::Length() const
{
	return m_dwLength;
}

LPCSTR VMemoryResPtr::Name() const
{
	return m_strName.c_str();
}

VRes2Memory*	VFile2ResFactory::CreateRes(LPCSTR psz,vBOOL bShareWrite)
{
	if(psz == NULL || psz[0] == 0)
		return NULL;
	VFile2Memory * pFM = new VFile2Memory;
	if(FALSE == pFM->Create(psz,bShareWrite))
	{
		delete pFM;
		return NULL;
	}
	return pFM;
}


NS_END

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif