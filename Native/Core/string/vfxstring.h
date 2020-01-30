// vfxstring.h
// 
// VictoryCore Code
// class VString
//
// Author : songyijiang(���佮)
// More author :
// Create time : 2002-6-13
// Modify time : 2006-8-15
//-----------------------------------------------------------------------------

#ifndef __VFX_STRING_H__
#define __VFX_STRING_H__

#pragma once

#include "../../BaseHead.h"
#include <string>
#include "../debug/vfxdebug.h"

//INT_PTR __cdecl _lwcstombsz(LPSTR mbstr, LPCSTR wcstr, size_t mbs_buffer_size,size_t wcs_len);
//INT_PTR __cdecl _lmbstowcsz(LPWSTR wcstr, LPCSTR mbstr, size_t wcs_buffer_size,size_t mbslen);
//
//#include "xchar_undef.h"
//#include "uchar_def.h"
//namespace vfx_unicode
//{
//#include "vfxstring_imp.header.h"
//}
//
//#include "xchar_undef.h"
//#include "schar_def.h"
//namespace vfx_singlebyte
//{
//#include "vfxstring_imp.header.h"
//}
//
//typedef vfx_unicode::XString		VString;
//typedef vfx_singlebyte::XString		VStringA;

//class VString : public VStringW
//{
//public:
//	typedef wchar_t char_type;
//	VString()
//		: VStringW()
//	{		
//	}
//	VString(LPCSTR str)
//		: VStringW(str)
//	{
//	}
//	VString(LPCSTR str,size_t len)
//		: VStringW(str,len)
//	{
//	}
//	 VString(LPCSTR str);
//	 VString(VBaseStringA str);
//	 void MakeLower();
//	 void MakeUpper();
//	 bool IsEmpty() const;
//	 void Empty();
//	 LPWSTR GetBufferSetLength(size_t size);
//
//	 void Delete(size_t start,size_t len);
//	 VString Left(size_t len);
//	 VString Right(size_t len);
//	 VString Mid(size_t start,size_t len);
//	 size_t Find(wchar_t c,size_t start=0);
//	 size_t Find(LPCSTR str,size_t start=0);
//	 size_t ReverseFind(wchar_t c);
//
//	 LPWSTR GetBuffer(size_t size);
//	 void ReleaseBuffer();
//	 void ReplaceAll(const wchar_t* oldStr,const wchar_t* newStr);
//
//	 VString operator= (LPCSTR rh);
//	 VString operator= (LPCSTR rh);
//	 VString operator= (const VStringW& rh);
//	 bool operator==(LPCSTR rh) const;
//	 bool operator==(const VStringW& rh) const;
//	inline bool operator!=(LPCSTR rh) const
//	{
//		return !operator==(rh);
//	}
//	inline bool operator!=(const VStringW& rh) const
//	{
//		return !operator==(rh);
//	}
//
//	 int CompareNoCase(LPCSTR rh) const;
//	 int CompareNoCase(LPCSTR rh,int num) const;
//
//	operator LPCSTR() const{
//		return this->c_str();
//	}
//
//	 VString Format(LPCSTR _format,...);
//	 static VString FormatV(LPCSTR _format,...);
//};

#if defined(V_STL_USE_ALLOCATOR)
#define STR_USE_ALLOCATOR
#endif

#if defined(STR_USE_ALLOCATOR)
namespace StrMem
{
	struct StringAllocator
	{
		static void * Malloc(size_t size)
		{
			//return _vfxMemoryNew(size, __FILE__, __LINE__);
			return _vfxMemoryNew(size, __FILE__, 0);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};
}

typedef std::basic_string<char, std::char_traits<char>, VMem::malloc_allocator<char, StrMem::StringAllocator> > VBaseStringA;
typedef std::basic_string<wchar_t, std::char_traits<wchar_t>, VMem::malloc_allocator<wchar_t, StrMem::StringAllocator> > VBaseStringW;
#else
typedef std::string VBaseStringA;
typedef VStringW VBaseStringW;
#endif

typedef VBaseStringA VStringA;
typedef VBaseStringW VStringW;

//class VStringA// : public VBaseStringA
//{
//public:
//	VBaseStringA	mBaseString;
//	typedef VBaseStringA::size_type size_type;
//	static const size_type npos;	// bad/missing length/position
//public:
//	static VStringA EmtpyString;
//	static int VStringANumber;
//	typedef char char_type;
//	VStringA()
//	{		
//		VStringANumber++;
//	}
//	VStringA(LPCSTR str)
//	{
//		mBaseString = str;
//		VStringANumber++;
//	}
//	VStringA(LPCSTR str,size_t len)
//	{
//		char* cutStr = new(__FILE__,__LINE__) char[len + 1];
//		size_t srcLen = (size_t)strlen(str);
//		if (srcLen < len)
//		{
//			ASSERT(false);
//			len = srcLen;
//		}
//		memcpy(cutStr, str, len);
//		cutStr[len] = '\0';
//		mBaseString = cutStr;
//		delete[] cutStr;
//		VStringANumber++;
//	}
//	~VStringA()
//	{
//		mBaseString = "";
//		VStringANumber--;
//	}
//	inline size_t size() const {
//		return mBaseString.size();
//	}
//	inline size_t length() const{
//		return mBaseString.length();
//	}
//	const char* c_str() const{
//		return mBaseString.c_str();
//	}
//	VStringA substr(size_type _Off = 0, size_type _Count = npos) const {
//		VStringA result;
//		result.mBaseString = mBaseString.substr(_Off, _Count);
//		return result;
//	}
//	int compare(const char*_Ptr) const {
//		return mBaseString.compare(_Ptr);
//	}
//	inline bool IsEmpty() const
//	{
//		return mBaseString.empty();
//	}
//	inline void Empty()
//	{
//		mBaseString.resize(0);
//	}
//	inline size_type find_last_of(char _Ch,
//		size_type _Off = npos) const
//	{	// look for _Ch before _Off
//		return mBaseString.find_last_of(_Ch, _Off);
//	}
//	inline size_type find_first_of(const char *_Ptr,
//		size_type _Off, size_type _Count) const
//	{
//		return mBaseString.find_first_of(_Ptr, _Off, _Count);
//	}
//	inline size_type find_first_of(const char *_Ptr,
//		size_type _Off = 0) const
//	{
//		return mBaseString.find_first_of(_Ptr, _Off);
//	}
//	inline size_type find_first_of(char _Ch,
//		size_type _Off = 0) const
//	{
//		return mBaseString.find_first_of(_Ch, _Off);
//	}
//	size_type find_first_not_of(const char*_Ptr,
//		size_type _Off = 0) const
//	{	// look for one of [_Ptr, <null>) at or after _Off
//		return mBaseString.find_first_not_of(_Ptr, _Off);
//	}
//	// VStringA(LPCSTR str);
//	// VStringA(VStringW str);
//	 void MakeLower();
//	 void MakeUpper();
//	// LPSTR GetBufferSetLength(size_t size);
//
//	 void Delete(size_t start,size_t len);
//	 VStringA Left(size_t len);
//	 VStringA Right(size_t len);
//	 VStringA Mid(size_t start,size_t len);
//
//	 size_t Find(char c,size_t start=0) const;
//	 size_t Find(LPCSTR str,size_t start=0) const;
//	 size_t ReverseFind(char c);
//
//	 LPSTR GetBuffer(size_t size);
//	 void ReleaseBuffer();
//
//	 void ReplaceAll(const char* oldStr,const char* newStr);
//	
//	VStringA& replace(size_type _Off, size_type _N0, const char*_Ptr)
//	{	// replace [_Off, _Off + _N0) with [_Ptr, <null>)
//		mBaseString.replace(_Off, _N0, _Ptr);
//		return *this;
//	}
//	inline VStringA operator= (LPCSTR rh) {
//		mBaseString = rh;
//		return *this;
//	}
//	inline VStringA operator= (const VStringA& rh) {
//		mBaseString = rh;
//		return *this;
//	}
//	inline VStringA operator +(const char* rh) const {
//		VStringA result;
//		result.mBaseString = mBaseString + rh;
//		return result;
//	}
//	inline VStringA operator +(const VStringA rh) const {
//		VStringA result;
//		result.mBaseString = mBaseString + rh.mBaseString;
//		return result;
//	}
//	inline VStringA operator += (const char* rh){
//		mBaseString.operator+=(rh);
//		return *this;
//	}
//	inline VStringA operator += (const VStringA rh){
//		mBaseString.operator+=(rh.mBaseString);
//		return *this;
//	}
//
//	 int CompareNoCase(LPCSTR rh) const;
//	 int CompareNoCase(LPCSTR rh,int num) const;
//
//	operator LPCSTR() const{
//		return mBaseString.c_str();
//	}	
//
//	 VStringA Format(LPCSTR _format,...);
//	 static VStringA FormatV(LPCSTR _format,...);
//
//	 static VStringA Gbk2Utf8(LPCSTR gbk);
//	 static VStringA Utf82Gbk(LPCSTR utf8);
//	 static VStringW Ansi2Unicode(LPCSTR gbk);
//	 static VStringA Unicode2Ansi(LPCWSTR unicode);
//	// static VString Utf82Utf16(LPCSTR utf8);
//
//	inline bool Contains(LPCSTR str) const
//	{
//		return Find(str) == VBaseStringA::npos ? false : true;
//	}
//};
//
//inline bool operator<(const VStringA _Left, const char *_Right)
//{	// test if string < NTCS
//	return (_Left.compare(_Right) < 0);
//}
//inline bool operator<(const char * _Left, const VStringA& _Right)
//{	// test if NTCS < string
//	return (_Right.compare(_Left) > 0);
//}
//inline bool operator<(const VStringA& _Left, const VStringA& _Right)
//{	// test if string < string
//	return (_Left.compare(_Right) < 0);
//}
//inline bool operator>(const VStringA& _Left, const VStringA& _Right)
//{
//	return (_Right < _Left);
//}
//inline bool operator>(const char* _Left, const VStringA& _Right)
//{
//	return (_Right < _Left);
//}
//inline bool operator>(const VStringA& _Left, const char *_Right)
//{	// test if string > NTCS
//	return (_Right < _Left);
//}
//inline bool operator<=(const VStringA& _Left, const VStringA& _Right)
//{	// test if string <= string
//	return (!(_Right < _Left));
//}
//inline bool operator<=(const char * _Left, const VStringA& _Right)
//{	// test if NTCS <= string
//	return (!(_Right < _Left));
//}
//inline bool operator<=(const VStringA& _Left, const char *_Right)
//{	// test if string <= NTCS
//	return (!(_Right < _Left));
//}
//inline bool operator>=(const VStringA& _Left, const VStringA& _Right)
//{	// test if string >= string
//	return (!(_Left < _Right));
//}
//inline bool operator>=(const char * _Left, const VStringA& _Right)
//{	// test if NTCS >= string
//	return (!(_Left < _Right));
//}
//inline bool operator>=(const VStringA& _Left, const char* _Right)
//{	// test if string >= NTCS
//	return (!(_Left < _Right));
//}
//
//inline bool operator==(const char* _Left,const VStringA& _Right)
//{
//	return (_Right.compare(_Left) == 0);
//}
//inline bool operator==(const VStringA& _Left,const char *_Right)
//{	// test for string vs. NTCS equality
//	return (_Left.compare(_Right) == 0);
//}
//inline bool operator==(const VStringA& _Left,const VStringA& _Right)
//{	// test for string equality
//	return (_Left.compare(_Right) == 0);
//}
//inline bool operator!=(const VStringA& _Left,const VStringA& _Right)
//{	// test for string inequality
//	return (!(_Left == _Right));
//}
//inline bool operator!=(const char *_Left,const VStringA& _Right)
//{	// test for NTCS vs. string inequality
//	return (!(_Left == _Right));
//}
//inline bool operator!=(const VStringA& _Left,const char *_Right)
//{	// test for string vs. NTCS inequality
//	return (!(_Left == _Right));
//}

typedef VStringA	VString;

#ifndef INOUT
#define INOUT 
#endif

void VStringA_MakeLower(INOUT VStringA& str);
void VStringA_MakeUpper(INOUT VStringA& str);
void VStringA_ReplaceAll(INOUT VStringA& str, const char* oldStr, const char* newStr);
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh, int num);
int VStringA_CompareNoCase(const VStringA& str, LPCSTR rh);
bool VStringA_Contains(const VStringA& str, LPCSTR rh);
VStringA VStringA_FormatV(LPCSTR _format, ...);
VStringW VStringA_Ansi2Unicode(LPCSTR gbk);
VStringA VStringA_Unicode2Ansi(LPCWSTR unicode);
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

inline VString vfxGetFileName(const VString& fullPath)
{
	auto pos = fullPath.find_first_of('.');
	return VString(fullPath.substr(pos, fullPath.length() - pos).c_str());
}

struct VStringIO
{
	template<class _typeio>
	static void Save(_typeio& io, VStringA& str)
	{
		INT nLen = (INT)str.length();
		io.Write(&nLen, sizeof(INT));

		if (nLen > 0)
		{
			io.Write( &str[0] , nLen*sizeof(char) );
		}
	}
	template<class _typeio>
	static void Load(_typeio& io, VStringA& str)
	{
		INT nLen;
		io.Read(&nLen, sizeof(INT));
		if (nLen > 0)
		{
#if defined(_DEBUG) && !defined(UNNEW)
			char* pChar = new(__FILE__,__LINE__) char[nLen + 1];
#else
			char* pChar = new char[nLen + 1];
#endif
			io.Read( pChar , nLen*sizeof(char) );
			pChar[nLen] = '\0';
			VString load_str = pChar;
			str = load_str.c_str();
			delete[] pChar;
		}
		else
		{
			str = "";
		}
	}
};

#endif // end __VFX_STRING_H__
