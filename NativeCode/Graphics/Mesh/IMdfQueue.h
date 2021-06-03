#pragma once
#include "../../RHI/RHI.h"
#include "Modifier/IModifier.h"

NS_BEGIN

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IMdfQueue : public VIUnknown
{
public:
	std::vector<AutoRef<IModifier>>		mMdfQueue;
public:
	TR_CONSTRUCTOR()
	IMdfQueue()
	{

	}
	TR_FUNCTION()
	void GetInputStreams(DWORD* pOutStreams);
	TR_FUNCTION()
	void GetProvideStreams(DWORD* pOutStreams);
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
};

NS_END