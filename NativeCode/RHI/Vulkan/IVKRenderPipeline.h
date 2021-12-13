#pragma once
#include "../IRenderPipeline.h"
#include "../IDrawCall.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKDrawCall;
class IVKCommandList;
class IViewPort;
class IVKRenderPipeline : public IRenderPipeline
{
public:
	IVKRenderPipeline();
	~IVKRenderPipeline();

	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout) override;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State) override;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) override;

	bool Init(IVKRenderContext* rc, const IRenderPipelineDesc* desc);
	
public:
	void OnSetPipeline(IVKCommandList* cmd, EPrimitiveType dpType);
	TObjectHandle<IVKRenderContext>	mRenderContext;
	VkPipeline						mGraphicsPipeline;
};

class IVKRenderPipelineManager : public VIUnknown
{
	struct Hash
	{
		std::size_t operator()(const IRenderPipelineDesc& lh) const
		{
#if defined(PTR_64)
			return HashHelper::CalcHash64(&lh, sizeof(lh)).Int64Value;
#else
			return HashHelper::APHash(&lh, sizeof(lh));
#endif
		}
	};

	struct Equal
	{
		bool operator()(const IRenderPipelineDesc& lh, const IRenderPipelineDesc& rh) const
		{
			return memcmp(&lh, &rh, sizeof(IRenderPipelineDesc)) == 0;
		}
	};
	std::unordered_map<IRenderPipelineDesc, VkPipeline, Hash, Equal>		mPipelines;
	IVKRenderContext* mRenderContext;
public:
	IVKRenderPipelineManager(IVKRenderContext* rc);
	~IVKRenderPipelineManager();
	void Cleanup();
	VkPipeline GetOrAddPipeline(const IRenderPipelineDesc* desc, EPrimitiveType dpType, const VkViewport* vp);
};

NS_END