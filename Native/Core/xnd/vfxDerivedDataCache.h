#pragma once

#include "../precompile.h"
#include "../thread/vfxcritical.h"
#include "../r2m/file_2_memory.h"

NS_BEGIN

class vfxDerivedDataCache : public VIUnknown
{
public:
	static vfxDerivedDataCache Instance;

	BYTE* LoadData(const char* xndFile, const char* path, DWORD flags, OUT UINT_PTR* length);
	void SaveData(const char* xndFile, const char* path, BYTE* pData, UINT_PTR size, DWORD flags);
public:
	std::string				mDDCDirectory;
};

NS_END
