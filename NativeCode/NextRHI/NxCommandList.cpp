#include "NxCommandList.h"
#include "NxDrawcall.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void ICommandList::PushGpuDraw(IGpuDraw* draw) 
	{
		mDrawcallArray.push_back(draw);
		mPrimitiveNum += draw->GetPrimitiveNum();
	}
	void ICommandList::FlushDraws()
	{
		for (auto i : mDrawcallArray)
		{
			i->Commit(this);
		}
	}
	void ICommandList::ResetGpuDraws() 
	{
		mDrawcallArray.clear();
		mPrimitiveNum = 0;
	}
}

NS_END