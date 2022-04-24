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

#include "../BaseHead.h"
#include "../io/MemStream.h"
#include "../r2m/file_2_memory.h"

NS_BEGIN

class XndAttribute;
class XndNode;
class XndHolder;

struct TR_CLASS()
	XndElement : public VIUnknownBase
{
	std::string			mName;
	UINT				mVersion;
	UINT				mFlags;
	const char* GetName() const {
		return mName.c_str();
	}
	void SetName(const char* v) {
		mName = v;
	}
	static UINT NameToHash(const std::string& name);
	TObjectHandle<XndHolder>	mHolder;
};

class TR_CLASS(SV_Dispose = self->Release())
	XndAttribute : public XndElement
{
	friend XndHolder;
protected:
	AutoRef<MemStreamWriter>	mMemWriter;
	AutoRef<MemStreamReader>	mMemReader;
	UINT64						mOffsetInResource;
	UINT						mAttrLength;
public:
	XndAttribute()
	{
		mOffsetInResource = 0;
		mAttrLength = 0;
	}
	bool BeginRead();
	void EndRead();
	void BeginWrite(UINT64 length = 0);
	void EndWrite();

	UINT64 GetReaderLength() {
		if (mMemReader == 0)
			return 0;
		return mMemReader->GetLength();
	}
	UINT64 GetReaderPosition() {
		if (mMemReader == 0)
			return 0;
		return mMemReader->Tell();
	}
	UINT64 GetWriterPosition() {
		if (mMemWriter == 0)
			return 0;
		return mMemWriter->Tell();
	}
	void ReaderSeek(UINT64 offset)
	{
		if (mMemReader == 0)
			return;
		mMemReader->Seek(offset);
	}
	void WriterSeek(UINT64 offset)
	{
		if (mMemWriter == 0)
			return;
		mMemWriter->Seek(offset);
	}

	UINT Read(void* pSrc, UINT t) {
		return mMemReader->Read(pSrc, t);
	}
	void Write(const void* pSrc, UINT t) {
		mMemWriter->Write(pSrc, t);
	}
	void Read(VNameString& v)
	{
		std::string text;
		Read(text);
		v = text;
	}
	void Read(std::string& text)
	{
		int len;
		mMemReader->Read(len);
		text.resize(len);
		mMemReader->Read(&text[0], (UINT)len);
	}
	void Write(const VNameString& v)
	{
		Write(v.AsStdString());
	}
	void Write(const std::string& text)
	{
		mMemWriter->Write((int)text.length());
		mMemWriter->Write(text.c_str(), (UINT)text.length());
	}

	template<typename _Type>
	UINT Read(_Type& v)
	{
		return mMemReader->Read(&v, sizeof(_Type));
	}

	template<typename _Type>
	void Write(const _Type& v)
	{
		return mMemWriter->Write(v);
	}
};

class TR_CLASS(SV_Dispose = self->Release())
	XndNode : public XndElement
{
protected:
	std::vector<AutoRef<XndAttribute>>		mAttributes;
	std::vector<AutoRef<XndNode>>			mNodes;
public:
	XndNode()
	{
	}
	XndAttribute* GetOrAddAttribute(const char* name, UINT ver, UINT flags);
	void AddAttribute(XndAttribute* pAttr) {
		AutoRef<XndAttribute> tmp;
		tmp.StrongRef(pAttr);
		mAttributes.push_back(tmp);
	}
	XndNode* GetOrAddNode(const char* name, UINT ver, UINT flags);
	void AddNode(XndNode* pNode) {
		AutoRef<XndNode> tmp;
		tmp.StrongRef(pNode);
		mNodes.push_back(tmp);
	}
	UINT GetNumOfAttribute() const{
		return (UINT)mAttributes.size();
	}
	XndAttribute* GetAttribute(UINT index) {
		if (index >= (UINT)mAttributes.size())
			return nullptr;
		return mAttributes[index];
	}
	XndAttribute* TryGetAttribute(const char* name) {
		for (auto& i : mAttributes)
		{
			if (i->mName == name)
				return i;
		}
		return nullptr;
	}
	XndNode* TryGetChildNode(const char* name) {
		for (auto& i : mNodes)
		{
			if (i->mName == name)
				return i;
		}
		return nullptr;
	}
	UINT GetNumOfNode() const {
		return (UINT)mNodes.size();
	}
	XndNode* GetNode(UINT index) {
		if (index >= (UINT)mNodes.size())
			return nullptr;
		return mNodes[index];
	}
	XndAttribute* FindFirstAttribute(const char* name) {
		for (auto i : mAttributes)
		{
			if (i->mName == name)
			{
				return i;
			}
		}
		return nullptr;
	}
	XndAttribute* FindFirstAttributeByFlags(UINT flags) {
		for (auto i : mAttributes)
		{
			if (i->mFlags == flags)
			{
				return i;
			}
		}
		return nullptr;
	}
	XndNode* FindFirstNode(const char* name) {
		for (auto i : mNodes)
		{
			if (i->mName == name)
			{
				return i;
			}
		}
		return nullptr;
	}
};

class TR_CLASS(SV_Dispose = self->Release())
	XndHolder : public VIUnknown
{
	AutoRef<XndNode>			mRootNode;
	AutoRef<VRes2Memory>		mResource;
public:
	XndHolder() 
	{
	}
	VRes2Memory* GetResouce() {
		return mResource;
	}
	XndAttribute* NewAttribute(const char* name, UINT ver, UINT flags);
	XndNode* NewNode(const char* name, UINT ver, UINT flags);

	XndNode* GetRootNode() {
		return mRootNode;
	}
	void SetRootNode(XndNode* node) {
		mRootNode.StrongRef(node);
	}

	bool LoadXnd(const char* file);
	void SaveXnd(const char* file);

	void TryReleaseHolder();
protected:
	void SaveXnd(IStreamWriter& ar, XndNode* node);
	void WriteNodeTree(IStreamWriter& ar, XndNode* node);
	void ReadNodeTree(IStreamReader& ar, XndNode* node);
};

NS_END
