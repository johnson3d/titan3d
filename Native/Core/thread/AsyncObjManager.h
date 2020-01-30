#pragma once
#include <vector>
#include <map>
#include "vfxcritical.h"

namespace MTHelper
{
	enum EForEachResult
	{
		FER_Continue,
		FER_Stop,
		FER_Erase,
	};
	template<typename V>
	struct RefFunction
	{
		static void AddRef(V obj)
		{

		}

		static void Release(V obj)
		{

		}
	};

	template<>
	struct RefFunction<VIUnknown*>
	{
		static void AddRef(VIUnknown* obj)
		{
			if(obj)
				obj->AddRef();
		}

		static void Release(VIUnknown* obj)
		{
			if(obj)
				obj->Release();
		}
	};

	struct AsyncObjManagerAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};

	template<typename K,typename V,typename RFun = RefFunction<V>, typename M = AsyncObjManagerAllocator>
	class AsyncObjManager
	{
	public:
		struct Pair 
		{
			Pair(K k, V v)
			{
				Key = k;
				Value = v;
			}
			K Key;
			V Value;
		};

		VMem::map<K, V, M> AllObjects;
		//VMem::vector<Pair, M> AddList;
		VMem::map<K, V, M> AddList;
		VMem::vector<K, M> RemoveList;

		VCritical Locker;

		void Clear()
		{
			VAutoLock(Locker);
			{
				RemoveList.clear();
				for (auto i = AddList.begin(); i != AddList.end(); i++)
				{
					RFun::Release(i->second);
				}
				AddList.clear();

				for (auto i = AllObjects.begin(); i != AllObjects.end(); i++)
				{
					RFun::Release(i->second);
				}
				AllObjects.clear();
			}
		}

		size_t Size()
		{
			return AllObjects.size() + AddList.size() - RemoveList.size();
		}

		bool SetObj(const K& key, const V& value)
		{
			VAutoLock(Locker);
			RFun::AddRef(value);
			for (auto ite = RemoveList.begin(); ite != RemoveList.end();)
			{
				if ((*ite) == key)
					ite = RemoveList.erase(ite);
				else
					ite++;
			}

			auto iter = AllObjects.find(key);
			if (iter == AllObjects.end())
			{
				auto addIter = AddList.find(key);
				if (addIter != AddList.end())
				{
					RFun::Release(addIter->second);
				}
				AddList[key] = value;
				return true;
			}
			else
			{
				RFun::Release(iter->second);
				AllObjects[key] = value;
				return true;
			}
			RFun::Release(value);
		}

		vBOOL AddObj(const K& key, const V& value)
		{
			VAutoLock(Locker);
			RFun::AddRef(value);
			{
				for (auto i = RemoveList.begin(); i != RemoveList.end(); )
				{
					if (*i == key)
					{
						i = RemoveList.erase(i);
					}
					else
					{
						i++;
					}
				}

				auto iter = AllObjects.find(key);
				if (iter != AllObjects.end())
				{
					RFun::Release(value);
					return FALSE;
				}

				if (AddList.find(key) != AddList.end())
				{
					RFun::Release(value);
					return FALSE;
				}

				//Pair e(key, value);
				AddList[key] = value;
				return TRUE;
			}
			RFun::Release(value);
		}

		vBOOL RemoveObj(const K& key)
		{
			VAutoLock(Locker);
			{
				auto addIter = AddList.find(key);
				if (addIter != AddList.end())
				{
					RFun::Release(addIter->second);
					AddList.erase(addIter);
					return TRUE;
				}

				auto iter = AllObjects.find(key);
				if (iter != AllObjects.end())
				{
					for (auto i = RemoveList.begin(); i != RemoveList.end(); i++)
					{
						if (*i == key)
						{
							return TRUE;
						}
					}
					RemoveList.push_back(key);
					return TRUE;
				}

				return FALSE;
			}
		}

		template<typename Visitor>
		void For_Each(Visitor* visitor)
		{
			BeforeTick();
			VAutoLock(Locker);
			{
				for(auto i = AllObjects.begin(); i!= AllObjects.end() ; )
				{
					EForEachResult eResult = visitor->OnVisit(i->first, i->second);
					switch(eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
						{
							RFun::Release(i->second);
							i = AllObjects.erase(i);
							break;
						}
					default:
						i++;
						break;
					}
				}
			}
		}

		typedef EForEachResult (FOnForEachVisit)(K key, V value);
		void For_EachEasy(std::function<FOnForEachVisit> fnVisit)
		{
			BeforeTick();
			VAutoLock(Locker);
			{
				for (auto i = AllObjects.begin(); i != AllObjects.end(); )
				{
					EForEachResult eResult = fnVisit(i->first, i->second);
					switch (eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
					{
						RFun::Release(i->second);
						i = AllObjects.erase(i);
						break;
					}
					default:
						i++;
						break;
					}
				}
			}
		}

		vBOOL Contains(const K& Key)
		{
			VAutoLock(Locker);
			for (auto ite = RemoveList.begin(); ite != RemoveList.end(); ite++)
			{
				if ((*ite) == Key)
					return FALSE;
			}

			auto iter = AllObjects.find(Key);
			if (iter != AllObjects.end())
				return TRUE;

			auto addIter = AddList.find(Key);
			if (addIter != AddList.end())
				return TRUE;

			return FALSE;
		}

		V FindObj(const K& key)
		{
			VAutoLock(Locker);
			{
				for (auto ite = RemoveList.begin(); ite != RemoveList.end(); ite++)
				{
					if ((*ite) == key)
						return (V)(NULL);
				}

				auto iter = AllObjects.find(key);
				if (iter != AllObjects.end())
					return iter->second;

				auto addIter = AddList.find(key);
				if (addIter != AddList.end())
					return addIter->second;

				return (V)(NULL);
			}
		}

		void BeforeTick()
		{
			VAutoLock(Locker);
			{
				for(auto i = AddList.begin(); i!=AddList.end() ; i++)
				{
					//typename std::map<K,V>::iterator iter = AllObjects.find((*i).first);
					//if(iter!= AllObjects.end())
					//{
					//	ASSERT(false);
					//	RFun::Release((*i).second);
					//	continue;
					//}
					AllObjects[(*i).first] = (*i).second;
				}
				AddList.clear();

				for(auto i = RemoveList.begin(); i!=RemoveList.end() ; i++)
				{
					typename std::map<K,V>::iterator iter = AllObjects.find(*i);
					if(iter== AllObjects.end())
						continue;

					RFun::Release(iter->second);
					AllObjects.erase(iter);
				}
				RemoveList.clear();
			}
		}
	};

	struct ConcurrentAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};
	template<typename K,typename V,typename RFun = RefFunction<V>, typename M = ConcurrentAllocator>
	class ConcurrentMap
	{	
	public:
		VMem::map<K, V , M> Objects;
		VCritical Locker;

		size_t Size()
		{
			return Objects.size();
		}
		void Clear()
		{
			VAutoLock(Locker);
			for(auto i = Objects.begin(); i!=Objects.end() ; i++)
			{
				RFun::Release(i->second);
			}
			Objects.clear();
		}

		bool SetObj(const K& key, const V& value)
		{
			VAutoLock(Locker);
			auto iter = Objects.find(key);
			if (iter == Objects.end())
			{
				RFun::AddRef(value);
				Objects[key] = value;
				return true;
			}
			else
			{
				RFun::Release(iter->second);
				RFun::AddRef(value);
				Objects[key] = value;
				return true;
			}
		}

		bool AddObj(const K& key, const V& value)
		{
			VAutoLock(Locker);
			auto iter = Objects.find(key);
			if(iter!=Objects.end())
			{
				if (iter->second != value)
				{
					RFun::Release(iter->second);
					RFun::AddRef(value);
					Objects[key] = value;
					return true;
				}
				return false;
			}

			RFun::AddRef(value);
			Objects[key] = value;
			return true;
		}

		void RemoveObj(const K& key)
		{
			VAutoLock(Locker);
			auto iter = Objects.find(key);
			if(iter==Objects.end())
				return;

			RFun::Release(iter->second);
			Objects.erase(iter);
		}

		V FindObj(const K& Key)
		{
			VAutoLock(Locker);
			auto iter = Objects.find(Key);
			if(iter!=Objects.end())
				return iter->second;

			return (V)NULL;
		}

		vBOOL Contains(const K& Key)
		{
			VAutoLock(Locker);
			auto iter = Objects.find(Key);
			return (iter != Objects.end());
		}

		template<typename Visitor>
		void For_Each(Visitor* visitor)
		{
			VAutoLock(Locker);
			{
				for(auto i = Objects.begin(); i!=Objects.end() ; )
				{
					EForEachResult eResult = visitor->OnVisit(i->first, i->second);
					switch(eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
						RFun::Release(i->second);
						i = Objects.erase(i);
						break;
					default:
						i++;
						break;
					}
				}
			}
		}

		typedef EForEachResult(FOnForEachVisit)(K key, V value);
		void For_EachEasy(std::function<FOnForEachVisit> fnVisit)
		{
			VAutoLock(Locker);
			{
				for (auto i = Objects.begin(); i != Objects.end(); )
				{
					EForEachResult eResult = fnVisit(i->first, i->second);
					switch (eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
						RFun::Release(i->second);
						i = Objects.erase(i);
						break;
					default:
						i++;
						break;
					}
				}
			}
		}
	};

	template<typename V, typename RFun = RefFunction<V>, typename M = ConcurrentAllocator>
	class ConcurrentVector
	{
	public:
		VMem::vector<V, M> Objects;
		VCritical Locker;

		size_t Size()
		{
			return Objects.size();
		}
		void Clear()
		{
			VAutoLock(Locker);
			for (auto i = Objects.begin(); i != Objects.end(); i++)
			{
				RFun::Release(*i);
			}
			Objects.clear();
		}

		V operator [](int i)
		{
			VAutoLock(Locker);
			if (i < 0)
				return (V)NULL;
			if (i >= (int)Objects.size())
				return (V)NULL;
			return Objects[i];
		}

		void PushBack(const V& value)
		{
			VAutoLock(Locker);
			RFun::AddRef(value);
			Objects.push_back(value);
		}
		void Remove(const V& value)
		{
			VAutoLock(Locker);
			for (auto ite = Objects.begin(); ite != Objects.end(); ite++)
			{
				if ((*ite) == value)
				{
					RFun::Release(*ite);
					Objects.erase(ite);
					break;
				}
			}
		}

		template<typename Visitor>
		void For_Each(Visitor* visitor)
		{
			VAutoLock(Locker);
			{
				for (auto i = Objects.begin(); i != Objects.end();)
				{
					EForEachResult eResult = visitor->OnVisit((*i));
					switch (eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
						RFun::Release(*i);
						i = Objects.erase(i);
						break;
					default:
						i++;
						break;
					}
				}
			}
		}

		typedef EForEachResult(FOnForEachVisit)(V value);
		void For_EachEasy(std::function<FOnForEachVisit> fnVisit)
		{
			VAutoLock(Locker);
			{
				for (auto i = Objects.begin(); i != Objects.end();)
				{
					EForEachResult eResult = fnVisit((*i));
					switch (eResult)
					{
					case FER_Continue:
						i++;
						break;
					case FER_Stop:
						return;
					case FER_Erase:
						RFun::Release(*i);
						i = Objects.erase(i);
						break;
					default:
						i++;
						break;
					}
				}
			}
		}
	};
}

