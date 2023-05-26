#pragma once
#include "TTStrip.h"
#include "TTCluster.h"

namespace TTStrip
{
	class FStripifier
	{
	public:
		FStripifier(int stripSize, float PartitionAngleThreshold = 30, float StripAngleThreshold = 30,
			float SelectFullStripExperimentFaceCountWeight = 1.2f,
			float SelectConnectedStripExperimentAngleWeight = 1.f / 8,
			UINT MaxStartFaces = 8, bool _FillDummyVerticesToStrip = false, bool _MergeFinalStrips = false)
			: kStripSize(stripSize),
			kNormalAngleThreshold(PartitionAngleThreshold / 180 * kPi),
			kStripNormalAngleThreshold(StripAngleThreshold / 180 * kPi),
			kSelectFullStripExperimentFaceCountWeight(SelectFullStripExperimentFaceCountWeight),
			kSelectConnectedStripExperimentAngleWeight(SelectConnectedStripExperimentAngleWeight),
			FillDummyVerticesToStrip(_FillDummyVerticesToStrip),
			MergeFinalStrips(_MergeFinalStrips),
			kMaxStartFaces(MaxStartFaces)
		{
		}
		~FStripifier();

		std::vector<std::vector<UINT>>& Stripify(const std::vector<Float3>& Vertices, const std::vector<UINT>& Indices);

		std::vector<std::vector<UINT>>& GetStripifiedStripLists() {
			return StripifiedStripLists;
		}

		const std::vector<UINT>& GetTriangleStripIds() { return TriangleStripIds; }
	private:
		void Preprocess(const std::vector<Float3>& Vertices, const std::vector<UINT>& Indices);
		void BuildSingleCluster();
		void BuildStrips();
		void PostProcess();
		void BuildTriangleStripId();

		std::vector<UINT> FindNeighbors(UINT FaceId, const std::vector<FUseFlag>& UseFlags) const;

		const int kStripSize;
		const UINT kPartitionSeedSelectGroupSize = 16;
		const double kPi = 3.14159265358979323846;
		const double kNormalAngleThreshold;// = 30.0 / 180 * kPi;
		const double kStripNormalAngleThreshold;
		const float kSelectFullStripExperimentFaceCountWeight;
		const float kSelectConnectedStripExperimentAngleWeight;
		const bool FillDummyVerticesToStrip = false;
		const bool MergeFinalStrips;
		const UINT kMaxStartFaces;

		std::vector<FFace> Faces;
		std::vector<FHalfEdge*> HalfEdges;
		// Mark the face is used.
		std::vector<FUseFlag> FaceUsedFlags;

		std::vector<std::unique_ptr<FCluster>> Clusters;
		UINT PartitionedFaces = 0;

		std::vector<std::vector<UINT>> StripifiedStripLists;
		std::vector<std::vector<UINT>> PostProcessedStripLists;

		std::vector<UINT> TriangleStripIds;
	};
}
