// vfxstring.h
// 
// VictoryCore Code
// class VString
//
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time : 2006-8-15
//-----------------------------------------------------------------------------

#ifndef __VFX_STRING_H__
#define __VFX_STRING_H__

#pragma once

#include "../BaseHead.h"
#include <string>
#include "../debug/vfxdebug.h"

typedef std::string			VStringA;
typedef std::wstring		VStringW;

void VStringA_MakeLower(VStringA& str);
void VStringA_MakeUpper(VStringA& str);
void VStringA_ReplaceAll(VStringA& str, const char* oldStr, const char* newStr);
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh, int num);
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh);
bool VStringA_Contains(const VStringA& str, LPCSTR rh);
VStringA VStringA_FormatV(LPCSTR _format, ...);
VStringA VStringA_Gbk2Utf8(LPCSTR gbk);
VStringA VStringA_Utf82Gbk(LPCSTR utf8);

class StringHelper
{
public:
	static std::vector<std::string> split(const  std::string& s, const std::string& delim)
	{
		std::vector<std::string> elems;
		size_t pos = 0;
		size_t len = s.length();
		size_t delim_len = delim.length();
		if (delim_len == 0)
			return elems;
		while (pos < len)
		{
			auto find_pos = s.find(delim, pos);
			if (find_pos == std::string::npos)
			{
				elems.push_back(s.substr(pos, len - pos));
				break;
			}
			elems.push_back(s.substr(pos, find_pos - pos));
			pos = find_pos + delim_len;
		}
		return elems;
	}
};


struct VNameString_t
{
	int Index;
};

#pragma pack(push)
#pragma pack(4)
struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 4)
VNameString
{
	int Index;
	static std::vector<std::string>		mNameStrings;
	static int GetIndexFromString(const char* str);

	VNameString() 
		: Index(-1)
	{

	}
	VNameString(const char* str)
	{
		Index = GetIndexFromString(str);
	}
	std::string AsStdString() const
	{
		return GetString();
	}
	inline const char* c_str() const
	{
		return GetString();
	}
	inline const char* GetString() const
	{
		if (Index < 0 || Index >= mNameStrings.size())
			return nullptr;
		return mNameStrings[Index].c_str();
	}
	inline void SetString(const char* str)
	{
		Index = GetIndexFromString(str);
	}
	inline operator const char*()
	{
		return GetString();
	}
	inline operator std::string ()
	{
		return GetString();
	}
	inline void operator=(const VNameString& rh)
	{
		Index = rh.Index;
	}
	inline void operator=(const char* rh)
	{
		Index = GetIndexFromString(rh);
	}
	inline void operator=(const std::string rh)
	{
		Index = GetIndexFromString(rh.c_str());
	}	
};
#pragma pack(pop)

inline bool operator==(const VNameString& lh, const VNameString& rh)
{
	return lh.Index == rh.Index;
}
inline bool operator!=(const VNameString& lh, const VNameString& rh)
{
	return lh.Index != rh.Index;
}
inline bool operator==(const VNameString& lh, const char* rh)
{
	return strcmp(lh.GetString(), rh) == 0;
}
inline bool operator!=(const VNameString& lh, const char* rh)
{
	return strcmp(lh.GetString(), rh) != 0;
}
inline bool operator<=(const VNameString& lh, const char* rh)
{
	return strcmp(lh.GetString(), rh) <= 0;
}
inline bool operator>=(const VNameString& lh, const char* rh)
{
	return strcmp(lh.GetString(), rh) >= 0;
}

#endif // end __VFX_STRING_H__