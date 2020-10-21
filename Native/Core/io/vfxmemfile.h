// vfxmemfile.h
//
// VictoryCore Code
// class VMemFile
//
// Author : johnson
// Modifer :	
// Create Timer :	2002-6-27  8:50
// Modify Timer :	2002-12-19  9:33
// 
//-----------------------------------------------------------------------------

#ifndef __vfxmemfile_H__27_6_2002_8_50
#define __vfxmemfile_H__27_6_2002_8_50

#pragma once
#include "vfxfile.h"

const int MEM_FILE_PAGE		= 4096;

class VMemFile : public VFile_Base
{
protected:
	typedef struct
	{
		INT8*		memory;
		INT_PTR		length;
		INT_PTR		memlen;
		INT_PTR		position;
		vBOOL		needfree;
	}VMemFileStruct;

	VMemFileStruct		m_MemStruct;

	virtual vBOOL		Open(LPCSTR lpszFileName, UINT nOpenFlags)
	{
		return FALSE;
	}
public:
	 VMemFile();
	 virtual ~VMemFile();

	 vBOOL OpenAndCopy(void* buffer, UINT size);

	 virtual vBOOL Flush(){
		return TRUE;
	}
	 virtual void Close();
	 virtual void Abort(){}

	 virtual UINT_PTR Read(void* lpBuf, UINT_PTR nCount);
	 virtual UINT_PTR Write(const void* lpBuf, UINT_PTR nCount);

	 virtual VFile_Base* Duplicate() const;

	 virtual UINT_PTR GetPosition() const;
	 virtual INT_PTR Seek(INT_PTR lOff, SeekPosition nFrom);
	 virtual vBOOL SetLength(UINT_PTR dwNewLen);
	 virtual UINT_PTR GetLength() const;

	 virtual vBOOL LockRange(UINT_PTR dwPos, UINT_PTR dwCount){
		return TRUE;
	}
	 virtual vBOOL UnlockRange(UINT_PTR dwPos, UINT_PTR dwCount){
		return TRUE;
	}

	 virtual UINT_PTR	GetBufferPtr(BufferCommand nCommand, UINT_PTR nCount = 0,
										void** ppBufStart = NULL, void** ppBufMax = NULL);
	void * GetMemory() const
	{
		return m_MemStruct.memory;
	}

protected:
	 void AllocPage(void);
};

#endif // end __vfxmemfile_H__27_6_2002_8_50
