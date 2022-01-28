// vfxstring.cpp
// 
// VictoryCore Code
// class VString
//
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time : 2006-8-15
//-----------------------------------------------------------------------------
#include "../BaseHead.h"
#include "vfxstring.h"
#include "../TextConverter/WordCodeHelper.h"
#include "../thread/vfxcritical.h"

#define new VNEW

static char cbuf[BUFSIZ];
static wchar_t wbuf[BUFSIZ];

char* wchar_to_char(const wchar_t* text, int len)
{
	if(!text)
		return NULL;

	//	int wlen = (len == 0) ? wcslen(text) : len;

	int index = 0;

	for (int i = 0; ; i++)
	{
		unsigned short ch = text[i];

		if (0x800 <= ch)
		{
			cbuf[index++] = ((char)(ch >> 12) & 0x0f ) | 0xe0;
			cbuf[index++] = ((char)(ch >> 6) & 0x3f ) | 0x80;
			cbuf[index++] = (char)(ch & 0x3f) | 0x80;
		}
		else if (0x80 <= ch)
		{
			cbuf[index++] = ((char)(ch >> 6) & 0x1f ) | 0xC0;
			cbuf[index++] = (char)(ch & 0x3f) | 0x80;
		}
		else
		{
			if( ch == 0 ){
				break;
			}

			cbuf[index++] = (char)ch;
		}
	}

	cbuf[index] = '\0';
	//#endif

	// memory copy
	size_t size = strlen(cbuf) + 1;
	char* ctext = new char[size];
	memcpy(ctext, cbuf, size);

	return ctext;
}

wchar_t* char_to_wchar(const char* text, int len)
{
	if(!text)
		return NULL;

	size_t clen = (len == 0) ? strlen(text) : len;

	const unsigned char* temp = (const unsigned char*)text;
	int index = 0;

	for (size_t i = 0; i < clen; )
	{
		if ( (0xf0 & temp[i]) == 0xe0 )
		{
			wbuf[index++] = ((wchar_t)(temp[i] & 0x0f) << 12 | (wchar_t)(temp[i + 1] & 0x3f) << 6 | (wchar_t)(temp[i + 2] & 0x3f));
			i += 3;
		}
		else if ( (0xe0 & temp[i]) == 0xc0 )
		{
			wbuf[index++] = ((wchar_t)(temp[i] & 0x1f) << 6 | ((wchar_t)temp[i + 1] & 0x3f));
			i += 2;
		}
		else if ( (0xc0 & temp[i]) == 0x80 ){
			i += 1;
			continue;
		}
		else
		{
			wbuf[index++] = (wchar_t)temp[i];
			i += 1;
		}
	}

	wbuf[index] = '\0';
	//#endif

	// memory copy
	size_t size = wcslen(wbuf) + 1;
	wchar_t* wtext = new wchar_t[size + 1];
	for (size_t i = 0; i < size; i++)
	{
		wtext[i] = wbuf[i];
	}
	//px_memcpy(wtext, wbuf, size);

	return wtext;
}

#define MAX_BUFFLEN 4096


void VStringA_MakeLower(VStringA &str)
{
	std::transform(str.begin(), str.end(), str.begin(), tolower);
}

void VStringA_MakeUpper(VStringA &str)
{
	std::transform(str.begin(), str.end(), str.begin(), toupper);
}
void VStringA_ReplaceAll(VStringA& str, const char* oldStr, const char* newStr)
{
	size_t olen = strlen(oldStr);
	VStringA tempStr = str;
	while (true)
	{
		VStringA::size_type   pos(0);
		if ((pos = tempStr.find(oldStr)) != VStringA::npos)
			tempStr = tempStr.replace(pos, olen, newStr).c_str();
		else
			break;
	}
	str = tempStr;
}
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh)
{
	return VStringA_CompareNoCase(str, rh, (int)strlen(rh));
}
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh, int num)
{
	size_t len = strlen(rh);
	if (num >= 0 && (size_t)num < len)
		len = (size_t)num;
	VStringA::const_iterator p = str.begin();
	size_t p2 = 0;
	while (p != str.end() && p2 != len)
	{
		if (toupper(*p) != toupper(rh[p2]))
			return (toupper(*p) < toupper(rh[p2])) ? -1 : 1;
		++p;
		++p2;
	}
	if (num == -1)
		return 0;
	return (len == str.size()) ? 0 : (str.size() < len) ? -1 : 1;
}
bool VStringA_Contains(const VStringA& str, LPCSTR rh)
{
	return str.find(rh) != VStringA::npos;
}
VStringA VStringA_FormatV(LPCSTR _format, ...)
{
	char szBuffer[MAX_BUFFLEN];
	memset(szBuffer, 0, sizeof(szBuffer));
	va_list ap;
	va_start(ap, _format);
	//try
	{
#ifdef WIN32
		_vsnprintf_s(szBuffer, MAX_BUFFLEN, MAX_BUFFLEN, _format, ap);
		//sprintf(szBuffer,_format,ap);
#else
		vsnprintf(szBuffer, MAX_BUFFLEN, _format, ap);
#endif

	}
	//catch (...)
	{
		//return VStringA("");
	}
	va_end(ap);
	return VStringA(szBuffer);
}


VStringA VStringA_Gbk2Utf8(LPCSTR gbk)
{
	if (gbk == NULL)
		return VStringA("");
	size_t srcLen = strlen(gbk);
	size_t dstLen = srcLen * 5 + 1;
	char* dst = new char[dstLen];
	memset(dst, 0, sizeof(char) * dstLen);
	WordCodeHelper::ChangeCodeStatic("GB18030//TRANSLIT", "UTF-8", gbk, &srcLen, (char*)dst, &dstLen);
	VStringA result = dst;
	delete[] dst;
	return result;
}

VStringA VStringA_Utf82Gbk(LPCSTR utf8)
{
	if (utf8 == NULL)
		return VStringA("");
	size_t srcLen = strlen(utf8);
	size_t dstLen = (srcLen * 5) + 1;
	char* dst = new char[dstLen];
	memset(dst, 0, sizeof(char) * dstLen);
	WordCodeHelper::ChangeCodeStatic("UTF-8", "GB18030//TRANSLIT", utf8, &srcLen, (char*)dst, &dstLen);
	VStringA result = dst;
	delete[] dst;
	return result;
}

static VSLLock		gNameLocker;

int VNameStringManager::GetIndexFromString(const char* str)
{
	VAutoVSLLock lk(gNameLocker);
	for (int i = 0; i < (int)mNameStrings.size(); i++)
	{
		if (mNameStrings[i] == str)
			return i;
	}
	mNameStrings.push_back(str);
	return (int)(mNameStrings.size() - 1);
}

const std::string& VNameStringManager::GetString(int Index) const
{
	VAutoVSLLock lk(gNameLocker);
	if (Index < 0 || Index >= mNameStrings.size())
	{
		static std::string tmp;
		return tmp;
	}
	return mNameStrings[Index];
}

VNameStringManager* VNameStringManager::Get()
{
	static VNameStringManager Manager;
	return &Manager;
}

