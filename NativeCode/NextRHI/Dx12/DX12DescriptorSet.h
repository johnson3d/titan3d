#pragma once
#include "../NxDescriptorSet.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12DesriptorPoolManager;
	
	class DX12DescriptorPoolWrapper : public IGpuResource
	{
	public:
		DX12GpuDevice* mDeviceRef = nullptr;
		AutoRef<ID3D12DescriptorHeap> mCbvSrvUavHeap;
		AutoRef<ID3D12DescriptorHeap> mSamplerHeap;
		UINT mCbvSrvUavDescriptorStride = 0;
		UINT mSamplerDescriptorStride = 0;
		UINT mCbvSrvUavPoolSize = 0;
		UINT mSamplerPoolSize = 0;
		UINT mCbvSrvUavCurrent = 0;
		UINT mSamplerCurrent = 0;

		void Initialize(DX12GpuDevice* device, UINT cbvsrvuav, UINT sampler);
		FDX12DescriptorHeap Alloc(int numDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type);
	};

	class DX12DescriptorPool : public IDescriptorPool
	{
	private:
		DX12GpuDevice* mDeviceRef;
		DX12DesriptorPoolManager* mManagerRef;
		D3D12_DESCRIPTOR_HEAP_TYPE mType{};
		std::vector<AutoRef<DX12DescriptorPoolWrapper>> mDescriptorPools;
		std::vector<AutoRef<DX12DescriptorPoolWrapper>> mFreeSuperBigDescriptorPools;
		std::vector<AutoRef<DX12DescriptorPoolWrapper>> mUsedSuperBigDescriptorPools;
		int mCurrentFreePoolIndex{};
		int GetCurrentPoolIndex();
		int IncreaseCurrentPoolIndex();
	public:
		void Initialize(DX12GpuDevice* device, DX12DesriptorPoolManager* manager);
		FDX12DescriptorHeap AllocDescriptorSet(int numDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type);
		virtual void Reset() override;
	};

	class DX12DesriptorPoolManager : public NxDesriptorPoolManager
	{
		DX12GpuDevice* mDeviceRef;
	public:
		int mCbvSrvUavPoolSize = 4096;
		int mSamplerPoolSize = 1024;
		virtual void Initialize(IGpuDevice* device) override;
		virtual IDescriptorPool* CreateDescriptorPool() override;
	};
}

NS_END