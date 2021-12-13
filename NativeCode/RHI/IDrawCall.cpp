#include "IDrawCall.h"
#include "IConstantBuffer.h"
#include "IRenderPipeline.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "IComputeShader.h"
#include "ICommandList.h"
#include "IShaderResourceView.h"
#include "ISamplerState.h"
#include "IShaderProgram.h"
#include "IUnorderedAccessView.h"
#include "IRasterizerState.h"
#include "Utility/GraphicsProfiler.h"
#include "../Base/vfxsampcounter.h"
#include "Utility/IMeshPrimitives.h"
#include "ShaderReflector.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IViewPort, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IScissorRect, EngineNS::VIUnknown);

void IScissorRect::SetRectNumber(UINT num) 
{
	Rects.resize(num);
}

void IScissorRect::SetSCRect(UINT idx, int left, int top, int right, int bottom) 
{
	if (idx >= (UINT)Rects.size())
		return;
	auto rc = &Rects[idx];
	rc->MinX = left;
	rc->MinY = top;
	rc->MaxX = right;
	rc->MaxY = bottom;
}

void IScissorRect::GetSCRect(UINT idx, SRRect* pRect) 
{
	if (idx >= (UINT)Rects.size())
		return;
	auto rc = &Rects[idx];
	pRect->MinX = rc->MinX;
	pRect->MinY = rc->MinY;
	pRect->MaxX = rc->MaxX;
	pRect->MaxY = rc->MaxY;
}

IDrawCall::IDrawCall()
{
	AtomIndex = 0;
	LodLevel = 0;
	NumInstances = 1;

	IndirectDrawOffsetForArgs = 0;
}

IDrawCall::~IDrawCall()
{
	
}

//#pragma optimize("atgwy", on)
//#pragma auto_inline(on)

void IDrawCall::SetInstanceNumber(int instNum) 
{
	NumInstances = instNum;
}

void IDrawCall::SetIndirectDraw(IGpuBuffer* pBuffer, UINT offset)
{
	IndirectDrawArgsBuffer.StrongRef(pBuffer);
	IndirectDrawOffsetForArgs = offset;
}

void IDrawCall::BindGeometry(IMeshPrimitives* mesh, UINT atom, float lod)
{
	MeshPrimitives.StrongRef(mesh);

	AtomIndex = atom;

	SetLod(lod);
}

void IDrawCall::SetLod(float lod)
{
	LodLevel = MeshPrimitives->GetLodLevel(AtomIndex, lod);
}

float IDrawCall::GetLod()
{
	if (LodLevel == 0)
		return 0;
	auto num = (float)(MeshPrimitives->GetAtomLOD(AtomIndex) - 1);
	return (float)LodLevel / num;
}

void IDrawCall::GetDrawPrimitive(DrawPrimitiveDesc* desc) 
{
	if (MeshPrimitives == nullptr)
		return;
	*desc = *MeshPrimitives->GetAtom(AtomIndex, LodLevel);
	desc->NumInstances = NumInstances;
}

bool IsSameScissors(IScissorRect* lh, IScissorRect* rh)
{
	if (lh == rh)
		return true;
	
	if (rh != nullptr && lh != nullptr)
	{
		if (lh->Rects.size() != rh->Rects.size())
			return false;
		for (size_t i = 0; i < lh->Rects.size(); i++)
		{
			if (lh->Rects[i].MinX != rh->Rects[i].MinX ||
				lh->Rects[i].MinY != rh->Rects[i].MinY ||
				lh->Rects[i].MaxX != rh->Rects[i].MaxX ||
				lh->Rects[i].MaxY != rh->Rects[i].MaxY)
				return false;
		}
		return true;
	}
	return false;
}

void IDrawCall::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	BuildPassDefault(cmd, bImmCBuffer);

	if (mShaderCBufferBinder != nullptr)
	{
		mShaderCBufferBinder->IsDirty = false;
	}
	if (mShaderSrvBinder != nullptr)
	{
		mShaderSrvBinder->IsDirty = false;
	}
	if (mShaderSamplerBinder != nullptr)
	{
		mShaderSamplerBinder->IsDirty = false;
	}
}

void IDrawCall::BuildPassDefault(ICommandList* cmd, vBOOL bImmCBuffer)
{
	if (mPipelineState == nullptr)
	{
		return;
	}
	if (mPipelineState->GetGpuProgram()->GetVertexShader() == nullptr)
	{
		VFX_LTRACE(ELTT_Graphics, "VertexShader is null\r\n");
		return;
	}

	const DrawPrimitiveDesc* dpDesc = nullptr;
	{
		AUTO_SAMP("Native.IPass.BuildPass.Geometry");
		vBOOL applyOK = ApplyGeomtry(cmd, bImmCBuffer);
		if (applyOK == FALSE)
			return;
		dpDesc = MeshPrimitives->GetAtom(AtomIndex, LodLevel);
		if (dpDesc == nullptr)
			return;
	}

	{
		AUTO_SAMP("Native.IPass.BuildPass.ShaderProgram");
		/*SetViewport(cmd, m_pViewport);

		if (m_pPipelineState->GetRasterizerState()->mDesc.ScissorEnable != 0 && ScissorRect == nullptr)
		{
			IScissorRect sr;
			sr.SetRectNumber(1);
			sr.SetSCRect(0, (int)m_pViewport->TopLeftX, (int)m_pViewport->TopLeftY,
				(int)m_pViewport->TopLeftX + (int)m_pViewport->Width,
				(int)m_pViewport->TopLeftY + (int)m_pViewport->Height);
			SetScissorRect(cmd, &sr);
		}
		else
		{
			if (cmd->mCurrentState.TrySet_ScissorRect(ScissorRect))
			{
				SetScissorRect(cmd, ScissorRect);
			}
		}*/
		SetPipeline(cmd, mPipelineState, dpDesc->PrimitiveType);
		mPipelineState->mIsDirty = false;

		auto gpuProgram = mPipelineState->GetGpuProgram();
		gpuProgram->ApplyShaders(cmd);

		auto vs = gpuProgram->GetVertexShader();
		if (vs && mShaderCBufferBinder != nullptr)
		{
			for (const auto& i : mShaderCBufferBinder->VSResources)
			{
				if (i.second == nullptr)
					continue;
				i.second->UpdateDrawPass(cmd, bImmCBuffer);
				VSSetConstantBuffer(cmd, i.first, i.second);
			}
		}
		auto ps = gpuProgram->GetPixelShader();
		if (ps && mShaderCBufferBinder != nullptr)
		{
			for (const auto& i : mShaderCBufferBinder->PSResources)
			{
				if (i.second == nullptr)
					continue;
				i.second->UpdateDrawPass(cmd, bImmCBuffer);
				PSSetConstantBuffer(cmd, i.first, i.second);
			}
		}
	}

	if (mShaderSrvBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Textures");
		for (const auto& i : mShaderSrvBinder->VSResources)
		{
			if (i.second != nullptr)
				VSSetShaderResource(cmd, i.first, i.second);
		}
		for (const auto& i : mShaderSrvBinder->PSResources)
		{
			if (i.second != nullptr)
				PSSetShaderResource(cmd, i.first, i.second);
		}
	}

	if (AttachSRVs != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Textures");
		for (const auto& i : AttachSRVs->VSResources)
		{
			if (i.second != nullptr)
				VSSetShaderResource(cmd, i.first, i.second);
		}
		for (const auto& i : AttachSRVs->PSResources)
		{
			if (i.second != nullptr)
				PSSetShaderResource(cmd, i.first, i.second);
		}
	}
	
	if (mShaderSamplerBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Samplers");
		for (const auto& i : mShaderSamplerBinder->VSResources)
		{
			if (i.second != nullptr)
				VSSetSampler(cmd, i.first, i.second);
		}
		for (const auto& i : mShaderSamplerBinder->PSResources)
		{
			if (i.second != nullptr)
				PSSetSampler(cmd, i.first, i.second);
		}
	}

	{
		if (AttachVBs != nullptr)
		{
			NumInstances = AttachVBs->mNumInstances;
		}
		/*else
		{
			NumInstances = 1;
		}*/

		if (cmd->OnPassBuilt != nullptr)
		{
			cmd->OnPassBuilt(cmd, this);
		}

		if(cmd->mPipelineStat!=nullptr)
		{
			cmd->mPipelineStat->mDrawCall++;
			cmd->mPipelineStat->mDrawTriangle += NumInstances * dpDesc->NumPrimitives;
		}

		if (IndirectDrawArgsBuffer)
		{
			this->DrawIndexedInstancedIndirect(cmd, dpDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
		}
		else
		{
			if (dpDesc->IsIndexDraw())
			{
				this->DrawIndexedPrimitive(cmd, dpDesc->PrimitiveType, dpDesc->BaseVertexIndex, dpDesc->StartIndex, dpDesc->NumPrimitives, NumInstances);
			}
			else
			{
				this->DrawPrimitive(cmd, dpDesc->PrimitiveType, dpDesc->BaseVertexIndex, dpDesc->NumPrimitives, NumInstances);
			}
		}
	}
}

vBOOL IDrawCall::ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer)
{
	auto geoMesh = MeshPrimitives->GetGeomtryMesh();
	if (geoMesh == nullptr)
		return FALSE;
	vBOOL applyOK = geoMesh->ApplyGeometry(cmd, this, bImmCBuffer);
	if (AttachVBs != nullptr)
		AttachVBs->ApplyVBs(cmd, this, bImmCBuffer);

	if (AttachIndexBuffer != nullptr)
		SetIndexBuffer(cmd, AttachIndexBuffer);

	return applyOK;
}

//#pragma optimize( "", on )

void IDrawCall::BindPipeline(IRenderPipeline* pipeline)
{
	mPipelineState.StrongRef(pipeline);
}

//void IDrawCall::BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer)
//{
//	GetCBufferResources()->BindVS(Index, CBuffer);
//}
//
//void IDrawCall::BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer)
//{
//	GetCBufferResources()->BindPS(Index, CBuffer);
//}

IConstantBuffer* IDrawCall::FindCBufferVS(const char* name) 
{
	if (mShaderCBufferBinder == nullptr)
		return nullptr;
	return mShaderCBufferBinder->FindVS(name);
}

IConstantBuffer* IDrawCall::FindCBufferPS(const char* name) 
{
	if (mShaderCBufferBinder == nullptr)
		return nullptr;
	return mShaderCBufferBinder->FindPS(name);
}

//UINT IDrawCall::FindCBufferIndex(const char* name)
//{
//	if (this->mPipelineState == nullptr)
//		return -1;
//	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
//	if (program == nullptr)
//		return -1;
//	return program->GetReflector()->FindShaderBinder(SBT_CBuffer, name);
//}
//
//void IDrawCall::BindCBufferAll(UINT cbIndex, IConstantBuffer* CBuffer)
//{
//	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
//	auto desc = program->GetReflector()->GetCBuffer(cbIndex);
//	if (desc != nullptr)
//	{
//		if (desc->VSBindPoint != 0xFFFFFFFF)
//			BindCBufferVS(desc->VSBindPoint, CBuffer);
//		if (desc->PSBindPoint != 0xFFFFFFFF)
//			BindCBufferPS(desc->PSBindPoint, CBuffer);
//	}
//}
//
//UINT IDrawCall::FindSRVIndex(const char* name)
//{
//	if (this->mPipelineState == nullptr)
//		return -1;
//	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
//	if (program == nullptr)
//		return -1;
//	return program->GetReflector()->FindShaderBinder(SBT_Srv, name);
//}
//
//void IDrawCall::BindSRVAll(UINT Index, IShaderResourceView* srv)
//{
//	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
//	if (program == nullptr)
//	{
//		ASSERT(false);
//		return;
//	}
//
//	auto info = program->GetReflector()->GetShaderBinder(SBT_Srv, Index);
//	if (info == nullptr)
//		return;
//
//	if (info->VSBindPoint != 0xFFFFFFFF)
//		mShaderSrvBinder->BindVS(info->VSBindPoint, srv);
//	if (info->PSBindPoint != 0xFFFFFFFF)
//		mShaderSrvBinder->BindPS(info->PSBindPoint, srv);
//}

void IDrawCall::BindShaderCBuffer(UINT index, IConstantBuffer* CBuffer)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	auto desc = program->GetReflector()->GetCBuffer(index);
	if (desc != nullptr)
	{
		if (desc->VSBindPoint != 0xFFFFFFFF)
			GetCBufferResources()->BindVS(desc->VSBindPoint, CBuffer);
		if (desc->PSBindPoint != 0xFFFFFFFF)
		{
			GetCBufferResources()->BindPS(desc->PSBindPoint, CBuffer);
		}
	}
}

void IDrawCall::BindShaderCBuffer(IShaderBinder* binder, IConstantBuffer* cbuffer)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	if (program == nullptr)
	{
		ASSERT(false);
		return;
	}

	if (binder->VSBindPoint != 0xFFFFFFFF)
		GetCBufferResources()->BindVS(binder->VSBindPoint, cbuffer);
	if (binder->PSBindPoint != 0xFFFFFFFF)
	{
		GetCBufferResources()->BindPS(binder->PSBindPoint, cbuffer);
	}
}

void IDrawCall::BindShaderSrv(UINT index, IShaderResourceView* srv)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	auto desc = program->GetReflector()->GetShaderBinder(SBT_Srv, index);
	if (desc != nullptr)
	{
		if (desc->VSBindPoint != 0xFFFFFFFF)
			GetShaderRViewResources()->BindVS(desc->VSBindPoint, srv);
		if (desc->PSBindPoint != 0xFFFFFFFF)
		{
			//ASSERT(desc->PSBindPoint != 26);
			GetShaderRViewResources()->BindPS(desc->PSBindPoint, srv);
		}
	}
}

void IDrawCall::BindShaderSrv(IShaderBinder* binder, IShaderResourceView* srv)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	if (program == nullptr)
	{
		ASSERT(false);
		return;
	}

	if (binder->VSBindPoint != 0xFFFFFFFF)
		GetShaderRViewResources()->BindVS(binder->VSBindPoint, srv);
	if (binder->PSBindPoint != 0xFFFFFFFF)
	{
		//ASSERT(binder->PSBindPoint != 26); 
		GetShaderRViewResources()->BindPS(binder->PSBindPoint, srv);
	}
}

void IDrawCall::BindShaderSampler(UINT index, ISamplerState* sampler)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	auto desc = program->GetReflector()->GetShaderBinder(SBT_Sampler, index);
	if (desc != nullptr)
	{
		if (desc->VSBindPoint != 0xFFFFFFFF)
			GetShaderSamplers()->BindVS(desc->VSBindPoint, sampler);
		if (desc->PSBindPoint != 0xFFFFFFFF)
			GetShaderSamplers()->BindPS(desc->PSBindPoint, sampler);
	}
}

void IDrawCall::BindShaderSampler(IShaderBinder* binder, ISamplerState* sampler)
{
	IShaderProgram* program = this->mPipelineState->GetGpuProgram();
	if (program == nullptr)
	{
		ASSERT(false);
		return;
	}

	if (binder->VSBindPoint != 0xFFFFFFFF)
		GetShaderSamplers()->BindVS(binder->VSBindPoint, sampler);
	if (binder->PSBindPoint != 0xFFFFFFFF)
		GetShaderSamplers()->BindPS(binder->PSBindPoint, sampler);
}

ShaderReflector* IDrawCall::GetReflector() 
{
	return mPipelineState->GetGpuProgram()->GetReflector();
}

///===============================================
void IComputeDrawcall::SetComputeShader(IComputeShader* shader)
{
	mComputeShader.StrongRef(shader);
}

void IComputeDrawcall::SetDispatch(UINT x, UINT y, UINT z)
{
	mDispatchX = x;
	mDispatchY = y;
	mDispatchZ = z;
}

void IComputeDrawcall::SetDispatchIndirectBuffer(IGpuBuffer* buffer, UINT offset) 
{
	IndirectDrawArgsBuffer.StrongRef(buffer);
	IndirectDrawArgsOffset = offset;
}

ShaderReflector* IComputeDrawcall::GetReflector() 
{
	return mComputeShader->GetReflector();
}

NS_END
