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
		virtual void Commit(ICommandList* cmdlist) override;

		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, IGpuResource* resource) override;

		void RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToDescriptSets(DX12GpuDevice* device, const FEffectBinder* binder, IGpuResource* resource);
		void BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12GraphicsEffect* effect);
	public:
		TObjectHandle<DX12GpuDevice>	mDeviceRef;
		bool							IsDirty = false;
		AutoRef<DX12DescriptorSetPagedObject>	mSrvTable = nullptr;
		AutoRef<DX12DescriptorSetPagedObject>	mSamplerTable = nullptr;
		UINT							FingerPrient = 0;
	};

	class DX12ComputeDraw : public IComputeDraw
	{
	public:
		DX12ComputeDraw();
		~DX12ComputeDraw();
		virtual void OnBindResource(const FShaderBinder* binder, IGpuResource* resource) override;
		virtual void Commit(ICommandList* cmdlist) override;

		void RebuildDescriptorSets(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToDescriptSets(DX12GpuDevice* device, const FShaderBinder* binder, IGpuResource* resource);
		void BindDescriptors(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12ComputeEffect* effect);
	public:
		TObjectHandle<DX12GpuDevice>			mDeviceRef;
		AutoRef<DX12DescriptorSetPagedObject>	mSrvTable = nullptr;
		AutoRef<DX12DescriptorSetPagedObject>	mSamplerTable = nullptr;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;
	};
}

NS_END