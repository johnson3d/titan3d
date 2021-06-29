#include "vfxxnd.h"
#include "../r2m/F2MManager.h"

#define new VNEW

NS_BEGIN

bool XndAttribute::BeginRead()
{
	if (mMemReader == nullptr)
	{
		mMemReader = AutoRef<MemStreamReader>(new MemStreamReader());
	}
	auto holder = mHolder.GetPtr();
	if (holder == nullptr)
		return false;
	BYTE* p = (BYTE*)holder->GetResouce()->Ptr(mOffsetInResource, mAttrLength);
	//mMemReader->ProxyPointer(p, holder->GetResouce()->Length());
	mMemReader->ProxyPointer(p, mAttrLength);
	return true;
}

void XndAttribute::EndRead()
{
	auto holder = mHolder.GetPtr();
	if (holder == nullptr)
		return;
	holder->GetResouce()->Free();
	mMemReader->Cleanup();
}

void XndAttribute::BeginWrite(UINT64 length)
{
	if (mMemWriter == nullptr)
	{
		auto p = new MemStreamWriter();
		mMemWriter = AutoRef<MemStreamWriter>(p);
	}
	mMemWriter->ResetBufferSize(length);
}

void XndAttribute::EndWrite()
{
	//mMemWriter->ResetStream(0);
}

XndAttribute* XndNode::GetOrAddAttribute(const char* name, UINT ver, UINT flags)
{
	auto xnd = mHolder.GetPtr();
	if (xnd == nullptr)
		return nullptr;
	auto result = xnd->NewAttribute(name, ver, flags);
	AutoRef<XndAttribute> tmp;
	tmp.StrongRef(result);
	mAttributes.push_back(tmp);
	result->Release();
	return result;
}

XndNode* XndNode::GetOrAddNode(const char* name, UINT ver, UINT flags)
{
	auto xnd = mHolder.GetPtr();
	if (xnd == nullptr)
		return nullptr;
	auto result = xnd->NewNode(name, ver, flags);
	AutoRef<XndNode> tmp;
	tmp.StrongRef(result);
	mNodes.push_back(tmp);
	result->Release();
	return result;
}

void XndHolder::TryReleaseHolder()
{
	if (mResource != nullptr)
		mResource->TryReleaseHolder();
}

XndAttribute* XndHolder::NewAttribute(const char* name, UINT ver, UINT flags)
{
	auto result = new XndAttribute();
	result->mName = name;
	result->mVersion = ver;
	result->mFlags = flags;
	result->mHolder.FromObject(this);
	return result;
}

XndNode* XndHolder::NewNode(const char* name, UINT ver, UINT flags)
{
	auto result = new XndNode();
	result->mName = name;
	result->mVersion = ver;
	result->mFlags = flags;
	result->mHolder.FromObject(this);
	return result;
}

bool XndHolder::LoadXnd(const char* file)
{
	mRootNode = nullptr;
	mResource = nullptr;

	{
		mResource = AutoRef<VRes2Memory>(F2MManager::Instance.GetF2M(file)); 
		if (mResource == nullptr)
			return false;
		UINT64 length = (UINT64)mResource->Length();

		auto ptr = mResource->Ptr(length - sizeof(UINT64), sizeof(UINT64));
		auto treeOffset = *(UINT64*)ptr;
		mResource->Free();

		ptr = mResource->Ptr(treeOffset, length - treeOffset);
		MemStreamReader ar;
		ar.ProxyPointer((BYTE*)ptr, length - treeOffset);
		mRootNode = AutoRef<XndNode>(new XndNode());
		ReadNodeTree(ar, mRootNode);
		mResource->Free();

		mResource->TryReleaseHolder();

		return true;
	}

	/*VFile io;
	if (io.Open(file, VFile::modeRead))
	{
		CHAR szName[4];
		io.Read(szName, 4);
		if (szName[0] != 'x' ||
			szName[1] != 'n' ||
			szName[2] != 'd' ||
			szName[3] != '0')
		{
			io.Close();
			return false;
		}

		INT64 off = 0 - sizeof(UINT64);
		io.Seek(off, VFile_Base::end);
		UINT64 offsetTrees;
		io.Read(&offsetTrees, sizeof(offsetTrees));
		io.Seek(offsetTrees, VFile_Base::begin);

		mRootNode = AutoRef<XndNode>(new XndNode());

		FileStreamReader ar(io);
		ReadNodeTree(ar, mRootNode);

		io.Seek(0, VFile_Base::begin);
		io.Close();

		mResource = AutoRef<VRes2Memory>(F2MManager::Instance.GetF2M(file));
		return true;
	}
	return false;*/
}

void XndHolder::SaveXnd(const char* file)
{
	VFile io;
	if (io.Open(file, VFile::modeCreate | VFile::modeWrite))
	{
		CHAR szName[4];
		szName[0] = 'x';
		szName[1] = 'n';
		szName[2] = 'd';
		szName[3] = '0';
		io.Write(szName, 4);

		FileStreamWriter ar(io);
		SaveXnd(ar, mRootNode);
		auto offsetTrees = (UINT64)ar.Tell();

		WriteNodeTree(ar, mRootNode);
		ar.Write(offsetTrees);

		io.Close();
	}
}

void XndHolder::SaveXnd(IStreamWriter& ar, XndNode* node)
{
	for (UINT i = 0; i < node->GetNumOfAttribute(); i++)
	{
		auto attr = node->GetAttribute(i);
		attr->mOffsetInResource = ar.Tell();
		
		ar.Write(attr->mMemWriter->GetPointer(), (UINT)attr->mMemWriter->Tell());

		attr->mAttrLength = (UINT)(ar.Tell() - attr->mOffsetInResource);
	}

	for (UINT i = 0; i < node->GetNumOfNode(); i++)
	{
		auto cld = node->GetNode(i);

		SaveXnd(ar, cld);
	}
}

void XndHolder::WriteNodeTree(IStreamWriter& ar, XndNode* node)
{
	ar.Write(node->mVersion);
	ar.Write(node->mFlags);
	ar.Write(node->mName);

	UINT num = node->GetNumOfAttribute();
	ar.Write(num);
	for (UINT i = 0; i < num; i++)
	{
		auto attr = node->GetAttribute(i);
		ar.Write(attr->mVersion);
		ar.Write(attr->mFlags);
		ar.Write(attr->mName);
		ar.Write(attr->mOffsetInResource);
		ar.Write(attr->mAttrLength);
	}

	num = node->GetNumOfNode();
	ar.Write(num);
	for (UINT i = 0; i < num; i++)
	{
		auto cld = node->GetNode(i);
		WriteNodeTree(ar, cld);
	}
}

void XndHolder::ReadNodeTree(IStreamReader& ar, XndNode* node)
{
	ar.Read(node->mVersion);
	ar.Read(node->mFlags);
	ar.Read(node->mName);
	
	UINT num;
	ar.Read(num);
	for (UINT i = 0; i < num; i++)
	{
		UINT version;
		ar.Read(version);
		UINT flags;
		ar.Read(flags);
		std::string name;
		ar.Read(name);

		AutoRef<XndAttribute> attr(this->NewAttribute(name.c_str(), version, flags));
		node->AddAttribute(attr);

		ar.Read(attr->mOffsetInResource);
		ar.Read(attr->mAttrLength);
	}

	ar.Read(num);
	for (UINT i = 0; i < num; i++)
	{
		AutoRef<XndNode> cld(new XndNode());
		ReadNodeTree(ar, cld);
		node->AddNode(cld);
	}
}

NS_END