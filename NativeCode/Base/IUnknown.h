#pragma once

#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#endif

#include <atomic>
#include <queue>
#include "debug/vfxnew.h"
#include "BaseHead.h"
#include "HashDefine.h"
#include "CoreSDK.h"
#include "CoreRtti.h"

class vfxObjectLocker;

NS_BEGIN

struct ObjectHandle;
struct IResourceState;

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
VIUnknown
{
private:
	ObjectHandle * Handle;
	std::atomic<int>	RefCount;
public:
	static INT64	EngineTime;
public:
	static CoreRtti _RttiInfo;
	static const vIID __UID__ = 0x0000000000000000;
	virtual CoreRtti* GetRtti() { return &_RttiInfo; }
	VIUnknown(const VIUnknown& rh)
	{
		assert(false);
	}
	inline VIUnknown& operator = (const VIUnknown& rh) {
		assert(false);
		return *this;
	}

	virtual long AddRef();
	virtual void Release();

	virtual void DeleteThis();

	virtual void Cleanup() {}

	virtual Hash64 GetHash64();
	virtual IResourceState* GetResourceState() {
		return nullptr;
	}
	virtual void InvalidateResource() {
		return;
	}
	virtual vBOOL RestoreResource() {
		return TRUE;
	}
	ObjectHandle* GetHandle();
	

	vfxObjectLocker* GetLocker(int index = 0) const;
	VIUnknown();
	virtual ~VIUnknown();
};

template<class T>
class AutoRef
{
	T*		Ptr;
public:
	AutoRef()
	{
		Ptr = nullptr;
	}
	AutoRef(T* ptr)
	{
		Ptr = ptr;
	}
	AutoRef(const AutoRef<T>& rh)
	{
		Ptr = rh.Ptr;
		if (Ptr != nullptr)
			Ptr->AddRef();
	}
	~AutoRef()
	{
		Safe_Release(Ptr);
	}
	void StrongRef(T* ptr)
	{
		if (ptr)
			ptr->AddRef();
		Safe_Release(Ptr);
		Ptr = ptr;
	}
	void WeakRef(T* ptr)
	{
		Safe_Release(Ptr);
		Ptr = ptr;
	}
	void Clear()
	{
		Safe_Release(Ptr);
	}
	AutoRef<T>& operator = (const AutoRef<T>& rh)
	{
		Safe_Release(Ptr);
		Ptr = rh.Ptr;
		if (Ptr != nullptr)
			Ptr->AddRef();
		return *this;
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
	/*T* operator->()
	{
		return Ptr;
	}
	const T* operator->() const
	{
		return Ptr;
	}*/
	template<class ConverType>
	ConverType* UnsafeConvertTo()
	{
#if PLATFORM_WIN
		return dynamic_cast<ConverType*>(Ptr);
#else
		return (ConverType*)(Ptr);
#endif
	}
	
	operator T*() const
	{
		return Ptr;
	}
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

struct ObjectHandle
{
	ObjectHandle()
	{
		RefCount = 1;
		mPtrAddress = nullptr;
	}
	int AddRef();
	void Release();
	std::atomic<int> RefCount;
	VIUnknown*	mPtrAddress;
	static ObjectHandle* NewHandle(VIUnknown* ptr);
};

template<class T>
class TObjectHandle
{
	ObjectHandle*	Handle;
public:
	TObjectHandle()
	{
		Handle = nullptr;
	}
	~TObjectHandle()
	{
		Safe_Release(Handle);
	}
	void FromObject(VIUnknown* obj)
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
	AutoRef<T> GetPtr() const
	{
		if (Handle == nullptr)
			return nullptr;
		if (Handle->mPtrAddress == nullptr)
			return nullptr;
		Handle->mPtrAddress->AddRef();
		return AutoRef<T>((T*)Handle->mPtrAddress);
	}
};

class VDefferedDeleteManager
{
	std::queue<VIUnknown*>		ObjectPool;
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
	void PushObject(VIUnknown* obj);
	void Tick(int limitTimes);
	void Cleanup();
};

#define EngineIsCleared VDefferedDeleteManager::GetInstance()->GetIsCleared()==1

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EStreamingState
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

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IResourceState
{
	EStreamingState		mStreamState;
	INT64				mAccessTime;
	unsigned int		mResourceSize;

	IResourceState()
	{
		mStreamState = SS_Invalid;
		mAccessTime = 0;
		mResourceSize = 0;
	}
	inline EStreamingState GetStreamState()
	{
		return mStreamState;
	}
	inline void SetStreamState(EStreamingState state)
	{
		mStreamState = state;
	}
	unsigned int GetResourceSize() const {
		return mResourceSize;
	}
	void SetResourceSize(unsigned int size) {
		mResourceSize = size;
	}
	INT64 GetAccessTime() const {
		return mAccessTime;
	}
	void SetAccessTime(INT64 t) {
		mAccessTime = t;
	}
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
EPlatformType
{
	PLTF_Windows = (1 << 0),
	PLTF_Android = (1 << 1),
	PLTF_AppleIOS = (1 << 2),
};

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef bool(*FOnManagedObjectHolderDestroy)(void* handle);

VTypeHelperDefine(FOnManagedObjectHolderDestroy, sizeof(void*));

StructBegin(FOnManagedObjectHolderDestroy, EngineNS)
StructEnd(void)

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
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