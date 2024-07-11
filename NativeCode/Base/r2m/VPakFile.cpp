#include "VPakFile.h"
#include "F2MManager.h"
#include "../generic/vfxtemplate.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::VPakFile);
ENGINE_RTTI_IMPL(EngineNS::VPackFile2Memory);

VPakFile::VPakFile()
{
	mMountOrder = 0;
}

VPakFile::~VPakFile()
{

}

vBOOL VPakFile::BuildPakFile(const VPakPair* const assets, UINT count, const char* pakFile)
{
	UINT packedNum = 0;
	VFile file;
	if (file.Open(pakFile, VFile::modeCreate | VFile::modeWrite) == FALSE)
		return FALSE;

	std::vector<VPakAssetDesc> descList;
	for (UINT i = 0; i < count; i++)
	{
		VFile srcFile;
		if (srcFile.Open(assets[i].SrcAddress, VFile::modeRead) == FALSE)
		{
			VFX_LTRACE(ELTT_Warning, "VPakFile [%s]: %s open failed\r\n", pakFile, assets[i].SrcAddress);
			continue;
		}

		std::vector<BYTE> buffer;
		buffer.resize(srcFile.GetLength());
		if(buffer.size()>0)
			srcFile.Read(&buffer[0], buffer.size());
		srcFile.Close();

		VPakAssetDesc desc;
		desc.Flags = assets[i].Flags;
		desc.Offset = file.GetPosition();
		desc.Size = (UINT)buffer.size();
		desc.SizeInPak = desc.Size;
		desc.FullPath = assets[i].FullName;

		if(buffer.size()>0)
			file.Write(&buffer[0], desc.Size);

		descList.push_back(desc);
		packedNum++;
	}

	file.Close();

	VFile tabFile;
	if (tabFile.Open((std::string(pakFile) + ".ftab").c_str(), VFile::modeCreate | VFile::modeWrite) == FALSE)
		return FALSE;

	tabFile.Write(&packedNum, sizeof(packedNum));
	for (size_t i = 0; i < descList.size(); i++)
	{
		tabFile.Write(&descList[i].Offset, sizeof(UINT64));
		tabFile.Write(&descList[i].Size, sizeof(UINT));
		tabFile.Write(&descList[i].SizeInPak, sizeof(UINT));
		tabFile.Write(&descList[i].Flags, sizeof(UINT));
		UINT slen = (UINT)descList[i].FullPath.length();
		tabFile.Write(&slen, sizeof(slen));
		tabFile.Write(&descList[i].FullPath[0], sizeof(char)*slen);
	}

	tabFile.Close();
	return TRUE;
}

vBOOL VPakFile::LoadPak(const char* name)
{
	VFile tabFile;
	if (tabFile.Open((std::string(name) + ".ftab").c_str(), VFile::modeRead) == FALSE)
		return FALSE;

	UINT packedNum;
	tabFile.Read(&packedNum, sizeof(packedNum));
	mSortedAssets.resize(packedNum);
	for (UINT i = 0; i < packedNum; i++)
	{
		tabFile.Read(&mSortedAssets[i].Offset, sizeof(UINT64));
		tabFile.Read(&mSortedAssets[i].Size, sizeof(UINT));
		tabFile.Read(&mSortedAssets[i].SizeInPak, sizeof(UINT));
		tabFile.Read(&mSortedAssets[i].Flags, sizeof(UINT));
		UINT slen;
		tabFile.Read(&slen, sizeof(slen));
		auto pStr = new char[slen + 1];
		tabFile.Read(pStr, sizeof(char) * slen);
		pStr[slen] = 0;
		mSortedAssets[i].FullPath = pStr;
		delete[] pStr;
	}

	tabFile.Close();

	_vfxQSort(&mSortedAssets[0], mSortedAssets.size(), AssetDescCmp());

	if (mPhysicalFile.Open(name, VFile::modeRead) == FALSE)
		return FALSE;
	return TRUE;
}

vBOOL VPakFile::MountPak(const char* mountPoint, int order)
{
	mMountOrder = order;
	mMountPoint = mountPoint;
	this->AddRef();
	F2MManager::Instance->mMountPaks.push_back(this);
	_vfxQSort(&F2MManager::Instance->mMountPaks[0], F2MManager::Instance->mMountPaks.size(), MountOrderCmp());
	return TRUE;
}

vBOOL VPakFile::UnMountPak()
{
	for (auto i = F2MManager::Instance->mMountPaks.begin(); i != F2MManager::Instance->mMountPaks.end(); i++)
	{
		if (*i == this)
		{
			F2MManager::Instance->mMountPaks.erase(i);
			this->Release();
			return TRUE;
		}
	}
	return FALSE;
}

vBOOL VPakFile::GetFullName(const char* address, std::string& outFullName)
{
	char* pos = strstr((char*)address, mMountPoint.c_str());
	if (pos == address)
	{
		outFullName = &address[mMountPoint.length()];
		return TRUE;
	}
	return FALSE;
}

vBOOL VPakFile::IsAssetExisting(const char* name) const
{
	auto pos = _vfxQFind(&mSortedAssets[0], mSortedAssets.size(), name, AssetDescCmp());
	if (pos < 0)
		return FALSE;
	return TRUE;
}

vBOOL VPakFile::GetAssetDesc(const char* name, VPakAssetDesc* outDesc) const
{
	auto pos = _vfxQFind(&mSortedAssets[0], mSortedAssets.size(), name, AssetDescCmp());
	if (pos < 0)
		return FALSE;
	*outDesc = mSortedAssets[pos];
	return TRUE;
}

VPackFile2Memory* VPakFile::CreateF2M(const char* name)
{
	auto result = new VPackFile2Memory();
	result->mPakFile.FromObject(this);
	if (FALSE == GetAssetDesc(name, &result->mAssetDesc))
	{
		result->Release();
		return nullptr;
	}
	return result;
}

VPackFile2Memory::VPackFile2Memory()
{
	mPtrOffset = 0;
}

void VPackFile2Memory::Release()
{
	VRes2Memory::Release();
}

VResPtr VPackFile2Memory::Ptr(UINT64 offset, UINT64 size)
{
	auto file = this->GetFile();
	if (file == nullptr)
		return nullptr;

	if (mAssetDesc.SizeInPak == 0)
		return nullptr;

	if (size == 0)
		size = mAssetDesc.SizeInPak - offset;
	
	if (offset + size > mAssetDesc.SizeInPak)
		return nullptr;

	if (offset >= mPtrOffset && (offset - mPtrOffset) + size <= mCachedBuffer.size())
	{
		return &mCachedBuffer[0];
	}
	
	mPtrOffset = offset;
	mCachedBuffer.resize(size);
	file->Seek(mAssetDesc.Offset + offset, VFile::begin);
	auto readCount = file->Read(&mCachedBuffer[0], size);
	if (readCount == size)
		return &mCachedBuffer[0];
	return nullptr;
}

vBOOL VPackFile2Memory::Free()
{	
	return TRUE;
}

UINT64 VPackFile2Memory::Length() const
{
	return mAssetDesc.Size;
}

LPCSTR VPackFile2Memory::Name() const
{
	return (mAssetDesc.FullPath).c_str();
}

void VPackFile2Memory::TryReleaseHolder()
{
	mCachedBuffer.clear();
}

VFile* VPackFile2Memory::GetFile() const
{
	auto pak = mPakFile.GetPtr();
	if (pak == nullptr)
		return nullptr;

	return &pak->mPhysicalFile;
}

NS_END
