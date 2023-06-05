#pragma once
#include "../Base/IUnknown.h"
#include "../Base/thread/vfxevent.h"
#include "NxRHIDefine.h"
#include <mutex>

NS_BEGIN

namespace NxRHI
{
	class ICmdQueue;
	struct TR_CLASS(SV_LayoutStruct = 8)
		FEventDesc
	{

	};
	class TR_CLASS()
		IEvent : public VIUnknown
	{
	public:
		ENGINE_RTTI(IEvent);
		virtual bool Wait(UINT timeOut = INFINITE) = 0;
		virtual void SetEvent() = 0;
		virtual void ResetEvent() = 0;

		const char* GetName() const {
			return Name.c_str();
		}
		std::string			Name;
	};
	enum TR_ENUM() EFenceType{
		NONE = 0,
		SHARED = 0x2,
		SHARED_CROSS_ADAPTER = 0x4,
		NON_MONITORED = 0x8
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FFenceDesc
	{
		EFenceType Type = EFenceType::NONE;
		UINT64 InitValue = 0;
	};
	class TR_CLASS()
		IFence : public IWeakReference
	{
	public:
		ENGINE_RTTI(IFence);
		virtual UINT64 GetCompletedValue() = 0;
		virtual UINT64 Wait(UINT64 value, UINT timeOut = INFINITE) = 0;
		const char* GetName() const {
			return Name.c_str();
		}
		virtual void SetDebugName(const char* name) {}
		inline UINT64 WaitToExpect(UINT timeOut = INFINITE) {
			return Wait(ExpectValue, timeOut);
		}
		UINT64 IncreaseExpect(ICmdQueue* queue, UINT64 num, EQueueType type) {
			std::lock_guard<std::mutex> lck(mLocker);
			ExpectValue += num;
			auto completed = this->GetCompletedValue();
			ASSERT(completed <= ExpectValue);
			Signal(queue, ExpectValue, type);
			return ExpectValue;
		}
		inline UINT64 GetExpectValue() const {
			return ExpectValue;
		}
	protected:
		virtual void CpuSignal(UINT64 value) = 0;
		virtual void Signal(ICmdQueue* queue, UINT64 value, EQueueType type) = 0;
	public:
		std::string		Name;
		FFenceDesc		Desc{};
	
		UINT64			ExpectValue;
		std::mutex		mLocker;
	};
}

NS_END