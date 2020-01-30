// VFile.cpp
//
// LZP_Graph
//
// �ļ�����
//
// Author		: johnson
// Create Date	: 2000-?-??
// Modify date	: 2002-4-16
//
//-----------------------------------------------------------------------------
#include "../precompile.h"
#include "../xnd/vfxxnd.h"
#include "vfxfile_Android.h"
#include "../thread/vfxthread.h"
#include "../r2m/VPakFile.h"

#define new VNEW

using namespace EngineNS;

vBOOL GFileOpenThreadCheck = FALSE;

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

	/* ȥ���ͷ�� './' �� '/' */
	if (strncmp(temp, "/", 1) == 0) 
	{
		pos += 1;
	}
	else if (strncmp(temp, "./", 2) == 0) 
	{
		pos += 2;
	}

	/* ѭ������Ŀ¼ */
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

	/* ������һ��Ŀ¼������'/'��β��
	����'\0'����ֹѭ����
	���ᴴ�����һ��Ŀ¼ */
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
}

 VFile::VFile(FILE* hFile)
{
	m_hFile = hFile;
	m_bCloseOnDelete = FALSE;
}

 VFile::VFile(LPCSTR lpszFileName, UINT nOpenFlags)
{
	ASSERT(lpszFileName);
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
	if(GFileOpenThreadCheck)
	{
		if (GRenderThreadId == vfxThread::GetCurrentThreadId())
		{
			VFX_LTRACE(ELTT_Resource, "VFile::Open open file %s in render thread!!!", lpszFileName);
		}
		if (GLogicThreadId == vfxThread::GetCurrentThreadId())
		{
			VFX_LTRACE(ELTT_Resource, "VFile::Open open file %s in logic thread!!!", lpszFileName);
		}
	}

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
	GFileOpenNumber++;
	if (GFileOpenNumber > 100)
	{
		VFX_LTRACE(ELTT_Resource, "Try Close Openning Files Begin(%d)\n", GFileOpenNumber);
		EngineNS::F2MManager::Instance.TryReleaseFile();
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
		if(Open((VString(lpszFileName) + vT(".reloader")).c_str(), nOpenFlags))
			return TRUE;
		else
			return FALSE;
	}

	return FALSE;
}
vBOOL  VFile::DefinitlyLoad(LPCSTR lpszFileName, XNDNode &xndNode)
{
	VStringA name = lpszFileName;
	rename((name + ".reloader").c_str(), name.c_str());
	//::MoveFileEx( (name + vT(".reloader")).c_str() , name,MOVEFILE_REPLACE_EXISTING );

	VFile2Memory * pFM = new VFile2Memory;
	if((int)NULL == pFM->Create(lpszFileName, FALSE))
	{
		delete pFM;
		return FALSE;
	}

	xndNode.Load(pFM);
	Safe_Release(pFM);

	return TRUE;
}
vBOOL  VFile::DefinitlySave(LPCSTR lpszFileName, XNDNode &xndNode)
{
	ViseFile io;
	if(io.DefinitlyOpen(lpszFileName, VFile::modeCreate|VFile::modeWrite) == TRUE)
	{
		xndNode.Save(io);
		io.Close();
		return TRUE;
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
	UINT_PTR dwLen, dwCur;

	// Seek is a non const operation
	VFile* pFile = (VFile*)this;
	dwCur = pFile->Seek(0L, current);
	dwLen = pFile->SeekToEnd();
	pFile->Seek(dwCur, begin);

	return dwLen;
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

VString  VTime::Format(LPCSTR pFormat) const
{
	CHAR szBuffer[128];

	struct tm* ptmTemp = localtime(&m_time);
	if (ptmTemp == NULL ||
		!strftime(szBuffer, _countof(szBuffer), pFormat, ptmTemp))
		szBuffer[0] = '\0';
	return szBuffer;
}

VString  VTime::FormatGmt(LPCSTR pFormat) const
{
	CHAR szBuffer[128];

	struct tm* ptmTemp = gmtime(&m_time);
	if (ptmTemp == NULL ||
		!strftime(szBuffer, _countof(szBuffer), pFormat, ptmTemp))
		szBuffer[0] = '\0';
	return szBuffer;
}

extern "C"
{
	VFX_API VFile* VFile_New()
	{
		return new ViseFile();
	}
	VFX_API void VFile_Delete(VFile* vFile)
	{
		Safe_Delete(vFile);
	}
	VFX_API vBOOL VFile_Open(VFile* vFile, LPCSTR fileName, UINT openFlags)
	{
		if(vFile == NULL)
			return FALSE;

		return vFile->Open(fileName, openFlags);
	}
	VFX_API void VFile_Close(VFile* vFile)
	{
		if(vFile != NULL)
			vFile->Close();
	}
	VFX_API void VFile_EnableFileOpenThreadCheck(vBOOL enable)
	{
		GFileOpenThreadCheck = enable;
	}

#if defined(PLATFORM_WIN)

	 long VFile_CopyFile(LPCWSTR lpExistingFileName, LPCWSTR lpNewFileName, vBOOL overrideFile)
	{
		auto retValue = CopyFileW(lpExistingFileName, lpNewFileName, overrideFile ? FALSE : TRUE);
		if(!retValue)
			return GetLastError();
		return 0;
	}
	 long VFile_CreateDirectory(LPCWSTR absPathName)
	{
		auto retValue = CreateDirectoryW(absPathName, NULL);
		if(!retValue)
			return GetLastError();
		return 0;
	}
	long RemoveSubDirectories(LPCWSTR absPathName, vBOOL recursive)
	{
		long retValue = 0;
		if (recursive)
		{
			size_t pathCharCount = wcslen(absPathName);
			WIN32_FIND_DATAW ffd;
			size_t folderStrCharLen = pathCharCount + 3;
			wchar_t* folderStr = new wchar_t[folderStrCharLen];
			swprintf_s(folderStr, folderStrCharLen, L"%s\\*", absPathName);
			auto hFind = FindFirstFileW(folderStr, &ffd);
			std::vector<wchar_t*> subFolders;
			if (INVALID_HANDLE_VALUE != hFind)
			{
				do
				{
					if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
					{
						if (StrCmpW(ffd.cFileName, L".") != 0 &&
							StrCmpW(ffd.cFileName, L"..") != 0)
						{
							size_t subFolderCharLen = pathCharCount + wcslen(ffd.cFileName) + 2;
							wchar_t* subFolder = new wchar_t[subFolderCharLen];
							swprintf_s(subFolder, subFolderCharLen, L"%s\\%s", absPathName, ffd.cFileName);
							auto delResult = RemoveSubDirectories(subFolder, recursive);
							subFolders.push_back(subFolder);
							//Safe_DeleteArray(subFolder);
							if (delResult != 0)
							{
								retValue = delResult;
							}
						}
					}
					else
					{
						size_t absFileNameCharLen = pathCharCount + wcslen(ffd.cFileName) + 2;
						wchar_t* absFileName = new wchar_t[absFileNameCharLen];
						swprintf_s(absFileName, absFileNameCharLen, L"%s\\%s", absPathName, ffd.cFileName);
						auto delResult = DeleteFileW(absFileName);
						Safe_DeleteArray(absFileName);
						if (!delResult)
						{
							auto lastErr = GetLastError();
							if (lastErr != 0)
								retValue = lastErr;
						}
					}
				} while (FindNextFileW(hFind, &ffd));

				FindClose(hFind);
			}
			Safe_DeleteArray(folderStr);

			for (auto ite = subFolders.begin(); ite != subFolders.end(); ite++)
			{
				RemoveDirectoryW(*ite);
				Safe_DeleteArray(*ite);
			}
		}
		return retValue;
	}
	 long VFile_RemoveDirectory(LPCWSTR absPathName, vBOOL recursive)
	{
		long retValue = RemoveSubDirectories(absPathName, recursive);

		auto result = RemoveDirectoryW(absPathName);
		if (!result)
		{
			auto ret = GetLastError();
			if (ret != 0)
				retValue = ret;
		}
		return retValue;
	}
	 long VFile_DeleteFile(LPCWSTR absPathName)
	{
		if (!DeleteFileW(absPathName))
		{
			return GetLastError();
		}
		return 0;
	}
	 vBOOL VFile_IsExist(LPCWSTR absName)
	{
		WIN32_FIND_DATAW ffd;
		auto hFind = FindFirstFileW(absName, &ffd);
		if (INVALID_HANDLE_VALUE == hFind)
			return FALSE;
		FindClose(hFind);
		return TRUE;
	}
	 int VFile_GetFilesCount(LPCWSTR absPathName)
	{
		WIN32_FIND_DATAW ffd;
		auto hFind = FindFirstFileW(absPathName, &ffd);
		if (INVALID_HANDLE_VALUE == hFind)
		{
			return 0;
		}

		int retCount = 0;
		do
		{
			if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
			}
			else
			{
				retCount++;
			}
		} while (FindNextFileW(hFind, &ffd));

		FindClose(hFind);
		return retCount;
	}
	 int VFile_GetDirectoriesCount(LPCWSTR absPathName)
	{
		WIN32_FIND_DATAW ffd;
		auto hFind = FindFirstFileW(absPathName, &ffd);
		if (INVALID_HANDLE_VALUE == hFind)
			return 0;

		int retCount = 0;
		do 
		{
			if (ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				if (StrCmpW(ffd.cFileName, L".") != 0 &&
					StrCmpW(ffd.cFileName, L"..") != 0)
				{
					retCount++;
				}
			}
		} while (FindNextFileW(hFind, &ffd));

		FindClose(hFind);
		return retCount;
	}
	 void VFile_FreeString(wchar_t* str)
	{
		if (str != NULL)
			delete[] str;
	}
	 HANDLE VFile_GetFindHandle(LPCWSTR absPathName)
	{
		WIN32_FIND_DATAW ffd;
		auto retHandle = FindFirstFileW(absPathName, &ffd);
		if (INVALID_HANDLE_VALUE == retHandle)
		{
			return NULL;
		}
		return retHandle;
	}
	 vBOOL VFile_FindClose(HANDLE findHandle)
	{
		return FindClose(findHandle) ? TRUE : FALSE;
	}
	 wchar_t* VFile_GetFiles(LPCWSTR absPathName, HANDLE findHandle)
	{
		if (findHandle == NULL)
			return NULL;

		WIN32_FIND_DATAW ffd;
		if (FindNextFileW(findHandle, &ffd))
		{
			if ((ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
				return VFile_GetFiles(absPathName, findHandle);
			else
			{
				auto strLen = wcslen(ffd.cFileName) + 1;
				wchar_t* retValue = new wchar_t[strLen];
				swprintf_s(retValue, strLen, L"%s", ffd.cFileName);
				return retValue;
			}
		}
		
		return NULL;
	}
	 wchar_t* VFile_GetDirectories(LPCWSTR absPathName, HANDLE findHandle)
	{
		if (findHandle == NULL)
			return NULL;

		WIN32_FIND_DATAW ffd;
		if (FindNextFileW(findHandle, &ffd))
		{
			if ((ffd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
			{
				if (StrCmpW(ffd.cFileName, L".") != 0 &&
					StrCmpW(ffd.cFileName, L"..") != 0)
				{
					auto strLen = wcslen(ffd.cFileName) + 1;
					wchar_t* retValue = new wchar_t[strLen];
					swprintf_s(retValue, strLen, L"%s", ffd.cFileName);
					return retValue;
				}
				else
					return VFile_GetDirectories(absPathName, findHandle);
			}
			else
			{
				return VFile_GetDirectories(absPathName, findHandle);
			}
		}
		return NULL;
	}

#endif
};

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif