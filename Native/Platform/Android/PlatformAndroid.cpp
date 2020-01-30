#include "PlatformAndroid.h"
#include "../../../CSharpAPI.h"
#include <android/asset_manager_jni.h>
#include <unistd.h>
#include <errno.h>
#include <pthread.h>
#include <sys/syscall.h>

#define new VNEW

NS_BEGIN

#ifndef CPU_ZERO
#define CPU_SETSIZE 1024
#define __NCPUBITS  (8 * sizeof (unsigned long))
typedef struct
{
	unsigned long __bits[CPU_SETSIZE / __NCPUBITS];
} cpu_set_t;

#define CPU_SET(cpu, cpusetp) \
  ((cpusetp)->__bits[(cpu)/__NCPUBITS] |= (1UL << ((cpu) % __NCPUBITS)))
#define CPU_ZERO(cpusetp) \
  memset((cpusetp), 0, sizeof(cpu_set_t))
#endif


PlatformAndroid	PlatformAndroid::Object;

PlatformAndroid::PlatformAndroid()
{
	mEnv = nullptr;
	mAssetManager = nullptr;
	mJavaVM = nullptr;
}


PlatformAndroid::~PlatformAndroid()
{
}

void PlatformAndroid::InitAndroid(JNIEnv* env, jobject assetMgr)
{
	mEnv = env;
	mAssetManager = AAssetManager_fromJava(env, assetMgr);

	mEnv->GetJavaVM(&mJavaVM);
}

EPlatformType PlatformAndroid::GetPlatformType()
{
	return PLTF_Android;
}

int PlatformAndroid::GetCPUCores()
{
	return sysconf(_SC_NPROCESSORS_CONF);
}


vBOOL PlatformAndroid::BindCurThreadToCPU(int cpu) 
{
	cpu_set_t mask;
	CPU_ZERO(&mask);
	CPU_SET(cpu, &mask);

	int err, syscallres;
	pid_t pid = gettid();
	syscallres = syscall(__NR_sched_setaffinity, pid, sizeof(mask), &mask);
	if (syscallres) {
		err = errno;
		return FALSE;
	}
	return TRUE;
}


NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API PlatformAndroid* SDK_PlatformAndroid_GetInstance()
	{
		return PlatformAndroid::GetInstance();
	}
	VFX_API void* Android_ANWinFromSurface(JNIEnv* jniEnv, jobject surface)
	{
		return (void*)ANativeWindow_fromSurface(jniEnv, (jobject)surface);
	}
	CSharpAPI2(EngineNS, PlatformAndroid, InitAndroid, JNIEnv*, jobject);
}
