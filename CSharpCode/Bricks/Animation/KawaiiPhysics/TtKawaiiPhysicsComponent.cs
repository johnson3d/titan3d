using System;
using System.Collections.Generic;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;

namespace EngineNS.Bricks.Animation.KawaiiPhysics
{
    /// <summary>
    /// High-level component that manages the full lifecycle of KawaiiPhysics simulation.
    /// Attaches to a skeleton, manages bone-driven colliders, and writes simulation results
    /// back to bone transforms each frame.
    /// 
    /// Usage:
    ///   1. Create component and call Initialize() with chain/cloth/rod setups
    ///   2. AddBoneCollider() for each bone-driven collider
    ///   3. Each frame: UpdateBoneTransforms() -> Tick() -> read results via GetBonePositions()
    /// </summary>
    public class TtKawaiiPhysicsComponent
    {
        private TtKawaiiPhysicsContext mContext;
        private List<BoneColliderBinding> mBoneColliders = new List<BoneColliderBinding>();
        private Vector3[] mBonePositions;
        private Quaternion[] mBoneRotations;
        private Vector3[] mBoneScales;
        private int[] mParentIndices;
        private int mNumBones;
        private bool mInitialized = false;
        private EngineNS.KawaiiPhysics.FKawaiiPhySettings mPhysicsSettings = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettings();
        private EngineNS.KawaiiPhysics.FKawaiiPhySettings mPhysicsSettingsRandom = TtKawaiiPhysicsSetupDefaults.CreatePhysicsSettingsRandom();

        [Rtti.Meta]
        public bool ApplyComponentPhysicsSettings { get; set; } = true;

        [Rtti.Meta]
        public EngineNS.KawaiiPhysics.FKawaiiPhySettings PhysicsSettings
        {
            get => mPhysicsSettings;
            set => mPhysicsSettings = value;
        }

        [Rtti.Meta]
        public EngineNS.KawaiiPhysics.FKawaiiPhySettings PhysicsSettingsRandom
        {
            get => mPhysicsSettingsRandom;
            set => mPhysicsSettingsRandom = value;
        }

        private void ApplyPhysicsSettings(TtKawaiiChainSetup setup)
        {
            if (!ApplyComponentPhysicsSettings || setup == null)
                return;

            setup.PhysicsSettings = mPhysicsSettings;
            setup.PhysicsSettingsRandom = mPhysicsSettingsRandom;
        }

        private void ApplyPhysicsSettings(TtKawaiiRodSetup setup)
        {
            if (!ApplyComponentPhysicsSettings || setup == null)
                return;

            setup.PhysicsSettings = mPhysicsSettings;
            setup.PhysicsSettingsRandom = mPhysicsSettingsRandom;
        }

        private struct BoneColliderBinding
        {
            public int ColliderIndex;
            public int BoneIndex;
            public TtKawaiiColliderDef Definition;
        }

        public TtKawaiiPhysicsContext Context => mContext;
        public bool IsInitialized => mInitialized;

        public TtKawaiiPhysicsComponent()
        {
            mContext = new TtKawaiiPhysicsContext();
        }

        #region Initialization

        /// <summary>
        /// Initialize the physics system with skeleton bone data and simulation setups.
        /// </summary>
        public void Initialize(
            Vector3[] bonePositions,
            Quaternion[] boneRotations,
            Vector3[] boneScales,
            int[] parentIndices,
            TtKawaiiChainSetup[] chainSetups = null,
            TtKawaiiClothSetup[] clothSetups = null,
            TtKawaiiRodSetup[] rodSetups = null)
        {
            mNumBones = bonePositions.Length;
            mBonePositions = new Vector3[mNumBones];
            mBoneRotations = new Quaternion[mNumBones];
            mBoneScales = new Vector3[mNumBones];
            mParentIndices = new int[mNumBones];

            Array.Copy(bonePositions, mBonePositions, mNumBones);
            Array.Copy(boneRotations, mBoneRotations, mNumBones);
            Array.Copy(boneScales, mBoneScales, mNumBones);
            Array.Copy(parentIndices, mParentIndices, mNumBones);

            // Build chains
            if (chainSetups != null && chainSetups.Length > 0)
            {
                mContext.InitializeChains(chainSetups.Length);
                for (int i = 0; i < chainSetups.Length; i++)
                {
                    ApplyPhysicsSettings(chainSetups[i]);
                    mContext.SetChainSetup(i, chainSetups[i]);
                }
                mContext.BuildChains(bonePositions, boneRotations, boneScales, parentIndices);
            }

            // Build cloth
            if (clothSetups != null && clothSetups.Length > 0)
            {
                mContext.InitializeClothSetups(clothSetups.Length);
                for (int i = 0; i < clothSetups.Length; i++)
                {
                    ApplyPhysicsSettings(clothSetups[i]);
                    mContext.SetClothSetup(i, clothSetups[i]);
                }
                mContext.BuildCloth(bonePositions, boneRotations, boneScales, parentIndices);
            }

            // Build rods
            if (rodSetups != null && rodSetups.Length > 0)
            {
                mContext.InitializeRods(rodSetups.Length);
                for (int i = 0; i < rodSetups.Length; i++)
                {
                    ApplyPhysicsSettings(rodSetups[i]);
                    mContext.SetRodSetup(i, rodSetups[i]);
                }
                mContext.BuildRods(bonePositions, boneRotations, boneScales, parentIndices);
            }

            mInitialized = true;
        }

        #endregion

        #region Collider Management

        /// <summary>
        /// Add a bone-driven collider. Returns the collider index for later updates.
        /// </summary>
        public int AddBoneCollider(TtKawaiiColliderDef definition)
        {
            int colliderIndex = -1;
            var shape = definition.Shape;
            var worldPos = ComputeColliderWorldPosition(definition);
            var worldRot = ComputeColliderWorldRotation(definition);

            switch (shape)
            {
                case Graphics.Mesh.PhysicsAsset.TtSphereShape sphere:
                    colliderIndex = mContext.AddSphereCollider(worldPos, sphere.Radius);
                    break;
                case Graphics.Mesh.PhysicsAsset.TtCapsuleShape capsule:
                    var capsuleDir = Vector3.TransformNormal(Vector3.UnitY, Matrix.RotationQuaternion(worldRot));
                    colliderIndex = mContext.AddCapsuleCollider(worldPos, capsuleDir, capsule.Radius, capsule.HalfHeight);
                    break;
                case Graphics.Mesh.PhysicsAsset.TtPlaneShape plane:
                    var planeNormal = Vector3.TransformNormal(plane.PlaneNormal, Matrix.RotationQuaternion(worldRot));
                    colliderIndex = mContext.AddPlaneCollider(worldPos, planeNormal);
                    break;
                case Graphics.Mesh.PhysicsAsset.TtBoxShape box:
                    colliderIndex = mContext.AddBoxCollider(worldPos, worldRot, box.HalfExtent);
                    break;
            }

            if (colliderIndex >= 0)
            {
                mBoneColliders.Add(new BoneColliderBinding
                {
                    ColliderIndex = colliderIndex,
                    BoneIndex = definition.AttachBoneIndex,
                    Definition = definition
                });
            }

            return colliderIndex;
        }

        /// <summary>
        /// Remove a collider by index.
        /// </summary>
        public void RemoveCollider(int colliderIndex)
        {
            mContext.RemoveCollider(colliderIndex);
            mBoneColliders.RemoveAll(b => b.ColliderIndex == colliderIndex);
        }

        /// <summary>
        /// Remove all colliders.
        /// </summary>
        public void ClearColliders()
        {
            mContext.ClearColliders();
            mBoneColliders.Clear();
        }

        private Vector3 ComputeColliderWorldPosition(TtKawaiiColliderDef def)
        {
            var localOffset = def.Shape?.Offset ?? Vector3.Zero;
            if (def.AttachBoneIndex >= 0 && def.AttachBoneIndex < mNumBones)
            {
                var bonePos = mBonePositions[def.AttachBoneIndex];
                var boneRot = mBoneRotations[def.AttachBoneIndex];
                return bonePos + Vector3.TransformNormal(localOffset, Matrix.RotationQuaternion(boneRot));
            }
            return localOffset;
        }

        private Quaternion ComputeColliderWorldRotation(TtKawaiiColliderDef def)
        {
            var localRotation = def.Shape?.Rotation ?? Quaternion.Identity;
            if (def.AttachBoneIndex >= 0 && def.AttachBoneIndex < mNumBones)
                return mBoneRotations[def.AttachBoneIndex] * localRotation;
            return localRotation;
        }

        #endregion

        #region Per-Frame Update

        /// <summary>
        /// Update bone transforms from the animation system before simulation.
        /// </summary>
        public void UpdateBoneTransforms(Vector3[] bonePositions, Quaternion[] boneRotations, Vector3[] boneScales)
        {
            if (!mInitialized) return;

            Array.Copy(bonePositions, mBonePositions, mNumBones);
            Array.Copy(boneRotations, mBoneRotations, mNumBones);
            Array.Copy(boneScales, mBoneScales, mNumBones);
        }

        /// <summary>
        /// Run the simulation for this frame. Call after UpdateBoneTransforms().
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!mInitialized) return;

            mContext.DeltaTime = deltaTime;

            // Update bone-driven colliders
            UpdateBoneColliders();

            // Update pose from current bone transforms
            mContext.UpdatePose(mBonePositions, mBoneRotations, mBoneScales);

            // Run physics
            mContext.Simulate();
        }

        /// <summary>
        /// Write simulation results back into the provided bone position array (position only).
        /// </summary>
        public void WriteBoneResults(Vector3[] outBonePositions)
        {
            if (!mInitialized) return;
            mContext.ExtractAllResults(outBonePositions);
        }

        /// <summary>
        /// Write simulation results back into both position and rotation arrays.
        /// Rotation is derived by comparing the original bone-to-child direction with
        /// the simulated bone-to-child direction, then applying that delta to the
        /// pre-simulation rotation. Tail bones (no children among modified bones)
        /// inherit their parent's rotation delta.
        /// </summary>
        public void WriteBoneResults(Vector3[] outBonePositions, Quaternion[] outBoneRotations)
        {
            if (!mInitialized) return;

            // Snapshot pre-simulation positions for aim direction comparison
            var preSimPositions = new Vector3[mNumBones];
            Array.Copy(mBonePositions, preSimPositions, mNumBones);

            var preSimRotations = new Quaternion[mNumBones];
            Array.Copy(mBoneRotations, preSimRotations, mNumBones);

            // Extract simulated positions and track which bones were modified
            var modifiedMask = new bool[mNumBones];
            mContext.ExtractAllResults(outBonePositions, modifiedMask);

            // Build child list for modified bones only
            var firstChildIndex = new int[mNumBones];
            for (int i = 0; i < mNumBones; i++)
                firstChildIndex[i] = -1;

            // For each modified bone, find its first modified child
            for (int i = 0; i < mNumBones; i++)
            {
                if (!modifiedMask[i]) continue;
                int parentIdx = mParentIndices[i];
                if (parentIdx >= 0 && modifiedMask[parentIdx] && firstChildIndex[parentIdx] < 0)
                {
                    firstChildIndex[parentIdx] = i;
                }
            }

            // Compute rotation deltas
            var rotationDeltas = new Quaternion[mNumBones];
            for (int i = 0; i < mNumBones; i++)
                rotationDeltas[i] = Quaternion.Identity;

            for (int i = 0; i < mNumBones; i++)
            {
                if (!modifiedMask[i]) continue;

                int childIdx = firstChildIndex[i];
                if (childIdx < 0)
                {
                    // Tail bone: inherit parent's rotation delta
                    int parentIdx = mParentIndices[i];
                    if (parentIdx >= 0 && modifiedMask[parentIdx])
                    {
                        rotationDeltas[i] = rotationDeltas[parentIdx];
                    }
                    continue;
                }

                // Compute aim direction before and after simulation
                var dirBefore = preSimPositions[childIdx] - preSimPositions[i];
                var dirAfter = outBonePositions[childIdx] - outBonePositions[i];

                float lenBefore = dirBefore.Length();
                float lenAfter = dirAfter.Length();

                const float epsilon = 1e-6f;
                if (lenBefore < epsilon || lenAfter < epsilon)
                    continue;

                dirBefore /= lenBefore;
                dirAfter /= lenAfter;

                rotationDeltas[i] = ComputeFromToRotation(dirBefore, dirAfter);
            }

            // Apply rotation deltas to pre-simulation rotations
            for (int i = 0; i < mNumBones; i++)
            {
                if (!modifiedMask[i])
                    continue;

                outBoneRotations[i] = rotationDeltas[i] * preSimRotations[i];
                outBoneRotations[i].Normalize();
            }
        }

        /// <summary>
        /// Compute the shortest-arc quaternion that rotates direction 'from' to direction 'to'.
        /// Both inputs must be unit vectors.
        /// </summary>
        private static Quaternion ComputeFromToRotation(in Vector3 from, in Vector3 to)
        {
            float dot = Vector3.Dot(from, to);

            // Nearly identical directions
            if (dot >= 1.0f - 1e-6f)
                return Quaternion.Identity;

            // Nearly opposite directions: pick an arbitrary perpendicular axis
            if (dot <= -1.0f + 1e-6f)
            {
                var fallbackAxis = Vector3.Cross(Vector3.UnitX, from);
                if (fallbackAxis.LengthSquared() < 1e-6f)
                    fallbackAxis = Vector3.Cross(Vector3.UnitY, from);
                fallbackAxis.Normalize();
                return Quaternion.RotationAxis(fallbackAxis, MathF.PI);
            }

            var axis = Vector3.Cross(from, to);
            // q = (cross, 1 + dot), then normalize → shortest arc
            var result = new Quaternion(axis.X, axis.Y, axis.Z, 1.0f + dot);
            result.Normalize();
            return result;
        }

        /// <summary>
        /// Reset all simulation state (call on teleport or large position change).
        /// </summary>
        public void ResetDynamics()
        {
            if (!mInitialized) return;
            mContext.ResetDynamics();
        }

        private void UpdateBoneColliders()
        {
            foreach (var binding in mBoneColliders)
            {
                var def = binding.Definition;
                var worldPos = ComputeColliderWorldPosition(def);
                var worldRot = ComputeColliderWorldRotation(def);

                switch (def.Shape)
                {
                    case Graphics.Mesh.PhysicsAsset.TtSphereShape sphere:
                        mContext.UpdateSphereCollider(binding.ColliderIndex, worldPos, sphere.Radius);
                        break;
                    case Graphics.Mesh.PhysicsAsset.TtCapsuleShape capsule:
                        var capsuleDir = Vector3.TransformNormal(Vector3.UnitY, Matrix.RotationQuaternion(worldRot));
                        mContext.UpdateCapsuleCollider(binding.ColliderIndex, worldPos, capsuleDir, capsule.Radius, capsule.HalfHeight);
                        break;
                }
            }
        }

        #endregion

        #region Configuration Shortcuts

        public void SetGravity(in Vector3 gravity)
        {
            mContext.Gravity = gravity;
        }

        public void SetGravityScale(float scale)
        {
            mContext.GravityScale = scale;
        }

        public void SetWind(in Vector3 windForce, bool enable = true)
        {
            mContext.SetWind(windForce, enable);
        }

        public void SetConstraintIterations(int iterations)
        {
            mContext.ConstraintIterations = iterations;
        }

        public void SetComponentTransform(in Vector3 position, in Quaternion rotation, in Vector3 scale)
        {
            mContext.SetComponentTransform(position, rotation, scale);
        }

        #endregion

        #region RuntimePose Integration

        // Reusable scratch buffers to avoid per-frame allocation
        private Vector3[] mScratchPositions;
        private Quaternion[] mScratchRotations;
        private Vector3[] mScratchScales;

        private void EnsureScratchBuffers(int boneCount)
        {
            if (mScratchPositions == null || mScratchPositions.Length < boneCount)
            {
                mScratchPositions = new Vector3[boneCount];
                mScratchRotations = new Quaternion[boneCount];
                mScratchScales = new Vector3[boneCount];
            }
        }

        /// <summary>
        /// All-in-one update: reads bone transforms from a MeshSpaceRuntimePose,
        /// runs simulation, then writes physics results (position + rotation) back
        /// into the same pose. Call this once per frame after animation evaluation.
        /// </summary>
        /// <param name="meshPose">The MeshSpace pose to read from and write back to.</param>
        /// <param name="deltaTime">Frame delta time in seconds.</param>
        public void UpdateFromMeshPose(TtMeshSpaceRuntimePose meshPose, float deltaTime)
        {
            if (!mInitialized || meshPose == null) return;

            int boneCount = meshPose.Transforms.Count;
            EnsureScratchBuffers(boneCount);

            // 1. Extract transforms from pose into flat arrays
            for (int i = 0; i < boneCount; i++)
            {
                var transform = meshPose.Transforms[i];
                mScratchPositions[i] = transform.Position.ToSingleVector3();
                mScratchRotations[i] = transform.Quat;
                mScratchScales[i] = transform.Scale;
            }

            // 2. Feed into physics component
            UpdateBoneTransforms(mScratchPositions, mScratchRotations, mScratchScales);

            // 3. Simulate
            Tick(deltaTime);

            // 4. Write back results (position + rotation)
            WriteBoneResults(mScratchPositions, mScratchRotations);

            // 5. Apply results back to the pose
            for (int i = 0; i < boneCount; i++)
            {
                var transform = meshPose.Transforms[i];
                transform.Position = mScratchPositions[i].AsDVector();
                transform.Quat = mScratchRotations[i];
                meshPose.Transforms[i] = transform;
            }
        }

        #endregion
    }
}
