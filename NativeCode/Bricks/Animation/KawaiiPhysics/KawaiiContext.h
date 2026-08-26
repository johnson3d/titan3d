#pragma once
#include "KawaiiPhySettings.h"
#include "Solvers/KawaiiChainSolver.h"
#include "Solvers/KawaiiClothSolver.h"
#include "Solvers/KawaiiRibbonSolver.h"
#include "Solvers/CosseratRodSolver.h"
#include "Collision/DynamicCollider.h"
#include "Collision/MeshCollider.h"
#include "Collision/TopLevelBVH.h"

NS_BEGIN

namespace KawaiiPhysics
{
	// =====================================================================
	// KawaiiPhysicsContext - TR_CLASS exported top-level manager.
	//
	// This is the C#-visible entry point for the entire KawaiiPhysics system.
	// C# code creates an instance, configures chains/cloth/rods and colliders,
	// then calls Simulate() each frame with updated bone transforms.
	// =====================================================================

	class TR_CLASS()
		KawaiiPhysicsContext : public IWeakRefObject
	{
	public:
	ENGINE_RTTI(KawaiiPhysicsContext)

	KawaiiPhysicsContext();
	~KawaiiPhysicsContext();

	// -----------------------------------------------------------------
	// Simulation context (set per-frame before Simulate)
	// -----------------------------------------------------------------

	void SetDeltaTime(float DeltaTime);
	void SetGravity(float X, float Y, float Z);
	void SetGravityScale(float Scale);
	void SetWind(float X, float Y, float Z, bool bEnable);
	void SetSimulationLOD(int32_t LOD);
	void SetConstraintIterations(int32_t Iterations);
	void SetCollisionSubSteps(int32_t SubSteps);
	void SetSpeedScale(float Scale);
	void SetMaxSpeed(float MaxSpeed);
	void SetSleepThreshold(float Threshold);

	void SetComponentTransform(
	float PosX, float PosY, float PosZ,
	float RotX, float RotY, float RotZ, float RotW,
	float ScaleX, float ScaleY, float ScaleZ);

	// -----------------------------------------------------------------
	// Chain solver interface
	// -----------------------------------------------------------------

	void InitializeChains(int32_t NumChains);
	void SetChainSetup(int32_t Index,
	const char* Name,
	int32_t RootBoneIndex, int32_t EndBoneIndex,
	float TailBoneLength, int32_t TailBoneAxis,
	bool bConstrainBoneLength, float BoneLengthConstraintBlend,
	bool bRootCollision, int32_t LODThreshold);
	void SetChainPhysicsSettings(int32_t Index,
	FKawaiiPhySettings PhysicsSettings,
	FKawaiiPhySettings PhysicsSettingsRandom);

	// Per-bone parameter curves. CurveId: 0=Stiffness 1=Damping 2=WorldDampLoc 3=WorldDampRot
	// 4=LimitAngle 5=Radius 6=Drag 7=Wind. Mode: 0=IndexRate 1=LengthRate 2=AbsoluteLengthRate.
	void SetChainCurveMode(int32_t Index, int32_t Mode);
	void SetChainPhysicsCurve(int32_t Index, int32_t CurveId,
	const float* Times, const float* Values, int32_t Count);

	void BuildChains(
	const v3dxVector3* BonePositions,
	const v3dxQuaternion* BoneRotations,
	const v3dxVector3* BoneScales,
	const int32_t* ParentIndices,
	int32_t NumBones);

	void SetChainSegmentCollision(bool bEnable);

	// -----------------------------------------------------------------
	// Cloth solver interface
	// -----------------------------------------------------------------

	void InitializeClothSetups(int32_t NumSetups);
	void SetClothSetup(int32_t Index,
	const char* Name,
	int32_t RootBoneIndex, int32_t EndBoneIndex,
	float TailBoneLength, int32_t TailBoneAxis,
	bool bConstrainBoneLength, float BoneLengthConstraintBlend,
	bool bRootCollision, int32_t LODThreshold,
	bool bLoopChains);
	void SetClothPhysicsSettings(int32_t Index,
	FKawaiiPhySettings PhysicsSettings,
	FKawaiiPhySettings PhysicsSettingsRandom);

	// Cloth reuses the chain per-bone physics curves (same CurveId map as SetChainPhysicsCurve).
	void SetClothCurveMode(int32_t Index, int32_t Mode);
	void SetClothPhysicsCurve(int32_t Index, int32_t CurveId,
	const float* Times, const float* Values, int32_t Count);
	// Cloth structural stiffness curves. CurveId: 0=VShrink 1=VStretch 2=HShrink 3=HStretch
	// 4=VBend 5=HBend 6=ShearShrink 7=ShearStretch.
	void SetClothStructuralCurve(int32_t Index, int32_t CurveId,
	const float* Times, const float* Values, int32_t Count);

	void BuildCloth(
	const v3dxVector3* BonePositions,
	const v3dxQuaternion* BoneRotations,
	const v3dxVector3* BoneScales,
	const int32_t* ParentIndices,
	int32_t NumBones);

	// -----------------------------------------------------------------
	// Cosserat rod solver interface
	// -----------------------------------------------------------------

	void InitializeRods(int32_t NumRods);
	void SetRodSetup(int32_t Index,
	const char* Name,
	int32_t RootBoneIndex, int32_t EndBoneIndex,
	float StretchShearStiffness, float BendTwistStiffness,
	float PointAttachStiffness, float OrientAttachStiffness,
	int32_t LODThreshold);
	void SetRodPhysicsSettings(int32_t Index,
	FKawaiiPhySettings PhysicsSettings,
	FKawaiiPhySettings PhysicsSettingsRandom);

	// Per-segment rod stiffness curves. CurveId: 0=StretchShear 1=BendTwist 2=PointAttach 3=OrientAttach.
	void SetRodPhysicsCurve(int32_t Index, int32_t CurveId,
	const float* Times, const float* Values, int32_t Count);

	void BuildRods(
	const v3dxVector3* BonePositions,
	const v3dxQuaternion* BoneRotations,
	const v3dxVector3* BoneScales,
	const int32_t* ParentIndices,
	int32_t NumBones);

	// -----------------------------------------------------------------
	// Ribbon solver interface
	// -----------------------------------------------------------------
	//
	// The ribbon parameter set is far larger than the other solvers', so it is pushed in
	// themed groups rather than one giant signature. Every group is optional: a ribbon that
	// only gets SetRibbonSetup still runs with the FKawaiiRibbonSetup defaults.
	// All angles are DEGREES here (converted once in BuildRibbons).

	void InitializeRibbons(int32_t NumRibbons);
	void SetRibbonSetup(int32_t Index,
	const char* Name,
	int32_t RootBoneIndex, int32_t EndBoneIndex,
	float TailBoneLength, int32_t TailBoneAxis,
	int32_t LODThreshold);

	void SetRibbonSway(int32_t Index,
	float SwingAngleDegrees, float SwayFrequency,
	float Inertia, float InertiaFalloff,
	float TipAmplify, float AmplifyCurvePower);
	void SetRibbonSwingPlane(int32_t Index, float SwingPlaneAngleDegrees, float RestTiltAngleDegrees);
	void SetRibbonNoise(int32_t Index, float NoiseMix, int32_t NoiseLayers, float NoiseRoughness, float NoiseScale);
	void SetRibbonWind(int32_t Index, float WindResponse, float WindGustiness, float GustFrequency);

	// Ribbon curves sampled by NormalizedLength. CurveId: 0=SwingAmplitude 1=WindInfluence.
	void SetRibbonCurve(int32_t Index, int32_t CurveId,
	const float* Times, const float* Values, int32_t Count);

	void BuildRibbons(
	const v3dxVector3* BonePositions,
	const v3dxQuaternion* BoneRotations,
	const v3dxVector3* BoneScales,
	const int32_t* ParentIndices,
	int32_t NumBones);

	// -----------------------------------------------------------------
	// Collider management
	// -----------------------------------------------------------------

	int32_t AddSphereCollider(float CenterX, float CenterY, float CenterZ, float Radius);
	int32_t AddCapsuleCollider(float CenterX, float CenterY, float CenterZ,
	float DirX, float DirY, float DirZ,
	float Radius, float HalfLength);
	int32_t AddPlaneCollider(float OriginX, float OriginY, float OriginZ,
	float NormalX, float NormalY, float NormalZ);
	int32_t AddBoxCollider(float CenterX, float CenterY, float CenterZ,
	float RotX, float RotY, float RotZ, float RotW,
	float HalfExtX, float HalfExtY, float HalfExtZ);

	void UpdateSphereCollider(int32_t Index, float CenterX, float CenterY, float CenterZ, float Radius);
	void UpdateCapsuleCollider(int32_t Index, float CenterX, float CenterY, float CenterZ,
	float DirX, float DirY, float DirZ, float Radius, float HalfLength);
	void RemoveCollider(int32_t Index);
	void ClearColliders();

	// -----------------------------------------------------------------
	// Simulation
	// -----------------------------------------------------------------

	void UpdatePose(
	const v3dxVector3* BonePositions,
	const v3dxQuaternion* BoneRotations,
	const v3dxVector3* BoneScales,
	int32_t NumBones);

	void Simulate();
	void ResetDynamics();

	// -----------------------------------------------------------------
	// Results query
	// -----------------------------------------------------------------

	int32_t GetChainCount() const;
	int32_t GetChainParticleCount(int32_t ChainIndex) const;
	void GetChainParticlePosition(int32_t ChainIndex, int32_t ParticleIndex,
	float& OutX, float& OutY, float& OutZ) const;
	int32_t GetChainParticleBoneIndex(int32_t ChainIndex, int32_t ParticleIndex) const;

	int32_t GetClothMeshCount() const;
	int32_t GetClothChainCount(int32_t MeshIndex) const;
	int32_t GetClothChainParticleCount(int32_t MeshIndex, int32_t ChainIndex) const;
	void GetClothParticlePosition(int32_t MeshIndex, int32_t ChainIndex, int32_t ParticleIndex,
	float& OutX, float& OutY, float& OutZ) const;
	int32_t GetClothParticleBoneIndex(int32_t MeshIndex, int32_t ChainIndex, int32_t ParticleIndex) const;

	int32_t GetRodCount() const;
	int32_t GetRodParticleCount(int32_t RodIndex) const;
	void GetRodParticlePosition(int32_t RodIndex, int32_t ParticleIndex,
	float& OutX, float& OutY, float& OutZ) const;
	int32_t GetRodParticleBoneIndex(int32_t RodIndex, int32_t ParticleIndex) const;

	int32_t GetRibbonCount() const;
	int32_t GetRibbonParticleCount(int32_t RibbonIndex) const;
	void GetRibbonParticlePosition(int32_t RibbonIndex, int32_t ParticleIndex,
	float& OutX, float& OutY, float& OutZ) const;
	int32_t GetRibbonParticleBoneIndex(int32_t RibbonIndex, int32_t ParticleIndex) const;

	private:
	FKawaiiPhysicsContext SimContext;

	KawaiiChainSolver ChainSolver;
	KawaiiClothSolver ClothSolver;
	KawaiiRibbonSolver RibbonSolver;

	std::vector<FKawaiiChainSetup> ChainSetups;
	std::vector<FKawaiiClothSetup> ClothSetups;
	std::vector<FKawaiiRodSetup> RodSetups;
	std::vector<FKawaiiRibbonSetup> RibbonSetups;
	std::vector<FCosseratRodData> Rods;

	FTopLevelBVH ColliderBVH;

	struct ColliderEntry
	{
	std::unique_ptr<FColliderBase> Collider;
	bool bActive = true;
	};
	std::vector<ColliderEntry> Colliders;

	// Bone data cache
	std::vector<v3dxVector3> CachedBonePositions;
	std::vector<v3dxQuaternion> CachedBoneRotations;
	std::vector<v3dxVector3> CachedBoneScales;
	std::vector<int32_t> CachedParentIndices;

	void RebuildColliderBVH();
	};

} // namespace KawaiiPhysics

NS_END
