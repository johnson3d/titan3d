#include "SimGeom.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"
#include "../../3rd/ConvexDecomposition/NvConvexDecomposition.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::SimGeom, EngineNS::VIUnknown);

void TriMesh::Convex::BuildConvex(const TriMesh* mesh, v3dxConvex* convex) const
{
	convex->Planes.resize(NumTriangle);
	for (UINT i = 0; i < NumTriangle; i++)
	{
		auto ia = mesh->mIndices[StartIndex + i * 3 + 0];
		auto ib = mesh->mIndices[StartIndex + i * 3 + 1];
		auto ic = mesh->mIndices[StartIndex + i * 3 + 2];

		auto a = mesh->mPositions[ia];
		auto b = mesh->mPositions[ib];
		auto c = mesh->mPositions[ic];

		convex->Planes[i].set(a, b, c);
	}
}

vBOOL TriMesh::BuildConvex(v3dxConvex** ppConvexes, UINT count) const
{
	if (count != GetConvexNum())
		return FALSE;
	for (UINT i = 0; i < count; i++)
	{
		mHulls[i].BuildConvex(this, ppConvexes[i]);
	}
	return TRUE;
}

SimGeom::SimGeom()
{
}


SimGeom::~SimGeom()
{
}

class EngineConvexDecompInterface
{
public:
	SimGeom*	mGeom;

	struct VertexFaces
	{
		std::vector<UINT>		Faces;
	};

	void ConvexDecompResult(CONVEX_DECOMPOSITION::ConvexHullResult &result)
	{
		std::vector<v3dxVector3> norms;
		norms.resize(result.mVcount);
		std::vector<v3dxVector2> uvs;
		uvs.resize(result.mVcount);

		std::vector<VertexFaces> VtxFaces;
		VtxFaces.resize(result.mVcount);
		
		for (UINT i = 0; i < result.mVcount; i++)
		{
			for (UINT j = 0; j < result.mTcount; j++)
			{
				if (result.mIndices[3 * j + 0] == i ||
					result.mIndices[3 * j + 1] == i ||
					result.mIndices[3 * j + 2] == i)
				{
					VtxFaces[i].Faces.push_back(j);
				}
			}
		}

		for (UINT i = 0; i < result.mVcount; i++)
		{
			norms[i].setValue(0, 0, 0);
			for (auto j : VtxFaces[i].Faces)
			{
				auto a = result.mIndices[3 * j + 0];
				auto b = result.mIndices[3 * j + 1];
				auto c = result.mIndices[3 * j + 2];

				v3dxVector3 vA, vB, vC;
				vA.x = (float)result.mVertices[3 * a + 0];
				vA.y = (float)result.mVertices[3 * a + 1];
				vA.z = (float)result.mVertices[3 * a + 2];

				vB.x = (float)result.mVertices[3 * b + 0];
				vB.y = (float)result.mVertices[3 * b + 1];
				vB.z = (float)result.mVertices[3 * b + 2];

				vC.x = (float)result.mVertices[3 * c + 0];
				vC.y = (float)result.mVertices[3 * c + 1];
				vC.z = (float)result.mVertices[3 * c + 2];

				v3dxVector3 nor;
				v3dxCalcNormal(&nor, &vA, &vB, &vC, TRUE);
				norms[i] += nor;
			}
			norms[i] /= (float)VtxFaces[i].Faces.size();
			norms[i].normalize();

			uvs[i].setValue(norms[i].x, norms[i].y);
		}

		TriMesh::Convex hall;
		auto start = mGeom->mTriMesh.GetVertexNumber();
		for (UINT i = 0; i < result.mVcount; i++)
		{
			v3dxVector3 pos;
			pos.x = (float)result.mVertices[i * 3 + 0];
			pos.y = (float)result.mVertices[i * 3 + 1];
			pos.z = (float)result.mVertices[i * 3 + 2];
			mGeom->mTriMesh.AddVertex(&pos, &norms[i], &uvs[i], 0);
		}

		hall.StartIndex = (UINT)mGeom->mTriMesh.mIndices.size();
		hall.NumTriangle = result.mTcount;
		//hall.Volume = (float)result.;
		for (UINT i = 0; i < result.mTcount; i++)
		{
			auto a = start + result.mIndices[3 * i + 0];
			auto b = start + result.mIndices[3 * i + 1];
			auto c = start + result.mIndices[3 * i + 2];
			mGeom->mTriMesh.AddTriangle(a, b, c);
		}

		mGeom->mTriMesh.mHulls.push_back(hall);
	}
};

vBOOL SimGeom::BuildTriMesh(IRenderContext* rc, GfxMeshPrimitives* mesh, ConvexDecompDesc* desc)
{
	auto vb = mesh->GetGeomtryMesh()->GetVertexBuffer(VST_Position);
	IBlobObject posBlob;
	vb->GetBufferData(rc, &posBlob);

	CONVEX_DECOMPOSITION::iConvexDecomposition *ic = CONVEX_DECOMPOSITION::createConvexDecomposition();
	
	UINT mVcount = posBlob.GetSize() / sizeof(v3dxVector3);
	auto pv = (float*)posBlob.GetData();
	
	auto ib = mesh->GetGeomtryMesh()->GetIndexBuffer();
	IBlobObject idxblob;
	ib->GetBufferData(rc, &idxblob);
	if (ib->mDesc.Type == IBT_Int16)
	{
		UINT mTcount = ib->mDesc.ByteWidth / sizeof(USHORT);
		mTcount = mTcount / 3;
		auto p = (USHORT*)idxblob.GetData();
		for (UINT i = 0; i < mTcount; i++)
		{
			NxU32 i1 = p[i * 3 + 0];
			NxU32 i2 = p[i * 3 + 1];
			NxU32 i3 = p[i * 3 + 2];

			const NxF32 *p1 = &pv[i1 * 3];
			const NxF32 *p2 = &pv[i2 * 3];
			const NxF32 *p3 = &pv[i3 * 3];

			ic->addTriangle(p1, p2, p3);
		}
	}
	else
	{
		UINT mTcount = ib->mDesc.ByteWidth / sizeof(int);
		mTcount = mTcount / 3;
		auto p = (UINT*)idxblob.GetData();
		for (UINT i = 0; i < mTcount; i++)
		{
			NxU32 i1 = p[i * 3 + 0];
			NxU32 i2 = p[i * 3 + 1];
			NxU32 i3 = p[i * 3 + 2];

			const NxF32 *p1 = &pv[i1 * 3];
			const NxF32 *p2 = &pv[i2 * 3];
			const NxF32 *p3 = &pv[i3 * 3];

			ic->addTriangle(p1, p2, p3);
		}
	}
	if (desc == NULL)
	{
		this->mTriMesh.mIsConvex = FALSE;
	}
	else
	{
		this->mTriMesh.mIsConvex = TRUE;
		ic->computeConvexDecomposition((float)desc->mSkinWidth,
			desc->mDepth,
			desc->mMaxVertices,
			(float)desc->mCpercent,
			(float)desc->mPpercent,
			(float)desc->mVolumeSplitPercent,
			true,
			false);

		NxU32 hullCount = ic->getHullCount();
		NxU32 vcount_base = 1;
		NxU32 vcount_total = 0;
		NxU32 tcount_total = 0;
		for (NxU32 i = 0; i < hullCount; i++)
		{
			CONVEX_DECOMPOSITION::ConvexHullResult result;
			ic->getConvexHullResult(i, result);

			EngineConvexDecompInterface cvd;
			cvd.mGeom = this;
			cvd.ConvexDecompResult(result);
		}

		this->mTriMesh.UpdateBox();
	}
	CONVEX_DECOMPOSITION::releaseConvexDecomposition(ic);

	return TRUE;
}

GfxMeshPrimitives* SimGeom::CreateMesh(IRenderContext* rc)
{
	auto result = new GfxMeshPrimitives();
	
	result->Init(rc, "SimGeom", 1);
	result->SetGeomtryMeshStream(rc, VST_Position, &mTriMesh.mPositions[0],
		(UINT)mTriMesh.mPositions.size() * sizeof(v3dxVector3), sizeof(v3dxVector3), 0);

	result->SetGeomtryMeshStream(rc, VST_Normal, &mTriMesh.mNormals[0],
		(UINT)mTriMesh.mNormals.size() * sizeof(v3dxVector3), sizeof(v3dxVector3), 0);

	result->SetGeomtryMeshStream(rc, VST_UV, &mTriMesh.mUVs[0],
		(UINT)mTriMesh.mUVs.size() * sizeof(v3dxVector2), sizeof(v3dxVector2), 0);

	result->SetGeomtryMeshIndex(rc, &mTriMesh.mIndices[0], (UINT)mTriMesh.mIndices.size() * sizeof(UINT), IBT_Int32, 0);

	DrawPrimitiveDesc desc;
	desc.NumPrimitives = (UINT)mTriMesh.mIndices.size() /  3;
	result->PushAtomLOD(0, &desc);

	result->GetResourceState()->SetResourceSize((UINT)mTriMesh.mPositions.size() * sizeof(v3dxVector3) * 2);
	result->GetResourceState()->SetStreamState(SS_Valid);
	result->GetResourceState()->SetKeepValid(TRUE);
	result->GetGeomtryMesh()->SetIsDirty(TRUE);

	return result;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI3(vBOOL, EngineNS, SimGeom, BuildTriMesh, IRenderContext*, GfxMeshPrimitives*, SimGeom::ConvexDecompDesc*);
	CSharpReturnAPI1(GfxMeshPrimitives*, EngineNS, SimGeom, CreateMesh, IRenderContext*);
	CSharpReturnAPI0(UINT, EngineNS, SimGeom, GetConvexNum);
	CSharpReturnAPI2(vBOOL, EngineNS, SimGeom, BuildConvex, v3dxConvex**, UINT);
}