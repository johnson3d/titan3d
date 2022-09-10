#include "NxCommandList.h"
#include "NxDrawcall.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void ICommandList::PushGpuDraw(IGpuDraw* draw) 
	{
		mDrawcallArray.push_back(draw);
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
	}
}

NS_END