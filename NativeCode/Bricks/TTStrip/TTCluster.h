#pragma once
#include "TTStrip.h"

namespace TTStrip
{
	struct FClusterFace
	{
		FClusterFace(UINT _GlobalFaceId) : GlobalFaceId(_GlobalFaceId)
		{
		}
		UINT GlobalFaceId = UINT_MAX;
		FUseFlag UseFlag;
	};

	struct FStripifyParam
	{
		UINT StartFace;
		FHalfEdge* StartEdge;
		bool TriangleMatchStripDirection = true;
	};

	struct FStripList
	{
		std::vector<std::vector<UINT>> StripLists;
		UINT VertexCount = 0;
	};

	struct FStripifyResult
	{
		// Used by both FullStrip and ConnectedStrip
		std::vector<UINT> ExperimentStrip;

		// Used by fullstrip
		UINT ConnectedStripCount = 0;

		// Used by fullstrip & connectedstrip
		UINT SharedFaceCount = 0;

		UINT DuplicatedVertices = 0;

		// Used by both FullStrip and ConnectedStrip
		std::vector<UINT> ExperimentStripFaces;

		// FaceMasks for Full strip only.
		std::vector<FUseFlag> FaceMasks;

		// Normals are for ConnectedStrip only.
		Double3 AccumNormal;
		Float3 AveNormal;

		float AngleWithFullStrip = V_PI;

		UINT ExperimentIndex;
	};

	class FCluster
	{
	public:
		FCluster(const std::vector<FFace>& _AllFaces, UINT EdgeCount, int StripSize, double NormalAngleThreshold,
			float SelectFullStripExperimentFaceCountWeight,
			float SelectConnectedStripExperimentAngleWeight,
			UINT MaxStartFaces)
			:kStripSize(StripSize),
			kNormalAngleThreshold(NormalAngleThreshold),
			kCosNormalAngleThreshold(cos(NormalAngleThreshold)),
			AllFaces(_AllFaces),
			kMaxStartFaces(MaxStartFaces),
			kSelectFullStripExperimentFaceCountWeight(SelectFullStripExperimentFaceCountWeight),
			kSelectConnectedStripExperimentAngleWeight(SelectConnectedStripExperimentAngleWeight),
			HalfEdges(EdgeCount),
			FaceMasks(_AllFaces.size(), FUseFlag())
		{

		}

		~FCluster();

		const FStripList& BuildStrips();

		void AddFace(const FFace& face);
		void SetNormal(const Float3 n)
		{
			Normal = n;
		}
		std::vector<FFace>& GetFaces()
		{
			return Faces;
		}

	private:
		void BuildStripsImp();

		std::vector<UINT> FindFullStripCandidateFaces(const std::shared_ptr<FStripifyResult> LastStripResult, UINT depth);
		std::vector<UINT> FindConnectedStripCandidateFaces(const FStripifyResult& FullStripResult);

		std::shared_ptr<FStripifyResult> SelectFullStripExperiments(std::vector<std::shared_ptr<FStripifyResult>>& ExperimentResult, const std::shared_ptr<FStripifyResult> LastStripResult, UINT depth);
		std::shared_ptr<FStripifyResult> SelectConnectedStripExperiments(const std::vector<std::shared_ptr<FStripifyResult>>& ExperimentResult, const FStripifyResult& CurrentFullStripResult);

		void BuildFullStripExperiment(const FStripifyParam& Param, FStripifyResult& Result);
		bool BuildConnectedStripExperiment(const FStripifyParam& Param, FStripifyResult& Result, FStripifyResult& RunningFullStripifyState);

		void CommitToFullStripExperiment(FStripifyResult& RunningFullStripState, const FStripifyResult& Experiment);

		UINT FindNeighborCount(const FFace& face) const;

		bool CheckAndUpdateFaceNormal(FStripifyResult& result, const FFace& f2);

		const int kStripSize;
		const double kNormalAngleThreshold;
		const double kCosNormalAngleThreshold;
		const std::vector<FFace>& AllFaces;
		const UINT kMaxStartFaces = 16;
		// Take Face Count as a weight when selecting FullStrip from experiments.
		const float kSelectFullStripExperimentFaceCountWeight = 1.2f;
		// Take Angle between current strip normal and full strip normal as a weight when selecting connected strip experiment.
		const float kSelectConnectedStripExperimentAngleWeight = 1.f / 8;

		std::vector<FFace> Faces;
		std::vector<FHalfEdge*> HalfEdges;
		Float3 Normal;

		// Running result of stripification.
		std::vector<FUseFlag> FaceMasks;
		FStripList RunningStrips;
		std::vector<FStripList> AllResults;

		bool EvaluateFaceNormal = true;
	};
}
