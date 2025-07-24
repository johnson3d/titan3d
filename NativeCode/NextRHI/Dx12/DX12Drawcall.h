#pragma once
#include "../NxDrawcall.h"
#include "DX12Effect.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12CommandList;
	class DX12ComputeEffect;
	struct DX12PagedHeap;
	class DX12GraphicDraw : public IGraphicDraw
	{
	public:
		DX12GraphicDraw();
		~DX12GraphicDraw();
		virtual void ResetResources() override;
		virtual IBindless* CreateBindless(const char* name) const override;
		virtual void BuildDrawcall(ICommandList* cmdlist) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, FBindResource& resource) override;

		void BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToHeap(DX12GpuDevice* device, const FEffectBinder* binder, FBindResource& resource, 
			AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder>& mSamplerHeap);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
	};

	class DX12ComputeDraw : public IComputeDraw
	{
	public:
		DX12ComputeDraw();
		~DX12ComputeDraw();
		virtual void ResetResources() override;
		virtual IBindless* CreateBindless(const char* name) const override;
		virtual void OnBindResource(const FShaderBinder* binder, FBindResource& resource) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		void BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd);
	private:
		void BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, FBindResource& resource,
			AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder>& mSamplerHeap);
	public:
		TWeakRefHandle<DX12GpuDevice>			mDeviceRef;
	};

	class DX12RayTracingDraw : public IRayTracingDraw
	{
	public:
		AutoRef<FUploadBuffer>			mHitGroupShaderBindTable;
		struct FHitGroupShaderBindTable
		{
			AutoRef<DX12RayTracingEffect::DX12HitGroup>		HitGroup;
		};
		std::vector<FHitGroupShaderBindTable>	ShaderBindTables;

		virtual IBindless* CreateBindless(const char* name) const override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;
	private:
		void BindResourceToHeap(DX12GpuDevice* device, const FShaderBinder* binder, FBindResource& resource,
			AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder>& mSamplerHeap);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		virtual void OnBindResource(const FShaderBinder* binder, FBindResource& resource) override;
		void BindDescriptorHeaps(DX12GpuDevice* device, DX12CommandList* dx12Cmd, DX12RayTracingEffect* effect, 
			AutoRef<DX12HeapHolder>& mCbvSrvUavHeap, AutoRef<DX12HeapHolder>& mSamplerHeap);
	};
}

NS_END