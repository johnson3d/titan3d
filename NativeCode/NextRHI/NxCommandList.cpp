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
	ICmdRecorder::ICmdRecorder(IGpuDevice* device, ECmdRecorderType types)
	{
		mDeviceRef = device;
		RecorderTypes = types;
	}
	ICmdRecorder::~ICmdRecorder()
	{
		ResetGpuDraws();
	}
	
	void ICmdRecorder::PushGpuDraw(IGpuDraw* draw)
	{
		AUTO_SAMP("NxRHI.ICmdRecorder.PushGpuDraw");
		ASSERT(draw != nullptr);
		VAutoVSLLock lk(mLocker);
		mDrawcallArray.push_back(draw);
		mPrimitiveNum += draw->GetPrimitiveNum();
	}
	void ICmdRecorder::PushGpuDraw(IGraphicDraw* draw)
	{
		ASSERT(RecorderTypes & ECmdRecorderType::CRT_Graphics);
		return PushGpuDraw((IGpuDraw*)draw);
	}
	void ICmdRecorder::PushGpuDraw(ICopyDraw* draw)
	{
		ASSERT(RecorderTypes & ECmdRecorderType::CRT_Copy);
		return PushGpuDraw((IGpuDraw*)draw);
	}
	void ICmdRecorder::PushGpuDraw(IComputeDraw* draw)
	{
		ASSERT(RecorderTypes & ECmdRecorderType::CRT_Compute);
		return PushGpuDraw((IGpuDraw*)draw);
	}
	void ICmdRecorder::PushGpuDraw(IRayTracingDraw* draw)
	{
		ASSERT(RecorderTypes & ECmdRecorderType::CRT_RayTracing);
		return PushGpuDraw((IGpuDraw*)draw);
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
		mFlushStart = 0;
		//mCmdList.FromObject(nullptr);
	}
	void ICmdRecorder::AppendRecorder(ICmdRecorder* pCmdRecorder)
	{
		VAutoVSLLock lk(mLocker);
		mDrawcallArray.insert(mDrawcallArray.end(), pCmdRecorder->mDrawcallArray.begin(), pCmdRecorder->mDrawcallArray.end());
		mRefBuffers.insert(mRefBuffers.end(), pCmdRecorder->mRefBuffers.begin(), pCmdRecorder->mRefBuffers.end());
		mDirectDrawNum += pCmdRecorder->mDirectDrawNum;
		mPrimitiveNum += pCmdRecorder->mPrimitiveNum;
	}
	void ICmdRecorder::FlushDraws(ICommandList* cmdlist)
	{
		//mCmdList.FromObject(cmdlist);
		//AUTO_SAMP("NxRHI.ICmdRecorder.FlushDraws");
		VAutoVSLLock lk(mLocker);
		{
			AUTO_SAMP("NxRHI.ICmdRecorder.FlushDraws.Build");
			for (UINT i = mFlushStart; i < (UINT)mDrawcallArray.size(); i++)
			{
				mDrawcallArray[i]->BuildDrawcall(cmdlist);
			}
		}
		{
			AUTO_SAMP("NxRHI.ICmdRecorder.FlushDraws.Commit");
			for (UINT i = mFlushStart; i < (UINT)mDrawcallArray.size(); i++)
			{
				mDrawcallArray[i]->Commit(cmdlist, false);
			}
		}
		mFlushStart = (UINT)mDrawcallArray.size();
	}
	ICmdRecorder* ICommandList::BeginCommand()
	{
		if (mCmdRecorder == nullptr)
		{
			mCmdRecorder = MakeWeakRef(new ICmdRecorder(mDevice.GetPtr(), ECmdRecorderType::CRT_All));
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
	IRenderPass* ICommandList::GetCurrentRenderPass() 
	{
		if (mCurrentFrameBuffers != nullptr)
			return mCurrentFrameBuffers->GetRenderPass();
		return nullptr;
	}
	bool ICommandList::PushGpuDrawImpl(IGpuDraw* draw, bool bCheck)
	{
		if (mIsDirectGpuDraw)
		{
			this->DirectGpuDraw(draw);
			return true;
		}
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
	bool ICommandList::PushGpuDraw(IGraphicDraw* draw)
	{
		return PushGpuDrawImpl(draw, true);
	}
	bool ICommandList::PushGpuDraw(IComputeDraw* draw)
	{
		ASSERT(mDevice.GetPtr()->GetGpuDeviceCaps()->IsSupportCSInRenderPass || GetCurrentRenderPass() == nullptr);
		return PushGpuDrawImpl(draw, true);
	}
	bool ICommandList::PushGpuDraw(IRayTracingDraw* draw)
	{
		return PushGpuDrawImpl(draw, true);
	}
	bool ICommandList::PushGpuDraw(IActionDraw* draw)
	{
		return PushGpuDrawImpl(draw, true);
	}
	bool ICommandList::PushGpuDraw(ICopyDraw* draw)
	{
		auto pass = GetCurrentRenderPass();
		if (pass)
		{
			pass->PushBeginCopyDraw(draw);
			return true;
		}
		else
		{
			return PushGpuDrawImpl(draw, true);
		}
	}
	void ICommandList::AppendDraws(ICmdRecorder* pCmdRecorder)
	{
		mCmdRecorder->AppendRecorder(pCmdRecorder);
		
	}
	void ICommandList::DirectGpuDraw(IGpuDraw* draw)
	{
		if (mCmdRecorder == nullptr)
		{
			ASSERT(false);
			return;
		}
		mCmdRecorder->mDirectDrawNum++;
		draw->BuildDrawcall(this);
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
		auto device = GetGpuDevice();
		for (UINT i = 0; i < Count; i++)
		{
			AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw());
			cpDraw->BindBufferDest(BufferWriters[i].Buffer);
			cpDraw->BindBufferSrc(copyBuffer);
			cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
			cpDraw->FootPrint.Format = EPixelFormat::PXF_UNKNOWN;
			cpDraw->FootPrint.X = 0;
			cpDraw->FootPrint.Y = 0;
			cpDraw->FootPrint.Z = 0;
			cpDraw->FootPrint.Width = sizeof(UINT);
			cpDraw->FootPrint.Height = 1;
			cpDraw->FootPrint.Depth = 1;
			cpDraw->FootPrint.RowPitch = sizeof(UINT);
			cpDraw->FootPrint.TotalSize = sizeof(UINT);
			cpDraw->DstX = BufferWriters[i].Offset;

			this->PushGpuDraw(cpDraw.GetPtr());
		}
	}
	EGpuResourceState FTransitionScope::Transition(ICommandList* cmd, IGpuBufferData* resource, EGpuResourceState toState, bool bTryRenderPass)
	{
		auto save = resource->GpuState;
		auto bNeedTransition = resource->GpuState != toState;
		if (bNeedTransition)
		{
			auto pass = cmd->GetCurrentRenderPass();
			if (bTryRenderPass && pass)
			{
				pass->PushBeginBarrier(resource, toState);
			}
			else
			{
				resource->TransitionTo(cmd, toState);
			}
		}
		return save;
	}
	FTransitionScope::FTransitionScope(ICommandList* cmd, IGpuBufferData* resource, EGpuResourceState toState)
	{
		CmdList = cmd;
		Resource = resource;
		SaveState = resource->GpuState;
		bNeedTransition = SaveState != toState;
		if (bNeedTransition)
		{
			resource->TransitionTo(cmd, toState);
		}
	}
	FTransitionScope::~FTransitionScope()
	{
		if (bNeedTransition)
		{
			Resource->TransitionTo(CmdList, SaveState);
		}
	}
}

NS_END