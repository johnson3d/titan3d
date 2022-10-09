#pragma once
#include "../NxEffect.h"
#include "DX12PreHead.h"
#include "../../Base/allocator/PagedAllocator.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Shader;
	class DX12ShaderEffect;
	struct DX12DescriptorSetPagedObject : public MemAlloc::FPagedObject<AutoRef<ID3D12DescriptorHeap>>
	{
		//DX12ShaderEffect*		ShaderEffect = nullptr;

		D3D12_CPU_DESCRIPTOR_HANDLE	GetHandle(int index);
	};
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<DX12DescriptorSetPagedObject>>
	{
		static void Destroy(AutoRef<DX12DescriptorSetPagedObject> obj, IGpuDevice* device1)
		{
			obj->Free();
		}
	};
	struct DX12DescriptorSetPage : public MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>
	{
		
	};
	struct DX12DescriptorSetCreator
	{
		TObjectHandle<DX12GpuDevice>		mDeviceRef;
		DX12ShaderEffect*					mShaderEffect = nullptr;
		D3D12_DESCRIPTOR_HEAP_DESC			mDesc{};
		bool								IsSampler = false;
		MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>* CreatePage(UINT pageSize);
		MemAlloc::FPagedObject<AutoRef<ID3D12DescriptorHeap>>* CreatePagedObject(MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>* page, UINT index);
		void OnFree(MemAlloc::FPagedObject<AutoRef<ID3D12DescriptorHeap>>* obj);
		void FinalCleanup(MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>* page);
	};
	struct DX12DescriptorSetAllocator : public MemAlloc::FPagedObjectAllocator<AutoRef<ID3D12DescriptorHeap>, DX12DescriptorSetCreator, 128>
	{
		UINT			mDescriptorStride = 0;
	};
	class DX12ShaderEffect : public IShaderEffect
	{
	public:
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist, IGraphicDraw* drawcall) override;

		AutoRef<ID3D12CommandSignature> GetIndirectDrawIndexCmdSig(ICommandList* cmdlist);
	public:
		int								mSrvTableSizeIndex;
		int								mSrvTableSize;
		int								mSamplerTableSizeIndex;
		int								mSamplerTableSize;
		AutoRef<ID3D12RootSignature>	mSignature;

		//AutoRef<ID3D12CommandSignature>	CmdSigForIndirectDrawIndex;
		AutoRef<DX12DescriptorSetAllocator>	mDescriptorAllocatorCSU;
		AutoRef<DX12DescriptorSetAllocator>	mDescriptorAllocatorSampler;
	};

	class DX12ComputeEffect : public IComputeEffect
	{
	public:
		virtual void BuildState(IGpuDevice* device) override;
		virtual void Commit(ICommandList* cmdlist) override;
	public:
		int								mSrvTableSizeIndex;
		int								mSrvTableSize;
		int								mSamplerTableSizeIndex;
		int								mSamplerTableSize;
		AutoRef<ID3D12RootSignature>	mSignature;
		AutoRef<ID3D12PipelineState>	mPipelineState;
	};
}

NS_END
