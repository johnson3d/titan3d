#pragma once
#include "../NxInputAssembly.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullInputLayout : public IInputLayout
	{
	public:
		NullInputLayout();
		~NullInputLayout();
		bool Init(NullGpuDevice* device, FInputLayoutDesc* desc);
	public:
	};
}

NS_END

