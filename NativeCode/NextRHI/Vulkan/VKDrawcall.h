#pragma once
#include "../NxDrawcall.h"
#include "VKShader.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKCommandList;
	class VKGraphicDraw : public IGraphicDraw
	{
	public:
		~VKGraphicDraw();
	protected:
		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, FBindResource& resource) override;

		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		void UpdateDescriptorSets(VKCommandList* vkCmd);
		void BindDescriptorSets(VKCommandList* cmdlist);
	private:
		void BindResourceToDescriptSets(VKGpuDevice* device, 
			const FEffectBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets);
	public:
		TWeakRefHandle<VKGpuDevice>				mDeviceRef;
		AutoRef<VKDescriptorSetHolder>			mDescriptorSetVS;
		AutoRef<VKDescriptorSetHolder>			mDescriptorSetPS;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;
	};

	class VKComputeDraw : public IComputeDraw
	{
	public:
		virtual void OnBindResource(const FShaderBinder* binder, FBindResource& resource) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

	protected:
		void UpdateDescriptorSets(VKCommandList* vkCmd);
		void BindDescriptorSets(VKCommandList* cmdlist);
	private:
		void BindResourceToDescriptSets(VKGpuDevice* device,
			const FShaderBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets);
	public:
		TWeakRefHandle<VKGpuDevice>				mDeviceRef;
		AutoRef<VKDescriptorSetHolder>			mDescriptorSetCS;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;
	};
}

NS_END
