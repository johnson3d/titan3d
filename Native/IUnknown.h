#pragma once

#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#endif

#include <atomic>
#include <queue>
#include "BaseHead.h"
#include "HashDefine.h"
#include "BaseDefines/CoreRtti.h"

typedef long long vTimeTick;

class vfxObjectLocker;

NS_BEGIN
struct ObjectHandle;
struct IResourceState;
class VIUnknown
{
private:
	ObjectHandle * Handle;
	std::atomic<int>	RefCount;
public:
	static vTimeTick	EngineTime;
public:
	static CoreRtti _RttiInfo;
	static const vIID __UID__ = 0x0000000000000000;
	virtual CoreRtti* GetRtti() { return &_RttiInfo; }

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

struct VStringObject : public VIUnknown
{
	RTTI_DEF(VStringObject, 0x1af1368c5f65da55, true);
	std::string		mText;
	const char* GetTextString() {
		return mText.c_str();
	}
};

template<typename T>
void Safe_Release(T*& p)
{
	if (p == NULL)
		return;
	p->Release();
	p = nullptr;
}

template<typename T>
void Safe_SetVIUnknown(T*& p, T* np)
{
	if (np == p)
		return;

	auto saved = p;
	if (np != nullptr)
		np->AddRef();
	p = np;
	if (saved != nullptr)
		saved->Release();
}


template<class TYPE>
inline void Safe_Delete(TYPE * & p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete refPtr;
	refPtr = NULL;
}

template<class TYPE>
inline void Safe_DeleteArray(TYPE * & p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete[] refPtr;
	refPtr = NULL;
}

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
	void UnsafeRef(T* ptr)
	{
		Safe_Release(Ptr);
		Ptr = ptr;
	}
	void StrongRef(T* ptr)
	{
		if (ptr)
			ptr->AddRef();
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
template<typename Type>
inline Type VGetTypeDefault()
{
	return (Type)(0);
}

inline void VGetTypeDefault()
{

}

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

enum	EStreamingState
{
	SS_Unknown,
	SS_WaitDownload,
	SS_Downloading,
	SS_DLFailed,
	SS_Invalid,
	SS_Pending,
	SS_Streaming,
	SS_Valid,
	SS_PendingKill,
	SS_Killing,
	SS_Killed
};

EnumBegin(EStreamingState)
	EnumMember(SS_Unknown);
	EnumMember(SS_WaitDownload);
	EnumMember(SS_Downloading);
	EnumMember(SS_DLFailed);
	EnumMember(SS_Invalid);
	EnumMember(SS_Pending);
	EnumMember(SS_Streaming);
	EnumMember(SS_Valid);
	EnumMember(SS_PendingKill);
	EnumMember(SS_Killing);
	EnumMember(SS_Killed);
EnumEnd(EStreamingState,Global)

struct IResourceState
{
	EStreamingState		mStreamState;
	vTimeTick			mAccessTime;
	unsigned int		mResourceSize;
	int					mKeepValid;
	int					mPreUseCounterBeforeStreaming;
	int					mLoadPriority;

	IResourceState()
	{
		mStreamState = SS_Invalid;
		mAccessTime = 0;
		mResourceSize = 0;
		mKeepValid = 0;
		mPreUseCounterBeforeStreaming = 0;
		mLoadPriority = 0;
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
	vTimeTick GetAccessTime() const {
		return mAccessTime;
	}
	void SetAccessTime(vTimeTick t) {
		mAccessTime = t;
	}
	int GetKeepValid() const {
		return mKeepValid;
	}
	void SetKeepValid(int keep) {
		mKeepValid = keep;
	}
	
	/*virtual void Invalidate() {
		mResourceSize = 0;
	}
	virtual bool Restore() {
		mResourceSize++;
		return true;
	}*/
};

enum EPlatformType
{
	PLTF_Windows = (1 << 0),
	PLTF_Android = (1 << 1),
	PLTF_AppleIOS = (1 << 2),
};

EnumBegin(EPlatformType)
	EnumMember(PLTF_Windows);
	EnumMember(PLTF_Android);
	EnumMember(PLTF_AppleIOS);
EnumEnd(EPlatformType,Global)

class IVPlatform : VIUnknown
{	
public:
	static IVPlatform*		Instance;
public:
	virtual EPlatformType GetPlatformType() = 0;
};

#define VDef_ReadOnly(name) auto Get##name(){return m##name;}
#define VDef_ReadOnlyReference(name) auto& GetRef##name(){return m##name;}
#define VDef_ReadWrite(type, name, prefix) type Get##name(){return prefix##name;} \
		void Set##name(type value){ prefix##name = value; }

#define TR_CLASS(...)
#define TR_FUNCTION(...)
#define TR_MEMBER(...)
#define TR_ENUM(...)
#define TR_CONSTRUCTOR(...)

NS_END