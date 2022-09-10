#pragma once

#include "../IUnknown.h"
#include "../thread/vfxcritical.h"

NS_BEGIN

namespace MemAlloc
{
	template<typename _Type>
	struct FPage;

	template<typename _Type>
	struct FPagedObject : public VIUnknownBase
	{
		_Type					RealObject{};
		FPagedObject<_Type>*	Next = nullptr;
		TObjectHandle<FPage<_Type>>		HostPage;

		void Free();
	};

	template<typename _Type>
	struct FAllocatorBase : public VIUnknown
	{
		virtual AutoRef<FPagedObject<_Type>> Alloc() = 0;
		virtual void Free(FPagedObject<_Type>* ptr) = 0;
	};

	template<typename _Type>
	struct FPage : public VIUnknown
	{
		TObjectHandle<FAllocatorBase<_Type>>	Allocator;
	};
	
	template<typename _Type, typename _CreatorType, UINT PageSize = 128, bool MultThread = true>
	struct FPagedObjectAllocator : public FAllocatorBase<_Type>
	{
		VSLLock							Locker;
		_CreatorType					Creator{};
		FPagedObject<_Type>*			FreePoint = nullptr;
		std::vector<AutoRef<FPage<_Type>>>		Pages;
		virtual AutoRef<FPagedObject<_Type>> Alloc() override
		{
			if constexpr (MultThread)
			{
				VAutoVSLLock al(Locker);
				return AllocImpl();
			}
			else
			{
				return AllocImpl();
			}
		}
		virtual void Free(FPagedObject<_Type>* ptr) override
		{
			if constexpr (MultThread)
			{
				VAutoVSLLock al(Locker);
				FreeImpl(ptr);
			}
			else
			{
				FreeImpl(ptr);
			}
		}
		void FinalCleanup()
		{
			if constexpr (MultThread)
			{
				VAutoVSLLock al(Locker);
				FinalCleanupImpl();
			}
			else
			{
				FinalCleanupImpl();
			}
		}
	protected:
		AutoRef<FPagedObject<_Type>> AllocImpl()
		{
			if (FreePoint == nullptr)
			{
				auto page = MakeWeakRef(Creator.CreatePage(PageSize));
				if (page == nullptr)
					return nullptr;

				page->Allocator.FromObject(this);

				FPagedObject<_Type>* prev = nullptr;
				FPagedObject<_Type>* first = nullptr;
				for (UINT i = 0; i < PageSize; i++)
				{
					auto obj = Creator.CreatePagedObject(page, i);
					obj->HostPage.FromObject(page);
					obj->Next = nullptr;
					if (first == nullptr)
					{
						first = obj;
					}
					if (prev != nullptr)
					{
						prev->Next = obj;
					}
					prev = obj;
				}
				FreePoint = first;
				Pages.push_back(page);
			}
			auto result = FreePoint;
			FreePoint = FreePoint->Next;
			result->Next = nullptr;
			return result;
		}
		void FreeImpl(FPagedObject<_Type>* ptr)
		{
			ptr->Next = FreePoint;
			FreePoint = ptr;
		}
		void FinalCleanupImpl()
		{
			while (FreePoint)
			{
				auto saved = FreePoint;
				FreePoint = FreePoint->Next;
				Creator.OnFree(saved);
				saved->Release();
			}
			for (auto& i : Pages)
			{
				Creator.FinalCleanup(i);
			}
		}
	};
	
	template<typename _Type>
	void FPagedObject<_Type>::Free()
	{
		auto page = HostPage.GetPtr();
		if (page == nullptr)
			return;
		auto allocator = page->Allocator.GetPtr();
		if (allocator == nullptr)
			return;
		allocator->Free(this);
	}
}

NS_END

