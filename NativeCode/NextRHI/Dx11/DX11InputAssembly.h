#pragma once
#include "../NxInputAssembly.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11InputLayout : public IInputLayout
	{
	public:
		DX11InputLayout();
		~DX11InputLayout();
		bool Init(DX11GpuDevice* device, FInputLayoutDesc* desc);
	public:
		ID3D11InputLayout* mLayout = nullptr;
	};
}

NS_END

