#include "IRenderResource.h"
#include "../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

RResourceSwapChain		RResourceSwapChain::Instance;

RTTI_IMPL(EngineNS::IBlobObject, EngineNS::VIUnknown);
void IBlobObject::ReadFromXnd(XndAttribute* attr)
{
	UINT size;
	attr->Read(size);
	mDatas.InstantArray(size);
	if (size > 0)
	{
		attr->Read(&mDatas[0], size);
	}
}

void IBlobObject::Write2Xnd(XndAttribute* attr)
{
	UINT size = (UINT)mDatas.GetSize();
	attr->Write(size);
	attr->Write(&mDatas[0], size);
}

IRenderResource::IRenderResource()
{
}


IRenderResource::~IRenderResource()
{
}

void IRenderResource::DeleteThis()
{
	VIUnknown::DeleteThis();
	//VDefferedDeleteManager::GetInstance()->PushObject(this);
}

void RResourceSwapChain::Cleanup()
{
	VAutoLock(mLocker);
	while (mResources.size() > 0)
	{
		auto r = mResources.front();
		mResources.pop();
		r->Release();
	}
}

void RResourceSwapChain::PushResource(IRenderResource* res) 
{
	if (res == nullptr)
		return;
	res->AddRef();
	VAutoLock(mLocker);
	mResources.push(res);
}

void RResourceSwapChain::TickSwap(IRenderContext* rc)
{
	VAutoLock(mLocker);
	while (mResources.size() > 0)
	{
		auto r = mResources.front();
		mResources.pop();
		r->DoSwap(rc);
		r->Release();
	}
}

NS_END

