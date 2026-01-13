#pragma once

#include "VPakFile.h"

NS_BEGIN

class F2MManager
{
	friend class VPakFile;
	VCritical				mLocker;
	VCritical				mTryReleaseLocker;
	std::map<VStringA, VRes2Memory*>	mF2Mems;

	std::vector<VPakFile*>	mMountPaks;
public:
	static F2MManager* Instance;
	std::atomic<int> FileOpenNumber = 0;
	F2MManager();
	~F2MManager();

	void Cleanup();

	int TryReleaseFile(VRes2Memory* exlude, int maxRelease);

	VRes2Memory* GetFile2Memory(LPCSTR file);
private:
	static VFile2Memory* CreateFile2Memory(LPCSTR psz, vBOOL bShareWrite/* = FALSE*/);
};


NS_END

