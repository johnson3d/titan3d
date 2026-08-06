#pragma once
#include "../NxDrawcall.h"
#include "VKShader.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	struct FDescriptorSetInfo
	{
		struct FRtasInfo
		{
			VkWriteDescriptorSetAccelerationStructureKHR asInfo;
			VkAccelerationStructureKHR asHandle;
		};
		union
		{
			VkDescriptorImageInfo imageInfo;
			VkDescriptorBufferInfo bufferInfo;
			FRtasInfo rtasInfo;
		};
	};
	class VKCommandList;
	class VKGraphicDraw : public IGraphicDraw
	{
	public:
		~VKGraphicDraw();
	protected:
		virtual void OnGpuDrawStateUpdated() override;
		virtual void OnBindResource(const FEffectBinder* binder, FBindResource& resource) override;

		virtual void BuildDrawcall(ICommandList* cmdlist) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

		void UpdateDescriptorSets(VKCommandList* vkCmd);
	public:
		TWeakRefHandle<VKGpuDevice>				mDeviceRef;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;

		std::vector<FDescriptorSetInfo>			mDescriptorSetInfos;
		std::vector<VkWriteDescriptorSet>		mDsWriteSets;
		std::vector<VkDescriptorSet>			mDescriptorSets;
	};

	class VKComputeDraw : public IComputeDraw
	{
	public:
		virtual void OnBindResource(const FShaderBinder* binder, FBindResource& resource) override;
		virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;

	protected:
		void UpdateDescriptorSets(VKCommandList* vkCmd);
	public:
		TWeakRefHandle<VKGpuDevice>				mDeviceRef;
		bool									IsDirty = false;
		UINT									FingerPrient = 0;

		std::vector<FDescriptorSetInfo>			mDescriptorSetInfos;
		std::vector<VkWriteDescriptorSet>		mDsWriteSets;
		std::vector<VkDescriptorSet>			mDescriptorSets;
	};
}

NS_END
