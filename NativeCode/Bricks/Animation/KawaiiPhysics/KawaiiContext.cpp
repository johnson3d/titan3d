#include "KawaiiContext.h"

NS_BEGIN

namespace KawaiiPhysics
{
	KawaiiPhysicsContext::KawaiiPhysicsContext() {}
	KawaiiPhysicsContext::~KawaiiPhysicsContext() { ClearColliders(); }

	// -----------------------------------------------------------------
	// Simulation context setters
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::SetDeltaTime(float DeltaTime) { SimContext.DeltaTime = DeltaTime; }
	void KawaiiPhysicsContext::SetGravity(float X, float Y, float Z) { SimContext.Gravity = v3dxVector3(X, Y, Z); }
	void KawaiiPhysicsContext::SetGravityScale(float Scale) { SimContext.GravityScale = Scale; }
	void KawaiiPhysicsContext::SetWind(float X, float Y, float Z, bool bEnable) { SimContext.WindForce = v3dxVector3(X, Y, Z); SimContext.bEnableWind = bEnable; }
	void KawaiiPhysicsContext::SetSimulationLOD(int32_t LOD) { SimContext.SimulationLOD = LOD; }
	void KawaiiPhysicsContext::SetConstraintIterations(int32_t Iterations) { SimContext.ConstraintIterations = Iterations; }
	void KawaiiPhysicsContext::SetCollisionSubSteps(int32_t SubSteps) { SimContext.CollisionSubSteps = SubSteps; }
	void KawaiiPhysicsContext::SetSpeedScale(float Scale) { SimContext.SpeedScale = Scale; }
	void KawaiiPhysicsContext::SetMaxSpeed(float MaxSpeed) { SimContext.MaxSpeed = MaxSpeed; }
	void KawaiiPhysicsContext::SetSleepThreshold(float Threshold) { SimContext.SleepThreshold = Threshold; }

	void KawaiiPhysicsContext::SetComponentTransform(
		float PosX, float PosY, float PosZ,
		float RotX, float RotY, float RotZ, float RotW,
		float ScaleX, float ScaleY, float ScaleZ)
	{
		SimContext.PrevComponentLocation = SimContext.ComponentLocation;
		SimContext.PrevComponentRotation = SimContext.ComponentRotation;
		SimContext.ComponentLocation = v3dxVector3(PosX, PosY, PosZ);
		SimContext.ComponentRotation = v3dxQuaternion(RotX, RotY, RotZ, RotW);
		SimContext.ComponentScale = v3dxVector3(ScaleX, ScaleY, ScaleZ);
	}

	// -----------------------------------------------------------------
	// Chain solver
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::InitializeChains(int32_t NumChains)
	{
		ChainSetups.clear();
		ChainSetups.resize(NumChains);
	}

	void KawaiiPhysicsContext::SetChainSetup(int32_t Index,
		const char* Name,
		int32_t RootBoneIndex, int32_t EndBoneIndex,
		float TailBoneLength, int32_t TailBoneAxis,
		bool bConstrainBoneLength, float BoneLengthConstraintBlend,
		bool bRootCollision, int32_t LODThreshold)
	{
		if (Index < 0 || Index >= (int32_t)ChainSetups.size()) return;
		auto& s = ChainSetups[Index];
		s.Name = Name ? Name : "";
		s.RootBoneIndex = RootBoneIndex;
		s.EndBoneIndex = EndBoneIndex;
		s.TailBoneLength = TailBoneLength;
		s.TailBoneForwardAxis = (ETailBoneAxis)KawaiiClamp(TailBoneAxis, 0, 5);
		s.bConstrainBoneLength = bConstrainBoneLength;
		s.BoneLengthConstraintBlend = BoneLengthConstraintBlend;
		s.bRootCollision = bRootCollision;
		s.LODThreshold = LODThreshold;
	}

	void KawaiiPhysicsContext::SetChainPhysicsSettings(int32_t Index,
		FKawaiiPhySettings PhysicsSettings,
		FKawaiiPhySettings PhysicsSettingsRandom)
	{
		if (Index < 0 || Index >= (int32_t)ChainSetups.size()) return;
		auto& s = ChainSetups[Index];
		s.PhysicsSettings = PhysicsSettings;
		s.PhysicsSettingsRandom = PhysicsSettingsRandom;
	}

	// Fill an FKawaiiCurve from parallel time/value arrays pushed from C#.
	static void FillKawaiiCurve(FKawaiiCurve& Curve, const float* Times, const float* Values, int32_t Count)
	{
		Curve.Keys.clear();
		if (!Times || !Values || Count <= 0) return;
		Curve.Keys.reserve(Count);
		for (int32_t i = 0; i < Count; ++i)
			Curve.Keys.push_back({ Times[i], Values[i] });
	}

	// Pick one of the 8 per-bone physics curves on a chain-setup base by CurveId.
	static FKawaiiCurve* PickPhysicsCurve(FKawaiiChainSetup& s, int32_t CurveId)
	{
		switch (CurveId)
		{
		case 0: return &s.StiffnessCurve;
		case 1: return &s.DampingCurve;
		case 2: return &s.WorldDampingLocationCurve;
		case 3: return &s.WorldDampingRotationCurve;
		case 4: return &s.LimitAngleCurve;
		case 5: return &s.RadiusCurve;
		case 6: return &s.DragCurve;
		case 7: return &s.WindCurve;
		default: return nullptr;
		}
	}

	void KawaiiPhysicsContext::SetChainCurveMode(int32_t Index, int32_t Mode)
	{
		if (Index < 0 || Index >= (int32_t)ChainSetups.size()) return;
		ChainSetups[Index].CurveMode = (EKawaiiCurveEvalMode)KawaiiClamp(Mode, 0, 2);
	}

	void KawaiiPhysicsContext::SetChainPhysicsCurve(int32_t Index, int32_t CurveId,
		const float* Times, const float* Values, int32_t Count)
	{
		if (Index < 0 || Index >= (int32_t)ChainSetups.size()) return;
		if (auto* c = PickPhysicsCurve(ChainSetups[Index], CurveId))
			FillKawaiiCurve(*c, Times, Values, Count);
	}

	void KawaiiPhysicsContext::BuildChains(
		const v3dxVector3* BonePositions,
		const v3dxQuaternion* BoneRotations,
		const v3dxVector3* BoneScales,
		const int32_t* ParentIndices,
		int32_t NumBones)
	{
		CachedBonePositions.assign(BonePositions, BonePositions + NumBones);
		CachedBoneRotations.assign(BoneRotations, BoneRotations + NumBones);
		CachedBoneScales.assign(BoneScales, BoneScales + NumBones);
		CachedParentIndices.assign(ParentIndices, ParentIndices + NumBones);

		ChainSolver.Initialize(ChainSetups, CachedBonePositions, CachedBoneRotations, CachedBoneScales, CachedParentIndices);
	}

	void KawaiiPhysicsContext::SetChainSegmentCollision(bool bEnable) { ChainSolver.bEnableSegmentCollision = bEnable; }

	// -----------------------------------------------------------------
	// Cloth solver
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::InitializeClothSetups(int32_t NumSetups)
	{
		ClothSetups.clear();
		ClothSetups.resize(NumSetups);
	}

	void KawaiiPhysicsContext::SetClothSetup(int32_t Index,
		const char* Name,
		int32_t RootBoneIndex, int32_t EndBoneIndex,
		float TailBoneLength, int32_t TailBoneAxis,
		bool bConstrainBoneLength, float BoneLengthConstraintBlend,
		bool bRootCollision, int32_t LODThreshold,
		bool bLoopChains)
	{
		if (Index < 0 || Index >= (int32_t)ClothSetups.size()) return;
		auto& s = ClothSetups[Index];
		s.Name = Name ? Name : "";
		s.RootBoneIndex = RootBoneIndex;
		s.EndBoneIndex = EndBoneIndex;
		s.TailBoneLength = TailBoneLength;
		s.TailBoneForwardAxis = (ETailBoneAxis)KawaiiClamp(TailBoneAxis, 0, 5);
		s.bConstrainBoneLength = bConstrainBoneLength;
		s.BoneLengthConstraintBlend = BoneLengthConstraintBlend;
		s.bRootCollision = bRootCollision;
		s.LODThreshold = LODThreshold;
		s.bLoopChains = bLoopChains;
	}

	void KawaiiPhysicsContext::SetClothPhysicsSettings(int32_t Index,
		FKawaiiPhySettings PhysicsSettings,
		FKawaiiPhySettings PhysicsSettingsRandom)
	{
		if (Index < 0 || Index >= (int32_t)ClothSetups.size()) return;
		auto& s = ClothSetups[Index];
		s.PhysicsSettings = PhysicsSettings;
		s.PhysicsSettingsRandom = PhysicsSettingsRandom;
	}

	// Pick one of the 8 cloth structural stiffness curves by CurveId.
	static FKawaiiCurve* PickStructuralCurve(FKawaiiClothSetup& s, int32_t CurveId)
	{
		switch (CurveId)
		{
		case 0: return &s.VerticalShrinkStiffness;
		case 1: return &s.VerticalStretchStiffness;
		case 2: return &s.HorizontalShrinkStiffness;
		case 3: return &s.HorizontalStretchStiffness;
		case 4: return &s.VerticalBendStiffness;
		case 5: return &s.HorizontalBendStiffness;
		case 6: return &s.ShearShrinkStiffness;
		case 7: return &s.ShearStretchStiffness;
		default: return nullptr;
		}
	}

	void KawaiiPhysicsContext::SetClothCurveMode(int32_t Index, int32_t Mode)
	{
		if (Index < 0 || Index >= (int32_t)ClothSetups.size()) return;
		ClothSetups[Index].CurveMode = (EKawaiiCurveEvalMode)KawaiiClamp(Mode, 0, 2);
	}

	void KawaiiPhysicsContext::SetClothPhysicsCurve(int32_t Index, int32_t CurveId,
		const float* Times, const float* Values, int32_t Count)
	{
		if (Index < 0 || Index >= (int32_t)ClothSetups.size()) return;
		if (auto* c = PickPhysicsCurve(ClothSetups[Index], CurveId))
			FillKawaiiCurve(*c, Times, Values, Count);
	}

	void KawaiiPhysicsContext::SetClothStructuralCurve(int32_t Index, int32_t CurveId,
		const float* Times, const float* Values, int32_t Count)
	{
		if (Index < 0 || Index >= (int32_t)ClothSetups.size()) return;
		if (auto* c = PickStructuralCurve(ClothSetups[Index], CurveId))
			FillKawaiiCurve(*c, Times, Values, Count);
	}

	void KawaiiPhysicsContext::BuildCloth(
		const v3dxVector3* BonePositions,
		const v3dxQuaternion* BoneRotations,
		const v3dxVector3* BoneScales,
		const int32_t* ParentIndices,
		int32_t NumBones)
	{
		CachedBonePositions.assign(BonePositions, BonePositions + NumBones);
		CachedBoneRotations.assign(BoneRotations, BoneRotations + NumBones);
		CachedBoneScales.assign(BoneScales, BoneScales + NumBones);
		CachedParentIndices.assign(ParentIndices, ParentIndices + NumBones);

		ClothSolver.Initialize(ClothSetups, CachedBonePositions, CachedBoneRotations, CachedBoneScales, CachedParentIndices);
	}

	// -----------------------------------------------------------------
	// Cosserat rod solver
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::InitializeRods(int32_t NumRods)
	{
		RodSetups.clear();
		RodSetups.resize(NumRods);
	}

	void KawaiiPhysicsContext::SetRodSetup(int32_t Index,
		const char* Name,
		int32_t RootBoneIndex, int32_t EndBoneIndex,
		float StretchShearStiffness, float BendTwistStiffness,
		float PointAttachStiffness, float OrientAttachStiffness,
		int32_t LODThreshold)
	{
		if (Index < 0 || Index >= (int32_t)RodSetups.size()) return;
		auto& s = RodSetups[Index];
		s.Name = Name ? Name : "";
		s.RootBoneIndex = RootBoneIndex;
		s.EndBoneIndex = EndBoneIndex;
		s.StretchAndShearStiffness = StretchShearStiffness;
		s.BendAndTwistStiffness = BendTwistStiffness;
		s.PointAttachmentStiffness = PointAttachStiffness;
		s.OrientationAttachmentStiffness = OrientAttachStiffness;
		s.LODThreshold = LODThreshold;
	}

	void KawaiiPhysicsContext::SetRodPhysicsSettings(int32_t Index,
		FKawaiiPhySettings PhysicsSettings,
		FKawaiiPhySettings PhysicsSettingsRandom)
	{
		if (Index < 0 || Index >= (int32_t)RodSetups.size()) return;
		auto& s = RodSetups[Index];
		s.PhysicsSettings = PhysicsSettings;
		s.PhysicsSettingsRandom = PhysicsSettingsRandom;
	}

	void KawaiiPhysicsContext::SetRodPhysicsCurve(int32_t Index, int32_t CurveId,
		const float* Times, const float* Values, int32_t Count)
	{
		if (Index < 0 || Index >= (int32_t)RodSetups.size()) return;
		auto& s = RodSetups[Index];
		FKawaiiCurve* c = nullptr;
		switch (CurveId)
		{
		case 0: c = &s.StretchAndShearStiffnessCurve; break;
		case 1: c = &s.BendAndTwistStiffnessCurve; break;
		case 2: c = &s.PointAttachmentStiffnessCurve; break;
		case 3: c = &s.OrientationAttachmentStiffnessCurve; break;
		default: break;
		}
		if (c)
			FillKawaiiCurve(*c, Times, Values, Count);
	}

	void KawaiiPhysicsContext::BuildRods(
		const v3dxVector3* BonePositions,
		const v3dxQuaternion* BoneRotations,
		const v3dxVector3* BoneScales,
		const int32_t* ParentIndices,
		int32_t NumBones)
	{
		CachedBonePositions.assign(BonePositions, BonePositions + NumBones);
		CachedBoneRotations.assign(BoneRotations, BoneRotations + NumBones);
		CachedBoneScales.assign(BoneScales, BoneScales + NumBones);
		CachedParentIndices.assign(ParentIndices, ParentIndices + NumBones);

		Rods.clear();
		Rods.resize(RodSetups.size());

		for (size_t i = 0; i < RodSetups.size(); ++i)
		{
			const FKawaiiRodSetup& setup = RodSetups[i];
			FCosseratRodData& rod = Rods[i];
			rod.Name = setup.Name;
			rod.StretchAndShearStiffness = setup.StretchAndShearStiffness;
			rod.BendAndTwistStiffness = setup.BendAndTwistStiffness;
			rod.PointAttachmentStiffness = setup.PointAttachmentStiffness;
			rod.OrientationAttachmentStiffness = setup.OrientationAttachmentStiffness;
			rod.StretchAndShearStiffnessCurve = setup.StretchAndShearStiffnessCurve;
			rod.BendAndTwistStiffnessCurve = setup.BendAndTwistStiffnessCurve;
			rod.PointAttachmentStiffnessCurve = setup.PointAttachmentStiffnessCurve;
			rod.OrientationAttachmentStiffnessCurve = setup.OrientationAttachmentStiffnessCurve;
			rod.LODThreshold = setup.LODThreshold;

			SimJointHelpers::BuildChainParticles(
				CachedBonePositions, CachedBoneRotations, CachedBoneScales, CachedParentIndices,
				setup.RootBoneIndex, setup.EndBoneIndex,
				TBA_X_Positive, 0.0f,
				rod.Particles);

			SimJointHelpers::ApplyPhysicsSettings(
				rod.Particles,
				setup.PhysicsSettings,
				setup.PhysicsSettingsRandom,
				(uint32_t)(i + 0x20000u));

			rod.FlatParticles.clear();
			for (auto& p : rod.Particles)
				rod.FlatParticles.push_back(&p);

			CosseratRodSolver::InitializeRodElements(rod);
		}
	}

	// -----------------------------------------------------------------
	// Ribbon solver
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::InitializeRibbons(int32_t NumRibbons)
	{
		RibbonSetups.clear();
		RibbonSetups.resize(NumRibbons);
	}

	void KawaiiPhysicsContext::SetRibbonSetup(int32_t Index,
		const char* Name,
		int32_t RootBoneIndex, int32_t EndBoneIndex,
		float TailBoneLength, int32_t TailBoneAxis,
		int32_t LODThreshold)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		s.Name = Name ? Name : "";
		s.RootBoneIndex = RootBoneIndex;
		s.EndBoneIndex = EndBoneIndex;
		s.TailBoneLength = TailBoneLength;
		s.TailBoneForwardAxis = (ETailBoneAxis)KawaiiClamp(TailBoneAxis, 0, 5);
		s.LODThreshold = LODThreshold;
	}

	void KawaiiPhysicsContext::SetRibbonSway(int32_t Index,
		float SwingAngleDegrees, float SwayFrequency,
		float Inertia, float InertiaFalloff,
		float TipAmplify, float AmplifyCurvePower)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		s.SwingAngleDegrees = SwingAngleDegrees;
		s.SwayFrequency = SwayFrequency;
		s.Inertia = Inertia;
		s.InertiaFalloff = InertiaFalloff;
		s.TipAmplify = TipAmplify;
		s.AmplifyCurvePower = AmplifyCurvePower;
	}

	void KawaiiPhysicsContext::SetRibbonSwingPlane(int32_t Index, float SwingPlaneAngleDegrees, float RestTiltAngleDegrees)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		s.SwingPlaneAngleDegrees = SwingPlaneAngleDegrees;
		s.RestTiltAngleDegrees = RestTiltAngleDegrees;
	}

	void KawaiiPhysicsContext::SetRibbonNoise(int32_t Index, float NoiseMix, int32_t NoiseLayers, float NoiseRoughness, float NoiseScale)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		s.NoiseMix = NoiseMix;
		s.NoiseLayers = NoiseLayers;
		s.NoiseRoughness = NoiseRoughness;
		s.NoiseScale = NoiseScale;
	}

	void KawaiiPhysicsContext::SetRibbonWind(int32_t Index, float WindResponse, float WindGustiness, float GustFrequency)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		s.WindResponse = WindResponse;
		s.WindGustiness = WindGustiness;
		s.GustFrequency = GustFrequency;
	}

	void KawaiiPhysicsContext::SetRibbonCurve(int32_t Index, int32_t CurveId,
		const float* Times, const float* Values, int32_t Count)
	{
		if (Index < 0 || Index >= (int32_t)RibbonSetups.size()) return;
		auto& s = RibbonSetups[Index];
		FKawaiiCurve* c = nullptr;
		switch (CurveId)
		{
		case 0: c = &s.SwingAmplitudeCurve; break;
		case 1: c = &s.WindInfluenceCurve; break;
		default: break;
		}
		if (c)
			FillKawaiiCurve(*c, Times, Values, Count);
	}

	void KawaiiPhysicsContext::BuildRibbons(
		const v3dxVector3* BonePositions,
		const v3dxQuaternion* BoneRotations,
		const v3dxVector3* BoneScales,
		const int32_t* ParentIndices,
		int32_t NumBones)
	{
		CachedBonePositions.assign(BonePositions, BonePositions + NumBones);
		CachedBoneRotations.assign(BoneRotations, BoneRotations + NumBones);
		CachedBoneScales.assign(BoneScales, BoneScales + NumBones);
		CachedParentIndices.assign(ParentIndices, ParentIndices + NumBones);

		RibbonSolver.Initialize(RibbonSetups, CachedBonePositions, CachedBoneRotations, CachedBoneScales, CachedParentIndices);
	}

	// -----------------------------------------------------------------
	// Collider management
	// -----------------------------------------------------------------

	int32_t KawaiiPhysicsContext::AddSphereCollider(float CenterX, float CenterY, float CenterZ, float Radius)
	{
		auto collider = std::make_unique<FSphereCollider>();
		collider->Center = v3dxVector3(CenterX, CenterY, CenterZ);
		collider->Radius = Radius;
		collider->UpdateBoundBox();

		int32_t idx = (int32_t)Colliders.size();
		ColliderBVH.AddCollider(collider->BoundBoxHandle);
		Colliders.push_back({ std::move(collider), true });
		return idx;
	}

	int32_t KawaiiPhysicsContext::AddCapsuleCollider(float CenterX, float CenterY, float CenterZ,
		float DirX, float DirY, float DirZ, float Radius, float HalfLength)
	{
		auto collider = std::make_unique<FCapsuleCollider>();
		collider->Center = v3dxVector3(CenterX, CenterY, CenterZ);
		collider->Direction = Vec3SafeNormal(v3dxVector3(DirX, DirY, DirZ));
		collider->Radius = Radius;
		collider->HalfLength = HalfLength;
		collider->UpdateBoundBox();

		int32_t idx = (int32_t)Colliders.size();
		ColliderBVH.AddCollider(collider->BoundBoxHandle);
		Colliders.push_back({ std::move(collider), true });
		return idx;
	}

	int32_t KawaiiPhysicsContext::AddPlaneCollider(float OriginX, float OriginY, float OriginZ,
		float NormalX, float NormalY, float NormalZ)
	{
		auto collider = std::make_unique<FPlaneCollider>();
		collider->Origin = v3dxVector3(OriginX, OriginY, OriginZ);
		collider->Normal = Vec3SafeNormal(v3dxVector3(NormalX, NormalY, NormalZ));
		collider->UpdateBoundBox();

		int32_t idx = (int32_t)Colliders.size();
		ColliderBVH.AddCollider(collider->BoundBoxHandle);
		Colliders.push_back({ std::move(collider), true });
		return idx;
	}

	int32_t KawaiiPhysicsContext::AddBoxCollider(float CenterX, float CenterY, float CenterZ,
		float RotX, float RotY, float RotZ, float RotW,
		float HalfExtX, float HalfExtY, float HalfExtZ)
	{
		auto collider = std::make_unique<FBoxCollider>();
		collider->Center = v3dxVector3(CenterX, CenterY, CenterZ);
		collider->Rotation = v3dxQuaternion(RotX, RotY, RotZ, RotW);
		collider->HalfExtent = v3dxVector3(HalfExtX, HalfExtY, HalfExtZ);
		collider->UpdateBoundBox();

		int32_t idx = (int32_t)Colliders.size();
		ColliderBVH.AddCollider(collider->BoundBoxHandle);
		Colliders.push_back({ std::move(collider), true });
		return idx;
	}

	void KawaiiPhysicsContext::UpdateSphereCollider(int32_t Index, float CenterX, float CenterY, float CenterZ, float Radius)
	{
		if (Index < 0 || Index >= (int32_t)Colliders.size() || !Colliders[Index].bActive) return;
		auto* sphere = dynamic_cast<FSphereCollider*>(Colliders[Index].Collider.get());
		if (!sphere) return;
		sphere->Center = v3dxVector3(CenterX, CenterY, CenterZ);
		sphere->Radius = Radius;
		sphere->UpdateBoundBox();
		ColliderBVH.UpdateCollider(sphere->BoundBoxHandle);
	}

	void KawaiiPhysicsContext::UpdateCapsuleCollider(int32_t Index, float CenterX, float CenterY, float CenterZ,
		float DirX, float DirY, float DirZ, float Radius, float HalfLength)
	{
		if (Index < 0 || Index >= (int32_t)Colliders.size() || !Colliders[Index].bActive) return;
		auto* capsule = dynamic_cast<FCapsuleCollider*>(Colliders[Index].Collider.get());
		if (!capsule) return;
		capsule->Center = v3dxVector3(CenterX, CenterY, CenterZ);
		capsule->Direction = Vec3SafeNormal(v3dxVector3(DirX, DirY, DirZ));
		capsule->Radius = Radius;
		capsule->HalfLength = HalfLength;
		capsule->UpdateBoundBox();
		ColliderBVH.UpdateCollider(capsule->BoundBoxHandle);
	}

	void KawaiiPhysicsContext::RemoveCollider(int32_t Index)
	{
		if (Index < 0 || Index >= (int32_t)Colliders.size() || !Colliders[Index].bActive) return;
		ColliderBVH.RemoveCollider(Colliders[Index].Collider->BoundBoxHandle);
		Colliders[Index].bActive = false;
	}

	void KawaiiPhysicsContext::ClearColliders()
	{
		ColliderBVH.Clear();
		Colliders.clear();
	}

	void KawaiiPhysicsContext::RebuildColliderBVH()
	{
		for (auto& entry : Colliders)
		{
			if (entry.bActive && entry.Collider)
			{
				entry.Collider->UpdateBoundBox();
				ColliderBVH.UpdateCollider(entry.Collider->BoundBoxHandle);
			}
		}
	}

	// -----------------------------------------------------------------
	// Simulation
	// -----------------------------------------------------------------

	void KawaiiPhysicsContext::UpdatePose(
		const v3dxVector3* BonePositions,
		const v3dxQuaternion* BoneRotations,
		const v3dxVector3* BoneScales,
		int32_t NumBones)
	{
		CachedBonePositions.assign(BonePositions, BonePositions + NumBones);
		CachedBoneRotations.assign(BoneRotations, BoneRotations + NumBones);
		CachedBoneScales.assign(BoneScales, BoneScales + NumBones);

		ChainSolver.UpdatePose(CachedBonePositions, CachedBoneRotations, CachedBoneScales);
		ClothSolver.UpdatePose(CachedBonePositions, CachedBoneRotations, CachedBoneScales);
		RibbonSolver.UpdatePose(CachedBonePositions, CachedBoneRotations, CachedBoneScales);

		// Update rod poses
		for (auto& rod : Rods)
		{
			for (auto& P : rod.Particles)
			{
				if (P.bDummy) continue;
				int32_t boneIdx = P.BoneIndex;
				if (boneIdx >= 0 && boneIdx < NumBones)
					P.UpdatePoseFromExternal(BonePositions[boneIdx], BoneRotations[boneIdx], BoneScales[boneIdx]);
			}
		}
	}

	void KawaiiPhysicsContext::Simulate()
	{
		RebuildColliderBVH();

		// Chain simulation
		ChainSolver.CollisionSubSteps = SimContext.CollisionSubSteps;
		ChainSolver.Simulate(SimContext, &ColliderBVH);

		// Cloth simulation
		ClothSolver.CollisionSubSteps = SimContext.CollisionSubSteps;
		ClothSolver.Simulate(SimContext, &ColliderBVH);

		// Ribbon simulation
		RibbonSolver.Simulate(SimContext);

		// Rod simulation
		float dt = SimContext.SubstepDeltaTime > 0.0f ? SimContext.SubstepDeltaTime : SimContext.DeltaTime;
		for (auto& rod : Rods)
		{
			if (!rod.IsLODValid(SimContext.SimulationLOD)) continue;

			// Predict positions
			v3dxVector3 gravity = SimContext.Gravity * SimContext.GravityScale;
			XPBDSolver::PredictPositions(rod.Particles, dt, gravity, SimContext.WindForce);

			// Solve rod constraints
			CosseratRodSolver::SolveRod(rod, dt, SimContext.ConstraintIterations);

			// Update velocities
			XPBDSolver::UpdateVelocities(rod.Particles, dt);
			XPBDSolver::ApplyDamping(rod.Particles, dt, SimContext.SpeedScale);

			// Update element orientations from positions
			CosseratRodSolver::UpdateElementOrientations(rod);
		}
	}

	void KawaiiPhysicsContext::ResetDynamics()
	{
		ChainSolver.ResetDynamics();
		ClothSolver.ResetDynamics();
		RibbonSolver.ResetDynamics();
		for (auto& rod : Rods)
		{
			for (auto& P : rod.Particles)
				P.SnapToPose();
			CosseratRodSolver::InitializeRodElements(rod);
		}
	}

	// -----------------------------------------------------------------
	// Results query
	// -----------------------------------------------------------------

	int32_t KawaiiPhysicsContext::GetChainCount() const { return (int32_t)ChainSolver.GetChains().size(); }

	int32_t KawaiiPhysicsContext::GetChainParticleCount(int32_t ChainIndex) const
	{
		const auto& chains = ChainSolver.GetChains();
		if (ChainIndex < 0 || ChainIndex >= (int32_t)chains.size()) return 0;
		return (int32_t)chains[ChainIndex].Particles.size();
	}

	void KawaiiPhysicsContext::GetChainParticlePosition(int32_t ChainIndex, int32_t ParticleIndex,
		float& OutX, float& OutY, float& OutZ) const
	{
		const auto& chains = ChainSolver.GetChains();
		if (ChainIndex < 0 || ChainIndex >= (int32_t)chains.size()) { OutX = OutY = OutZ = 0; return; }
		const auto& particles = chains[ChainIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) { OutX = OutY = OutZ = 0; return; }
		OutX = particles[ParticleIndex].Position.X;
		OutY = particles[ParticleIndex].Position.Y;
		OutZ = particles[ParticleIndex].Position.Z;
	}

	int32_t KawaiiPhysicsContext::GetChainParticleBoneIndex(int32_t ChainIndex, int32_t ParticleIndex) const
	{
		const auto& chains = ChainSolver.GetChains();
		if (ChainIndex < 0 || ChainIndex >= (int32_t)chains.size()) return -1;
		const auto& particles = chains[ChainIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) return -1;
		return particles[ParticleIndex].BoneIndex;
	}

	int32_t KawaiiPhysicsContext::GetClothMeshCount() const { return (int32_t)ClothSolver.GetClothMeshes().size(); }

	int32_t KawaiiPhysicsContext::GetClothChainCount(int32_t MeshIndex) const
	{
		const auto& meshes = ClothSolver.GetClothMeshes();
		if (MeshIndex < 0 || MeshIndex >= (int32_t)meshes.size()) return 0;
		return (int32_t)meshes[MeshIndex].ChainTable.size();
	}

	int32_t KawaiiPhysicsContext::GetClothChainParticleCount(int32_t MeshIndex, int32_t ChainIndex) const
	{
		const auto& meshes = ClothSolver.GetClothMeshes();
		if (MeshIndex < 0 || MeshIndex >= (int32_t)meshes.size()) return 0;
		if (ChainIndex < 0 || ChainIndex >= (int32_t)meshes[MeshIndex].ChainTable.size()) return 0;
		return (int32_t)meshes[MeshIndex].ChainTable[ChainIndex].size();
	}

	void KawaiiPhysicsContext::GetClothParticlePosition(int32_t MeshIndex, int32_t ChainIndex, int32_t ParticleIndex,
		float& OutX, float& OutY, float& OutZ) const
	{
		const auto& meshes = ClothSolver.GetClothMeshes();
		if (MeshIndex < 0 || MeshIndex >= (int32_t)meshes.size()) { OutX = OutY = OutZ = 0; return; }
		if (ChainIndex < 0 || ChainIndex >= (int32_t)meshes[MeshIndex].ChainTable.size()) { OutX = OutY = OutZ = 0; return; }
		const auto& chain = meshes[MeshIndex].ChainTable[ChainIndex];
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)chain.size()) { OutX = OutY = OutZ = 0; return; }
		OutX = chain[ParticleIndex].Position.X;
		OutY = chain[ParticleIndex].Position.Y;
		OutZ = chain[ParticleIndex].Position.Z;
	}

	int32_t KawaiiPhysicsContext::GetClothParticleBoneIndex(int32_t MeshIndex, int32_t ChainIndex, int32_t ParticleIndex) const
	{
		const auto& meshes = ClothSolver.GetClothMeshes();
		if (MeshIndex < 0 || MeshIndex >= (int32_t)meshes.size()) return -1;
		if (ChainIndex < 0 || ChainIndex >= (int32_t)meshes[MeshIndex].ChainTable.size()) return -1;
		const auto& chain = meshes[MeshIndex].ChainTable[ChainIndex];
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)chain.size()) return -1;
		return chain[ParticleIndex].BoneIndex;
	}

	int32_t KawaiiPhysicsContext::GetRodCount() const { return (int32_t)Rods.size(); }

	int32_t KawaiiPhysicsContext::GetRodParticleCount(int32_t RodIndex) const
	{
		if (RodIndex < 0 || RodIndex >= (int32_t)Rods.size()) return 0;
		return (int32_t)Rods[RodIndex].Particles.size();
	}

	void KawaiiPhysicsContext::GetRodParticlePosition(int32_t RodIndex, int32_t ParticleIndex,
		float& OutX, float& OutY, float& OutZ) const
	{
		if (RodIndex < 0 || RodIndex >= (int32_t)Rods.size()) { OutX = OutY = OutZ = 0; return; }
		const auto& particles = Rods[RodIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) { OutX = OutY = OutZ = 0; return; }
		OutX = particles[ParticleIndex].Position.X;
		OutY = particles[ParticleIndex].Position.Y;
		OutZ = particles[ParticleIndex].Position.Z;
	}

	int32_t KawaiiPhysicsContext::GetRodParticleBoneIndex(int32_t RodIndex, int32_t ParticleIndex) const
	{
		if (RodIndex < 0 || RodIndex >= (int32_t)Rods.size()) return -1;
		const auto& particles = Rods[RodIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) return -1;
		return particles[ParticleIndex].BoneIndex;
	}

	int32_t KawaiiPhysicsContext::GetRibbonCount() const { return (int32_t)RibbonSolver.GetRibbons().size(); }

	int32_t KawaiiPhysicsContext::GetRibbonParticleCount(int32_t RibbonIndex) const
	{
		const auto& ribbons = RibbonSolver.GetRibbons();
		if (RibbonIndex < 0 || RibbonIndex >= (int32_t)ribbons.size()) return 0;
		return (int32_t)ribbons[RibbonIndex].Particles.size();
	}

	void KawaiiPhysicsContext::GetRibbonParticlePosition(int32_t RibbonIndex, int32_t ParticleIndex,
		float& OutX, float& OutY, float& OutZ) const
	{
		const auto& ribbons = RibbonSolver.GetRibbons();
		if (RibbonIndex < 0 || RibbonIndex >= (int32_t)ribbons.size()) { OutX = OutY = OutZ = 0; return; }
		const auto& particles = ribbons[RibbonIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) { OutX = OutY = OutZ = 0; return; }
		OutX = particles[ParticleIndex].Position.X;
		OutY = particles[ParticleIndex].Position.Y;
		OutZ = particles[ParticleIndex].Position.Z;
	}

	int32_t KawaiiPhysicsContext::GetRibbonParticleBoneIndex(int32_t RibbonIndex, int32_t ParticleIndex) const
	{
		const auto& ribbons = RibbonSolver.GetRibbons();
		if (RibbonIndex < 0 || RibbonIndex >= (int32_t)ribbons.size()) return -1;
		const auto& particles = ribbons[RibbonIndex].Particles;
		if (ParticleIndex < 0 || ParticleIndex >= (int32_t)particles.size()) return -1;
		return particles[ParticleIndex].BoneIndex;
	}

} // namespace KawaiiPhysics

NS_END
