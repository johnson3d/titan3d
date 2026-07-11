#pragma once
#include "KawaiiPhySettings.h"
#include "Solvers/KawaiiChainSolver.h"
#include "Solvers/KawaiiClothSolver.h"
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

void BuildRods(
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

private:
FKawaiiPhysicsContext SimContext;

KawaiiChainSolver ChainSolver;
KawaiiClothSolver ClothSolver;

std::vector<FKawaiiChainSetup> ChainSetups;
std::vector<FKawaiiClothSetup> ClothSetups;
std::vector<FKawaiiRodSetup> RodSetups;
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
