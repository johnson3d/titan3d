#pragma once

#include "../IUnknown.h"
#include "../thread/vfxcritical.h"

NS_BEGIN

namespace MemAlloc
{
	template<typename _Type>
	struct FPage;
	template<typename _Type>
	struct FAllocatorBase;

	template<typename _Type>
	struct FPagedObject : public VIUnknown
	{
		_Type					RealObject{};
		FPagedObject<_Type>*	Next = nullptr;
		TWeakRefHandle<FPage<_Type>>		HostPage;

		FAllocatorBase<_Type>* GetAllocator() {
			auto page = HostPage.GetPtr();
			if (page == nullptr)
				return nullptr;
			return page->Allocator.GetPtr();
		}
		void Free();
	};

	struct IAllocator : public IWeakReference
	{
		UINT							LiveCount = 0;
		UINT							PoolCount = 0;
		UINT GetTotalCount() const {
			return LiveCount + PoolCount;
		}
	};

	template<typename _Type>
	struct FAllocatorBase : public IAllocator
	{
		typedef FPagedObject<_Type>		FPagedObjectType;

		template<typename _FPagedObjectType = FPagedObjectType>
		AutoRef<_FPagedObjectType> Alloc() {
			auto ptr = dynamic_cast<_FPagedObjectType*>(AllocObject());
			return MakeWeakRef(ptr);
		}

		virtual FPagedObjectType* AllocObject() = 0;
		virtual void Free(FPagedObjectType* ptr) = 0;
		virtual void OnFree(FPagedObjectType* ptr) = 0;
	};

	template<typename _Type>
	struct FPage : public IWeakReference
	{
		TWeakRefHandle<FAllocatorBase<_Type>>	Allocator;
	};
	
	template<typename _Type, typename _CreatorType, bool MultThread = true>
	struct FPagedObjectAllocator : public FAllocatorBase<_Type>
	{
		typedef FAllocatorBase<_Type>	BaseType;
		typedef FPagedObject<_Type>		FPagedObjectType;
		typedef FPage<_Type>			FPageType;
		
		//UINT							PageSize = 128;
		VSLLock							Locker;
		_CreatorType					Creator{};
		FPagedObjectType*				FreePoint = nullptr;
		std::vector<AutoRef<FPageType>>	Pages;
		
		virtual FPagedObjectType* AllocObject() override
		{
			if constexpr (MultThread)
			{
				VAutoVSLLock al(Locker);
				auto result = AllocImpl();
				Creator.OnAlloc(this, result);
				return result;
			}
			else
			{
				auto result = AllocImpl();
				Creator.OnAlloc(this, result);
				return result;
			}
		}
		virtual void Free(FPagedObjectType* ptr) override
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
		virtual void OnFree(FPagedObjectType* ptr) override
		{
			Creator.OnFree(this, ptr);
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
		~FPagedObjectAllocator()
		{
			FinalCleanup();
		}
	protected:
		FPagedObjectType* AllocImpl()
		{
			BaseType::GetTotalCount();
			if (FreePoint == nullptr)
			{
				ASSERT(BaseType::PoolCount == 0);
				auto PageSize = Creator.GetPageSize();
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

				BaseType::PoolCount += PageSize;
			}
			auto result = FreePoint;
			FreePoint = FreePoint->Next;
			result->Next = nullptr;
			BaseType::LiveCount++;
			BaseType::PoolCount--;
			return result;
		}
		void FreeImpl(FPagedObjectType* ptr)
		{
			ptr->AddRef();
			ptr->Next = FreePoint;
			FreePoint = ptr;

			BaseType::LiveCount--;
			BaseType::PoolCount++;
		}
		void FinalCleanupImpl()
		{
			while (FreePoint)
			{
				auto saved = FreePoint;
				FreePoint = FreePoint->Next;
				Creator.OnFree(this, saved);
				saved->Release();
			}
			for (auto& i : Pages)
			{
				Creator.FinalCleanup(i);
			}
			Pages.clear();
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
		allocator->OnFree(this);
		allocator->Free(this);
	}
}

NS_END

