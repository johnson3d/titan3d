#pragma once

//#include "../../BaseHead.h"

#if defined(PLATFORM_WIN)
#pragma warning(disable:4819)
#include "../../../3rd/native/iconv/iconv.h"
#elif defined(PLATFORM_DROID)
#include "../../../3rd/native/iconv/iconv.h"
#elif defined(PLATFORM_IOS)
#include <iconv.h>
#endif

//#include "../../BaseHead.h"


class WordCodeHelper
{
public:
	const char* mOriginCode;
	const char* mDestCode;
public:
	static const char* SrcCode;
	static const char* DesCode;
public:
	WordCodeHelper();
	~WordCodeHelper();
	void SetOriginCode(const char* orginCode);
	void SetDestCode(const char* destCode);
	size_t ChangeCode(
		const char* pInBuf,
		size_t* iInLen,
		char* pOutBuf,
		size_t* iOutLen);
 	static int ChangeCodeStatic(const char* srcCode, const char* desCode, const char* pInBuf, size_t* iInLen, char* pOutBuf, size_t* iOutLen);
};
