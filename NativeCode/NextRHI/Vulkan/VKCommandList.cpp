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

#include <pix3.h>

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<MemAlloc::FPagedObject<VkCommandBuffer>>>
	{
		static void Destroy(AutoRef<MemAlloc::FPagedObject<VkCommandBuffer>> obj, IGpuDevice* device1)
		{
			ASSERT(false);
			vkResetCommandBuffer(obj->RealObject, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
			obj->Free();
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkCommandPool>
	{
		static void Destroy(VkCommandPool obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyCommandPool(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	VKCommandbufferAllocator::~VKCommandbufferAllocator()
	{
		for (size_t i = 0; i < mWaitFrees.size(); i++)
		{
			auto& holder = mWaitFrees[i];
			holder.CmdBuffer->Release();
			holder.CmdBuffer = nullptr;
			holder.Fence = nullptr;
		}
		mWaitFrees.clear();
	}
	void VKCommandbufferAllocator::TickForRecycle(VKGpuDevice* device)
	{
		VAutoVSLLock lk(mLocker);
		for (size_t i = 0; i < mWaitFrees.size(); i++)
		{
			auto& holder = mWaitFrees[i];
			auto pAllocator = (VKCommandbufferAllocator*)holder.CmdBuffer->HostPage.GetPtr()->Allocator.GetPtr();
			ASSERT(pAllocator == device->mCmdAllocatorManager->GetThreadContext());
			auto completed = holder.Fence->GetCompletedValue();
			if (completed >= holder.CmdBuffer->mTargetValue)
			{
				MemAlloc::FPagedObject<VkCommandBuffer>* pTmp = holder.CmdBuffer;
				vkResetCommandBuffer(holder.CmdBuffer->RealObject, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
				auto checkValue = ((VKCommandBufferPagedObject*)pTmp)->mState;
				ASSERT(checkValue == EPagedCmdBufferState::PCBS_Commiting);
				((VKCommandBufferPagedObject*)pTmp)->mState = EPagedCmdBufferState::PCBS_WaitFree;
				holder.CmdBuffer->Free();
				((VKCommandBufferPagedObject*)pTmp)->mState = EPagedCmdBufferState::PCBS_Free;
				
				mWaitFrees.erase(mWaitFrees.begin() + i);
				i--;
			}
		}
	}
	void VKCommandbufferAllocator::PushRecycle(const AutoRef<IFence>& fence, AutoRef<VKCommandBufferPagedObject>& buffer)
	{
		ASSERT(buffer != nullptr);
		FCmdBufferHolder tmp;
		tmp.CmdBuffer = buffer;
		tmp.Fence = fence;
		buffer->mTargetValue = UINT64_MAX;
		
		VAutoVSLLock lk(mLocker);
		mWaitFrees.push_back(tmp);
	}
	void VKCmdBufferManager::Initialize(VKGpuDevice* device)
	{
		mDevice = device;
	}
	void VKCmdBufferManager::InitContext(VKCommandbufferAllocator* context)
	{
		context->Creator.mDeviceRef.FromObject(mDevice);
	}
	VKCommandBufferPage::~VKCommandBufferPage()
	{
		//Allocator.GetPtr();
	}
	MemAlloc::FPage<VkCommandBuffer>* VKCommandBufferCreator::CreatePage(UINT pageSize)
	{
		auto device = mDeviceRef.GetPtr();
		VkCommandPoolCreateInfo poolInfo = {};
		poolInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
		poolInfo.queueFamilyIndex = ((VKCmdQueue*)device->GetCmdQueue())->mGraphicsQueueIndex;
		poolInfo.flags = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;

		VkCommandPool descPool;
		if (vkCreateCommandPool(device->mDevice, &poolInfo, device->GetVkAllocCallBacks(), &descPool) != VK_SUCCESS)
		{
			return nullptr;
		}

		auto result = new VKCommandBufferPage();
		result->mCommandPool = descPool;
		return result;
	}
	MemAlloc::FPagedObject<VkCommandBuffer>* VKCommandBufferCreator::CreatePagedObject(MemAlloc::FPage<VkCommandBuffer>* page, UINT index)
	{
		auto device = mDeviceRef.GetPtr();

		VkCommandBufferAllocateInfo allocInfo{};
		allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
		allocInfo.commandPool = ((VKCommandBufferPage*)page)->mCommandPool;
		allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
		allocInfo.commandBufferCount = 1;

		VkCommandBuffer tmp = nullptr;
		auto hr = vkAllocateCommandBuffers(device->mDevice, &allocInfo, &tmp);
		if (hr != VK_SUCCESS)
		{
			return nullptr;
		}

		auto result = new VKCommandBufferPagedObject();
		result->RealObject = tmp;

		return result;
	}
	void VKCommandBufferCreator::OnFree(MemAlloc::FPagedObject<VkCommandBuffer>* obj)
	{
		vkResetCommandBuffer(obj->RealObject, VK_COMMAND_BUFFER_RESET_RELEASE_RESOURCES_BIT);
	}
	void VKCommandBufferCreator::FinalCleanup(MemAlloc::FPage<VkCommandBuffer>* page)
	{
		auto device = mDeviceRef.GetPtr();
		auto pPage = (VKCommandBufferPage*)page;
		vkDestroyCommandPool(device->mDevice, pPage->mCommandPool, device->GetVkAllocCallBacks());
		pPage->mCommandPool = nullptr;
	}

	VKCommandList::VKCommandList()
	{
		
	}
	VKCommandList::~VKCommandList()
	{
		if (GetVKDevice() == nullptr)
		{
			if (mCommandBuffer != nullptr)
			{
				mCommandBuffer->Release();
				mCommandBuffer = nullptr;
			}
			return;
		}

		if (mCommandBuffer != nullptr)
		{
			GetVKDevice()->DelayDestroy(mCommandBuffer);
			mCommandBuffer = nullptr;
		}
	}
	bool VKCommandList::Init(VKGpuDevice* device)
	{
		mDevice.FromObject(device);
		
		FFenceDesc desc;
		desc.InitValue = 0;
		mCommitFence = MakeWeakRef(device->CreateFence(&desc, "Dx12Cmdlist Commit fence"));
		
		//mCommandBuffer = device->mCmdAllocatorManager->Alloc(device);

		mIsRecording = false;
		return true;
	}
	bool VKCommandList::BeginCommand()
	{
		return BeginCommand(VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT);
	}
	bool VKCommandList::BeginCommand(VkCommandBufferUsageFlagBits flags)
	{
		if (mIsRecording)
		{
			ASSERT(false);
			return true;
		}

		ASSERT(mCommandBuffer == nullptr);
		/*if (mCommandBuffer != nullptr)
		{
			ASSERT(false);
			auto targetValue = mCommitFence->GetAspectValue() + 1;
			GetVKDevice()->mCmdAllocatorManager->GetThreadContext()->PushRecycle(mCommitFence, targetValue, mCommandBuffer);
			mCommandBuffer = nullptr;
		}*/
		auto vkCmdBuffer = GetVKDevice()->mCmdAllocatorManager->GetThreadContext()->Alloc();;
		mCommandBuffer = (VKCommandBufferPagedObject*)vkCmdBuffer.GetPtr();
		MemAlloc::FPagedObject<VkCommandBuffer>* pTmp = mCommandBuffer;
		ASSERT(((VKCommandBufferPagedObject*)pTmp)->mState == EPagedCmdBufferState::PCBS_Free);
		VkCommandBufferBeginInfo beginInfo{};
		beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		beginInfo.flags = VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT;
		vkBeginCommandBuffer(mCommandBuffer->RealObject, &beginInfo);
		((VKCommandBufferPagedObject*)pTmp)->mState = EPagedCmdBufferState::PCBS_Recording;

		mIsRecording = true;
		return true;
	}
	void VKCommandList::EndCommand()
	{
		EndCommand(true);
	}
	void VKCommandList::EndCommand(bool bRecycle)
	{
		if (mIsRecording)
		{
			vkEndCommandBuffer(mCommandBuffer->RealObject);

			if (bRecycle)
				GetVKDevice()->mCmdAllocatorManager->GetThreadContext()->PushRecycle(mCommitFence, mCommandBuffer);
		}
		mIsRecording = false;
	}
		
	
	bool VKCommandList::BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name)
	{
		ASSERT(mIsRecording);
		mDebugName = name;
		BeginEvent(name);
		mCurRtvs.clear();
		mCurRtvs.resize(fb->mRenderPass->Desc.NumOfMRT);
		for (UINT i = 0; i < fb->mRenderPass->Desc.NumOfMRT; i++)
		{
			auto rtv = fb->mRenderTargets[i].UnsafeConvertTo<VKRenderTargetView>();
			mCurRtvs[i] = std::make_pair(rtv->GpuResource->GpuState, rtv);
			if (rtv != nullptr)
			{
				auto pDxTexture = rtv->GpuResource.UnsafeConvertTo<VKTexture>();
				pDxTexture->TransitionTo(this, EGpuResourceState::GRS_RenderTarget);
			}
		}
		if (fb->mDepthStencilView != nullptr)
		{
			auto dsv = fb->mDepthStencilView.UnsafeConvertTo<VKDepthStencilView>();
			if (dsv != nullptr)
			{
				auto pDxTexture = dsv->GpuResource.UnsafeConvertTo<VKTexture>();
				mCurDsv = std::make_pair(pDxTexture->GpuState, dsv);
				pDxTexture->TransitionTo(this, EGpuResourceState::GRS_DepthStencil);
			}
		}
		mCurrentFrameBuffers = fb;
		auto dxFB = ((VKFrameBuffers*)fb);

		auto pVKFrameBuffers = ((VKFrameBuffers*)fb);
		VkRenderPassBeginInfo renderPassInfo{};
		renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
		auto pRenderPass = fb->mRenderPass.UnsafeConvertTo<VKRenderPass>();
		renderPassInfo.renderPass = pRenderPass->mRenderPass;
		renderPassInfo.framebuffer = pVKFrameBuffers->mFrameBuffer;
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
		vkCmdBeginRenderPass(mCommandBuffer->RealObject, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);

		return true;
	}
	void VKCommandList::SetViewport(UINT Num, const FViewPort* pViewports)
	{
		ASSERT(mIsRecording);
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
		vkCmdSetViewport(mCommandBuffer->RealObject, 0, Num, (const VkViewport*)pViewports);
	}
	void VKCommandList::UseCurrentViewports()
	{
		if (mCurrentViewports.size() == 0)
			return;
		vkCmdSetViewport(mCommandBuffer->RealObject, 0, (UINT)mCurrentViewports.size(), (const VkViewport*)&mCurrentViewports[0]);
	}
	void VKCommandList::SetScissor(UINT Num, const FScissorRect* pScissor)
	{
		ASSERT(mIsRecording);
		//mContext->RSSetScissorRects(Num, (const D3D12_RECT*)pScissor);
		ASSERT(Num < 32);
		mCurrentScissorRects.resize(Num);
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
		vkCmdSetScissor(mCommandBuffer->RealObject, 0, Num, &mCurrentScissorRects[0]);
	}
	void VKCommandList::UseCurrentScissors()
	{
		if (mCurrentScissorRects.size() == 0)
			return;
		vkCmdSetScissor(mCommandBuffer->RealObject, 0, (UINT)mCurrentScissorRects.size(), &mCurrentScissorRects[0]);
	}
	void VKCommandList::EndPass()
	{
		if (mIsRecording)
		{
			vkCmdEndRenderPass(mCommandBuffer->RealObject);
		}
		mCurrentFrameBuffers = nullptr;
		for (auto& i : mCurRtvs)
		{
			auto pDxTexture = i.second->GpuResource.UnsafeConvertTo<VKTexture>();
			pDxTexture->TransitionTo(this, i.first);
		}
		mCurRtvs.clear();
		if (mCurDsv.second != nullptr)
		{
			auto pDxTexture = mCurDsv.second->GpuResource.UnsafeConvertTo<VKTexture>();
			pDxTexture->TransitionTo(this, mCurDsv.first);
			mCurDsv.second = nullptr;
		}

		EndEvent();
		ASSERT(mIsRecording);
	}
	void VKCommandList::BeginEvent(const char* info)
	{
		ASSERT(mIsRecording);
		
		VkDebugMarkerMarkerInfoEXT markerInfo{};
		markerInfo.sType = VK_STRUCTURE_TYPE_DEBUG_MARKER_MARKER_INFO_EXT;
		markerInfo.pMarkerName = info;
		VKGpuSystem::fn_vkCmdDebugMarkerBeginEXT(mCommandBuffer->RealObject, &markerInfo);
	}
	void VKCommandList::EndEvent()
	{
		ASSERT(mIsRecording);
		
		VKGpuSystem::fn_vkCmdDebugMarkerEndEXT(mCommandBuffer->RealObject);
	}
	void VKCommandList::SetShader(IShader* shader)
	{
		ASSERT(mIsRecording);
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
		ASSERT(mIsRecording);
		buffer->FlushDirty(this);
		
		//buffer->Buffer->TransitionTo(this, EGpuResourceState::GRS_GenericRead);
		//auto handle = ((VKCbView*)buffer)->mView;
		/*auto device = GetDX12Device()->mDevice;
		if (type == EShaderType::SDT_ComputeShader)
		{
			device->CopyDescriptorsSimple(1, mCurrentComputeSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		else
		{
			device->CopyDescriptorsSimple(1, mCurrentSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}*/
		//vkUpdateDescriptorSets
	}
	void VKCommandList::SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view)
	{
		ASSERT(mIsRecording);
		view->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
		/*if (type == EShaderType::SDT_PixelShader)
			view->Buffer->TransitionTo(this, EGpuResourceState::GRS_SrvPS);
		else
			view->Buffer->TransitionTo(this, EGpuResourceState::GRS_GenericRead);*/
		//auto handle = ((VKSrView*)view)->mBufferView;
		/*auto device = GetDX12Device()->mDevice;
		if (type == EShaderType::SDT_ComputeShader)
		{
			device->CopyDescriptorsSimple(1, mCurrentComputeSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		else
		{
			device->CopyDescriptorsSimple(1, mCurrentSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}*/
		//vkUpdateDescriptorSets
	}
	void VKCommandList::SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view)
	{
		ASSERT(mIsRecording);
		/*auto pAddr = ((ID3D12Resource*)view->Buffer->GetHWBuffer())->GetGPUVirtualAddress();
		mContext->SetGraphicsRootUnorderedAccessView(binder->DescriptorIndex, pAddr);*/
		//view->Buffer->TransitionTo(this, EGpuResourceState::GRS_Uav);
		
		/*auto device = GetDX12Device()->mDevice;
		if (type == EShaderType::SDT_ComputeShader)
		{
			device->CopyDescriptorsSimple(1, mCurrentComputeSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}
		else
		{
			device->CopyDescriptorsSimple(1, mCurrentSrvTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
		}*/
		//vkUpdateDescriptorSets
	}
	void VKCommandList::SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler)
	{
		ASSERT(mIsRecording);
		auto handle = ((VKSampler*)sampler)->mView;
		/*auto device = GetDX12Device()->mDevice;
		if (type == EShaderType::SDT_ComputeShader) 
		{
			device->CopyDescriptorsSimple(1, mCurrentComputeSamplerTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}
		else 
		{
			device->CopyDescriptorsSimple(1, mCurrentSamplerTable->GetHandle(binder->DescriptorIndex), handle->Handle, D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);
		}*/
		//vkUpdateDescriptorSets
	}
	void VKCommandList::SetVertexBuffer(UINT slot, IVbView* buffer, UINT32 Offset, UINT Stride)
	{
		ASSERT(mIsRecording);
		VkDeviceSize vkOffset = Offset;

		if (buffer == nullptr)
		{
			auto device = (VKGpuDevice*)mDevice.GetPtr();
			vkCmdBindVertexBuffers(mCommandBuffer->RealObject, slot, 1, &device->mNullVB->mBuffer, &vkOffset);
			return;
		}
		vkCmdBindVertexBuffers(mCommandBuffer->RealObject, slot, 1, &buffer->Buffer.UnsafeConvertTo<VKBuffer>()->mBuffer, &vkOffset);
	}
	void VKCommandList::SetIndexBuffer(IIbView* buffer, bool IsBit32)
	{
		ASSERT(mIsRecording);
		VkIndexType type;
		if (IsBit32)
		{
			type = VkIndexType::VK_INDEX_TYPE_UINT32;
		}
		else
		{
			type = VkIndexType::VK_INDEX_TYPE_UINT16;
		}

		vkCmdBindIndexBuffer(mCommandBuffer->RealObject, buffer->Buffer.UnsafeConvertTo<VKBuffer>()->mBuffer, 0, type);
	}
	void VKCommandList::SetGraphicsPipeline(const IGpuDrawState* drawState)
	{
		ASSERT(mIsRecording);
		vkCmdBindPipeline(mCommandBuffer->RealObject, VkPipelineBindPoint::VK_PIPELINE_BIND_POINT_GRAPHICS, ((VKGpuDrawState*)drawState)->mGraphicsPipeline);
	}
	void VKCommandList::SetComputePipeline(const IComputeEffect* drawState)
	{
		ASSERT(mIsRecording);
		vkCmdBindPipeline(mCommandBuffer->RealObject, VkPipelineBindPoint::VK_PIPELINE_BIND_POINT_COMPUTE, ((VKComputeEffect*)drawState)->mComputePipeline);
	}
	void VKCommandList::SetInputLayout(IInputLayout* layout)
	{
		ASSERT(mIsRecording);
		//mContext->IASetInputLayout(((VKInputLayout*)layout)->mLayout);
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
		ASSERT(mIsRecording);
		
		UINT dpCount = 0;
		PrimitiveTopology2VK(topology, DrawCount, dpCount);

		vkCmdDraw(mCommandBuffer->RealObject, dpCount, Instance, BaseVertex, 0);
	}
	void VKCommandList::DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance)
	{
		ASSERT(mIsRecording);
		
		UINT dpCount = 0;
		PrimitiveTopology2VK(topology, DrawCount, dpCount);

		vkCmdDrawIndexed(mCommandBuffer->RealObject, dpCount, Instance, StartIndex, BaseVertex, 0);
	}
	void VKCommandList::IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset)
	{
		ASSERT(mIsRecording);
		
		vkCmdDrawIndexedIndirect(mCommandBuffer->RealObject, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset, 1, sizeof(UINT) * 5);
	}
	void VKCommandList::Dispatch(UINT x, UINT y, UINT z)
	{
		ASSERT(mIsRecording);
		vkCmdDispatch(mCommandBuffer->RealObject, x, y, z);
	}
	void VKCommandList::IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset)
	{
		ASSERT(mIsRecording);
		
		vkCmdDispatchIndirect(mCommandBuffer->RealObject, ((VKBuffer*)indirectArg)->mBuffer, indirectArgOffset);
	}
	//UINT64 VKCommandList::SignalFence(IFence* fence, UINT64 value, IEvent* evt)
	//{
	//	ASSERT(mIsRecording);
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
	//	ASSERT(mIsRecording);
	//	auto dxFence = ((VKFence*)fence);
	//	//mContext->Wait(dxFence->mFence, value);
	//	//vkCmdWaitEvents(mCommandBuffer, )
	//	ASSERT(false);
	//}
	void VKCommandList::CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size)
	{
		auto tarSave = target->GetGpuResourceState();
		target->TransitionTo(this, EGpuResourceState::GRS_CopyDst);
		auto srcSave = src->GetGpuResourceState();
		src->TransitionTo(this, EGpuResourceState::GRS_CopySrc);

		VkBufferCopy copyRegion{};
		copyRegion.srcOffset = SrcOffset;
		copyRegion.dstOffset = DstOffset;
		copyRegion.size = Size;

		vkCmdCopyBuffer(mCommandBuffer->RealObject, (VkBuffer)src->GetHWBuffer(), (VkBuffer)target->GetHWBuffer(), 1, &copyRegion);

		target->TransitionTo(this, tarSave);
		src->TransitionTo(this, srcSave);
	}
	void VKCommandList::CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* source, UINT srcSubRes, const FSubresourceBox* box)
	{
		auto tarSave = target->GetGpuResourceState();
		target->TransitionTo(this, EGpuResourceState::GRS_CopyDst);
		auto srcSave = source->GetGpuResourceState();

		source->TransitionTo(this, EGpuResourceState::GRS_CopySrc);
		
		VkImageCopy region{};
		// We copy the image aspect, layer 0, mip 0:
		region.srcSubresource.aspectMask = ((VKTexture*)source)->GetImageAspect();
		region.srcSubresource.baseArrayLayer = srcSubRes;
		region.srcSubresource.layerCount = 1;
		region.srcSubresource.mipLevel = 0;
		// (0, 0, 0) in the first image corresponds to (0, 0, 0) in the second image:
		region.srcOffset = { (int)box->Left, (int)box->Top, (int)box->Front };
		region.dstSubresource = region.srcSubresource;
		region.dstSubresource.baseArrayLayer = tarSubRes;
		region.dstOffset = { (int)DstX, (int)DstY, (int)DstZ };

		region.extent = { box->Right - box->Left, box->Bottom - box->Top, box->Back - box->Front };

		vkCmdCopyImage(mCommandBuffer->RealObject, (VkImage)source->GetHWBuffer(), ((VKTexture*)source)->GetImageLayout(), (VkImage)target->GetHWBuffer(), ((VKTexture*)target)->GetImageLayout(), 1, &region);
		
		target->TransitionTo(this, tarSave);
		source->TransitionTo(this, srcSave);
	}
	void VKCommandList::CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint)
	{
		auto tarSave = target->GetGpuResourceState();
		target->TransitionTo(this, EGpuResourceState::GRS_CopyDst);

		auto srcSave = src->GetGpuResourceState();
		ASSERT(srcSave != EGpuResourceState::GRS_Undefine);
		src->TransitionTo(this, EGpuResourceState::GRS_CopySrc);

		VkBufferImageCopy region{};
		region.imageSubresource.baseArrayLayer = (UINT)subRes / target->Desc.MipLevels;
		region.imageSubresource.mipLevel = (UINT)subRes % target->Desc.MipLevels;
		region.bufferOffset = 0;
		region.bufferRowLength = 0;// desc.InitData[i].RowPitch;
		region.bufferImageHeight = 0;// height;
		region.imageSubresource.aspectMask = ((VKTexture*)src)->GetImageAspect();
		region.imageSubresource.layerCount = 1;// Desc.ArraySize;
		region.imageOffset = { (int)footprint->X, (int)footprint->Y, (int)footprint->Z };
		region.imageExtent = { footprint->Width, footprint->Height, footprint->Depth };

		vkCmdCopyBufferToImage(mCommandBuffer->RealObject, (VkBuffer)src->GetHWBuffer(), (VkImage)target->GetHWBuffer(), VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);

		target->TransitionTo(this, tarSave);
		src->TransitionTo(this, srcSave);
	}
	void VKCommandList::CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* source, UINT subRes)
	{
		auto tarSave = target->GetGpuResourceState();
		target->TransitionTo(this, EGpuResourceState::GRS_CopyDst);

		auto srcSave = source->GetGpuResourceState();
		ASSERT(srcSave != EGpuResourceState::GRS_Undefine);
		source->TransitionTo(this, EGpuResourceState::GRS_CopySrc);
		
		VkBufferImageCopy region{};
		region.imageSubresource.baseArrayLayer = (UINT)subRes / source->Desc.MipLevels;
		region.imageSubresource.mipLevel = (UINT)subRes % source->Desc.MipLevels;
		region.bufferOffset = 0;
		region.bufferRowLength = 0;// footprint->RowPitch;
		region.bufferImageHeight = 0;// footprint->Height;
		region.imageSubresource.aspectMask = ((VKTexture*)source)->GetImageAspect();
		region.imageSubresource.layerCount = 1;// Desc.ArraySize;
		region.imageOffset = { (int)footprint->X, (int)footprint->Y, (int)footprint->Z };
		region.imageExtent = { footprint->Width, footprint->Height, footprint->Depth };

		auto vkSource = (VKTexture*)source;
		/*VkMemoryRequirements memRequirements;
		auto device = (VKGpuDevice*)mDevice.GetPtr();
		vkGetImageMemoryRequirements(device->mDevice, vkSource->mImage, &memRequirements);*/
		vkCmdCopyImageToBuffer(mCommandBuffer->RealObject, vkSource->mImage, vkSource->GetImageLayout(), (VkBuffer)target->GetHWBuffer(), 1, &region);

		target->TransitionTo(this, tarSave);
		source->TransitionTo(this, srcSave);
	}
	void VKCommandList::Flush()
	{
		ASSERT(mIsRecording);
		ASSERT(false);
		//mContext->Flush();
	}
}

NS_END