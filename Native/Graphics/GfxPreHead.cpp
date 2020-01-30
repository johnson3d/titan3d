#include "GfxPreHead.h"

#define new VNEW

NS_BEGIN

std::vector<LayoutElement> GLayouts;

const std::vector<LayoutElement>& GetEngineVertexLayout() 
{
	if (GLayouts.size() == 0)
	{
		GLayouts.resize(EVertexSteamType::VST_Number);
		LayoutElement tmpElem;
		//VST_Position,
		tmpElem.SemanticName = "POSITION";
		tmpElem.SemanticIndex = 0;
		tmpElem.Format = PXF_R32G32B32_FLOAT;
		tmpElem.InputSlot = VST_Position;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_Position] = tmpElem;

		//VST_Normal,
		tmpElem.SemanticName = "NORMAL";
		tmpElem.SemanticIndex = 0;
		tmpElem.Format = PXF_R32G32B32_FLOAT;
		tmpElem.InputSlot = VST_Normal;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_Normal] = tmpElem;

		//VST_Tangent,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 0;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_Tangent;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_Tangent] = tmpElem;

		//VST_Color,
		tmpElem.SemanticName = "COLOR";
		tmpElem.SemanticIndex = 0;
		tmpElem.Format = PXF_R8G8B8A8_UNORM;
		tmpElem.InputSlot = VST_Color;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_Color] = tmpElem;

		//VST_UV,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 1;
		tmpElem.Format = PXF_R32G32_FLOAT;
		tmpElem.InputSlot = VST_UV;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_UV] = tmpElem;

		//VST_LightMap,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 2;
		tmpElem.Format = PXF_R32G32_FLOAT;
		tmpElem.InputSlot = VST_LightMap;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_LightMap] = tmpElem;

		//VST_SkinIndex,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 3;
		tmpElem.Format = PXF_R8G8B8A8_UINT;
		tmpElem.InputSlot = VST_SkinIndex;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_SkinIndex] = tmpElem;

		//VST_SkinWeight,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 4;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_SkinWeight;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_SkinWeight] = tmpElem;

		//VST_TerrainIndex,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 5;
		tmpElem.Format = PXF_R8G8B8A8_UINT;
		tmpElem.InputSlot = VST_TerrainIndex;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_TerrainIndex] = tmpElem;

		//VST_TerrainGradient,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 6;
		tmpElem.Format = PXF_R8G8B8A8_UINT;
		tmpElem.InputSlot = VST_TerrainGradient;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_TerrainGradient] = tmpElem;

		//VST_InstPos,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 7;
		tmpElem.Format = PXF_R32G32B32_FLOAT;
		tmpElem.InputSlot = VST_InstPos;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = true;
		tmpElem.InstanceDataStepRate = 1;
		GLayouts[VST_InstPos] = tmpElem;

		//VST_InstQuat,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 8;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_InstQuat;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = true;
		tmpElem.InstanceDataStepRate = 1;
		GLayouts[VST_InstQuat] = tmpElem;

		//VST_InstScale,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 9;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_InstScale;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = true;
		tmpElem.InstanceDataStepRate = 1;
		GLayouts[VST_InstScale] = tmpElem;

		//VST_F4_1,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 10;
		tmpElem.Format = PXF_R32G32B32A32_UINT; //PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_F4_1;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = true;
		tmpElem.InstanceDataStepRate = 1;
		GLayouts[VST_F4_1] = tmpElem;

		//VST_F4_2,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 11;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_F4_2;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_F4_2] = tmpElem;

		//VST_F4_3,
		tmpElem.SemanticName = "TEXCOORD";
		tmpElem.SemanticIndex = 12;
		tmpElem.Format = PXF_R32G32B32A32_FLOAT;
		tmpElem.InputSlot = VST_F4_3;
		tmpElem.AlignedByteOffset = 0;
		tmpElem.IsInstanceData = false;
		tmpElem.InstanceDataStepRate = 0;
		GLayouts[VST_F4_3] = tmpElem;
	}

	return GLayouts;
}

int GetEngineVertexBufferStride(EVertexSteamType stream)
{
	switch (stream)
	{
	case EngineNS::VST_Position:
		return sizeof(v3dxVector3);
	case EngineNS::VST_Normal:
		return sizeof(v3dxVector3);
	case EngineNS::VST_Tangent:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_Color:
		return sizeof(DWORD);
	case EngineNS::VST_UV:
		return sizeof(v3dxVector2);
	case EngineNS::VST_LightMap:
		return sizeof(v3dxVector2);
	case EngineNS::VST_SkinIndex:
		return sizeof(DWORD);
	case EngineNS::VST_SkinWeight:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_TerrainIndex:
		return sizeof(DWORD);
	case EngineNS::VST_TerrainGradient:
		return sizeof(DWORD);
	case EngineNS::VST_InstPos:
		return sizeof(v3dxVector3);
	case EngineNS::VST_InstQuat:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_InstScale:
		return sizeof(v3dxVector3);
	case EngineNS::VST_F4_1:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_F4_2:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_F4_3:
		return sizeof(v3dxQuaternion);
	case EngineNS::VST_Number:
		return sizeof(v3dxQuaternion);
	default:
		return -1;
	}
}

NS_END

using namespace EngineNS;
extern "C"
{
	VFX_API void SDK_IInputLayoutDesc_SetEngineVertexLayout(IInputLayoutDesc* self)
	{
		if (self == nullptr)
			return;
		self->Layouts = GetEngineVertexLayout();
	}
}