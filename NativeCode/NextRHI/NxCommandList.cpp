#include "NxCommandList.h"
#include "NxDrawcall.h"
#include "NxFrameBuffers.h"
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
		//VAutoVSLLock al(mLocker);
		for (auto i : mDrawcallArray)
		{
			i->Commit(this);
		}
	}
	void ICommandList::ResetGpuDraws() 
	{
		//VAutoVSLLock al(mLocker);
		mDrawcallArray.clear();
		mPrimitiveNum = 0;
	}
	void ICommandList::InheritPass(ICommandList* cmdlist)
	{
		mCurrentFrameBuffers = cmdlist->mCurrentFrameBuffers;
	}
}

NS_END