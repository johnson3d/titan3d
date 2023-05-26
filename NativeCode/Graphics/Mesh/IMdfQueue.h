#pragma once
#include "../../NextRHI/NxRHI.h"
#include "Modifier/IModifier.h"

NS_BEGIN

class TR_CLASS()
	IMdfQueue : public IWeakReference
{
public:
	std::vector<AutoRef<IModifier>>		mMdfQueue;
public:
	TR_CONSTRUCTOR()
	IMdfQueue()
	{

	}
	TR_FUNCTION()
	void GetInputStreams(UINT* pOutStreams);
	TR_FUNCTION()
	void GetProvideStreams(UINT* pOutStreams);
	TR_FUNCTION()
	IModifier* GetModifier(UINT index) {
		if (index >= mMdfQueue.size())
			return nullptr;
		return mMdfQueue[index];
	}
	TR_FUNCTION()
	void PushModifier(IModifier* p) {
		AutoRef<IModifier> t;
		t.StrongRef(p);
		mMdfQueue.push_back(t);
	}
	IMdfQueue* CloneMdfQueue();
	void RemoveModifier(IModifier* p) {
		for (auto i = mMdfQueue.begin(); i != mMdfQueue.end(); i++)
		{
			if (*i == p)
			{
				mMdfQueue.erase(i);
				return;
			}
		}
	}
	void ClearModifiers() {
		mMdfQueue.clear();
	}
};

NS_END