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

	void ICmdRecorder::SortGraphicDrawWithEffect()
	{
		std::sort(mDrawcallArray.begin(), mDrawcallArray.end(), [](const AutoRef<IGpuDraw>& a, const AutoRef<IGpuDraw>& b)
			{
				ASSERT(a.UnsafeConvertTo<IGraphicDraw>() != nullptr && b.UnsafeConvertTo<IGraphicDraw>() != nullptr);
				auto lh = (const IGraphicDraw*)a.GetPtr();
				auto rh = (const IGraphicDraw*)b.GetPtr();
				if (lh->ShaderEffect.GetPtr() > rh->ShaderEffect.GetPtr())
					return true;
				else if (lh->ShaderEffect.GetPtr() < rh->ShaderEffect.GetPtr())
					return false;
				else
				{
					if (lh->Mesh.GetPtr() > rh->Mesh.GetPtr())
						return true;
					else if (lh->Mesh.GetPtr() < rh->Mesh.GetPtr())
						return false;
					else
					{
						return lh->MeshAtom > rh->MeshAtom;
					}
				}
			});
	}
	
	void ICmdRecorder::PushGpuDraw(IGpuDraw* draw)
	{
		AUTO_SAMP("NxRHI.ICmdRecorder.PushGpuDraw");
		ASSERT(draw != nullptr);
		//VAutoVSLLock lk(mLocker);
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
		//VAutoVSLLock lk(mLocker);
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
		//VAutoVSLLock lk(mLocker);
		mDrawcallArray.insert(mDrawcallArray.end(), pCmdRecorder->mDrawcallArray.begin(), pCmdRecorder->mDrawcallArray.end());
		mRefBuffers.insert(mRefBuffers.end(), pCmdRecorder->mRefBuffers.begin(), pCmdRecorder->mRefBuffers.end());
		mDirectDrawNum += pCmdRecorder->mDirectDrawNum;
		mPrimitiveNum += pCmdRecorder->mPrimitiveNum;
	}
	bool ICmdRecorder::HasPendingShaderDraws() const
	{
		for (UINT i = mFlushStart; i < (UINT)mDrawcallArray.size(); i++)
		{
			auto draw = mDrawcallArray[i].GetPtr();
			if (dynamic_cast<const IGraphicDraw*>(draw) || dynamic_cast<const IComputeDraw*>(draw) || dynamic_cast<const IRayTracingDraw*>(draw))
				return true;
		}
		return false;
	}
	void ICmdRecorder::FlushDraws(ICommandList* cmdlist)
	{
		//mCmdList.FromObject(cmdlist);
		//AUTO_SAMP("NxRHI.ICmdRecorder.FlushDraws");
		//VAutoVSLLock lk(mLocker);
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
		// Graphic draw 只能在 pass 内提交; pass 外 push 会在下个 pass 内 Build, 转换被收进 BeginBarriers 却排在其前执行
		// 需要提前收集的请改用独立 TtCmdRecorder, 在 BeginPass 之后 AppendDraws
		ASSERT(GetCurrentRenderPass() != nullptr);
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
		auto copyBuffer = MakeWeakRef(GetGpuDevice()->CreateBuffer(&bfDesc, __FILE__, __LINE__));
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
			if (BufferWriters[i].Buffer->Map(0, &mapped, false))
			{
				auto ptr = (UINT*)((BYTE*)mapped.pData + BufferWriters[i].Offset);
				ptr[0] = BufferWriters[i].Value;
				BufferWriters[i].Buffer->Unmap(0);
				continue;
			}

			AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
			cpDraw->BindBufferDest(BufferWriters[i].Buffer);
			cpDraw->BindBufferSrc(copyBuffer);
			cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
			cpDraw->FootPrint.Format = EPixelFormat::PXF_UNKNOWN;
			cpDraw->FootPrint.X = i * sizeof(UINT);
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
	EGpuResourceState FTransitionScope::TryAutoTransition(ICommandList* cmd, IGpuBufferDataBase* resource, const FSubresourceRange& range, EGpuResourceState toState, bool bTryRenderPass)
	{
		auto save = resource->LayoutTracker.GetState(range);
		if (resource->IsAutoTransition == false)
			return save;
		if (resource->LayoutTracker.NeedTransition(range, toState))
		{
			auto pass = cmd->GetCurrentRenderPass();
			if (bTryRenderPass && pass)
			{
				pass->PushBeginBarrier(resource, range, toState);
			}
			else
			{
				resource->TransitionTo(cmd, range, toState);
			}
		}
		return save;
	}
	EGpuResourceState FTransitionScope::TryAutoTransition(ICommandList* cmd, IGpuViewBase* view, bool bTryRenderPass)
	{
		if (view == nullptr)
			return EGpuResourceState::GRS_Undefine;
		auto resource = view->GetBufferDataBase();
		if (resource == nullptr)
			return EGpuResourceState::GRS_Undefine;
		auto layout = view->GetViewLayout();
		if (layout.State == EGpuResourceState::GRS_Undefine)
			return resource->LayoutTracker.GetState(layout.Range);
		return TryAutoTransition(cmd, resource, layout.Range, layout.State, bTryRenderPass);
	}
	FTransitionScope::FTransitionScope(ICommandList* cmd, IGpuBufferDataBase* resource, const FSubresourceRange& range, EGpuResourceState toState)
	{
		CmdList = cmd;
		Resource = resource;
		resource->LayoutTracker.Transition(range, toState, [&](const FSubresourceRange& seg, EGpuResourceState from) {
			SaveSegments.push_back(FSaveSegment{ seg, from });
			resource->EmitTransition(cmd, seg, from, toState);
		});
	}
	FTransitionScope::~FTransitionScope()
	{
		for (const auto& i : SaveSegments)
		{
			Resource->TransitionTo(CmdList, i.Range, i.State);
		}
	}
}

NS_END