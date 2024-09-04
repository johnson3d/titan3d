#pragma once

#include "../IUnknown.h"
#include "../debug/vfxdebug.h"
#include "CsBinder.h"

NS_BEGIN

struct FCsDictionaryIterator;

struct TR_CLASS()
CsDictionaryImpl : public IWeakReference
{
	CsDictionaryImpl()
	{

	}
	struct TtAnyValue_less
	{
		constexpr bool operator()(const TtAnyValue& _Left, const TtAnyValue& _Right) const {
			return _Left < _Right;
		}
	};
	std::map<TtAnyValue, TtAnyValue, TtAnyValue_less>		mContain;
	bool Add(const TtAnyValue* key, const TtAnyValue* value)
	{
		auto iter = mContain.find(*key);
		if (iter == mContain.end())
			return false;
		mContain.insert(std::make_pair(*key, *value));
		return true;
	}
	void Remove(const TtAnyValue* key)
	{
		auto iter = mContain.find(*key);
		if (iter == mContain.end())
			return;
		mContain.erase(iter);
	}
	void Clear()
	{
		mContain.clear();
	}
	bool Find(const TtAnyValue* key, TtAnyValue* value)
	{
		auto iter = mContain.find(*key);
		if (iter == mContain.end())
			return false;

		*value = iter->second;
		return true;
	}
	UINT GetCount()
	{
		return (UINT)mContain.size();
	}

	FCsDictionaryIterator* NewIterator();
};

struct TR_CLASS(SV_Dispose = delete self)
	FCsDictionaryIterator
{
	std::map<TtAnyValue, TtAnyValue, CsDictionaryImpl::TtAnyValue_less>::iterator It;
	void MoveNext()
	{
		It++;
	}
	bool IsEnd(CsDictionaryImpl * contain)
	{
		return (It == contain->mContain.end());
	}
	void GetKeyValue(TtAnyValue * pKey, TtAnyValue * pValue)
	{
		if (pKey != nullptr)
		{
			*pKey = It->first;
		}
		if (pValue != nullptr)
		{
			*pValue = It->second;
		}
	}
	void Reset(CsDictionaryImpl* contain)
	{
		It = contain->mContain.begin();
	}
};

template<typename _KeyType, typename _ValueType>
struct CsDictionary
{
	CsDictionaryImpl			mImpl;
	bool Add(const _KeyType& key, const _ValueType& value)
	{
		TtAnyValue tKey;
		tKey.SetValue(key);
		TtAnyValue tValue;
		tValue.SetValue(value);
		mImpl.Add(&tKey, &tValue);
	}
	void Remove(const _KeyType& key)
	{
		TtAnyValue tKey;
		tKey.SetValue(key);
		mImpl.Remove(&tKey);
	}
	void Clear()
	{
		mImpl.Clear();
	}
	UINT GetCount()
	{
		return mImpl.GetCount();
	}
	bool Find(const _KeyType& key, _ValueType* value = nullptr)
	{
		TtAnyValue tKey;
		tKey.SetValue(key);

		TtAnyValue tValue;
		if (mImpl.Find(&tKey, &tValue))
		{
			if (value != nullptr)
				tValue.GetValue(*value);
			return true;
		}
		return false;
	}
};

NS_END
