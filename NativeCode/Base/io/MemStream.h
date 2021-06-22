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

class IStreamWriter : public VIUnknown
{
public:
	virtual UINT64 Tell() = 0;
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

class IStreamReader : public VIUnknown
{
public:
	virtual UINT64 GetLength() const = 0;
	virtual UINT64 Tell() = 0;
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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_Dispose=self->Release())
MemStreamWriter : public IStreamWriter
{
	BYTE*					mDataStream;
	UINT64					mBufferSize;
	UINT64					mPosition;
public:
	MemStreamWriter();
	MemStreamWriter(UINT size);
	~MemStreamWriter();

	TR_FUNCTION(SV_SuppressGC = true)
	void ResetStream(UINT64 size = 0);
	TR_FUNCTION(SV_SuppressGC = true)
	inline void* GetDataPointer() {
		return &mDataStream[0];
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual UINT64 GetLength() const {
		return mBufferSize;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual UINT64 Tell() {
		return mPosition;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual bool Seek(UINT64 offset);
	virtual void Write(const void* pSrc, UINT t);
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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_Dispose = self->Release())
MemStreamReader : public IStreamReader
{
	BYTE*					mProxyPointer;
	UINT64					mLength;
	UINT64					mPosition;
public:
	MemStreamReader()
		: mProxyPointer(nullptr)
		, mLength(0)
		, mPosition(0)
	{

	}
	TR_FUNCTION(SV_SuppressGC = true)
	void ProxyPointer(BYTE* ptr, UINT64 len)
	{
		mProxyPointer = ptr;
		mLength = len;
		mPosition = 0;
	}
	void Cleanup()
	{
		mProxyPointer = nullptr;
		mLength = 0;
		mPosition = 0;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	BYTE* GetPointer() {
		return mProxyPointer;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual UINT64 GetLength() const{
		return mLength;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual UINT64 Tell() {
		return mPosition;
	}
	TR_FUNCTION(SV_SuppressGC = true)
	virtual bool Seek(UINT64 offset) {
		if (mPosition <= offset)
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
	void Read(std::string& v)
	{
		UINT len;
		Read(&len, sizeof(len));
		v.resize(len + 1);
		Read(&v[0], len);
		v[len] = '\0';
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
	virtual UINT64 Tell() {
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
	void Write(const std::string& v)
	{
		UINT len = (UINT)v.length();
		Write(&len, sizeof(len));
		Write(v.c_str(), len);
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
	virtual UINT64 Tell() {
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
	void Read(std::string& v)
	{
		UINT len;
		Read(&len, sizeof(len));
		v.resize(len + 1);
		Read(&v[0], len);
		v[len] = '\0';
	}
};

NS_END
