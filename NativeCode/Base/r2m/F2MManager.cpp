#include "F2MManager.h"

#define new VNEW

NS_BEGIN

F2MManager* F2MManager::Instance = nullptr;

VRes2Memory* VRes2Memory::CreateFromFile(LPCSTR pszFile)
{
	auto result = F2MManager::Instance->GetF2M(pszFile);
	//result->AddRef();
	return result;
}

//////////////////////////////////////////////////////////////////////////
extern "C"  void vfxMemory_SetDebugInfo(void* memory, LPCSTR info);
VFile2Memory* F2MManager::_F2M(LPCSTR psz, vBOOL bShareWrite/* = FALSE*/)
{
	if (psz == NULL || psz[0] == 0)
		return NULL;
	VFile2Memory* pFM = new VFile2Memory;
	vfxMemory_SetDebugInfo(pFM, psz);
	if (FALSE == pFM->Create(psz, bShareWrite))
	{
		delete pFM;
		return NULL;
	}
	return pFM;
}

VRes2Memory* F2MManager::GetF2M(LPCSTR file)
{
	if (file == nullptr)
		return nullptr;
	auto it = mF2Mems.find(file);
	if (it == mF2Mems.end())
	{
		//1.read from OS file system 
		//2.read from apk on android platform
		//3.read from mounted tpak
		VRes2Memory* f2m = _F2M(file, FALSE);
		if (f2m == nullptr)
		{
			for (auto p : mMountPaks)
			{
				std::string fullName;
				if (p->GetFullName(file, fullName))
				{
					f2m = p->CreateF2M(fullName.c_str());
					break;
				}
			}
		}
		if (f2m == nullptr)
			return nullptr;
		f2m->AddRef();

		VAutoLock(mLocker);
		if (it == mF2Mems.end())
		{
			mF2Mems.insert(std::make_pair(file, f2m));
			return f2m;
		}
		else
		{
			f2m->Release();
			return it->second;
		}
	}
	/*auto pMem = it->second->Ptr(0, 1);

	if (pMem == NULL)
	{
		it->second->Free();
		return NULL;
	}
	it->second->Free();*/

	it->second->AddRef();
	return it->second;
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
		Safe_Release(it->second);
	}
	mF2Mems.clear();
}

int F2MManager::TryReleaseFile(VRes2Memory* exlude, int maxRelease)
{
	VAutoLock(mTryReleaseLocker);
	int t = FileOpenNumber;
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
	t = FileOpenNumber;
	VFX_LTRACE(ELTT_Resource, "TryReleaseFile(%d/%d/%d)\n", ReleaseCount, DoCount, (int)mF2Mems.size());
	return ReleaseCount;
}

NS_END
