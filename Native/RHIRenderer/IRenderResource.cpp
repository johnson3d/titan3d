#include "IRenderResource.h"
#include "../Core/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

RResourceSwapChain		RResourceSwapChain::Instance;

RTTI_IMPL(EngineNS::IBlobObject, EngineNS::VIUnknown);
void IBlobObject::ReadFromXnd(XNDAttrib* attr) 
{
	UINT size;
	attr->Read(size);
	mDatas.InstantArray(size);
	if (size > 0)
	{
		attr->Read(&mDatas[0], size);
	}
}

void IBlobObject::Write2Xnd(XNDAttrib* attr) 
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
	RHIUnknown::DeleteThis();
	//VDefferedDeleteManager::GetInstance()->PushObject(this);
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

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, IBlobObject, GetSize);
	Cpp2CS0(EngineNS, IBlobObject, GetData);
	Cpp2CS2(EngineNS, IBlobObject, PushData);
	Cpp2CS1(EngineNS, IBlobObject, ReSize);

	Cpp2CS1(EngineNS, IBlobObject, ReadFromXnd);
	Cpp2CS1(EngineNS, IBlobObject, Write2Xnd);

	VFX_API void SDK_RResourceSwapChain_TickSwap(IRenderContext* rc)
	{
		RResourceSwapChain::GetInstance()->TickSwap(rc);
	}
}