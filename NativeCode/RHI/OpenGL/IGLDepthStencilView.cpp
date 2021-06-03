#include "IGLDepthStencilView.h"
#include "IGLRenderContext.h"
#include "IGLShaderResourceView.h"

#define new VNEW

NS_BEGIN

IGLDepthStencilView::IGLDepthStencilView()
{
	/*m_pTex2d = nullptr;
	m_pSRV = nullptr;*/
}

IGLDepthStencilView::~IGLDepthStencilView()
{
	if (EngineIsCleared)
		return;
	
	Cleanup();
}

void IGLDepthStencilView::Cleanup() 
{
	/*if (m_pTex2d != nullptr)
	{
		m_pTex2d->Release();
		m_pTex2d = nullptr;
	}
	Safe_Release(m_pSRV);*/
}

bool IGLDepthStencilView::Init(IGLRenderContext* rc, const IDepthStencilViewDesc* pDesc)
{
	mDesc = *pDesc;
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);
	/*auto sdk = rc->mImmCmdList;
	GLuint	Tex2d = 0;
	if(0)
	{
		sdk->GenRenderbuffers(1, &Tex2d);
		sdk->BindRenderbuffer(GL_RENDERBUFFER, Tex2d);
		sdk->RenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT, mDesc.Width, mDesc.Height);
		sdk->BindRenderbuffer(GL_RENDERBUFFER, 0);
	}
	else
	{*/
//		sdk->GenTextures(1, &Tex2d);
//		sdk->BindTexture(GL_TEXTURE_2D, Tex2d);
//		GLuint format = GL_DEPTH_COMPONENT;
//		GLenum type = GL_UNSIGNED_INT;
//		GLenum internalFormat = GL_DEPTH_COMPONENT16;
//		switch (desc->Format)
//		{
//		case PXF_D24_UNORM_S8_UINT:
//			format = GL_DEPTH_STENCIL;
//			type = GL_UNSIGNED_INT_24_8;
//			internalFormat = GL_DEPTH24_STENCIL8;
//			break;
//		case PXF_D32_FLOAT:
//			type = GL_FLOAT;
//#if defined(PLATFORM_WIN)
//			format = GL_DEPTH_COMPONENT;
//			internalFormat = GL_DEPTH_COMPONENT32F;
//#elif defined(PLATFORM_DROID)
//			format = GL_DEPTH_COMPONENT;
//			internalFormat = GL_DEPTH_COMPONENT32F;
//#else
//#endif
//			break;
//		case PXF_D32_FLOAT_S8X24_UINT:
//			//format = GL_DEPTH_DEPTH32F;
//			break;
//		case PXF_D16_UNORM:
//			format = GL_DEPTH_COMPONENT;
//			type = GL_UNSIGNED_INT;
//			internalFormat = GL_DEPTH_COMPONENT16;
//			break;
//		default:
//			break;
//		}
//		sdk->TexImage2D(GL_TEXTURE_2D, 0, internalFormat, mDesc.Width, mDesc.Height, 0, format, type, 0);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);
//		sdk->TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);
//		sdk->BindTexture(GL_TEXTURE_2D, 0);
//		
//		m_pTex2d = new IGLTexture2D();
//		m_pTex2d->mGlesTexture2D = Tex2d;

		/*m_pSRV = new IGLShaderResourceView();
		m_refSRV_DS.StrongRef(m_pSRV);
		m_refSRV_DS->m_refTexture2D.StrongRef(m_pTex2d);
		m_refSRV_DS->GetResourceState()->SetStreamState(SS_Valid);*/
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);
	//}
	
	return true;
}

NS_END