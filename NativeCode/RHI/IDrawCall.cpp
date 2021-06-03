#include "IDrawCall.h"
#include "IConstantBuffer.h"
#include "IRenderPipeline.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
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

RTTI_IMPL(EngineNS::IShaderResources, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IShaderSamplers, EngineNS::VIUnknown);

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

IShaderResources::~IShaderResources()
{
}

void IShaderResources::VSBindTexture(UINT slot, IShaderResourceView* tex)
{
	for (auto& i : VSResources)
	{
		if (i.first == (USHORT)slot)
		{
			if (i.second == tex)
				return;
			i.second.StrongRef(tex);
			return;
		}
	}
	AutoRef<IShaderResourceView> tmp;
	tmp.StrongRef(tex);
	VSResources.push_back(std::make_pair((USHORT)slot, tmp));
}

void IShaderResources::PSBindTexture(UINT slot, IShaderResourceView* tex)
{
	for (auto& i : PSResources)
	{
		if (i.first == (USHORT)slot)
		{
			if (i.second == tex)
				return;
			i.second.StrongRef(tex);
			return;
		}
	}
	AutoRef<IShaderResourceView> tmp;
	tmp.StrongRef(tex);
	PSResources.push_back(std::make_pair((USHORT)slot, tmp));
}

IShaderResourceView* IShaderResources::GetBindTextureVS(UINT slot)
{
	for (auto& i : VSResources)
	{
		if (i.first == (USHORT)slot)
		{
			return i.second;
		}
	}
	return nullptr;
}

IShaderResourceView* IShaderResources::GetBindTexturePS(UINT slot)
{
	for (auto& i : PSResources)
	{
		if (i.first == (USHORT)slot)
		{
			return i.second;
		}
	}
	return nullptr;
}

IShaderSamplers::~IShaderSamplers()
{
}

void IShaderSamplers::VSBindSampler(UINT slot, ISamplerState* sampler)
{
	for (auto& i : VSSamplers)
	{
		if (i.first == (USHORT)slot)
		{
			if (i.second == sampler)
				return;
			i.second.StrongRef(sampler);
			return;
		}
	}
	AutoRef<ISamplerState> tmp;
	tmp.StrongRef(sampler);
	VSSamplers.push_back(std::make_pair((USHORT)slot, tmp));
}

void IShaderSamplers::PSBindSampler(UINT slot, ISamplerState* sampler)
{
	for (auto& i : PSSamplers)
	{
		if (i.first == (USHORT)slot)
		{
			if (i.second == sampler)
				return;
			i.second.StrongRef(sampler);
			return;
		}
	}
	AutoRef<ISamplerState> tmp;
	tmp.StrongRef(sampler);
	PSSamplers.push_back(std::make_pair((USHORT)slot, tmp));
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
	if (m_pPipelineState == nullptr)
	{
		return;
	}
	if (m_pPipelineState->GetGpuProgram()->GetVertexShader() == nullptr)
	{
		VFX_LTRACE(ELTT_Graphics, "VertexShader is null\r\n");
		return;
	}
	//if (cmd->mCurrentState.ViewPort != m_pViewport)
	//{
		//SetViewport(cmd, m_pViewport);
		//cmd->mCurrentState.ViewPort = m_pViewport;
	//}

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
		SetPipeline(cmd, m_pPipelineState);

		auto gpuProgram = m_pPipelineState->GetGpuProgram();
		gpuProgram->ApplyShaders(cmd);

		auto vs = gpuProgram->GetVertexShader();
		if (vs)
		{
			for (const auto& i : CBuffersVS)
			{
				if (i.second == nullptr)
					continue;
				i.second->UpdateDrawPass(cmd, bImmCBuffer);
				VSSetConstantBuffer(cmd, i.first, i.second);
			}
		}
		auto ps = gpuProgram->GetPixelShader();
		if (ps)
		{
			for (const auto& i : CBuffersPS)
			{
				if (i.second == nullptr)
					continue;
				i.second->UpdateDrawPass(cmd, bImmCBuffer);
				PSSetConstantBuffer(cmd, i.first, i.second);
			}
		}
	}

	if (m_pShaderTexBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Textures");
		for (const auto& i : m_pShaderTexBinder->VSResources)
		{
			if (i.second != nullptr)
				VSSetShaderResource(cmd, i.first, i.second);
		}
		for (const auto& i : m_pShaderTexBinder->PSResources)
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
	
	if (m_pShaderSamplerBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Samplers");
		for (const auto& i : m_pShaderSamplerBinder->VSSamplers)
		{
			if (i.second != nullptr)
				VSSetSampler(cmd, i.first, i.second);
		}
		for (const auto& i : m_pShaderSamplerBinder->PSSamplers)
		{
			if (i.second != nullptr)
				PSSetSampler(cmd, i.first, i.second);
		}
	}

	{
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

		if (AttachVBs != nullptr)
		{
			NumInstances = AttachVBs->mNumInstances;
		}
		else
		{
			NumInstances = 1;
		}

		if (cmd->OnPassBuilt != nullptr)
		{
			cmd->OnPassBuilt(cmd, this);
		}

		cmd->mDrawCall++;
		cmd->mDrawTriangle += NumInstances * dpDesc->NumPrimitives;

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
	m_pPipelineState.StrongRef(pipeline);
}

void IDrawCall::BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer)
{
	if (Index == 0xFFFFFFFF)
		return;
	for (auto& i : CBuffersVS)
	{
		if (i.first == (USHORT)Index)
		{
			i.second.StrongRef(CBuffer);
			return;
		}
	}

	AutoRef<IConstantBuffer> tmp;
	tmp.StrongRef(CBuffer);
	CBuffersVS.push_back(std::make_pair((USHORT)Index, tmp));
}

void IDrawCall::BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer)
{
	if (Index == 0xFFFFFFFF)
		return;
	for (auto& i : CBuffersPS)
	{
		if (i.first == (USHORT)Index)
		{
			i.second.StrongRef(CBuffer);
			return;
		}
	}

	AutoRef<IConstantBuffer> tmp;
	tmp.StrongRef(CBuffer);
	CBuffersPS.push_back(std::make_pair((USHORT)Index, tmp));
}

IConstantBuffer* IDrawCall::FindCBufferVS(const char* name) 
{
	for (auto& i : CBuffersVS)
	{
		if (i.second == nullptr)
			continue;

		if (i.second->Desc.Name == name)
		{
			return i.second;
		}
	}
	return nullptr;
}

IConstantBuffer* IDrawCall::FindCBufferPS(const char* name) 
{
	for (auto& i : CBuffersPS)
	{
		if (i.second == nullptr)
			continue;

		if (i.second->Desc.Name == name)
		{
			return i.second;
		}
	}
	return nullptr;
}

UINT IDrawCall::FindCBufferIndex(const char* name)
{
	if (this->m_pPipelineState == nullptr)
		return -1;
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	if (program == nullptr)
		return -1;
	return program->FindCBuffer(name);
}

void IDrawCall::BindCBufferAll(UINT cbIndex, IConstantBuffer* CBuffer)
{
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	IConstantBufferDesc desc;
	if (program->GetCBufferDesc(cbIndex, &desc) != FALSE)
	{
		if (desc.VSBindPoint != 0xFFFFFFFF)
			BindCBufferVS(desc.VSBindPoint, CBuffer);
		if (desc.PSBindPoint != 0xFFFFFFFF)
			BindCBufferPS(desc.PSBindPoint, CBuffer);
	}
}

UINT IDrawCall::FindSRVIndex(const char* name)
{
	if (this->m_pPipelineState == nullptr)
		return -1;
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	if (program == nullptr)
		return -1;
	return program->GetTextureBindSlotIndex(name);
}

void IDrawCall::BindSRVAll(UINT Index, IShaderResourceView* srv)
{
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	TSBindInfo info;
	if (program->GetShaderResourceBindInfo(Index, &info, sizeof(TSBindInfo)) == false)
		return;

	if (info.VSBindPoint != 0xFFFFFFFF)
		m_pShaderTexBinder->VSBindTexture(info.VSBindPoint, srv);
	if (info.PSBindPoint != 0xFFFFFFFF)
		m_pShaderTexBinder->PSBindTexture(info.PSBindPoint, srv);
}

bool IDrawCall::GetSRVBindInfo(const char* name, TSBindInfo* info, int dataSize)
{
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	UINT slot = program->GetTextureBindSlotIndex(name);
	if (slot == 0xFFFFFFFF)
		return false;

	return program->GetShaderResourceBindInfo(slot, info, dataSize);
}

bool IDrawCall::GetSamplerBindInfo(const char* name, TSBindInfo* info, int dataSize)
{
	IShaderProgram* program = this->m_pPipelineState->GetGpuProgram();
	UINT slot = program->GetSamplerBindSlotIndex(name);
	if (slot == 0xFFFFFFFF)
		return false;

	return program->GetSamplerBindInfo(slot, info, dataSize);
}

NS_END
