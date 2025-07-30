#include "VKCommandList.h"
#include "VKGpuDevice.h"
#include "VKShader.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "VKEvent.h"
#include "VKInputAssembly.h"
#include "VKFrameBuffers.h"
#include "VKEffect.h"
#include "../NxDrawcall.h"
#include "../../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{	
	void VKThreadCmdBufferManager::Initialize(VKGpuDevice* device)
	{
		mThreadName = vfxThread::GetCurrentThreadName();
		mDevice = device;
		VkCommandPoolCreateInfo poolInfo = {};
		poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
		poolInfo.queueFamilyIndex = ((VKCmdQueue*)device->GetCmdQueue())->mGraphicsQueueIndex;
		poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

		if (vkCreateCommandPool(device->mDevice, &poolInfo, device->GetVkAllocCallBacks(), &mCmdPool) != VK_SUCCESS)
		{
			ASSERT(false);
			return;
		}
	}
	AutoRef<VKCmdRecorder> VKThreadCmdBufferManager::Alloc(VKCommandList* cmdlist)
	{
		VAutoVSLLock lk(mLocker);
		if (CmdAllocators.size() == 0)
		{
			for (int i = 0; i < 10; i++)
			{
				AutoRef<VKCmdRecorder> tmp = MakeWeakRef(new VKCmdRecorder(mDevice, ECmdRecorderType::CRT_All));
				VkCommandBufferAllocateInfo allocInfo{};
				allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
				allocInfo.commandPool = mCmdPool;
				allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
				allocInfo.commandBufferCount = 1;

				VkCommandBuffer buffer = nullptr;
				auto hr = vkAllocateCommandBuffers(mDevice->mDevice, &allocInfo, &buffer);
				if (hr != VK_SUCCESS)
				{
					return nullptr;
				}
				tmp->mCommandBuffer = buffer;
				tmp->mManager = this;
				CmdAllocators.push(tmp);
			}
		}
		auto result = CmdAllocators.front();
		ASSERT(result->GetDrawcallNumber() == 0);
		result->mCmdlist = cmdlist;
		result->ResetGpuDraws();

		CmdAllocators.pop();
		return result;
	}
	void VKThreadCmdBufferManager::Free(const AutoRef<VKCmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence)
	{
		ASSERT(waitValue > 0);
		VAutoVSLLock lk(mLocker);
		ASSERT(allocator != nullptr);
		if (allocator->GetDrawcallNumber() == 0)
		{
			allocator->ResetGpuDraws();
			allocator->mCmdlist = nullptr;
			CmdAllocators.push(allocator);
			return;
		}
		allocator->mCmdlist = nullptr;
		FWaitRecycle tmp;
		tmp.Allocator = allocator;
		tmp.WaitValue = waitValue;
		tmp.Fence = fence;
		tmp.WaitFrameCount = 5;
		Recycles.push_back(tmp);
	}
	void VKThreadCmdBufferManager::UnsafeDirectFree(const AutoRef<VKCmdRecorder>& allocator)
	{//never used
		ASSERT(allocator != nullptr);
		VAutoVSLLock lk(mLocker);
		CmdAllocators.push(allocator);
	}
	void VKThreadCmdBufferManager::TickRecycle()
	{
		AUTO_SAMP("NxRHI.VKThreadCmdBufferManager.TickRecycle");
		VAutoVSLLock lk(mLocker);
		auto nRecycle = Recycles.size();
		for (auto i = Recycles.begin(); i != Recycles.end(); )
		{
			auto value = i->Fence->GetCompletedValue();
			auto waitValue = i->WaitValue;
			ASSERT(waitValue > 0);
			//fuck off: d12 or driver reference the resource but didn't AddRef
			if (value >= waitValue)
			{
				ASSERT(i->Allocator->GetDrawcallNumber() != 0);
				i->Allocator->ResetGpuDraws();
				CmdAllocators.push(i->Allocator);
				i = Recycles.erase(i);
			}
			else
			{
				i->WaitFrameCount--;
				if (i->WaitFrameCount == 0)
				{
					VFX_LTRACE(ELTT_Warning, "VKCmdAllocator always alive %d / %d\r\n", value, waitValue);
				}
				i++;
			}
		}
		if (CmdAllocators.size() > nRecycle * 2)
		{
			auto rmv = (CmdAllocators.size() - nRecycle * 2);
			for (size_t i = 0; i < rmv; i++)
			{
				auto cmd = CmdAllocators.front();
				cmd->FinalCleanup(this);
				CmdAllocators.pop();
			}
		}
	}
	void VKThreadCmdBufferManager::FinalCleanup()
	{
		VAutoVSLLock lk(mLocker);
		for (auto i = Recycles.begin(); i != Recycles.end(); )
		{
			auto value = i->Fence->GetCompletedValue();
			auto waitValue = i->WaitValue;
			ASSERT(waitValue > 0);
			if (value >= waitValue)
			{
				i->Allocator->FinalCleanup(this);

				CmdAllocators.push(i->Allocator);
				i = Recycles.erase(i);
			}
			else
			{
				i++;
			}
		}
		//ASSERT(Recycles.size() == 0);
		while (CmdAllocators.size() > 0)
		{
			auto recorder = CmdAllocators.front();
			CmdAllocators.pop();
			recorder->FinalCleanup(this);
		}
		if (Recycles.size() == 0)
		{
			vkDestroyCommandPool(mDevice->mDevice, mCmdPool, mDevice->GetVkAllocCallBacks());
			mCmdPool = nullptr;
		}
		//mDevice = nullptr;
	}

	thread_local VKThreadCmdBufferManager* VKCmdBufferManager::mThreadManager = nullptr;

	AutoRef<VKCmdRecorder> VKCmdBufferManager::Alloc(VKCommandList* cmdlist)
	{
		if (mThreadManager == nullptr)
		{
			mThreadManager = new VKThreadCmdBufferManager();
			mThreadManager->Initialize(mDevice);
			mThreadManager->mThreadStaticAddr = &mThreadManager;
			mAllManagers.push_back(mThreadManager);
		}
		return mThreadManager->Alloc(cmdlist);
	}
	void VKCmdBufferManager::Free(const AutoRef<VKCmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence)
	{
		mThreadManager->Free(allocator, waitValue, fence);
	}
	void VKCmdBufferManager::TickRecycle()
	{
		for (auto i : mAllManagers)
		{
			i->TickRecycle();
		}
	}
	bool VKCmdBufferManager::FinalCleanup()
	{
		int remain = 0;
		for (auto i : mAllManagers)
		{
			i->FinalCleanup();
			remain += (int)i->Recycles.size();
		}
		if (remain == 0)
		{
			for (auto i : mAllManagers)
			{
				//(*i->mThreadStaticAddr) = nullptr;
				delete i;
			}
			mAllManagers.clear();
		}
		return remain == 0;
	}

	void VKCmdRecorder::ResetGpuDraws()
	{
		//When main thread call TickRecycle(),[VkCommandPool mCmdPool] maybe used by other thread, so we can't reset command buffer here.
		// VKCommandList::BeginCommand() will reset command buffer.And every thing is fine
		//vkResetCommandBuffer(mCommandBuffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
		ICmdRecorder::ResetGpuDraws();
		mCmdlist = nullptr;
	}
	void VKCmdRecorder::FinalCleanup(VKThreadCmdBufferManager* manager)
	{
		ResetGpuDraws();
		if (mCommandBuffer)
		{
			vkResetCommandBuffer(mCommandBuffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
			vkFreeCommandBuffers(manager->mDevice->mDevice, manager->mCmdPool, 1, &mCommandBuffer);
			mCommandBuffer = nullptr;
		}
		mCmdlist = nullptr;
		mManager = nullptr;
	}

	VKCommandList::VKCommandList()
	{
		
	}
	VKCommandList::~VKCommandList()
	{
		auto device = GetVKDevice();
		if (device == nullptr)
		{
			return;
		}

		mCmdRecorder = nullptr;
	}
	bool VKCommandList::Init(VKGpuDevice* device)
	{
		mDevice.FromObject(device);
		
		FFenceDesc desc;
		desc.InitValue = 0;
		mCommitFence = MakeWeakRef(device->CreateFence(&desc, "VKCmdlist Commit fence"));
		
		//mCommandBuffer = device->mCmdAllocatorManager->Alloc(device);

		return true;
	}

	ICmdRecorder* VKCommandList::BeginCommand()
	{
		if (mCmdListState != ECmdListState::None)
		{
			ASSERT(false);
			SetMemoryBarrier(EPipelineStage::PPLS_ALL_COMMANDS, EPipelineStage::PPLS_ALL_COMMANDS, EBarrierAccess::BAS_MemoryWrite, (EBarrierAccess)(EBarrierAccess::BAS_MemoryRead | EBarrierAccess::BAS_MemoryWrite));
			vkEndCommandBuffer(mCmdRecorder.UnsafeConvertTo<VKCmdRecorder>()->mCommandBuffer);
		}

		ASSERT(mCmdRecorder == nullptr);
		if (mCmdRecorder == nullptr)
		{
			mCmdRecorder = GetVKDevice()->mCmdAllocatorManager->Alloc(this);
		}
		else
		{
			ASSERT(false);
			mCmdRecorder->ResetGpuDraws();
		}

		VkCommandBufferBeginInfo beginInfo{};
		beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		beginInfo.flags = VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT;
		vkResetCommandBuffer(GetVKCmdRecorder()->mCommandBuffer, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
		vkBeginCommandBuffer(GetVKCmdRecorder()->mCommandBuffer, &beginInfo);
		
		mCmdListState = ECmdListState::Recording;
		GetVKCmdRecorder()->mIsRecording = true;
		//this->BeginEvent(mDebugName.c_str());

		return mCmdRecorder;
	}
	
	void VKCommandList::EndCommand()
	{
		//this->EndEvent();
		ICommandList::EndCommand();
		if (mCmdListState == ECmdListState::Recording)
		{
			SetMemoryBarrier(EPipelineStage::PPLS_ALL_COMMANDS, EPipelineStage::PPLS_ALL_COMMANDS, EBarrierAccess::BAS_MemoryWrite, (EBarrierAccess)(EBarrierAccess::BAS_MemoryRead | EBarrierAccess::BAS_MemoryWrite));
			vkEndCommandBuffer(mCmdRecorder.UnsafeConvertTo<VKCmdRecorder>()->mCommandBuffer);
		}
		else
		{
			ASSERT(false);
		}
		mCmdListState = ECmdListState::ExecuteWaiting;
		GetVKCmdRecorder()->mIsRecording = false;
	}

	void VKCommandList::Commit(VKCmdQueue* cmdQueue, EQueueType type)
	{
		ASSERT(mCmdListState == ECmdListState::ExecuteWaiting);
		if (GetVKCmdRecorder() == nullptr)
			return;
		ASSERT(GetVKCmdRecorder()->mIsRecording == false);
		auto device = mDevice.GetCastPtr<VKGpuDevice>();
		//device->EnableImmExecute = true;
		if (device->EnableImmExecute)
		{
			cmdQueue->Flush(type);//copy to
		}

		auto vkFence = mCommitFence.UnsafeConvertTo<VKFence>();
		if (GetCmdRecorder()->GetDrawcallNumber() > 0)
		{
			VkSubmitInfo submitInfo{};
			submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
			submitInfo.commandBufferCount = 1;
			submitInfo.pCommandBuffers = &GetVKCmdRecorder()->mCommandBuffer;
			submitInfo.waitSemaphoreCount = 0;
			submitInfo.pWaitSemaphores = nullptr;
			VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;/*VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT |
				VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT |
				VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;*/
			submitInfo.pWaitDstStageMask = &waitStage;
			submitInfo.signalSemaphoreCount = 1;
			VkSemaphore signalSemas[1]{};
			signalSemas[0] = vkFence->mSemaphore;
			submitInfo.pSignalSemaphores = signalSemas;

			VkTimelineSemaphoreSubmitInfo timelineInfo{};
			submitInfo.pNext = &timelineInfo;
			timelineInfo.sType = VK_STRUCTURE_TYPE_TIMELINE_SEMAPHORE_SUBMIT_INFO;

			timelineInfo.waitSemaphoreValueCount = 0;
			timelineInfo.pWaitSemaphoreValues = nullptr;

			UINT64 signalValues[1]{};
			signalValues[0] = ++vkFence->ExpectValue;
			timelineInfo.signalSemaphoreValueCount = 1;
			timelineInfo.pSignalSemaphoreValues = signalValues;

			cmdQueue->SafeQueueSubmit(1, &submitInfo, nullptr, EQueueType::QU_Default);
		}
		else
		{
			//PIXBeginEvent(cmdQueue->mCmdQueue.GetPtr(), 0, mDebugNameW.c_str());
			//cmdQueue->mCmdQueue->ExecuteCommandLists(1, (ID3D12CommandList**)&mContext);
			//PIXEndEvent(cmdQueue->mCmdQueue.GetPtr());
		}
		mCmdListState = ECmdListState::None;

		//EndEvent();
		if (device->EnableImmExecute)
		{
			cmdQueue->Flush(type);
			//device->mDeviceRemovedCallback = nullptr;
		}

		auto targetValue = cmdQueue->IncreaseSignal(mCommitFence, type);
		GetVKCmdRecorder()->Free(targetValue, mCommitFence);

		mCmdRecorder = nullptr;
	}
	
	bool VKCommandList::BeginRendering(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name)
	{
		mCurrentFrameBuffers = fb;

		AutoRef<VKCmdBeginRenderingDraw>	mBeginRenderingDraw;
		if (mBeginRenderingDraw == nullptr)
		{
			mBeginRenderingDraw = MakeWeakRef(new VKCmdBeginRenderingDraw());
		}

		GetCmdRecorder()->UseResource(fb);
		GetCmdRecorder()->UseResource(((VKFrameBuffers*)fb)->mFrameBuffer);

		mCurRtvs.clear();
		mCurRtvs.resize(fb->mRenderPass->Desc.NumOfMRT);
		
		mBeginRenderingDraw->mColorAttachments.clear();
		auto pass = GetCurrentRenderPass();
		for (UINT i = 0; i < fb->mRenderPass->Desc.NumOfMRT; i++)
		{
			UINT flags = ((UINT)passClears->ClearFlags) & (1 << (i + 2));
			VkRenderingAttachmentInfo colorAttachment = {};
			colorAttachment.sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO;
			auto rtv = fb->mRenderTargets[i].UnsafeConvertTo<VKRenderTargetView>();
			mCurRtvs[i] = std::make_pair(rtv->GpuResource->GpuState, rtv);
			if (rtv != nullptr)
			{
				FTransitionScope::Transition(this, rtv->GpuResource, EGpuResourceState::GRS_RenderTarget, false);
				colorAttachment.imageView = rtv->mView->mImageView;
			}
			else
			{
				colorAttachment.imageView = nullptr;
			}
			colorAttachment.imageLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
			if (flags == 0)
			{
				colorAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			}
			else
			{
				colorAttachment.loadOp = FrameBufferLoadAction2VK(fb->mRenderPass->Desc.AttachmentMRTs[i].LoadAction);
			}
			
			colorAttachment.storeOp = FrameBufferStoreAction2VK(fb->mRenderPass->Desc.AttachmentMRTs[i].StoreAction);
			memcpy(colorAttachment.clearValue.color.float32, &passClears->ClearColor[i], sizeof(float)*4);

			mBeginRenderingDraw->mColorAttachments.push_back(colorAttachment);
		}
		
		VkRenderingAttachmentInfo& depthAttachment = mBeginRenderingDraw->mDepthAttachment;
		VkRenderingAttachmentInfo& stencilAttachment = mBeginRenderingDraw->mStencilAttachment;
		depthAttachment.sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO;
		stencilAttachment.sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO;
		if (fb->mDepthStencilView != nullptr)
		{
			auto dsv = fb->mDepthStencilView.UnsafeConvertTo<VKDepthStencilView>();
			if (dsv != nullptr)
			{
				FTransitionScope::Transition(this, dsv->GpuResource, EGpuResourceState::GRS_DepthStencil, false);
				pass->PushBeginBarrier(dsv->GpuResource, EGpuResourceState::GRS_DepthStencil);
				depthAttachment.imageView = dsv->mView->mImageView;
			}
			depthAttachment.imageLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
			if (passClears->ClearFlags & (ERenderPassClearFlags::CLEAR_DEPTH))
			{
				depthAttachment.loadOp = FrameBufferLoadAction2VK(fb->mRenderPass->Desc.AttachmentDepthStencil.LoadAction);
			}
			else
			{
				depthAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			}
			depthAttachment.storeOp = FrameBufferStoreAction2VK(fb->mRenderPass->Desc.AttachmentDepthStencil.StoreAction);

			stencilAttachment.imageLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
			if (passClears->ClearFlags & (ERenderPassClearFlags::CLEAR_STENCIL))
			{
				stencilAttachment.loadOp = FrameBufferLoadAction2VK(fb->mRenderPass->Desc.AttachmentDepthStencil.LoadAction);
			}
			else
			{
				stencilAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			}
			stencilAttachment.storeOp = FrameBufferStoreAction2VK(fb->mRenderPass->Desc.AttachmentDepthStencil.StoreAction);
		}
		else
		{
			depthAttachment.imageView = nullptr;
		}
		VkRenderingInfo& renderingInfo = mBeginRenderingDraw->mRenderingInfo;
		renderingInfo.sType = VK_STRUCTURE_TYPE_RENDERING_INFO;
		UINT width = 0;
		UINT height = 0;
		if (fb->mRenderPass->Desc.NumOfMRT > 0)
		{
			width = fb->mRenderTargets[0]->Desc.Width;
			height = fb->mRenderTargets[0]->Desc.Height;
		}
		else if (fb->mDepthStencilView != nullptr)
		{
			width = fb->mDepthStencilView->Desc.Width;
			height = fb->mDepthStencilView->Desc.Height;
		}
		else
		{
			ASSERT(false);
		}
		renderingInfo.renderArea = { {0, 0}, {width, height} };
		renderingInfo.layerCount = 1;
		renderingInfo.colorAttachmentCount = (UINT)mBeginRenderingDraw->mColorAttachments.size();
		renderingInfo.pColorAttachments = mBeginRenderingDraw->mColorAttachments.data();
		renderingInfo.pDepthAttachment = &depthAttachment;
		renderingInfo.pStencilAttachment = &stencilAttachment;

		this->PushGpuDrawImpl(pass->BeginCopyDraws);
		this->PushGpuDrawImpl(pass->BeginBarriers);
		this->PushGpuDrawImpl(mBeginRenderingDraw);
		//vkCmdBeginRendering(GetVKCmdRecorder()->mCommandBuffer, &renderingInfo);

		return true;
	}
	void VKCommandList::VKCmdBeginRenderingDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		vkCmdBeginRendering(((VKCommandList*)cmdlist)->GetVKCmdRecorder()->mCommandBuffer, &mRenderingInfo);
	}
	void VKCommandList::EndRendering()
	{
		vkCmdEndRendering(GetVKCmdRecorder()->mCommandBuffer);
		
		auto pass = GetCurrentRenderPass();

		mCurrentFrameBuffers = nullptr;
	}
	void VKCommandList::VKCmdBeginRenderPassDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		vkCmdBeginRenderPass(((VKCommandList*)cmdlist)->GetVKCmdRecorder()->mCommandBuffer, &mRenderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
	}
	bool VKCommandList::BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name)
	{
		if (GetVKDevice()->mVulkanExt.IsDynamicRendering)
		{
			this->BeginEvent(name);
			return BeginRendering(fb, passClears, name);
		}
		else
		{
			ASSERT(mCmdListState == ECmdListState::Recording);
			this->BeginEvent(name);
			mDebugName = name;
			mCurRtvs.clear();
			mCurRtvs.resize(fb->mRenderPass->Desc.NumOfMRT);
			
			mCurrentFrameBuffers = fb;
			auto pass = GetCurrentRenderPass();
			for (UINT i = 0; i < fb->mRenderPass->Desc.NumOfMRT; i++)
			{
				auto rtv = fb->mRenderTargets[i].UnsafeConvertTo<VKRenderTargetView>();
				mCurRtvs[i] = std::make_pair(rtv->GpuResource->GpuState, rtv);
				if (rtv != nullptr)
				{
					FTransitionScope::Transition(this, rtv->GpuResource, EGpuResourceState::GRS_RenderTarget, false);
				}
			}
			if (fb->mDepthStencilView != nullptr)
			{
				auto dsv = fb->mDepthStencilView.UnsafeConvertTo<VKDepthStencilView>();
				if (dsv != nullptr)
				{
					FTransitionScope::Transition(this, dsv->GpuResource, EGpuResourceState::GRS_DepthStencil, false);
				}
			}
			
			GetCmdRecorder()->UseResource(fb);
			GetCmdRecorder()->UseResource(((VKFrameBuffers*)fb)->mFrameBuffer);

			AutoRef<VKCmdBeginRenderPassDraw>	mBeginRenderingDraw;
			if (mBeginRenderingDraw == nullptr)
			{
				mBeginRenderingDraw = MakeWeakRef(new VKCmdBeginRenderPassDraw());
			}

			auto pVKFrameBuffers = ((VKFrameBuffers*)fb);
			VkRenderPassBeginInfo& renderPassInfo = mBeginRenderingDraw->mRenderPassInfo;
			renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
			auto pRenderPass = fb->mRenderPass.UnsafeConvertTo<VKRenderPass>();
			renderPassInfo.renderPass = pRenderPass->mRenderPass;
			renderPassInfo.framebuffer = pVKFrameBuffers->mFrameBuffer->mFrameBuffer;
			if (renderPassInfo.framebuffer == nullptr)
				return false;

			//UINT width = pVKFrameBuffers->mRenderTargets[0]->Desc.Width;
			//UINT height = pVKFrameBuffers->mRenderTargets[0]->Desc.Height;
			UINT width = 0;
			UINT height = 0;
			if (fb->mRenderPass->Desc.NumOfMRT > 0)
			{
				width = pVKFrameBuffers->mRenderTargets[0]->Desc.Width;
				height = pVKFrameBuffers->mRenderTargets[0]->Desc.Height;
			}
			else if (fb->mDepthStencilView != nullptr)
			{
				width = fb->mDepthStencilView->Desc.Width;
				height = fb->mDepthStencilView->Desc.Height;
			}
			else
			{
				ASSERT(false);
			}
			renderPassInfo.renderArea.offset = { 0, 0 };
			renderPassInfo.renderArea.extent.width = width;
			renderPassInfo.renderArea.extent.height = height;

			VkClearValue clearValues[9]{};
			int NumOfClear = pRenderPass->Desc.NumOfMRT;
			if (passClears != nullptr)
			{
				for (int i = 0; i < NumOfClear; i++)
				{
					memcpy(&clearValues[i].color, &passClears->ClearColor[i], sizeof(v3dxColor4));
				}
				if (pRenderPass->Desc.AttachmentDepthStencil.Format != PXF_UNKNOWN)
				{
					clearValues[NumOfClear].depthStencil = { passClears->DepthClearValue, passClears->StencilClearValue };
					NumOfClear++;
				}
			}

			renderPassInfo.clearValueCount = NumOfClear;// pRenderPass->mDesc.NumOfMRT;
			renderPassInfo.pClearValues = clearValues;

			//BeginEvent(debugName);
			this->PushGpuDrawImpl(pass->BeginCopyDraws);
			this->PushGpuDrawImpl(pass->BeginBarriers);
			this->PushGpuDrawImpl(mBeginRenderingDraw);
			//vkCmdBeginRenderPass(GetVKCmdRecorder()->mCommandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
			//vkCmdBeginRendering()

			return true;
		}
	}
	void VKCommandList::EndPass()
	{
		if (GetVKDevice()->mVulkanExt.IsDynamicRendering)
		{
			EndRendering();
			this->EndEvent();
		}
		else
		{
			ASSERT(mCurrentFrameBuffers != nullptr);
			ASSERT(mCmdListState == ECmdListState::Recording);

			vkCmdEndRenderPass(GetVKCmdRecorder()->mCommandBuffer);

			auto pass = GetCurrentRenderPass();
			mCurrentFrameBuffers = nullptr;
			
			this->EndEvent();
		}
	}
	void VKCommandList::SetViewport(UINT Num, const FViewPort* pViewports)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		//mContext->RSSetViewports(Num, (const D3D12_VIEWPORT*)pViewports);
		mCurrentViewports.resize(Num);
		for (UINT i = 0; i < Num; i++)
		{
			auto& mVkViewport = mCurrentViewports[i];

			mVkViewport.x = pViewports->TopLeftX;
			//mVkViewport.y = vp->TopLeftY;
			mVkViewport.y = pViewports->TopLeftY + pViewports->Height;
			mVkViewport.width = pViewports->Width;
			//mVkViewport.height = vp->Height;
			mVkViewport.height = -pViewports->Height;
			mVkViewport.minDepth = pViewports->MinDepth;
			mVkViewport.maxDepth = pViewports->MaxDepth;
		}
		vkCmdSetViewport(GetVKCmdRecorder()->mCommandBuffer, 0, Num, (const VkViewport*)pViewports);
	}
	void VKCommandList::UseCurrentViewports()
	{
		if (mCurrentViewports.size() == 0)
			return;
		vkCmdSetViewport(GetVKCmdRecorder()->mCommandBuffer, 0, (UINT)mCurrentViewports.size(), (const VkViewport*)&mCurrentViewports[0]);
	}
	void VKCommandList::SetScissor(UINT Num, const FScissorRect* pScissor)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		//mContext->RSSetScissorRects(Num, (const D3D12_RECT*)pScissor);
		ASSERT(Num < 32);
		mCurrentScissorRects.resize(Num);
		if (Num == 0)
		{
			vkCmdSetScissor(GetVKCmdRecorder()->mCommandBuffer, 0, 0, nullptr);
		}
		else
		{
			for (UINT i = 0; i < Num; i++)
			{
				mCurrentScissorRects[i].offset.x = pScissor[i].MinX;
				mCurrentScissorRects[i].offset.y = pScissor[i].MinY;

				mCurrentScissorRects[i].extent.width = (UINT)(pScissor[i].MaxX - pScissor[i].MinX);
				mCurrentScissorRects[i].extent.height = (UINT)(pScissor[i].MaxY - pScissor[i].MinY);
			}
			if (pScissor == nullptr)
			{
				if (mCurrentViewports.size() > 0)
				{
					mCurrentScissorRects.resize(1);
					mCurrentScissorRects[0].offset.x = (int)mCurrentViewports[0].x;
					mCurrentScissorRects[0].offset.y = (int)(mCurrentViewports[0].y + mCurrentViewports[0].height);

					mCurrentScissorRects[0].extent.width = (UINT)mCurrentViewports[0].width;
					mCurrentScissorRects[0].extent.height = (UINT)(-mCurrentViewports[0].height);
					Num = 1;
				}
				else
				{
					return;
				}
			}
			vkCmdSetScissor(GetVKCmdRecorder()->mCommandBuffer, 0, Num, &mCurrentScissorRects[0]);
		}
	}
	void VKCommandList::UseCurrentScissors()
	{
		if (mCurrentScissorRects.size() == 0)
			return;
		vkCmdSetScissor(GetVKCmdRecorder()->mCommandBuffer, 0, (UINT)mCurrentScissorRects.size(), &mCurrentScissorRects[0]);
	}
	
	void VKCommandList::BeginEvent(const char* info)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		GetCmdRecorder()->mDirectDrawNum++;
		if (VKGpuSystem::vkCmdBeginDebugUtilsLabelEXT != nullptr)
		{
			VkDebugUtilsLabelEXT markerInfo{};
			markerInfo.sType = VK_STRUCTURE_TYPE_DEBUG_UTILS_LABEL_EXT;
			markerInfo.pLabelName = info;
			VKGpuSystem::vkCmdBeginDebugUtilsLabelEXT(GetVKCmdRecorder()->mCommandBuffer, &markerInfo);
		}
		else if (VKGpuSystem::vkCmdDebugMarkerBeginEXT != nullptr)
		{
			VkDebugMarkerMarkerInfoEXT markerInfo{};
			markerInfo.sType = VK_STRUCTURE_TYPE_DEBUG_MARKER_MARKER_INFO_EXT;
			markerInfo.pMarkerName = info;
			VKGpuSystem::vkCmdDebugMarkerBeginEXT(GetVKCmdRecorder()->mCommandBuffer, &markerInfo);
		}
	}
	void VKCommandList::EndEvent()
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		GetCmdRecorder()->mDirectDrawNum++;
		if (VKGpuSystem::vkCmdEndDebugUtilsLabelEXT != nullptr)
		{
			VKGpuSystem::vkCmdEndDebugUtilsLabelEXT(GetVKCmdRecorder()->mCommandBuffer);
		}
		else if (VKGpuSystem::vkCmdDebugMarkerEndEXT != nullptr)
		{
			VKGpuSystem::vkCmdDebugMarkerEndEXT(GetVKCmdRecorder()->mCommandBuffer);
		}
	}
	void VKCommandList::SetShader(IShader* shader)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		/*switch (shader->Desc->Type)
		{
			case EShaderType::SDT_ComputeShader:
			{
				auto d11CSShader = (VKShader*)shader;
				mContext->CSSetShader(d11CSShader->mComputeShader, nullptr, 0);
			}
			break;
			default:
				break;
		}*/
	}
	void VKCommandList::SetCBV(EShaderType type, const FShaderBinder* binder, ICbView* buffer)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
	}
	void VKCommandList::SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		if (view == nullptr)
			return;
		view->GetResourceState()->SetAccessFrame(IWeakRefObject::EngineCurrentFrame);

		FTransitionScope::Transition(this, view->Buffer, EGpuResourceState::GRS_GenericRead, true);
	}
	void VKCommandList::SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		if (view == nullptr)
			return;
		FTransitionScope::Transition(this, view->Buffer, EGpuResourceState::GRS_Uav, true);
	}
	void VKCommandList::SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
	}
	void VKCommandList::SetVertexBuffer(UINT slot, IVbView* buffer, UINT Offset, UINT Stride)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		VkDeviceSize vkOffset = Offset;

		if (buffer == nullptr)
		{
			auto device = (VKGpuDevice*)mDevice.GetNakedPtr();
			vkCmdBindVertexBuffers(GetVKCmdRecorder()->mCommandBuffer, slot, 1, &device->mNullVB->mBuffer, &vkOffset);
			return;
		}
		vkCmdBindVertexBuffers(GetVKCmdRecorder()->mCommandBuffer, slot, 1, &buffer->Buffer.UnsafeConvertTo<VKBuffer>()->mBuffer, &vkOffset);
	}
	void VKCommandList::SetIndexBuffer(IIbView* buffer, bool IsBit32)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		VkIndexType type;
		if (IsBit32)
		{
			type = VkIndexType::VK_INDEX_TYPE_UINT32;
		}
		else
		{
			type = VkIndexType::VK_INDEX_TYPE_UINT16;
		}

		vkCmdBindIndexBuffer(GetVKCmdRecorder()->mCommandBuffer, buffer->Buffer.UnsafeConvertTo<VKBuffer>()->mBuffer, 0, type);
	}
	void VKCommandList::SetGraphicsPipeline(const IGpuDrawState* drawState)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		vkCmdBindPipeline(GetVKCmdRecorder()->mCommandBuffer, VkPipelineBindPoint::VK_PIPELINE_BIND_POINT_GRAPHICS, ((VKGpuDrawState*)drawState)->mGraphicsPipeline);
	}
	void VKCommandList::SetComputePipeline(const IComputeEffect* drawState)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		vkCmdBindPipeline(GetVKCmdRecorder()->mCommandBuffer, VkPipelineBindPoint::VK_PIPELINE_BIND_POINT_COMPUTE, ((VKComputeEffect*)drawState)->mComputePipeline);
	}
	void VKCommandList::SetInputLayout(IInputLayout* layout)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		//mContext->IASetInputLayout(((VKInputLayout*)layout)->mLayout);
	}
	void VKCommandList::SetViewInstanceMask(UINT Mask)
	{
		ASSERT(false);
	}
	inline VkPrimitiveTopology PrimitiveTopology2VK(EPrimitiveType type, UINT NumPrimitives, UINT& indexCount)
	{
		switch (type)
		{
		case EPrimitiveType::EPT_PointList:
			indexCount = NumPrimitives * 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
		case EPrimitiveType::EPT_LineList:
			indexCount = NumPrimitives + 1;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
		case EPrimitiveType::EPT_LineStrip:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
		case EPrimitiveType::EPT_TriangleList:
			indexCount = NumPrimitives * 3;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
		case EPrimitiveType::EPT_TriangleStrip:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
		case EPrimitiveType::EPT_TriangleFan:
			indexCount = NumPrimitives + 2;
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN;
		default:
			ASSERT(false);
			return VkPrimitiveTopology::VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
		}
	}
	void VKCommandList::Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		
		UINT dpCount = 0;
		PrimitiveTopology2VK(topology, DrawCount, dpCount);

		vkCmdDraw(GetVKCmdRecorder()->mCommandBuffer, dpCount, Instance, BaseVertex, 0);
	}
	void VKCommandList::IndirectDraw(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset, IBuffer* countBuffer)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);

		const auto argOffset = offsetof(FIndirectDrawArgument, VertexCountPerInstance);

		if (countBuffer == nullptr)
			vkCmdDrawIndirect(GetVKCmdRecorder()->mCommandBuffer, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset + argOffset, 1, sizeof(UINT) * 4);
		else
			vkCmdDrawIndirectCount(GetVKCmdRecorder()->mCommandBuffer, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset + argOffset,
				((VKBuffer*)countBuffer)->mBuffer, 0, 1024, sizeof(UINT) * 4);
	}
	void VKCommandList::DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		
		UINT dpCount = 0;
		PrimitiveTopology2VK(topology, DrawCount, dpCount);

		vkCmdDrawIndexed(GetVKCmdRecorder()->mCommandBuffer, dpCount, Instance, StartIndex, BaseVertex, 0);
	}
	void VKCommandList::IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset, IBuffer* countBuffer)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		
		const auto argOffset = offsetof(FIndirectDrawIndexArgument, VertexCountPerInstance);
		
		if (countBuffer == nullptr)
			vkCmdDrawIndexedIndirect(GetVKCmdRecorder()->mCommandBuffer, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset + argOffset, 1, sizeof(UINT) * 5);
		else
			vkCmdDrawIndexedIndirectCount(GetVKCmdRecorder()->mCommandBuffer, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset + argOffset,
				((VKBuffer*)countBuffer)->mBuffer, 0, 1024, sizeof(UINT) * 5);
	}
	void VKCommandList::Dispatch(UINT x, UINT y, UINT z)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		vkCmdDispatch(GetVKCmdRecorder()->mCommandBuffer, x, y, z);
	}
	void VKCommandList::IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset)
	{
		ASSERT(mCmdListState == ECmdListState::Recording);
		
		const auto argOffset = offsetof(FIndirectDispatchArgument, X);

		vkCmdDispatchIndirect(GetVKCmdRecorder()->mCommandBuffer, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset + argOffset);
	}
	void VKCommandList::DispatchMesh(UINT x, UINT y, UINT z)
	{
		ASSERT(false);		
	}
	void VKCommandList::IndirectDispatchMesh(IBuffer* indirectArg, UINT indirectArgOffset)
	{
		ASSERT(false);
	}
	VkAccessFlags BarrierAccessToVK(EBarrierAccess flags)
	{
		VkAccessFlags result = VkAccessFlagBits::VK_ACCESS_NONE;
		if (flags & EBarrierAccess::BAS_IndirectRead)
			result |= VkAccessFlagBits::VK_ACCESS_INDIRECT_COMMAND_READ_BIT;
		if (flags & EBarrierAccess::BAS_IndexRead)
			result |= VkAccessFlagBits::VK_ACCESS_INDEX_READ_BIT;
		if (flags & EBarrierAccess::BAS_VertexRead)
			result |= VkAccessFlagBits::VK_ACCESS_VERTEX_ATTRIBUTE_READ_BIT;
		if (flags & EBarrierAccess::BAS_CBufferRead)
			result |= VkAccessFlagBits::VK_ACCESS_UNIFORM_READ_BIT;
		if (flags & EBarrierAccess::BAS_InputStreamRead)
			result |= VkAccessFlagBits::VK_ACCESS_INPUT_ATTACHMENT_READ_BIT;
		if (flags & EBarrierAccess::BAS_ShaderRead)
			result |= VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
		if (flags & EBarrierAccess::BAS_ShaderWrite)
			result |= VkAccessFlagBits::VK_ACCESS_SHADER_WRITE_BIT;
		if (flags & EBarrierAccess::BAS_RenderTargetRead)
			result |= VkAccessFlagBits::VK_ACCESS_COLOR_ATTACHMENT_READ_BIT;
		if (flags & EBarrierAccess::BAS_RenderTargetWrite)
			result |= VkAccessFlagBits::VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		if (flags & EBarrierAccess::BAS_DepthStencilRead)
			result |= VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
		if (flags & EBarrierAccess::BAS_DepthStencilWrite)
			result |= VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
		if (flags & EBarrierAccess::BAS_CopyRead)
			result |= VkAccessFlagBits::VK_ACCESS_TRANSFER_READ_BIT;
		if (flags & EBarrierAccess::BAS_CopyWrite)
			result |= VkAccessFlagBits::VK_ACCESS_TRANSFER_WRITE_BIT;
		if (flags & EBarrierAccess::BAS_CpuRead)
			result |= VkAccessFlagBits::VK_ACCESS_HOST_READ_BIT;
		if (flags & EBarrierAccess::BAS_CpuWrite)
			result |= VkAccessFlagBits::VK_ACCESS_HOST_WRITE_BIT;
		if (flags & EBarrierAccess::BAS_MemoryRead)
			result |= VkAccessFlagBits::VK_ACCESS_MEMORY_READ_BIT;
		if (flags & EBarrierAccess::BAS_MemoryWrite)
			result |= VkAccessFlagBits::VK_ACCESS_MEMORY_WRITE_BIT;
		return result;
	}
	VkPipelineStageFlags PipelineStateToVK(EPipelineStage stage)
	{
		VkPipelineStageFlags result = VkPipelineStageFlagBits::VK_PIPELINE_STAGE_NONE;
		if (stage & EPipelineStage::PPLS_TOP_OF_PIPE)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		if (stage & EPipelineStage::PPLS_DRAW_INDIRECT)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_DRAW_INDIRECT_BIT;
		if (stage & EPipelineStage::PPLS_VERTEX_INPUT)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_VERTEX_INPUT_BIT;
		if (stage & EPipelineStage::PPLS_VERTEX_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_VERTEX_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_TESSELLATION_CONTROL_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TESSELLATION_CONTROL_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_TESSELLATION_EVALUATION_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TESSELLATION_EVALUATION_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_GEOMETRY_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_GEOMETRY_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_FRAGMENT_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_EARLY_FRAGMENT_TESTS)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
		if (stage & EPipelineStage::PPLS_LATE_FRAGMENT_TESTS)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT;
		if (stage & EPipelineStage::PPLS_COLOR_ATTACHMENT_OUTPUT)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
		if (stage & EPipelineStage::PPLS_COMPUTE_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
		if (stage & EPipelineStage::PPLS_TRANSFER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TRANSFER_BIT;
		if (stage & EPipelineStage::PPLS_BOTTOM_OF_PIPE)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;
		if (stage & EPipelineStage::PPLS_HOST)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_HOST_BIT;
		if (stage & EPipelineStage::PPLS_ALL_GRAPHICS)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_ALL_GRAPHICS_BIT;
		if (stage & EPipelineStage::PPLS_ALL_COMMANDS)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;
		if (stage & EPipelineStage::PPLS_TRANSFORM_FEEDBACK)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TRANSFORM_FEEDBACK_BIT_EXT;
		if (stage & EPipelineStage::PPLS_CONDITIONAL_RENDERING)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_CONDITIONAL_RENDERING_BIT_EXT;
		if (stage & EPipelineStage::PPLS_ACCELERATION_STRUCTURE_BUILD)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR;
		if (stage & EPipelineStage::PPLS_RAY_TRACING_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_RAY_TRACING_SHADER_BIT_KHR;
		if (stage & EPipelineStage::PPLS_TASK_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_TASK_SHADER_BIT_NV;
		if (stage & EPipelineStage::PPLS_MESH_SHADER)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_MESH_SHADER_BIT_NV;
		if (stage & EPipelineStage::PPLS_FRAGMENT_DENSITY_PROCESS)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_DENSITY_PROCESS_BIT_EXT;
		if (stage & EPipelineStage::PPLS_FRAGMENT_SHADING_RATE_ATTACHMENT)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_SHADING_RATE_ATTACHMENT_BIT_KHR;
		return result;
	}
	void GpuResourceStateToVKAccessAndPipeline(EGpuResourceState state, VkAccessFlags& outAccessFlags, VkPipelineStageFlagBits& outPipelineStages)
	{//https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkAccessFlagBits.html
		outPipelineStages = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		switch (state)
		{
		case EngineNS::NxRHI::GRS_Undefine:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_NONE;
			outPipelineStages = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
			return;
		case EngineNS::NxRHI::GRS_Present:
		case EngineNS::NxRHI::GRS_SrvPS:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_GenericRead:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT);
			return;
		case EngineNS::NxRHI::GRS_Uav:
			outAccessFlags = (VkAccessFlagBits)(VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT | VkAccessFlagBits::VK_ACCESS_SHADER_WRITE_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_UavIndirect:
			outAccessFlags = (VkAccessFlagBits)(VkAccessFlagBits::VK_ACCESS_INDIRECT_COMMAND_READ_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_DRAW_INDIRECT_BIT;
			return;
		case EngineNS::NxRHI::GRS_RenderTarget:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
			outPipelineStages = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
			return;
		case EngineNS::NxRHI::GRS_DepthStencil:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT | VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_DepthRead:
		case EngineNS::NxRHI::GRS_StencilRead:
		case EngineNS::NxRHI::GRS_DepthStencilRead:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_CopySrc:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_TRANSFER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_TRANSFER_BIT | VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		case EngineNS::NxRHI::GRS_CopyDst:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_TRANSFER_WRITE_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_TRANSFER_BIT | VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		default:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT);
			return;
		}
	}
	void GpuResourceStateToVKAccessAndPipeline2(EGpuResourceState state, VkAccessFlags2& outAccessFlags, VkPipelineStageFlagBits2& outPipelineStages)
	{
		outPipelineStages = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		switch (state)
		{
		case EngineNS::NxRHI::GRS_Undefine:
			outAccessFlags = VK_ACCESS_2_NONE;
			outPipelineStages = VK_PIPELINE_STAGE_2_TOP_OF_PIPE_BIT;
			return;
		case EngineNS::NxRHI::GRS_Present:
		case EngineNS::NxRHI::GRS_SrvPS:
			outAccessFlags = VK_ACCESS_2_SHADER_READ_BIT;
			outPipelineStages = VK_PIPELINE_STAGE_2_FRAGMENT_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_GenericRead:
			outAccessFlags = VK_ACCESS_2_SHADER_READ_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_2_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_2_FRAGMENT_SHADER_BIT);
			return;
		case EngineNS::NxRHI::GRS_Uav:
			outAccessFlags = (VK_ACCESS_2_SHADER_READ_BIT | VK_ACCESS_2_SHADER_WRITE_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_2_COMPUTE_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_UavIndirect:
			outAccessFlags = (VK_ACCESS_2_INDIRECT_COMMAND_READ_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_2_DRAW_INDIRECT_BIT;
			return;
		case EngineNS::NxRHI::GRS_RenderTarget:
			outAccessFlags = VK_ACCESS_2_COLOR_ATTACHMENT_WRITE_BIT;
			outPipelineStages = VK_PIPELINE_STAGE_2_COLOR_ATTACHMENT_OUTPUT_BIT;
			return;
		case EngineNS::NxRHI::GRS_DepthStencil:
			outAccessFlags = VK_ACCESS_2_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT | VK_ACCESS_2_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_2_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_DepthRead:
		case EngineNS::NxRHI::GRS_StencilRead:
		case EngineNS::NxRHI::GRS_DepthStencilRead:
			outAccessFlags = VK_ACCESS_2_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_2_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_CopySrc:
			outAccessFlags = VK_ACCESS_2_TRANSFER_READ_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_TRANSFER_BIT | VK_PIPELINE_STAGE_2_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		case EngineNS::NxRHI::GRS_CopyDst:
			outAccessFlags = VK_ACCESS_2_TRANSFER_WRITE_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_TRANSFER_BIT | VK_PIPELINE_STAGE_2_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		default:
			outAccessFlags = VK_ACCESS_2_SHADER_READ_BIT;
			outPipelineStages = (VK_PIPELINE_STAGE_2_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_2_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_2_FRAGMENT_SHADER_BIT);
			return;
		}
	}
	void VKCommandList::SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess)
	{
		VkMemoryBarrier mb{};
		mb.sType = VK_STRUCTURE_TYPE_MEMORY_BARRIER;
		//mb.pNext = NULL;
		mb.srcAccessMask = BarrierAccessToVK(srcAccess);
		mb.dstAccessMask = BarrierAccessToVK(dstAccess);

		auto _srcStages = PipelineStateToVK(srcStage);
		auto _dstStages = PipelineStateToVK(dstStage);
		vkCmdPipelineBarrier(
			GetVKCmdRecorder()->mCommandBuffer,
			_srcStages, 
			_dstStages,
			0,
			1,
			&mb,
			0,
			nullptr,
			0,
			nullptr
		);
	}
	void VKCommandList::SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{
		VkBufferMemoryBarrier barrier{};
		barrier.sType = VK_STRUCTURE_TYPE_BUFFER_MEMORY_BARRIER;
		barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		barrier.buffer = (VkBuffer)pResource->GetHWBuffer();
		barrier.offset = 0;
		barrier.size = pResource->Desc.Size;

		VkPipelineStageFlagBits srcStages, dstStages;
		GpuResourceStateToVKAccessAndPipeline(srcAccess, barrier.srcAccessMask, srcStages);
		GpuResourceStateToVKAccessAndPipeline(dstAccess, barrier.dstAccessMask, dstStages);

		vkCmdPipelineBarrier(
			GetVKCmdRecorder()->mCommandBuffer,
			srcStages,
			dstStages,//VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,//VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
			0,
			0, nullptr,
			1, &barrier,
			0, nullptr
		);
	}
	void VKCommandList::SetTextureBarrier(ITexture* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{
		auto oldLayout = GpuStateToVKImageLayout(srcAccess);
		auto newLayout = GpuStateToVKImageLayout(dstAccess);
		if (oldLayout != newLayout)
		{
			if (false)
			{
				VkImageMemoryBarrier barrier{};
				barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
				barrier.oldLayout = oldLayout;
				barrier.newLayout = newLayout;
				barrier.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
				barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
				barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
				barrier.image = (VkImage)pResource->GetHWBuffer();
				barrier.subresourceRange.baseArrayLayer = 0;
				barrier.subresourceRange.layerCount = pResource->Desc.ArraySize;
				barrier.subresourceRange.baseMipLevel = 0;//All
				barrier.subresourceRange.levelCount = pResource->Desc.MipLevels;
				
				if (pResource->Desc.BindFlags & EBufferType::BFT_SRV)
					barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(pResource->Desc.Format, true, false);
				else
					barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(pResource->Desc.Format, true, true);
				
				VkPipelineStageFlagBits srcStages, dstStages;
				GpuResourceStateToVKAccessAndPipeline(srcAccess, barrier.srcAccessMask, srcStages);
				GpuResourceStateToVKAccessAndPipeline(dstAccess, barrier.dstAccessMask, dstStages);

				vkCmdPipelineBarrier(
					GetVKCmdRecorder()->mCommandBuffer,
					srcStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
					dstStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,//VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
					VK_DEPENDENCY_BY_REGION_BIT,
					0, nullptr,
					0, nullptr,
					1, &barrier
				);
			}
			else
			{
				VkImageMemoryBarrier2 barrier2 = {};
				barrier2.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER_2;
				barrier2.oldLayout = oldLayout;
				barrier2.newLayout = newLayout;
				barrier2.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
				barrier2.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
				barrier2.image = (VkImage)pResource->GetHWBuffer();
				barrier2.subresourceRange.baseArrayLayer = 0;
				barrier2.subresourceRange.layerCount = pResource->Desc.ArraySize;
				barrier2.subresourceRange.baseMipLevel = 0;//All
				barrier2.subresourceRange.levelCount = pResource->Desc.MipLevels;

				if (pResource->Desc.BindFlags & EBufferType::BFT_SRV)
					barrier2.subresourceRange.aspectMask = FormatToVKImageAspectFlags(pResource->Desc.Format, true, false);
				else
					barrier2.subresourceRange.aspectMask = FormatToVKImageAspectFlags(pResource->Desc.Format, true, true);

				GpuResourceStateToVKAccessAndPipeline2(srcAccess, barrier2.srcAccessMask, barrier2.srcStageMask);
				GpuResourceStateToVKAccessAndPipeline2(dstAccess, barrier2.dstAccessMask, barrier2.dstStageMask);
				
				VkDependencyInfo dependencyInfo = {};
				dependencyInfo.sType = VK_STRUCTURE_TYPE_DEPENDENCY_INFO;
				dependencyInfo.dependencyFlags = VK_DEPENDENCY_BY_REGION_BIT; 
				dependencyInfo.imageMemoryBarrierCount = 1;
				dependencyInfo.pImageMemoryBarriers = &barrier2;

				vkCmdPipelineBarrier2(GetVKCmdRecorder()->mCommandBuffer, &dependencyInfo);
			}
		}
	}
	//UINT64 VKCommandList::SignalFence(IFence* fence, UINT64 value, IEvent* evt)
	//{
	//	ASSERT(mCmdListState == ECmdListState::Recording);
	//	auto dxFence = ((VKFence*)fence);
	//	/*if (evt != nullptr)
	//	{
	//		dxFence->mFence->SetEventOnCompletion(value, ((VKEvent*)evt)->mHandle);
	//	}
	//	else
	//	{
	//		dxFence->mFence->SetEventOnCompletion(value, dxFence->mEvent->mHandle);
	//	}
	//	mContext->Signal(dxFence->mFence, value);*/
	//	VkSemaphoreSignalInfo info{};
	//	info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_SIGNAL_INFO;
	//	/*info.semaphore = mSemaphore;
	//	info.value = count;*/
	//	auto device = (VKGpuDevice*)mDevice.GetPtr();
	//	vkSignalSemaphore(device->mDevice, &info);

	//	ASSERT(false);
	//	return value;
	//}
	//void VKCommandList::WaitGpuFence(IFence* fence, UINT64 value)
	//{
	//	ASSERT(mCmdListState == ECmdListState::Recording);
	//	auto dxFence = ((VKFence*)fence);
	//	//mContext->Wait(dxFence->mFence, value);
	//	//vkCmdWaitEvents(mCommandBuffer, )
	//	ASSERT(false);
	//}
	void VKCommandList::CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size)
	{
		GetCmdRecorder()->UseResource(target);
		GetCmdRecorder()->UseResource(src);

		VkBufferCopy copyRegion{};
		copyRegion.srcOffset = SrcOffset;
		copyRegion.dstOffset = DstOffset;
		copyRegion.size = Size;

		vkCmdCopyBuffer(GetVKCmdRecorder()->mCommandBuffer, (VkBuffer)src->GetHWBuffer(), (VkBuffer)target->GetHWBuffer(), 1, &copyRegion);
	}
	void VKCommandList::CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* source, UINT srcSubRes, const FSubresourceBox* box)
	{
		GetCmdRecorder()->UseResource(target);
		GetCmdRecorder()->UseResource(source);
		
		VkImageCopy region{};
		// We copy the image aspect, layer 0, mip 0:
		region.srcSubresource.aspectMask = ((VKTexture*)source)->GetImageAspect();
		region.srcSubresource.baseArrayLayer = srcSubRes;
		region.srcSubresource.layerCount = 1;
		region.srcSubresource.mipLevel = 0;
		// (0, 0, 0) in the first image corresponds to (0, 0, 0) in the second image:
		if (box != nullptr)
		{
			region.srcOffset = { (int)box->Left, (int)box->Top, (int)box->Front };
			region.extent = { box->Right - box->Left, box->Bottom - box->Top, box->Back - box->Front };
		}
		else
		{
			region.srcOffset = { 0, 0, 0 };
			region.extent = { source->Desc.Width, 
				source->Desc.Height == 0 ? 1 : source->Desc.Height,
				source->Desc.Depth == 0 ? 1 : source->Desc.Depth };
		}
		region.dstSubresource = region.srcSubresource;
		region.dstSubresource.baseArrayLayer = tarSubRes;
		region.dstOffset = { (int)DstX, (int)DstY, (int)DstZ };

		vkCmdCopyImage(GetVKCmdRecorder()->mCommandBuffer, (VkImage)source->GetHWBuffer(), ((VKTexture*)source)->GetImageLayout(), (VkImage)target->GetHWBuffer(), ((VKTexture*)target)->GetImageLayout(), 1, &region);
	}
	void VKCommandList::CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint)
	{
		GetCmdRecorder()->UseResource(target);
		GetCmdRecorder()->UseResource(src);

		VkBufferImageCopy region{};
		region.imageSubresource.baseArrayLayer = (UINT)subRes / target->Desc.MipLevels;
		region.imageSubresource.mipLevel = (UINT)subRes % target->Desc.MipLevels;
		region.bufferOffset = 0;
		auto pixelWidth = GetPixelByteWidth(footprint->Format);
		if (pixelWidth > 0)
			region.bufferRowLength = footprint->RowPitch / pixelWidth;
		else
			region.bufferRowLength = 0;
		region.bufferImageHeight = 0;// height;
		region.imageSubresource.aspectMask = ((VKTexture*)src)->GetImageAspect();
		region.imageSubresource.layerCount = 1;// Desc.ArraySize;
		region.imageOffset = { (int)footprint->X, (int)footprint->Y, (int)footprint->Z };
		region.imageExtent = { footprint->Width, footprint->Height, footprint->Depth };

		vkCmdCopyBufferToImage(GetVKCmdRecorder()->mCommandBuffer, (VkBuffer)src->GetHWBuffer(), (VkImage)target->GetHWBuffer(), VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);
	}
	void VKCommandList::CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* source, UINT subRes)
	{
		GetCmdRecorder()->UseResource(target);
		GetCmdRecorder()->UseResource(source);
		
		VkBufferImageCopy region{};
		region.imageSubresource.baseArrayLayer = (UINT)subRes / source->Desc.MipLevels;
		region.imageSubresource.mipLevel = (UINT)subRes % source->Desc.MipLevels;
		region.bufferOffset = 0;
		auto pixelWidth = GetPixelByteWidth(footprint->Format);
		if (pixelWidth > 0)
			region.bufferRowLength = footprint->RowPitch / pixelWidth;
		else
			region.bufferRowLength = 0;
		region.bufferImageHeight = 0;// footprint->Height;
		region.imageSubresource.aspectMask = ((VKTexture*)source)->GetImageAspect();
		region.imageSubresource.layerCount = 1;// Desc.ArraySize;
		region.imageOffset = { (int)footprint->X, (int)footprint->Y, (int)footprint->Z };
		region.imageExtent = { footprint->Width, footprint->Height, footprint->Depth };

		auto vkSource = (VKTexture*)source;
		/*VkMemoryRequirements memRequirements;
		auto device = (VKGpuDevice*)mDevice.GetPtr();
		vkGetImageMemoryRequirements(device->mDevice, vkSource->mImage, &memRequirements);*/
		vkCmdCopyImageToBuffer(GetVKCmdRecorder()->mCommandBuffer, vkSource->mImage, vkSource->GetImageLayout(), (VkBuffer)target->GetHWBuffer(), 1, &region);
	}
}

NS_END