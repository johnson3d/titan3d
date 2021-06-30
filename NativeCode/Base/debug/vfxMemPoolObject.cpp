#include "vfxMemPoolObject.h"
#include "vfxmemory.h"

//namespace VFX_Memory
//{
//	typedef void(WINAPI *FOnMemAlloc)(size_t size, const char* file, size_t line, size_t id);
//	typedef void(WINAPI *FOnMemFree)(size_t size, const char* file, size_t line, size_t id);
//	typedef void(WINAPI *FOnMemLeak)(void* ptr, size_t size, const char* file, size_t line, size_t id, const char* debugInfo);
//
//	extern FOnMemAlloc GOnMemAllocCallBack;
//	extern FOnMemFree GOnMemFreeCallBack;
//	extern FOnMemLeak GOnMemLeakCallBack;
//}

namespace VMem
{
#ifdef WIN64
	const static size_t c_uMalloc0xCC = 0xCCCCCCCCCCCCCCCC;
#else
	const static size_t c_uMalloc0xCC = 0xCCCCCCCC;
#endif
	
	VCritical GMemPoolLocker;
	bool MemPoolManager::mFinalized = false;

	MemPoolManager* MemPoolManager_obj = NULL;
	MemPoolManager* MemPoolManager::GetInstance() 
	{
		if (MemPoolManager_obj == NULL)
		{
			MemPoolManager_obj = (MemPoolManager*)malloc(sizeof(MemPoolManager));
			::new(MemPoolManager_obj, __FILE__, __LINE__) MemPoolManager();
		}
		return MemPoolManager_obj;
	}
	MemPoolManager::MemPoolManager()
	{
	}
	MemPoolManager::~MemPoolManager()
	{
		FinalCleanup();
	}
	void MemPoolManager::FinalCleanup()
	{
		Check();
		Dump();

		VAutoLock(mLocker);
		for (auto i = mPools.begin(); i != mPools.end(); )
		{
			MemPool* pool = *i;
			if (pool->mMemChunks.size() == 0)
			{
				delete pool;
				i = mPools.erase(i);
			}
			else
			{
				i++;
			}
		}
		mFinalized = true;
	}
	void MemPoolManager::Check()
	{
		if (mFinalized)
			return;
		VAutoLock(mLocker);
		for (auto i = mPools.begin(); i != mPools.end(); i++)
		{
			(*i)->Check();
		}
	}
	void MemPoolManager::Dump()
	{
		if (mFinalized)
			return;
		VAutoLock(mLocker);
		for (auto i = mPools.begin(); i != mPools.end(); i++)
		{
			(*i)->Dump();
		}
	}

	MemPool* MemPoolManager::GetPool(const char* cname)
	{
		if (mFinalized)
			return NULL;
		VAutoLock(mLocker);
		for (auto i = mPools.begin(); i != mPools.end(); i++)
		{
			if (strcmp((*i)->mClassName, cname) == 0)
			{
				return *i;
			}
		}
		return NULL;
	}
	size_t MemPoolManager::GetAllPoolSize()
	{
		VAutoLock(mLocker);
		size_t size = 0;
		for (auto i = mPools.begin(); i != mPools.end(); i++)
		{
			MemPool* pool = (*i);
			size_t sz = (pool->mSize + sizeof(MemPool::MemElement) + sizeof(size_t)) * pool->mChunkSize * pool->mMemChunks.size();
			size += sz;
		}
		return size;
	}
	size_t MemPoolManager::GetAllUsedSize()
	{
		VAutoLock(mLocker);
		size_t size = 0;
		for (auto i = mPools.begin(); i != mPools.end(); i++)
		{
			MemPool* pool = (*i);
			size_t sz = pool->mSize * pool->mLiveNumber;
			size += sz;
		}
		return size;
	}
	size_t MemPoolManager::GetPoolSize(const char* cname)
	{
		auto pool = GetPool(cname);
		if (pool == NULL)
			return 0;
		return (pool->mSize + sizeof(MemPool::MemElement) + sizeof(size_t)) * pool->mChunkSize * pool->mMemChunks.size();
	}
	size_t MemPoolManager::GetUsedSize(const char* cname)
	{
		auto pool = GetPool(cname);
		if (pool == NULL)
			return 0;
		return pool->mSize * pool->mLiveNumber;
	}
	MemPool* MemPoolManager::CreatePool(const char* cname, size_t size, size_t chunkSize)
	{
		VAutoLock(mLocker);
		auto ret = new MemPool(cname, size, chunkSize);
		mPools.push_back(ret);
		return ret;
	}

	//////////////////////////////////////////////////////////////////////////
	
	MemPool::MemPool(const char* cname, size_t size, size_t chunkSize)
	{
		mCleaning = false;
		mLiveNumber = 0;
		mAllocSerialId = 0;
		mClassName = cname;
		mSize = size;
		mChunkSize = chunkSize;
		mCurChunk = NULL;
	}
	MemPool::~MemPool()
	{
		Cleanup();
	}
	void MemPool::Cleanup()
	{
		VAutoLock(mLocker);
		for (auto i = mMemChunks.begin(); i != mMemChunks.end(); i++)
		{
			_vfxMemoryDelete(*i, __FILE__, __LINE__);
		}
		mMemChunks.clear();
		mCurChunk = NULL;
	}
	void* MemPool::Alloc(size_t size, const char* file, int line)
	{
		ASSERT(size == mSize);
		VAutoLock(mLocker);
		if (mCleaning == true)
		{
			mCleaning = false;
		}
		if (mCurChunk == NULL)
		{
			size_t elemSize = size + sizeof(MemElement) + sizeof(size_t);
			mCurChunk = (char*)_vfxMemoryNew(elemSize*mChunkSize, __FILE__, __LINE__);
			memset(mCurChunk, 0, elemSize*mChunkSize);
			MemElement* pElem = (MemElement*)mCurChunk;
			pElem->Dccc = c_uMalloc0xCC;
			pElem->Id = -1;
			auto pDCCC = (size_t*)(&mCurChunk[sizeof(MemElement) + size]);
			*pDCCC = c_uMalloc0xCC;
			for (size_t i = 1; i < mChunkSize; i++)
			{
				auto next = (MemElement*)&mCurChunk[elemSize*i];
				auto pDCCC = (size_t*)&mCurChunk[elemSize*i + size + sizeof(MemElement)];
				next->Dccc = c_uMalloc0xCC;
				next->Id = -1;
				*pDCCC = c_uMalloc0xCC;
				pElem->Next = next;
				pElem = next;
			}
			pElem->Next = NULL;
			mMemChunks.push_back(mCurChunk);
		}

		mLiveNumber++;
		MemElement* pElem = (MemElement*)mCurChunk;
		pElem->File = file;
		pElem->Line = line;
		pElem->Id = mAllocSerialId++;
		auto nextFree = pElem->Next;
		void* result = &mCurChunk[sizeof(MemElement)];
		mCurChunk = (char*)nextFree;
		
		if (VFX_Memory::GOnMemFreeCallBack)
		{
			VFX_Memory::GOnMemFreeCallBack(mSize, pElem->File, pElem->Line, pElem->Id);
		}
		return result;
	}
	void MemPool::Free(size_t size, void* p)
	{
		VAutoLock(mLocker);
		{	
			MemElement* pElem = (MemElement*)((char*)p - sizeof(MemElement));

			if (VFX_Memory::GOnMemFreeCallBack)
			{
				VFX_Memory::GOnMemFreeCallBack(mSize, pElem->File, pElem->Line, pElem->Id);
			}

			pElem->Next = (MemElement*)mCurChunk;
			pElem->Id = -1;
			mCurChunk = (char*)pElem;
			mLiveNumber--;
		}
		
		if (mLiveNumber == 0)
		{
			VFX_LTRACE(ELTT_Warning, "MemPool [%s] Cleanup that mLiveNumber == 0\r\n", mClassName);
			mCleaning = true;
			Cleanup();
		}
	}
	void MemPool::Check()
	{
		VAutoLock(mLocker);
		size_t elemSize = mSize + sizeof(MemElement) + sizeof(size_t);
		for (auto i = mMemChunks.begin(); i != mMemChunks.end(); i++)
		{
			auto chunk = (*i);
			for (size_t j = 0; j < mChunkSize; j++)
			{
				MemElement* elem = (MemElement*)&chunk[elemSize * j];
				if (elem->Dccc != c_uMalloc0xCC)
				{
					VFX_Memory::__MemoryTrace(vT("%s(%d) : Class(%s) Verify pointer Head(0x%p) failed!!! Alloc ID : %d\n"),
						elem->File, elem->Line, mClassName, &chunk[elemSize * j + sizeof(MemElement)], elem->Id);
				}
				size_t* tail = (size_t*)&chunk[elemSize * j + sizeof(MemElement) + mSize];
				if (*tail != c_uMalloc0xCC)
				{
					VFX_Memory::__MemoryTrace(vT("%s(%d) : Class(%s) Verify pointer Fail(0x%p) failed!!! Alloc ID : %d\n"),
						elem->File, elem->Line, mClassName, &chunk[elemSize * j + sizeof(MemElement)], elem->Id);
				}
			}
		}
	}
	void MemPool::Dump()
	{
		VAutoLock(mLocker);
		size_t elemSize = mSize + sizeof(MemElement) + sizeof(size_t);
		for (auto i = mMemChunks.begin(); i != mMemChunks.end(); i++)
		{
			auto chunk = (*i);
			for (size_t j = 0; j < mChunkSize; j++)
			{
				MemElement* elem = (MemElement*)&chunk[elemSize * j];
				if (elem->Id != -1)
				{
					if (VFX_Memory::GOnMemLeakCallBack)
						VFX_Memory::GOnMemLeakCallBack(elem+sizeof(MemElement), mSize, elem->File, elem->Line, elem->Id, elem->DebugInfo);
					if (elem->DebugInfo)
					{
						VFX_Memory::__MemoryTrace("%s(%d) : Class(%s) Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d Info : %s\n",
							elem->File, elem->Line, mClassName, mSize, elem->Id, elem->DebugInfo);
					}
					else
					{
						VFX_Memory::__MemoryTrace("%s(%d) : Class(%s) Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d\n",
							elem->File, elem->Line, mClassName, mSize, elem->Id);
					}
				}
			}
		}
	}
}

extern "C"
{
	 void MemPoolManager_Check()
	{
		VMem::MemPoolManager::GetInstance()->Check();
	}
	 void MemPoolManager_Dump()
	{
		VMem::MemPoolManager::GetInstance()->Dump();
	}
	 UINT MemPoolManager_GetPoolSize(const char* cname)
	{
		return (UINT)VMem::MemPoolManager::GetInstance()->GetPoolSize(cname);
	}
	 UINT MemPoolManager_GetUsedSize(const char* cname)
	{
		return (UINT)VMem::MemPoolManager::GetInstance()->GetUsedSize(cname);
	}
	 UINT MemPoolManager_GetAllPoolSize()
	{
		return (UINT)VMem::MemPoolManager::GetInstance()->GetAllPoolSize();
	}
	 UINT MemPoolManager_GetAllUsedSize()
	{
		return (UINT)VMem::MemPoolManager::GetInstance()->GetAllUsedSize();
	}
	/* void MemPoolManager_Check()
	{
		VMem::MemPoolManager::GetInstance()->Check();
	}
	 void MemPoolManager_Dump()
	{
		VMem::MemPoolManager::GetInstance()->Dump();
	}*/
}


namespace VTest
{ 
	class A
	{
		int Number;
	public:
		A(int a)
		{
			Number = 1;
		}
		A(int a, int b)
		{
			Number = 1;
			//throw (int)1;
		}
		~A()
		{

		}
	};

	void TestMemPool()
	{
		A* a1 = NULL;
		/*try
		{
			a1 = new(__FILE__, __LINE__) A(1,1);
		}
		catch (int e)
		{
			e++;
			delete a1;
		}*/
		//a1 = new("",1) A(1, 1);
		a1 = new A(1, 1);
		A* a2 = new(__FILE__, __LINE__)  A(2);
		delete a1;

		A* a3 = new(__FILE__, __LINE__)  A(3);
		delete a2;
		delete a3;
	}
}
