#include "ID11InputLayout.h"
#include "ID11RenderContext.h"
#include "D11PreHead.h"
#include "../IShader.h"

#define new VNEW

NS_BEGIN

ID11InputLayout::ID11InputLayout()
{
	mLayout = nullptr;
}

ID11InputLayout::~ID11InputLayout()
{
	Safe_Release(mLayout);
}

bool ID11InputLayout::Init(ID11RenderContext* rc, const IInputLayoutDesc* desc)
{
	mDesc.StrongRef((IInputLayoutDesc*)desc);
	std::vector<D3D11_INPUT_ELEMENT_DESC>	elems;
	for (size_t i=0; i<desc->Layouts.size();i++)
	{
		D3D11_INPUT_ELEMENT_DESC e;
		const LayoutElement& se = desc->Layouts[i];
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
	if (desc->ShaderDesc != nullptr)
	{
		const std::vector<BYTE>& codes = desc->ShaderDesc->GetCodes();
		auto hr = rc->mDevice->CreateInputLayout(&elems[0], (UINT)elems.size(), &codes[0],
			(UINT)codes.size(), &mLayout);
		if (FAILED(hr))
			return false;
	}
	else
	{
		auto hr = rc->mDevice->CreateInputLayout(&elems[0], (UINT)elems.size(), nullptr,
			0, &mLayout);
		if (FAILED(hr))
			return false;
	}

#ifdef _DEBUG
	mLayout->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	mLayout->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("Layout"), "Layout");
#endif

	return true;
}

NS_END