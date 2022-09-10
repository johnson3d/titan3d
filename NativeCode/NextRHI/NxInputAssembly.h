#pragma once
#include "NxBuffer.h"

NS_BEGIN

namespace NxRHI
{
	struct TR_CLASS(SV_LayoutStruct = 8)
		FLayoutElement
	{
		UINT SemanticIndex;
		EPixelFormat Format;
		EVertexStreamType InputSlot;
		UINT AlignedByteOffset;
		vBOOL IsInstanceData;
		UINT InstanceDataStepRate;
		VNameString SemanticName;
		int GLAttribLocation;
	};

	struct TR_CLASS()
		FInputLayoutDesc : public VIUnknownBase
	{
		ENGINE_RTTI(FInputLayoutDesc);
		FInputLayoutDesc()
		{
			
		}
		std::vector<FLayoutElement>	Layouts;
		AutoRef<FShaderDesc> ShaderDesc;
		
		void SetShaderDesc(FShaderDesc* desc) {
			ShaderDesc = desc;
		}
		void AddElement(const char* SemanticName,
			UINT SemanticIndex,
			EPixelFormat Format,
			EVertexStreamType InputSlot,
			UINT AlignedByteOffset,
			vBOOL IsInstanceData,
			UINT InstanceDataStepRate)
		{
			FLayoutElement elem;
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
		
		//const FLayoutElement* FindElementBySemantic(const char* name, UINT index);

		UINT64 GetLayoutHash64();
	};

	class TR_CLASS()
		IInputLayout : public IGpuResource
	{
	public:
		ENGINE_RTTI(IInputLayout);

		FInputLayoutDesc* GetLayoutDesc() {
			return mDesc;
		}
	public:
		AutoRef<FInputLayoutDesc>		mDesc;
	};
}

NS_END
