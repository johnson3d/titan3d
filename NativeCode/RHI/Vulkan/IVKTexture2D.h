#pragma once
#include "../ITextureBase.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKTexture2D : public ITexture2D
{
public:
	IVKTexture2D();
	~IVKTexture2D();
public:
	VKGpuMemory*		mImageMemory;
	VkImage				mVkImage;
	VkImageLayout		mCurLayout;
	TObjectHandle<IVKRenderContext> mContext;
private:
	bool				mDestroyImage;
public:
	void TransitionImageLayout(IVKRenderContext* rc, VkImage image, UINT mipLevel, UINT mipCount, VkFormat format, VkImageLayout oldLayout, VkImageLayout newLayout, VkCommandBuffer cmdBuffer = nullptr);
	void TransitionImageToLayout(IVKRenderContext* rc, VkImageLayout newLayout, VkCommandBuffer cmdBuffer = nullptr);
public:
	bool Init(IVKRenderContext* rc, const ITexture2DDesc* desc);
	bool InitVkImage(IVKRenderContext* rc, VkImage texture, const ITexture2DDesc* desc);

	virtual void* GetHWBuffer() const override {
		return mVkImage;
	}

	virtual vBOOL MapMipmap(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch) override;
	virtual void UnmapMipmap(ICommandList* cmd, int MipLevel) override;
	virtual void UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch) override;
};

NS_END