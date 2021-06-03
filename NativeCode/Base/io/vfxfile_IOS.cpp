#include "../../BaseHead.h"
//#include "../../Core/r2m/file_2_memory.h"
#include "vfxfile_IOS.h"

#define new VNEW

typedef void (WINAPI *FVFile_IOS_GetAssetsFileName)(char* assets, UINT bufferSize, const char* fullFileName, vBOOL fromDocument);

FVFile_IOS_GetAssetsFileName g_GetAssetsFileNameEvent = nullptr;

VFile_IOS::VFile_IOS()
	: VFile()
{
}
VFile_IOS::VFile_IOS(FILE* hFile)
	: VFile(hFile)
{

}
VFile_IOS::VFile_IOS(LPCSTR lpszFileName, UINT nOpenFlags)
	: VFile(lpszFileName, nOpenFlags)
{

}

VFile_IOS::~VFile_IOS()
{
	if (m_bCloseOnDelete)
		Close();
}

int GFileOpenNumberIOS = 0;
std::map<VStringA, VStringA>		GLostAssets;
vBOOL VFile_IOS::Open(LPCSTR lpszFileName, UINT nOpenFlags)
{
    Close();

	if (g_GetAssetsFileNameEvent == NULL)
	{
		return FALSE;
	}

	if ((nOpenFlags & 3) == modeRead)
	{
		if (GLostAssets.find(lpszFileName) != GLostAssets.end())
			return FALSE;// 不在apk的资源
	}

	m_bCloseOnDelete = FALSE;

	char assetsName[256];
	memset(assetsName, 0, sizeof(assetsName));
	g_GetAssetsFileNameEvent(assetsName, 256, lpszFileName, TRUE);
	m_strFileName = "@Document/";
	m_strFileName += assetsName;
	
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
		arg += "rb+";
		break;
	default:
		ASSERT(FALSE);
		break;
	}

	if (nOpenFlags & modeCreate)
	{
		arg = "wb+";
	}

    extern int errno;
	// 优先尝试读取Documents目录文件
	m_hFile = fopen(assetsName, arg.c_str());
	if (m_hFile == NULL)
	{
		memset(assetsName, 0, sizeof(assetsName));
		// 尝试读取assets目录文件
		g_GetAssetsFileNameEvent(assetsName, 256, lpszFileName, FALSE);
		m_strFileName = "@Assets/";
		m_strFileName += assetsName;
        int preerror = errno;
		m_hFile = fopen(assetsName, arg.c_str());
		if (m_hFile == NULL)
		{
            int afterror = errno;
			GLostAssets[lpszFileName] = lpszFileName;
            VFX_LTRACE(ELTT_Error, "errno =  (%s) %d - > %d:%s\r\n", lpszFileName, preerror, afterror, strerror(afterror));
			return FALSE;
		}
	}
    
    GFileOpenNumberIOS++;
    if (GFileOpenNumberIOS > 100)
    {
        VFX_LTRACE(ELTT_Resource, "Try Close Openning Files Begin(%d)\n", GFileOpenNumberIOS);
        //F2MManager::Instance.TryReleaseFile();
        VFX_LTRACE(ELTT_Resource, "Try Close Openning Files End(%d)\n", GFileOpenNumberIOS);
    }

	m_bCloseOnDelete = TRUE;
	return TRUE;
}
void VFile_IOS::Close()
{
	if (m_hFile)
	{
        GFileOpenNumberIOS--;
		fclose(m_hFile);
		m_hFile = NULL;
	}
	m_bCloseOnDelete = FALSE;
	m_strFileName = "";
}
void VFile_IOS::Abort()
{
	if (m_hFile)
	{
		fclose(m_hFile);
		m_hFile = NULL;
	}
	m_strFileName = "";
}

UINT_PTR	VFile_IOS::Read(void* lpBuf, UINT_PTR nCount)
{
	if (m_hFile == NULL || nCount == 0)
		return 0;

	return (UINT_PTR)fread(lpBuf, sizeof(char), nCount, m_hFile);
}
UINT_PTR	VFile_IOS::Write(const void* lpBuf, UINT_PTR nCount)
{
	if (m_hFile == NULL || nCount == 0)
		return 0;
	return (UINT_PTR)fwrite(lpBuf, sizeof(char), nCount, m_hFile);
}

UINT_PTR	VFile_IOS::GetPosition() const
{
	if (m_hFile == NULL)
		return 0;
	return (UINT_PTR)ftell(m_hFile);
}
INT_PTR	VFile_IOS::Seek(INT_PTR lOff, SeekPosition nFrom)
{
	if (m_hFile == NULL)
		return 0;
	fseek(m_hFile, (long)lOff, nFrom);
	return ftell(m_hFile);
}
vBOOL		VFile_IOS::SetLength(UINT_PTR dwNewLen)
{
	return fseek(m_hFile, (long)dwNewLen, SEEK_SET) == dwNewLen;
}
UINT_PTR	VFile_IOS::GetLength() const
{
	UINT_PTR dwLen, dwCur;

	// Seek is a non const operation
	VFile* pFile = (VFile*)this;
	dwCur = pFile->Seek(0L, current);
	dwLen = pFile->SeekToEnd();
	pFile->Seek(dwCur, begin);

	return dwLen;
}

using namespace EngineNS;

extern "C"
{
	VFX_API void SDK_PlatformIOS_SetAssetsFileNameCB(FVFile_IOS_GetAssetsFileName fun)
	{
		g_GetAssetsFileNameEvent = fun;
	}
}