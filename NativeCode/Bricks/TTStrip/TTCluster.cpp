#include "TTCluster.h"
#include <algorithm> 
#include <array>
#include <memory>
#include <thread>
#include <functional>
#include <cmath>

//#define PARALLEL_EXECUTE
#if !PLATFORM_APPLE
#define CPP_17 
#endif

#ifdef CPP_17
#include <execution>
#endif

// #define PRIORITIZE_NEIGHBOR_FACES
#define DEGENERATE_TRIANGLE_INVALID_INDEX

namespace TTStrip
{
    FCluster::~FCluster()
    {
        // clean all edges.
        for (auto& e : HalfEdges)
        {
            while (e != nullptr)
            {
                auto t = e;
                e = e->Next;
                delete t;
            }
        }
    }

    UINT FCluster::FindNeighborCount(const FFace& face) const
    {
        UINT c = 0;
        for (UINT i = 0; i < 3; i++)
        {
            if (face.Edges[i]->NeighborFace != -1)
                c++;
        }
        return c;
    }

    void FCluster::AddFace(const FFace& face)
    {

        FFace NewFace;
        memcpy(NewFace.VertIdx, face.VertIdx, sizeof(NewFace.VertIdx));
        NewFace.Normal = face.Normal;

        const UINT* v = &NewFace.VertIdx[0];
        UINT LocalFaceId = (UINT)Faces.size();

        NewFace.Edges[0] = AddEdge(v[0], v[1], LocalFaceId, HalfEdges);
        NewFace.Edges[1] = AddEdge(v[1], v[2], LocalFaceId, HalfEdges);
        NewFace.Edges[2] = AddEdge(v[2], v[0], LocalFaceId, HalfEdges);

        NewFace.Edges[0]->NextEdgeInFace = NewFace.Edges[1];
        NewFace.Edges[1]->NextEdgeInFace = NewFace.Edges[2];
        NewFace.Edges[2]->NextEdgeInFace = NewFace.Edges[0];

        Faces.push_back(NewFace);
    }

    bool FCluster::CheckAndUpdateFaceNormal(FStripifyResult& result, const FFace& f2)
    {
        double dotResult = result.AveNormal.Dot(f2.Normal);
        if (dotResult > kCosNormalAngleThreshold)
        {
            result.AccumNormal += Double3(f2.Normal.x, f2.Normal.y, f2.Normal.z);
            result.AveNormal = result.AccumNormal.Normalize();
            return true;
        }
        else
            return false;
    }

#ifdef PRIORITIZE_NEIGHBOR_FACES
    /// <summary>
    /// Goal: Try to find all faces that are
    /// a. within angle threshold and
    /// b. are the neighbors of current running full strip.
    /// </summary>
    /// <param name="LastStripResult"></param>
    /// <returns></returns>
    std::vector<UINT> FCluster::FindConnectedStripCandidateFaces(const FStripifyResult& RunningFullStripState)
    {
        // TODO: optimize it. Cache 0 value face flag.
        // Find all neighbor faces for current running full strip.
        std::vector<FUseFlag> UsedFaceFlags(Faces.size(), FUseFlag());
        std::vector<FUseFlag> CheckedFaceFlags(Faces.size(), FUseFlag());
        std::vector<UINT> NeighborFaces;
        for (UINT f : RunningFullStripState.ExperimentStripFaces)
        {
            UsedFaceFlags[f].SetUsed();
        }
        for (UINT f : RunningFullStripState.ExperimentStripFaces)
        {
            const FFace& face = Faces[f];
            for (UINT e = 0; e < 3; e++)
            {
                UINT NeighborFace = face.Edges[e]->NeighborFace;
                if (NeighborFace != UINT_MAX)
                {
                    if (!FaceMasks[NeighborFace].GetUsed() && !UsedFaceFlags[NeighborFace].GetUsed() && !CheckedFaceFlags[NeighborFace].GetUsed())
                    {
                        NeighborFaces.push_back(NeighborFace);
                        CheckedFaceFlags[NeighborFace].SetUsed();
                    }
                }
            }
        }
        // Select neighbor faces that fit the normal angle limit.
        std::vector<UINT> NormalFilteredNeighborFaces;
        for (UINT f : NeighborFaces)
        {
            const FFace& face = Faces[f];
            if (RunningFullStripState.AveNormal.Dot(face.Normal) > kCosNormalAngleThreshold)
            {
                NormalFilteredNeighborFaces.push_back(f);
            }
        }

        // If there are faces fit the angle limit, return the NormalFilteredNeighborFaces.
        // otherwise return neighbor list.
        std::vector<UINT>* FilteredFaces = NormalFilteredNeighborFaces.size() > 0 ? &NormalFilteredNeighborFaces : &NeighborFaces;

        if (FilteredFaces->size() > 0)
            return *FilteredFaces;
        else
        {
            /// Can't find neighbor faces to start, try faces in the whole space.
            // Find boundary triangles
            std::vector<UINT> BoundaryFaceGroup[4];
            for (UINT i = 0; i < 4; i++)
                BoundaryFaceGroup->reserve(Faces.size());
            for (UINT f = 0; f < Faces.size(); f++)
            {
                if (FaceMasks[f].GetUsed() || UsedFaceFlags[f].GetUsed())
                    continue;

                FFace& Face = Faces[f];

                UINT NeighborFaceCount = 0;
                for (UINT e = 0; e < 3; e++)
                {
                    UINT NeighborFace = Face.Edges[e]->NeighborFace;
                    if (NeighborFace != -1)
                    {
                        if (!FaceMasks[NeighborFace].GetUsed() && !UsedFaceFlags[NeighborFace].GetUsed())
                            NeighborFaceCount++;
                    }
                }
                BoundaryFaceGroup[NeighborFaceCount].push_back(f);
            }

            std::vector<UINT>* BoundaryFaces = nullptr;
            UINT SelectMinBoundaryFaceGroupId = BoundaryFaceGroup[0].size() > 0 ? 0 :
                (BoundaryFaceGroup[1].size() > 0 ? 1 :
                    (BoundaryFaceGroup[2].size() > 0 ? 2 :
                        (BoundaryFaceGroup[3].size() > 0 ? 3 : UINT_MAX)));

            if (SelectMinBoundaryFaceGroupId != UINT_MAX)
            {
                std::vector<UINT> ResultFaces(1);

                BoundaryFaces = &BoundaryFaceGroup[SelectMinBoundaryFaceGroupId];
                float MaxCosAngle = -1;
                for (auto f : *BoundaryFaces)
                {
                    const FFace& face = Faces[f];
                    float CosAngle = RunningFullStripState.AveNormal.Dot(face.Normal);
                    if (CosAngle > MaxCosAngle)
                    {
                        MaxCosAngle = CosAngle;
                        ResultFaces[0] = f;
                    }
                }
                if (MaxCosAngle != -1)
                    return ResultFaces;
            }
            return std::vector<UINT>();
        }
    }
#else
    // Return the candidate face if:
    // a. In the neighbor face list, all faces that are within the angle threshold.
    // b. If no face in a, group all faces with boundary faces count, select face with least angle in each boundary face group.
    std::vector<UINT> FCluster::FindConnectedStripCandidateFaces(const FStripifyResult& RunningFullStripState)
    {
        // TODO: optimize it. Cache 0 value face flag.
        // Find all neighbor faces for current running full strip.
        std::vector<FUseFlag> UsedFaceFlags(Faces.size(), FUseFlag());
        std::vector<FUseFlag> CheckedFaceFlags(Faces.size(), FUseFlag());
        std::vector<UINT> NeighborFaces;
        for (UINT f : RunningFullStripState.ExperimentStripFaces)
        {
            UsedFaceFlags[f].SetUsed();
        }
        for (UINT f : RunningFullStripState.ExperimentStripFaces)
        {
            const FFace& face = Faces[f];
            for (UINT e = 0; e < 3; e++)
            {
                UINT NeighborFace = face.Edges[e]->NeighborFace;
                if (NeighborFace != UINT_MAX)
                {
                    if (!FaceMasks[NeighborFace].GetUsed() && !UsedFaceFlags[NeighborFace].GetUsed() && !CheckedFaceFlags[NeighborFace].GetUsed())
                    {
                        NeighborFaces.push_back(NeighborFace);
                        CheckedFaceFlags[NeighborFace].SetUsed();
                    }
                }
            }
        }
        // Select neighbor faces that fit the normal angle limit.
        std::vector<UINT> NormalFilteredNeighborFaces;
        for (UINT f : NeighborFaces)
        {
            const FFace& face = Faces[f];
            if (RunningFullStripState.AveNormal.Dot(face.Normal) > kCosNormalAngleThreshold)
            {
                NormalFilteredNeighborFaces.push_back(f);
            }
        }

        // If there are faces fit the angle limit, return the NormalFilteredNeighborFaces.
        if (NormalFilteredNeighborFaces.size() > 0)
            return NormalFilteredNeighborFaces;
        else
        {
            /// Can't find neighbor faces to start, try faces in the whole space.
            // Find boundary triangles
            std::vector<UINT> BoundaryFaceGroup[4];
            for (UINT i = 0; i < 4; i++)
                BoundaryFaceGroup->reserve(Faces.size());
            for (UINT f = 0; f < Faces.size(); f++)
            {
                if (FaceMasks[f].GetUsed() || UsedFaceFlags[f].GetUsed())
                    continue;

                FFace& Face = Faces[f];

                UINT NeighborFaceCount = 0;
                for (UINT e = 0; e < 3; e++)
                {
                    UINT NeighborFace = Face.Edges[e]->NeighborFace;
                    if (NeighborFace != -1)
                    {
                        if (!FaceMasks[NeighborFace].GetUsed() && !UsedFaceFlags[NeighborFace].GetUsed())
                            NeighborFaceCount++;
                    }
                }
                BoundaryFaceGroup[NeighborFaceCount].push_back(f);
            }

            // Find face with least angle difference in each boundary face group.
            std::vector<UINT>* BoundaryFaces = nullptr;
            float MaxCosAngle = -1;
            std::vector<UINT> ResultFaces(1);
            for (UINT i = 0; i <= 3; i++)
            {
                BoundaryFaces = &BoundaryFaceGroup[i];

                for (auto f : *BoundaryFaces)
                {
                    const FFace& face = Faces[f];
                    float CosAngle = RunningFullStripState.AveNormal.Dot(face.Normal);
                    if (CosAngle > MaxCosAngle)
                    {
                        MaxCosAngle = CosAngle;
                        ResultFaces[0] = f;
                    }
                }
                if (MaxCosAngle > kCosNormalAngleThreshold)
                    return ResultFaces;
            }
            if (MaxCosAngle != -1)
                return ResultFaces;

            return std::vector<UINT>();
        }
    }
#endif

    /// <summary>
    /// Param.TriangleMatchStripDirection means whether the next triangle to be built is CCW if we take the 3 indices in strip in order.
    /// The Param.StartEdge means the shared edge between the previous triangle and the next triangle to be built.(Next triangle is actually the start triangle for empty strip.
    /// </summary>
    /// <param name="Param"></param>
    /// <param name="Result"></param>
    /// <param name="RunningFullStripifyState"></param>
    bool FCluster::BuildConnectedStripExperiment(const FStripifyParam& Param, FStripifyResult& Result, FStripifyResult& RunningFullStripifyState)
    {
        const bool FirstConnectedStrip = RunningFullStripifyState.ExperimentStrip.size() == 0;

        const bool RunningFullStrip_TriangleMatchStripDirection = (RunningFullStripifyState.ExperimentStrip.size() % 2 == 0);

        /// TriangleMatchStripDirection means whether the next triangle to be built is CCW in the strip if we take the 3 indices in order.
        // Build forward strip list.
        // Try to build the strip with the reverse order as it only need one nan vertex to connect 2 strips with reverse order.
        bool ForwardStripTriangleMatchStripDirection = Param.TriangleMatchStripDirection;

        bool TriangleMatchStripDirection = ForwardStripTriangleMatchStripDirection;

        int RemainFullStripSpace = kStripSize - (int)RunningFullStripifyState.ExperimentStrip.size();// FirstConnectedStrip ? kStripSize - (int)RunningFullStripifyState.ExperimentStrip.size() - 1 : kStripSize - (int)RunningFullStripifyState.ExperimentStrip.size() - 3;
        if (!FirstConnectedStrip)
        {
            RemainFullStripSpace -= 2;
        }

        // Remain space need to be enough to build only 1 triangle.
        if (RemainFullStripSpace < 3)
            return false;

        UINT StartV = Param.StartEdge->StartVertex;
        UINT EndV = Param.StartEdge->EndVertex;
        UINT V1 = EndV;

        const FHalfEdge* StartEdge = Param.StartEdge;

        std::vector<UINT>& StripFaces = Result.ExperimentStripFaces;
        std::vector<UINT>& StripIndices = Result.ExperimentStrip;

        // Reserve kStripSize, it is chosen heuristically, could ends up larger than reserved.
        StripFaces.reserve(kStripSize);
        StripIndices.reserve(kStripSize);

        // Push the first 2 vertices into the list.
        if (TriangleMatchStripDirection)
        {
            StripIndices.push_back(StartV);
            StripIndices.push_back(V1);
        }
        else
        {
            StripIndices.push_back(V1);
            StripIndices.push_back(StartV);
        }


        {
            const FHalfEdge* IterateEdge = StartEdge;
            // Don't check normal angle for first run because it was checked in FindConnectedStripCandidateFaces. 
            bool ContinueLoop = IterateEdge != nullptr && !FaceMasks[IterateEdge->Face].GetUsed() && !RunningFullStripifyState.FaceMasks[IterateEdge->Face].GetUsed();
            while (ContinueLoop)
            {
                const FHalfEdge* NextEdge = GetNextEdgeInFace(IterateEdge);

                if (StripFaces.size() == 0)
                {
                    Result.AveNormal = Faces[IterateEdge->Face].Normal;
                    Result.AccumNormal = Result.AveNormal;
                }
                StripIndices.push_back(NextEdge->EndVertex);
                StripFaces.push_back(IterateEdge->Face);
                RunningFullStripifyState.FaceMasks[IterateEdge->Face].SetUsed();

                TriangleMatchStripDirection = !TriangleMatchStripDirection;

                if ((int)StripIndices.size() == RemainFullStripSpace)
                {
                    break;
                }

                // Use next of next edge to find dual edge if TriangleMatchStripDirection
                if (TriangleMatchStripDirection)
                    NextEdge = GetNextEdgeInFace(NextEdge);//FindEdgeForFace(HalfEdges[NextEdge->EndVertex], NextEdge->Face);

                const FHalfEdge* OldEdge = NextEdge;

                IterateEdge = NextEdge->GetReverseEdge();

                ContinueLoop = false;
                if (IterateEdge != nullptr && !FaceMasks[IterateEdge->Face].GetUsed() && !RunningFullStripifyState.FaceMasks[IterateEdge->Face].GetUsed())
                {
                    if (CheckAndUpdateFaceNormal(Result, Faces[IterateEdge->Face]))
                        ContinueLoop = true;
                }
            }
        }

        // Now TriangleMatchStripDirection means whether the next triangle in backward strip is CCW if we take 3 sequential vertices in the strip.
        TriangleMatchStripDirection = !ForwardStripTriangleMatchStripDirection;

        RemainFullStripSpace -= (int)StripIndices.size();
        // Reserve additional vertex if we add backward strip, in case the additional backward strip size is odd, which change the direction of strip, in this case we need one more duplicated vertex.
        //if (!FirstConnectedStrip || Param.TriangleMatchStripDirection)
        RemainFullStripSpace -= 1;

        if (RemainFullStripSpace > 0)
        {
            // Build backward strip list
            std::vector<UINT> BackwardStripIndices;
            std::vector<UINT> BackwordFaces;
            BackwordFaces.reserve(kStripSize);
            BackwardStripIndices.reserve(kStripSize);

            {
                const FHalfEdge* ReverseEdge = StartEdge->GetReverseEdge();

                while (true)
                {
                    bool ContinueLoop = ReverseEdge != nullptr && !FaceMasks[ReverseEdge->Face].GetUsed() && !RunningFullStripifyState.FaceMasks[ReverseEdge->Face].GetUsed();
                    if (ContinueLoop)
                        ContinueLoop &= CheckAndUpdateFaceNormal(Result, Faces[ReverseEdge->Face]);
                    if (!ContinueLoop)
                        break;

                    const FHalfEdge* NextEdge = GetNextEdgeInFace(ReverseEdge);
                    BackwardStripIndices.push_back(NextEdge->EndVertex);
                    BackwordFaces.push_back(ReverseEdge->Face);
                    RunningFullStripifyState.FaceMasks[ReverseEdge->Face].SetUsed();

                    TriangleMatchStripDirection = !TriangleMatchStripDirection;

                    if ((int)BackwardStripIndices.size() == RemainFullStripSpace)
                    {
                        break;
                    }

                    if (!TriangleMatchStripDirection)
                        NextEdge = GetNextEdgeInFace(NextEdge);

                    const FHalfEdge* OldEdge = NextEdge;
                    ReverseEdge = NextEdge->GetReverseEdge();

                }
            }

            // Reverse BackwardStripIndices
            for (UINT i = 0; i < BackwardStripIndices.size() / 2; i++)
            {
                auto t = BackwardStripIndices[i];
                BackwardStripIndices[i] = BackwardStripIndices[BackwardStripIndices.size() - 1 - i];
                BackwardStripIndices[BackwardStripIndices.size() - 1 - i] = t;

                auto tf = BackwordFaces[i];
                BackwordFaces[i] = BackwordFaces[BackwordFaces.size() - 1 - i];
                BackwordFaces[BackwordFaces.size() - 1 - i] = tf;
            }

            // Merge forward and backward strip list.
            if (BackwardStripIndices.size() > 0)
            {
                StripIndices.insert(StripIndices.begin(), BackwardStripIndices.begin(), BackwardStripIndices.end());
                StripFaces.insert(StripFaces.begin(), BackwordFaces.begin(), BackwordFaces.end());
            }
        }

        std::vector<UINT> AdditionalVertices;
        if (TriangleMatchStripDirection == RunningFullStrip_TriangleMatchStripDirection)
        {
#ifndef DEGENERATE_TRIANGLE_INVALID_INDEX
            AdditionalVertices.push_back(StripIndices[0]);
#else
            AdditionalVertices.push_back(UINT_MAX);
#endif
            Result.DuplicatedVertices++;
        }

        if (!FirstConnectedStrip)
        {
            if (RunningFullStripifyState.ExperimentStrip.back() != StripIndices[0])
            {
#ifndef DEGENERATE_TRIANGLE_INVALID_INDEX
                AdditionalVertices.insert(AdditionalVertices.begin(), RunningFullStripifyState.ExperimentStrip.back());
                AdditionalVertices.push_back(StripIndices[0]);
#else
                if (AdditionalVertices.size() == 0)
                {
                    AdditionalVertices.push_back(UINT_MAX);
                    AdditionalVertices.push_back(UINT_MAX);
                    Result.DuplicatedVertices += 2;
                }
#endif
            }
        }

        if (AdditionalVertices.size())
        {
            StripIndices.insert(StripIndices.begin(), AdditionalVertices.begin(), AdditionalVertices.end());
        }

        // Clear local used FaceMasks as it is only the experiment run.
        for (auto f : StripFaces)
        {
            RunningFullStripifyState.FaceMasks[f].ResetUsed();
        }
        return true;
    }

    /// <summary>
    /// First Select one experiment who has least angle normal angle difference with Current running full strip.
    /// Then find the experiment who shares the most faces + longest strip with running full strip.
    /// </summary>
    /// <param name="ExperimentResults"></param>
    /// <param name="CurrentFullStripResult"></param>
    /// <returns></returns>
    std::shared_ptr<FStripifyResult> FCluster::SelectConnectedStripExperiments(const std::vector<std::shared_ptr<FStripifyResult>>& ExperimentResults, const FStripifyResult& CurrentFullStripResult)
    {
        std::vector<std::shared_ptr<FStripifyResult>> ValideExperiments;
        std::vector<std::shared_ptr<FStripifyResult>> FilteredNormalAngleResult;
        const std::vector<std::shared_ptr<FStripifyResult>>* FilteredResult;

        std::vector<FUseFlag> ConnectedStripFaceMasks(Faces.size(), FUseFlag());

        for (const std::shared_ptr<FStripifyResult>& Experiment : ExperimentResults)
        {
            if (Experiment->ExperimentStrip.size() > 0)
            {
                float dotNormal = CurrentFullStripResult.AveNormal.Dot(Experiment->AveNormal);
                // Prevent numeric error which will cause AngleWithFullStrip to be nan.
                dotNormal = std::min(1.f, dotNormal);
                dotNormal = std::max(-1.f, dotNormal);
                Experiment->AngleWithFullStrip = std::acos(dotNormal);

                if (dotNormal > kCosNormalAngleThreshold)
                {
                    FilteredNormalAngleResult.push_back(Experiment);
                }
                ValideExperiments.push_back(Experiment);
            }
        }

        if (ValideExperiments.size() == 0)
            return nullptr;

        FilteredResult = FilteredNormalAngleResult.size() > 0 ? &FilteredNormalAngleResult : &ValideExperiments;

        float MaxNeighborFaceCountPlusStripLength = std::numeric_limits<float>::lowest();
        std::shared_ptr<FStripifyResult> MaxNeighborFaceCountExperiment = nullptr;

        for (const std::shared_ptr<FStripifyResult>& Experiment : *FilteredResult)
        {
            UINT NeighborFaceCount = 0;
            for (UINT f : Experiment->ExperimentStripFaces)
            {
                const FFace& Face = Faces[f];
                for (UINT e = 0; e < 3; e++)
                {
                    const FHalfEdge* Edge = Face.Edges[e];
                    if (Edge->NeighborFace != UINT_MAX)
                    {
                        if (CurrentFullStripResult.FaceMasks[Edge->NeighborFace].GetUsed())
                        {
                            NeighborFaceCount++;
                        }
                    }
                }
            }
            Experiment->SharedFaceCount = NeighborFaceCount;

            // Use NeighborFaceCount plus Strip length to select the candidate strip.
            float NeighborFaceCountPlusStripLength = (float)NeighborFaceCount + (float)Experiment->ExperimentStripFaces.size() - (float)Experiment->DuplicatedVertices;
            // Scale weight of angle by 8.
            NeighborFaceCountPlusStripLength -= Experiment->AngleWithFullStrip * 180 / V_PI * kSelectConnectedStripExperimentAngleWeight;

            if (NeighborFaceCountPlusStripLength >= MaxNeighborFaceCountPlusStripLength)
            {
                MaxNeighborFaceCountPlusStripLength = NeighborFaceCountPlusStripLength;
                MaxNeighborFaceCountExperiment = Experiment;
            }
        }

        return MaxNeighborFaceCountExperiment;
    }

    /// <summary>
    /// The goal of building Full strip is to
    /// a. make sure it share most vertices with last full strip,
    /// b. has least duplicated vertices.
    /// We try to achieve a. in FindFullStripCandidateFaces
    /// First we Select Least Boundary Triangles.
    /// Then we also add a few triangles from the start and end triangle of last full strip.
    /// </summary>
    std::vector<UINT> FCluster::FindFullStripCandidateFaces(const std::shared_ptr<FStripifyResult> LastStripResult, UINT depth)
    {
        // Find boundary triangles
        std::vector<UINT> BoundaryFaceGroup[4];
        for (UINT i = 0; i < 4; i++)
            BoundaryFaceGroup->reserve(Faces.size());
        for (UINT f = 0; f < Faces.size(); f++)
        {

            if (FaceMasks[f].GetUsed())
                continue;

            FFace& Face = Faces[f];

            UINT NeighborFaceCount = 0;
            for (UINT e = 0; e < 3; e++)
            {
                UINT NeighborFace = Face.Edges[e]->NeighborFace;
                if (NeighborFace != -1)
                {
                    if (!FaceMasks[NeighborFace].GetUsed())
                        NeighborFaceCount++;
                }
            }
            BoundaryFaceGroup[NeighborFaceCount].push_back(f);
        }

        std::vector<UINT>* BoundaryFaces = nullptr;
        UINT SelectMinBoundaryFaceGroupId = BoundaryFaceGroup[0].size() > 0 ? 0 :
            (BoundaryFaceGroup[1].size() > 0 ? 1 :
                (BoundaryFaceGroup[2].size() > 0 ? 2 :
                    (BoundaryFaceGroup[3].size() > 0 ? 3 : UINT_MAX)));

        if (SelectMinBoundaryFaceGroupId != UINT_MAX)
            BoundaryFaces = &BoundaryFaceGroup[SelectMinBoundaryFaceGroupId];

        // Built for all faces. we finished.
        if (BoundaryFaces == nullptr)
            return std::vector<UINT>();

        return *BoundaryFaces;

    }
    /// <summary>
    /// Build single Full Strip as experiment. Done.
    /// </summary>
    /// <param name="Param"></param>
    /// <param name="Result"></param> Result is Output only.
    void FCluster::BuildFullStripExperiment(const FStripifyParam& Param, FStripifyResult& Result)
    {
        FStripifyResult& RunningFullStripState = Result;

        // Initialize FaceMasks in FStrififyResult
        RunningFullStripState.FaceMasks.resize(Faces.size(), FUseFlag());

        {
            FStripifyResult ConnectedStripResult;
            BuildConnectedStripExperiment(Param, ConnectedStripResult, RunningFullStripState);

            CommitToFullStripExperiment(RunningFullStripState, ConnectedStripResult);
        }

        UINT IterationCount = 0;
        while ((UINT)RunningFullStripState.ExperimentStrip.size() < (UINT)kStripSize - 3)
        {
            /// Find boundary Face.
            std::vector<UINT> CandidateFaces = FindConnectedStripCandidateFaces(RunningFullStripState);
            // No more free faces left. Stripification is finished.
            if (CandidateFaces.size() == 0)
                break;

            ///  Build multiple Connected Strip Experiments
            std::vector<std::shared_ptr<FStripifyResult>>  StripfyExperiments(CandidateFaces.size() * 3);
            std::vector<FStripifyParam>  StripfyParams(CandidateFaces.size() * 3);

            UINT ExperimentId = 0;
            for (auto CandidateFace : CandidateFaces)
            {
                for (UINT e = 0; e < 3; e++)
                {
                    auto& StripifyResult = StripfyExperiments[ExperimentId];

                    FStripifyParam& StripfyParam = StripfyParams[ExperimentId];
                    StripfyParam.StartFace = CandidateFace;
                    StripfyParam.StartEdge = Faces[CandidateFace].Edges[e];

                    StripfyParam.TriangleMatchStripDirection = (RunningFullStripState.ExperimentStrip.size() % 2 != 0);

                    StripifyResult = std::make_shared<FStripifyResult>();
                    StripifyResult->ExperimentIndex = ExperimentId;

                    BuildConnectedStripExperiment(StripfyParam, *StripifyResult, RunningFullStripState);

                    ExperimentId++;
                }
            }

            // Need to save local connectedstrip statistics into SelectedExperimentIds, which will be used in SelectFullStripExperiments later.
            std::shared_ptr<FStripifyResult> SelectedExperiment = SelectConnectedStripExperiments(StripfyExperiments, RunningFullStripState);

            // Finished this full strip.
            if (SelectedExperiment == nullptr)
                break;

            CommitToFullStripExperiment(RunningFullStripState, *SelectedExperiment);
        }
    }

    // Sort FullStrip using weighting function, which is the combined weighted sum of SharedFaceCount and ValidFaces.
    std::shared_ptr<FStripifyResult> FCluster::SelectFullStripExperiments(std::vector<std::shared_ptr<FStripifyResult>>& ExperimentResults, const std::shared_ptr<FStripifyResult> LastStripResult, UINT depth)
    {
        auto CalculateFullStripWeight = [this](const FStripifyResult& r) -> float {
            return (float)r.SharedFaceCount + (float)r.ExperimentStripFaces.size() * kSelectFullStripExperimentFaceCountWeight;
        };
        std::sort(ExperimentResults.begin(), ExperimentResults.end(), [&](const std::shared_ptr<FStripifyResult>& r1, const std::shared_ptr<FStripifyResult>& r2) {
            float weight1 = CalculateFullStripWeight(*r1);
        float weight2 = CalculateFullStripWeight(*r2);
        return weight1 > weight2;
            });
        return ExperimentResults[0];
    }

    void FCluster::CommitToFullStripExperiment(FStripifyResult& RunningFullStripState, const FStripifyResult& Experiment)
    {
        RunningFullStripState.ExperimentStrip.insert(RunningFullStripState.ExperimentStrip.end(), Experiment.ExperimentStrip.begin(), Experiment.ExperimentStrip.end());
        RunningFullStripState.ExperimentStripFaces.insert(RunningFullStripState.ExperimentStripFaces.end(), Experiment.ExperimentStripFaces.begin(), Experiment.ExperimentStripFaces.end());

        RunningFullStripState.AccumNormal += Experiment.AccumNormal;
        RunningFullStripState.AveNormal = RunningFullStripState.AccumNormal.Normalize();

        RunningFullStripState.ConnectedStripCount++;
        RunningFullStripState.SharedFaceCount += Experiment.SharedFaceCount;
        RunningFullStripState.DuplicatedVertices += Experiment.DuplicatedVertices;

        for (UINT f : Experiment.ExperimentStripFaces)
        {
            RunningFullStripState.FaceMasks[f].SetUsed();
        }
    }

    void FCluster::BuildStripsImp()
    {
        std::vector<std::thread> threads;

        std::shared_ptr<FStripifyResult> LastStripResult;
        UINT IterationCount = 0;
        while (true)
        {
            /// Find boundary Face.
            std::vector<UINT> FindedCandidateFaces = FindFullStripCandidateFaces(LastStripResult, IterationCount);
            std::vector<UINT> CandidateFaces;

            if ((UINT)FindedCandidateFaces.size() > kMaxStartFaces)
            {
                UINT div = (UINT)FindedCandidateFaces.size() / kMaxStartFaces;
                for (UINT i = 0; i < kMaxStartFaces; i++)
                {
                    CandidateFaces.push_back(FindedCandidateFaces[i * div]);
                }
            }
            else
                CandidateFaces = FindedCandidateFaces;

            // Built all faces into strips.
            if (CandidateFaces.size() == 0)
            {
                AllResults.push_back(RunningStrips);
                break;
            }

            ///  Build OneStrip Experiments
            std::vector<std::shared_ptr<FStripifyResult>>  StripfyExperiments(CandidateFaces.size() * 3);
            std::vector<FStripifyParam>  StripfyParams(CandidateFaces.size() * 3);

            std::vector<UINT> ExperimentIndices;
            UINT ExperimentId = 0;
            for (auto CandidateFace : CandidateFaces)
            {
                for (UINT e = 0; e < 3; e++)
                {
                    auto& StripifyResult = StripfyExperiments[ExperimentId];

                    FStripifyParam& StripfyParam = StripfyParams[ExperimentId];
                    StripfyParam.StartFace = CandidateFace;
                    StripfyParam.StartEdge = Faces[CandidateFace].Edges[e];
                    StripfyParam.TriangleMatchStripDirection = true;

                    StripifyResult = std::make_shared<FStripifyResult>();
                    StripifyResult->ExperimentIndex = ExperimentId;

                    ExperimentIndices.push_back(ExperimentId);
                    ExperimentId++;
                }
            }

#ifndef PARALLEL_EXECUTE
            for (UINT index : ExperimentIndices)
            {
                BuildFullStripExperiment(StripfyParams[index], *StripfyExperiments[index]);
            }
#else

#ifdef CPP_17
            std::for_each(std::execution::par, ExperimentIndices.begin(), ExperimentIndices.end(), [&](auto index) {
                BuildFullStripExperiment(StripfyParams[index], *StripfyExperiments[index]);
                });
#else
            threads.resize(ExperimentIndices.size());
            for (UINT index : ExperimentIndices)
            {
                threads[index] = std::thread(
                    [&, index]() {
                        BuildFullStripExperiment(StripfyParams[index], *StripfyExperiments[index]);
                    }
                );
            }
            std::for_each(threads.begin(), threads.end(), [](std::thread& x) {x.join(); });
#endif
#endif

            {
                /// Select Experiment.
                std::shared_ptr<FStripifyResult> SelectedExperiment = SelectFullStripExperiments(StripfyExperiments, LastStripResult, IterationCount);

                // Commit the strip experiment to running result.
                // This FaceMasks should be local to the full strip experiment.!!!!
                for (auto f : SelectedExperiment->ExperimentStripFaces)
                {
                    FaceMasks[f].SetUsed();
                }
                // Recover it later. For now be sure we are not missing anything.
                RunningStrips.StripLists.push_back(SelectedExperiment->ExperimentStrip);

                LastStripResult = SelectedExperiment;
            }
            IterationCount++;
        }
    }
    const FStripList& FCluster::BuildStrips()
    {
        BuildStripsImp();

        std::vector<Float3> ResultsAreaTemp;
        ResultsAreaTemp.resize(AllResults.size());
        UINT ResultsAreaindex = 0;
        for (FStripList& StripListsData : AllResults)
        {
            Float3 AA;
            Float3 BB;
            StripListsData.VertexCount = 0;
            for (auto& Strip : StripListsData.StripLists)
            {
                StripListsData.VertexCount += (UINT)Strip.size();
                // Check to make sure there are maximum 2 duplicated vertex (invalid vertices)
                int sameIndexCount = 1;

                for (UINT i = 1; i < (UINT)Strip.size(); i++)
                {
                    if (Strip[i] == Strip[i - 1])
                    {
                        sameIndexCount++;
                    }
                    else
                    {
                        sameIndexCount = 1;
                    }
                    
                    ASSERT(sameIndexCount <= 2);
                }
            }
        }
        //// sort with least number of strips, then sort with least number of vertices.
        std::sort(AllResults.begin(), AllResults.end(), [](const auto& a, const auto& b) {
            if (a.StripLists.size() < b.StripLists.size())
            return true;
            else if (a.StripLists.size() == b.StripLists.size())
            {
                return a.VertexCount < b.VertexCount;
            }
            else
            {
                return false;
            }
            });


        return AllResults[0];//ResultsAreaTemp[0].y
    }

}
