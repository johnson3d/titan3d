#include "IGLCommandList.h"
#include "IGLPass.h"
#include "IGLFrameBuffers.h"
#include "IGLRenderContext.h"
#include "IGLComputeShader.h"
#include "IGLUnorderedAccessView.h"
#include "IGLSwapChain.h"
#include "IGLShaderResourceView.h"
#include "IGLFrameBuffers.h"

#include "../../Base/vfxsampcounter.h"
#include "../Utility/GraphicsProfiler.h"

#define new VNEW

NS_BEGIN

IGLCommandList::IGLCommandList()
{
	
}


IGLCommandList::~IGLCommandList()
{
	Cleanup();
}

void IGLCommandList::Cleanup()
{
}

void IGLCommandList::BeginCommand()
{
	ICommandList::BeginCommand();
}

void IGLCommandList::EndCommand()
{
	ICommandList::EndCommand();
}

void IGLCommandList::BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer)
{
	if (mProfiler != nullptr && mProfiler->mNoPixelWrite)
	{
		pFrameBuffer = mProfiler->mOnePixelFB;
	}

	//mSavedFrameBuffer = mCmdList->GetIntegerv(GL_FRAMEBUFFER_BINDING);
	((IGLFrameBuffers*)pFrameBuffer)->ApplyBuffers(mCmdList);
	
	GLbitfield flag = 0;
	if (pRenderPassDesc->mFBLoadAction_Color == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_COLOR_BUFFER_BIT;
		mCmdList->ClearColor(pRenderPassDesc->mFBClearColorRT0.r, pRenderPassDesc->mFBClearColorRT0.g, pRenderPassDesc->mFBClearColorRT0.b, pRenderPassDesc->mFBClearColorRT0.a);

	}

	if (pRenderPassDesc->mFBLoadAction_Depth == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_DEPTH_BUFFER_BIT;
		mCmdList->ClearDepthf(pRenderPassDesc->mDepthClearValue);
	}

	if (pRenderPassDesc->mFBLoadAction_Stencil == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_STENCIL_BUFFER_BIT;
		mCmdList->ClearStencil(pRenderPassDesc->mStencilClearValue);
	}

	if (flag != 0)
	{
		mCmdList->Clear(flag);
	}
}
//
//void IGLCommandList::BuildRenderPass(vBOOL bImmCBuffer, IPass** ppPass)
//{
//	AUTO_SAMP("Native.IPass.BuildPass");
//	for (auto i = mPassArray.begin(); i != mPassArray.end(); i++)
//	{
//		(*i)->BuildPass(this, bImmCBuffer);
//		if (ppPass != nullptr)
//		{
//			*ppPass = *i;
//		}
//	}	
//}

void IGLCommandList::EndRenderPass()
{
	mDrawCall = 0;
	mDrawTriangle = 0;
	ICommandList::EndRenderPass();
	
	//mCmdList->BindFramebuffer(GL_FRAMEBUFFER, mSavedFrameBuffer);
	//mSavedFrameBuffer.reset();
}


void IGLCommandList::Blit2DefaultFrameBuffer(IFrameBuffers* FrameBuffers, int dstWidth, int dstHeight)
{
	IGLFrameBuffers *fbo = (IGLFrameBuffers*)FrameBuffers;
	if (fbo == NULL)
		return;

	mCmdList->BindFramebuffer(GL_READ_FRAMEBUFFER, fbo->mFrameBufferId);
	mCmdList->BindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
	/*auto rtv = */fbo->GetRenderTargetView(0);	
	mCmdList->BlitFramebuffer(0, 0, dstWidth, dstHeight, 0, 0, dstWidth, dstHeight, GL_COLOR_BUFFER_BIT, GL_NEAREST);
	//mCmdList->BlitFramebuffer(0, 0, swapChain->mDesc.Width, swapChain->mDesc.Height, 0, 0, swapChain->mDesc.Width, swapChain->mDesc.Height, GL_COLOR_BUFFER_BIT, GL_NEAREST);
}

void IGLCommandList::Commit(IRenderContext* pRHICtx)
{
	if (pRHICtx == NULL)
	{
		return;
	}

	if (this->mCmdList->mWriteCmds)
	{
		_vfxTraceA("IGLCommandList::Execute Begin = %d", mCmdList->GetCommandNumber());
	}
	
	mCmdCount = (UINT)mCmdList->mCommands.size();

	{
		mCmdList->Execute();
	}
	
	if (this->mCmdList->mWriteCmds)
	{
		_vfxTraceA("IGLCommandList::Execute End");
	}

	this->mCurrentState.Reset();

	/*GLSdk* refImmCmdList = ((IGLRenderContext*)pRHICtx)->mImmCmdList;
	if (refImmCmdList->GetCommandNumber() > 0)
	{
		refImmCmdList->Execute();
	}*/

	/*if (pSwapChain != nullptr)
	{
		IGLSwapChain* refSwapChain = ((IGLSwapChain*)pSwapChain);
		refSwapChain->BindCurrent();
		refSwapChain->Present(0, 0);
	}*/

	/*ICommandList::BeginCommand();

	mFrameBuffers->ApplyBuffers();

	ImplClearMRT();
	mDrawCall = 0;
	mDrawTriangle = 0;
	for (auto i = mPassArray.begin(); i != mPassArray.end(); i++)
	{
		(*i)->BuildPass(this);
	}

	glBindFramebuffer(GL_FRAMEBUFFER, 0);
	GLCheck;
	ICommandList::EndCommand();*/
}

bool IGLCommandList::Init(IGLRenderContext* rc, const ICommandListDesc* desc, GLSdk* sdk)
{
	if (sdk == nullptr)
	{
		mCmdList = new GLSdk();
	}
	else
	{
		mCmdList.StrongRef(sdk);
	}
	mRHIContext.FromObject(rc);
	return true;
}

void IGLCommandList::SetComputeShader(IComputeShader* ComputerShader)
{
	auto glCS = (IGLComputeShader*)ComputerShader;
	if(glCS!=nullptr)
		mCmdList->UseProgram(glCS->mProgram, nullptr);
	else
		mCmdList->UseProgram(0, nullptr);
}

void IGLCommandList::CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{
	auto saved = GLSdk::CheckGLError;
	GLSdk::CheckGLError = false;

	auto rTexture = (IGLShaderResourceView*)Texture;

	mCmdList->ActiveTexture(GL_TEXTURE0 + Index);

	mCmdList->BindTexture2D(rTexture);
	//mCmdList->BindTexture(GL_TEXTURE_2D, rTexture->mView);
	mCmdList->Uniform1i(Index, Index);

	GLSdk::CheckGLError = saved;
}

void IGLCommandList::CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts)
{
	auto glView = ((IGLUnorderedAccessView*)view);
	
	GLenum target = GL_SHADER_STORAGE_BUFFER;// GL_UNIFORM_BUFFER;
	/*if(glView->mBuffer->mTarget == GL_SHADER_STORAGE_BUFFER)
		mCmdList->BindBufferBase(GL_SHADER_STORAGE_BUFFER, Index, glView->mBuffer->mBufferId);*/
	
	//mCmdList->BindBuffer(GL_SHADER_STORAGE_BUFFER, glView->mBuffer->mBufferId);//glView->mBuffer->mTarget
	if (view == nullptr)
	{
		//mCmdList->BindBuffer(target, 0);
		mCmdList->BindBufferBase(target, Index, 0);
	}
	else
	{
		//mCmdList->BindBuffer(target, glView->mBuffer->mBufferId);
		mCmdList->BindBufferBase(target, Index, glView->mBuffer->mBufferId);
	}
}

void IGLCommandList::CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
{
	auto rCBuffer = (IGLConstantBuffer*)cbuffer;

	mCmdList->BindConstantBuffer(Index, rCBuffer);
}

void IGLCommandList::CSDispatch(UINT x, UINT y, UINT z)
{
	if (IRenderContext::mChooseShaderModel < 4)
	{
		ASSERT(false);
		return;
	}
	mCmdList->ES31_DispatchCompute(x, y, z);
}

vBOOL IGLCommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	AutoRef<ITexture2D> srcTex2D;
	srcTex2D.StrongRef(src->GetTexture2D());

	IGLTexture2D* pGLTexture = (IGLTexture2D*)(*ppTexture);
	bool needCreateTexture = false;
	if (pGLTexture == nullptr)
	{
		needCreateTexture = true;
	}
	else
	{
		if (pGLTexture->mDesc.Width != srcTex2D->mDesc.Width ||
			pGLTexture->mDesc.Height != srcTex2D->mDesc.Height)
		{
			needCreateTexture = true;
		}
	}

	GLSdk* sdk = mCmdList;
	UINT RowPitch = ((srcTex2D->mDesc.Width * GetPixelByteWidth(srcTex2D->mDesc.Format) + 3) / 4) * 4;
	GLsizeiptr PboSize = RowPitch * srcTex2D->mDesc.Height;

	std::shared_ptr<GLSdk::GLBufferId> pBufferId;
	if (needCreateTexture)
	{
		pBufferId = std::shared_ptr<GLSdk::GLBufferId>(sdk->GenBufferId());
		sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, pBufferId);
		sdk->BufferData(GL_PIXEL_PACK_BUFFER, PboSize, 0, GL_STATIC_READ);
		sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, 0);
	}
	else
	{
		pBufferId = pGLTexture->mGlesTexture2D;
	}

	{
		((IGLFrameBuffers*)pFrameBuffers)->ApplyBuffers(sdk);

		sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, pBufferId);
		{
			GLint internalFormat;
			GLint format;
			GLenum type;
			FormatToGL(srcTex2D->mDesc.Format, internalFormat, format, type);
			sdk->ReadPixels(0, 0, srcTex2D->mDesc.Width, srcTex2D->mDesc.Height, format, GL_UNSIGNED_BYTE, 0);
		}

		sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, 0);
		//sdk->BindFramebuffer(GL_FRAMEBUFFER, 0);
	}

	if (needCreateTexture)
	{
		Safe_Release(pGLTexture);
		pGLTexture = new IGLTexture2D();
		pGLTexture->mDesc = srcTex2D->mDesc;
		pGLTexture->mIsReadable = true;
		pGLTexture->mGlesTexture2D = pBufferId;

		*ppTexture = pGLTexture;
	}
	
	return TRUE;
}

NS_END