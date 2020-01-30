#pragma once

#include "../string/vfxstring.h"

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunused-private-field"
#endif

#pragma pack(push,4)
class Guid
{
public:
#ifndef IOS
	unsigned long	Data1;
#else
    unsigned int    Data1;
#endif
    unsigned short	Data2;
    unsigned short	Data3;
    unsigned char   Data4[8];
    
public:
	 Guid(void);
	 ~Guid(void);

	 void ToString(VStringA& str) const;
	// void ToString(VStringW& str) const;
	// void Parse(const wchar_t* str);
	 void Parse(const char* str);

	 void MakeEmpty();
	 bool IsEmpty() const;

	 bool operator==(const Guid& rh) const;
	 bool operator!=(const Guid& rh) const;
	 bool operator<(const Guid& rh) const;
};
class GuidGuard : public Guid
{
	int unused;
};
#pragma pack(pop)
#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif