#include "WordCodeHelper.h"

#define new VNEW

#if defined(PLATFORM_WIN) 
const char* WordCodeHelper::SrcCode = "GB18030//TRANSLIT";
const char* WordCodeHelper::DesCode = "UTF-16LE";
#else
const char* WordCodeHelper::SrcCode = "UTF-8";
const char* WordCodeHelper::DesCode = "UTF-32LE";
#endif

WordCodeHelper::WordCodeHelper()
{
#if defined(PLATFORM_WIN) 
	mOriginCode = "GB18030//TRANSLIT";
#elif defined(PLATFORM_ROID)
	mOriginCode = "UTF-8";
#endif // ANDROID

	if (sizeof(wchar_t) == 2)
	{
		//mOriginCode = "GB18030//TRANSLIT";
		mDestCode = "UTF-16LE";
	}
	if (sizeof(wchar_t) == 4)
	{
		//mOriginCode = "UTF-8";
		mDestCode = "UTF-32LE";
	}

}


WordCodeHelper::~WordCodeHelper()
{
}

void WordCodeHelper::SetOriginCode(const char* orginCode)
{
	if (orginCode)
		mOriginCode = orginCode;
}

void WordCodeHelper::SetDestCode(const char* destCode)
{
	if (destCode)
		mDestCode = destCode;
}

size_t WordCodeHelper::ChangeCode(const char* pInBuf, size_t* iInLen, char* pOutBuf, size_t* iOutLen)
{
	size_t iRet = 0;
	iconv_t hIconv = iconv_open(mDestCode, mOriginCode);
	if ((iconv_t)-1 == hIconv)
	{
		return -1;//failed, unsupport character set

	}
	while (*iInLen > 0)
	{
		iRet = iconv(hIconv, (char**)(&pInBuf), iInLen, (char**)(&pOutBuf), iOutLen);
		if (iRet == -1)
		{
			return -1;
		}
	}
	iconv_close(hIconv);
	return iRet;
}

int WordCodeHelper::ChangeCodeStatic(const char* srcCode, const char* desCode, const char* pInBuf, size_t* iInLen, char* pOutBuf, size_t* iOutLen)
{
	int iRet = 0;
	iconv_t hIconv = iconv_open(desCode, srcCode);
	if ((iconv_t)-1 == hIconv)
	{
		return -1;

	}
	/*memset(pOutBuf, 0x00,(int)iOutLen);*/

	while (*iInLen > 0)
	{
		iRet = (int)iconv(hIconv, (char**)(&pInBuf), iInLen, (char**)(&pOutBuf), iOutLen);
		if (iRet == -1)
		{
			iconv_close(hIconv);
			return -1;
		}
	}
	iconv_close(hIconv);
	return iRet;
}
