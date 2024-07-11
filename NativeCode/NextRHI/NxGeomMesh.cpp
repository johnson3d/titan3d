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
	void FTransientBuffer::Initialize(IGpuDevice* device, UINT size, EBufferType type, EGpuUsage usage, ECpuAccess cpuAccess)
	{
		FBufferDesc desc;
		desc.SetDefault(false, type);
		desc.Size = size;
		desc.CpuAccess = cpuAccess;
		desc.Usage = usage;
		mBuffer = MakeWeakRef(device->CreateBuffer(&desc));
		Reset();
	}
	UINT FTransientBuffer::Alloc(IGpuDevice* device, UINT size, bool bGrow)
	{
		if (mBuffer == nullptr || mCurrentOffset + size >= mBuffer->Desc.Size)
		{
			if (bGrow == false)
			{
				return UINT32_MAX;
			}
			else
			{
				FBufferDesc desc;
				desc.SetDefault(false, (EBufferType)(EBufferType::BFT_Vertex | EBufferType::BFT_Index));
				desc.Size = mCurrentOffset + size * 10;
				//desc.CpuAccess = cpuAccess;
				desc.Usage = EGpuUsage::USAGE_DEFAULT;
				mBuffer = MakeWeakRef(device->CreateBuffer(&desc));
			}
		}
		auto result = mCurrentOffset;
		mCurrentOffset += size;
		return result;
	}
	IVbView* FTransientBuffer::AllocVBV(IGpuDevice* device, UINT stride, UINT size, bool bGrow)
	{
		FVbvDesc vbvDesc{};
		vbvDesc.SetDefault();
		vbvDesc.Stride = stride;
		vbvDesc.Size = size;
		vbvDesc.Offset = Alloc(device, size, bGrow);
		return device->CreateVBV(mBuffer, &vbvDesc);
	}
	IIbView* FTransientBuffer::AllocIBV(IGpuDevice* device, UINT stride, UINT size, bool bGrow)
	{
		FIbvDesc ibvDesc{};
		ibvDesc.SetDefault();
		ibvDesc.Stride = stride;
		ibvDesc.Size = size;

		ibvDesc.Offset = Alloc(device, size, bGrow);
		return device->CreateIBV(mBuffer, &ibvDesc);
	}

	void FVertexArray::GetStreamInfo(EVertexStreamType type, UINT* stride, UINT* element, int* varType)
	{
		NxRHI::GetVertexStreamInfo(type, stride, element, (EShaderVarType*)varType);
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
	/*FGeomMesh::FGeomMesh()
	{
		VertexArray = MakeWeakRef(new FVertexArray());
	}*/
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

		mClustersIndexView = nullptr;
        if (mClustersVertexArray != nullptr)
			mClustersVertexArray->Reset();
	}

	bool FMeshPrimitives::Init(IGpuDevice* device, const char* name, UINT atom)
	{
		mName = name;

		mGeometryMesh = MakeWeakRef(device->CreateGeomMesh());
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
				mDesc.PolyNumber += i[0].NumPrimitives;
			}
			mDesc.VertexNumber = pos_vb->Desc.Size / sizeof(v3dxVector3);
			mDesc.Flags = 0;
			mDesc.UnUsed = 0;
			mDesc.GeoTabeNumber = 0;
			pAttr->Write(mDesc);

            /*AutoRef<NxRHI::IBuffer> copyVB;
            {
                FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadVB");
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
            copyVB->FetchGpuData(0, &vbBuffer);*/
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
					FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadIB");
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
				}
				device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
				copyVB->FetchGpuData(0, &ibBuffer);
			}
			
			pAttr->Write((BYTE*)ibBuffer.GetData() + sizeof(UINT) * 2, desc.Size);
			pAttr->EndWrite();
		}
#if 0
		// cluster
		std::vector<QuarkCluster> clusters;

		std::vector<v3dxVector3> verts;
		verts.resize(mDesc.VertexNumber);
		memcpy((BYTE*)&verts[0], (BYTE*)vbBuffer.GetData() + sizeof(UINT) * 2, mDesc.VertexNumber*sizeof(v3dxVector3));

		if (!mGeometryMesh->IsIndex32)
		{
            std::vector<WORD> indexData;
            indexData.resize(ib->Buffer->Desc.Size / sizeof(WORD));
            memcpy((BYTE*)&indexData[0], (BYTE*)ibBuffer.GetData() + sizeof(UINT) * 2, ib->Buffer->Desc.Size);

			// TODO: deal with WORD/int32
			std::vector<UINT> indexes;
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
	
	bool FMeshPrimitives::GetMeshBuffer(IGpuDevice* device, std::vector<v3dxVector3>& Verts, std::vector<UINT>& Indexes)
    {
		if (!device)
			return false;

        IBlobObject vbBuffer, ibBuffer;

		// vb
        auto pos_vb = mGeometryMesh->GetVertexBuffer(VST_Position);

        AutoRef<NxRHI::IBuffer> copyVB;
        {
            FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadVB");
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

		// ib
        auto ib = mGeometryMesh->GetIndexBuffer();
		if (ib)
		{
            if (false == ib->Buffer->FetchGpuData(0, &ibBuffer))
            {
                FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadIB");
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
            }

            device->GetCmdQueue()->Flush(EQueueType::QU_Transfer);
            copyVB->FetchGpuData(0, &ibBuffer);
		}
		ASSERT(mDesc.VertexNumber > 0);
		Verts.resize(mDesc.VertexNumber);
        memcpy((BYTE*)&Verts[0], (BYTE*)vbBuffer.GetData() + sizeof(UINT) * 2, mDesc.VertexNumber * sizeof(v3dxVector3));

		// TODO: havn't deal with 32-bit ib
		if (mGeometryMesh->IsIndex32)
		{
			return false;
		}

        std::vector<WORD> indexData;
        indexData.resize(ib->Buffer->Desc.Size / sizeof(WORD));
        memcpy((BYTE*)&indexData[0], (BYTE*)ibBuffer.GetData() + sizeof(UINT) * 2, ib->Buffer->Desc.Size);

        // TODO: deal with WORD/int32
        for (int i = 0; i < indexData.size(); ++i)
        {
            // TODO: check ib range
            if (indexData[i] > Verts.size())
                break;

			Indexes.push_back(indexData[i]);
        }
		return true;
	}
	int FMeshPrimitives::ClusterizeTriangles(IGpuDevice* device)
	{
		std::vector<v3dxVector3> Verts;
		std::vector<UINT> Indexes;

		if (!GetMeshBuffer(device, Verts, Indexes))
			return 0;
#if true
		// calculate bouding box
		v3dxBox3 MeshBounds;
		for (int i = 0; i < Verts.size(); ++i)
		{
			MeshBounds += Verts[i];
			
			// TODO: check vertex color
		}
		mClusters.clear();

        UINT NumTriangles = UINT(Indexes.size()) / 3;

        FAdjacency Adjacency(UINT(Indexes.size()));
        FEdgeHash EdgeHash(UINT(Indexes.size()));

        auto GetPosition = [&Verts, &Indexes](UINT EdgeIndex, v3dxVector3& result)
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

        for (UINT EdgeIndex = 0, Num = UINT(Indexes.size()); EdgeIndex < Num; EdgeIndex++)
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
            auto GetCenter = [&Verts, &Indexes](UINT TriIndex, v3dxVector3& Center)
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

            for (UINT i = 0; i < NumTriangles; i++)
            {
                Graph->AdjacencyOffset[i] = INT32(Graph->Adjacency.size());

                UINT TriIndex = Partitioner.Indexes[i];

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
            Graph->AdjacencyOffset[NumTriangles] = INT32(Graph->Adjacency.size());

			bool bSingleThreaded = NumTriangles < 5000;

            Partitioner.PartitionStrict(Graph, QuarkCluster::ClusterSize - 4, QuarkCluster::ClusterSize, !bSingleThreaded);
            ASSERT(Partitioner.Ranges.size() > 0);
        }

		// build cluster
		mClusters.resize(Partitioner.Ranges.size());
		for (int i = 0; i < Partitioner.Ranges.size(); ++i)
		{
			auto& Range = Partitioner.Ranges[i];
			mClusters[i] = QuarkCluster(Verts, Indexes, Range.Begin, Range.End, Partitioner, Adjacency);
		}
		return int(mClusters.size());
#else
		// export all vb, ib 
		QuarkCluster cluster;	
		cluster.Verts.resize(Verts.size() * 3);
		cluster.Indexes.resize(Indexes.size());
		memcpy(&cluster.Verts[0], &Verts[0], Verts.size() * sizeof(v3dxVector3));
		memcpy(&cluster.Indexes[0], &Indexes[0], Indexes.size() * sizeof(UINT));

		cluster.VertexStart = 0;
		cluster.VertexCount = Verts.size();
		cluster.IndexStart = 0;
		cluster.IndexCount = Indexes.size();

		mClusters.push_back(cluster);
#endif
	}
    bool FMeshPrimitives::SaveClusters(XndNode* pNode)
    {
        ASSERT(mClusters.size() >= 0);
        auto pAttr = pNode->GetOrAddAttribute("Cluster", 0, 0);
        pAttr->BeginWrite();
		
		int clusterCount = int(mClusters.size());
        pAttr->Write(clusterCount);

        for (int i = 0; i < mClusters.size(); ++i)
        {
            // vb
            int vertCount = int(mClusters[i].Verts.size());
            pAttr->Write(vertCount);
            pAttr->Write((BYTE*)&mClusters[i].Verts[0], vertCount * sizeof(float));
            // ib
            int indexCount = int(mClusters[i].Indexes.size());
            pAttr->Write(indexCount);
            pAttr->Write((BYTE*)&mClusters[i].Indexes[0], indexCount * sizeof(UINT));
			// bounding box
			pAttr->Write((BYTE*)&mClusters[i].Bounds, sizeof(v3dxBox3));
        }

        pAttr->EndWrite();

        return TRUE;
    }
	int FMeshPrimitives::LoadClusters(XndHolder* xnd, IGpuDevice* device)
	{
		if (xnd == nullptr)
			return 0;

		mClusters.clear();

		auto pNode = xnd->GetRootNode();
		XndAttribute* pAttr = pNode->TryGetAttribute("Cluster");
		int vbCount, ibCount;
		vbCount = ibCount = 0;
		if (pAttr)
		{
			pAttr->BeginRead();
			int clusterCount;
			pAttr->Read(clusterCount);

			mClusters.resize(clusterCount);
			for (int i = 0; i < clusterCount; ++i)
			{
				// vb
				int vertCount;
				pAttr->Read(vertCount);
				vbCount += vertCount / 3;
				mClusters[i].Verts.resize(vertCount);
				pAttr->Read((BYTE*)&mClusters[i].Verts[0], vertCount*sizeof(float));

				// ib
				int indexCount;
				pAttr->Read(indexCount);
				ibCount += indexCount;
				mClusters[i].Indexes.resize(indexCount);
				pAttr->Read((BYTE*)&mClusters[i].Indexes[0], indexCount * sizeof(UINT));

				// bounding box				
                pAttr->Read((BYTE*)&mClusters[i].Bounds, sizeof(v3dxBox3));
			}

			pAttr->EndRead();
		}

		// post process vb, ib info
		if (vbCount > 0 && ibCount > 0)
		{
            int vbOffset, ibOffset;
            vbOffset = ibOffset = 0;

            mClustersVB.resize(vbCount);
            mClustersIB.resize(ibCount);

            for (int i = 0; i < mClusters.size(); ++i)
            {
                // vb, ib offset info
                mClusters[i].VertexStart = vbOffset;
                mClusters[i].VertexCount = int(mClusters[i].Verts.size()) / 3;
                mClusters[i].IndexStart = ibOffset;
                mClusters[i].IndexCount = int(mClusters[i].Indexes.size());

                memcpy((BYTE*)&mClustersVB[vbOffset], (BYTE*)&mClusters[i].Verts[0], int(mClusters[i].Verts.size()) * sizeof(float));
                memcpy((BYTE*)&mClustersIB[ibOffset], (BYTE*)&mClusters[i].Indexes[0], int(mClusters[i].Indexes.size()) * sizeof(UINT));

                vbOffset += mClusters[i].VertexCount;
                ibOffset += mClusters[i].IndexCount;
            }

			// vb view
			mClustersVertexArray = MakeWeakRef(new FVertexArray());
            FVbvDesc vbvDesc{};
            vbvDesc.Stride = sizeof(v3dxVector3);
            vbvDesc.Size = vbvDesc.Stride * vbCount;
			vbvDesc.InitData = (BYTE*)&mClustersVB[0];
			auto vb = MakeWeakRef(device->CreateVBV(nullptr, &vbvDesc));
			mClustersVertexArray->BindVB(EVertexStreamType::VST_Position, vb);

			// ib view
            FIbvDesc ibvDesc{};
            ibvDesc.Stride = sizeof(UINT);
            ibvDesc.Size = ibCount * ibvDesc.Stride;

            ibvDesc.InitData = (BYTE*)&mClustersIB[0];
			mClustersIndexView = MakeWeakRef(device->CreateIBV(nullptr, &ibvDesc));
		}
		
		return int(mClusters.size());
	}
	QuarkCluster* FMeshPrimitives::GetCluster(int index)
	{
		ASSERT(index < int(mClusters.size()));
		return &mClusters[index];
	}
	bool FMeshPrimitives::LoadXnd(IGpuDevice* device, const char* name, XndHolder* xnd, bool isLoad)
	{
		if (xnd == nullptr)
			return FALSE;
		mXnd = xnd;
		mName = name;
		auto pNode = xnd->GetRootNode();
		
		mGeometryMesh = MakeWeakRef(device->CreateGeomMesh());

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
				mAABB.MergeVertex(pPosition[i]);
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
				FTransientCmd cmd(device, NxRHI::QU_Transfer, "Mesh.ReadVB");
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

	bool FMeshPrimitives::SetGeomtryMeshStream(ICommandList* cmd, EVertexStreamType stream, void* data, UINT size, UINT stride, ECpuAccess cpuAccess)
	{
		
		IVbView* ovb = nullptr;
		if (mGeometryMesh != nullptr &&
			(ovb = mGeometryMesh->VertexArray->VertexBuffers[stream]) != nullptr &&
			ovb->Desc.Stride == stride &&
			ovb->Desc.Size >= size)
		{
			ovb->UpdateGpuData(cmd, 0, data, size);
			return true;
		}

		if (mVertexBuffer != nullptr)
		{
			auto vb = MakeWeakRef(mVertexBuffer->AllocVBV(cmd->mDevice.GetPtr(), stride, size, true));
			if (vb == nullptr)
				return false;
			vb->UpdateGpuData(cmd, 0, data, size);
			mGeometryMesh->VertexArray->BindVB(stream, vb);
		}
		else
		{
			FVbvDesc vbvDesc{};
			vbvDesc.Stride = stride;
			vbvDesc.Size = size;
			vbvDesc.InitData = data;
			auto vb = MakeWeakRef(cmd->mDevice.GetPtr()->CreateVBV(nullptr, &vbvDesc));
			if (vb == nullptr)
				return false;
			mGeometryMesh->VertexArray->BindVB(stream, vb);
		}
		return true;
	}

	bool FMeshPrimitives::SetGeomtryMeshIndex(ICommandList* cmd, void* data, UINT size, bool isBit32, ECpuAccess cpuAccess)
	{
		IIbView* oib = nullptr;
		if (mGeometryMesh != nullptr &&
			(oib = mGeometryMesh->IndexBuffer) != nullptr &&
			oib->Desc.Stride == (isBit32 ? sizeof(UINT) : sizeof(USHORT)) &&
			oib->Desc.Size >= size)
		{
			oib->UpdateGpuData(cmd, 0, data, size);
			return true;
		}

		mGeometryMesh->IsIndex32 = isBit32;
		if (mIndexBuffer != nullptr)
		{
			auto ib = MakeWeakRef(mIndexBuffer->AllocIBV(cmd->mDevice.GetPtr(), isBit32 ? sizeof(UINT) : sizeof(USHORT), size, true));
			if (ib == nullptr)
				return false;
			ib->UpdateGpuData(cmd, 0, data, size);
			mGeometryMesh->BindIndexBuffer(ib);
		}
		else
		{
			FIbvDesc ibvDesc{};
			ibvDesc.Stride = isBit32 ? sizeof(UINT) : sizeof(USHORT);
			ibvDesc.Size = size;
			ibvDesc.InitData = data;
			auto ib = MakeWeakRef(cmd->mDevice.GetPtr()->CreateIBV(nullptr, &ibvDesc));
			if (ib == nullptr)
				return false;

			mGeometryMesh->BindIndexBuffer(ib);
		}
		return true;
	}

	
}

NS_END
