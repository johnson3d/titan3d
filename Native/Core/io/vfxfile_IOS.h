#pragma once

#include "vfxfile.h"

class VFile_IOS : public VFile
{
public:
	VFile_IOS();
	VFile_IOS(FILE* hFile);
	VFile_IOS(LPCSTR lpszFileName, UINT nOpenFlags);

	virtual ~VFile_IOS();

	virtual vBOOL Open(LPCSTR lpszFileName, UINT nOpenFlags);
	virtual bool IsFileOpened() const
	{
		if (m_hFile)
			return true;
		return false;
	}

	virtual void Close();
	virtual void Abort();

	virtual UINT_PTR	Read(void* lpBuf, UINT_PTR nCount);
	virtual UINT_PTR	Write(const void* lpBuf, UINT_PTR nCount);

	virtual UINT_PTR	GetPosition() const;
	virtual INT_PTR	Seek(INT_PTR lOff, SeekPosition nFrom);
	virtual vBOOL		SetLength(UINT_PTR dwNewLen);
	virtual UINT_PTR	GetLength() const;
};
