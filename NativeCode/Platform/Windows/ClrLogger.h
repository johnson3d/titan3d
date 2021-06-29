#pragma once
#include "../../base/TypeUtility.h"

enum TR_ENUM(SV_EnumNoFlags=true)
	EClrLogStringType
{
	ObjectAlloc,
	ObjectsAllocdByClass,
	GCStart,
	GCFinish,
};

class TR_CLASS(SV_LayoutStruct = 8)
ClrString
{
public:
	char mString[1024];
	int mSize;
	EClrLogStringType mType;

	ClrString(const char* text = "");
	~ClrString();
	void SetText(const char* text);
	void Append(const char* text);
	void Append(int num);
	int GetSize() const{
		return mSize;
	}
	const char* GetString() const{
		return mString;
	}
};

class TR_CLASS()
ClrLogger
{
	static constexpr int MaxLogInfo = 1024;
	ClrString		mStrings[MaxLogInfo];
	int				mBegin;
	int				mEnd;
public:
	ClrLogger();
	~ClrLogger();

	static void StartClrLogger();
	static ClrLogger* GetInstance();
	static void PushLogInfo(EClrLogStringType type, const char* info) {
		if (GetInstance() != nullptr) {
			GetInstance()->PushLog(type, info);
		}
	}
	static void PushLogInfo(EClrLogStringType type, const ClrString& info) {
		if (GetInstance() != nullptr) {
			GetInstance()->PushLog(type, info.GetString());
		}
	}
	static bool PopLogInfo(ClrString* clrStr) {
		if (GetInstance() != nullptr) {
			return GetInstance()->PopLog(clrStr);
		}
		return false;
	}
	static void StopClrLogger();

	static bool bMessageBox;
	static void SetMessageBox(bool b) {
		bMessageBox = b;
	}
	static void ShowMessageBox(const char* info);

	bool IsFull();
	bool IsEmpty();
	bool PopLog(ClrString* clrStr);
	ClrString* PopLog();
	TR_FUNCTION(SV_NoStringConverter = true)
	const char* PopLogText();
	TR_DISCARD(SV_NoBind = true)
	void PushLog(EClrLogStringType type, const char* info);
};

