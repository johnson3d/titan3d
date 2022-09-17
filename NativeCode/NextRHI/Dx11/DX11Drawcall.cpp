#include "DX11Drawcall.h"
#include "DX11CommandList.h"
#include "DX11Buffer.h"
#include "DX11GpuState.h"
#include "DX11FrameBuffers.h"
#include "../NxGeomMesh.h"
#include "../NxEffect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void DX11GraphicDraw::Commit(ICommandList* cmdlist)
	{
		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		//Mesh->Commit(cmdlist);
		ID3D11Buffer* dxVBs[VST_Number];
		UINT strides[VST_Number]{};
		UINT offset[VST_Number]{};
		for (int i = 0; i < VST_Number; i++)
		{
			auto vbv = Mesh->VertexArray->VertexBuffers[i];
			if (vbv != nullptr)
			{
				auto vb = vbv->Buffer.UnsafeConvertTo<DX11Buffer>();
				dxVBs[i] = vb->mBuffer;
				strides[i] = vb->Desc.StructureStride;
			}
			else
				dxVBs[i] = nullptr;
		}
		((DX11CommandList*)cmdlist)->mContext->IASetVertexBuffers(0, VST_Number, dxVBs, strides, offset);
		cmdlist->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);

		if (AttachVB != nullptr)
		{
			AttachVB->Commit(cmdlist);
		}

		UpdateGpuDrawState(cmdlist->GetGpuDevice(), cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
		/*if (GpuDrawState == nullptr)
		{
			UpdateGpuDrawState(cmdlist->mDevice, cmdlist, );
			if (GpuDrawState == nullptr)
				return;
		}*/
		cmdlist->SetGraphicsPipeline(GpuDrawState);

		auto effect = GetShaderEffect();

		effect->Commit(cmdlist, this);

		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					effect->BindCBuffer(cmdlist, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second;
					effect->BindUav(cmdlist, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second;
					effect->BindSampler(cmdlist, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		ASSERT(pDrawDesc);
		if (IndirectDrawArgsBuffer)
		{
			cmdlist->IndirectDrawIndexed(pDrawDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
		}
		else
		{
			if (pDrawDesc->IsIndexDraw())
			{
				cmdlist->DrawIndexed(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->StartIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
			else
			{
				cmdlist->Draw(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
		}
	}
}

NS_END
