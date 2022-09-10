#pragma once
#include "../NxInputAssembly.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12InputLayout : public IInputLayout
	{
	public:
		DX12InputLayout();
		~DX12InputLayout();
		bool Init(DX12GpuDevice* device, FInputLayoutDesc* desc);
	public:
		void GetDX12Elements(std::vector<D3D12_INPUT_ELEMENT_DESC>& mDx12Elements);
	};
}

NS_END

