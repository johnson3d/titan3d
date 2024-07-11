#include "MeshDataProvider.h"
#include "../../NextRHI/NxCommandList.h"
#include "../../Math/v3dxRayCast.h"
#include "TanGen/tgen.h"

NS_BEGIN

namespace NxRHI
{
	//=============================================================

	FMeshDataProvider::FMeshDataProvider()
	{
	}

	FMeshDataProvider::~FMeshDataProvider()
	{
		Cleanup();
	}

	void FMeshDataProvider::Cleanup()
	{
		for (int i = 0; i < VST_Number; i++)
		{
			mVertexBuffers[i] = nullptr;
		}
		IndexBuffer = nullptr;
		FaceBuffer = nullptr;

		mAtoms.clear();
	}

	bool FMeshDataProvider::BuildTangent()
	{
		for (auto& i : mAtoms)
		{
			for (auto& j : i)
			{
				if (j.PrimitiveType != EPrimitiveType::EPT_TriangleList)
					return false;
			}
		}
		std::vector<tgen::VIndexT> triIndicesPos;
		std::vector<tgen::VIndexT> triIndicesUV;
		std::vector<tgen::RealT> positions3D;
		std::vector<tgen::RealT> uvs2D;

		auto pSrcPos = (float*)mVertexBuffers[VST_Position]->GetData();
		auto pSrcUV = (float*)mVertexBuffers[VST_UV]->GetData();
		for (UINT i = 0; i < VertexNumber; i++)
		{
			positions3D.push_back(pSrcPos[3 * i + 0]);
			positions3D.push_back(pSrcPos[3 * i + 1]);
			positions3D.push_back(pSrcPos[3 * i + 2]);

			uvs2D.push_back(pSrcPos[2 * i + 0]);
			uvs2D.push_back(pSrcPos[2 * i + 1]);
		}
		if (IsIndex32)
		{
			auto pIndices = (int*)IndexBuffer->GetData();
			for (UINT i = 0; i < PrimitiveNumber; i++)
			{
				triIndicesPos.push_back(pIndices[i * 3 + 0]);
				triIndicesPos.push_back(pIndices[i * 3 + 1]);
				triIndicesPos.push_back(pIndices[i * 3 + 2]);

				triIndicesUV.push_back(pIndices[i * 3 + 0]);
				triIndicesUV.push_back(pIndices[i * 3 + 1]);
				triIndicesUV.push_back(pIndices[i * 3 + 2]);
			}
		}
		else
		{
			auto pIndices = (short*)IndexBuffer->GetData();
			for (UINT i = 0; i < PrimitiveNumber; i++)
			{
				triIndicesPos.push_back(pIndices[i * 3 + 0]);
				triIndicesPos.push_back(pIndices[i * 3 + 1]);
				triIndicesPos.push_back(pIndices[i * 3 + 2]);

				triIndicesUV.push_back(pIndices[i * 3 + 0]);
				triIndicesUV.push_back(pIndices[i * 3 + 1]);
				triIndicesUV.push_back(pIndices[i * 3 + 2]);
			}
		}

		std::vector<tgen::RealT> cTangents3D;
		std::vector<tgen::RealT> cBitangents3D;

		tgen::computeCornerTSpace(triIndicesPos, triIndicesUV, positions3D, uvs2D, cTangents3D, cBitangents3D);

		std::vector<tgen::RealT> vTangents3D;
		std::vector<tgen::RealT> vBitangents3D;
		tgen::computeVertexTSpace(triIndicesUV, cTangents3D, cBitangents3D, VertexNumber, vTangents3D, vBitangents3D);

		if (vTangents3D.size() != VertexNumber * 3)
			return false;

		auto pTanVB = CreateStream(EVertexStreamType::VST_Tangent);
		if (pTanVB->GetSize() != (vTangents3D.size() * 4 / 3 ) * sizeof(float))
		{
			pTanVB->ReSize(((UINT)vTangents3D.size() * 4 / 3) * sizeof(float));
		}
		auto pTarTan = (v3dxQuaternion*)pTanVB->GetData();
		auto pSrcNorm = (v3dxVector3*)mVertexBuffers[VST_Normal]->GetData();
		for (UINT i = 0; i < VertexNumber; i++)
		{
			pTarTan[i].X = (float)vTangents3D[i * 3 + 0];
			pTarTan[i].Y = (float)vTangents3D[i * 3 + 1];
			pTarTan[i].Z = (float)vTangents3D[i * 3 + 2];
			v3dxVector3 vbt;
			vbt.X = (float)vBitangents3D[i * 3 + 0];
			vbt.Y = (float)vBitangents3D[i * 3 + 1];
			vbt.Z = (float)vBitangents3D[i * 3 + 2];
			v3dxVector3 tmp;
			v3dxVec3Cross(&tmp, &vbt, (v3dxVector3*)&pTarTan[i]);
			v3dxVec3Normalize(&tmp, &tmp);
			if (v3dxVec3Dot(&tmp, &pSrcNorm[i]) > 0.0f)
				pTarTan[i].W = 0;//Need set by vTangents3D & vBitangents3D
			else
				pTarTan[i].W = 1;
		}
		return true;
	}

	FInputLayoutDesc* FMeshDataProvider::GetInputLayoutDesc()
	{
		return nullptr;
	}

	bool FMeshDataProvider::ToMesh(ICommandList* cmd, FMeshPrimitives* mesh)
	{
		mesh->Reset(false);
		UINT resSize = 0;
		for (int i = 0; i < VST_Number; i++)
		{
			auto vb = mVertexBuffers[i];// geom->GetVertexBuffer((EVertexSteamType)i);
			if (vb == nullptr)
				continue;

			if (mVertexBuffers[i]->GetSize() == 0 || mVertexBuffers[i]->GetData() == nullptr)
				continue;

			resSize += mVertexBuffers[i]->GetSize();
			mesh->SetGeomtryMeshStream(cmd, (EVertexStreamType)i,
				mVertexBuffers[i]->GetData(),
				mVertexBuffers[i]->GetSize(),
				FMeshPrimitives::GetStreamTypeInfo((EVertexStreamType)i).Stride, ECpuAccess::CAS_DEFAULT);
		}
		if (IndexBuffer->GetSize() > 0)
			mesh->SetGeomtryMeshIndex(cmd, IndexBuffer->GetData(), IndexBuffer->GetSize(), IsIndex32, ECpuAccess::CAS_DEFAULT);
		//mesh->mGeometryMesh->BindInputLayout();

		mesh->mGeometryMesh->Atoms = mAtoms;
		if (mVertexBuffers[VST_Position] != nullptr)
		{
			auto pPos = (v3dxVector3*)mVertexBuffers[VST_Position]->GetData();
			mesh->mAABB.InitializeBox();
			for (UINT i = 0; i < VertexNumber; i++)
			{
				mesh->mAABB.MergeVertex(pPos[i]);
			}
		}
		else
		{
			mesh->mAABB.InitializeBox();
			mesh->mAABB.MergeVertex(v3dxVector3::ZERO);
		}
		mesh->mDesc.AtomNumber = (UINT)mAtoms.size();
		mesh->mDesc.VertexNumber = VertexNumber;
		mesh->mDesc.PolyNumber = PrimitiveNumber;
		if (mAtomExtDatas.size() > 0)
		{
			mesh->ValidAtomExtData();
			for (UINT i = 0; i < mAtomExtDatas.size(); i++)
			{
				mesh->SetAtomExtData(i, GetAtomExtData(i));
			}
		}

		mesh->GetResourceState()->SetStreamState(SS_Valid);
		mesh->GetResourceState()->SetResourceSize(resSize);
		return TRUE;
	}

	bool FMeshDataProvider::InitFromMesh(IGpuDevice* device, FMeshPrimitives* mesh)
	{
		Cleanup();

		mAABB = mesh->mAABB;
		StreamTypes = 0;
		auto geom = mesh->GetGeomtryMesh();
		auto ib = geom->GetIndexBuffer();
		if (ib == nullptr)
		{
			return FALSE;
		}

		for (int i = 0; i < VST_Number; i++)
		{
			auto vb = geom->GetVertexBuffer((EVertexStreamType)i);
			if (vb == nullptr)
				continue;

			StreamTypes |= (1 << i);

			mVertexBuffers[i] = MakeWeakRef(new IBlobObject());

			AutoRef<NxRHI::IBuffer> copyVB;
			{
				FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadVB");
				auto copyDesc = vb->Buffer->Desc;
				copyDesc.Usage = USAGE_STAGING;
				copyDesc.CpuAccess = ECpuAccess::CAS_READ;
				copyDesc.MiscFlags = (EResourceMiscFlag)0;
				copyDesc.RowPitch = copyDesc.Size;
				copyDesc.DepthPitch = copyDesc.Size;
				copyVB = MakeWeakRef(device->CreateBuffer(&copyDesc));
				cmd.GetCmdList()->CopyBufferRegion(copyVB, 0, vb->Buffer, 0, copyDesc.Size);
			}
			device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);

			IBlobObject buffData;
			copyVB->FetchGpuData(0, &buffData);

			mVertexBuffers[i]->PushData((BYTE*)buffData.GetData() + sizeof(UINT) * 2, vb->Buffer->Desc.Size);
		}

		IndexBuffer = MakeWeakRef(new IBlobObject());

		{
			AutoRef<NxRHI::IBuffer> copyIB;
			{
				FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadVB");
				auto copyDesc = ib->Buffer->Desc;
				copyDesc.Usage = USAGE_STAGING;
				copyDesc.CpuAccess = ECpuAccess::CAS_READ;
				copyDesc.MiscFlags = (EResourceMiscFlag)0;
				copyDesc.RowPitch = copyDesc.Size;
				copyDesc.DepthPitch = copyDesc.Size;
				copyIB = MakeWeakRef(device->CreateBuffer(&copyDesc));
				cmd.GetCmdList()->CopyBufferRegion(copyIB, 0, ib->Buffer, 0, copyDesc.Size);
			}
			device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);

			IBlobObject buffData;
			copyIB->FetchGpuData(0, &buffData);
			IndexBuffer->PushData((BYTE*)buffData.GetData() + sizeof(UINT) * 2, ib->Buffer->Desc.Size);
		}

		IsIndex32 = mesh->mGeometryMesh->IsIndex32;

		PrimitiveNumber = mesh->mDesc.PolyNumber;
		if (IsIndex32)
		{
			PrimitiveNumber = IndexBuffer->GetSize() / (sizeof(DWORD) * 3);
		}
		else
		{
			PrimitiveNumber = IndexBuffer->GetSize() / (sizeof(USHORT) * 3);
		}

		VertexNumber = mVertexBuffers[VST_Position]->GetSize() / sizeof(v3dxVector3);

		mAtoms = mesh->mGeometryMesh->Atoms;

		return true;
	}

	void FMeshDataProvider::Reset()
	{
		PrimitiveNumber = 0;
		VertexNumber = 0;

		AtomSize = 0;
		mAtoms.resize(0);
		mAtomExtDatas.resize(0);
		for (int i = 0; i < VST_Number; i++)
		{
			if (StreamTypes & (1 << i))
			{
				if (mVertexBuffers[i] == nullptr)
				{
					mVertexBuffers[i] = MakeWeakRef(new IBlobObject());
				}
				else
				{
					mVertexBuffers[i]->ReSize(0);
				}
			}
		}

		if (IndexBuffer == nullptr)
			IndexBuffer = MakeWeakRef(new IBlobObject());
		else
			IndexBuffer->ReSize(0);
	}
	bool FMeshDataProvider::Init(DWORD streams, bool isIndex32, int atom)
	{
		StreamTypes = streams;
		IsIndex32 = isIndex32;
		AtomSize = atom;

		return Init();
	}
	bool FMeshDataProvider::Init()
	{
		Cleanup();

		mAtoms.resize(AtomSize);
		for (int i = 0; i < VST_Number; i++)
		{
			if (StreamTypes & (1 << i))
			{
				mVertexBuffers[i] = MakeWeakRef(new IBlobObject());
			}
		}

		IndexBuffer = MakeWeakRef(new IBlobObject());


		PrimitiveNumber = 0;
		VertexNumber = 0;
		return true;
	}

	void FMeshDataProvider::LoadVB(XndAttribute* pAttr, UINT stride, EVertexStreamType stream)
	{
		pAttr->BeginRead();
		UINT uVert, uKey, uStride;
		pAttr->Read(uStride);
		if (stride != uStride)
		{
			ASSERT(false);
		}
		pAttr->Read(uVert);
		pAttr->Read(uKey);
		TimeKeys tkeys;
		tkeys.Load(*pAttr);
		//int ByteWidth = uStride * uKey * uVert;
		int ByteWidth = uStride * 1 * uVert;
		BYTE* data = new BYTE[ByteWidth];
		pAttr->Read(data, ByteWidth);
		pAttr->EndRead();

		mVertexBuffers[stream] = MakeWeakRef(new IBlobObject());
		mVertexBuffers[stream]->PushData(data, ByteWidth);
		delete[] data;
	}

	bool FMeshDataProvider::LoadFromMeshPrimitive(XndNode* pNode, EVertexStreamType streams)
	{
		FMeshPrimitives::FModelDesc mDesc;
		XndAttribute* pAttr = pNode->TryGetAttribute("HeadAttrib");
		if (pAttr)
		{
			pAttr->BeginRead();
			pAttr->Read(mDesc);
			pAttr->Read(mAABB);
			pAttr->EndRead();
			VertexNumber = mDesc.VertexNumber;
			PrimitiveNumber = mDesc.PolyNumber;
		}
		else
		{
			return false;
		}
		pAttr = pNode->TryGetAttribute("RenderAtom");
		if (pAttr)
		{
			mAtoms.clear();
			mAtoms.resize(mDesc.AtomNumber);
			pAttr->BeginRead();
			for (size_t i = 0; i < mDesc.AtomNumber; i++)
			{
				FMeshAtomDesc dpDesc;
				pAttr->Read(dpDesc.PrimitiveType);
				if (dpDesc.PrimitiveType != EPT_TriangleList)
					return false;
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

		IndexBuffer = MakeWeakRef(new IBlobObject());
		pAttr = pNode->TryGetAttribute("Indices");
		if (pAttr)
		{
			pAttr->BeginRead();

			FBufferDesc desc;
			desc.SetDefault();

			UINT count;
			pAttr->Read(count);
			vBOOL bFormatIndex32;
			pAttr->Read(bFormatIndex32);
			IsIndex32 = bFormatIndex32 ? true : false;
			desc.Size = count * ((bFormatIndex32) ? sizeof(DWORD) : sizeof(WORD));

			BYTE* data = new BYTE[desc.Size];
			pAttr->Read(data, desc.Size);

			pAttr->EndRead();

			IndexBuffer->PushData(data, desc.Size);
			Safe_DeleteArray(data);
		}
		else
		{
			return false;
		}

		for (int i = EVertexStreamType::VST_Position; i < EVertexStreamType::VST_Number; i++)
		{
			auto info = FMeshPrimitives::GetStreamTypeInfo((EVertexStreamType)i);
			if (info.XndName == nullptr)
				continue;
			if (streams & (1 << i))
			{
				pAttr = pNode->TryGetAttribute(info.XndName);
				if (pAttr)
				{
					LoadVB(pAttr, info.Stride, (EVertexStreamType)i);
				}
			}
		}

		return true;
	}

	UINT FMeshDataProvider::GetVertexNumber() const
	{
		return VertexNumber;
	}

	UINT FMeshDataProvider::GetPrimitiveNumber() const
	{
		return PrimitiveNumber;
	}

	UINT FMeshDataProvider::GetAtomNumber() const
	{
		return (UINT)mAtoms.size();
	}

	IBlobObject* FMeshDataProvider::CreateStream(EVertexStreamType index)
	{
		if (mVertexBuffers[index] == nullptr)
		{
			mVertexBuffers[index] = MakeWeakRef(new IBlobObject());
			StreamTypes |= (1 << index);
			UINT stride = 0;
			GetVertexStreamInfo(index, &stride);
			mVertexBuffers[index]->ReSize(VertexNumber * stride);
		}
		return mVertexBuffers[index];
	}
	IBlobObject* FMeshDataProvider::GetStream(EVertexStreamType index)
	{
		return mVertexBuffers[index];
	}

	IBlobObject* FMeshDataProvider::GetIndices()
	{
		return IndexBuffer;
	}

	FMeshAtomDesc* FMeshDataProvider::GetAtom(UINT index, UINT lod)
	{
		if (index >= (UINT)mAtoms.size())
			return nullptr;

		if (lod >= (UINT)mAtoms[index].size())
			return nullptr;

		return &mAtoms[index][lod];
	}
	void FMeshDataProvider::PushAtom(const FMeshAtomDesc* pDescLODs, UINT count, VIUnknown* ext)
	{
		std::vector<FMeshAtomDesc> tmp;
		for (UINT i = 0; i < count; i++)
		{
			tmp.push_back(pDescLODs[i]);
		}
		mAtoms.push_back(tmp);
		mAtomExtDatas.push_back(ext);
	}
	bool FMeshDataProvider::SetAtom(UINT index, UINT lod, const FMeshAtomDesc& desc)
	{
		if (index >= (UINT)mAtoms.size())
			return false;

		if (lod >= (UINT)mAtoms[index].size())
			return false;

		mAtoms[index][lod] = desc;
		return true;
	}

	void FMeshDataProvider::PushAtomLOD(UINT index, const FMeshAtomDesc& desc)
	{
		mAtoms[index].push_back(desc);
	}

	UINT FMeshDataProvider::GetAtomLOD(UINT index)
	{
		return (UINT)mAtoms[index].size();
	}

	bool FMeshDataProvider::GetAtomTriangle(UINT atom, UINT index, UINT* vA, UINT* vB, UINT* vC)
	{
		auto desc = GetAtom(atom, 0);
		UINT startIndex = desc->StartIndex;
		if (IsIndex32)
		{
			auto pIndices = (UINT*)IndexBuffer->GetData();
			UINT count = IndexBuffer->GetSize() / sizeof(UINT);
			if (startIndex + (index + 1) * 3 > count)
			{
				return false;
			}
			*vA = (UINT)pIndices[startIndex + index * 3 + 0];
			*vB = (UINT)pIndices[startIndex + index * 3 + 1];
			*vC = (UINT)pIndices[startIndex + index * 3 + 2];
			return true;
		}
		else
		{
			auto pIndices = (USHORT*)IndexBuffer->GetData();
			UINT count = IndexBuffer->GetSize() / sizeof(USHORT);
			if (startIndex + (index + 1) * 3 > count)
			{
				return false;
			}
			*vA = (UINT)pIndices[startIndex + index * 3 + 0];
			*vB = (UINT)pIndices[startIndex + index * 3 + 1];
			*vC = (UINT)pIndices[startIndex + index * 3 + 2];
			return true;
		}
	}

	bool FMeshDataProvider::GetTriangle(int index, UINT* vA, UINT* vB, UINT* vC)
	{
		if (IsIndex32)
		{
			auto pIndices = (UINT*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / sizeof(UINT);
			if (index * 3 + 2 > count)
			{
				return false;
			}
			*vA = (UINT)pIndices[index * 3 + 0];
			*vB = (UINT)pIndices[index * 3 + 1];
			*vC = (UINT)pIndices[index * 3 + 2];
			return true;
		}
		else
		{
			auto pIndices = (USHORT*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / sizeof(USHORT);
			if (index * 3 + 2 > count)
			{
				return false;
			}
			*vA = (UINT)pIndices[index * 3 + 0];
			*vB = (UINT)pIndices[index * 3 + 1];
			*vC = (UINT)pIndices[index * 3 + 2];
			return true;
		}
	}

	int FMeshDataProvider::IntersectTriangle(const v3dxVector3* scale, const v3dxVector3* vStart, const v3dxVector3* vEnd, VHitResult* result)
	{
		auto start = *vStart;
		auto end = *vEnd;
		v3dxVector3 rcqScale;
		if (scale != nullptr)
		{
			rcqScale = v3dxVector3(1.0f / scale->X, 1.0f / scale->Y, 1.0f / scale->Z);
			start *= rcqScale;
			end *= rcqScale;
		}
		auto pPos = (v3dxVector3*)mVertexBuffers[VST_Position]->GetData();
		v3dxVector3 dir = end - start;
		if (IsIndex32)
		{
			int triangle = -1;
			float closedDist = FLT_MAX;
			auto pIndices = (int*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / (sizeof(int) * 3);
			for (int i = 0; i < count; i++)
			{
				auto ia = pIndices[i * 3 + 0];
				auto ib = pIndices[i * 3 + 1];
				auto ic = pIndices[i * 3 + 2];

				auto vA = pPos[ia];
				auto vB = pPos[ib];
				auto vC = pPos[ic];
				/*if (scale != nullptr)
				{
					vA *= (*scale);
					vB *= (*scale);
					vC *= (*scale);
				}*/

				float u, v, dist;
				if (v3dxIntersectTri(&vA, &vB, &vC, &start, &dir, &u, &v, &dist))
				{
					if (dist < closedDist)
					{
						closedDist = dist;
						triangle = i;
						result->Distance = dist;
						result->FaceId = i;
						result->Position = start + dir * dist;
						if (scale != nullptr)
						{
							result->Position *= (*scale);
							auto singleVector = result->Position.ToSingleVector();
							result->Distance = sqrt(v3dxCalSquareDistance(&singleVector, vStart));
						}
						v3dxCalcNormal(&result->Normal, &vA, &vB, &vC, TRUE);
						result->U = u;
						result->V = v;
					}
				}
			}
			return triangle;
		}
		else
		{
			int triangle = -1;
			float closedDist = FLT_MAX;
			auto pIndices = (USHORT*)IndexBuffer->GetData();
			int count = IndexBuffer->GetSize() / (sizeof(USHORT) * 3);
			for (int i = 0; i < count; i++)
			{
				auto ia = pIndices[i * 3 + 0];
				auto ib = pIndices[i * 3 + 1];
				auto ic = pIndices[i * 3 + 2];

				auto vA = pPos[ia];
				auto vB = pPos[ib];
				auto vC = pPos[ic];
				/*if (scale != nullptr)
				{
					vA *= (*scale);
					vB *= (*scale);
					vC *= (*scale);
				}*/

				float u, v, dist;
				if (v3dxIntersectTri(&vA, &vB, &vC, &start, &dir, &u, &v, &dist))
				{
					if (dist < closedDist)
					{
						closedDist = dist;
						triangle = i;
						result->Distance = dist;
						result->FaceId = i;
						result->Position = start + dir * dist;
						if (scale != nullptr)
						{
							result->Position *= (*scale);
							auto singleVector = result->Position.ToSingleVector();
							result->Distance = sqrt(v3dxCalSquareDistance(&singleVector, vStart));
						}
						v3dxCalcNormal(&result->Normal, &vA, &vB, &vC, TRUE);
						result->U = u;
						result->V = v;
					}
				}
			}
			return triangle;
		}

		return -1;
	}

	void* FMeshDataProvider::GetVertexPtr(EVertexStreamType stream, UINT index)
	{
		auto vb = mVertexBuffers[stream];
		if (vb == nullptr)
			return nullptr;

		auto stride = FMeshPrimitives::GetStreamTypeInfo(stream).Stride;
		if ((index + 1) * stride >= vb->GetSize())
			return nullptr;
		auto pData = (BYTE*)vb->GetData();
		return pData + index * stride;
	}

	void FMeshDataProvider::ResizeVertexBuffers(UINT size)
	{
		for (int i = 0; i < VST_Number; i++)
		{
			if (mVertexBuffers[i] != nullptr)
			{
				UINT stride = 0;
				NxRHI::GetVertexStreamInfo((EVertexStreamType)i, &stride);
				mVertexBuffers[i]->ReSize(size * stride);
			}
		}
	}

	UINT FMeshDataProvider::AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, DWORD color)
	{
		auto cur = mVertexBuffers[VST_Position];
		if (cur != nullptr && pos != nullptr)
		{
			cur->PushData(pos, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Normal];
		if (cur != nullptr && nor != nullptr)
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
		if (cur != nullptr && uv != nullptr)
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

	UINT FMeshDataProvider::AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector3* tangent, const v3dxVector2* uv, DWORD color)
	{
		auto cur = mVertexBuffers[VST_Position];
		if (cur != nullptr && pos != nullptr)
		{
			cur->PushData(pos, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Normal];
		if (cur != nullptr && nor != nullptr)
		{
			cur->PushData(nor, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Tangent];
		if (cur != nullptr)
		{
			cur->PushData(tangent, sizeof(v3dxQuaternion));
		}
		cur = mVertexBuffers[VST_Color];
		if (cur != nullptr)
		{
			cur->PushData(&color, sizeof(DWORD));
		}
		cur = mVertexBuffers[VST_UV];
		if (cur != nullptr && uv != nullptr)
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

	UINT FMeshDataProvider::AddVertex(const v3dxVector3* pos, const v3dxVector3* nor, const v3dxVector2* uv, const v3dxQuaternion* lighmapUV, DWORD color)
	{
		auto cur = mVertexBuffers[VST_Position];
		if (cur != nullptr && pos != nullptr)
		{
			cur->PushData(pos, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Normal];
		if (cur != nullptr && nor != nullptr)
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
		if (cur != nullptr && uv != nullptr)
		{
			cur->PushData(uv, sizeof(v3dxVector2));
		}
		cur = mVertexBuffers[VST_LightMap];
		if (cur != nullptr)
		{
			cur->PushData(lighmapUV, sizeof(v3dxQuaternion));
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

	void FMeshDataProvider::AddVertex(const FMeshVertex* pVertex, UINT num)
	{
		//todo optimize
		for (UINT i = 0; i < num; i++)
		{
			AddVertex(pVertex[i]);
		}
	}
	bool FMeshDataProvider::AddVertex_Pos_UV_Color_Index(const void* pVertex, UINT num, bool bInvertY, float CanvasHeight)
	{
		struct FTmpVertex
		{
			v3dxVector3 Pos;
			v3dxVector2 UV;
			DWORD Color;
			DWORD Index;
		};
		auto verts = (FTmpVertex*)pVertex;

		size_t posOffset = 0;
		size_t uvOffset = 0;
		size_t colorOffset = 0;
		size_t indexOffset = 0;

		this->VertexNumber += num;

		auto posVB = mVertexBuffers[VST_Position];
		if (posVB != nullptr)
		{
			posOffset = posVB->GetSize();
			posVB->PushData(nullptr, sizeof(v3dxVector3) * num);
		}
		else
		{
			return false;
		}
		auto uvVB = mVertexBuffers[VST_UV];
		if (uvVB != nullptr)
		{
			uvOffset = uvVB->GetSize();
			uvVB->PushData(nullptr, sizeof(v3dxVector2) * num);
		}
		else
		{
			return false;
		}
		auto colorVB = mVertexBuffers[VST_Color];
		if (colorVB != nullptr)
		{
			colorOffset = colorVB->GetSize();
			colorVB->PushData(nullptr, sizeof(DWORD) * num);
		}
		else
		{
			return false;
		}
		auto indexVB = mVertexBuffers[VST_SkinIndex];
		if (indexVB != nullptr)
		{
			indexOffset = indexVB->GetSize();
			indexVB->PushData(nullptr, sizeof(DWORD) * num);
		}
		else
		{
			return false;
		}
		if (bInvertY)
		{
			for (UINT i = 0; i < num; i++)
			{
				auto pos = verts[i].Pos;
				pos.Y = CanvasHeight - pos.Y;
				posVB->SetValueToOffset((UINT)posOffset, pos);
				posOffset += sizeof(v3dxVector3);
				uvVB->SetValueToOffset((UINT)uvOffset, verts[i].UV);
				uvOffset += sizeof(v3dxVector2);
				colorVB->SetValueToOffset((UINT)colorOffset, verts[i].Color);
				colorOffset += sizeof(DWORD);
				indexVB->SetValueToOffset((UINT)indexOffset, verts[i].Index);
				indexOffset += sizeof(DWORD);
			}
		}
		else
		{
			for (UINT i = 0; i < num; i++)
			{
				posVB->SetValueToOffset((UINT)posOffset, verts[i].Pos);
				posOffset += sizeof(v3dxVector3);
				uvVB->SetValueToOffset((UINT)uvOffset, verts[i].UV);
				uvOffset += sizeof(v3dxVector2);
				colorVB->SetValueToOffset((UINT)colorOffset, verts[i].Color);
				colorOffset += sizeof(DWORD);
				indexVB->SetValueToOffset((UINT)indexOffset, verts[i].Index);
				indexOffset += sizeof(DWORD);
			}
		}

		return true;
	}
	UINT FMeshDataProvider::AddVertex(const FMeshVertex& vertex)
	{
		auto cur = mVertexBuffers[VST_Position];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.Position, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Normal];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.Normal, sizeof(v3dxVector3));
		}
		cur = mVertexBuffers[VST_Tangent];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.Tangent, sizeof(v3dxQuaternion));
		}
		cur = mVertexBuffers[VST_Color];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.Color, sizeof(DWORD));
		}
		cur = mVertexBuffers[VST_UV];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.UV, sizeof(v3dxVector2));
		}
		cur = mVertexBuffers[VST_LightMap];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.LightMap, sizeof(v3dxQuaternion));
		}
		cur = mVertexBuffers[VST_SkinIndex];
		if (cur != nullptr)
		{
			DWORD value = 0;
			cur->PushData(&vertex.SkinIndex, sizeof(DWORD));
		}
		cur = mVertexBuffers[VST_SkinWeight];
		if (cur != nullptr)
		{
			cur->PushData(&vertex.SkinWeight, sizeof(v3dxQuaternion));
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

	vBOOL FMeshDataProvider::AddTriangle(UINT a, UINT b, UINT c)
	{
		if (VertexNumber > 0)
		{
			if (a >= VertexNumber ||
				b >= VertexNumber ||
				c >= VertexNumber)
				return FALSE;
		}

		if (IsIndex32)
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

		if (FaceBuffer != nullptr)
		{
			USHORT faceData = 0;
			FaceBuffer->PushData(&faceData, sizeof(USHORT));
		}

		PrimitiveNumber++;
		return TRUE;
	}

	vBOOL FMeshDataProvider::AddTriangle(UINT a, UINT b, UINT c, USHORT faceData)
	{
		FaceBuffer->PushData(&faceData, sizeof(USHORT));
		if (FALSE == AddTriangle(a, b, c))
			return FALSE;
		return TRUE;
	}

	vBOOL FMeshDataProvider::AddTriangle(UINT* pTri, UINT numOfTri)
	{
		if (IsIndex32)
		{
			IndexBuffer->PushData(pTri, sizeof(UINT) * numOfTri * 3);
		}
		else
		{
			for (UINT i = 0; i < numOfTri; i++)
			{
				USHORT a, b, c;
				a = (USHORT)pTri[i * 3 + 0];
				b = (USHORT)pTri[i * 3 + 1];
				c = (USHORT)pTri[i * 3 + 2];
				IndexBuffer->PushData(&a, sizeof(USHORT));
				IndexBuffer->PushData(&b, sizeof(USHORT));
				IndexBuffer->PushData(&c, sizeof(USHORT));
			}
		}

		if (FaceBuffer)
		{
			FaceBuffer->PushData(nullptr, sizeof(USHORT) * numOfTri);
		}

		PrimitiveNumber += numOfTri;
		return true;
	}

	vBOOL FMeshDataProvider::AddLine(UINT a, UINT b)
	{
		if (VertexNumber > 0)
		{
			if (a >= VertexNumber ||
				b >= VertexNumber)
				return FALSE;
		}

		if (IsIndex32)
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
}

NS_END