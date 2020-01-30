#include "GfxMdfQueue.h"
#include "GfxModifier.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxMdfQueue, EngineNS::VIUnknown);

GfxMdfQueue::GfxMdfQueue()
{
}

GfxMdfQueue::~GfxMdfQueue()
{
	Cleanup();
}

void GfxMdfQueue::Cleanup()
{
	ClearModifiers();
	VIUnknown::Cleanup();
}

UINT GfxMdfQueue::GetMdfNumber() const
{
	return (UINT)mModQueue.size();
}

void GfxMdfQueue::AddModifier(GfxModifier* modifier)
{
	if (modifier == nullptr)
		return;
	modifier->AddRef();
	mModQueue.push_back(modifier);
}

UINT GfxMdfQueue::FindModifier(const char* name)
{
	for (UINT i = 0; i < (UINT)mModQueue.size(); i++)
	{
		if (mModQueue[i]->mName == name)
			return i;
	}
	return -1;
}

void GfxMdfQueue::RemoveModifier(UINT index)
{
	if (index >= (UINT)mModQueue.size())
		return;
	mModQueue[index]->Release();
	mModQueue.erase(mModQueue.begin() + index);
}

void GfxMdfQueue::ClearModifiers()
{
	for (auto i : mModQueue)
	{
		i->Release();
	}
	mModQueue.clear();
}

GfxModifier* GfxMdfQueue::GetModifier(UINT index)
{
	if (index >= (UINT)mModQueue.size())
		return nullptr;
	return mModQueue[index];
}

void GfxMdfQueue::TickLogic(IRenderContext* rc, GfxMesh* mesh, vTimeTick time)
{
	for (auto i : mModQueue)
	{
		i->TickLogic(rc, mesh, time);
	}
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(UINT, EngineNS, GfxMdfQueue, GetMdfNumber);
	CSharpAPI1(EngineNS, GfxMdfQueue, AddModifier, GfxModifier*);
	CSharpReturnAPI1(UINT, EngineNS, GfxMdfQueue, FindModifier, const char*);
	CSharpAPI1(EngineNS, GfxMdfQueue, RemoveModifier, UINT);
	CSharpAPI0(EngineNS, GfxMdfQueue, ClearModifiers);
	CSharpReturnAPI1(GfxModifier*, EngineNS, GfxMdfQueue, GetModifier, UINT);
	CSharpAPI3(EngineNS, GfxMdfQueue, TickLogic, IRenderContext*, GfxMesh*, vTimeTick);
}