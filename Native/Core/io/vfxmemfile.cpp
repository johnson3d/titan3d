// vfxmemfile.cpp
//
// VictoryCore Code
// class VMemFile
//
// Author : lanzhengpeng(À¼Õ÷Åô)
// Modifer :	
// Create Timer :	2002-6-27  8:59
// Modify Timer :	2002-12-19  9:34
// 
//-----------------------------------------------------------------------------
#ifdef  PLATFORM_IOS
#include <stdlib.h>
#else
#include <malloc.h>
#endif //  IOS
#include "../precompile.h"

#define new VNEW

//	VMemFileStruct		m_MemStruct;
UINT_PTR VMemFile::GetPosition() const
{
	ASSERT(m_MemStruct.memory);
	return m_MemStruct.position;
}

// Operations
vBOOL VMemFile::OpenAndCopy(void* buffer, UINT size)
{
	ASSERT(m_MemStruct.memory == NULL);

	m_MemStruct.memory = (INT8 *)malloc(size);
	if (buffer)
	{
		memcpy(m_MemStruct.memory, buffer, size);
	}
	m_MemStruct.position = 0;
	m_MemStruct.length = size;
	m_MemStruct.memlen = size;
	m_MemStruct.needfree = TRUE;

	return TRUE;
}
// Overridables
VFile_Base* VMemFile::Duplicate() const
{
	ASSERT(this != NULL);
	ASSERT(m_MemStruct.memory != NULL);

	VMemFile* pFile = new VMemFile();
	pFile->m_MemStruct.needfree = m_MemStruct.needfree;
	pFile->m_MemStruct = m_MemStruct;

	if(m_MemStruct.needfree)
	{
		pFile->m_MemStruct.memory = (INT8 *)malloc(m_MemStruct.memlen);
		memmove(pFile->m_MemStruct.memory,m_MemStruct.memory,m_MemStruct.length);
	}

	return pFile;
}

INT_PTR VMemFile::Seek(INT_PTR lOff, SeekPosition nFrom)
{
	ASSERT(m_MemStruct.memory);
	
	INT_PTR oldPos = m_MemStruct.position;
	switch(nFrom)
	{
	case VFile::begin:
		m_MemStruct.position = lOff;
		break;
	case VFile::current:
		m_MemStruct.position += lOff;
		break;
	case VFile::end:
		m_MemStruct.position = m_MemStruct.length - lOff;
		break;
	}
	if(m_MemStruct.position >= m_MemStruct.length)
		m_MemStruct.position = m_MemStruct.length;
	else if(m_MemStruct.position < 0)
		m_MemStruct.position = 0;
	
	return oldPos;
}

vBOOL VMemFile::SetLength(UINT_PTR dwNewLen)
{
	if((INT_PTR)dwNewLen > m_MemStruct.memlen)
	{
		if(m_MemStruct.needfree)
		{
			UINT_PTR alllen = ((dwNewLen + MEM_FILE_PAGE - 1) / MEM_FILE_PAGE) * MEM_FILE_PAGE;
			void * pNewMemory = realloc(m_MemStruct.memory,alllen);
			if(pNewMemory == NULL)
				VFX_LTRACE(ELTT_Error, vT("VMemFile::SetLength(%Iu) failed!\r\n"),dwNewLen);
			m_MemStruct.memory = (INT8 *)pNewMemory;
			//LPVOID p = VirtualAlloc(NULL,alllen,MEM_COMMIT,PAGE_READWRITE);
			//MoveMemory(p,m_MemStruct.memory,m_MemStruct.memlen);
			//VirtualFree(m_MemStruct.memory,m_MemStruct.memlen,MEM_DECOMMIT);

			//m_MemStruct.memory = (INT8 *)p;
			m_MemStruct.memlen = alllen;
		}
		m_MemStruct.length = (INT32)dwNewLen;
	}
	else 
	{
		m_MemStruct.length = (INT32)dwNewLen;
		if(m_MemStruct.position >= m_MemStruct.length)
			m_MemStruct.position = m_MemStruct.length;
	}

	return TRUE;
}

UINT_PTR VMemFile::GetLength() const
{
	return m_MemStruct.length;
}

UINT_PTR VMemFile::Read(void* lpBuf, UINT_PTR nCount)
{
	if (nCount == 0)
		return 0;
	UINT_PTR readlength = nCount;
	if(readlength + m_MemStruct.position > (UINT)m_MemStruct.length)
		readlength = m_MemStruct.length - m_MemStruct.position;
	if (readlength <= 0)
		return 0;
	memcpy(lpBuf,m_MemStruct.memory + m_MemStruct.position,readlength);
	m_MemStruct.position += readlength;
	
	return readlength;
}

UINT_PTR VMemFile::Write(const void* lpBuf, UINT_PTR nCount)
{
	UINT_PTR readlength = nCount;
	
	if(readlength + m_MemStruct.position > (UINT)m_MemStruct.memlen)
		SetLength(readlength + m_MemStruct.position);
	if (m_MemStruct.memory == NULL)
	{
		VFX_LTRACE(ELTT_Error, vT("VMemFile::Write(%p,%Id) find m_MemStruct.memory == NULL"), lpBuf, nCount);
	}
	memcpy(m_MemStruct.memory + m_MemStruct.position,lpBuf,readlength);
	m_MemStruct.position += readlength;

	if(m_MemStruct.position >= m_MemStruct.length)
		m_MemStruct.length = m_MemStruct.position;

	return readlength;
}

void VMemFile::Close()
{
	if(m_MemStruct.needfree)
		free(m_MemStruct.memory);//VirtualFree(m_MemStruct.memory,m_MemStruct.memlen,MEM_DECOMMIT);
	memset(&m_MemStruct,0,sizeof(m_MemStruct));
}

// Implementation
VMemFile::VMemFile()
{
	memset(&m_MemStruct,0,sizeof(m_MemStruct));
}

VMemFile::~VMemFile()
{
	Close();
}

UINT_PTR VMemFile::GetBufferPtr(BufferCommand nCommand, UINT_PTR nCount,void** ppBufStart, void** ppBufMax )
{
	return 0;
}
