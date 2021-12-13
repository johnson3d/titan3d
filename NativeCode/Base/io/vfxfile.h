// vfxfile.h
// 
// VictoryCore Code
// class VFile
//
// Author : johnson&lzp
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------

#ifndef __VFX_FILE_H__
#define __VFX_FILE_H__

#pragma once
#include "../string/vfxstring.h"

namespace EngineNS
{
	class XNDNode;
}

class VTime
{
public:

	static VTime PASCAL GetCurrentTime(){	
		return VTime(::time(NULL));
	}

	VTime(){}
	VTime(time_t time){
		m_time = time; 
	}
	VTime(const VTime& timeSrc){
		m_time = timeSrc.m_time; 
	}
	 VTime(WORD wDosDate, WORD wDosTime, INT32 nDST = -1);
	 VTime(INT32 nYear, INT32 nMonth, INT32 nDay, INT32 nHour, INT32 nMin, INT32 nSec,
		INT32 nDST = -1);

	 VTime(const SYSTEMTIME& sysTime, INT32 nDST = -1);
	
	VTime& operator=(const VTime& timeSrc){ 
		m_time = timeSrc.m_time; return *this; 
	}
	VTime& operator=(time_t t){
		m_time = t; return *this; 
	}

// Attributes
	 struct tm* GetGmtTm(struct tm* ptm = NULL) const;
	 struct tm* GetLocalTm(struct tm* ptm = NULL) const;
	vBOOL  GetAsSystemTime(SYSTEMTIME& timeDest) const;

	time_t GetTime() const{ 
		return m_time; 
	}
	INT GetYear() const{ 
		return (GetLocalTm(NULL)->tm_year) + 1900; 
	}
	INT GetMonth() const{ 
		return GetLocalTm(NULL)->tm_mon + 1; 
	}
	INT GetDay() const{ 
		return GetLocalTm(NULL)->tm_mday; 
	}
	INT GetHour() const{ 
		return GetLocalTm(NULL)->tm_hour; 
	}
	INT GetMinute() const{ 
		return GetLocalTm(NULL)->tm_min; 
	}
	INT GetSecond() const{ 
		return GetLocalTm(NULL)->tm_sec; 
	}
	INT GetDayOfWeek() const{ 
		return GetLocalTm(NULL)->tm_wday + 1; 
	}

// Operations
	// time math
	vBOOL operator==(VTime time) const{ 
		return m_time == time.m_time; 
	}
	vBOOL operator!=(VTime time) const{ 
		return m_time != time.m_time; 
	}
	vBOOL operator<(VTime time) const{ 
		return m_time < time.m_time; 
	}
	vBOOL operator>(VTime time) const{ 
		return m_time > time.m_time; 
	}
	vBOOL operator<=(VTime time) const{
		return m_time <= time.m_time; 
	}
	vBOOL operator>=(VTime time) const{
		return m_time >= time.m_time; 
	}

	// formatting using "C" strftime
	 VStringA Format(LPCSTR pFormat) const;
	 VStringA FormatGmt(LPCSTR pFormat) const;
private:
	time_t m_time;
};

class VFile_Base
{
public:
	enum SeekPosition { begin = 0x0, current = 0x1, end = 0x2 };
	enum BufferCommand { bufferRead, bufferWrite, bufferCommit, bufferCheck };

	virtual  ~VFile_Base()
	{
	}

	virtual vBOOL		Open(LPCSTR lpszFileName, UINT nOpenFlags) = 0;
	virtual vBOOL		Flush() = 0;
	virtual void		Close() = 0;
	virtual void		Abort() = 0;

	virtual UINT_PTR	Read(void* lpBuf, UINT_PTR nCount) = 0;
	virtual UINT_PTR	Write(const void* lpBuf, UINT_PTR nCount) = 0;

	virtual VFile_Base* Duplicate() const = 0;

	virtual UINT_PTR	GetPosition() const = 0;
	virtual INT_PTR	Seek(INT_PTR lOff, SeekPosition nFrom) = 0;
	virtual vBOOL		SetLength(UINT_PTR dwNewLen) = 0;
	virtual UINT_PTR	GetLength() const = 0;

	virtual UINT_PTR	GetBufferPtr(BufferCommand nCommand, UINT_PTR nCount = 0,
										void** ppBufStart = NULL, void** ppBufMax = NULL) = 0;
};

class VFile : public VFile_Base
{
public:
// Flag values
	enum OpenFlags {
		modeRead =          0x0000,
		modeWrite =         0x0001,
		modeReadWrite =     0x0002,
		shareDenyWrite =    0x0020,
		shareDenyRead =     0x0030,
		shareDenyNone =     0x0040,
		modeNoInherit =     0x0080,
		modeCreate =        0x1000,
		modeNoTruncate =    0x2000,
		typeText =          0x4000, // typeText and typeBinary are used in
		typeBinary =   (INT32)0x8000 // derived classes only
		};

	enum Attribute {
		normal =    0x00,
		readOnly =  0x01,
		hidden =    0x02,
		system =    0x04,
		volume =    0x08,
		directory = 0x10,
		archive =   0x20
		};

// Constructors
	 VFile();
	 VFile(FILE* hFile);
	 VFile(LPCSTR lpszFileName, UINT nOpenFlags);

// Attributes
	
	operator FILE*() const{
		return m_hFile; 
	}

	virtual bool IsFileOpened() const
	{
		return m_hFile?true:false;
	}
	
	virtual UINT_PTR  GetPosition() const;
	virtual VStringA  GetFileName() const {
		return m_strFileName;
	}

// Operations
	virtual vBOOL  Open(LPCSTR lpszFileName, UINT nOpenFlags);
	virtual vBOOL  DefinitlyOpen(LPCSTR lpszFileName, UINT nOpenFlags);
	
	UINT_PTR SeekToEnd(){
		return Seek(0, end); 
	}
	void SeekToBegin(){
		Seek(0, begin);
	}

	// backward compatible ReadHuge and WriteHuge
	UINT_PTR ReadHuge(void* lpBuffer, UINT_PTR dwCount){
		return Read(lpBuffer,dwCount);
	}
	UINT_PTR WriteHuge(const void* lpBuffer, UINT_PTR dwCount){
		return Write(lpBuffer,dwCount);
	}

// Overridables
	virtual  VFile_Base* Duplicate() const;

	virtual INT_PTR  Seek(INT_PTR lOff, SeekPosition nFrom);
	virtual vBOOL  SetLength(UINT_PTR dwNewLen);
	virtual UINT_PTR  GetLength() const;

	virtual UINT_PTR  Read(void* lpBuf, UINT_PTR nCount);
	virtual UINT_PTR  Write(const void* lpBuf, UINT_PTR nCount);

	virtual vBOOL  Flush();
	virtual void  Abort();
	virtual void  Close();

// Implementation
public:
	virtual  ~VFile();

	virtual UINT_PTR  GetBufferPtr(BufferCommand nCommand, UINT_PTR nCount = 0,void** ppBufStart = NULL, void** ppBufMax = NULL);

	static int MakesureDir(LPCSTR path);
protected:
	UINT_PTR mFileLength;
	FILE* m_hFile;
	vBOOL m_bCloseOnDelete;
	VStringA m_strFileName;
};

#if defined(PLATFORM_DROID)
#include "vfxfile_Android.h"
#define ViseFile VFile_Android
#elif defined(PLATFORM_IOS)
#include "vfxfile_IOS.h"
#define ViseFile VFile_IOS
#elif defined(PLATFORM_WIN)
#define ViseFile VFile
#else
#define ViseFile VFile
#endif

#endif	//__VFX_FILE_H__