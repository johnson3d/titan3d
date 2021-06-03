#include "IMeshPrimitives.h"
#include "../IRenderContext.h"
#include "../ISamplerState.h"
#include "../IShaderResourceView.h"
#include "../../Base/thread/vfxthread.h"
#include "../../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::IMeshPrimitives, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::IMeshDataProvider, EngineNS::VIUnknown);

void IMeshPrimitives::CalcNormals32(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const UINT* triangles, UINT nTri)
{
	struct VertexFaces
	{
		std::vector<UINT>		Faces;
	};

	std::vector<VertexFaces> VtxFaces;
	VtxFaces.resize(nVert);

	for (UINT i = 0; i < nVert; i++)
	{
		for (UINT j = 0; j < nTri; j++)
		{
			if (triangles[3 * j + 0] == i ||
				triangles[3 * j + 1] == i ||
				triangles[3 * j + 2] == i)
			{
				VtxFaces[i].Faces.push_back((UINT)j);
			}
		}
	}

	normals.resize(nVert);
	for (UINT i = 0; i < nVert; i++)
	{
		normals[i].setValue(0, 0, 0);
		for (auto j : VtxFaces[i].Faces)
		{
			auto a = triangles[3 * j + 0];
			auto b = triangles[3 * j + 1];
			auto c = triangles[3 * j + 2];

			const v3dxVector3& vA = pos[a];
			const v3dxVector3& vB = pos[b];
			const v3dxVector3& vC = pos[c];

			v3dxVector3 nor;
			v3dxCalcNormal(&nor, &vA, &vB, &vC, TRUE);
			normals[i] += nor;
		}
		normals[i] /= (float)VtxFaces[i].Faces.size();
		normals[i].normalize();
	}
}

void IMeshPrimitives::CalcNormals16(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const USHORT* triangles, UINT nTri)
{
	struct VertexFaces
	{
		std::vector<UINT>		Faces;
	};

	std::vector<VertexFaces> VtxFaces;
	VtxFaces.resize(nVert);

	for (UINT i = 0; i < nVert; i++)
	{
		for (UINT j = 0; j < nTri; j++)
		{
			if ((UINT)triangles[3 * j + 0] == i ||
				(UINT)triangles[3 * j + 1] == i ||
				(UINT)triangles[3 * j + 2] == i)
			{
				VtxFaces[i].Faces.push_back((UINT)j);
			}
		}
	}

	normals.resize(nVert);
	for (UINT i = 0; i < nVert; i++)
	{
		normals[i].setValue(0, 0, 0);
		for (auto j : VtxFaces[i].Faces)
		{
			auto a = triangles[3 * j + 0];
			auto b = triangles[3 * j + 1];
			auto c = triangles[3 * j + 2];

			const v3dxVector3& vA = pos[a];
			const v3dxVector3& vB = pos[b];
			const v3dxVector3& vC = pos[c];

			v3dxVector3 nor;
			v3dxCalcNormal(&nor, &vA, &vB, &vC, TRUE);
			normals[i] += nor;
		}
		normals[i] /= (float)VtxFaces[i].Faces.size();
		normals[i].normalize();
	}
}

IMeshPrimitives::IMeshPrimitives()
{
}

IMeshPrimitives::~IMeshPrimitives()
{
	Cleanup();
}

void IMeshPrimitives::Cleanup()
{

}

vBOOL IMeshPrimitives::Init(IRenderContext* rc, const char* name, UINT atom)
{
	mContext.FromObject(rc);
	mName = name;
	mAtoms.resize(atom);
	mDesc.AtomNumber = atom;

	mGeometryMesh = rc->CreateGeometryMesh();

	return TRUE;
}

vBOOL IMeshPrimitives::InitFromGeomtryMesh(IRenderContext* rc, IGeometryMesh* mesh, UINT atom, const v3dxBox3* aabb)
{
	ASSERT(GLogicThreadId == vfxThread::GetCurrentThreadId());
	mContext.FromObject(rc);
	mAtoms.resize(atom);
	mDesc.AtomNumber = atom;

	mGeometryMesh.StrongRef(mesh);

	mAABB = *aabb;

	this->GetResourceState()->SetStreamState(SS_Valid);

	return TRUE;
}

void IMeshPrimitives::Save2Xnd(IRenderContext* rc, XndNode* pNode)
{
	XndAttribute* pAttr = pNode->GetOrAddAttribute("HeadAttrib", 0, 0);
	pAttr->BeginWrite();
	{
		auto pos_vb = mGeometryMesh->GetVertexBuffer(VST_Position);
		mDesc.AtomNumber = (UINT)mAtoms.size();
		mDesc.PolyNumber = mAtoms[0][0].NumPrimitives / 3;
		mDesc.VertexNumber = pos_vb->mDesc.ByteWidth / sizeof(v3dxVector3);
		mDesc.Flags = 0;
		mDesc.UnUsed = 0;
		mDesc.GeoTabeNumber = 0;
		pAttr->Write(mDesc);
	}
	
	pAttr->Write(mAABB);
	pAttr->EndWrite();

	pAttr = pNode->GetOrAddAttribute("RenderAtom", 0, 0);
	pAttr->BeginWrite();
	for (size_t i = 0; i < mAtoms.size(); i++)
	{
		const auto& dpDesc = mAtoms[i][0];
		pAttr->Write(dpDesc.PrimitiveType);
		UINT uLodLevel = (UINT)mAtoms[i].size();
		pAttr->Write(uLodLevel);
		for (UINT j = 0; j < uLodLevel; j++)
		{
			const auto& dpDesc1 = mAtoms[i][j];
			pAttr->Write(dpDesc1.StartIndex);
			pAttr->Write(dpDesc1.NumPrimitives);
		}
	}
	pAttr->EndWrite();

	auto vb = mGeometryMesh->GetVertexBuffer(VST_Position);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("Position", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_Position], sizeof(v3dxVector3));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_Normal);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("Normal", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_Normal], sizeof(v3dxVector3));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_Tangent);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("Tangent", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_Tangent], sizeof(v3dVector4_t));
	}
	/*vb = mGeometryMesh->GetVertexBuffer(VST_Tangent);
	if (vb)
	{
		pAttr = pNode->AddAttribute("Binormal");
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_Tangent], sizeof(v3dxVector3));
	}*/
	vb = mGeometryMesh->GetVertexBuffer(VST_UV);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("DiffuseUV", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_UV], sizeof(v3dxVector2));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_LightMap);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("LightMapUV", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_LightMap], sizeof(v3dxVector2));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_Color);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("VertexColor", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_Color], sizeof(DWORD));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_SkinIndex);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("BlendIndex", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_SkinIndex], sizeof(DWORD));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_SkinWeight);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("BlendWeight", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_SkinWeight], sizeof(v3dVector4_t));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_TerrainIndex);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("Fix_VIDTerrain", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_TerrainIndex], sizeof(DWORD));
	}
	vb = mGeometryMesh->GetVertexBuffer(VST_TerrainGradient);
	if (vb)
	{
		pAttr = pNode->GetOrAddAttribute("TerrainGradient", 0, 0);
		SaveVB(rc, pAttr, vb, mGeometryMesh->MopherKeys[VST_TerrainGradient], sizeof(DWORD));
	}
	
	auto ib = mGeometryMesh->GetIndexBuffer();
	if (ib)
	{
		pAttr = pNode->GetOrAddAttribute("Indices", 0, 0);
		pAttr->BeginWrite();

		const IIndexBufferDesc& desc = ib->mDesc;

		UINT count = 0;
		if (desc.Type == IBT_Int32)
		{
			count = desc.ByteWidth / sizeof(DWORD);
			pAttr->Write(count);
			vBOOL bFormatIndex32 = 1;
			pAttr->Write(bFormatIndex32);
		}
		else
		{
			count = desc.ByteWidth / sizeof(WORD);
			pAttr->Write(count);
			vBOOL bFormatIndex32 = 0;
			pAttr->Write(bFormatIndex32);
		}
		
		IBlobObject buffData;
		while(buffData.GetSize() == 0)
		{
			ib->GetBufferData(rc, &buffData);
			Sleep(10);
		}
		pAttr->Write(buffData.GetData(), desc.ByteWidth);
		pAttr->EndWrite();
	}
}

vBOOL IMeshPrimitives::LoadXnd(IRenderContext* rc, const char* name, XndHolder* xnd, bool isLoad)
{
	if (xnd == nullptr)
		return FALSE;
	mContext.FromObject(rc);
	mXnd.StrongRef(xnd);
	mName = name;
	auto pNode = xnd->GetRootNode();
	if (mGeometryMesh == nullptr)
	{
		mGeometryMesh = rc->CreateGeometryMesh();
	}
	else
	{
		mGeometryMesh->Cleanup();
	}

	XndAttribute* pAttr = pNode->TryGetAttribute("HeadAttrib");
	if (pAttr)
	{
		pAttr->BeginRead();
		pAttr->Read(mDesc);
		pAttr->Read(mAABB);
		pAttr->EndRead();
	}
	pAttr = pNode->TryGetAttribute("RenderAtom");
	if (pAttr)
	{
		mAtoms.clear();
		mAtoms.resize(mDesc.AtomNumber);
		pAttr->BeginRead();
		for (size_t i = 0; i < mDesc.AtomNumber; i++)
		{
			DrawPrimitiveDesc dpDesc;
			pAttr->Read(dpDesc.PrimitiveType);
			UINT uLodLevel;
			pAttr->Read(uLodLevel);
			for (UINT j = 0; j < uLodLevel; j++)
			{
				pAttr->Read(dpDesc.StartIndex);
				pAttr->Read(dpDesc.NumPrimitives);
				mAtoms[i].push_back(dpDesc);
			}
		}
		pAttr->EndRead();
	}
	else
	{
		return FALSE;
	}

	if (mAABB.IsEmpty())
	{
		isLoad = true;
		VFX_LTRACE(ELTT_Resource, "Mesh %s AABB is empty, Core will Restore object and recalculate it\r\n", this->GetName());
	}
	mXnd->TryReleaseHolder();
	if (isLoad == false)
	{
		
		return TRUE;
	}
	else
	{
		return RestoreResource();
	}
}

void IMeshPrimitives::InvalidateResource()
{
	mGeometryMesh->InvalidateResource(); 
}

const char* VBType2String(EVertexSteamType type)
{
	switch (type)
	{
	case EngineNS::VST_Position:
		return "Pos";
	case EngineNS::VST_Normal:
		return "Nor";
	case EngineNS::VST_Tangent:
		return "Tan";
	case EngineNS::VST_Color:
		return "Color";
	case EngineNS::VST_UV:
		return "UV";
	case EngineNS::VST_LightMap:
		return "LightMap";
	case EngineNS::VST_SkinIndex:
		return "SkinIndex";
	case EngineNS::VST_SkinWeight:
		return "SkinWeight";
	case EngineNS::VST_TerrainIndex:
		return "TerrainIndex";
	case EngineNS::VST_TerrainGradient:
		return "TerrainGradient";
	case EngineNS::VST_InstPos:
		return "InstPos";
	case EngineNS::VST_InstQuat:
		return "InstQuat";
	case EngineNS::VST_InstScale:
		return "InstScale";
	case EngineNS::VST_F4_1:
		return "F41";
	case EngineNS::VST_F4_2:
		return "F42";
	case EngineNS::VST_F4_3:
		return "F43";
	default:
		return "Unknown";
	}
}

AutoRef<IVertexBuffer> IMeshPrimitives::LoadVB(IRenderContext* rc, XndAttribute* pAttr, UINT stride, TimeKeys& tkeys, UINT& resSize, EVertexSteamType stream)
{
	pAttr->BeginRead();
	UINT uVert, uKey, uStride;
	pAttr->Read(uStride);
	pAttr->Read(uVert);
	pAttr->Read(uKey);
	tkeys.Load(*pAttr);
	IVertexBufferDesc desc;
	desc.CPUAccess = 0;
	desc.Stride = uStride;
	ASSERT(desc.Stride == stride);
	desc.ByteWidth = uStride * uKey * uVert;
	BYTE* data = new BYTE[desc.ByteWidth];
	//data = new BYTE[desc.ByteWidth];
	pAttr->Read(data, desc.ByteWidth);
	desc.InitData = data;
	pAttr->EndRead();

	if (stream == VST_Position && mAABB.IsEmpty())
	{	
		mAABB.InitializeBox();
		v3dxVector3* pPosition = (v3dxVector3*)data;
		for (UINT i = 0; i < uVert; i++)
		{
			mAABB.OptimalVertex(pPosition[i]);
		}
	}

	AutoRef<IVertexBuffer> vb = rc->CreateVertexBuffer(&desc);
	Safe_DeleteArray(data);

	resSize += desc.ByteWidth;
	return vb;
}

void IMeshPrimitives::SaveVB(IRenderContext* rc, XndAttribute* pAttr, IVertexBuffer* vb, TimeKeys& tkeys, UINT stride)
{
	IBlobObject buffData;
	vb->GetBufferData(rc, &buffData);
	if (buffData.GetSize() == 0)
		return;

	pAttr->BeginWrite();
	
	UINT uVert, uKey;
	uKey = tkeys.GetKeyCount();
	if (uKey == 0)
		uKey = 1;
	uVert = (UINT)buffData.GetSize() / (uKey*stride);
	pAttr->Write(stride);
	pAttr->Write(uVert);
	pAttr->Write(uKey);
	tkeys.Save(*pAttr);
	pAttr->Write(buffData.GetData(), (UINT)buffData.GetSize());

	pAttr->EndWrite();
}

vBOOL IMeshPrimitives::RefreshResource(IRenderContext* rc, const char* name, XndNode* node)
{
	if (GetResourceState()->GetStreamState() == SS_Streaming)
		return FALSE;
	if (mXnd != nullptr)
	{
		mXnd->TryReleaseHolder();
		if (LoadXnd(mContext.GetPtr(), mName.c_str(), mXnd, true))
		{
			GetResourceState()->SetStreamState(SS_Valid);
			return TRUE;
		}
	}
	return FALSE;
}

vBOOL IMeshPrimitives::RestoreResource()
{
	if (mXnd == nullptr)
		return false;

	auto rc = mContext.GetPtr();
	if (rc == nullptr)
		return false;

	auto pNode = mXnd->GetRootNode();
	UINT resSize = 0;
	XndAttribute* pAttr = pNode->TryGetAttribute("Position");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dxVector3), mGeometryMesh->MopherKeys[VST_Position], resSize, VST_Position);
		mGeometryMesh->BindVertexBuffer(VST_Position, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":Pos").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("Normal");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dxVector3), mGeometryMesh->MopherKeys[VST_Normal], resSize, VST_Normal);
		mGeometryMesh->BindVertexBuffer(VST_Normal, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":Nor").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("Tangent");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dVector4_t), mGeometryMesh->MopherKeys[VST_Tangent], resSize, VST_Tangent);
		mGeometryMesh->BindVertexBuffer(VST_Tangent, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":Tan").c_str());
#endif
	}
	/*pAttr = pNode->TryGetAttribute("Binormal");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dxVector3), mGeometryMesh->MopherKeys[VST_Tangent], resSize);
		mGeometryMesh->BindVertexBuffer(VST_Tangent, vb);
	}*/
	pAttr = pNode->TryGetAttribute("DiffuseUV");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dxVector2), mGeometryMesh->MopherKeys[VST_UV], resSize, VST_UV);
		mGeometryMesh->BindVertexBuffer(VST_UV, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":UV").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("LightMapUV");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dxVector2), mGeometryMesh->MopherKeys[VST_LightMap], resSize, VST_LightMap);
		mGeometryMesh->BindVertexBuffer(VST_LightMap, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":LightMap").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("VertexColor");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(DWORD), mGeometryMesh->MopherKeys[VST_Color], resSize, VST_Color);
		mGeometryMesh->BindVertexBuffer(VST_Color, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":Color").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("BlendIndex");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(DWORD), mGeometryMesh->MopherKeys[VST_SkinIndex], resSize, VST_SkinIndex);
		mGeometryMesh->BindVertexBuffer(VST_SkinIndex, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":BlendIndex").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("BlendWeight");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(v3dVector4_t), mGeometryMesh->MopherKeys[VST_SkinWeight], resSize, VST_SkinWeight);
		mGeometryMesh->BindVertexBuffer(VST_SkinWeight, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":BlendWeight").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("Fix_VIDTerrain");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(DWORD), mGeometryMesh->MopherKeys[VST_TerrainIndex], resSize, VST_TerrainIndex);
		mGeometryMesh->BindVertexBuffer(VST_TerrainIndex, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":TerrainIndex").c_str());
#endif
	}
	pAttr = pNode->TryGetAttribute("TerrainGradient");
	if (pAttr)
	{
		TimeKeys tkeys;
		auto vb = LoadVB(rc, pAttr, sizeof(DWORD), mGeometryMesh->MopherKeys[VST_TerrainGradient], resSize, VST_TerrainGradient);
		mGeometryMesh->BindVertexBuffer(VST_TerrainGradient, vb);
#if _DEBUG
		vb->SetDebugInfo((mName + ":TerrainGradient").c_str());
#endif
	}

	pAttr = pNode->TryGetAttribute("Indices");
	if (pAttr)
	{
		pAttr->BeginRead();

		IIndexBufferDesc desc;
		desc.CPUAccess = 0;

		UINT count;
		pAttr->Read(count);
		vBOOL bFormatIndex32;
		pAttr->Read(bFormatIndex32);
		desc.ByteWidth = count * ((bFormatIndex32) ? sizeof(DWORD) : sizeof(WORD));
		desc.Type = bFormatIndex32 ? IBT_Int32 : IBT_Int16;

		BYTE* data = new BYTE[desc.ByteWidth];
		pAttr->Read(data, desc.ByteWidth);

		pAttr->EndRead();

		desc.InitData = data;
		AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&desc);
		mGeometryMesh->BindIndexBuffer(ib);
		Safe_DeleteArray(data);

		//ib->SetDebugInfo((mName + ":IB").c_str());

		resSize += desc.ByteWidth;
	}

	mXnd->TryReleaseHolder();
	this->GetResourceState()->SetResourceSize(resSize);
	mGeometryMesh->mIsDirty = TRUE;

	return true;
}

const char* IMeshPrimitives::GetName() const
{
	return mName.c_str();
}

UINT IMeshPrimitives::GetAtomNumber() const
{
	return (UINT)mAtoms.size();
}

vBOOL IMeshPrimitives::GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const
{
	if (index >= (UINT)mAtoms.size())
		return FALSE;
	
	if (lod >= (UINT)mAtoms[index].size())
		return FALSE;

	*desc = mAtoms[index][lod];
	return TRUE;
}

vBOOL IMeshPrimitives::SetAtom(UINT index, UINT lod, const DrawPrimitiveDesc* desc)
{
	if (index >= (UINT)mAtoms.size())
		return FALSE;

	if (lod >= (UINT)mAtoms[index].size())
		return FALSE;

	mAtoms[index][lod] = *desc;
	return TRUE;
}

void IMeshPrimitives::PushAtomLOD(UINT index, const DrawPrimitiveDesc* desc)
{
	mAtoms[index].push_back(*desc);
}

UINT IMeshPrimitives::GetAtomLOD(UINT index)
{
	return (UINT)mAtoms[index].size();
}

UINT IMeshPrimitives::GetLodLevel(UINT index, float lod)
{
	auto num = (float)(GetAtomLOD(index));
	
	return (UINT)(num * lod);
}

vBOOL IMeshPrimitives::SetGeomtryMeshStream(IRenderContext* rc, EVertexSteamType stream, void* data, UINT size, UINT stride, UINT cpuAccess)
{
	IVertexBufferDesc desc;
	desc.ByteWidth = size;
	desc.Stride = stride;
	desc.CPUAccess = cpuAccess;
	desc.InitData = data;
	AutoRef<IVertexBuffer> vb = rc->CreateVertexBuffer(&desc);
	if (vb == nullptr)
		return FALSE;
	mGeometryMesh->BindVertexBuffer(stream, vb);
	return TRUE;
}

vBOOL IMeshPrimitives::SetGeomtryMeshIndex(IRenderContext* rc, void* data, UINT size, EIndexBufferType type, UINT cpuAccess)
{
	IIndexBufferDesc desc;
	desc.ByteWidth = size;
	desc.CPUAccess = cpuAccess;
	desc.Type = type;
	desc.InitData = data;

	AutoRef<IIndexBuffer> ib = rc->CreateIndexBuffer(&desc);
	if (ib == nullptr)
		return FALSE;
	mGeometryMesh->BindIndexBuffer(ib);
	return TRUE;
}

//-----------------------------------------

IMeshDataProvider::IMeshDataProvider()
{
	memset(mVertexBuffers, 0, sizeof(mVertexBuffers));
	IndexBuffer = nullptr;
	IBType = IBT_Int16;
}

IMeshDataProvider::~IMeshDataProvider()
{
	Cleanup();
}

void IMeshDataProvider::Cleanup()
{
	for (int i = 0; i < VST_Number; i++)
	{
		Safe_Release(mVertexBuffers[i]);
	}
	Safe_Release(IndexBuffer);

	mAtoms.clear();
}

vBOOL IMeshDataProvider::ToMesh(IRenderContext* rc, IMeshPrimitives* mesh)
{
	auto geom = mesh->GetGeomtryMesh();
	UINT resSize = 0;
	for (int i = 0; i < VST_Number; i++)
	{
		auto vb = mVertexBuffers[i];// geom->GetVertexBuffer((EVertexSteamType)i);
		if (vb == nullptr)
			continue;

		resSize += mVertexBuffers[i]->GetSize();
		mesh->SetGeomtryMeshStream(rc, (EVertexSteamType)i,
			mVertexBuffers[i]->GetData(),
			mVertexBuffers[i]->GetSize(),
			GetStreamStride((EVertexSteamType)i), 0);
	}
	mesh->SetGeomtryMeshIndex(rc, IndexBuffer->GetData(), IndexBuffer->GetSize(), IBType, 0);
	mesh->mAtoms = mAtoms;
	if (mVertexBuffers[VST_Position] != nullptr)
	{
		auto pPos = (v3dxVector3*)mVertexBuffers[VST_Position]->GetData();
		mesh->mAABB.InitializeBox();
		for (UINT i = 0; i < VertexNumber; i++)
		{
			mesh->mAABB.OptimalVertex(pPos[i]);
		}
	}
	else
	{
		mesh->mAABB.InitializeBox();
		mesh->mAABB.OptimalVertex(v3dxVector3::ZERO);
	}
	mesh->mDesc.AtomNumber = (UINT)mAtoms.size();
	mesh->mDesc.VertexNumber = VertexNumber;
	mesh->mDesc.PolyNumber = PrimitiveNumber;
	mesh->GetGeomtryMesh()->mIsDirty = TRUE;
	mesh->GetResourceState()->SetStreamState(SS_Valid);
	mesh->GetResourceState()->SetResourceSize(resSize);
	return TRUE;
}

vBOOL IMeshDataProvider::InitFromMesh(IRenderContext* rc, IMeshPrimitives* mesh)
{
	Cleanup();
	auto geom = mesh->GetGeomtryMesh();
	auto ib = geom->GetIndexBuffer();
	if (ib == nullptr)
	{
		return FALSE;
	}

	for (int i = 0; i < VST_Number; i++)
	{
		auto vb = geom->GetVertexBuffer((EVertexSteamType)i);
		if(vb==nullptr)
			continue;

		mVertexBuffers[i] = new IBlobObject();
		vb->GetBufferData(rc, mVertexBuffers[i]);
	}

	IndexBuffer = new IBlobObject();
	ib->GetBufferData(rc, IndexBuffer);
	IBType = ib->mDesc.Type;

	switch (IBType)
	{
	case EngineNS::IBT_Int16:
		PrimitiveNumber = IndexBuffer->GetSize() / (sizeof(USHORT) * 3);
		break;
	case EngineNS::IBT_Int32:
		PrimitiveNumber = IndexBuffer->GetSize() / (sizeof(int) * 3);
		break;
	default:
		break;
	}
	VertexNumber = mVertexBuffers[VST_Position]->GetSize() / sizeof(v3dxVector3);

	mAtoms = mesh->mAtoms;
	
	return TRUE;
}

vBOOL IMeshDataProvider::Init(DWORD streams, EIndexBufferType ibType, int atom)
{
	Cleanup();
	for (int i = 0; i < VST_Number; i++)
	{
		if (streams&(1 << i))
		{
			mVertexBuffers[i] = new IBlobObject();
		}
	}

	IndexBuffer = new IBlobObject();
	IBType = ibType;

	switch (IBType)
	{
	case EngineNS::IBT_Int16:
		PrimitiveNumber = IndexBuffer->GetSize() / (3 * sizeof(USHORT));
		break;
	case EngineNS::IBT_Int32:
		PrimitiveNumber = IndexBuffer->GetSize() / (3 * sizeof(int));
		break;
	default:
		break;
	}
	if (mVertexBuffers[VST_Position] != nullptr)
		VertexNumber = mVertexBuffers[VST_Position]->GetSize() / sizeof(v3dxVector3);
	else
		VertexNumber = 0;

	mAtoms.resize(atom);

	return TRUE;
}

UINT IMeshDataProvider::GetVertexNumber() const
{
	return VertexNumber;
}

UINT IMeshDataProvider::GetPrimitiveNumber() const
{
	return PrimitiveNumber;
}

UINT IMeshDataProvider::GetAtomNumber() const
{
	return (UINT)mAtoms.size();
}

IBlobObject* IMeshDataProvider::GetStream(EVertexSteamType index)
{
	return mVertexBuffers[index];
}

IBlobObject* IMeshDataProvider::GetIndices()
{
	return IndexBuffer;
}

vBOOL IMeshDataProvider::GetAtom(UINT index, UINT lod, DrawPrimitiveDesc* desc) const
{
	if (index >= (UINT)mAtoms.size())
		return FALSE;

	if (lod >= (UINT)mAtoms[index].size())
		return FALSE;

	*desc = mAtoms[index][lod];
	return TRUE;
}

vBOOL IMeshDataProvider::SetAtom(UINT index, UINT lod, const DrawPrimitiveDesc* desc)
{
	if (index >= (UINT)mAtoms.size())
		return FALSE;

	if (lod >= (UINT)mAtoms[index].size())
		return FALSE;

	mAtoms[index][lod] = *desc;
	return TRUE;
}

void IMeshDataProvider::PushAtomLOD(UINT index, const DrawPrimitiveDesc* desc)
{
	mAtoms[index].push_back(*desc);
}

UINT IMeshDataProvider::GetAtomLOD(UINT index)
{
	return (UINT)mAtoms[index].size();
}

vBOOL IMeshDataProvider::GetAtomTriangle(UINT atom, UINT index, UINT* vA, UINT* vB, UINT* vC)
{
	auto desc = GetAtom(atom, 0);
	UINT startIndex = desc->StartIndex;
	switch (IBType)
	{
	case EngineNS::IBT_Int16:
	{
		auto pIndices = (USHORT*)IndexBuffer->GetData();
		UINT count = IndexBuffer->GetSize() / sizeof(USHORT);
		if (startIndex + (index + 1) * 3 > count)
		{
			return FALSE;
		}
		*vA = (UINT)pIndices[startIndex + index * 3 + 0];
		*vB = (UINT)pIndices[startIndex + index * 3 + 1];
		*vC = (UINT)pIndices[startIndex + index * 3 + 2];
		return TRUE;
	}
	case EngineNS::IBT_Int32:
	{
		auto pIndices = (UINT*)IndexBuffer->GetData();
		UINT count = IndexBuffer->GetSize() / sizeof(UINT);
		if (startIndex + (index + 1) * 3 > count)
		{
			return FALSE;
		}
		*vA = (UINT)pIndices[startIndex + index * 3 + 0];
		*vB = (UINT)pIndices[startIndex + index * 3 + 1];
		*vC = (UINT)pIndices[startIndex + index * 3 + 2];
		return TRUE;
	}
	default:
		return FALSE;
	}
}

vBOOL IMeshDataProvider::GetTriangle(int index, UINT* vA, UINT* vB, UINT* vC)
{
	switch (IBType)
	{
	case EngineNS::IBT_Int16:
		{
			auto pIndices = (USHORT*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / sizeof(USHORT);
			if (index * 3 + 2 > count)
			{
				return FALSE;
			}
			*vA = (UINT)pIndices[index * 3 + 0];
			*vB = (UINT)pIndices[index * 3 + 1];
			*vC = (UINT)pIndices[index * 3 + 2];
			return TRUE;
		}
	case EngineNS::IBT_Int32:
		{
			auto pIndices = (UINT*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / sizeof(UINT);
			if (index * 3 + 2 > count)
			{
				return FALSE;
			}
			*vA = (UINT)pIndices[index * 3 + 0];
			*vB = (UINT)pIndices[index * 3 + 1];
			*vC = (UINT)pIndices[index * 3 + 2];
			return TRUE;
		}
	default:
		return FALSE;
	}
}

int IMeshDataProvider::IntersectTriangle(const v3dxVector3* vStart, const v3dxVector3* vEnd, float* closedDist)
{
	auto pPos = (v3dxVector3*)mVertexBuffers[VST_Position]->GetData();
	v3dxVector3 dir = *vEnd - *vStart;
	switch (IBType)
	{
	case EngineNS::IBT_Int16:
	{
		int triangle = -1;
		*closedDist = FLT_MAX;
		auto pIndices = (USHORT*)IndexBuffer->GetData();
		int count = IndexBuffer->GetSize() / (sizeof(USHORT)*3);
		for (int i = 0; i < count; i++)
		{
			auto ia = pIndices[i * 3 + 0];
			auto ib = pIndices[i * 3 + 1];
			auto ic = pIndices[i * 3 + 2];
			float u, v, dist;
			if (v3dxIntersectTri(&pPos[ia], &pPos[ib], &pPos[ic], vStart, &dir, &u, &v, &dist))
			{
				if (dist < *closedDist)
				{
					*closedDist = dist;
					triangle = i;
				}
			}
		}
		return triangle;
	}
	case EngineNS::IBT_Int32:
	{
		int triangle = -1;
		*closedDist = FLT_MAX;
		auto pIndices = (int*)IndexBuffer->GetData();
		int count = IndexBuffer->GetSize() / (sizeof(int) * 3);
		for (int i = 0; i < count; i++)
		{
			auto ia = pIndices[i * 3 + 0];
			auto ib = pIndices[i * 3 + 1];
			auto ic = pIndices[i * 3 + 2];
			float u, v, dist;
			if (v3dxIntersectTri(&pPos[ia], &pPos[ib], &pPos[ic], vStart, &dir, &u, &v, &dist))
			{
				if (dist < *closedDist)
				{
					*closedDist = dist;
					triangle = i;
				}
			}
		}
		return triangle;
	}
	default:
		break;
	}
	return -1;
}

void* IMeshDataProvider::GetVertexPtr(EVertexSteamType stream, UINT index)
{
	auto vb = mVertexBuffers[stream];
	if (vb == nullptr)
		return nullptr;

	auto stride = GetStreamStride(stream);
	if ((index + 1) * stride >= vb->GetSize())
		return nullptr;
	auto pData = (BYTE*)vb->GetData();
	return pData + index * stride;
}

UINT IMeshDataProvider::GetStreamStride(EVertexSteamType stream)
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
	default:
		return 0;
	}
}

UINT IMeshDataProvider::AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color)
{
	auto cur = mVertexBuffers[VST_Position];
	if (cur != nullptr)
	{
		cur->PushData(pos, sizeof(v3dxVector3));
	}
	cur = mVertexBuffers[VST_Normal];
	if (cur != nullptr)
	{
		cur->PushData(nor, sizeof(v3dxVector3));
	}
	cur = mVertexBuffers[VST_Tangent];
	if (cur != nullptr)
	{
		cur->PushData(&v3dxQuaternion::ZERO, sizeof(v3dxQuaternion));
	}
	cur = mVertexBuffers[VST_Color];
	if (cur != nullptr)
	{
		cur->PushData(&color, sizeof(DWORD));
	}
	cur = mVertexBuffers[VST_UV];
	if (cur != nullptr)
	{
		cur->PushData(uv, sizeof(v3dxVector2));
	}
	cur = mVertexBuffers[VST_LightMap];
	if (cur != nullptr)
	{
		cur->PushData(&v3dxVector3::ZERO, sizeof(v3dxVector2));
	}
	cur = mVertexBuffers[VST_SkinIndex];
	if (cur != nullptr)
	{
		DWORD value = 0;
		cur->PushData(&value, sizeof(DWORD));
	}
	cur = mVertexBuffers[VST_SkinWeight];
	if (cur != nullptr)
	{
		cur->PushData(&v3dxQuaternion::ZERO, sizeof(v3dxQuaternion));
	}
	cur = mVertexBuffers[VST_TerrainIndex];
	if (cur != nullptr)
	{
		DWORD value = 0;
		cur->PushData(&value, sizeof(DWORD));
	}
	cur = mVertexBuffers[VST_TerrainGradient];
	if (cur != nullptr)
	{
		DWORD value = 0;
		cur->PushData(&value, sizeof(DWORD));
	}

	return VertexNumber++;
}

vBOOL IMeshDataProvider::AddTriangle(UINT a, UINT b, UINT c)
{
	if (VertexNumber > 0)
	{
		if (a >= VertexNumber ||
			b >= VertexNumber ||
			c >= VertexNumber)
			return FALSE;
	}

	if (IBType == IBT_Int32)
	{
		IndexBuffer->PushData(&a, sizeof(UINT));
		IndexBuffer->PushData(&b, sizeof(UINT));
		IndexBuffer->PushData(&c, sizeof(UINT));
	}
	else
	{
		IndexBuffer->PushData(&a, sizeof(USHORT));
		IndexBuffer->PushData(&b, sizeof(USHORT));
		IndexBuffer->PushData(&c, sizeof(USHORT));
	}

	PrimitiveNumber++;
	return TRUE;
}

vBOOL IMeshDataProvider::AddLine(UINT a, UINT b)
{
	if (VertexNumber > 0)
	{
		if (a >= VertexNumber ||
			b >= VertexNumber)
			return FALSE;
	}

	if (IBType == IBT_Int32)
	{
		IndexBuffer->PushData(&a, sizeof(UINT));
		IndexBuffer->PushData(&b, sizeof(UINT));
	}
	else
	{
		IndexBuffer->PushData(&a, sizeof(USHORT));
		IndexBuffer->PushData(&b, sizeof(USHORT));
	}

	PrimitiveNumber++;
	return TRUE;
}

NS_END
