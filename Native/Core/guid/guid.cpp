#include "../precompile.h"
#include "guid.h"
#include "../string/vfxstring.h"

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wformat"
#endif

#define new VNEW

Guid::Guid(void)
{
	MakeEmpty();
}


Guid::~Guid(void)
{
}

void Guid::ToString(VStringA& str) const
{
	str = VStringA_FormatV("%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",Data1,Data2,Data3,Data4[0],Data4[1],Data4[2],Data4[3]
										,Data4[4],Data4[5],Data4[6],Data4[7]);
}
//void Guid::ToString(VStringW& str) const
//{
//	str = VStringA_FormatV("%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",Data1,Data2,Data3,Data4[0],Data4[1],Data4[2],Data4[3]
//										,Data4[4],Data4[5],Data4[6],Data4[7]);
//}
//void Guid::Parse(const wchar_t* str)
//{
//	GuidGuard temp;
//	swscanf(str, "%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",&temp.Data1,&temp.Data2,&temp.Data3,&temp.Data4[0],&temp.Data4[1],&temp.Data4[2],&temp.Data4[3]
//										,&temp.Data4[4],&temp.Data4[5],&temp.Data4[6],&temp.Data4[7]);
//	memcpy(this, &temp, sizeof(Guid));
//	return;
//}
void Guid::Parse(const char* str)
{
	GuidGuard temp;
	/*INT32 data2 = (INT32)&temp.Data2;
	INT32 data3 = (INT32)&temp.Data3;
	INT32 data40 = (INT32)&temp.Data4[0];
	INT32 data41 = (INT32)&temp.Data4[1];
	INT32 data42 = (INT32)&temp.Data4[2];
	INT32 data43 = (INT32)&temp.Data4[3];
	INT32 data44 = (INT32)&temp.Data4[4];
	INT32 data45 = (INT32)&temp.Data4[5];
	INT32 data46 = (INT32)&temp.Data4[6];
	INT32 data47 = (INT32)&temp.Data4[7];
	sscanf(str,"%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",&temp.Data1, data2
		, data3
		, data40
		, data41
		, data42
		, data43
		, data44
		, data45
		, data46
		, data47);*/

	sscanf(str, "%08X-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X", &temp.Data1, &temp.Data2, &temp.Data3, &temp.Data4[0], &temp.Data4[1], &temp.Data4[2], &temp.Data4[3]
		, &temp.Data4[4], &temp.Data4[5], &temp.Data4[6], &temp.Data4[7]);
	memcpy(this, &temp, sizeof(Guid));
	return;
}

void Guid::MakeEmpty()
{
	memset(&Data1, 0, sizeof(Guid));
}

bool Guid::IsEmpty() const
{
	return Data1==0&&Data2==0&&Data3==0&&Data4[0]==0&&Data4[1]==0&&Data4[2]==0&&Data4[3]==0&&Data4[4]==0&&Data4[5]==0
		&&Data4[6]==0&&Data4[7]==0;
}

bool Guid::operator==(const Guid& rh) const
{
	return memcmp(&Data1,&rh,sizeof(Guid))==0;
}

bool Guid::operator!=(const Guid& rh) const
{
	return memcmp(&Data1,&rh,sizeof(Guid))!=0;
}

bool Guid::operator<(const Guid& rh) const
{
	return memcmp(&Data1,&rh,sizeof(Guid))<0;
}

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif