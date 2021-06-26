#include <WinSock2.h>
#include <mswsock.h>
#include <windows.h>

#include "ClrLogger.h"
#include <stdlib.h>
#include <string.h>

#include "../../base/thread/vfxcritical.h"
#include "../../base/debug/vfxnew.h"

#define new VNEW

VCritical			gClrLogLocker;

ClrString::ClrString(const char* text)
{
	SetText(text);
}

ClrString::~ClrString()
{
	mSize = 0;
}

void ClrString::SetText(const char* text)
{
	mSize = (int)strlen(text);
	if (mSize > 1024)
	{
		mSize = 0;
		mString[mSize] = '\0';
		return;
	}
	mString[mSize] = '\0';
	memcpy(mString, text, mSize);
}

void ClrString::Append(const char* text)
{
	auto len = (int)strlen(text);
	if (mSize + len > 1024)
	{
		return;
	}
	memcpy(&mString[mSize], text, len);
	mSize += len;
	mString[mSize] = '\0';
}

void ClrString::Append(int num)
{

}

ClrLogger* gClrLogger = nullptr;
void ClrLogger::StartClrLogger()
{
	if (gClrLogger == nullptr)
	{
		gClrLogger = new ClrLogger();
	}
}

ClrLogger* ClrLogger::GetInstance()
{
	return gClrLogger;
}

void ClrLogger::StopClrLogger()
{
	delete gClrLogger;
	gClrLogger = nullptr;
}

ClrLogger::ClrLogger()
{
	mBegin = 0;
	mEnd = 0;
}

ClrLogger::~ClrLogger()
{

}

bool ClrLogger::IsEmpty()
{
	return mEnd == mBegin;
}

bool ClrLogger::IsFull()
{
	if (mEnd < MaxLogInfo)
	{
		if (mEnd + 1 == mBegin)
			return true;
	}
	else
	{
		if (mBegin == 0)
			return true;
	}
	return false;
}

bool ClrLogger::PopLog(ClrString* clrStr)
{
	auto info = PopLog();
	if (info == nullptr)
		return false;

	*clrStr = *info;
	return true;
}

ClrString* ClrLogger::PopLog()
{
	VAutoVSLLock lk(gClrLogLocker);
	if (mEnd == mBegin)
		return nullptr;
	auto result = &mStrings[mBegin];

	if (mBegin < MaxLogInfo - 1)
	{
		mBegin++;
	}
	else
	{
		mBegin = 0;
	}

	return result;
}

const char* ClrLogger::PopLogText()
{
	auto clrStr = PopLog();
	if (clrStr == nullptr)
		return nullptr;
	return clrStr->GetString();
}

void ClrLogger::PushLog(EClrLogStringType type, const char* info)
{
	VAutoVSLLock lk(gClrLogLocker);
	if (IsFull())
	{
		PopLog();
	}
	if (mEnd < MaxLogInfo - 1)
	{
		mEnd++;
	}
	else
	{
		mEnd = 0;		
	}
	mStrings[mEnd].mType = type;
	mStrings[mEnd].SetText(info);
}
