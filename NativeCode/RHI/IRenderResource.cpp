#include "IRenderResource.h"
#include "../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::IBlobObject);
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

void IRenderResource::Cleanup()
{

}

NS_END

