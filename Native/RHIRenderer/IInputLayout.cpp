#include "IInputLayout.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IInputLayoutDesc, EngineNS::VIUnknown)

const LayoutElement* IInputLayoutDesc::FindElementBySemantic(const char* name, UINT index)
{
	for (size_t i = 0; i<Layouts.size(); i++)
	{
		if (Layouts[i].SemanticName == name && Layouts[i].SemanticIndex == index)
		{
			return &Layouts[i];
		}
	}
	return nullptr;
}

IInputLayout::IInputLayout()
{
}


IInputLayout::~IInputLayout()
{
}

NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS7(EngineNS, IInputLayoutDesc, AddElement);
	Cpp2CS1(EngineNS, IInputLayoutDesc, SetShaderDesc);

	Cpp2CS0(EngineNS, IInputLayout, GetElemNumber);
}