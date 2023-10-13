#include "NxCommandList.h"
#include "NxDrawcall.h"
#include "NxFrameBuffers.h"
#include "NxBuffer.h"
#include "NxGpuDevice.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	ICmdRecorder::~ICmdRecorder()
	{
		ResetGpuDraws();
	}
	
	void ICmdRecorder::PushGpuDraw(IGpuDraw* draw)
	{
		ASSERT(draw != nullptr);
		VAutoVSLLock lk(mLocker);
		mDrawcallArray.push_back(draw);
		mPrimitiveNum += draw->GetPrimitiveNum();
	}
	void ICmdRecorder::ResetGpuDraws()
	{
		VAutoVSLLock lk(mLocker);
		mDrawcallArray.clear();
		for (auto& i : mRefBuffers)
		{
			i->ReleaseCmdRefCount();
		}
		mRefBuffers.clear();
		mDirectDrawNum = 0;
		mPrimitiveNum = 0;
		//mCmdList.FromObject(nullptr);
	}
	void ICmdRecorder::FlushDraws(ICommandList* cmdlist)
	{
		//mCmdList.FromObject(cmdlist);
		//AUTO_SAMP("NxRHI.ICmdRecorder.FlushDraws");
		VAutoVSLLock lk(mLocker);
		for (auto i : mDrawcallArray)
		{
			i->Commit(cmdlist, false);
		}
	}
	ICmdRecorder* ICommandList::BeginCommand()
	{
		if (mCmdRecorder == nullptr)
		{
			mCmdRecorder = MakeWeakRef(new ICmdRecorder());
		}
		mCmdRecorder->ResetGpuDraws();
		mPrimitiveNum = 0;
		return mCmdRecorder;
	}
	void ICommandList::EndCommand()
	{
		if (mCmdRecorder == nullptr)
		{
			ASSERT(false);
			return;
		}
	}
	bool ICommandList::PushGpuDrawImpl(IGpuDraw* draw, bool bCheck)
	{
		auto pCmdRecorder = mCmdRecorder;
		if (pCmdRecorder == nullptr || IsRecording() == false)
		{
			ASSERT(bCheck == false);
			return false;
		}
		ASSERT(mCmdRecorder != nullptr);
		pCmdRecorder->PushGpuDraw(draw);
		return true;
	}
	void ICommandList::AppendDraws(ICmdRecorder* pCmdRecorder)
	{
		mCmdRecorder->mDrawcallArray.insert(mCmdRecorder->mDrawcallArray.end(), pCmdRecorder->mDrawcallArray.begin(), pCmdRecorder->mDrawcallArray.end());
	}
	void ICommandList::DirectGpuDraw(IGpuDraw* draw)
	{
		if (mCmdRecorder == nullptr)
		{
			ASSERT(false);
			return;
		}
		mCmdRecorder->mDirectDrawNum++;
		draw->Commit(this, false);
	}
	void ICommandList::InheritPass(ICommandList* cmdlist)
	{
		mCurrentFrameBuffers = cmdlist->mCurrentFrameBuffers;
	}
	void ICommandList::WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters)
	{
		if (Count == 0)
			return;
		FBufferDesc bfDesc{};
		bfDesc.SetDefault();
		bfDesc.Usage = EGpuUsage::USAGE_STAGING;
		bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;
		bfDesc.Type = EBufferType::BFT_NONE;
		bfDesc.Size = Count * sizeof(UINT);
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		auto copyBuffer = MakeWeakRef(GetGpuDevice()->CreateBuffer(&bfDesc));
		FMappedSubResource mapped{};
		if (copyBuffer->Map(0, &mapped, false))
		{
			auto ptr = (UINT*)mapped.pData;
			for (UINT i = 0; i < Count; i++)
			{
				ptr[i] = BufferWriters[i].Value;
			}
			copyBuffer->Unmap(0);
		}
		for (UINT i = 0; i < Count; i++)
		{
			CopyBufferRegion(BufferWriters[i].Buffer, BufferWriters[i].Offset, copyBuffer, i * sizeof(UINT), sizeof(UINT));
		}
	}
}

NS_END