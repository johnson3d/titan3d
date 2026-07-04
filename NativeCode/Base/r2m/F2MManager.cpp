#include "F2MManager.h"
#include "VPakFile.h"

#define new VNEW

NS_BEGIN

F2MManager* F2MManager::Instance = nullptr;

VRes2Memory* VRes2Memory::CreateFromFile(LPCSTR pszFile)
{
	if (F2MManager::Instance == nullptr)
		return nullptr;
	auto result = F2MManager::Instance->GetFile2Memory(pszFile);
	//result->AddRef();
	return result;
}

void VRes2Memory::OnBeforeWriteFile(LPCSTR pszFile)
{
	if (F2MManager::Instance == nullptr)
		return;
	F2MManager::Instance->OnBeforeWriteFile(pszFile);
}
void VRes2Memory::OnAfterWriteFile(LPCSTR pszFile)
{
	if (F2MManager::Instance == nullptr)
		return;
	F2MManager::Instance->OnAfterWriteFile(pszFile);
}

//////////////////////////////////////////////////////////////////////////
extern "C"  void vfxMemory_SetDebugInfo(void* memory, LPCSTR info);
VFile2Memory* F2MManager::CreateFile2Memory(LPCSTR psz, vBOOL bShareWrite/* = FALSE*/)
{
	if (psz == NULL || psz[0] == 0)
		return NULL;
	VFile2Memory* pFM = new VFile2Memory;
	vfxMemory_SetDebugInfo(pFM, psz);
	if (FALSE == pFM->Create(psz, bShareWrite))
	{
		pFM->Release();
		return NULL;
	}
	return pFM;
}
void F2MManager::OnBeforeWriteFile(LPCSTR file)
{
	if (file == nullptr)
		return;
	VAutoLock(mLocker);
	auto it = mF2Mems.find(file);
	if (it == mF2Mems.end())
	{
		return;
	}
	auto f2m = it->second;
	f2m->OnBeforeWriteFile();
}
void F2MManager::OnAfterWriteFile(LPCSTR file)
{
	if (file == nullptr)
		return;
	VAutoLock(mLocker);
	auto it = mF2Mems.find(file);
	if (it == mF2Mems.end())
	{
		return;
	}
	auto f2m = it->second;
	f2m->OnAfterWriteFile();
	mF2Mems.erase(it);
}
VRes2Memory* F2MManager::GetFile2Memory(LPCSTR file)
{
	if (file == nullptr)
		return nullptr;

	VAutoLock(mLocker);
	auto it = mF2Mems.find(file);
	if (it == mF2Mems.end())
	{
		mLocker.Unlock();
		//1.read from OS file system 
		//2.read from apk on android platform
		//3.read from mounted tpak
		auto f2m = MakeWeakRef<VRes2Memory>(CreateFile2Memory(file, FALSE));
		if (f2m == nullptr)
		{
			for (auto p : mMountPaks)
			{
				std::string fullName;
				if (p->GetFullName(file, fullName))
				{
					VPackFile2Memory* p_f2m = p->CreateF2M(fullName.c_str());
					VRes2Memory* tt = static_cast<VRes2Memory*>(p_f2m);
					f2m = MakeWeakRef(tt);
					break;
				}
			}
		}
		if (f2m == nullptr)
			return nullptr;

		mF2Mems.insert(std::make_pair(file, f2m));
		f2m->AddRef();
		return f2m;
	}
	else
	{
		it->second->AddRef();
		return it->second;
	}
}

F2MManager::F2MManager()
{

}

F2MManager::~F2MManager()
{
	Cleanup();
}

bool GF2MManagerCleanuped = false;
void F2MManager::Cleanup()
{
	if (GF2MManagerCleanuped)
		return;
	GF2MManagerCleanuped = true;

	VAutoLock(mLocker);

	for (auto it = mMountPaks.begin(); it != mMountPaks.end(); it++)
	{
		(*it)->Release();
	}
	mMountPaks.clear();

	for (auto it = mF2Mems.begin(); it != mF2Mems.end(); it++)
	{
		it->second->TryReleaseHolder();
	}
	mF2Mems.clear();
}

int F2MManager::TryReleaseFile(VRes2Memory* exlude, int maxRelease)
{
	//todo: Choose to close the file and ClearCache based on long time no used
	VAutoLock(mTryReleaseLocker);
	int DoCount = 0;
	int ReleaseCount = 0;
	for (auto it = mF2Mems.begin(); it != mF2Mems.end(); it++)
	{
		if (exlude == it->second)
			continue;
		if (it->second->TryReleaseHolder())
		{
			ReleaseCount++;
			if (ReleaseCount >= maxRelease)
				break;
		}
		DoCount++;
	}
	auto t = (int)FileOpenNumber;
	VFX_LTRACE(ELTT_Resource, "TryReleaseFile(%d/%d/%d)->%d\n", ReleaseCount, DoCount, (int)mF2Mems.size(), t);
	return ReleaseCount;
}

NS_END
