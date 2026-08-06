#pragma once
#include "../NxBuffer.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	struct VKMemoryViewWrapper : public IGpuResource
	{
		VKMemoryViewWrapper()
		{
			mBufferView = nullptr;
		}
		~VKMemoryViewWrapper()
		{
			FreeView();
		}
		void Initialize(VKGpuDevice* device);
		void AsBufferView(IBuffer* buffer)
		{
			mIsBufferView = true;
			mBuffer = buffer;
		}
		void AsTextureView(ITexture* buffer)
		{
			mIsBufferView = false;
			mBuffer = buffer;
		}
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		AutoRef<IGpuBufferData> mBuffer;
		bool mIsBufferView = false;
		union
		{
			VkBufferView				mBufferView;
			VkImageView					mImageView;
		};
		void FreeView();
		void* GetHWBuffer() {
			if (mIsBufferView)
			{
				return mBufferView;
			}
			else
			{
				return (void*)mImageView;
			}
		}
		void SetDebugName(const char* name);
	};

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
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT index) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;

		UINT64 GetGPUVirtualAddress();
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		VkBuffer mBuffer = nullptr;
		VmaAllocation mAllocation = nullptr;
		VmaAllocationInfo mAllocationInfo{};
		//valid when this buffer is the storage of a BLAS/TLAS(BFT_RTAS)
		VkAccelerationStructureKHR mAccelerationStructure = VK_NULL_HANDLE;
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
		virtual bool Map(UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT subRes) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;
		virtual bool GetFootprint(FSubResourceFootPrint* fp, UINT64* rowSize, UINT64* totalSize, UINT subRes, UINT64 offset) override;

		VkImageLayout GetImageLayout();
		VkImageAspectFlagBits GetImageAspect();
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		VkImage mImage = (VkImage)nullptr;
		VmaAllocation mAllocation = nullptr;
		VmaAllocationInfo mAllocationInfo{};
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
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKVbView : public IVbView
	{
	public:
		VKVbView();
		~VKVbView();
		bool Init(VKGpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc);
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKIbView : public IIbView
	{
	public:
		VKIbView();
		~VKIbView();
		bool Init(VKGpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc);
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		//VkBufferView				mView = nullptr;
	};

	class VKSrView : public ISrView
	{
	public:
		VKSrView();
		~VKSrView();
		virtual void* GetHWBuffer() override {
			return mView->GetHWBuffer();
		}
		bool Init(VKGpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc& desc);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
		virtual UINT GetFingerPrint() const override {
			return 0;
		}
		virtual void SetDebugName(const char* name) override;
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		
		AutoRef<VKMemoryViewWrapper> mView;
	};

	class VKUaView : public IUaView
	{
	public:
		VKUaView();
		~VKUaView();
		virtual void* GetHWBuffer() override {
			return mView->GetHWBuffer();
		}
		bool Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		AutoRef<VKMemoryViewWrapper> mView;
	};

	class VKRenderTargetView : public IRenderTargetView
	{
	public:
		VKRenderTargetView();
		~VKRenderTargetView();
		virtual void* GetHWBuffer() override {
			return mView->GetHWBuffer();
		}
		bool Init(VKGpuDevice* device, ITexture* pBuffer, const FRtvDesc* desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		AutoRef<VKMemoryViewWrapper> mView;
	};

	class VKDepthStencilView : public IDepthStencilView
	{
	public:
		VKDepthStencilView();
		~VKDepthStencilView();
		virtual void* GetHWBuffer() override {
			return mView->GetHWBuffer();
		}
		bool Init(VKGpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc);
		virtual void SetDebugName(const char* name) override;
	public:
		TWeakRefHandle<VKGpuDevice>	mDeviceRef;
		AutoRef<VKMemoryViewWrapper> mView;
	};

	class VKAccelerationStructure : public IAccelerationStructure
	{
	public:
		~VKAccelerationStructure();
		bool Init(VKGpuDevice* device, const FAccelerationStructureDesc* desc);

		TWeakRefHandle<VKGpuDevice>					mDeviceRef;
		std::vector<AutoRef<FMeshPrimitives>>		mMeshes;
		VkAccelerationStructureKHR					mAccelerationStructure = VK_NULL_HANDLE;
		VkDeviceAddress								mDeviceAddress = 0;
	};

	class VKAStructureInstance : public IAStructureInstance
	{
	public:
		bool Init(VKGpuDevice* device, const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure);
	};

	class VKTopAccelerationStructure : public ITopAccelerationStructure
	{
	public:
		~VKTopAccelerationStructure();
		bool Init(VKGpuDevice* device, const FTopAccelerationStructureDesc* desc);
		virtual bool BuildAcclerationStruture() override;

		TWeakRefHandle<VKGpuDevice>					mDeviceRef;
		Hash128										mInstanceHash;
		std::vector<VkAccelerationStructureInstanceKHR>	mInstDescs;
		VkAccelerationStructureKHR					mAccelerationStructure = VK_NULL_HANDLE;
	private:
		bool IsBuild(VKGpuDevice* device);
	};
}

NS_END
