#pragma once
#include "vfxdebug.h"
#include "../thread/vfxcritical.h"

namespace VMem
{
	class MemPool;

	class MemPoolManager
	{
		struct MyAllocator
		{
			static void * Malloc(size_t size)
			{
				return malloc(size);
			}
			static void Free(void* p)
			{
				free(p);
			}
		};
		VMem::vector<MemPool*, MyAllocator>	mPools;
		static bool				mFinalized;
		VCritical				mLocker;
	public:
		static bool IsFinalized() {
			return mFinalized;
		}
		static MemPoolManager* GetInstance();

		MemPoolManager();
		~MemPoolManager();
		void FinalCleanup();
		void Check();
		void Dump();
		MemPool* CreatePool(const char* cname, size_t size, size_t chunkSize);
		MemPool* GetPool(const char* cname);
		size_t GetPoolSize(const char* cname);
		size_t GetUsedSize(const char* cname);
		size_t GetAllPoolSize();
		size_t GetAllUsedSize();
	};
	class MemPool
	{
		friend MemPoolManager;
		struct MyAllocator
		{
			static void * Malloc(size_t size)
			{
				return malloc(size);
			}
			static void Free(void* p)
			{
				free(p);
			}
		};

		struct MemElement
		{
			MemElement*		Next;
			const char*		File;
			int				Line;
			const char*		DebugInfo;
			size_t			Id;
			size_t			Dccc;
		};
		VMem::vector<char*, MyAllocator>		mMemChunks;
		size_t					mSize;
		size_t					mChunkSize;
		char*					mCurChunk;

		const char*				mClassName;
		size_t					mAllocSerialId;
		int						mLiveNumber;
		bool					mCleaning;
		VCritical				mLocker;
	public:
		MemPool(const char* cname, size_t size, size_t chunkSize);
		~MemPool();
		void Cleanup();
		void* Alloc(size_t size, const char* file, int line);
		void Free(size_t size, void* p);
		void Check();
		void Dump();
	};

	template<class T, int chunkSize = 10>
	class PooledObject
	{
		static MemPool*		mMemPool;
		static void*		mHolder;
		static MemPool* GetMemPool() 
		{
			if (mMemPool == NULL)
			{
				mMemPool = MemPoolManager::GetInstance()->CreatePool(T::GetPooledObjectClassName(), sizeof(T), chunkSize);
			}
			return mMemPool;
		}
	public:
		#define USE_POOLEDOBJECT
		static void BeginHold()
		{
#if defined(USE_POOLEDOBJECT)
			if (mHolder == NULL)
			{
				mHolder = GetMemPool()->Alloc(sizeof(T), __FILE__, __LINE__);
			}
#endif
		}
		static void EndHold(void* p)
		{
#if defined(USE_POOLEDOBJECT)
			if (mHolder)
			{
				GetMemPool()->Free(sizeof(T), mHolder);
			}
#endif
		}
#if defined(USE_POOLEDOBJECT)
		void* operator new(size_t size, const char* file, int line)// _cdecl
		{
			return GetMemPool()->Alloc(size, file, line);
		}
		void* operator new(size_t size)// _cdecl
		{
			return GetMemPool()->Alloc(size, NULL, 0);;
		}
		void operator delete(void* p, const char* file, int line)//_cdecl 
		{
			GetMemPool()->Free(0, p);
		}
		void operator delete(void* p, size_t size)//_cdecl 
		{
			GetMemPool()->Free(size, p);
		}
#endif
	};

	template<class T, int chunkSize>
	MemPool* PooledObject<T, chunkSize>::mMemPool = NULL;// (T::GetPooledObjectClassName(), sizeof(T), chunkSize);
	template<class T, int chunkSize>
	void* PooledObject<T, chunkSize>::mHolder = NULL;
}