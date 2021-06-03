#pragma once

#include "vfxfile.h"

struct AAssetManager;
struct AAsset;

class VFile_Android : public VFile
{
protected:
	AAsset* m_pAsset;
public:
	VFile_Android();
	VFile_Android(FILE* hFile);
	VFile_Android(LPCSTR lpszFileName, UINT nOpenFlags);

	virtual ~VFile_Android();

	virtual vBOOL Open(LPCSTR lpszFileName, UINT nOpenFlags);

	virtual bool IsFileOpened() const
	{
		if (m_hFile)
			return true;
		return m_pAsset ? true : false;
	}

	bool IsOpendFromAsset() {
		return m_pAsset ? true : false;
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
