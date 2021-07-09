#pragma once

#include "VPakFile.h"

NS_BEGIN

class F2MManager
{
	friend class VPakFile;
	VCritical				mLocker;
	std::map<VStringA, VRes2Memory*>	mF2Mems;

	std::vector<VPakFile*>	mMountPaks;
public:
	static F2MManager* Instance;
	F2MManager();
	~F2MManager();

	void Cleanup();

	void TryReleaseFile();

	VRes2Memory* GetF2M(LPCSTR file);
private:
	static VFile2Memory* _F2M(LPCSTR psz, vBOOL bShareWrite/* = FALSE*/);
};


NS_END

