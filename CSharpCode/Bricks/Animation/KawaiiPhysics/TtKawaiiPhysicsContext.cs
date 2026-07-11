using System;
using System.Collections.Generic;

namespace EngineNS.Bricks.Animation.KawaiiPhysics
{
    /// <summary>
    /// C# wrapper for the native KawaiiPhysicsContext (XPBD bone-driven physics solver).
    /// Manages chain/cloth/rod simulation and colliders via the native C++ implementation.
    /// </summary>
    public class TtKawaiiPhysicsContext : AuxPtrType<EngineNS.KawaiiPhysics.KawaiiPhysicsContext>
    {
        public TtKawaiiPhysicsContext()
        {
            mCoreObject = EngineNS.KawaiiPhysics.KawaiiPhysicsContext.CreateInstance();
        }

        #region Simulation Context

        public float DeltaTime
        {
            set => mCoreObject.SetDeltaTime(value);
        }

        public Vector3 Gravity
        {
            set => mCoreObject.SetGravity(value.X, value.Y, value.Z);
        }

        public float GravityScale
        {
            set => mCoreObject.SetGravityScale(value);
        }

        public void SetWind(in Vector3 windForce, bool enable)
        {
            mCoreObject.SetWind(windForce.X, windForce.Y, windForce.Z, enable);
        }

        public int SimulationLOD
        {
            set => mCoreObject.SetSimulationLOD(value);
        }

        public int ConstraintIterations
        {
            set => mCoreObject.SetConstraintIterations(value);
        }

        public int CollisionSubSteps
        {
            set => mCoreObject.SetCollisionSubSteps(value);
        }

        public float SpeedScale
        {
            set => mCoreObject.SetSpeedScale(value);
        }

        public float MaxSpeed
        {
            set => mCoreObject.SetMaxSpeed(value);
        }

        public float SleepThreshold
        {
            set => mCoreObject.SetSleepThreshold(value);
        }

        public void SetComponentTransform(in Vector3 position, in Quaternion rotation, in Vector3 scale)
        {
            mCoreObject.SetComponentTransform(
                position.X, position.Y, position.Z,
                rotation.X, rotation.Y, rotation.Z, rotation.W,
                scale.X, scale.Y, scale.Z);
        }

        #endregion

        #region Chain Solver

        public void InitializeChains(int count)
        {
            mCoreObject.InitializeChains(count);
        }

        public void SetChainSetup(int index, TtKawaiiChainSetup setup)
        {
            mCoreObject.SetChainSetup(index,
                setup.Name,
                setup.RootBoneIndex, setup.EndBoneIndex,
                setup.TailBoneLength, (int)setup.TailBoneAxis,
                setup.ConstrainBoneLength, setup.BoneLengthConstraintBlend,
                setup.RootCollision, setup.LODThreshold);
            mCoreObject.SetChainPhysicsSettings(index,
                setup.PhysicsSettings,
                setup.PhysicsSettingsRandom);
        }

        public unsafe void BuildChains(Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales, int[] parentIndices)
        {
            int numBones = bonePositions.Length;
            fixed (Vector3* pPos = bonePositions)
            fixed (Quaternion* pRot = boneRotations)
            fixed (Vector3* pScale = boneScales)
            fixed (int* pParent = parentIndices)
            {
                mCoreObject.BuildChains(pPos, pRot, pScale, pParent, numBones);
            }
        }

        public bool ChainSegmentCollision
        {
            set => mCoreObject.SetChainSegmentCollision(value);
        }

        #endregion

        #region Cloth Solver

        public void InitializeClothSetups(int count)
        {
            mCoreObject.InitializeClothSetups(count);
        }

        public void SetClothSetup(int index, TtKawaiiClothSetup setup)
        {
            mCoreObject.SetClothSetup(index,
                setup.Name,
                setup.RootBoneIndex, setup.EndBoneIndex,
                setup.TailBoneLength, (int)setup.TailBoneAxis,
                setup.ConstrainBoneLength, setup.BoneLengthConstraintBlend,
                setup.RootCollision, setup.LODThreshold,
                setup.LoopChains);
            mCoreObject.SetClothPhysicsSettings(index,
                setup.PhysicsSettings,
                setup.PhysicsSettingsRandom);
        }

        public unsafe void BuildCloth(Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales, int[] parentIndices)
        {
            int numBones = bonePositions.Length;
            fixed (Vector3* pPos = bonePositions)
            fixed (Quaternion* pRot = boneRotations)
            fixed (Vector3* pScale = boneScales)
            fixed (int* pParent = parentIndices)
            {
                mCoreObject.BuildCloth(pPos, pRot, pScale, pParent, numBones);
            }
        }

        #endregion

        #region Cosserat Rod Solver

        public void InitializeRods(int count)
        {
            mCoreObject.InitializeRods(count);
        }

        public void SetRodSetup(int index, TtKawaiiRodSetup setup)
        {
            mCoreObject.SetRodSetup(index,
                setup.Name,
                setup.RootBoneIndex, setup.EndBoneIndex,
                setup.StretchShearStiffness, setup.BendTwistStiffness,
                setup.PointAttachStiffness, setup.OrientAttachStiffness,
                setup.LODThreshold);
            mCoreObject.SetRodPhysicsSettings(index,
                setup.PhysicsSettings,
                setup.PhysicsSettingsRandom);
        }

        public unsafe void BuildRods(Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales, int[] parentIndices)
        {
            int numBones = bonePositions.Length;
            fixed (Vector3* pPos = bonePositions)
            fixed (Quaternion* pRot = boneRotations)
            fixed (Vector3* pScale = boneScales)
            fixed (int* pParent = parentIndices)
            {
                mCoreObject.BuildRods(pPos, pRot, pScale, pParent, numBones);
            }
        }

        #endregion

        #region Colliders

        public int AddSphereCollider(in Vector3 center, float radius)
        {
            return mCoreObject.AddSphereCollider(center.X, center.Y, center.Z, radius);
        }

        public int AddCapsuleCollider(in Vector3 center, in Vector3 direction, float radius, float halfLength)
        {
            return mCoreObject.AddCapsuleCollider(center.X, center.Y, center.Z,
                direction.X, direction.Y, direction.Z, radius, halfLength);
        }

        public int AddPlaneCollider(in Vector3 origin, in Vector3 normal)
        {
            return mCoreObject.AddPlaneCollider(origin.X, origin.Y, origin.Z,
                normal.X, normal.Y, normal.Z);
        }

        public int AddBoxCollider(in Vector3 center, in Quaternion rotation, in Vector3 halfExtent)
        {
            return mCoreObject.AddBoxCollider(center.X, center.Y, center.Z,
                rotation.X, rotation.Y, rotation.Z, rotation.W,
                halfExtent.X, halfExtent.Y, halfExtent.Z);
        }

        public void UpdateSphereCollider(int index, in Vector3 center, float radius)
        {
            mCoreObject.UpdateSphereCollider(index, center.X, center.Y, center.Z, radius);
        }

        public void UpdateCapsuleCollider(int index, in Vector3 center, in Vector3 direction, float radius, float halfLength)
        {
            mCoreObject.UpdateCapsuleCollider(index, center.X, center.Y, center.Z,
                direction.X, direction.Y, direction.Z, radius, halfLength);
        }

        public void RemoveCollider(int index)
        {
            mCoreObject.RemoveCollider(index);
        }

        public void ClearColliders()
        {
            mCoreObject.ClearColliders();
        }

        #endregion

        #region Simulation

        public unsafe void UpdatePose(Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales)
        {
            int numBones = bonePositions.Length;
            fixed (Vector3* pPos = bonePositions)
            fixed (Quaternion* pRot = boneRotations)
            fixed (Vector3* pScale = boneScales)
            {
                mCoreObject.UpdatePose(pPos, pRot, pScale, numBones);
            }
        }

        public void Simulate()
        {
            mCoreObject.Simulate();
        }

        public void ResetDynamics()
        {
            mCoreObject.ResetDynamics();
        }

        #endregion

        #region Results Query - Chains

        public int ChainCount => mCoreObject.GetChainCount();

        public int GetChainParticleCount(int chainIndex)
        {
            return mCoreObject.GetChainParticleCount(chainIndex);
        }

        public Vector3 GetChainParticlePosition(int chainIndex, int particleIndex)
        {
            float x = 0, y = 0, z = 0;
            mCoreObject.GetChainParticlePosition(chainIndex, particleIndex, ref x, ref y, ref z);
            return new Vector3(x, y, z);
        }

        public int GetChainParticleBoneIndex(int chainIndex, int particleIndex)
        {
            return mCoreObject.GetChainParticleBoneIndex(chainIndex, particleIndex);
        }

        /// <summary>
        /// Extracts all chain simulation results into the provided bone transform arrays.
        /// Only bones that are driven by the simulation will be modified.
        /// </summary>
        public void ExtractChainResults(Vector3[] outBonePositions)
        {
            int chainCount = ChainCount;
            for (int c = 0; c < chainCount; c++)
            {
                int particleCount = GetChainParticleCount(c);
                for (int p = 0; p < particleCount; p++)
                {
                    int boneIndex = GetChainParticleBoneIndex(c, p);
                    if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                        continue;
                    outBonePositions[boneIndex] = GetChainParticlePosition(c, p);
                }
            }
        }

        #endregion

        #region Results Query - Cloth

        public int ClothMeshCount => mCoreObject.GetClothMeshCount();

        public int GetClothChainCount(int meshIndex)
        {
            return mCoreObject.GetClothChainCount(meshIndex);
        }

        public int GetClothChainParticleCount(int meshIndex, int chainIndex)
        {
            return mCoreObject.GetClothChainParticleCount(meshIndex, chainIndex);
        }

        public Vector3 GetClothParticlePosition(int meshIndex, int chainIndex, int particleIndex)
        {
            float x = 0, y = 0, z = 0;
            mCoreObject.GetClothParticlePosition(meshIndex, chainIndex, particleIndex, ref x, ref y, ref z);
            return new Vector3(x, y, z);
        }

        public int GetClothParticleBoneIndex(int meshIndex, int chainIndex, int particleIndex)
        {
            return mCoreObject.GetClothParticleBoneIndex(meshIndex, chainIndex, particleIndex);
        }

        public void ExtractClothResults(Vector3[] outBonePositions)
        {
            int meshCount = ClothMeshCount;
            for (int m = 0; m < meshCount; m++)
            {
                int chainCount = GetClothChainCount(m);
                for (int c = 0; c < chainCount; c++)
                {
                    int particleCount = GetClothChainParticleCount(m, c);
                    for (int p = 0; p < particleCount; p++)
                    {
                        int boneIndex = GetClothParticleBoneIndex(m, c, p);
                        if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                            continue;
                        outBonePositions[boneIndex] = GetClothParticlePosition(m, c, p);
                    }
                }
            }
        }

        #endregion

        #region Results Query - Rods

        public int RodCount => mCoreObject.GetRodCount();

        public int GetRodParticleCount(int rodIndex)
        {
            return mCoreObject.GetRodParticleCount(rodIndex);
        }

        public Vector3 GetRodParticlePosition(int rodIndex, int particleIndex)
        {
            float x = 0, y = 0, z = 0;
            mCoreObject.GetRodParticlePosition(rodIndex, particleIndex, ref x, ref y, ref z);
            return new Vector3(x, y, z);
        }

        public int GetRodParticleBoneIndex(int rodIndex, int particleIndex)
        {
            return mCoreObject.GetRodParticleBoneIndex(rodIndex, particleIndex);
        }

        public void ExtractRodResults(Vector3[] outBonePositions)
        {
            int rodCount = RodCount;
            for (int r = 0; r < rodCount; r++)
            {
                int particleCount = GetRodParticleCount(r);
                for (int p = 0; p < particleCount; p++)
                {
                    int boneIndex = GetRodParticleBoneIndex(r, p);
                    if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                        continue;
                    outBonePositions[boneIndex] = GetRodParticlePosition(r, p);
                }
            }
        }

        #endregion

        #region Convenience - Extract All Results

        /// <summary>
        /// Extracts all simulation results (chains + cloth + rods) into the provided bone position array.
        /// </summary>
        public void ExtractAllResults(Vector3[] outBonePositions)
        {
            ExtractChainResults(outBonePositions);
            ExtractClothResults(outBonePositions);
            ExtractRodResults(outBonePositions);
        }

        /// <summary>
        /// Extracts all simulation results into both position and a boolean mask indicating
        /// which bone indices were modified by the simulation.
        /// </summary>
        public void ExtractAllResults(Vector3[] outBonePositions, bool[] outModifiedMask)
        {
            ExtractChainResultsWithMask(outBonePositions, outModifiedMask);
            ExtractClothResultsWithMask(outBonePositions, outModifiedMask);
            ExtractRodResultsWithMask(outBonePositions, outModifiedMask);
        }

        private void ExtractChainResultsWithMask(Vector3[] outBonePositions, bool[] outModifiedMask)
        {
            int chainCount = ChainCount;
            for (int c = 0; c < chainCount; c++)
            {
                int particleCount = GetChainParticleCount(c);
                for (int p = 0; p < particleCount; p++)
                {
                    int boneIndex = GetChainParticleBoneIndex(c, p);
                    if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                        continue;
                    outBonePositions[boneIndex] = GetChainParticlePosition(c, p);
                    outModifiedMask[boneIndex] = true;
                }
            }
        }

        private void ExtractClothResultsWithMask(Vector3[] outBonePositions, bool[] outModifiedMask)
        {
            int meshCount = ClothMeshCount;
            for (int m = 0; m < meshCount; m++)
            {
                int chainCount = GetClothChainCount(m);
                for (int c = 0; c < chainCount; c++)
                {
                    int particleCount = GetClothChainParticleCount(m, c);
                    for (int p = 0; p < particleCount; p++)
                    {
                        int boneIndex = GetClothParticleBoneIndex(m, c, p);
                        if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                            continue;
                        outBonePositions[boneIndex] = GetClothParticlePosition(m, c, p);
                        outModifiedMask[boneIndex] = true;
                    }
                }
            }
        }

        private void ExtractRodResultsWithMask(Vector3[] outBonePositions, bool[] outModifiedMask)
        {
            int rodCount = RodCount;
            for (int r = 0; r < rodCount; r++)
            {
                int particleCount = GetRodParticleCount(r);
                for (int p = 0; p < particleCount; p++)
                {
                    int boneIndex = GetRodParticleBoneIndex(r, p);
                    if (boneIndex < 0 || boneIndex >= outBonePositions.Length)
                        continue;
                    outBonePositions[boneIndex] = GetRodParticlePosition(r, p);
                    outModifiedMask[boneIndex] = true;
                }
            }
        }

        #endregion
    }
}
