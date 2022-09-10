#include "DX11InputAssembly.h"
#include "DX11GpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX11InputLayout::DX11InputLayout()
	{

	}
	DX11InputLayout::~DX11InputLayout()
	{
		Safe_Release(mLayout);
	}
	bool DX11InputLayout::Init(DX11GpuDevice* device, FInputLayoutDesc* desc)
	{
		mDesc = desc;
		
		std::vector<D3D11_INPUT_ELEMENT_DESC>	elems;
		for (size_t i = 0; i < desc->Layouts.size(); i++)
		{
			D3D11_INPUT_ELEMENT_DESC e;
			const auto& se = desc->Layouts[i];
			e.AlignedByteOffset = se.AlignedByteOffset;
			e.Format = FormatToDXFormat(se.Format);
			e.InputSlot = se.InputSlot;
			if (se.IsInstanceData)
			{
				e.InputSlotClass = D3D11_INPUT_PER_INSTANCE_DATA;
			}
			else
			{
				e.InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA;
			}
			e.InstanceDataStepRate = se.InstanceDataStepRate;
			e.SemanticIndex = se.SemanticIndex;
			e.SemanticName = se.SemanticName.c_str();
			elems.push_back(e);
			//mDesc->AddElement(se.SemanticName.c_str(), se.SemanticIndex, se.Format, se.InputSlot, se.AlignedByteOffset, se.IsInstanceData, se.InstanceDataStepRate);
		}

		auto hr = device->mDevice->CreateInputLayout(&elems[0], (UINT)elems.size(), &desc->ShaderDesc->Dxbc[0],
			desc->ShaderDesc->Dxbc.size(), &mLayout);
		if (FAILED(hr))
			return false;
		
		desc->ShaderDesc = nullptr;
		return true;
	}
}

NS_END
