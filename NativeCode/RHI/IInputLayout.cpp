#include "IInputLayout.h"
#include "../../Base/HashDefine.h"

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

UINT64 IInputLayoutDesc::GetLayoutHash64()
{
	std::string hashStr = "";
	for (size_t i = 0; i < Layouts.size(); i++)
	{
		hashStr += VStringA_FormatV("%s_%d_%d_%d_%d_%d_%d",
			Layouts[i].SemanticName.c_str(),
			Layouts[i].SemanticIndex,
			Layouts[i].Format,
			Layouts[i].InputSlot,
			Layouts[i].AlignedByteOffset,
			Layouts[i].IsInstanceData,
			Layouts[i].InstanceDataStepRate);
	}
	return HashHelper::CalcHash64(hashStr.c_str(), (int)hashStr.length()).Int64Value;
}

IInputLayout::IInputLayout()
{
}


IInputLayout::~IInputLayout()
{
}

NS_END
