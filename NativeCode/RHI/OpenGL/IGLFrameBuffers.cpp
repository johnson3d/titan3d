#include "IGLFrameBuffers.h"
#include "IGLRenderTargetView.h"
#include "IGLDepthStencilView.h"
#include "IGLTexture2D.h"
#include "IGLShaderResourceView.h"
#include "IGLRenderContext.h"
#include "IGLSwapChain.h"

#define new VNEW

NS_BEGIN

IGLFrameBuffers::IGLFrameBuffers()
{
	mNeedUpdate = true;
}

IGLFrameBuffers::~IGLFrameBuffers()
{
	Cleanup();
}

void IGLFrameBuffers::Cleanup()
{
	mFrameBufferId.reset();
}

void IGLFrameBuffers::BindRenderTargetView(UINT index, IRenderTargetView* rt)
{
	IFrameBuffers::BindRenderTargetView(index, rt);

	mNeedUpdate = true;
}

void IGLFrameBuffers::BindDepthStencilView(IDepthStencilView* ds)
{
	if (mDesc.UseDSV == FALSE)
		return;
	IFrameBuffers::BindDepthStencilView(ds);

	mNeedUpdate = true;
}

void IGLFrameBuffers::ApplyBuffers(GLSdk* sdk)
{
	if (mDesc.IsSwapChainBuffer == FALSE)
	{
		if (mNeedUpdate == false)
		{
			sdk->BindFramebuffer(GL_FRAMEBUFFER, mFrameBufferId);
		}
		else
		{
			mNeedUpdate = false;

			sdk->BindFramebuffer(GL_FRAMEBUFFER, mFrameBufferId);
			std::vector<GLenum> drawBuffers;
			for (int i = 0; i < MAX_MRT_NUM; i++)
			{
				if (mRenderTargets[i] == nullptr || mRenderTargets[i]->GetTexture2D() == nullptr)
					break;
				auto refTex2d = (IGLTexture2D*)mRenderTargets[i]->GetTexture2D();
				//GLenum target, GLenum attachment, GLenum textarget, GLuint texture, GLint level
				//glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0 + i, texture->mView, i);
				sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0 + i, GL_TEXTURE_2D, refTex2d->mGlesTexture2D, 0);
				drawBuffers.push_back(GL_COLOR_ATTACHMENT0 + i);
			}

			if (mDesc.UseDSV == TRUE)
			{
				if (m_pDepthStencilView != NULL)
				{
					auto rDSView = (IGLDepthStencilView*)m_pDepthStencilView;
					auto GlesTex2d = ((IGLTexture2D*)(ITexture2D*)rDSView->m_refTexture2D)->mGlesTexture2D;

					switch (rDSView->mDesc.Format)
					{
					case PXF_D24_UNORM_S8_UINT:
						sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_TEXTURE_2D, GlesTex2d, 0);
						break;
					case PXF_D32_FLOAT:
					case PXF_D16_UNORM:
						sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, GlesTex2d, 0);
						break;
					default:
						sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, GlesTex2d, 0);
						break;
					}
				}
				else
				{
					sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_TEXTURE_2D, 0, 0);
					//sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_TEXTURE_2D, 0, 0);
				}
			}
			else
			{
				sdk->FramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_TEXTURE_2D, 0, 0);
			}

			if (drawBuffers.size() > 0)
			{
				sdk->DrawBuffers((GLsizei)drawBuffers.size(), &drawBuffers[0], [=]()->void {
					VFX_LTRACE(ELTT_Graphics, "DrawBuffers Error: Num=%d\r\n", (int)drawBuffers.size());
					for (int i = 0; i < MAX_MRT_NUM; i++)
					{
						if (mRenderTargets[i] == nullptr || mRenderTargets[i]->GetTexture2D() == nullptr)
							break;
						auto refTex2d = (IGLTexture2D*)mRenderTargets[i]->GetTexture2D();
						ITexture2DDesc& desc = refTex2d->mDesc;

						VFX_LTRACE(ELTT_Graphics, "DrawBuffer(%d): %s\r\n", i, desc.ToString().c_str());
					}
					if (mDesc.UseDSV == TRUE)
					{
						if (m_pDepthStencilView != NULL)
						{
							auto rDSView = (IGLDepthStencilView*)m_pDepthStencilView;
							auto GlesTex2d = ((IGLTexture2D*)(ITexture2D*)rDSView->m_refTexture2D);
							VFX_LTRACE(ELTT_Graphics, "DrawBuffer(%d) DSV: Format = %d\r\n", GlesTex2d->mDesc.Format);
						}
					}
				});
			}
			else
			{
				//sdk->DrawBuffers((GLsizei)0, nullptr);
			}
		}
	}
	else
	{
		mSwapChain->BindCurrent();
		sdk->BindFramebuffer(GL_FRAMEBUFFER, 0);

		/*auto glSwapChain = mSwapChain.UnsafeConvertTo<IGLSwapChain>();
		auto Framebuffer0 = wglAcquireDCFramebuffer(glSwapChain->mDC);
		sdk->BindFramebuffer(GL_FRAMEBUFFER, Framebuffer0);*/
	}
}

bool IGLFrameBuffers::Init(IGLRenderContext* rc, const IFrameBuffersDesc* desc)
{
	mDesc = *desc;
	auto sdk = GLSdk::ImmSDK;
	if (mDesc.IsSwapChainBuffer==FALSE)
	{
		mFrameBufferId = std::shared_ptr<GLSdk::GLBufferId>(sdk->GenFramebuffers());
	}
	else
	{

	}
	return true;
}

NS_END