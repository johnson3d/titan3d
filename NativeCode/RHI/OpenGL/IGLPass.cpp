#include "IGLPass.h"
#include "IGLRenderContext.h"
#include "IGLCommandList.h"
#include "IGLRenderPipeline.h"
#include "IGLVertexBuffer.h"
#include "IGLIndexBuffer.h"
#include "IGLInputLayout.h"
#include "IGLTexture2D.h"
#include "IGLShaderResourceView.h"
#include "IGLRenderPipeline.h"
#include "IGLShaderProgram.h"
#include "IGLConstantBuffer.h"
#include "IGLSamplerState.h"
#include "IGLUnorderedAccessView.h"
#include "Utility/IMeshPrimitives.h"

#define new VNEW

NS_BEGIN

void GLSdk::BindVertexBuffer(IGLVertexBuffer* vb)
{
	std::shared_ptr<GLSdk::GLBufferId> bufferId;
	if (vb != nullptr)
	{
		bufferId = std::shared_ptr<GLSdk::GLBufferId>(vb->mGpuBuffer->mBufferId);
	}
	CMD_PUSH
	{
		if (bufferId == nullptr)
		{
			::glBindBuffer(GL_ARRAY_BUFFER, 0);
		}
		else
		{
			::glBindBuffer(GL_ARRAY_BUFFER, bufferId->BufferId);
		}
	}
	CMD_END(glBindVertexBuffer);
}

void GLSdk::BindIndexBuffer(IGLIndexBuffer* ib)
{
	std::shared_ptr<GLSdk::GLBufferId> bufferId;
	if (ib != nullptr)
	{
		bufferId = std::shared_ptr<GLSdk::GLBufferId>(ib->mGpuBuffer->mBufferId);
	}
	CMD_PUSH
	{
		if (bufferId == nullptr)
		{
			::glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
		}
		else
		{
			::glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, bufferId->BufferId);
		}
	}
	CMD_END(glBindIndexBuffer);
}

void GLSdk::BindConstantBuffer(UINT Index, IGLConstantBuffer* cb)
{
	std::shared_ptr<GLSdk::GLBufferId> bufferId;
	if (cb != nullptr)
	{
		bufferId = std::shared_ptr<GLSdk::GLBufferId>(cb->mBuffer);
	}
	CMD_PUSH
	{
		if (bufferId == nullptr)
		{
			::glBindBufferBase(GL_UNIFORM_BUFFER, Index, 0);
		}
		else
		{
			::glBindBufferBase(GL_UNIFORM_BUFFER, Index, bufferId->BufferId);
		}
	}
	CMD_END(glBindConstantBuffer);
}

void GLSdk::BindTexture2D(IGLShaderResourceView* srv)
{
	if (srv->mBuffer != nullptr)
	{
		ASSERT(false);
		return;
	}
	std::shared_ptr<GLSdk::GLBufferId> bufferId;
	if (srv != nullptr && srv->mTexture2D != nullptr)
	{
		bufferId = std::shared_ptr<GLSdk::GLBufferId>(srv->mTexture2D.UnsafeConvertTo<IGLTexture2D>()->mGlesTexture2D);
	}
	CMD_PUSH
	{
		if (bufferId == nullptr || bufferId->BufferId == 0)
		{
			mCurBindTexture = nullptr;
			return;
		}
		else
		{
			mCurBindTexture = bufferId;
			::glBindTexture(GL_TEXTURE_2D, bufferId->BufferId);
		}
	}
	CMD_END(glBindTexture2D);

#if _DEBUG
	this->PushCommand([=]()->void {
		if (bufferId == nullptr)
			return;
		auto debugInfo = bufferId->DebugInfo.GetPtr();
		if (debugInfo != nullptr)
		{
			auto pSRV = debugInfo.UnsafeConvertTo<IGLShaderResourceView>();
			if (pSRV != nullptr)
			{
				/*if (pSRV->mResourceFile.find("b3") != std::string::npos)
				{
					DWORD* pData = new DWORD[1024* 1024];
					memset(pData, 0, sizeof(DWORD)* 1024 * 1024);
					::glGetTexImage(GL_TEXTURE_2D, 0, GL_RGBA, GL_UNSIGNED_BYTE, pData);
					delete[] pData;
				}*/
			}
		}
	}, "GLSdk::BindTexture2D");
#endif
}

IGLDrawCall::IGLDrawCall()
{
	mVertexArray = 0;
}

IGLDrawCall::~IGLDrawCall()
{
	Cleanup();
}

void IGLDrawCall::Cleanup()
{
	mVertexArray.reset();
}

void IGLDrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	sdk->Viewport((GLint)vp->TopLeftX, (GLint)vp->TopLeftY, (GLint)vp->Width, (GLint)vp->Height);
	
	sdk->DepthRangef(vp->MinDepth, vp->MaxDepth);
}

void IGLDrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (sr == nullptr || sr->Rects.size() == 0)
	{
		sdk->Disable(GL_SCISSOR_TEST);
		return;
	}
	sdk->Enable(GL_SCISSOR_TEST);
	auto& rc = sr->Rects[0];
	sdk->Scissor(rc.MinX, rc.MinY, rc.MaxX - rc.MinX, rc.MaxY - rc.MinY);
}

//����Ⱦ״̬
void IGLDrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	((IGLRenderPipeline*)pipeline)->ApplyState(cmd);
}

void IGLDrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;

	if (VertexBuffer == nullptr)
	{
		//sdk->DisableVertexAttribArray((GLuint)StreamIndex);
		return;
	}

	if (m_pPipelineState == nullptr)
		return;
	auto gpuProgram = (IGLShaderProgram*)m_pPipelineState->GetGpuProgram();
	if (gpuProgram == nullptr)
		return;
	auto layout = gpuProgram->GetInputLayout();
	auto& vtxLayouts = layout->mDesc.Layouts;
	auto& mapper = gpuProgram->mVBSlotMapper;
	auto devStreamIndex = mapper[StreamIndex];
	if (devStreamIndex == 0xFFFFFFFF)
		return;

	auto glVB = (IGLVertexBuffer*)VertexBuffer;

	sdk->BindVertexBuffer(glVB);
	sdk->EnableVertexAttribArray((GLuint)devStreamIndex);

	if (vtxLayouts[StreamIndex].IsInstanceData)
		sdk->VertexAttribDivisor(devStreamIndex, 1);
	else
		sdk->VertexAttribDivisor(devStreamIndex, 0);

	GLint size;
	GLenum type;
	GLboolean normolized;
	bool isIntegerVertexAttrib;
	FormatToGLElement(vtxLayouts[StreamIndex].Format, size, type, normolized, isIntegerVertexAttrib);
	UINT_PTR ptrOffset = vtxLayouts[StreamIndex].AlignedByteOffset;
	const GLvoid* pointer = (const GLvoid*)ptrOffset;
	if (isIntegerVertexAttrib == TRUE)
	{
		sdk->VertexAttribIPointer(
			devStreamIndex,// attribute
			size,                  // size
			type,           // type
			VertexBuffer->mDesc.Stride, // stride
			pointer // array buffer offset
		);
	}
	else
	{
		sdk->VertexAttribPointer(
			devStreamIndex,// attribute
			size,                  // size
			type,           // type
			normolized,           // normalized?
			VertexBuffer->mDesc.Stride, // stride
			pointer // array buffer offset
		);
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

void IGLDrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	sdk->BindIndexBuffer((IGLIndexBuffer*)IndexBuffer);
	//sdk->BindBuffer(GL_ELEMENT_ARRAY_BUFFER, ((IGLIndexBuffer*)IndexBuffer)->mBuffer);
}

void IGLDrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	if (cmd->mCurrentState.TrySet_CBuffers(Index,CBuffer)==false)
		return;

	auto rCBuffer = (IGLConstantBuffer*)CBuffer;

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	sdk->BindConstantBuffer(Index, rCBuffer);
	//sdk->BindBufferBase(GL_UNIFORM_BUFFER, Index, rCBuffer->mBuffer);
}

void IGLDrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	/*auto rCBuffer = (IGLConstantBuffer*)CBuffer;
	glUniformBlockBinding(((IGLShaderProgram*)m_pGpuProgram)->mProgram, rCBuffer->Desc.PSBindPoint, Index);
	GLCheck;

	glBindBufferBase(GL_UNIFORM_BUFFER, Index, rCBuffer->mBuffer);
	GLCheck;*/
	//OpenGL UniformBuffer set in Program,VS has set first.
	return;
}

void IGLDrawCall::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	//RHI_ASSERT(false);

	if (Texture == nullptr)
		return;

	Texture->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (Texture->GetResourceState()->GetStreamState() != SS_Valid)
		return;

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	auto rTexture = (IGLShaderResourceView*)Texture;

	if (Texture->mSrvDesc.ViewDimension == RESOURCE_DIMENSION_BUFFER)
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
}

void IGLDrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	if (cmd->mCurrentState.TrySet_PSTextures(Index, Texture) == false)
		return;
	
	if (Texture == nullptr)
		return;

	Texture->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (Texture->GetResourceState()->GetStreamState() != SS_Valid)
		return;

	auto saved = GLSdk::CheckGLError;
	GLSdk::CheckGLError = false;

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	auto rTexture = (IGLShaderResourceView*)Texture;
	if (Texture->mSrvDesc.ViewDimension == RESOURCE_DIMENSION_BUFFER)
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

void IGLDrawCall::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	//RHI_ASSERT(false);
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (cmd->mCurrentState.TrySet_VSSampler(Index, Sampler) == false)
	{
		sdk->Uniform1i(Index, Index);
		return;
	}
	
	auto glSamp = (IGLSamplerState*)Sampler;

	//auto d11cmd = (IGLCommandList*)cmd;

	sdk->BindSampler(Index, glSamp->mSampler);
	sdk->Uniform1i(Index, Index);
}

void IGLDrawCall::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	
	if (cmd->mCurrentState.TrySet_PSSampler(Index, Sampler) == false)
	{
		sdk->Uniform1i(Index, Index);
		return;
	}

	auto glSamp = (IGLSamplerState*)Sampler;
	
	sdk->BindSampler(Index, glSamp->mSampler);

	sdk->Uniform1i(Index, Index);
}

void IGLDrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, NumPrimitives, &count);
	if (NumInstances == 1)
		sdk->DrawArrays(dpType, BaseVertexIndex, count);
	else
		sdk->DrawArraysInstanced(dpType, BaseVertexIndex, count, NumInstances);
}

void IGLDrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, NumPrimitives, &count);
	auto ib = MeshPrimitives->GetGeomtryMesh()->GetIndexBuffer();
	if (ib == nullptr)
		return;
	if (ib->mDesc.Type == IBT_Int16)
	{
		if (NumInstances == 1)
			sdk->DrawElements(dpType, count, GL_UNSIGNED_SHORT, (const GLvoid*)(StartIndex * sizeof(SHORT)));
		else
			sdk->DrawElementsInstanced(dpType, count, GL_UNSIGNED_SHORT, (const GLvoid*)(StartIndex * sizeof(SHORT)), NumInstances);
	}
	else
	{
		if (NumInstances == 1)
			sdk->DrawElements(dpType, count, GL_UNSIGNED_INT, (const GLvoid*)(StartIndex * sizeof(int)));
		else
			sdk->DrawElementsInstanced(dpType, count, GL_UNSIGNED_INT, (const GLvoid*)(StartIndex * sizeof(int)), NumInstances);
	}
}

void IGLDrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	ASSERT(IRenderContext::mChooseShaderModel > 3);
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	UINT count = 0;
	auto dpType = PrimitiveTypeToGL(PrimitiveType, 0, &count);
	auto ib = MeshPrimitives->GetGeomtryMesh()->GetIndexBuffer();
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

void IGLDrawCall::BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer)
{
	IDrawCall::BindCBufferVS(Index, CBuffer);
}

void IGLDrawCall::BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer)
{
	IDrawCall::BindCBufferPS(Index, CBuffer);
}

bool IGLDrawCall::Init(IRenderContext* rc, const IDrawCallDesc* desc)
{
	return true;
}

vBOOL IGLDrawCall::ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer)
{
	auto glCmd = ((IGLCommandList*)cmd);
	ApplyVertexArray(glCmd, bImmCBuffer);

	if (AttachVBs != nullptr)
		AttachVBs->ApplyVBs(cmd, this, bImmCBuffer);

	if (AttachIndexBuffer != nullptr)
	{
		SetIndexBuffer(cmd, AttachIndexBuffer);
	}
	else
	{
		auto geoMesh = MeshPrimitives->GetGeomtryMesh();
		if (geoMesh->GetIndexBuffer() != nullptr)
			this->SetIndexBuffer(cmd, geoMesh->GetIndexBuffer());
	}

	return TRUE;
}

void IGLDrawCall::ApplyVertexArray(IGLCommandList* cmd, vBOOL bImmCBuffer)
{
	auto geoMesh = MeshPrimitives->GetGeomtryMesh();
	if (geoMesh->mIsDirty == TRUE || mVertexArray == 0)
	{
		geoMesh->mIsDirty = FALSE;

		auto sdk = cmd->mCmdList;

		mVertexArray = sdk->GenVertexArrays();
		sdk->BindVertexArray(mVertexArray);

		geoMesh->ApplyGeometry(cmd, this, bImmCBuffer);
	}
	else
	{
		cmd->mCmdList->BindVertexArray(mVertexArray);
	}
}

NS_END