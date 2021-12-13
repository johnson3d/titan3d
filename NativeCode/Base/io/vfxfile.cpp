// VFile.cpp
//
//
//
// Author		: johnson
// Create Date	: 2000-?-??
// Modify date	: 2002-4-16
//
//-----------------------------------------------------------------------------
#include "../BaseHead.h"
#include "vfxfile_Android.h"
#include "../r2m/F2MManager.h"

#define new VNEW

using namespace EngineNS;

#undef _MAX_PATH
#define _MAX_PATH		1024
#undef _MAX_FNAME
#define _MAX_FNAME		1024

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wnull-arithmetic"
#endif

////////////////////////////////////////////////////////////////////////////
//  implementation
int VFile::MakesureDir(LPCSTR path)
{
	if (path == NULL) 
	{
		return -1;
	}
#if defined(PLATFORM_WIN)
	char *temp = _strdup(path);
#else
	char *temp = strdup(path);
#endif
	char *pos = temp;

	if (strncmp(temp, "/", 1) == 0) 
	{
		pos += 1;
	}
	else if (strncmp(temp, "./", 2) == 0) 
	{
		pos += 2;
	}

	for (; *pos != '\0'; ++pos) 
	{
		if (*pos == '/') 
		{
			*pos = '\0';
#if defined(PLATFORM_WIN)
			_mkdir(temp);
#elif defined(PLATFORM_IOS)
			//_mkdir(temp);
#else
			mkdir(temp, 0644);
#endif
			*pos = '/';
		}
	}

	if (*(pos - 1) != '/') 
	{
#if defined(PLATFORM_WIN)
		_mkdir(temp);
#elif defined(PLATFORM_IOS)
		//_mkdir(temp);
#else
		mkdir(temp, 0644);
#endif
	}
	free(temp);
	return 0;
}

 VFile::VFile()
{
	m_hFile = NULL;
	m_bCloseOnDelete = FALSE;
	mFileLength = 0;
}

 VFile::VFile(FILE* hFile)
{
	m_hFile = hFile;
	m_bCloseOnDelete = FALSE;
	mFileLength = 0;
}

 VFile::VFile(LPCSTR lpszFileName, UINT nOpenFlags)
{
	ASSERT(lpszFileName);
	mFileLength = 0;
	Open(lpszFileName, nOpenFlags);
//	if (!Open(lpszFileName, nOpenFlags))
//		throw DWORD(::GetLastError());
}

 VFile::~VFile()
{
	if (m_hFile != NULL && m_bCloseOnDelete)
		Close();
}

 VFile_Base* VFile::Duplicate() const
{
	return NULL;
}

std::map<VStringA, VStringA>		GVFReadAssets;
std::map<VStringA, VStringA>		GVFLostAssets;
#if defined PLATFORM_WIN
PTSTR GetErrorMessage(DWORD dwErrCode, DWORD dwLanguageId = MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT))
{
	DWORD dwRet = 0;
	PTSTR szResult = NULL;

	dwRet = FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		dwErrCode,
		dwLanguageId,
		(PTSTR)&szResult,
		0,
		NULL);
	if (dwRet == 0)
	{
		szResult = NULL;
	}

	return szResult;
}
#else
#endif

int GFileOpenNumber = 0;
vBOOL  VFile::Open(LPCSTR lpszFileName, UINT nOpenFlags)
{
	Close();

	m_bCloseOnDelete = FALSE;
	m_strFileName = lpszFileName;
	
    VStringA_MakeLower(m_strFileName);
	
	VStringA arg = "";
	switch (nOpenFlags & 3)
	{
	case modeRead:
		arg += "rb";
		break;
	case modeWrite:
		arg += "wb";
		break;
	case modeReadWrite:
		//dwAccess = GENERIC_READ|GENERIC_WRITE;
		arg += "rb+";
		break;
	default:
		ASSERT(FALSE);  // invalid share mode
	}

	if (nOpenFlags & modeCreate)
	{
		arg = "wb+";
	}

	/*if (GVFLostAssets.find(lpszFileName) != GVFLostAssets.end())
	{
		int i = 0;
		i++;
	}*/

	m_hFile = fopen(m_strFileName.c_str(), arg.c_str());
	if (m_hFile == NULL)
	{
#if defined WIN
		auto dwError = ::GetLastError();
		if(dwError != 2&& dwError != 3)
		{
			auto errorStr = ::GetErrorMessage(dwError);
			VFX_LTRACE(ELTT_Resource, "VFile::Open file(%s)��(%s) open error = %d,%s\n", m_strFileName.c_str(), arg.c_str(), dwError, errorStr);
			::LocalFree(errorStr);
		}
#endif
		GVFLostAssets[lpszFileName] = lpszFileName;
		return FALSE;
	}

	auto saved = fseek(m_hFile, 0, current);
	fseek(m_hFile, 0, end);
	mFileLength = ftell(m_hFile);
	fseek(m_hFile, saved, begin);

	GFileOpenNumber++;
	if (GFileOpenNumber > 100)
	{
		VFX_LTRACE(ELTT_Resource, "Try Close Openning Files Begin(%d)\n", GFileOpenNumber);
		EngineNS::F2MManager::Instance->TryReleaseFile();
		VFX_LTRACE(ELTT_Resource, "Try Close Openning Files End(%d)\n", GFileOpenNumber);
	}

	m_bCloseOnDelete = TRUE;

	/*if (GVFReadAssets.find(lpszFileName) != GVFReadAssets.end())
	{
		if (m_strFileName.Contains(".tech"))
		{
			int i = 0;
			i++;
		}
		VFX_LTRACE(ELTT_info, "Win ReRead Assets %s", lpszFileName);
	}
	GVFReadAssets[lpszFileName] = lpszFileName;*/
	return TRUE;
}

vBOOL  VFile::DefinitlyOpen(LPCSTR lpszFileName, UINT nOpenFlags)
{
	if(Open(lpszFileName, nOpenFlags) == TRUE)
	{
		return TRUE;
	}
	else
	{
		if(Open((VStringA(lpszFileName) + vT(".reloader")).c_str(), nOpenFlags))
			return TRUE;
		else
			return FALSE;
	}

	return FALSE;
}

UINT_PTR  VFile::Read(void* lpBuf, UINT_PTR nCount)
{
	if (m_hFile == NULL || nCount == 0)
		return 0;   // avoid Win32 "null-read"

	return (UINT_PTR)fread(lpBuf, sizeof(char), nCount, m_hFile);
}

UINT_PTR  VFile::Write(const void* lpBuf, UINT_PTR nCount)
{
	if (m_hFile == NULL || nCount == 0)
		return 0;     // avoid Win32 "null-write" option
	return (UINT_PTR)fwrite(lpBuf, sizeof(char), nCount, m_hFile);
}

INT_PTR  VFile::Seek(INT_PTR lOff, SeekPosition nFrom)
{
	if (m_hFile == NULL)
		return 0;
	fseek(m_hFile, (long)lOff, nFrom);
	auto result = ftell(m_hFile);
	return result;
}

UINT_PTR  VFile::GetPosition() const
{
	if (m_hFile == NULL)
		return 0;
	return (UINT_PTR)ftell(m_hFile);
}

vBOOL  VFile::Flush()
{
	if (m_hFile == NULL)
		return FALSE;

	return fflush(m_hFile);
}

void  VFile::Close()
{
	if (m_hFile)
	{
		GFileOpenNumber--;
		fclose(m_hFile);
		m_hFile = NULL;
	}
	m_bCloseOnDelete = FALSE;
	m_strFileName = "";
}

void  VFile::Abort()
{
	if (m_hFile)
	{
		fclose(m_hFile);
		m_hFile = NULL;
	}
	m_strFileName = "";
}

vBOOL  VFile::SetLength(UINT_PTR dwNewLen)
{
	return fseek(m_hFile, (long)dwNewLen, SEEK_SET)==dwNewLen;
}

UINT_PTR  VFile::GetLength() const
{
	return mFileLength;
	//UINT_PTR dwLen, dwCur;

	//// Seek is a non const operation
	//VFile* pFile = (VFile*)this;
	//dwCur = pFile->Seek(0L, current);
	//dwLen = pFile->SeekToEnd();
	//pFile->Seek(dwCur, begin);

	//return dwLen;
}

// VFile does not support direct buffering (CMemFile does)
UINT_PTR  VFile::GetBufferPtr(BufferCommand nCommand, UINT_PTR /*nCount*/,void** /*ppBufStart*/, void** /*ppBufMax*/)
{
	ASSERT(nCommand == bufferCheck);

	return 0;   // no support
}

/////////////////////////////////////////////////////////////////////////////
// VTime - absolute time

 VTime::VTime(INT32 nYear, INT32 nMonth, INT32 nDay, INT32 nHour, INT32 nMin, INT32 nSec,INT32 nDST)
{
	struct tm atm;
	atm.tm_sec = nSec;
	atm.tm_min = nMin;
	atm.tm_hour = nHour;
	ASSERT(nDay >= 1 && nDay <= 31);
	atm.tm_mday = nDay;
	ASSERT(nMonth >= 1 && nMonth <= 12);
	atm.tm_mon = nMonth - 1;        // tm_mon is 0 based
	ASSERT(nYear >= 1900);
	atm.tm_year = nYear - 1900;     // tm_year is 1900 based
	atm.tm_isdst = nDST;
	m_time = mktime(&atm);
	ASSERT(m_time != -1);       // indicates an illegal input time
}

 VTime::VTime(WORD wDosDate, WORD wDosTime, INT32 nDST)
{
	struct tm atm;
	atm.tm_sec = (wDosTime & ~0xFFE0) << 1;
	atm.tm_min = (wDosTime & ~0xF800) >> 5;
	atm.tm_hour = wDosTime >> 11;

	atm.tm_mday = wDosDate & ~0xFFE0;
	atm.tm_mon = ((wDosDate & ~0xFE00) >> 5) - 1;
	atm.tm_year = (wDosDate >> 9) + 80;
	atm.tm_isdst = nDST;
	m_time = mktime(&atm);
	ASSERT(m_time != -1);       // indicates an illegal input time
}

 VTime::VTime(const SYSTEMTIME& sysTime, INT32 nDST)
{
	if (sysTime.wYear < 1900)
	{
		time_t time0 = 0L;
		VTime timeT(time0);
		*this = timeT;
	}
	else
	{
		VTime timeT(
			(INT32)sysTime.wYear, (INT32)sysTime.wMonth, (INT32)sysTime.wDay,
			(INT32)sysTime.wHour, (INT32)sysTime.wMinute, (INT32)sysTime.wSecond,
			nDST);
		*this = timeT;
	}
}

 struct tm* VTime::GetGmtTm(struct tm* ptm) const
{
	if (ptm != NULL)
	{
		*ptm = *gmtime(&m_time);
		return ptm;
	}
	else
		return gmtime(&m_time);
}

 struct tm* VTime::GetLocalTm(struct tm* ptm) const
{
	if (ptm != NULL)
	{
		struct tm* ptmTemp = localtime(&m_time);
		if (ptmTemp == NULL)
			return NULL;    // indicates the m_time was not initialized!

		*ptm = *ptmTemp;
		return ptm;
	}
	else
		return localtime(&m_time);
}

vBOOL  VTime::GetAsSystemTime(SYSTEMTIME& timeDest) const
{
	struct tm* ptm = GetLocalTm(NULL);
	if (ptm == NULL)
		return FALSE;

	timeDest.wYear = (WORD) (1900 + ptm->tm_year);
	timeDest.wMonth = (WORD) (1 + ptm->tm_mon);
	timeDest.wDayOfWeek = (WORD) ptm->tm_wday;
	timeDest.wDay = (WORD) ptm->tm_mday;
	timeDest.wHour = (WORD) ptm->tm_hour;
	timeDest.wMinute = (WORD) ptm->tm_min;
	timeDest.wSecond = (WORD) ptm->tm_sec;
	timeDest.wMilliseconds = 0;

	return TRUE;
}

VStringA  VTime::Format(LPCSTR pFormat) const
{
	CHAR szBuffer[128];

	struct tm* ptmTemp = localtime(&m_time);
	if (ptmTemp == NULL ||
		!strftime(szBuffer, _countof(szBuffer), pFormat, ptmTemp))
		szBuffer[0] = '\0';
	return szBuffer;
}

VStringA  VTime::FormatGmt(LPCSTR pFormat) const
{
	CHAR szBuffer[128];

	struct tm* ptmTemp = gmtime(&m_time);
	if (ptmTemp == NULL ||
		!strftime(szBuffer, _countof(szBuffer), pFormat, ptmTemp))
		szBuffer[0] = '\0';
	return szBuffer;
}

#if defined(PLATFORM_DROID)
#include "vfxfile_Android.cpp"
#define ViseFile VFile_Android
#elif defined(PLATFORM_IOS)
#include "vfxfile_IOS.cpp"
#define ViseFile VFile_IOS
#elif defined(PLATFORM_WIN)
#define ViseFile VFile
#else
#define ViseFile VFile
#endif

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif