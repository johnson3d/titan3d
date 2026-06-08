#pragma once
#include "../KawaiiTypes.h"
#include "../Solvers/SimParticle.h"
#include "CollisionUtils.h"
#include <vector>

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// FSimSurface - Triangulated surface built from a cloth mesh grid
	// =====================================================================

	struct FSimJointIndex
	{
		int32_t ChainIndex = -1;
		int32_t JointIndex = -1;

		bool IsValid() const { return ChainIndex >= 0 && JointIndex >= 0; }
	};

	struct FSimTriangle
	{
		FSimJointIndex Indices[3];
	};

	struct FSimEdge
	{
		FSimJointIndex Indices[2];
	};

	struct FSimSurface
	{
		std::vector<FSimTriangle> Triangles;
		std::vector<FSimEdge> Edges;

		bool IsValid() const { return !Triangles.empty(); }
	};

	// =====================================================================
	// FKawaiiLayerGroup - Multi-layer cloth collision group
	// =====================================================================

	struct FKawaiiLayerPair
	{
		int32_t InnerLayerIndex = -1;
		int32_t OuterLayerIndex = -1;
	};

	struct FKawaiiLayerGroup
	{
		std::vector<FKawaiiLayerPair> LayerPairs;
		float LayerThickness = 1.0f;
		float LayerFriction = 0.2f;
	};

	// =====================================================================
	// ClothUntangling - Multi-layer collision solver
	// =====================================================================

	namespace ClothUntangling
	{
		// Particle accessor: given a FSimJointIndex, returns a pointer to the particle.
		// The caller provides this via a callback so the solver is decoupled from storage layout.
		using FParticleAccessor = std::function<FSimParticle*(const FSimJointIndex&)>;

		// Solve point-face collisions between two layers
		void SolvePointFaceCollisions(
			const FSimSurface& InnerSurface,
			const FSimSurface& OuterSurface,
			const FParticleAccessor& GetParticle,
			float Thickness,
			float Friction,
			bool bUseLayerNormal);

		// Solve edge-edge collisions between two layers
		void SolveEdgeEdgeCollisions(
			const FSimSurface& InnerSurface,
			const FSimSurface& OuterSurface,
			const FParticleAccessor& GetParticle,
			float Thickness,
			float Friction,
			bool bUseLayerNormal);

		// Compute the average outward normal of a surface
		v3dxVector3 ComputeSurfaceNormal(
			const FSimSurface& Surface,
			const FParticleAccessor& GetParticle);
	}

} // namespace KawaiiPhysics

NS_END
