#include "DX12DescriptorSet.h"
#include "DX12GpuDevice.h"
#include "DX12Shader.h"

NS_BEGIN

namespace NxRHI
{
	void DX12DescriptorPoolWrapper::Initialize(DX12GpuDevice* device, UINT cbvsrvuav, UINT sampler)
	{
		mDeviceRef = device;

		mCbvSrvUavDescriptorStride = device->mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
		mSamplerDescriptorStride = device->mDevice->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER);

		D3D12_DESCRIPTOR_HEAP_DESC desc{};
		desc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		desc.NumDescriptors = cbvsrvuav;
		desc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
		mCbvSrvUavPoolSize = cbvsrvuav;
		
		auto hr = device->mDevice->CreateDescriptorHeap(&desc, IID_ID3D12DescriptorHeap, (void**)mCbvSrvUavHeap.GetAddressOf());
		ASSERT(hr == S_OK);

		desc.Type = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER;
		desc.NumDescriptors = sampler;
		mSamplerPoolSize = sampler;
		hr = device->mDevice->CreateDescriptorHeap(&desc, IID_ID3D12DescriptorHeap, (void**)mSamplerHeap.GetAddressOf());
		ASSERT(hr == S_OK);

		mCbvSrvUavCurrent = 0;
		mSamplerCurrent = 0;
	}
	FDX12DescriptorHeap DX12DescriptorPoolWrapper::Alloc(int numDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type)
	{
		FDX12DescriptorHeap result{};
		if (type == D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV)
		{
			if (mCbvSrvUavCurrent + numDescriptor > mCbvSrvUavPoolSize)
				return result;

			result.Heap = mCbvSrvUavHeap;
			result.Stride = mCbvSrvUavDescriptorStride;
			result.Start = mCbvSrvUavCurrent;
			result.Num = numDescriptor;
			result.DescriptorMask = 0;
			result.DescriptorState = 0;
			if (numDescriptor < 32)
			{
				for (int i = 0; i < numDescriptor; i++)
				{
					result.DescriptorMask |= (1 << i);
				}
			}

			mCbvSrvUavCurrent += numDescriptor;
		}
		else
		{
			if (mSamplerCurrent + numDescriptor > mSamplerPoolSize)
				return result;

			result.Heap = mSamplerHeap;
			result.Stride = mSamplerDescriptorStride;
			result.Start = mSamplerCurrent;
			result.Num = numDescriptor;
			result.DescriptorMask = 0;
			result.DescriptorState = 0;
			if (numDescriptor < 32)
			{
				for (int i = 0; i < numDescriptor; i++)
				{
					result.DescriptorMask |= (1 << i);
				}
			}

			mSamplerCurrent += numDescriptor;
		}
		return result;
	}

	void DX12DescriptorPool::Initialize(DX12GpuDevice* device, DX12DesriptorPoolManager* manager)
	{
		mDeviceRef = device;
		mManagerRef = manager;
	}
	void DX12DescriptorPool::Reset()
	{
		for (auto& pool : mDescriptorPools)
		{
			pool->mCbvSrvUavCurrent = 0;
			pool->mSamplerCurrent = 0;
		}
		mCurrentFreePoolIndex = 0;
		mFrameFenceValue = UINT64_MAX;

		mFreeSuperBigDescriptorPools.insert(mFreeSuperBigDescriptorPools.end(), mUsedSuperBigDescriptorPools.begin(), mUsedSuperBigDescriptorPools.end());
		mUsedSuperBigDescriptorPools.clear();
		for (auto& i : mFreeSuperBigDescriptorPools)
		{
			i->mCbvSrvUavCurrent = 0;
			i->mSamplerCurrent = 0;
		}
	}
	int DX12DescriptorPool::GetCurrentPoolIndex()
	{
		size_t curIndex = mCurrentFreePoolIndex;
		auto& curPools = mDescriptorPools;
		if (curPools.size() <= curIndex)
		{
			auto num = curIndex + 1 - curPools.size();
			for (size_t i = 0; i < num; i++)
			{
				DX12DescriptorPoolWrapper* newPool = new DX12DescriptorPoolWrapper();
				newPool->Initialize(mDeviceRef, mManagerRef->mCbvSrvUavPoolSize, mManagerRef->mSamplerPoolSize); // Assuming 4096 is the page size
				curPools.push_back(MakeWeakRef(newPool));
			}
		}
		return (int)curIndex;
	}
	int DX12DescriptorPool::IncreaseCurrentPoolIndex()
	{
		mCurrentFreePoolIndex++;
		return GetCurrentPoolIndex();
	}
	FDX12DescriptorHeap DX12DescriptorPool::AllocDescriptorSet(int numDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type)
	{
		size_t curIndex = GetCurrentPoolIndex();
		if (type == D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV)
		{
			if (numDescriptor > mManagerRef->mCbvSrvUavPoolSize)
			{
				AutoRef<DX12DescriptorPoolWrapper> pool;
				for (auto it = mFreeSuperBigDescriptorPools.begin(); it != mFreeSuperBigDescriptorPools.end(); it++)
				{
					if ((*it)->mCbvSrvUavPoolSize >= (UINT)numDescriptor)
					{
						pool = *it;
						mUsedSuperBigDescriptorPools.push_back(pool);
						mFreeSuperBigDescriptorPools.erase(it);
						break;
					}
				}
				if (pool != nullptr)
				{
					return pool->Alloc(numDescriptor, type);
				}
				pool = MakeWeakRef(new DX12DescriptorPoolWrapper());
				pool->Initialize(mDeviceRef, numDescriptor, 1);

				mUsedSuperBigDescriptorPools.push_back(pool);
				return pool->Alloc(numDescriptor, type);
			}
		}
		else
		{
			if (numDescriptor > mManagerRef->mSamplerPoolSize)
			{
				AutoRef<DX12DescriptorPoolWrapper> pool;
				for (auto it = mFreeSuperBigDescriptorPools.begin(); it != mFreeSuperBigDescriptorPools.end(); it++)
				{
					if ((*it)->mSamplerPoolSize >= (UINT)numDescriptor)
					{
						pool = *it;
						mUsedSuperBigDescriptorPools.push_back(pool);
						mFreeSuperBigDescriptorPools.erase(it);
						break;
					}
				}
				if (pool != nullptr)
				{
					return pool->Alloc(numDescriptor, type);
				}
				pool = MakeWeakRef(new DX12DescriptorPoolWrapper());
				pool->Initialize(mDeviceRef, 1, numDescriptor);

				mUsedSuperBigDescriptorPools.push_back(pool);
				return pool->Alloc(numDescriptor, type);
			}
		}
		auto& curPools = mDescriptorPools;
		auto result = curPools[curIndex]->Alloc(numDescriptor, type);
		if (result.Num != 0)
			return result;

		curIndex = IncreaseCurrentPoolIndex();
		return curPools[curIndex]->Alloc(numDescriptor, type);
	}
	void DX12DesriptorPoolManager::Initialize(IGpuDevice* device)
	{
		mDeviceRef = (DX12GpuDevice*)device;
		NxDesriptorPoolManager::Initialize(device);
	}
	IDescriptorPool* DX12DesriptorPoolManager::CreateDescriptorPool()
	{
		auto result = new DX12DescriptorPool();
		result->Initialize(mDeviceRef, this);
		return result;
	}
}

NS_END