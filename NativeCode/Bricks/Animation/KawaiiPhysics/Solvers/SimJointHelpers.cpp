#include "SimJointHelpers.h"

NS_BEGIN

namespace KawaiiPhysics
{
namespace SimJointHelpers
{
	void BuildChainParticles(
		const std::vector<v3dxVector3>& BonePositions,
		const std::vector<v3dxQuaternion>& BoneRotations,
		const std::vector<v3dxVector3>& BoneScales,
		const std::vector<int32_t>& ParentIndices,
		int32_t RootBoneIndex,
		int32_t EndBoneIndex,
		ETailBoneAxis TailBoneForwardAxis,
		float TailBoneLength,
		std::vector<FSimParticle>& OutParticles)
	{
		OutParticles.clear();

		// Trace from root to end collecting bone indices
		std::vector<int32_t> chainBones;
		if (EndBoneIndex >= 0 && EndBoneIndex != RootBoneIndex)
		{
			// Build path from end to root, then reverse
			int32_t current = EndBoneIndex;
			while (current >= 0)
			{
				chainBones.push_back(current);
				if (current == RootBoneIndex) break;
				current = (current < (int32_t)ParentIndices.size()) ? ParentIndices[current] : -1;
			}
			std::reverse(chainBones.begin(), chainBones.end());
		}
		else
		{
			// Single bone chain
			chainBones.push_back(RootBoneIndex);
		}

		// Create particles
		for (size_t i = 0; i < chainBones.size(); ++i)
		{
			int32_t boneIdx = chainBones[i];
			if (boneIdx < 0 || boneIdx >= (int32_t)BonePositions.size()) continue;

			FSimParticle particle;
			particle.BoneIndex = boneIdx;
			particle.PinMode = (i == 0) ? KPM_Kinematic : KPM_Dynamic;
			particle.Initialize(
				BonePositions[boneIdx], BoneRotations[boneIdx],
				BonePositions[boneIdx], BoneRotations[boneIdx],
				BoneScales[boneIdx]);

			OutParticles.push_back(particle);
		}

		// Add tail dummy
		if (!OutParticles.empty() && TailBoneLength > 0.0f)
		{
			FSimParticle dummy;
			dummy.PinMode = KPM_Dynamic;
			dummy.InitializeDummy(TailBoneForwardAxis, TailBoneLength, OutParticles.back());
			OutParticles.push_back(dummy);
		}

		ComputeChainLengths(OutParticles);
	}

	void ComputeChainLengths(std::vector<FSimParticle>& Particles)
	{
		if (Particles.empty()) return;

		Particles[0].LengthFromRoot = 0.0f;
		float totalLength = 0.0f;

		for (size_t i = 1; i < Particles.size(); ++i)
		{
			float segLen = Vec3Distance(Particles[i].RestPosition, Particles[i - 1].RestPosition);
			totalLength += segLen;
			Particles[i].LengthFromRoot = totalLength;
		}

		if (totalLength > KAWAII_SMALL_NUMBER)
		{
			for (auto& P : Particles)
				P.NormalizedLength = P.LengthFromRoot / totalLength;
		}
	}

	void BuildVerticalConstraints(
		const std::vector<FSimParticle>& Particles,
		std::vector<FDistanceConstraint>& OutConstraints,
		float DefaultShrinkCompliance,
		float DefaultStretchCompliance)
	{
		OutConstraints.clear();
		for (size_t i = 1; i < Particles.size(); ++i)
		{
			FDistanceConstraint c;
			c.IndexA = (int32_t)(i - 1);
			c.IndexB = (int32_t)i;
			c.RestLength = Vec3Distance(Particles[i].RestPosition, Particles[i - 1].RestPosition);
			c.ShrinkCompliance = DefaultShrinkCompliance;
			c.StretchCompliance = DefaultStretchCompliance;
			c.bCollision = true;
			OutConstraints.push_back(c);
		}
	}

	void BuildHorizontalConstraints(
		const std::vector<std::vector<FSimParticle>>& ChainTable,
		const std::vector<int32_t>& ChainOffsets,
		std::vector<FDistanceConstraint>& OutConstraints,
		bool bLoop,
		float DefaultShrinkCompliance,
		float DefaultStretchCompliance)
	{
		OutConstraints.clear();
		if (ChainTable.size() < 2) return;

		int32_t numChains = (int32_t)ChainTable.size();

		for (int32_t chainA = 0; chainA < numChains - 1; ++chainA)
		{
			int32_t chainB = chainA + 1;
			int32_t minLen = (int32_t)std::min(ChainTable[chainA].size(), ChainTable[chainB].size());

			for (int32_t j = 0; j < minLen; ++j)
			{
				FDistanceConstraint c;
				c.IndexA = ChainOffsets[chainA] + j;
				c.IndexB = ChainOffsets[chainB] + j;
				c.RestLength = Vec3Distance(ChainTable[chainA][j].RestPosition, ChainTable[chainB][j].RestPosition);
				c.ShrinkCompliance = DefaultShrinkCompliance;
				c.StretchCompliance = DefaultStretchCompliance;
				c.bCollision = false;
				OutConstraints.push_back(c);
			}
		}

		if (bLoop && numChains >= 3)
		{
			int32_t chainA = numChains - 1;
			int32_t chainB = 0;
			int32_t minLen = (int32_t)std::min(ChainTable[chainA].size(), ChainTable[chainB].size());

			for (int32_t j = 0; j < minLen; ++j)
			{
				FDistanceConstraint c;
				c.IndexA = ChainOffsets[chainA] + j;
				c.IndexB = ChainOffsets[chainB] + j;
				c.RestLength = Vec3Distance(ChainTable[chainA][j].RestPosition, ChainTable[chainB][j].RestPosition);
				c.ShrinkCompliance = DefaultShrinkCompliance;
				c.StretchCompliance = DefaultStretchCompliance;
				c.bCollision = false;
				OutConstraints.push_back(c);
			}
		}
	}

	void BuildShearConstraints(
		const std::vector<std::vector<FSimParticle>>& ChainTable,
		const std::vector<int32_t>& ChainOffsets,
		std::vector<FDistanceConstraint>& OutConstraints,
		bool bLoop,
		float DefaultShrinkCompliance,
		float DefaultStretchCompliance)
	{
		OutConstraints.clear();
		if (ChainTable.size() < 2) return;

		int32_t numChains = (int32_t)ChainTable.size();

		auto AddDiagonals = [&](int32_t chainA, int32_t chainB)
		{
			int32_t minLen = (int32_t)std::min(ChainTable[chainA].size(), ChainTable[chainB].size());
			for (int32_t j = 0; j + 1 < minLen; ++j)
			{
				// Diagonal A[j] -> B[j+1]
				{
					FDistanceConstraint c;
					c.IndexA = ChainOffsets[chainA] + j;
					c.IndexB = ChainOffsets[chainB] + j + 1;
					c.RestLength = Vec3Distance(ChainTable[chainA][j].RestPosition, ChainTable[chainB][j + 1].RestPosition);
					c.ShrinkCompliance = DefaultShrinkCompliance;
					c.StretchCompliance = DefaultStretchCompliance;
					OutConstraints.push_back(c);
				}
				// Diagonal A[j+1] -> B[j]
				{
					FDistanceConstraint c;
					c.IndexA = ChainOffsets[chainA] + j + 1;
					c.IndexB = ChainOffsets[chainB] + j;
					c.RestLength = Vec3Distance(ChainTable[chainA][j + 1].RestPosition, ChainTable[chainB][j].RestPosition);
					c.ShrinkCompliance = DefaultShrinkCompliance;
					c.StretchCompliance = DefaultStretchCompliance;
					OutConstraints.push_back(c);
				}
			}
		};

		for (int32_t c = 0; c < numChains - 1; ++c)
			AddDiagonals(c, c + 1);

		if (bLoop && numChains >= 3)
			AddDiagonals(numChains - 1, 0);
	}

	void BuildChainBendingConstraints(
		const std::vector<FSimParticle>& Particles,
		std::vector<FBendingConstraint>& OutConstraints,
		float DefaultCompliance,
		float DefaultDeadZone)
	{
		OutConstraints.clear();
		if (Particles.size() < 3) return;

		for (size_t i = 0; i + 2 < Particles.size(); ++i)
		{
			FBendingConstraint bc;
			bc.IndexP1 = (int32_t)i;
			bc.IndexPivot = (int32_t)(i + 1);
			bc.IndexP2 = (int32_t)(i + 2);

			v3dxVector3 centroid = (Particles[i].RestPosition + Particles[i + 1].RestPosition + Particles[i + 2].RestPosition) * (1.0f / 3.0f);
			bc.RestLength = Vec3Distance(Particles[i + 1].RestPosition, centroid);
			bc.Compliance = DefaultCompliance;
			bc.MaxBending = DefaultDeadZone;
			OutConstraints.push_back(bc);
		}
	}

	void BuildHorizontalBendingConstraints(
		const std::vector<std::vector<FSimParticle>>& ChainTable,
		const std::vector<int32_t>& ChainOffsets,
		std::vector<FBendingConstraint>& OutConstraints,
		bool bLoop,
		float DefaultCompliance,
		float DefaultDeadZone)
	{
		OutConstraints.clear();
		if (ChainTable.size() < 3) return;

		int32_t numChains = (int32_t)ChainTable.size();

		for (int32_t c = 0; c + 2 < numChains; ++c)
		{
			int32_t minLen = (int32_t)std::min({ChainTable[c].size(), ChainTable[c + 1].size(), ChainTable[c + 2].size()});
			for (int32_t j = 0; j < minLen; ++j)
			{
				FBendingConstraint bc;
				bc.IndexP1 = ChainOffsets[c] + j;
				bc.IndexPivot = ChainOffsets[c + 1] + j;
				bc.IndexP2 = ChainOffsets[c + 2] + j;

				v3dxVector3 centroid = (ChainTable[c][j].RestPosition + ChainTable[c + 1][j].RestPosition + ChainTable[c + 2][j].RestPosition) * (1.0f / 3.0f);
				bc.RestLength = Vec3Distance(ChainTable[c + 1][j].RestPosition, centroid);
				bc.Compliance = DefaultCompliance;
				bc.MaxBending = DefaultDeadZone;
				OutConstraints.push_back(bc);
			}
		}

		if (bLoop && numChains >= 3)
		{
			// Wrap-around bending
			auto AddWrapBending = [&](int32_t cA, int32_t cB, int32_t cC)
			{
				int32_t minLen = (int32_t)std::min({ChainTable[cA].size(), ChainTable[cB].size(), ChainTable[cC].size()});
				for (int32_t j = 0; j < minLen; ++j)
				{
					FBendingConstraint bc;
					bc.IndexP1 = ChainOffsets[cA] + j;
					bc.IndexPivot = ChainOffsets[cB] + j;
					bc.IndexP2 = ChainOffsets[cC] + j;
					v3dxVector3 centroid = (ChainTable[cA][j].RestPosition + ChainTable[cB][j].RestPosition + ChainTable[cC][j].RestPosition) * (1.0f / 3.0f);
					bc.RestLength = Vec3Distance(ChainTable[cB][j].RestPosition, centroid);
					bc.Compliance = DefaultCompliance;
					bc.MaxBending = DefaultDeadZone;
					OutConstraints.push_back(bc);
				}
			};

			AddWrapBending(numChains - 2, numChains - 1, 0);
			AddWrapBending(numChains - 1, 0, 1);
		}
	}

	void BuildClothSurface(
		const std::vector<std::vector<FSimParticle>>& ChainTable,
		bool bLoop,
		FSimSurface& OutSurface)
	{
		OutSurface.Triangles.clear();
		OutSurface.Edges.clear();

		int32_t numChains = (int32_t)ChainTable.size();
		if (numChains < 2) return;

		int32_t numPairsToProcess = bLoop ? numChains : (numChains - 1);

		for (int32_t c = 0; c < numPairsToProcess; ++c)
		{
			int32_t nextC = (c + 1) % numChains;
			int32_t minLen = (int32_t)std::min(ChainTable[c].size(), ChainTable[nextC].size());

			for (int32_t j = 0; j + 1 < minLen; ++j)
			{
				// Two triangles per quad
				FSimTriangle tri1;
				tri1.Indices[0] = { c, j };
				tri1.Indices[1] = { nextC, j };
				tri1.Indices[2] = { c, j + 1 };
				OutSurface.Triangles.push_back(tri1);

				FSimTriangle tri2;
				tri2.Indices[0] = { nextC, j };
				tri2.Indices[1] = { nextC, j + 1 };
				tri2.Indices[2] = { c, j + 1 };
				OutSurface.Triangles.push_back(tri2);

				// Edges (vertical + horizontal + diagonal)
				OutSurface.Edges.push_back({{ {c, j}, {c, j + 1} }});
				OutSurface.Edges.push_back({{ {c, j}, {nextC, j} }});
				OutSurface.Edges.push_back({{ {nextC, j}, {c, j + 1} }});
			}

			// Last horizontal edge row
			if (minLen > 0)
			{
				OutSurface.Edges.push_back({{ {c, minLen - 1}, {nextC, minLen - 1} }});
			}
		}
	}

	void BuildFlatParticlePointers(
		std::vector<std::vector<FSimParticle>>& ChainTable,
		std::vector<FSimParticle*>& OutFlat,
		std::vector<int32_t>& OutChainOffsets)
	{
		OutFlat.clear();
		OutChainOffsets.clear();

		for (auto& chain : ChainTable)
		{
			OutChainOffsets.push_back((int32_t)OutFlat.size());
			for (auto& p : chain)
				OutFlat.push_back(&p);
		}
	}

} // namespace SimJointHelpers
} // namespace KawaiiPhysics

NS_END
