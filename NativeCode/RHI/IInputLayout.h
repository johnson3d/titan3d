#pragma once
#include "IRenderResource.h"
#include "IShader.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
LayoutElement
{
	UINT SemanticIndex;
	EPixelFormat Format;
	UINT InputSlot;
	UINT AlignedByteOffset;
	vBOOL IsInstanceData;
	UINT InstanceDataStepRate;
	VNameString SemanticName;
	int GLAttribLocation;
};

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IInputLayoutDesc : public VIUnknown
{
	RTTI_DEF(IInputLayoutDesc, 0x47e443e45b0395a3, true);
	IInputLayoutDesc()
	{

	}
	std::vector<LayoutElement>	Layouts;
	//only for d11,try to match input & vs layout.maybe It is not necessary
	AutoRef<IShaderDesc>		ShaderDesc;

	TR_FUNCTION()
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
		elem.GLAttribLocation = -1;

		Layouts.push_back(elem);
	}
	TR_FUNCTION()
	void SetShaderDesc(IShaderDesc* shader)
	{
		ShaderDesc.StrongRef(shader);
	}
	TR_FUNCTION()
	const LayoutElement* FindElementBySemantic(const char* name, UINT index);

	TR_FUNCTION()
	UINT64 GetLayoutHash64();
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IInputLayout : public IRenderResource
{
public:
	IInputLayout();
	~IInputLayout();

	TR_MEMBER(SV_NoBind)
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