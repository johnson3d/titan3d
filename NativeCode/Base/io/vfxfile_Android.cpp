#include "../BaseHead.h"
#include "vfxfile_Android.h"
//#include "../../Platform/Android/PlatformAndroid.h"
#include "../thread/vfxthread.h"

#define new VNEW

typedef void (WINAPI *FVFile_Android_GetRelativeFileName)(char* assets, UINT bufferSize, const char* fullFileName);
FVFile_Android_GetRelativeFileName g_GetRelativeFileNameEvent = nullptr;

VFile_Android::VFile_Android()
	: VFile()
	, m_pAsset(NULL)
{

}

VFile_Android::VFile_Android(FILE* hFile)
	: VFile(hFile)
	, m_pAsset(NULL)
{

}
VFile_Android::VFile_Android(LPCSTR lpszFileName, UINT nOpenFlags)
	: VFile(lpszFileName, nOpenFlags)
	, m_pAsset(NULL)
{

}

VFile_Android::~VFile_Android()
{
	if (m_pAsset != NULL && m_bCloseOnDelete)
		Close();
}

std::map<VStringA, VStringA>		GLostAssets;
std::map<VStringA, VStringA>		GReadAssets;

vBOOL VFile_Android::Open(LPCSTR lpszFileName, UINT nOpenFlags)
{
	Close();

	if((nOpenFlags & 3) == modeRead)
	{
		if (GLostAssets.find(lpszFileName) != GLostAssets.end())
			return FALSE;//����apk����Դ
	}

	m_bCloseOnDelete = FALSE;
	m_strFileName = lpszFileName;
	//VStringA_MakeLower(m_strFileName);

	// ���ȳ��Զ�ȡsd��Ŀ¼�ļ�
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
	}

	if (nOpenFlags & modeCreate)
	{
		arg = "wb+";
	}

	auto pstr = m_strFileName.c_str();
	m_hFile = fopen(pstr, arg.c_str());
	if (m_hFile == NULL)
	{
		//VFX_LTRACE(ELTT_info, "Begin Android Read Assets %s", lpszFileName);
		auto jniEnv = EngineNS::PlatformAndroid::GetInstance()->mEnv;
		auto assetManager = EngineNS::PlatformAndroid::GetInstance()->mAssetManager;
		auto javaVM = EngineNS::PlatformAndroid::GetInstance()->mJavaVM;
		if (jniEnv == NULL || assetManager == NULL)
		{
			GLostAssets[lpszFileName] = lpszFileName;
			VFX_LTRACE(ELTT_Resource, "Android Assets Manager is invalid\r\n");
			return FALSE;
		}	
		
		if (javaVM->AttachCurrentThread(&jniEnv, 0) == JNI_OK)
		{
			// û�ж�ȡ���ļ����ȡAPK��Assets����ļ�

			// ת��Ϊ���·��
			if (g_GetRelativeFileNameEvent == NULL)
			{ 
				GLostAssets[lpszFileName] = lpszFileName;
				VFX_LTRACE(ELTT_Resource, "Android Assets Path Converter is nullptr\r\n");
				return FALSE;
			}
			
			char assetsName[256];
			memset(assetsName, 0, sizeof(assetsName));
			g_GetRelativeFileNameEvent(assetsName, 256, lpszFileName);
			m_strFileName = "@Assets/";
			m_strFileName += assetsName;
			//VStringA_MakeLower(m_strFileName);
			m_pAsset = AAssetManager_open(assetManager, assetsName, AASSET_MODE_UNKNOWN);
			if (m_pAsset == NULL)
			{
				GLostAssets[lpszFileName] = lpszFileName;
				VFX_LTRACE(ELTT_Resource, "Android Read Assets Failed: %s\r\n", assetsName);
				return FALSE;
			}
		}
		else
		{
			GLostAssets[lpszFileName] = lpszFileName;
			VFX_LTRACE(ELTT_Resource, "Android AttachCurrentThread Failed: %s\r\n", lpszFileName);
			return FALSE;
		}
	}

	m_bCloseOnDelete = TRUE;

	return TRUE;
}

void VFile_Android::Close()
{
	if (m_hFile)
	{
		fclose(m_hFile);
		m_hFile = NULL;
	}
	if (m_pAsset)
	{
		//GetJavaVM()->AttachCurrentThread(&g_JNIEnv, 0);
		AAsset_close(m_pAsset);
		//GetJavaVM()->DetachCurrentThread();
		m_pAsset = NULL;
	}

	m_bCloseOnDelete = FALSE;
	m_strFileName = "";
}
void VFile_Android::Abort()
{
	if (m_hFile)
	{
		fclose(m_hFile);
		m_hFile = NULL;
	}
	if (m_pAsset)
	{
		//GetJavaVM()->AttachCurrentThread(&g_JNIEnv, 0);
		AAsset_close(m_pAsset);
		//GetJavaVM()->DetachCurrentThread();
		m_pAsset = NULL;
	}
	m_strFileName = "";
}

UINT_PTR VFile_Android::Read(void* lpBuf, UINT_PTR nCount)
{
	if (nCount == 0)
		return 0;

	if (m_hFile != NULL)
		return (UINT_PTR)fread(lpBuf, sizeof(char), nCount, m_hFile);
	if (m_pAsset != NULL)
	{
		//GetJavaVM()->AttachCurrentThread(&g_JNIEnv, 0);
		auto ret = AAsset_read(m_pAsset, lpBuf, nCount);
		//GetJavaVM()->DetachCurrentThread();
		return ret;
	}

	return 0;
}
UINT_PTR VFile_Android::Write(const void* lpBuf, UINT_PTR nCount)
{
	if (nCount == 0)
		return 0;

	if(m_hFile != NULL)
		return (UINT_PTR)fwrite(lpBuf, sizeof(char), nCount, m_hFile);

	return 0;
}

UINT_PTR VFile_Android::GetPosition() const
{
	if(m_hFile != NULL)
		return (UINT_PTR)ftell(m_hFile);
	if (m_pAsset != NULL)
	{
		return AAsset_seek(m_pAsset, 0, SEEK_CUR);
	}

	return 0;
}
INT_PTR VFile_Android::Seek(INT_PTR lOff, SeekPosition nFrom)
{
	if (m_hFile != NULL)
	{
		fseek(m_hFile, lOff, nFrom);
		return ftell(m_hFile);
	}
	if (m_pAsset != NULL)
	{
		return AAsset_seek(m_pAsset, lOff, nFrom);
	}

	return 0;
}
vBOOL VFile_Android::SetLength(UINT_PTR dwNewLen)
{
	if(m_hFile != NULL)
		return fseek(m_hFile, (long)dwNewLen, SEEK_SET) == dwNewLen;
	if (m_pAsset != NULL)
		return AAsset_seek(m_pAsset, dwNewLen, SEEK_SET) == dwNewLen;

	return FALSE;
}
UINT_PTR VFile_Android::GetLength() const
{
	if (m_hFile != NULL)
	{
		UINT_PTR dwLen, dwCur;

		// Seek is a non const operation
		VFile* pFile = (VFile*)this;
		dwCur = pFile->Seek(0L, current);
		dwLen = pFile->SeekToEnd();
		pFile->Seek(dwCur, begin);

		return dwLen;
	}
	if (m_pAsset != NULL)
	{
		return AAsset_getLength(m_pAsset);
	}

	return 0;
}

using namespace EngineNS;

extern "C"
{
	VFX_API void SDK_PlatformAndroid_SetAssetsFileNameCB(FVFile_Android_GetRelativeFileName fun)
	{
		g_GetRelativeFileNameEvent = fun;
	}
}
