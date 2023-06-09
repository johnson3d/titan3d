#pragma once

#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#endif

#include "BaseHead.h"

#include "debug/vfxnew.h"
#include "HashDefine.h"
#include "CoreSDK.h"
#include "CoreRtti.h"

class vfxObjectLocker;

NS_BEGIN

struct WeakRefHandle;
struct FResourceState;

class TR_CLASS()
	IWeakReference : public VIUnknown
{
private:
	WeakRefHandle* Handle;
public:
	static UINT64	EngineCurrentFrame;
public:	
	IWeakReference();
	virtual ~IWeakReference();
	virtual long AddRef() override
	{
		return ++RefCount;
	}
	virtual void Release() override
	{
		RefCount--;
		if (RefCount == 0)
		{
			DeleteThis();
		}
	}
	long UnsafeGetRefCount() {
		return RefCount;
	}

	virtual Hash64 GetHash64();
	virtual void Cleanup();
	
	WeakRefHandle* GetHandle();

	vfxObjectLocker* GetLocker(int index = 0) const;	
};


template<class T, bool isArray = false>
class AutoPtr
{
	T*		Ptr;
public:
	AutoPtr()
	{
		Ptr = nullptr;
	}
	AutoPtr(T* ptr)
	{
		Ptr = ptr;
	}
	~AutoPtr()
	{
		if (isArray)
		{
			Safe_DeleteArray(Ptr);
		}
		else
		{
			Safe_Delete(Ptr);
		}
	}
	void UnsafeSet(T* ptr)
	{
		Ptr = ptr;
	}
	bool operator==(T* rh) const
	{
		return (Ptr == rh);
	}
	bool operator !=(T* rv) const
	{
		return (Ptr != rv);
	}

	T* operator->() const
	{
		return Ptr;
	}
	T* GetPtr() const {
		return Ptr;
	}
	operator T*() const
	{
		return Ptr;
	}
};

struct WeakRefHandle
{
	WeakRefHandle()
	{
		RefCount = 1;
		mPtrAddress = nullptr;
	}
	int AddRef();
	void Release();
	std::atomic<int> RefCount;
	IWeakReference*	mPtrAddress;
	static WeakRefHandle* NewHandle(IWeakReference* ptr);
};

template<class T>
class TWeakRefHandle
{
	WeakRefHandle*	Handle;
public:
	TWeakRefHandle()
	{
		Handle = nullptr;
	}
	~TWeakRefHandle()
	{
		Safe_Release(Handle);
	}
	void FromObject(IWeakReference* obj)
	{
		Safe_Release(Handle);
		if(obj!=nullptr)
			Handle = obj->GetHandle();
	}
	bool IsValid()
	{
		if (Handle == nullptr)
			return false;
		if (Handle->mPtrAddress == nullptr)
			return false;
		return true;
	}
	T* GetPtr() const
	{
		if (Handle == nullptr)
			return nullptr;
		return (T*)Handle->mPtrAddress;
	}
};

class VDefferedDeleteManager
{
	std::queue<IWeakReference*>		ObjectPool;
	int							IsCleared;
public:
	VDefferedDeleteManager()
	{
		IsCleared = 0;
	}
	int GetIsCleared() const{
		return IsCleared;
	}
	static VDefferedDeleteManager* GetInstance();
	void PushObject(IWeakReference* obj);
	void Tick(int limitTimes);
	void Cleanup();
};

#define EngineIsCleared VDefferedDeleteManager::GetInstance()->GetIsCleared()==1

enum TR_ENUM()
	EResourceState
{
	SS_Unknown,
	SS_Invalid,
	SS_Pending,
	SS_Streaming,
	SS_Valid,
	SS_PendingKill,
	SS_Killing,
	SS_Killed
};

struct TR_CLASS(SV_LayoutStruct = 8)
	FResourceState
{
	EResourceState		mStreamState = SS_Invalid;
	UINT				mResourceSize = 0;
	UINT64				mAccessFrame = 0;
	

	FResourceState()
	{
		mStreamState = SS_Invalid;
		mAccessFrame = 0;
		mResourceSize = 0;
	}
	inline EResourceState GetStreamState()
	{
		return mStreamState;
	}
	inline void SetStreamState(EResourceState state)
	{
		mStreamState = state;
	}
	unsigned int GetResourceSize() const {
		return mResourceSize;
	}
	void SetResourceSize(unsigned int size) {
		mResourceSize = size;
	}
	UINT64 GetAccessFrame() const {
		return mAccessFrame;
	}
	void SetAccessFrame(UINT64 t) {
		mAccessFrame = t;
	}
};

class TR_CLASS()
	IResourceBase : public VIUnknown
{
public:
	virtual FResourceState* GetResourceState() {
		return nullptr;
	}
	virtual void InvalidateResource() {
		return;
	}
	virtual bool RestoreResource(IWeakReference* pDevice) {
		return true;
	}
};

enum TR_ENUM()
	EPlatformType
{
	PLTF_Windows = (1 << 0),
	PLTF_Android = (1 << 1),
	PLTF_AppleIOS = (1 << 2),
};

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef bool(*FOnManagedObjectHolderDestroy)(void* handle);

class TR_CLASS()
	IManagedObjectHolder
{
	static FOnManagedObjectHolderDestroy OnManagedObjectHolderDestroy;
public:
	IManagedObjectHolder()
		: mHandle(nullptr)
	{

	}
	~IManagedObjectHolder()
	{
		if (mHandle != nullptr)
		{
			if (OnManagedObjectHolderDestroy != nullptr)
				OnManagedObjectHolderDestroy(mHandle);
			mHandle = nullptr;
		}
	}
	void SetHandle(void* handle)
	{
		if (mHandle != nullptr)
		{
			if (OnManagedObjectHolderDestroy != nullptr)
				OnManagedObjectHolderDestroy(mHandle);
		}
		mHandle = handle;
	}
	static void SetOnManagedObjectHolderDestroy(FOnManagedObjectHolderDestroy fn)
	{
		OnManagedObjectHolderDestroy = fn;
	}
	void*		mHandle;
};

NS_END