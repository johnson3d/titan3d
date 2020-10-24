#include "IUnknown.h"
#include "BaseHead.h"
#include "Core/thread/vfxcritical.h"
#include "RHIRenderer/PreHead.h"
#include "CSharpAPI.h"

#define new VNEW

NS_BEGIN

EnumImpl(EStreamingState);
EnumImpl(EPlatformType);


CoreRtti VIUnknown::_RttiInfo("EngineNS::VIUnknown", "", VIUnknown::__UID__, sizeof(VIUnknown), nullptr, __FILE__, __LINE__);
const vIID VIUnknown::__UID__;
vTimeTick VIUnknown::EngineTime = 0;

RTTI_IMPL(EngineNS::VStringObject, EngineNS::VIUnknown);

vfxObjectLocker gDefaultObjectLocker;

VIUnknown::VIUnknown()
{
	RefCount = 1;
	//Handle = ObjectHandle::NewHandle(this);
	Handle = nullptr;
}

VIUnknown::~VIUnknown()
{
	if (Handle != nullptr)
	{
		Handle->mPtrAddress = nullptr;
		Safe_Release(Handle);
	}
}

long VIUnknown::AddRef()
{
	return ++RefCount;
}

void VIUnknown::Release()
{
	RefCount--;
	if (RefCount == 0)
	{
		DeleteThis();
	}
	return;
}

void VIUnknown::DeleteThis()
{
	delete this;
}

vfxObjectLocker* VIUnknown::GetLocker(int index) const
{
	return &gDefaultObjectLocker;
}

ObjectHandle* VIUnknown::GetHandle()
{
	if (Handle == nullptr)
	{
		Handle = ObjectHandle::NewHandle(this);
	}
	Handle->AddRef();
	return Handle;
}

Hash64 VIUnknown::GetHash64() 
{
	return Hash64::Empty;
}

int ObjectHandle::AddRef()
{
	return ++RefCount;
}

void ObjectHandle::Release()
{
	RefCount--;
	if (RefCount == 0)
		delete this;
}

ObjectHandle* ObjectHandle::NewHandle(VIUnknown* ptr)
{
	auto handle = new ObjectHandle();
	handle->mPtrAddress = ptr;
	return handle;
}

VDefferedDeleteManager* VDefferedDeleteManager::GetInstance()
{
	static VDefferedDeleteManager obj;
	return &obj;
}

void VDefferedDeleteManager::Cleanup()
{
	Tick(-1);
	IsCleared = 1;
}

VCritical GDefferedDeleteLocker;
void VDefferedDeleteManager::PushObject(VIUnknown* obj)
{
	if (IsCleared == 1)
	{
		delete obj;
		return;
	}
	VAutoLock(GDefferedDeleteLocker);
	auto refCount = obj->AddRef();
	if (refCount != 1)
	{
		ASSERT(false);
		VFX_LTRACE(ELTT_Memory, "Object %s will be deleted, Its RefCount = %d\r\n", obj->GetRtti()->ClassName.c_str(), refCount);
		return;
	}
	
	ObjectPool.push(obj);
}

void VDefferedDeleteManager::Tick(int limitTimes)
{
	while (ObjectPool.size() > 0)
	{
		VIUnknown* cur;
		{
			VAutoLock(GDefferedDeleteLocker);
			cur = ObjectPool.front();
			ObjectPool.pop();
		}
		auto ref = cur->AddRef();
		ASSERT(ref == 2);
		delete cur;
	}
}

int func() { return 0; } // termination version

template<typename Arg1, typename... Args>
int func(const Arg1& arg1, const Args&... args)
{
	//process(arg1); 
	return func(args...); // note: arg1 does not appear here!
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API void SDK_VDefferedDeleteManager_Tick(int limitTimes)
	{
		VDefferedDeleteManager::GetInstance()->Tick(limitTimes);
	}
	VFX_API void SDK_VDefferedDeleteManager_Cleanup()
	{
		VDefferedDeleteManager::GetInstance()->Cleanup();
	}
	Cpp2CS0(EngineNS, VIUnknown, AddRef);
	Cpp2CS0(EngineNS, VIUnknown, Release);
	Cpp2CS0(EngineNS, VIUnknown, Cleanup);
	VFX_API void SDK_VIUnknown_GetHash64(VIUnknown* self, Hash64* hash)
	{
		if (self == nullptr)
		{
			*hash = Hash64::Empty;
			return;
		}
		*hash = self->GetHash64();
	}
	Cpp2CS0(EngineNS, VIUnknown, GetResourceState);
	Cpp2CS0(EngineNS, VIUnknown, InvalidateResource);
	Cpp2CS0(EngineNS, VIUnknown, RestoreResource);

	Cpp2CS0(EngineNS, VIUnknown, GetRtti);
	Cpp2CS0(EngineNS, IResourceState, GetStreamState);
	Cpp2CS1(EngineNS, IResourceState, SetStreamState);
	Cpp2CS0(EngineNS, IResourceState, GetResourceSize);
	Cpp2CS1(EngineNS, IResourceState, SetResourceSize);
	Cpp2CS0(EngineNS, IResourceState, GetAccessTime);
	Cpp2CS1(EngineNS, IResourceState, SetAccessTime);
	Cpp2CS0(EngineNS, IResourceState, GetKeepValid);
	Cpp2CS1(EngineNS, IResourceState, SetKeepValid);

	VFX_API IResourceState* SDK_New_IResourceState()
	{
		return new IResourceState();
	}
	VFX_API void SDK_Delete_IResourceState(IResourceState* state)
	{
		delete state;
	}

	Cpp2CS0(EngineNS, VStringObject, GetTextString);
}