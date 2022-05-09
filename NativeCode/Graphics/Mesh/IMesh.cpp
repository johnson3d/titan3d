#include "IMesh.h"

#define new VNEW

NS_BEGIN

IMesh::IMesh()
{

}

void IMesh::Initialize(IMeshPrimitives* mesh, IMdfQueue* mdf)
{
	mGeoms.StrongRef(mesh);
	mMdfQueue.StrongRef(mdf);
	mVertexArray = MakeWeakRef(new IVertexArray());
}

void IMesh::SetInputStreams(IVertexArray* draw)
{
	if (mVertexArray == nullptr)
		return;

	mVertexArray->Reset();
	for (auto i : mMdfQueue->mMdfQueue)
	{
		i->SetInputStreams(mGeoms, draw);
	}
}

IMesh* IMesh::CloneMesh()
{
	auto result = new IMesh();
	result->mGeoms = mGeoms;
	auto mdf = mMdfQueue->CloneMdfQueue();
	result->mMdfQueue.WeakRef(mdf);

	result->SetInputStreams(result->mVertexArray);
	return result;
}


void GetEngineVertexLayout(std::vector<LayoutElement>& GLayouts)
{
	if (GLayouts.size() == 0)
	{
		GLayouts.resize(EVertexStreamType::VST_Number);
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
}

IInputLayoutDesc* IMesh::CreateInputLayoutDesc(UINT streams)
{
	auto result = new IInputLayoutDesc();
	GetEngineVertexLayout(result->Layouts);
	return result;
	
	//for (int i = 0; i < VST_Number; i++)
	//{
	//	if (streams & (1 << i))
	//	{
	//		//result->AddElement()
	//	}
	//}
	//return result;
}

NS_END