#include "IGLCommandList.h"
#include "IGLPass.h"
#include "IGLFrameBuffers.h"
#include "IGLRenderContext.h"
#include "IGLComputeShader.h"
#include "IGLUnorderedAccessView.h"
#include "IGLSwapChain.h"
#include "IGLShaderResourceView.h"
#include "IGLFrameBuffers.h"
#include "IGLSamplerState.h"
#include "IGLShaderProgram.h"
#include "IGLInputLayout.h"
#include "IGLVertexShader.h"
#include "IGLPixelShader.h"
#include "IGLRenderPipeline.h"
#include "../Utility/IMeshPrimitives.h"
#include "../Utility/IGeometryMesh.h"

#include "../../Base/vfxsampcounter.h"
#include "Utility/GraphicsProfiler.h"

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

bool IGLCommandList::BeginCommand()
{
	ICommandList::BeginCommand();
	return true;
}

void IGLCommandList::EndCommand()
{
	ICommandList::EndCommand();
}

bool IGLCommandList::BeginRenderPass(IFrameBuffers* pFrameBuffer, const IRenderPassClears* passClears, const char* debugName)
{
	IRenderPass* pRenderPass = pFrameBuffer->mRenderPass;
	if (passClears == nullptr)
		return true;

	auto pRenderPassDesc = &pRenderPass->mDesc;
	
	if (mProfiler != nullptr && mProfiler->mNoPixelWrite)
	{
		pFrameBuffer = mProfiler->mOnePixelFB;
	}

	//mSavedFrameBuffer = mCmdList->GetIntegerv(GL_FRAMEBUFFER_BINDING);
	((IGLFrameBuffers*)pFrameBuffer)->ApplyBuffers(mCmdList);
	
	GLbitfield flag = 0;
	if (pRenderPassDesc->AttachmentMRTs[0].LoadAction == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_COLOR_BUFFER_BIT;
		mCmdList->ClearColor(passClears->ClearColor[0].r,
			passClears->ClearColor[0].g,
			passClears->ClearColor[0].b,
			passClears->ClearColor[0].a);

	}

	if (pRenderPassDesc->AttachmentMRTs[0].LoadAction == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_DEPTH_BUFFER_BIT;
		mCmdList->ClearDepthf(passClears->DepthClearValue);
	}

	if (pRenderPassDesc->AttachmentMRTs[0].StencilLoadAction == FrameBufferLoadAction::LoadActionClear)
	{
		flag |= GL_STENCIL_BUFFER_BIT;
		mCmdList->ClearStencil(passClears->StencilClearValue);
	}

	if (flag != 0)
	{
		mCmdList->Clear(flag);
	}
	return true;
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
	
	if (mPipelineStat != nullptr)
		mPipelineStat->mCmdCount = (UINT)mCmdList->mCommands.size();

	{
		GLSdk::ImmSDK->Execute();
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

void IGLCommandList::CSDispatchIndirect(IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	if (IRenderContext::mChooseShaderModel < 4)
	{
		ASSERT(false);
		return;
	}
	auto sdk = mCmdList;
	auto buffer = (IGLGpuBuffer*)pBufferForArgs;
	sdk->BindBuffer(GL_DISPATCH_INDIRECT_BUFFER, buffer->mBufferId);
	mCmdList->ES31_DispatchComputeIndirect(AlignedByteOffsetForArgs);
}

void IGLCommandList::SetScissorRect(IScissorRect* sr)
{
	auto sdk = mCmdList;
	if (sr == nullptr || sr->Rects.size() == 0)
	{
		sdk->Disable(GL_SCISSOR_TEST);
		return;
	}
	sdk->Enable(GL_SCISSOR_TEST);
	auto& rc = sr->Rects[0];
	sdk->Scissor(rc.MinX, rc.MinY, rc.MaxX - rc.MinX, rc.MaxY - rc.MinY);
}

void IGLCommandList::SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	/*if (cmd->IsDoing() == false)
			return;*/

	auto sdk = mCmdList;

	if (VertexBuffer == nullptr)
	{
		//sdk->DisableVertexAttribArray((GLuint)StreamIndex);
		return;
	}

	if (mPipelineState == nullptr)
		return;
	auto gpuProgram = (IGLShaderProgram*)mPipelineState->GetGpuProgram();
	if (gpuProgram == nullptr)
		return;

	auto glVB = (IGLVertexBuffer*)VertexBuffer;
	sdk->BindVertexBuffer(glVB);
	
	auto layout = gpuProgram->GetInputLayout();
	auto& vtxLayouts = layout->mDesc->Layouts;
	for (size_t i = 0; i < vtxLayouts.size(); i++)
	{
		const auto& elem = vtxLayouts[i];
		if (elem.GLAttribLocation == -1)
			continue;
		
		if (elem.InputSlot == StreamIndex)
		{
			sdk->EnableVertexAttribArray((GLuint)elem.GLAttribLocation);
			if (elem.IsInstanceData)
				sdk->VertexAttribDivisor(elem.GLAttribLocation, 1);
			else
				sdk->VertexAttribDivisor(elem.GLAttribLocation, 0);

			GLint size;
			GLenum type;
			GLboolean normolized;
			bool isIntegerVertexAttrib;
			FormatToGLElement(elem.Format, size, type, normolized, isIntegerVertexAttrib);
			UINT_PTR ptrOffset = elem.AlignedByteOffset;
			const GLvoid* pointer = (const GLvoid*)ptrOffset;
			if (isIntegerVertexAttrib == TRUE)
			{
				sdk->VertexAttribIPointer(
					elem.GLAttribLocation,// attribute
					size,                  // size
					type,           // type
					Stride,//VertexBuffer->mDesc.Stride, // stride
					pointer // array buffer offset
				);
			}
			else
			{
				sdk->VertexAttribPointer(
					elem.GLAttribLocation,// attribute
					size,                  // size
					type,           // type
					normolized,           // normalized?
					Stride,//VertexBuffer->mDesc.Stride, // stride
					pointer // array buffer offset
				);
			}
		}
	}

	//for (size_t i = 0; i < vtxLayouts.size(); i++)
	//{
	//	if (vtxLayouts[i].InputSlot == StreamIndex)
	//	{
	//		GLuint devStreamIndex = StreamIndex;
	//		auto& mapper = ((IGLShaderProgram*)m_pGpuProgram)->mVBSlotMapper;
	//		devStreamIndex = mapper[StreamIndex];
	//		if (devStreamIndex == 0xFFFFFFFF)
	//			continue;
	//		auto glVB = (IGLVertexBuffer*)VertexBuffer;

	//		sdk->BindVertexBuffer(glVB);
	//		sdk->EnableVertexAttribArray((GLuint)devStreamIndex);

	//		if (vtxLayouts[i].IsInstanceData)
	//			sdk->VertexAttribDivisor(devStreamIndex, 1);
	//		else
	//			sdk->VertexAttribDivisor(devStreamIndex, 0);

	//		GLint size;
	//		GLenum type;
	//		GLboolean normolized;
	//		bool isIntegerVertexAttrib;
	//		FormatToGLElement(vtxLayouts[i].Format, size, type, normolized, isIntegerVertexAttrib);
	//		UINT_PTR ptrOffset = vtxLayouts[i].AlignedByteOffset;
	//		const GLvoid* pointer = (const GLvoid*)ptrOffset;
	//		if (isIntegerVertexAttrib == TRUE)
	//		{
	//			sdk->VertexAttribIPointer(
	//				devStreamIndex,// attribute
	//				size,                  // size
	//				type,           // type
	//				VertexBuffer->mDesc.Stride, // stride
	//				pointer // array buffer offset
	//			);
	//		}
	//		else
	//		{
	//			sdk->VertexAttribPointer(
	//				devStreamIndex,// attribute
	//				size,                  // size
	//				type,           // type
	//				normolized,           // normalized?
	//				VertexBuffer->mDesc.Stride, // stride
	//				pointer // array buffer offset
	//			);
	//		}
	//	}
	//}
	//auto elem = layout->GetElement(StreamIndex);
	//if (elem == nullptr)
	//	return;
	//glEnableVertexAttribArray(StreamIndex);
	//GLCheck;
	//glBindBuffer(GL_ARRAY_BUFFER, ((IGLVertexBuffer*)VertexBuffer)->mBuffer);
	//GLCheck;
	//GLint size;
	//GLenum type;
	//GLboolean normolized;
	//FormatToGLElement(elem->Format, size, type, normolized);
	//glVertexAttribPointer(
	//	StreamIndex,// attribute
	//	size,                  // size
	//	type,           // type
	//	normolized,           // normalized?
	//	VertexBuffer->mDesc.Stride, // stride
	//	(const void*)elem->AlignedByteOffset // array buffer offset
	//);
	//GLCheck;
}

void IGLCommandList::SetIndexBuffer(IIndexBuffer* IndexBuffer)
{
	/*if (cmd->IsDoing() == false)
			return;*/

	mIndexBuffer.StrongRef(IndexBuffer);
	auto sdk = mCmdList;
	sdk->BindIndexBuffer((IGLIndexBuffer*)IndexBuffer);
	//sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, ((IGLIndexBuffer*)IndexBuffer)->mBuffer);
}

void IGLCommandList::DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
			return;*/

	auto sdk = mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, NumPrimitives, &count);
	if (NumInstances == 1)
		sdk->DrawArrays(dpType, BaseVertexIndex, count);
	else
		sdk->DrawArraysInstanced(dpType, BaseVertexIndex, count, NumInstances);
}

void IGLCommandList::DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
			return;*/

	auto sdk = mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, NumPrimitives, &count);
	auto ib = mIndexBuffer;
	if (ib == nullptr)
		return;
	if (ib->mDesc.Type == IBT_Int16)
	{
		if (NumInstances == 1)
			sdk->ES32_DrawElementsBaseVertex(dpType, count, GL_UNSIGNED_SHORT, (const GLvoid*)(StartIndex * sizeof(SHORT)), BaseVertexIndex);
		else
			sdk->ES32_DrawElementsInstancedBaseVertex(dpType, count, GL_UNSIGNED_SHORT, (const GLvoid*)(StartIndex * sizeof(SHORT)), NumInstances, BaseVertexIndex);
	}
	else
	{
		if (NumInstances == 1)
			sdk->ES32_DrawElementsBaseVertex(dpType, count, GL_UNSIGNED_INT, (const GLvoid*)(StartIndex * sizeof(int)), BaseVertexIndex);
		else
			sdk->ES32_DrawElementsInstancedBaseVertex(dpType, count, GL_UNSIGNED_INT, (const GLvoid*)(StartIndex * sizeof(int)), NumInstances, BaseVertexIndex);
	}
}

void IGLCommandList::DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	ASSERT(IRenderContext::mChooseShaderModel > 3);
	auto sdk = mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, 0, &count);
	auto ib = mIndexBuffer;
	if (ib == nullptr)
		return;

	auto pGLArgBuffer = (IGLGpuBuffer*)pBufferForArgs;
	sdk->BindBuffer(GL_DRAW_INDIRECT_BUFFER, pGLArgBuffer->mBufferId);
	if (ib->mDesc.Type == IBT_Int16)
	{
		sdk->ES31_DrawElementsIndirect(dpType, GL_UNSIGNED_SHORT, (void*)((size_t)AlignedByteOffsetForArgs));
	}
	else
	{
		sdk->ES31_DrawElementsIndirect(dpType, GL_UNSIGNED_INT, (void*)((size_t)AlignedByteOffsetForArgs));
	}
	sdk->BindBuffer(GL_DRAW_INDIRECT_BUFFER, 0);
}

void IGLCommandList::IASetInputLayout(IInputLayout* pInputLayout)
{

}

void IGLCommandList::SetRasterizerState(IRasterizerState* State)
{

}

void IGLCommandList::SetDepthStencilState(IDepthStencilState* State)
{

}

void IGLCommandList::SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask)
{

}

void IGLCommandList::VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances)
{
	auto program = (IGLShaderProgram*)mPipelineState->GetGpuProgram();
	auto sdk = mCmdList;
	auto glvs = (IGLVertexShader*)pVertexShader;
	sdk->AttachShader(program->mProgram, glvs->mShader);
	sdk->LinkProgram(program->mProgram, program);
	sdk->UseProgram(program->mProgram, this);
}

void IGLCommandList::PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances)
{
	auto program = (IGLShaderProgram*)mPipelineState->GetGpuProgram();
	auto sdk = mCmdList;
	auto glps = (IGLPixelShader*)pPixelShader;
	sdk->AttachShader(program->mProgram, glps->mShader);
	sdk->LinkProgram(program->mProgram, program);
	sdk->UseProgram(program->mProgram, this);
}

void IGLCommandList::SetViewport(IViewPort* vp)
{
	auto sdk = mCmdList;
	sdk->Viewport((GLint)vp->TopLeftX, (GLint)vp->TopLeftY, (GLint)vp->Width, (GLint)vp->Height);

	sdk->DepthRangef(vp->MinDepth, vp->MaxDepth);
}

void IGLCommandList::VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
			return;*/

	if (mCurrentState.TrySet_CBuffers(Index, CBuffer) == false)
		return;

	auto rCBuffer = (IGLConstantBuffer*)CBuffer;

	auto sdk = mCmdList;
	sdk->BindConstantBuffer(Index, rCBuffer);
	//sdk->BindBufferBase(GL_UNIFORM_BUFFER, Index, rCBuffer->mBuffer);
}

void IGLCommandList::PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer) 
{
	/*if (cmd->IsDoing() == false)
			return;*/

	/*auto rCBuffer = (IGLConstantBuffer*)CBuffer;
	glUniformBlockBinding(((IGLShaderProgram*)m_pGpuProgram)->mProgram, rCBuffer->Desc.PSBindPoint, Index);
	GLCheck;

	glBindBufferBase(GL_UNIFORM_BUFFER, Index, rCBuffer->mBuffer);
	GLCheck;*/
	//OpenGL UniformBuffer set in Program,VS has set first.
}

void IGLCommandList::VSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{

}

void IGLCommandList::PSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{
	if (mCurrentState.TrySet_PSTextures(Index, Texture) == false)
		return;

	if (Texture == nullptr)
		return;

	Texture->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (Texture->GetResourceState()->GetStreamState() != SS_Valid)
		return;

	auto saved = GLSdk::CheckGLError;
	GLSdk::CheckGLError = false;

	auto sdk = mCmdList;
	auto rTexture = (IGLShaderResourceView*)Texture;
	if (Texture->mSrvDesc.ViewDimension == SRV_DIMENSION_BUFFER)
	{
		GLenum target = GL_SHADER_STORAGE_BUFFER;// GL_UNIFORM_BUFFER;
		auto pGpuBuffer = rTexture->mBuffer.UnsafeConvertTo<IGLGpuBuffer>();
		sdk->BindBufferBase(target, Index, pGpuBuffer->mBufferId);
	}
	else
	{
		sdk->ActiveTexture(GL_TEXTURE0 + Index);

		sdk->BindTexture2D(rTexture);
		//sdk->BindTexture(GL_TEXTURE_2D, rTexture->mView);
		sdk->Uniform1i(Index, Index);
	}

	GLSdk::CheckGLError = saved;
}

void IGLCommandList::VSSetSampler(UINT32 Index, ISamplerState* Sampler)
{

}

void IGLCommandList::PSSetSampler(UINT32 Index, ISamplerState* Sampler)
{
	auto sdk = mCmdList;

	if (mCurrentState.TrySet_PSSampler(Index, Sampler) == false)
	{
		sdk->Uniform1i(Index, Index);
		return;
	}

	auto glSamp = (IGLSamplerState*)Sampler;

	sdk->BindSampler(Index, glSamp->mSampler);

	sdk->Uniform1i(Index, Index);
}

void IGLCommandList::SetRenderPipeline(IRenderPipeline* pipeline, EPrimitiveType dpType)
{
	mPipelineState.StrongRef(pipeline);
	((IGLRenderPipeline*)pipeline)->ApplyState(this);
	pipeline->GetGpuProgram()->ApplyShaders(this);	
}

vBOOL IGLCommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	if (src->mSrvDesc.Type != ST_Texture2D)
		return FALSE;
	auto srcTex2D = src->mBuffer.UnsafeConvertTo<IGLTexture2D>();

	IGLTexture2D* pGLTexture = (IGLTexture2D*)(*ppTexture);
	bool needCreateTexture = false;
	if (pGLTexture == nullptr)
	{
		needCreateTexture = true;
	}
	else
	{
		if (pGLTexture->mTextureDesc.Width != srcTex2D->mTextureDesc.Width ||
			pGLTexture->mTextureDesc.Height != srcTex2D->mTextureDesc.Height)
		{
			needCreateTexture = true;
		}
	}

	GLSdk* sdk = mCmdList;
	UINT RowPitch = ((srcTex2D->mTextureDesc.Width * GetPixelByteWidth(srcTex2D->mTextureDesc.Format) + 3) / 4) * 4;
	GLsizeiptr PboSize = RowPitch * srcTex2D->mTextureDesc.Height;

	std::shared_ptr<GLSdk::GLBufferId> pBufferId;
	if (needCreateTexture)
	{
		pBufferId = std::shared_ptr<GLSdk::GLBufferId>(sdk->GenBufferId());
		/*sdk->PushCommand([=]()->void
		{
			int xxx = 0;
		}, nullptr);*/
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
			FormatToGL(srcTex2D->mTextureDesc.Format, internalFormat, format, type);
			sdk->ReadPixels(0, 0, srcTex2D->mTextureDesc.Width, srcTex2D->mTextureDesc.Height, format, GL_UNSIGNED_BYTE, 0);
		}

		sdk->BindBuffer(GL_PIXEL_PACK_BUFFER, 0);
		//sdk->BindFramebuffer(GL_FRAMEBUFFER, 0);
	}

	if (needCreateTexture)
	{
		Safe_Release(pGLTexture);
		pGLTexture = new IGLTexture2D();
		pGLTexture->mTextureDesc = srcTex2D->mTextureDesc;
		pGLTexture->mIsReadable = true;
		pGLTexture->mGlesTexture2D = pBufferId;

		*ppTexture = pGLTexture;
	}
	
	return TRUE;
}

NS_END