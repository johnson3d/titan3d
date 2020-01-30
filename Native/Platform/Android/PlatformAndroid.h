#pragma once
#include "../../BaseHead.h"

NS_BEGIN

class PlatformAndroid : public EngineNS::IVPlatform
{
	static PlatformAndroid	Object;
public:
	PlatformAndroid();
	~PlatformAndroid();

	static PlatformAndroid* GetInstance() {
		return &Object;
	}
public:
	JNIEnv*				mEnv;
	AAssetManager*		mAssetManager;
	JavaVM*				mJavaVM;
public:
	virtual EngineNS::EPlatformType GetPlatformType() override;
	void InitAndroid(JNIEnv* env, jobject assetMgr);
	int GetCPUCores();
	vBOOL BindCurThreadToCPU(int mask);
};

NS_END