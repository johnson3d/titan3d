#include "vfxDerivedDataCache.h"
#include "../r2m/VPakFile.h"

#define new VNEW

NS_BEGIN

vfxDerivedDataCache vfxDerivedDataCache::Instance;

BYTE* vfxDerivedDataCache::LoadData(const char* xndFile, const char* path, DWORD flags, OUT UINT_PTR* length)
{
	VFile io;
	std::string xndDir = mDDCDirectory + xndFile;
	std::string xndPath = xndDir + path;
	if (io.Open(xndPath.c_str(), VFile::modeRead))
	{
		*length = io.GetLength();
		BYTE* result = new BYTE[*length];
		io.Read(result, *length);
		io.Close();
		return result;
	}
	else
	{
		*length = 0;
		return nullptr;
	}
}

void vfxDerivedDataCache::SaveData(const char* xndFile, const char* path, BYTE* pData, UINT_PTR size, DWORD flags)
{
	VFile io;
	std::string xndDir = mDDCDirectory + xndFile;
	std::string xndPath = xndDir + path;
	if (io.Open(xndPath.c_str(), VFile::modeCreate | VFile::modeWrite))
	{
		io.Write(pData, size);
		io.Close();
	}
}

NS_END

using namespace EngineNS;

extern "C"
{
	
};
