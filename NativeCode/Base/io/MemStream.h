/********************************************************************
	created:	2006/08/16
	created:	16:8:2006   11:30
	filename: 	d:\New-Work\Victory\Code\victorycoreex\vfxxnd.h
	file path:	d:\New-Work\Victory\Code\victorycoreex
	file base:	vfxxnd
	file ext:	h
	author:		johnson
	
	purpose:	
*********************************************************************/
#pragma once

#include "../IUnknown.h"
#include "vfxfile.h"

NS_BEGIN

class IStreamWriter : public IWeakRefObject
{
public:
	virtual UINT64 Tell() const = 0;
	virtual bool Seek(UINT64 offset) = 0;
	virtual void Write(const void* pSrc, UINT t) = 0;

	template<typename _Type>
	void Write(const _Type& v)
	{
		return Write(&v, sizeof(_Type));
	}
	void Write(const std::string& v)
	{
		UINT len = (UINT)v.length();
		Write(&len, sizeof(len));
		Write(v.c_str(), len);
	}
};

class IStreamReader : public IWeakRefObject
{
public:
	virtual UINT64 GetLength() const = 0;
	virtual UINT64 Tell() const = 0;
	virtual bool Seek(UINT64 offset) = 0;
	virtual UINT Read(void* pSrc, UINT t) = 0;

	template<typename _Type>
	UINT Read(_Type& v)
	{
		return Read(&v, sizeof(_Type));
	}
	void Read(std::string& v)
	{
		UINT len;
		Read(&len, sizeof(len));
		v.resize(len);
		Read(&v[0], len);
		//v[len] = NULL;
	}
};

class TR_CLASS(SV_Dispose = self->Release())
	MemStreamWriter : public IStreamWriter
{
	BYTE*					mDataStream;
	UINT64					mBufferSize;
	UINT64					mPosition;
public:
	MemStreamWriter();
	MemStreamWriter(UINT size);
	~MemStreamWriter();

	void ResetBufferSize(UINT64 size = 0);
	inline void* GetPointer() {
		return &mDataStream[0];
	}
	virtual UINT64 GetLength() const {
		return mBufferSize;
	}
	virtual UINT64 Tell() const {
		return mPosition;
	}
	virtual bool Seek(UINT64 offset);
	virtual void Write(const void* pSrc, UINT t);
	
	template<typename _Type>
	void Write(const _Type& v)
	{
		return Write(&v, sizeof(_Type));
	}
};

class TR_CLASS(SV_Dispose = self->Release())
	ProxyMemStreamWriter : public IStreamWriter
{
	BYTE *					mDataStream;
	UINT64					mBufferSize;
	UINT64					mPosition;
public:
	ProxyMemStreamWriter(void* pMem, UINT64 size)
	{
		mDataStream = (BYTE*)pMem;
		mBufferSize = size;
		mPosition = 0;
	}

	inline void* GetPointer() {
		return mDataStream;
	}
	virtual UINT64 GetLength() const {
		return mBufferSize;
	}
	virtual UINT64 Tell() const {
		return mPosition;
	}
	virtual bool Seek(UINT64 offset);
	virtual void Write(const void* pSrc, UINT t);

	template<typename _Type>
	void Write(const _Type& v)
	{
		return Write(&v, sizeof(_Type));
	}
};

class TR_CLASS(SV_Dispose = self->Release())
	MemStreamReader : public IStreamReader
{
	BYTE*					mProxyPointer;
	UINT64					mLength;
	UINT64					mPosition;
	bool					mCopyData = false;
public:
	MemStreamReader()
		: mProxyPointer(nullptr)
		, mLength(0)
		, mPosition(0)
		, mCopyData(false)
	{

	}
	~MemStreamReader();
	void ProxyPointer(BYTE* ptr, UINT64 len);
	void CopyData(BYTE* ptr, UINT64 len);
	void ResetData(UINT64 len);
	void Cleanup();
	BYTE* GetPointer() {
		return mProxyPointer;
	}
	virtual UINT64 GetLength() const{
		return mLength;
	}
	virtual UINT64 Tell() const {
		return mPosition;
	}
	virtual bool Seek(UINT64 offset) {
		if (mLength <= offset)
		{
			return false;
		}
		mPosition = offset;
		return true;
	}
	virtual UINT Read(void* pSrc, UINT t);
	template<typename _Type>
	UINT Read(_Type& v)
	{
		return Read(&v, sizeof(_Type));
	}
};

class FileStreamWriter : public IStreamWriter
{
	VFile&		mFile;
public:
	FileStreamWriter(VFile& f)
		: mFile(f)
	{

	}
	virtual UINT64 Tell() const {
		return mFile.GetPosition();
	}
	virtual bool Seek(UINT64 offset) {
		mFile.Seek(offset, VFile_Base::begin);
		return true;
	}
	virtual void Write(const void* pSrc, UINT t)
	{
		mFile.Write(pSrc, t);
	}
	template<typename _Type>
	void Write(const _Type& v)
	{
		return Write(&v, sizeof(_Type));
	}
};

class FileStreamReader : public IStreamReader
{
	VFile&		mFile;
public:
	FileStreamReader(VFile& f)
		: mFile(f)
	{

	}
	virtual UINT64 GetLength() const {
		return (UINT64)mFile.GetLength();
	}
	virtual UINT64 Tell() const {
		return mFile.GetPosition();
	}
	virtual bool Seek(UINT64 offset) {
		mFile.Seek(offset, VFile_Base::SeekPosition::begin);
		return true;
	}
	bool Seek(UINT64 offset, VFile_Base::SeekPosition eMode) {
		mFile.Seek(offset, eMode);
		return true;
	}
	virtual UINT Read(void* pTar, UINT t)
	{
		return (UINT)mFile.Read(pTar, t);
	}
	template<typename _Type>
	UINT Read(_Type& v)
	{
		return Read(&v, sizeof(_Type));
	}
};

NS_END
