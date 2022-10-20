#pragma once
#include "../NxBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKBuffer : public IBuffer
	{
	public:
		VKBuffer();
		~VKBuffer();
		bool Init(VKGpuDevice* device, const FBufferDesc& desc);

		virtual void* GetHWBuffer() override{
			return (void*)mBuffer;
		}
		virtual void Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size) override;
		virtual bool Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(ICommandList* cmd, UINT index) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;

		UINT64 GetGPUVirtualAddress()
		{
			return 0;
		}
	public:
		TObjectHandle<VKGpuDevice>		mDeviceRef;
		VkBuffer						mBuffer = (VkBuffer)nullptr;
		AutoRef<FGpuMemory>				mGpuMemory;
	};

	class VKTexture : public ITexture
	{
	public:
		VKTexture();
		~VKTexture();
		bool Init(VKGpuDevice* device, const FTextureDesc& desc);
		virtual void* GetHWBuffer() override {
			return (void*)mImage;
		}
		virtual bool Map(ICommandList* cmd, UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(ICommandList* cmd, UINT subRes) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;

		VkImageLayout GetImageLayout();
		VkImageAspectFlagBits GetImageAspect();
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		VkImage						mImage = (VkImage)nullptr;
		//VkImageLayout				mLayout = VkImageLayout::VK_IMAGE_LAYOUT_UNDEFINED;
		AutoRef<FGpuMemory>			mGpuMemory;
		EShaderType					mShaderStages = EShaderType::SDT_AllStages;
	};

	class VKCbView : public ICbView
	{
	public:
		VKCbView();
		~VKCbView();
		virtual void* GetHWBuffer() override {
			//return mView;
			return Buffer->GetHWBuffer();
		}
		bool Init(VKGpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc);
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKVbView : public IVbView
	{
	public:
		VKVbView();
		~VKVbView();
		bool Init(VKGpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc);
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKIbView : public IIbView
	{
	public:
		VKIbView();
		~VKIbView();
		bool Init(VKGpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc);
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKSrView : public ISrView
	{
	public:
		VKSrView();
		~VKSrView();
		virtual void* GetHWBuffer() override {
			if (Desc.Type == ESrvType::ST_BufferSRV)
			{
				if (Desc.Format == EPixelFormat::PXF_UNKNOWN)
				{
					return Buffer->GetHWBuffer();
				}
				ASSERT(false);
				return nullptr;
			}
			else
			{
				return (void*)mImageView;
			}
		}
		bool Init(VKGpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc& desc);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
		virtual UINT GetFingerPrint() const override {
			return mFingerPrint;
		}
		void FreeView();
		virtual void SetDebugName(const char* name) override;
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		
		VkImageView					mImageView = (VkImageView)nullptr;
		UINT						mFingerPrint = 0;
	};

	class VKUaView : public IUaView
	{
	public:
		VKUaView();
		~VKUaView();
		virtual void* GetHWBuffer() override {
			if (Desc.ViewDimension == EDimensionUAV::UAV_DIMENSION_BUFFER)
			{
				if (Desc.Format == EPixelFormat::PXF_UNKNOWN)
				{
					return Buffer->GetHWBuffer();
				}
				ASSERT(false);
				return nullptr;
			}
			else
			{
				return (void*)mImageView;
			}
		}
		bool Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		VkImageView					mImageView = (VkImageView)nullptr;
	};

	class VKRenderTargetView : public IRenderTargetView
	{
	public:
		VKRenderTargetView();
		~VKRenderTargetView();
		virtual void* GetHWBuffer() override {
			return (void*)mView;
		}
		bool Init(VKGpuDevice* device, ITexture* pBuffer, const FRtvDesc* desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		VkImageView					mView = (VkImageView)nullptr;
	};

	class VKDepthStencilView : public IDepthStencilView
	{
	public:
		VKDepthStencilView();
		~VKDepthStencilView();
		virtual void* GetHWBuffer() override {
			return (void*)mView;
		}
		bool Init(VKGpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		VkImageView					mView = (VkImageView)nullptr;
	};
}

NS_END
