#include "DX12GeomMesh.h"
#include "DX12CommandList.h"
#include "DX12Buffer.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void DX12VertexArray::Commit(DX12CommandList* dx12Cmd, UINT NumOfVA, DX12VertexArray** VAs)
	{
		D3D12_VERTEX_BUFFER_VIEW dxVBs[VST_Number]{};
		for (UINT i = 0; i < NumOfVA; i++)
		{
			if (VAs[i] == nullptr)
				continue;

			dx12Cmd->GetCmdRecorder()->UseResource(VAs[i]->RefResources);
			for (int j = 0; j < VST_Number; j++)
			{
				auto vbv = VAs[i]->VertexBuffers[j];
				if (vbv != nullptr)
				{
					auto vb = vbv->Buffer.UnsafeConvertTo<DX12Buffer>();
					auto& dxvbv = dxVBs[j];
					dxvbv.BufferLocation = vb->GetGPUVirtualAddress() + vbv->Desc.Offset;
					dxvbv.StrideInBytes = vbv->Desc.Stride;// vb->Desc.StructureStride;
					dxvbv.SizeInBytes = vbv->Desc.Size;
				}
			}
		}
		dx12Cmd->mContext->IASetVertexBuffers(0, VST_Number, dxVBs);
	}
	void DX12VertexArray::Commit(ICommandList* cmdlist)
	{
		auto dx12Cmd = (DX12CommandList*)cmdlist;
		dx12Cmd->GetCmdRecorder()->UseResource(RefResources);
		
		D3D12_VERTEX_BUFFER_VIEW dxVBs[VST_Number]{};
		for (int i = 0; i < VST_Number; i++)
		{
			auto vbv = VertexBuffers[i];
			if (vbv != nullptr)
			{
				auto vb = vbv->Buffer.UnsafeConvertTo<DX12Buffer>();
				auto& vbv = dxVBs[i];
				vbv.BufferLocation = vb->GetGPUVirtualAddress();
				vbv.StrideInBytes = vb->Desc.StructureStride;
				vbv.SizeInBytes = vb->Desc.Size;
			}
		}
		dx12Cmd->mContext->IASetVertexBuffers(0, VST_Number, dxVBs);
	}
	void DX12VertexArray::BindVB(EVertexStreamType stream, IVbView* buffer)
	{
		FVertexArray::BindVB(stream, buffer);
		RefResources = MakeWeakRef(new FRefResources());

		for (int i = 0; i < VST_Number; i++)
		{
			if (VertexBuffers[i] == nullptr)
			{
				continue;
			}

			RefResources->Resources.push_back(VertexBuffers[i]);
		}
	}
}

NS_END
