#include "NullInputAssembly.h"
#include "NullGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	NullInputLayout::NullInputLayout()
	{

	}
	NullInputLayout::~NullInputLayout()
	{
		
	}
	bool NullInputLayout::Init(NullGpuDevice* device, FInputLayoutDesc* desc)
	{
		mDesc = desc;
		
		desc->ShaderDesc = nullptr;
		return true;
	}
}

NS_END
