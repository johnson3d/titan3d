#include "NxGeomMesh.h"
#include "NxBuffer.h"
#include "NxCommandList.h"
#include "NxInputAssembly.h"
#include "NxDrawcall.h"
#include "../../Math/v3dxRayCast.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	struct FStreamTypeInfo
	{
		const char* XndName = nullptr;
		int Stride = 0;
	};
	FStreamTypeInfo GetStreamTypeInfo(EVertexStreamType type)
	{
		FStreamTypeInfo result;
		switch (type)
		{
			case VST_Position:
			{
				result.XndName = "Position";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_Normal:
			{
				result.XndName = "Normal";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_Tangent:
			{
				result.XndName = "Tangent";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_Color:
			{
				result.XndName = "VertexColor";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_UV:
			{
				result.XndName = "DiffuseUV";
				result.Stride = sizeof(v3dxVector2);
				break;
			}
			case VST_LightMap:
			{
				result.XndName = "LightMapUV";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_SkinIndex:
			{
				result.XndName = "BlendIndex";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_SkinWeight:
			{
				result.XndName = "BlendWeight";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_TerrainIndex:
			{
				result.XndName = "Fix_VIDTerrain";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_TerrainGradient:
			{
				result.XndName = "TerrainGradient";
				result.Stride = sizeof(DWORD);
				break;
			}
			case VST_InstPos:
			{
				result.XndName = nullptr;// "InstPos";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_InstQuat:
			{
				result.XndName = nullptr;// "InstQuat";
				result.Stride = sizeof(v3dxQuaternion);
				break;
			}
			case VST_InstScale:
			{
				result.XndName = nullptr;// "InstScale";
				result.Stride = sizeof(v3dxVector3);
				break;
			}
			case VST_F4_1:
			{
				result.XndName = nullptr;// "F41";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_F4_2:
			{
				result.XndName = nullptr;// "F42";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			case VST_F4_3:
			{
				result.XndName = nullptr;// "F43";
				result.Stride = sizeof(v3dVector4_t);
				break;
			}
			default:
				break;
		}

		return result;
	}

	void FVertexArray::GetStreamInfo(EVertexStreamType type, UINT* stride, UINT* element, int* varType)
	{
		NxRHI::GetVertexStreamInfo(type, stride, element, (EShaderVarType*)varType);
	}

	FVertexArray::FVertexArray()
	{

	}
	void FVertexArray::BindVB(EVertexStreamType stream, IVbView* buffer)
	{
		VertexBuffers[stream] = buffer;
	}
	void FVertexArray::Commit(ICommandList* cmdlist)
	{
		for (int i = 0; i < VST_Number; i++)
		{
			if (VertexBuffers[i] == nullptr)
			{
				cmdlist->SetVertexBuffer(i, nullptr, 0, 0);
				continue;
			}	

			cmdlist->SetVertexBuffer(i, VertexBuffers[i], 0, VertexBuffers[i]->Desc.Stride);
		}
	}
	FGeomMesh::FGeomMesh()
	{
		VertexArray = MakeWeakRef(new FVertexArray());
	}
	void FGeomMesh::Reset(bool bClearBuffer)
	{
		if (bClearBuffer)
		{
			IndexBuffer = nullptr;
			if (VertexArray != nullptr)
				VertexArray->Reset();
		}
		Atoms.clear();
	}
	void FGeomMesh::Commit(ICommandList* cmdlist)
	{
		if (VertexArray != nullptr)
			VertexArray->Commit(cmdlist);

		cmdlist->SetIndexBuffer(IndexBuffer, IsIndex32);
	}
	void FGeomMesh::BindIndexBuffer(IIbView* buffer)
	{
		IndexBuffer = buffer;
	}

	//===================================================================
	ENGINE_RTTI_IMPL(FMeshPrimitives);
	ENGINE_RTTI_IMPL(FMeshDataProvider);

	void FMeshPrimitives::CalcNormals32(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const UINT* triangles, UINT nTri)
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

	void FMeshPrimitives::CalcNormals16(OUT std::vector<v3dxVector3>& normals, const v3dxVector3* pos, UINT nVert, const USHORT* triangles, UINT nTri)
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

	FMeshPrimitives::FMeshPrimitives()
	{
	}

	FMeshPrimitives::~FMeshPrimitives()
	{
		
	}

	void FMeshPrimitives::Reset(bool bClearBuffer)
	{
		mDesc.SetDefault(); 
		mAtomExtData.clear();

		if (mGeometryMesh != nullptr)
			mGeometryMesh->Reset(bClearBuffer);
	}

	bool FMeshPrimitives::Init(IGpuDevice* device, const char* name, UINT atom)
	{
		mName = name;

		mGeometryMesh = MakeWeakRef(new FGeomMesh());
		mGeometryMesh->Atoms.resize(atom);
		mDesc.AtomNumber = atom;

		return true;
	}

	bool FMeshPrimitives::Init(IGpuDevice* device, FGeomMesh* mesh, const v3dxBox3* aabb)
	{
		//ASSERT(GLogicThreadId == vfxThread::GetCurrentThreadId());
		mDesc.AtomNumber = (UINT)mesh->Atoms.size();
		
		mGeometryMesh = mesh;

		mAABB = *aabb;

		this->GetResourceState()->SetStreamState(SS_Valid);

		return true;
	}

	void FMeshPrimitives::Save2Xnd(IGpuDevice* device, XndNode* pNode)
	{
        IBlobObject vbBuffer, ibBuffer;

		XndAttribute* pAttr = pNode->GetOrAddAttribute("HeadAttrib", 0, 0);
		pAttr->BeginWrite();
		{
			auto pos_vb = mGeometryMesh->GetVertexBuffer(VST_Position);
			mDesc.AtomNumber = (UINT)mGeometryMesh->Atoms.size();
			mDesc.PolyNumber = 0;
			for (const auto& i : mGeometryMesh->Atoms)
			{
				mDesc.PolyNumber += i[0].NumPrimitives / 3;
			}
			mDesc.VertexNumber = pos_vb->Desc.Size / sizeof(v3dxVector3);
			mDesc.Flags = 0;
			mDesc.UnUsed = 0;
			mDesc.GeoTabeNumber = 0;
			pAttr->Write(mDesc);

            AutoRef<NxRHI::IBuffer> copyVB;
            {
                FTransientCmd cmd(device, NxRHI::QU_Transfer);
                auto copyDesc = pos_vb->Buffer->Desc;
                copyDesc.Usage = USAGE_STAGING;
                copyDesc.CpuAccess = ECpuAccess::CAS_READ;
                copyDesc.MiscFlags = (EResourceMiscFlag)0;
                copyDesc.RowPitch = copyDesc.Size;
                copyDesc.DepthPitch = copyDesc.Size;
                copyVB = MakeWeakRef(device->CreateBuffer(&copyDesc));
                cmd.GetCmdList()->CopyBufferRegion(copyVB, 0, pos_vb->Buffer, 0, copyDesc.Size);
            }
            device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
            copyVB->FetchGpuData(0, &vbBuffer);
		}

		pAttr->Write(mAABB);
		pAttr->EndWrite();

		pAttr = pNode->GetOrAddAttribute("RenderAtom", 0, 0);
		pAttr->BeginWrite();
		for (const auto& i : mGeometryMesh->Atoms)
		{
			pAttr->Write(i[0].PrimitiveType);
			UINT uLodLevel = (UINT)i.size();
			pAttr->Write(uLodLevel);
			for (const auto& j : i)
			{
				pAttr->Write(j.StartIndex);
				pAttr->Write(j.NumPrimitives);
			}
		}
		pAttr->EndWrite();

		for (int i = EVertexStreamType::VST_Position; i < EVertexStreamType::VST_Number; i++)
		{
			auto vb = mGeometryMesh->GetVertexBuffer((EVertexStreamType)i);
			if (vb == nullptr)
				continue;
			auto info = GetStreamTypeInfo((EVertexStreamType)i);
			if (info.XndName == nullptr)
				continue;
			pAttr = pNode->GetOrAddAttribute(info.XndName, 0, 0);
			SaveVB(device, pAttr, vb, mMopherKeys[(EVertexStreamType)i], info.Stride);
		}
		
		auto ib = mGeometryMesh->GetIndexBuffer();
		if (ib)
		{
			pAttr = pNode->GetOrAddAttribute("Indices", 0, 0);
			pAttr->BeginWrite();

			const auto& desc = ib->Desc;

			UINT count = 0;
			if (mGeometryMesh->IsIndex32)
			{
				count = desc.Size / sizeof(DWORD);
				pAttr->Write(count);
				vBOOL bFormatIndex32 = 1;
				pAttr->Write(bFormatIndex32);
			}
			else
			{
				count = desc.Size / sizeof(WORD);
				pAttr->Write(count);
				vBOOL bFormatIndex32 = 0;
				pAttr->Write(bFormatIndex32);
			}

			AutoRef<NxRHI::IBuffer> copyVB;
			if (false == ib->Buffer->FetchGpuData(0, &ibBuffer))
			{
				{
					FTransientCmd cmd(device, NxRHI::QU_Transfer);
					auto copyDesc = ib->Buffer->Desc;
					copyDesc.Usage = USAGE_STAGING;
					copyDesc.CpuAccess = ECpuAccess::CAS_READ;
					copyDesc.MiscFlags = (EResourceMiscFlag)0;
					copyDesc.RowPitch = copyDesc.Size;
					copyDesc.DepthPitch = copyDesc.Size;
					copyVB = MakeWeakRef(device->CreateBuffer(&copyDesc));
					//cmd.GetCmdList()->CopyBufferRegion(copyVB, 0, ib->Buffer, 0, copyDesc.Size);

					AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw());
					cpDraw->BindBufferDest(copyVB);
					cpDraw->BindBufferSrc(ib->Buffer);
					cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
					cpDraw->FootPrint.Format = PXF_UNKNOWN;
					cpDraw->FootPrint.X = 0;
					cpDraw->FootPrint.Y = 0;
					cpDraw->FootPrint.Z = 0;
					cpDraw->FootPrint.Width = copyDesc.Size;
					cpDraw->FootPrint.Height = 1;
					cpDraw->FootPrint.Depth = 1;
					cpDraw->FootPrint.RowPitch = copyDesc.RowPitch;
					cpDraw->FootPrint.TotalSize = copyDesc.RowPitch * 1;

					cmd.GetCmdList()->PushGpuDraw(cpDraw);
					cmd.GetCmdList()->FlushDraws(true);
					
				}
			}

			device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
			copyVB->FetchGpuData(0, &ibBuffer);
			pAttr->Write((BYTE*)ibBuffer.GetData() + sizeof(UINT) * 2, desc.Size);
			pAttr->EndWrite();
		}
#if 1
		// cluster
		std::vector<FCluster> clusters;

		std::vector<v3dxVector3> verts;
		verts.resize(mDesc.VertexNumber);
		memcpy((BYTE*)&verts[0], (BYTE*)vbBuffer.GetData() + sizeof(UINT) * 2, mDesc.VertexNumber*sizeof(v3dxVector3));

		if (!mGeometryMesh->IsIndex32)
		{
            std::vector<WORD> indexData;
            indexData.resize(ib->Buffer->Desc.Size / sizeof(WORD));
            memcpy((BYTE*)&indexData[0], (BYTE*)ibBuffer.GetData() + sizeof(UINT) * 2, ib->Buffer->Desc.Size);

			// TODO: deal with WORD/int32
			std::vector<UINT32> indexes;
			for (int i = 0; i < indexData.size(); ++i)
			{
				// TODO: check ib range
				if (indexData[i] > verts.size())
					break;

				indexes.push_back(indexData[i]);
			}			
            RasterizeTriangles(verts, indexes, clusters);

			if (clusters.size() > 0)
			{
                pAttr = pNode->GetOrAddAttribute("Cluster", 0, 0);
                pAttr->BeginWrite();

				pAttr->Write(clusters.size());
				for (int i = 0; i < clusters.size(); ++i)
				{

				}

                pAttr->EndWrite();
			}
		}
#endif
	}

	//template<typename IndexType>
	void FMeshPrimitives::RasterizeTriangles(std::vector<v3dxVector3>& Verts, std::vector<UINT32>& Indexes, std::vector<FCluster>& clusters)
	{
		// calculate bouding box
		v3dxBox3 MeshBounds;
		for (int i = 0; i < Verts.size(); ++i)
		{
			MeshBounds += Verts[i];
			
			// TODO: check vertex color
		}

		clusters.clear();

        UINT32 NumTriangles = Indexes.size() / 3;

        FAdjacency Adjacency(Indexes.size());
        FEdgeHash EdgeHash(Indexes.size());

        auto GetPosition = [&Verts, &Indexes](UINT32 EdgeIndex, v3dxVector3& result)
        {
			// TODO: check index range
			if (Verts.size() > Indexes[EdgeIndex])
			{
				result = Verts[Indexes[EdgeIndex]];
				return true;
			}
			return false;
        };

        for (int EdgeIndex = 0; EdgeIndex < Indexes.size(); EdgeIndex++)
        {
            EdgeHash.Add_Concurrent(EdgeIndex, GetPosition);
        };

        for (int EdgeIndex = 0; EdgeIndex < Indexes.size(); EdgeIndex++)
        {
            INT32 AdjIndex = -1;
            INT32 AdjCount = 0;
            EdgeHash.ForAllMatching(EdgeIndex, false, GetPosition,
                [&](INT32 EdgeIndex, INT32 OtherEdgeIndex)
                {
                    AdjIndex = OtherEdgeIndex;
                    AdjCount++;
                });

            if (AdjCount > 1)
                AdjIndex = -2;

            Adjacency.Direct[EdgeIndex] = AdjIndex;
        }

        FDisjointSet DisjointSet(NumTriangles);

        for (UINT32 EdgeIndex = 0, Num = Indexes.size(); EdgeIndex < Num; EdgeIndex++)
        {
            if (Adjacency.Direct[EdgeIndex] == -2)
            {
                // EdgeHash is built in parallel, so we need to sort before use to ensure determinism.
                // This path is only executed in the rare event that an edge is shared by more than two triangles,
                // so performance impact should be negligible in practice.
                std::vector<INT32> Edges;
                EdgeHash.ForAllMatching(EdgeIndex, false, GetPosition,
                    [&](INT32 EdgeIndex0, INT32 EdgeIndex1)
                    {
                        Edges.push_back(EdgeIndex1);
                    });
                //Edges.Sort();
				std::sort(Edges.begin(), Edges.end());

                for (int i = 0; i < Edges.size(); ++i)
                {
                    Adjacency.Link(EdgeIndex, Edges[i]);
                }
            }

            Adjacency.ForAll(EdgeIndex,
                [&](INT32 EdgeIndex0, INT32 EdgeIndex1)
                {
                    if (EdgeIndex0 > EdgeIndex1)
                        DisjointSet.UnionSequential(EdgeIndex0 / 3, EdgeIndex1 / 3);
                });
        }

		// patition mesh
        FGraphPartitioner Partitioner(NumTriangles);
        {
            auto GetCenter = [&Verts, &Indexes](UINT32 TriIndex, v3dxVector3& Center)
            {
				if (Indexes[TriIndex * 3 + 0] >= Verts.size() ||
					Indexes[TriIndex * 3 + 1] >= Verts.size() ||
					Indexes[TriIndex * 3 + 2] >= Verts.size())
				{
					return false;
				}
                Center = Verts[Indexes[TriIndex * 3 + 0]];
                Center += Verts[Indexes[TriIndex * 3 + 1]];
                Center += Verts[Indexes[TriIndex * 3 + 2]];
                Center *= (1.0f / 3.0f);

				return true;
            };
			// TODO: material group
			std::vector<INT32> MaterialIndexes;
			MaterialIndexes.clear();

            Partitioner.BuildLocalityLinks(DisjointSet, MeshBounds, MaterialIndexes, GetCenter);

            auto Graph = Partitioner.NewGraph(NumTriangles * 3);

            for (UINT32 i = 0; i < NumTriangles; i++)
            {
                Graph->AdjacencyOffset[i] = Graph->Adjacency.size();

                UINT32 TriIndex = Partitioner.Indexes[i];

                for (int k = 0; k < 3; k++)
                {
                    Adjacency.ForAll(3 * TriIndex + k,
                        [&Partitioner, Graph](INT32 EdgeIndex, INT32 AdjIndex)
                        {
                            Partitioner.AddAdjacency(Graph, AdjIndex / 3, 4 * 65);
                        });
                }

                Partitioner.AddLocalityLinks(Graph, TriIndex, 1);
            }
            Graph->AdjacencyOffset[NumTriangles] = Graph->Adjacency.size();

			bool bSingleThreaded = NumTriangles < 5000;

            Partitioner.PartitionStrict(Graph, FCluster::ClusterSize - 4, FCluster::ClusterSize, !bSingleThreaded);
            ASSERT(Partitioner.Ranges.size() > 0);
        }

		// build cluster
		clusters.resize(Partitioner.Ranges.size());
		for (int i = 0; i < Partitioner.Ranges.size(); ++i)
		{
			auto& Range = Partitioner.Ranges[i];
			clusters[i] = FCluster(Verts, Indexes, Range.Begin, Range.End, Partitioner, Adjacency);
		}
	}
	bool FMeshPrimitives::LoadXnd(IGpuDevice* device, const char* name, XndHolder* xnd, bool isLoad)
	{
		if (xnd == nullptr)
			return FALSE;
		mXnd = xnd;
		mName = name;
		auto pNode = xnd->GetRootNode();
		
		mGeometryMesh = MakeWeakRef(new FGeomMesh());

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
			mGeometryMesh->Atoms.clear();
			mGeometryMesh->Atoms.resize(mDesc.AtomNumber);
			pAttr->BeginRead();
			for (UINT i = 0; i < mDesc.AtomNumber; i++)
			{
				FMeshAtomDesc dpDesc;
				pAttr->Read(dpDesc.PrimitiveType);
				UINT uLodLevel;
				pAttr->Read(uLodLevel);
				for (UINT j = 0; j < uLodLevel; j++)
				{
					pAttr->Read(dpDesc.StartIndex);
					pAttr->Read(dpDesc.NumPrimitives);
					mGeometryMesh->PushAtomDesc(i, dpDesc);
				}
			}
			pAttr->EndRead();
		}
		else
		{
			return false;
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
			return RestoreResource(device);
		}
	}

	void FMeshPrimitives::InvalidateResource()
	{
		//mGeometryMesh->InvalidateResource();
	}

	AutoRef<IVbView> FMeshPrimitives::LoadVB(IGpuDevice* device, XndAttribute* pAttr, UINT stride, TimeKeys& tkeys, UINT& resSize, EVertexStreamType stream)
	{
		pAttr->BeginRead();
		UINT uVert, uKey, uStride;
		pAttr->Read(uStride);
		pAttr->Read(uVert);
		pAttr->Read(uKey);
		tkeys.Load(*pAttr);
		
		FVbvDesc vbvDesc{};
		vbvDesc.Stride = stride;
		vbvDesc.Size = stride * uKey * uVert;
		BYTE* data = new BYTE[vbvDesc.Size];
		if (stride == uStride)
		{	
			pAttr->Read(data, vbvDesc.Size);
		}
		else if (stride > uStride)
		{
			for (UINT i = 0; i<uKey; i++)
			{
				BYTE* pCurKeyAddr = data + i * (uVert * stride);
				for (UINT j = 0; j < uVert; j++)
				{
					pAttr->Read(pCurKeyAddr + j * stride, uStride);
				}
			}
		}
		else
		{
			for (UINT i = 0; i < uKey; i++)
			{
				BYTE* pCurKeyAddr = data + i * (uVert * stride);
				for (UINT j = 0; j < uVert; j++)
				{
					pAttr->Read(pCurKeyAddr + j * stride, vbvDesc.Stride);
				}
			}
		}
		vbvDesc.InitData = data;
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

		auto vb = MakeWeakRef(device->CreateVBV(nullptr, &vbvDesc));
		
		Safe_DeleteArray(data);
		resSize += vbvDesc.Size;

		return vb;
	}

	void FMeshPrimitives::SaveVB(IGpuDevice* device, XndAttribute* pAttr, IVbView* vb, TimeKeys& tkeys, UINT stride)
	{
		AutoRef<NxRHI::IBuffer> copyVB;
		IBlobObject buffData;
		if (false == vb->Buffer->FetchGpuData(0, &buffData))
		{
			{
				FTransientCmd cmd(device, NxRHI::QU_Transfer);
				auto copyDesc = vb->Buffer->Desc;
				copyDesc.Usage = USAGE_STAGING;
				copyDesc.CpuAccess = ECpuAccess::CAS_READ;
				copyDesc.MiscFlags = (EResourceMiscFlag)0;
				copyDesc.RowPitch = copyDesc.Size;
				copyDesc.DepthPitch = copyDesc.Size;
				copyVB = MakeWeakRef(device->CreateBuffer(&copyDesc));
				//cmd.GetCmdList()->CopyBufferRegion(copyVB, 0, vb->Buffer, 0, copyDesc.Size);

				AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw());
				cpDraw->BindBufferDest(copyVB);
				cpDraw->BindBufferSrc(vb->Buffer);
				cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
				cpDraw->FootPrint.Format = PXF_UNKNOWN;
				cpDraw->FootPrint.X = 0;
				cpDraw->FootPrint.Y = 0;
				cpDraw->FootPrint.Z = 0;
				cpDraw->FootPrint.Width = copyDesc.Size;
				cpDraw->FootPrint.Height = 1;
				cpDraw->FootPrint.Depth = 1;
				cpDraw->FootPrint.RowPitch = copyDesc.RowPitch;
				cpDraw->FootPrint.TotalSize = copyDesc.RowPitch * 1;

				cmd.GetCmdList()->PushGpuDraw(cpDraw);
				cmd.GetCmdList()->FlushDraws(true);
			}
			device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
			copyVB->FetchGpuData(0, &buffData);
		}

		if (buffData.GetSize() == 0)
			return;

		pAttr->BeginWrite();

		UINT uVert, uKey;
		uKey = tkeys.GetKeyCount();
		if (uKey == 0)
			uKey = 1;
		uVert = (UINT)vb->Desc.Size / (uKey * stride);
		pAttr->Write(stride);
		pAttr->Write(uVert);
		pAttr->Write(uKey);
		tkeys.Save(*pAttr);
		pAttr->Write((BYTE*)buffData.GetData() + sizeof(UINT) * 2, (UINT)vb->Desc.Size);

		pAttr->EndWrite();
	}

	bool FMeshPrimitives::RefreshResource(IGpuDevice* device, const char* name, XndNode* node)
	{
		if (GetResourceState()->GetStreamState() == SS_Streaming)
			return FALSE;
		if (mXnd != nullptr)
		{
			mXnd->TryReleaseHolder();
			if (LoadXnd(device, mName.c_str(), mXnd, true))
			{
				GetResourceState()->SetStreamState(SS_Valid);
				return TRUE;
			}
		}
		return false;
	}

	bool FMeshPrimitives::RestoreResource(IWeakReference* pDevice)
	{
		if (mXnd == nullptr)
			return false;

		IGpuDevice* device = (IGpuDevice*)pDevice;

		auto pNode = mXnd->GetRootNode();

		UINT resSize = 0;
		for (int i = EVertexStreamType::VST_Position; i < EVertexStreamType::VST_Number; i++)
		{
			auto info = GetStreamTypeInfo((EVertexStreamType)i);
			if (info.XndName == nullptr)
				continue;
			auto pAttr = pNode->TryGetAttribute(info.XndName);
			if (pAttr != nullptr)
			{	
				auto vb = LoadVB(device, pAttr, info.Stride, mMopherKeys[i], resSize, (EVertexStreamType)i);
				mGeometryMesh->VertexArray->BindVB((EVertexStreamType)i, vb);

				vb->SetDebugName((mName + ":" + info.XndName).c_str());
			}
		}

		auto pAttr = pNode->TryGetAttribute("Indices");
		if (pAttr)
		{
			pAttr->BeginRead();

			FIbvDesc ibvDesc{};
			UINT count;
			pAttr->Read(count);
			vBOOL bFormatIndex32;
			pAttr->Read(bFormatIndex32);
			ibvDesc.Stride = ((bFormatIndex32) ? sizeof(DWORD) : sizeof(WORD));
			ibvDesc.Size = count * ibvDesc.Stride;

			BYTE* data = new BYTE[ibvDesc.Size];
			pAttr->Read(data, ibvDesc.Size);
			pAttr->EndRead();

			ibvDesc.InitData = data;
			auto ib = MakeWeakRef(device->CreateIBV(nullptr, &ibvDesc));
			mGeometryMesh->BindIndexBuffer(ib);
			mGeometryMesh->IsIndex32 = bFormatIndex32;
			Safe_DeleteArray(data);

			ib->SetDebugName((mName + ":IB").c_str());

			resSize += ibvDesc.Size;
		}

		mXnd->TryReleaseHolder();
		this->GetResourceState()->SetResourceSize(resSize);
		
		return true;
	}

	const char* FMeshPrimitives::GetName() const
	{
		return mName.c_str();
	}

	UINT FMeshPrimitives::GetAtomNumber() const
	{
		return (UINT)mGeometryMesh->Atoms.size();
	}

	FMeshAtomDesc* FMeshPrimitives::GetAtom(UINT index, UINT lod) const
	{
		return mGeometryMesh->GetAtomDesc(index,lod);
	}

	void FMeshPrimitives::SetAtom(UINT index, UINT lod, const FMeshAtomDesc& desc)
	{
		mGeometryMesh->SetAtomDesc(index, lod, desc);
	}

	bool FMeshPrimitives::SetGeomtryMeshStream(IGpuDevice* device, EVertexStreamType stream, void* data, UINT size, UINT stride, ECpuAccess cpuAccess)
	{
		IVbView* ovb = nullptr;
		if (mGeometryMesh != nullptr &&
			(ovb = mGeometryMesh->VertexArray->VertexBuffers[stream]) != nullptr &&
			ovb->Desc.Stride == stride &&
			ovb->Desc.Size >= size)
		{
			ovb->UpdateGpuData(0, data, size);
			return true;
		}

		FVbvDesc vbvDesc{};
		vbvDesc.Stride = stride;
		vbvDesc.Size = size;
		vbvDesc.InitData = data;
		auto vb = MakeWeakRef(device->CreateVBV(nullptr, &vbvDesc));
		if (vb == nullptr)
			return false;
		mGeometryMesh->VertexArray->BindVB(stream, vb);
		return true;
	}

	bool FMeshPrimitives::SetGeomtryMeshIndex(IGpuDevice* device, void* data, UINT size, bool isBit32, ECpuAccess cpuAccess)
	{
		IIbView* oib = nullptr;
		if (mGeometryMesh != nullptr &&
			(oib = mGeometryMesh->IndexBuffer) != nullptr &&
			oib->Desc.Stride == (isBit32 ? sizeof(UINT) : sizeof(USHORT)) &&
			oib->Desc.Size >= size)
		{
			oib->UpdateGpuData(0, data, size);
			return true;
		}

		FIbvDesc ibvDesc{};
		ibvDesc.Stride = isBit32 ? sizeof(UINT) : sizeof(USHORT);
		ibvDesc.Size = size;
		ibvDesc.InitData = data;
		auto ib = MakeWeakRef(device->CreateIBV(nullptr, &ibvDesc));
		if (ib == nullptr)
			return false;

		mGeometryMesh->IsIndex32 = isBit32;
		mGeometryMesh->BindIndexBuffer(ib);
		return true;
	}

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
	
	FInputLayoutDesc* FMeshDataProvider::GetInputLayoutDesc()
	{
		return nullptr;
	}

	bool FMeshDataProvider::ToMesh(IGpuDevice* device, FMeshPrimitives* mesh)
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
			mesh->SetGeomtryMeshStream(device, (EVertexStreamType)i,
				mVertexBuffers[i]->GetData(),
				mVertexBuffers[i]->GetSize(),
				GetStreamTypeInfo((EVertexStreamType)i).Stride, ECpuAccess::CAS_DEFAULT);
		}
		if (IndexBuffer->GetSize() > 0)
			mesh->SetGeomtryMeshIndex(device, IndexBuffer->GetData(), IndexBuffer->GetSize(), IsIndex32, ECpuAccess::CAS_DEFAULT);
		//mesh->mGeometryMesh->BindInputLayout();
		
		mesh->mGeometryMesh->Atoms = mAtoms;
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

			mVertexBuffers[i] = MakeWeakRef(new IBlobObject());
			IBlobObject buffData;
			vb->Buffer->FetchGpuData(0, &buffData);

			mVertexBuffers[i]->PushData((BYTE*)buffData.GetData() + sizeof(UINT) * 2, vb->Buffer->Desc.Size);
		}

		IndexBuffer = MakeWeakRef(new IBlobObject());
		IBlobObject buffData;
		ib->Buffer->FetchGpuData(0, &buffData);
		
		IndexBuffer->PushData((BYTE*)buffData.GetData() + sizeof(UINT) * 2, ib->Buffer->Desc.Size);
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
			auto info = GetStreamTypeInfo((EVertexStreamType)i);
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
			rcqScale = v3dxVector3(1.0f / scale->x, 1.0f / scale->y, 1.0f / scale->z);
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
							result->Distance = sqrt(v3dxCalSquareDistance(&result->Position, vStart));
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
							result->Distance = sqrt(v3dxCalSquareDistance(&result->Position, vStart));
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

		auto stride = GetStreamTypeInfo(stream).Stride;
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
	bool FMeshDataProvider::AddVertex_Pos_UV_Color(const void* pVertex, UINT num, bool bInvertY, float CanvasHeight)
	{
		struct FTmpVertex
		{
			v3dxVector3 Pos;
			v3dxVector2 UV;
			DWORD Color;
		};
		auto verts = (FTmpVertex*)pVertex;

		size_t posOffset = 0;
		size_t uvOffset = 0;
		size_t colorOffset = 0;

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
		if (bInvertY)
		{
			for (UINT i = 0; i < num; i++)
			{
				auto pos = verts[i].Pos;
				pos.y = CanvasHeight - pos.y;
				posVB->SetValueToOffset((UINT)posOffset, pos);
				posOffset += sizeof(v3dxVector3);
				uvVB->SetValueToOffset((UINT)uvOffset, verts[i].UV);
				uvOffset += sizeof(v3dxVector2);
				colorVB->SetValueToOffset((UINT)colorOffset, verts[i].Color);
				colorOffset += sizeof(DWORD);
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
