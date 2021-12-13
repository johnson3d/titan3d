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
	if (srv != nullptr && srv->mBuffer != nullptr)
	{
		bufferId = *(std::shared_ptr<GLSdk::GLBufferId>*)srv->mBuffer->GetHWBuffer();
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
		auto debugInfo = (IGLShaderResourceView*)bufferId->DebugInfo.GetPtr();
		if (debugInfo != nullptr)
		{
			auto pSRV = debugInfo;
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
	
}

IGLDrawCall::~IGLDrawCall()
{
	Cleanup();
}

void IGLDrawCall::Cleanup()
{
	
}

void IGLDrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
{
	cmd->SetViewport(vp);
}

void IGLDrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{
	cmd->SetScissorRect(sr);
}

void IGLDrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline, EPrimitiveType dpType)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	cmd->SetRenderPipeline(pipeline, dpType);
	//((IGLRenderPipeline*)pipeline)->ApplyState(cmd);
}

void IGLDrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	cmd->SetVertexBuffer(StreamIndex, VertexBuffer, Offset, Stride);
}

void IGLDrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{
	cmd->SetIndexBuffer(IndexBuffer);
}

void IGLDrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	cmd->VSSetConstantBuffer(Index, CBuffer);
}

void IGLDrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	cmd->PSSetConstantBuffer(Index, CBuffer);
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
}

void IGLDrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	cmd->PSSetShaderResource(Index, Texture);
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
	cmd->PSSetSampler(Index, Sampler);
}

void IGLDrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	cmd->DrawPrimitive(PrimitiveType, BaseVertexIndex, NumPrimitives, NumInstances);
}

void IGLDrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	cmd->DrawIndexedPrimitive(PrimitiveType, BaseVertexIndex, StartIndex, NumPrimitives, NumInstances);
}

void IGLDrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	cmd->DrawIndexedInstancedIndirect(PrimitiveType, pBufferForArgs, AlignedByteOffsetForArgs);
}

bool IGLDrawCall::Init(IRenderContext* rc, const IDrawCallDesc* desc)
{
	return true;
}

vBOOL IGLGeometryMesh::ApplyGeometry(ICommandList* cmd, IDrawCall* pass, vBOOL bImm)
{
	auto sdk = ((IGLCommandList*)cmd)->mCmdList;
	if (mIsDirty == TRUE || mVertexArray == 0)
	{
		mIsDirty = FALSE;

		mVertexArray = sdk->GenVertexArrays();
		sdk->BindVertexArray(mVertexArray);

		IGeometryMesh::ApplyGeometry(cmd, pass, bImm);
	}
	else
	{
		sdk->BindVertexArray(mVertexArray);
	}

	if (GetIndexBuffer() != nullptr)
		cmd->SetIndexBuffer(GetIndexBuffer());

	return TRUE;
}

vBOOL IGLDrawCall::ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer)
{
	auto geoMesh = MeshPrimitives->GetGeomtryMesh();
	geoMesh->ApplyGeometry(cmd, this, bImmCBuffer);
	
	//auto glCmd = ((IGLCommandList*)cmd);

	if (AttachVBs != nullptr)
		AttachVBs->ApplyVBs(cmd, this, bImmCBuffer);

	if (AttachIndexBuffer != nullptr)
	{
		SetIndexBuffer(cmd, AttachIndexBuffer);
	}

	return TRUE;
}

void IGLComputeDrawcall::BuildPass(ICommandList* cmd)
{

}

bool IGLComputeDrawcall::Init(IRenderContext* rc)
{
	return true;
}

NS_END