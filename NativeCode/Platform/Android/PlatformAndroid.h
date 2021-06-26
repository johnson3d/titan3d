#pragma once
#include "../../Base/IUnknown.h"

NS_BEGIN

class TR_CLASS()
PlatformAndroid : public VIUnknown
{
public:
	JNIEnv*			mEnv;
	AAssetManager*	mAssetManager;
	JavaVM*			mJavaVM;
public:
	static PlatformAndroid Instance;
	static PlatformAndroid* GetInstance() {
		return &Instance;
	}
};

NS_END

