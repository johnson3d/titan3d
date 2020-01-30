#pragma once
#include "IRenderResource.h"
#include "IShader.h"

NS_BEGIN

struct LayoutElement
{
	UINT SemanticIndex;
	EPixelFormat Format;
	UINT InputSlot;
	UINT AlignedByteOffset;
	vBOOL IsInstanceData;
	UINT InstanceDataStepRate;
	std::string SemanticName;
};

struct IInputLayoutDesc : public VIUnknown
{
	RTTI_DEF(IInputLayoutDesc, 0x47e443e45b0395a3, true);
	std::vector<LayoutElement>	Layouts;
	AutoRef<IShaderDesc>		ShaderDesc;

	void AddElement(const char* SemanticName,
				UINT SemanticIndex,
				EPixelFormat Format,
				UINT InputSlot,
				UINT AlignedByteOffset,
				vBOOL IsInstanceData,
				UINT InstanceDataStepRate)
	{
		LayoutElement elem;
		elem.SemanticName = SemanticName;
		elem.SemanticIndex = SemanticIndex;
		elem.Format = Format;
		elem.InputSlot = InputSlot;
		elem.AlignedByteOffset = AlignedByteOffset;
		elem.IsInstanceData = IsInstanceData;
		elem.InstanceDataStepRate = InstanceDataStepRate;

		Layouts.push_back(elem);
	}
	void SetShaderDesc(IShaderDesc* shader)
	{
		ShaderDesc.StrongRef(shader);
	}
	const LayoutElement* FindElementBySemantic(const char* name, UINT index);
};

class IInputLayout : public IRenderResource
{
public:
	IInputLayout();
	~IInputLayout();

	IInputLayoutDesc		mDesc;
public:
	inline UINT GetElemNumber() const {
		return (UINT)mDesc.Layouts.size();
	}
	inline const LayoutElement* GetElement(UINT StreamIndex) const {
		for (size_t i = 0; i < mDesc.Layouts.size(); i++)
		{
			if (mDesc.Layouts[i].InputSlot == StreamIndex)
				return &mDesc.Layouts[i];
		}
		return nullptr;
	}
};

NS_END