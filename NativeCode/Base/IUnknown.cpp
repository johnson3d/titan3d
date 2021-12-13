#include "IUnknown.h"
#include "BaseHead.h"
#include "thread/vfxcritical.h"
//#include "RHIRenderer/PreHead.h"
#include "CSharpAPI.h"

#define new VNEW

NS_BEGIN

void VIUnknownBase::DeleteThis()
{
	delete this;
}

CoreRtti VIUnknown::_RttiInfo("EngineNS::VIUnknown", "", VIUnknown::__UID__, sizeof(VIUnknown), nullptr, __FILE__, __LINE__);
const vIID VIUnknown::__UID__;
INT64 VIUnknown::EngineTime = 0;

FOnManagedObjectHolderDestroy IManagedObjectHolder::OnManagedObjectHolderDestroy = nullptr;

StructImpl(FOnManagedObjectHolderDestroy)

vfxObjectLocker gDefaultObjectLocker;

VIUnknown::VIUnknown()
{
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

void VIUnknown::Cleanup()
{

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
