#pragma once
#include "../NxDrawcall.h"
#include "DX12Effect.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12CommandList;
	class DX12ComputeEffect;
	struct DX12DescriptorSetPagedObject;
	class DX12GraphicDraw : public IGraphicDraw
	{
	public:
		DX12GraphicDraw();
		~DX12GraphicDraw();
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, IGpuResource* resource) override;

		void RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToDescriptSets(DX12GpuDevice* device, const FEffectBinder* binder, IGpuResource* resource);
		void BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12GraphicsEffect* effect);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		bool							IsDirty = false;
		AutoRef<DX12DescriptorSetPagedObject>	mCbvSrvUavHeap = nullptr;
		AutoRef<DX12DescriptorSetPagedObject>	mSamplerHeap = nullptr;
		UINT							FingerPrient = 0;
	};

	class DX12ComputeDraw : public IComputeDraw
	{
	public:
		DX12ComputeDraw();
		~DX12ComputeDraw();
		virtual void OnBindResource(const FShaderBinder* binder, IGpuResource* resource) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		void RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToDescriptSets(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource);
		void BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12ComputeEffect* effect);
	public:
		TWeakRefHandle<DX12GpuDevice>			mDeviceRef;
		bool									IsDirty = false;
		AutoRef<DX12DescriptorSetPagedObject>	mCbvSrvUavHeap = nullptr;
		AutoRef<DX12DescriptorSetPagedObject>	mSamplerHeap = nullptr;		
		UINT									FingerPrient = 0xffffffff;
	};
}

NS_END