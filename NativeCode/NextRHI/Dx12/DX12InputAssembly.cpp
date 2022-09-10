#include "DX12InputAssembly.h"
#include "DX12GpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX12InputLayout::DX12InputLayout()
	{

	}
	DX12InputLayout::~DX12InputLayout()
	{
	}
	bool DX12InputLayout::Init(DX12GpuDevice* device, FInputLayoutDesc* desc)
	{
		mDesc = desc;
		
		desc->ShaderDesc = nullptr;
		return true;
	}
	void DX12InputLayout::GetDX12Elements(std::vector<D3D12_INPUT_ELEMENT_DESC>& mDx12Elements)
	{
		mDx12Elements.clear();
		for (const auto& i : mDesc->Layouts)
		{
			D3D12_INPUT_ELEMENT_DESC tmp;
			tmp.SemanticName = i.SemanticName.c_str();
			tmp.SemanticIndex = i.SemanticIndex;
			tmp.Format = FormatToDX12Format(i.Format);
			tmp.InputSlot = i.InputSlot;
			tmp.AlignedByteOffset = i.AlignedByteOffset;
			tmp.InputSlotClass = i.IsInstanceData ? D3D12_INPUT_CLASSIFICATION_PER_INSTANCE_DATA : D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA;
			tmp.InstanceDataStepRate = i.InstanceDataStepRate;
			mDx12Elements.push_back(tmp);
		}
	}
}

NS_END
