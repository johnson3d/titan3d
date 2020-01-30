#pragma once

#include "file_2_memory.h"

NS_BEGIN

class VPackFile2Memory;

struct VPakAssetDesc
{
	VPakAssetDesc()
	{
		Offset = 0;
		Size = 0;
		Flags = 0;
	}
	UINT64			Offset;
	UINT			Size;
	UINT			SizeInPak;//Ϊ���ѹ����Ԥ��
	UINT			Flags;
	std::string		FullPath;
};

struct VPakPair
{
	const char*		SrcAddress;
	const char*		FullName;
	const char*		MD5;
	UINT			Flags;
	UINT			Pad0;
};

class VPakFile : public VIUnknown
{
	friend class VPackFile2Memory;
private:
	struct MountOrderCmp
	{
		int operator()(const VPakFile* lh, const VPakFile* rh)
		{
			return (lh->mMountOrder - rh->mMountOrder);
		}
	};
	struct AssetDescCmp
	{
		int operator()(const VPakAssetDesc& lh, const char* rh)
		{
			return strcmp(lh.FullPath.c_str(), rh);
		}
		int operator()(const VPakAssetDesc& lh, const VPakAssetDesc& rh)
		{
			return strcmp(lh.FullPath.c_str(), rh.FullPath.c_str());
		}
	};
	std::vector<VPakAssetDesc>		mSortedAssets;
	VFile							mPhysicalFile;
	int								mMountOrder;
	std::string						mMountPoint;
public:
	VPakFile();
	~VPakFile();
	RTTI_DEF(VPakFile, 0x7eba775b5e7597ca, true);

	static vBOOL BuildPakFile(const VPakPair* const assets, UINT count, const char* pakFile);

	vBOOL LoadPak(const char* name);
	vBOOL MountPak(const char* mountPoint, int order);
	vBOOL UnMountPak();

	UINT GetAssetsNum() {
		return (UINT)mSortedAssets.size();
	}
	UINT GetAssetSize(UINT index) {
		return mSortedAssets[index].Size;
	}
	UINT GetAssetSizeInPak(UINT index) {
		return mSortedAssets[index].SizeInPak;
	}
	const char* GetAssetName(UINT index) {
		return mSortedAssets[index].FullPath.c_str();
	}

	//Ҫ��address��MoutPoint���Ǳ�׼���ַ�֮���·��
	vBOOL GetFullName(const char* address, std::string& outFullName);
	vBOOL IsAssetExisting(const char* name) const;
	vBOOL GetAssetDesc(const char* name, VPakAssetDesc* outDesc) const;

	VPackFile2Memory* CreateF2M(const char* name);
};

class VPackFile2Memory : public VRes2Memory
{
	friend class VPakFile;
protected:
	TObjectHandle<VPakFile>			mPakFile;
	
	VPakAssetDesc					mAssetDesc;
	UINT_PTR						mPtrOffset;
	std::vector<BYTE>				mCachedBuffer;
	VFile* GetFile() const;
public:
	RTTI_DEF(VFile2Memory, 0x953fa2135e7597f0, true);
	VPackFile2Memory();

	virtual void Release() override;

	virtual  VResPtr	Ptr(UINT_PTR offset, UINT_PTR size = -1) override;
	/*!	\copydoc VRes2Memory::Free */
	virtual  vBOOL		Free() override;
	/*!	\copydoc VRes2Memory::Length */
	virtual  UINT_PTR	Length() const override;
	/*!	\copydoc VRes2Memory::Name */
	virtual  LPCSTR		Name() const override;

	virtual  void		TryReleaseHolder() override;
};

class F2MManager
{
	friend class VPakFile;
	VCritical				mLocker;
	std::map<VStringA, VRes2Memory*>	mF2Mems;

	std::vector<VPakFile*>	mMountPaks;
public:
	static F2MManager Instance;
	F2MManager();
	~F2MManager();

	void Cleanup();

	void TryReleaseFile();

	VRes2Memory* GetF2M(LPCSTR file);
private:
	static VFile2Memory* _F2M(LPCSTR psz, vBOOL bShareWrite/* = FALSE*/);
};


NS_END

