#include "IGeometryMesh.h"
#include "IPass.h"
#include "IRenderContext.h"
#include "../Math/v3dxVector2.h"
#include "../Math/v3dxVector3.h"
#include "../Math/v3dxBox3.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IGeometryMesh, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IVertexArray, EngineNS::VIUnknown);

IVertexArray::~IVertexArray()
{
	for (int i = 0; i < VST_Number; i++)
	{
		Safe_Release(VertexBuffers[i]);
	}
}

void IVertexArray::ApplyVBs(ICommandList* cmd, IPass* pass, vBOOL bImm)
{
	for (int i = 0; i < VST_Number; i++)
	{
		if (VertexBuffers[i] == nullptr)
			continue;
		pass->SetVertexBuffer(cmd, i, VertexBuffers[i], 0, VertexBuffers[i]->mDesc.Stride);
	}
}


IGeometryMesh::~IGeometryMesh()
{
	InvalidateResource();
}

void IGeometryMesh::InvalidateResource()
{
	for (int i = 0; i < VST_Number; i++)
	{
		Safe_Release(VertexBuffers[i]);
		MopherKeys[i].FinalCleanup();
	}
	Safe_Release(IndexBuffer);
}

vBOOL IGeometryMesh::ApplyGeometry(ICommandList* cmd, IPass* pass, vBOOL bImm)
{
	for (int i = 0; i < VST_Number; i++)
	{
		auto vb = VertexBuffers[i];
		//pass->SetVertexBuffer(cmd, i, vb, 0, vb->mDesc.Stride);
		if (vb == nullptr)
		{
			//continue;
			pass->SetVertexBuffer(cmd, i, nullptr, 0, 0);
		}
		else
		{
			//vb->UpdateDrawPass(cmd, bImm);
			pass->SetVertexBuffer(cmd, i, vb, 0, vb->mDesc.Stride);
		}
	}
	if(IndexBuffer!=nullptr)
		pass->SetIndexBuffer(cmd, IndexBuffer);

	return TRUE;
}

UINT GetVBStride(EVertexSteamType type)
{
	switch (type)
	{
	case EngineNS::VST_Position:
		return sizeof(v3dxVector3);
	case EngineNS::VST_Normal:
		return sizeof(v3dxVector3);
	case EngineNS::VST_Tangent:
		return sizeof(v3dVector4_t);
	case EngineNS::VST_Color:
		return sizeof(UINT);
	case EngineNS::VST_UV:
		return sizeof(v3dxVector2);
	case EngineNS::VST_LightMap:
		return sizeof(v3dxVector2);
	case EngineNS::VST_SkinIndex:
		return sizeof(UINT);
	case EngineNS::VST_SkinWeight:
		return sizeof(v3dVector4_t);
	case EngineNS::VST_TerrainIndex:
		return sizeof(UINT);
	case EngineNS::VST_TerrainGradient:
		return sizeof(UINT);
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
		return sizeof(v3dxVector3);
	}
}

IGeometryMesh* IGeometryMesh::MergeGeoms(IRenderContext* rc, IGeometryMesh** meshArray, v3dxMatrix4* transform, int count, v3dxBox3* aabb)
{
	auto result = new IGeometryMesh();
	std::vector<v3dxVector3> vertPos;
	std::vector<BYTE> verts[VST_Number];
	std::vector<int> ibData;
	aabb->InitializeBox();
	int currentMeshStartIndex = 0;
	for (int i = 0; i < count; i++)
	{
		auto mesh = meshArray[i];
		v3dxMatrix4 transMatrix = transform[i];
		if (mesh->IndexBuffer->mDesc.Type == IBT_Int16)
		{
			int idxCount = mesh->IndexBuffer->mDesc.ByteWidth / sizeof(USHORT);
			IBlobObject idxData;
			mesh->IndexBuffer->GetBufferData(rc, &idxData);
			auto pSrc = (USHORT*)idxData.GetData();
			for (int j = 0; j < idxCount; j++)
			{
				ibData.push_back(currentMeshStartIndex + pSrc[j]);
			}
		}
		else
		{
			int idxCount = mesh->IndexBuffer->mDesc.ByteWidth / sizeof(int);
			IBlobObject idxData;
			mesh->IndexBuffer->GetBufferData(rc, &idxData);
			auto pSrc = (int*)idxData.GetData();
			for (int j = 0; j < idxCount; j++)
			{
				ibData.push_back(currentMeshStartIndex + pSrc[j]);
			}
		}

		IBlobObject subVerts[VST_Number];
		int vertCount = mesh->VertexBuffers[VST_Position]->mDesc.ByteWidth / (sizeof(float)*3);// sizeof(v3dxVector3);
		{
			IBlobObject posSub;
			mesh->VertexBuffers[VST_Position]->GetBufferData(rc, &posSub);
			auto src = (v3dxVector3*)posSub.GetData();
			for (int j = 0; j < vertCount; j++)
			{
				auto nPos = src[j] * transMatrix;

				vertPos.push_back(nPos);
				aabb->OptimalVertex(nPos);
			}
		}
		for (int j = 0; j < VST_Number; j++)
		{
			auto vb = mesh->VertexBuffers[j];
			if(vb==nullptr)
				continue;

			vb->GetBufferData(rc, &subVerts[j]);
			if (j == VST_Position)
			{
				continue;
			}
			else
			{
				for (int i = 0; i < subVerts[j].mDatas.GetSize(); i++)
				{
					verts[j].push_back(subVerts[j].mDatas[i]);
				}
				//verts[j].insert(verts[j].begin(), subVerts[j].mDatas.begin(), subVerts[j].mDatas.end());
			}
		}
		currentMeshStartIndex += vertCount;
	}

	IVertexBufferDesc vbDesc;
	for (int i = 0; i < VST_Number; i++)
	{
		vbDesc.Stride = GetVBStride((EVertexSteamType)i);
		if (i == VST_Position)
		{
			vbDesc.ByteWidth = (UINT)vertPos.size() * sizeof(v3dxVector3);
			auto pAddr = &vertPos[0];
			vbDesc.InitData = pAddr;
			vbDesc.Stride = sizeof(v3dxVector3);
			AutoRef<IVertexBuffer> nvb = rc->CreateVertexBuffer(&vbDesc);
			result->BindVertexBuffer((EVertexSteamType)i, nvb);
		}
		else
		{
			vbDesc.ByteWidth = (UINT)verts[i].size();
			if (vbDesc.ByteWidth == 0)
				continue;

			vbDesc.InitData = &verts[i][0];
			AutoRef<IVertexBuffer> nvb = rc->CreateVertexBuffer(&vbDesc);
			result->BindVertexBuffer((EVertexSteamType)i, nvb);
		}
	}
	IIndexBufferDesc ibDesc;
	ibDesc.Type = IBT_Int32;
	ibDesc.ByteWidth = (UINT)ibData.size() * sizeof(int);
	ibDesc.InitData = &ibData[0];
	result->IndexBuffer = rc->CreateIndexBuffer(&ibDesc);

	result->SetIsDirty(TRUE);
	return result;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI2(EngineNS, IVertexArray, BindVertexBuffer, EVertexSteamType, IVertexBuffer*);
	CSharpReturnAPI1(IVertexBuffer*, EngineNS, IVertexArray, GetVertexBuffer, EVertexSteamType);
	CSharpAPI1(EngineNS, IVertexArray, SetNumInstances, UINT);
	CSharpReturnAPI0(UINT, EngineNS, IVertexArray, GetNumInstances);

	VFX_API IGeometryMesh* SDK_IGeometryMesh_MergeGeoms(IRenderContext* rc, IGeometryMesh** meshArray, v3dxMatrix4* transform, int count, v3dxBox3* aabb)
	{
		return IGeometryMesh::MergeGeoms(rc, meshArray, transform, count, aabb);
	}
	CSharpReturnAPI0(vBOOL, EngineNS, IGeometryMesh, GetIsDirty);
	CSharpAPI1(EngineNS, IGeometryMesh, SetIsDirty, vBOOL);

	CSharpAPI2(EngineNS, IGeometryMesh, BindVertexBuffer, EVertexSteamType, IVertexBuffer*);
	CSharpAPI1(EngineNS, IGeometryMesh, BindIndexBuffer, IIndexBuffer*);
	CSharpReturnAPI1(IVertexBuffer*, EngineNS, IGeometryMesh, GetVertexBuffer, EVertexSteamType);
	CSharpReturnAPI0(IIndexBuffer*, EngineNS, IGeometryMesh, GetIndexBuffer);
}