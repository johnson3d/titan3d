#include "IPass.h"
#include "IConstantBuffer.h"
#include "IRenderPipeline.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "ICommandList.h"
#include "IShaderResourceView.h"
#include "ISamplerState.h"
#include "IShaderProgram.h"
#include "IUnorderedAccessView.h"
#include "GraphicsProfiler.h"
#include "../Core/vfxSampCounter.h"
#include "../Graphics/Mesh/GfxMeshPrimitives.h"

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
	for (int i = 0; i < MaxTexOrSamplerSlot; i++)
	{
		Safe_Release(PSResources[i]);
	}
	for (int i = 0; i < MaxTexOrSamplerSlot; i++)
	{
		Safe_Release(VSResources[i]);
	}
}

void IShaderResources::VSBindTexture(BYTE slot, IShaderResourceView* tex)
{
	if (slot >= MaxTexOrSamplerSlot)
		return;
	if (VSResources[slot] == tex)
		return;
	if (tex)
		tex->AddRef();
	Safe_Release(VSResources[slot]);
	VSResources[slot] = tex;
}

void IShaderResources::PSBindTexture(BYTE slot, IShaderResourceView* tex)
{
	if (slot >= MaxTexOrSamplerSlot)
		return;
	if (PSResources[slot] == tex)
		return;
	if (tex)
		tex->AddRef();
	Safe_Release(PSResources[slot]);
	PSResources[slot] = tex;
}

IShaderResourceView* IShaderResources::GetBindTextureVS(BYTE slot)
{
	return PSResources[slot];
}

IShaderResourceView* IShaderResources::GetBindTexturePS(BYTE slot)
{
	return PSResources[slot];
}

IShaderSamplers::~IShaderSamplers()
{
	for (int i = 0; i < MaxTexOrSamplerSlot; i++)
	{
		Safe_Release(VSSamplers[i]);
		Safe_Release(PSSamplers[i]);
	}
}

void IShaderSamplers::VSBindSampler(BYTE slot, ISamplerState* sampler)
{
	if (slot >= MaxTexOrSamplerSlot)
		return;
	if (sampler)
		sampler->AddRef();
	Safe_Release(VSSamplers[slot]);
	VSSamplers[slot] = sampler;
}

void IShaderSamplers::PSBindSampler(BYTE slot, ISamplerState* sampler)
{
	if (slot >= MaxTexOrSamplerSlot)
		return;
	if (sampler)
		sampler->AddRef();
	Safe_Release(PSSamplers[slot]);
	PSSamplers[slot] = sampler;
}

IPass::IPass()
{
	mUserFlags = 0;

	m_pPipelineState = nullptr;
	m_pGpuProgram = nullptr;
	MeshPrimitives = nullptr;
	AtomIndex = 0;
	LodLevel = 0;
	NumInstances = 1;
	m_pShaderTexBinder = nullptr;
	m_pShaderSamplerBinder = nullptr;
	m_pViewport = nullptr;
	ScissorRect = nullptr;

	IndirectDrawOffsetForArgs = 0;

	memset(CBuffersVS, 0, sizeof(CBuffersVS));
	memset(CBuffersPS, 0, sizeof(CBuffersPS));
}

IPass::~IPass()
{
	Safe_Release(ScissorRect);
	Safe_Release(m_pViewport);
	Safe_Release(m_pShaderTexBinder);
	Safe_Release(m_pShaderSamplerBinder);
	Safe_Release(m_pPipelineState);
	Safe_Release(m_pGpuProgram);
	Safe_Release(MeshPrimitives);
	for (int i = 0; i < MaxCB; i++)
	{
		Safe_Release(CBuffersVS[i]);
		Safe_Release(CBuffersPS[i]);
	}
}


//#pragma optimize("atgwy", on)
//#pragma auto_inline(on)

void IPass::SetInstanceNumber(int instNum) 
{
	NumInstances = instNum;
}

void IPass::SetIndirectDraw(IGpuBuffer* pBuffer, UINT offset)
{
	IndirectDrawArgsBuffer.StrongRef(pBuffer);
	IndirectDrawOffsetForArgs = offset;
}

void IPass::BindGeometry(GfxMeshPrimitives* mesh, UINT atom, float lod)
{
	VAutoVSLLock<VSLLockNoSleep> lk(mLocker);
	if (mesh)
		mesh->AddRef();
	Safe_Release(MeshPrimitives);
	MeshPrimitives = mesh;

	AtomIndex = atom;

	SetLod(lod);
}

void IPass::SetLod(float lod)
{
	LodLevel = MeshPrimitives->GetLodLevel(AtomIndex, lod);
}

float IPass::GetLod()
{
	if (LodLevel == 0)
		return 0;
	auto num = (float)(MeshPrimitives->GetAtomLOD(AtomIndex) - 1);
	return (float)LodLevel / num;
}

void IPass::GetDrawPrimitive(DrawPrimitiveDesc* desc) 
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

void IPass::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	if (m_pGpuProgram == nullptr || m_pPipelineState == nullptr)
	{
		return;
	}
	if (m_pGpuProgram->GetVertexShader() == nullptr)
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
		SetViewport(cmd, m_pViewport);

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
		}
		SetPipeline(cmd, m_pPipelineState);

		m_pGpuProgram->ApplyShaders(cmd);

		auto vs = m_pGpuProgram->GetVertexShader();
		if (vs)
		{
			for (int i = 0; i < MaxCB; i++)
			{
				if (CBuffersVS[i] == nullptr)
					continue;

				CBuffersVS[i]->UpdateDrawPass(cmd, bImmCBuffer);
				VSSetConstantBuffer(cmd, i, CBuffersVS[i]);
			}
		}
		auto ps = m_pGpuProgram->GetPixelShader();
		if (ps)
		{
			for (int i = 0; i < MaxCB; i++)
			{
				if (CBuffersPS[i] == nullptr)
					continue;
				CBuffersPS[i]->UpdateDrawPass(cmd, bImmCBuffer);
				PSSetConstantBuffer(cmd, i, CBuffersPS[i]);
			}
		}
	}

	if (m_pShaderTexBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Textures");
		for (UINT i = 0; i < MaxTexOrSamplerSlot; i++)
		{
			if (m_pShaderTexBinder->VSResources[i] != nullptr)
			{
				VSSetShaderResource(cmd, i, m_pShaderTexBinder->VSResources[i]);
			}
			if (m_pShaderTexBinder->PSResources[i] != nullptr)
			{
				PSSetShaderResource(cmd, i, m_pShaderTexBinder->PSResources[i]);
			}
		}
	}

	if (AttachSRVs != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Textures");
		for (UINT i = 0; i < MaxTexOrSamplerSlot; i++)
		{
			if (AttachSRVs->VSResources[i] != nullptr)
			{
				VSSetShaderResource(cmd, i, AttachSRVs->VSResources[i]);
			}
			if (AttachSRVs->PSResources[i] != nullptr)
			{
				PSSetShaderResource(cmd, i, AttachSRVs->PSResources[i]);
			}
		}
	}
	
	if (m_pShaderSamplerBinder != nullptr)
	{
		AUTO_SAMP("Native.IPass.BuildPass.Samplers");
		for (int i = 0; i < MaxTexOrSamplerSlot; i++)
		{
			if (m_pShaderSamplerBinder->VSSamplers[i] != nullptr)
			{
				VSSetSampler(cmd, i, m_pShaderSamplerBinder->VSSamplers[i]);
			}
			if (m_pShaderSamplerBinder->PSSamplers[i] != nullptr)
			{
				PSSetSampler(cmd, i, m_pShaderSamplerBinder->PSSamplers[i]);
			}
		}
	}

	{
		const DrawPrimitiveDesc* dpDesc = nullptr;
		{
			AUTO_SAMP("Native.IPass.BuildPass.Geometry");
			VAutoVSLLock<VSLLockNoSleep> lk(mLocker);
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

vBOOL IPass::ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer)
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

void IPass::BindPipeline(IRenderPipeline* pipeline)
{
	if (pipeline)
	{
		pipeline->AddRef();
	}
	Safe_Release(m_pPipelineState);
	m_pPipelineState = pipeline;
}

void IPass::BindGpuProgram(IShaderProgram* pGpuProgram)
{
	if (pGpuProgram != nullptr)
	{
		pGpuProgram->AddRef();
	}
	Safe_Release(m_pGpuProgram);
	m_pGpuProgram = pGpuProgram;
}

void IPass::BindCBufferVS(UINT32 Index, IConstantBuffer* CBuffer)
{
	if (Index >= MaxCB)
		return;

	if (CBuffer != nullptr)
		CBuffer->AddRef();
	Safe_Release(CBuffersVS[Index]);
	CBuffersVS[Index] = CBuffer;
}

void IPass::BindCBufferPS(UINT32 Index, IConstantBuffer* CBuffer)
{
	if (Index >= MaxCB)
		return;

	if (CBuffer != nullptr)
		CBuffer->AddRef();
	Safe_Release(CBuffersPS[Index]);
	CBuffersPS[Index] = CBuffer;
}

IConstantBuffer* IPass::FindCBufferVS(const char* name) 
{
	for (int i = 0; i < MaxCB; i++)
	{
		if(CBuffersVS[i]==nullptr)
			continue;;
		if (CBuffersVS[i]->Desc.Name == name)
			return CBuffersVS[i];
	}
	return nullptr;
}

IConstantBuffer* IPass::FindCBufferPS(const char* name) 
{
	for (int i = 0; i < MaxCB; i++)
	{
		if (CBuffersPS[i] == nullptr)
			continue;;
		if (CBuffersPS[i]->Desc.Name == name)
			return CBuffersPS[i];
	}
	return nullptr;
}

void IPass::BindCBuffAll(IShaderProgram* shaderProgram, UINT cbIndex, IConstantBuffer* CBuffer)
{
	IConstantBufferDesc desc;
	if (shaderProgram->GetCBufferDesc(cbIndex, &desc)!=FALSE)
	{
		BindCBufferVS(desc.VSBindPoint, CBuffer);
		BindCBufferPS(desc.PSBindPoint, CBuffer);
	}
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS0(EngineNS, IViewPort, GetTopLeftX);
	Cpp2CS1(EngineNS, IViewPort, SetTopLeftX);
	Cpp2CS0(EngineNS, IViewPort, GetTopLeftY);
	Cpp2CS1(EngineNS, IViewPort, SetTopLeftY);
	Cpp2CS0(EngineNS, IViewPort, GetWidth);
	Cpp2CS1(EngineNS, IViewPort, SetWidth);
	Cpp2CS0(EngineNS, IViewPort, GetHeight);
	Cpp2CS1(EngineNS, IViewPort, SetHeight);
	Cpp2CS0(EngineNS, IViewPort, GetMinDepth);
	Cpp2CS1(EngineNS, IViewPort, SetMinDepth);
	Cpp2CS0(EngineNS, IViewPort, GetMaxDepth);
	Cpp2CS1(EngineNS, IViewPort, SetMaxDepth);

	Cpp2CS1(EngineNS, IScissorRect, SetRectNumber);
	Cpp2CS0(EngineNS, IScissorRect, GetRectNumber);
	Cpp2CS5(EngineNS, IScissorRect, SetSCRect);
	Cpp2CS2(EngineNS, IScissorRect, GetSCRect);

	Cpp2CS2(EngineNS, IShaderResources, VSBindTexture);
	Cpp2CS2(EngineNS, IShaderResources, PSBindTexture);
	Cpp2CS1(EngineNS, IShaderResources, GetBindTextureVS);
	Cpp2CS1(EngineNS, IShaderResources, GetBindTexturePS);

	Cpp2CS0(EngineNS, IShaderResources, PSResourceNum);
	Cpp2CS0(EngineNS, IShaderResources, VSResourceNum);

	Cpp2CS2(EngineNS, IShaderSamplers, VSBindSampler);
	Cpp2CS2(EngineNS, IShaderSamplers, PSBindSampler);

	Cpp2CS0(EngineNS, IPass, GetUserFlags);
	Cpp2CS1(EngineNS, IPass, SetUserFlags);

	Cpp2CS1(EngineNS, IPass, BindPipeline);
	Cpp2CS1(EngineNS, IPass, BindGpuProgram);
	Cpp2CS3(EngineNS, IPass, BindGeometry);
	Cpp2CS0(EngineNS, IPass, GetLod);
	Cpp2CS1(EngineNS, IPass, SetLod);
	Cpp2CS1(EngineNS, IPass, BindShaderResouces);
	Cpp2CS1(EngineNS, IPass, BindShaderSamplers);
	Cpp2CS1(EngineNS, IPass, BindViewPort);
	Cpp2CS1(EngineNS, IPass, BindScissor);

	Cpp2CS2(EngineNS, IPass, BindCBufferVS);
	Cpp2CS2(EngineNS, IPass, BindCBufferPS);

	Cpp2CS1(EngineNS, IPass, SetInstanceNumber);
	Cpp2CS2(EngineNS, IPass, SetIndirectDraw);

	Cpp2CS1(EngineNS, IPass, BindAttachVBs);
	Cpp2CS1(EngineNS, IPass, BindAttachIndexBuffer);
	Cpp2CS1(EngineNS, IPass, BindAttachSRVs);

	Cpp2CS0(EngineNS, IPass, GetPipeline);
	Cpp2CS0(EngineNS, IPass, GetGpuProgram);
	Cpp2CS0(EngineNS, IPass, GetShaderResurces);
	Cpp2CS1(EngineNS, IPass, GetDrawPrimitive);
	Cpp2CS1(EngineNS, IPass, FindCBufferVS);
	Cpp2CS1(EngineNS, IPass, FindCBufferPS);

	Cpp2CS3(EngineNS, IPass, BindCBuffAll)
}