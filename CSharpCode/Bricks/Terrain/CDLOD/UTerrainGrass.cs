using EngineNS.Bricks.Procedure;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    [NodeGraph.Expandable("PGC")]
    [BufferTypeOperator(typeof(FGrassTransformDataOperator))]
    public struct FGrassTransformData
    {
        [Rtti.Meta]
        public FTransform Transform;
        [Rtti.Meta]
        public int MaterialIdx;
        [Rtti.Meta]
        public int GrassIdx;

        public void Reset()
        {
            Transform = FTransform.Identity;
            MaterialIdx = -1;
            GrassIdx = -1;
        }
        public readonly static FGrassTransformData Identity = FGrassTransformData.Create();

        public static FGrassTransformData Create()
        {
            var retVal = new FGrassTransformData();
            retVal.Reset();
            return retVal;
        }
    }
    public struct FGrassTransformDataOperator : Procedure.ISuperPixelOperator<FGrassTransformData>
    {
        public Rtti.UTypeDesc ElementType => Rtti.UTypeDescGetter<FGrassTransformData>.TypeDesc;
        public Rtti.UTypeDesc BufferType => Rtti.UTypeDescGetter<USuperBuffer<FGrassTransformData, FGrassTransformDataOperator>>.TypeDesc;
        public FGrassTransformData MaxValue => FGrassTransformData.Identity;
        public FGrassTransformData MinValue => FGrassTransformData.Identity;

        public unsafe void Abs(void* result, void* left)
        {
            throw new NotImplementedException();
        }

        public FGrassTransformData Add(in FGrassTransformData left, in FGrassTransformData right)
        {
            throw new NotImplementedException();
        }

        public unsafe void Add(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
            throw new NotImplementedException();
        }

        public unsafe int Compare(void* left, void* right)
        {
            ref var l = ref *(FGrassTransformData*)left;
            ref var r = ref *(FGrassTransformData*)right;
            return Compare(in l, in r);
        }
        public int Compare(in FGrassTransformData left, in FGrassTransformData right)
        {
            return 0;
        }

        public unsafe void Copy(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
            (*(FGrassTransformData*)tar) = (*(FGrassTransformData*)src);
        }

        public unsafe void Div(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
                return;

            FGrassTransformData rValue = FGrassTransformData.Identity;
            if (right != (void*)0)
                rValue = *(FGrassTransformData*)right;
            FTransform.Multiply(out (*(FGrassTransformData*)result).Transform, in (*(FGrassTransformData*)left).Transform, in (*(FGrassTransformData*)right).Transform);
        }

        public unsafe void Lerp(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.UTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            var rValue = FGrassTransformData.Identity;
            if (right != (void*)0)
                rValue = *(FGrassTransformData*)right;

            (*(FGrassTransformData*)result).Transform = (*(FGrassTransformData*)left).Transform;
            ((FGrassTransformData*)result)->Transform.BlendWith(in rValue.Transform, factor);
        }

        public unsafe void Max(void* result, void* left, void* right)
        {
        }

        public unsafe void Min(void* result, void* left, void* right)
        {
        }

        public unsafe void Mul(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
                return;

            var rValue = FGrassTransformData.Identity;
            if (right != (void*)0)
                rValue = *(FGrassTransformData*)right;
            FTransform.Multiply(out (*(FGrassTransformData*)result).Transform, in (*(FGrassTransformData*)left).Transform, in (*(FGrassTransformData*)right).Transform);
        }

        public unsafe void SetAsMaxValue(void* tar)
        {
            *(FGrassTransformData*)tar = FGrassTransformData.Identity;
        }

        public unsafe void SetAsMinValue(void* tar)
        {
            *(FGrassTransformData*)tar = FGrassTransformData.Identity;
        }

        public unsafe void SetIfGreateThan(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
        }

        public unsafe void SetIfLessThan(UTypeDesc tarTyp, void* tar, UTypeDesc srcType, void* src)
        {
        }

        public unsafe void Sub(UTypeDesc resultType, void* result, UTypeDesc leftType, void* left, UTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.UTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
                return;
            var rValue = FGrassTransformData.Identity;
            if (right != (void*)0)
                rValue = *(FGrassTransformData*)right;
            FTransform.Multiply(out (*(FGrassTransformData*)result).Transform, in (*(FGrassTransformData*)left).Transform, in (*(FGrassTransformData*)right).Transform);
        }
    }

    public class UTerrainGrass : IO.BaseSerializer
    {
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        public RName MeshName { get; set; }

        [Rtti.Meta]
        public float MinScale { get; set; } = 0.5f;
        [Rtti.Meta]
        public float MaxScale { get; set; } = 2.0f;

        [Rtti.Meta]
        public float Density { get; set; } = 1;
        [Rtti.Meta]
        public bool RandomRotation { get; set; } = true;
        [Rtti.Meta]
        public bool FollowHeight { get; set; } = true;
        [Rtti.Meta]
        public bool NoShadow { get; set; } = true;

        public float GetScaleRange()
        {
            return MaxScale - MinScale;
        }
    }
    public class UTerrainGrassManager : IDisposable
    {
        public void Dispose()
        {
            foreach(var i in GrassTypes)
            {
                i.Value.Dispose();
            }
            GrassTypes.Clear();
        }
        public class UGrassInstance
        {
            public UGrassType GrassType;
            public UInt32 Data;
            public float TerrainHeight;
            // test only /////////////////////////////////////
            //public Vector3 GrassPosition;
            //public float GrassScale;
            //public Quaternion GrassQuat;
            //////////////////////////////////////////////////
            public uint InstanceIndex = uint.MaxValue;

            public UGrassInstance()
            {
                
            }
        }
        public class UGrassType : IDisposable
        {
            public void Dispose()
            {
                CoreSDK.DisposeObject(ref Mesh);
                CoreSDK.DisposeObject(ref GrassCBuffer);
            }
            public UPatch Patch;
            public UTerrainGrass GrassDesc { get; set; }
            public List<UGrassInstance> ObjInstances { get; } = new List<UGrassInstance>();
            public Graphics.Mesh.UMaterialMesh MaterialMesh;
            public Graphics.Mesh.UMesh Mesh;
            public Grass.UMdfGrassStaticMesh InstanceMdf;
            public NxRHI.UCbView GrassCBuffer;
            public int RandomSeed;
            public int WeightStride;
            public byte[] WeightMap;
            public bool CreateFinished = false;

            public async System.Threading.Tasks.Task Create(UPatch patch, DVector3 patchOffset, UTerrainGrass desc)
            {
                Patch = patch;
                GrassDesc = desc;
                MaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
                Mesh = new Graphics.Mesh.UMesh();
                if(desc.NoShadow)
                {
                    if (desc.FollowHeight)
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
                    else
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);
                }
                else
                {
                    if (desc.FollowHeight)
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
                    else
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);
                }

                Mesh.IsCastShadow = true;
                InstanceMdf = Mesh.MdfQueue as Grass.UMdfGrassStaticMesh;
                InstanceMdf.GrassModifier.GrassType = this;

                Mesh.IsDrawHitproxy = false;
                var meshTrans = FTransform.CreateTransform(in patchOffset, in Vector3.One, in Quaternion.Identity);
                var world = patch.Level.Level.Node.GetWorld();
                Mesh.SetWorldTransform(meshTrans, world, false);
                Mesh.UpdateCameraOffset(world);

                CreateFinished = true;
            }
            public async System.Threading.Tasks.Task Create(UPatch patch, DVector3 patchOffset, byte[] weights, int weightStride, UTerrainGrass desc)
            {
                Patch = patch;
                GrassDesc = desc;
                WeightMap = weights;
                WeightStride = weightStride;
                MaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
                if (MaterialMesh == null)
                {
                    CreateFinished = true;
                    return;
                }
                Mesh = new Graphics.Mesh.UMesh();
                if (desc.NoShadow)
                {
                    if (desc.FollowHeight)
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
                    else
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);
                }
                else
                {
                    if (desc.FollowHeight)
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
                    else
                        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);
                }

                Mesh.IsCastShadow = true;
                InstanceMdf = Mesh.MdfQueue as Grass.UMdfGrassStaticMesh;
                InstanceMdf.GrassModifier.GrassType = this;

                Mesh.IsDrawHitproxy = false;
                var meshTrans = FTransform.CreateTransform(in patchOffset, in Vector3.One, in Quaternion.Identity);
                var world = patch.Level.Level.Node.GetWorld();
                if (world != null)
                {
                    Mesh.SetWorldTransform(meshTrans, world, false);
                    Mesh.UpdateCameraOffset(world);
                }

                CreateFinished = true;
            }
            public void PushInstance(UGrassInstance obj, int capacity)
            {
                obj.GrassType = this;
                if (InstanceMdf == null)
                    return;
                InstanceMdf.GrassModifier.SureBuffers((uint)capacity);
                ObjInstances.Add(obj);
                obj.InstanceIndex = InstanceMdf.GrassModifier.PushInstance(obj.Data, obj.TerrainHeight);
            }
        }
        public Dictionary<UTerrainGrass, UGrassType> GrassTypes = new Dictionary<UTerrainGrass, UGrassType>();
        public uint CameralOffsetSerialId = 0;
        UPatch mHostPatch;
        int mCurrentPatchLOD;

        public UTerrainGrassManager(UPatch patch)
        {
            mHostPatch = patch;
            mCurrentPatchLOD = mHostPatch.CurrentLOD;
        }

        void PushInstance(UPatch patch, UGrassType type, in DVector3 patchOffset, UTerrainGrass grass, in FTransform trans, int capacity)
        {
            var patchSize = patch.Level.Level.Node.PatchSize;
            var inst = new UGrassInstance();
            var pos = (trans.Position - patchOffset).ToSingleVector3();
            UInt32 xPos = (UInt32)(pos.X * 4096.0f / patchSize);
            UInt32 zPos = (UInt32)(pos.Z * 4096.0f / patchSize);
            UInt32 rot = (UInt32)(trans.Quat.GetAngleWithAxis(Vector3.Up) * 7.5f / System.Math.PI);
            UInt32 scale = (UInt32)((trans.Scale.Y - type.GrassDesc.MinScale) * 15.0f / type.GrassDesc.GetScaleRange());
            inst.Data = (xPos << 20) + (zPos << 8) + (rot << 4) + scale;
            inst.TerrainHeight = pos.Y;
            //inst.GrassPosition = pos;
            //inst.GrassScale = trans.Scale.Y;
            //inst.GrassQuat = trans.Quat;
            type.PushInstance(inst, capacity);
        }
        public void AddGrass(in DVector3 patchOffset, UTerrainGrass grass, in FTransform trans, int capacity)
        {
            UGrassType type;
            if(GrassTypes.TryGetValue(grass, out type) == false)
            {
                type = new UGrassType();
                var task = type.Create(mHostPatch, patchOffset, grass);
                GrassTypes.Add(grass, type);
            }
            if(type.CreateFinished == false)
            {
                var tempPatchOffset = patchOffset;
                var tempTrans = trans;
                UEngine.Instance.EventPoster.RunOnUntilFinish((out bool isFinish, Thread.Async.TtAsyncTaskStateBase state) =>
                {
                    isFinish = type.CreateFinished;
                    if (isFinish)
                        PushInstance(mHostPatch, type, tempPatchOffset, grass, tempTrans, capacity);
                    return true;
                }, Thread.Async.EAsyncTarget.Logic);
            }
            else
                PushInstance(mHostPatch, type, patchOffset, grass, trans, capacity);
        }

        struct GrassInsData
        {
            public UInt32 Data;
            public float TerrainHeight;
        }
        void UpdateGrass(UPatch patch, UGrassType type)
        {
            if (type.WeightMap == null || type.WeightStride == 0)
                return;

            var randObj = new Support.URandom();
            randObj.mCoreObject.SetSeed(type.RandomSeed);
            var terrainNode = patch.Level.GetTerrainNode();
            var patchSize = terrainNode.PatchSize;
            var weightStride = type.WeightStride;
            int maxGrassStride = (int)Math.Ceiling(Math.Abs(type.GrassDesc.Density * patchSize));
            var div = 1.0f / maxGrassStride;
            var patchPosX = patch.IndexX * patchSize;
            var patchPosZ = patch.IndexZ * patchSize;
            Support.UNativeArray<GrassInsData> tempArray = Support.UNativeArray<GrassInsData>.CreateInstance();
            for (int x=0; x< maxGrassStride; x++)
            {
                for(int z=0; z< maxGrassStride; z++)
                {
                    var posX = (x + randObj.GetUnit()) * div * patchSize;
                    var posZ = (z + randObj.GetUnit()) * div * patchSize;

                    var u = posX / patchSize;
                    var v = posZ / patchSize;
                    var tPX = (int)Math.Clamp(weightStride * u, 0, weightStride - 1);
                    var tPZ = (int)Math.Clamp(weightStride * v, 0, weightStride - 1);

                    var weight = type.WeightMap[tPZ * weightStride + tPX];
                    var lodDelta = 1 - (float)patch.CurrentLOD / patch.TerrainMesh.Length;
                    weight = (byte)(weight * lodDelta);
                    if(weight > 0.0f && weight >= randObj.GetNextByte())
                    {
                        try
                        {
                            var posY = patch.Level.GetAltitude(posX + patchPosX, posZ + patchPosZ, terrainNode.GridTileSize);// terrainNode.GridSize);
                            var inst = new GrassInsData();
                            UInt32 xPos = (UInt32)(posX * 4096.0f / patchSize);
                            UInt32 zPos = (UInt32)(posZ * 4096.0f / patchSize);
                            UInt32 rot = 0;
                            if(type.GrassDesc.RandomRotation)
                                rot = (UInt32)(randObj.GetNextByte() / 16);
                            UInt32 scale = (UInt32)(randObj.GetUnit() * 16);
                            inst.Data = (xPos << 20) + (zPos << 8) + (rot << 4) + scale;
                            inst.TerrainHeight = posY;
                            tempArray.Add(inst);
                        }
                        catch(System.Exception)
                        {

                        }
                    }
                }
            }

            if(tempArray.Count > 0)
            {
                foreach (var data in tempArray)
                {
                    var inst = new UGrassInstance();
                    inst.Data = data.Data;
                    inst.TerrainHeight = data.TerrainHeight;
                    type.PushInstance(inst, tempArray.Count);
                }
            }

            tempArray.Dispose();
        }
        public void AddGrass(in DVector3 patchOffset, UTerrainGrass grass, Procedure.UBufferConponent weights, float weightMin, float weightMax)
        {
            var patchSize = mHostPatch.Level.GetTerrainNode().PatchSize;
            var weightStride = (int)Math.Ceiling(patchSize);
            var weightDatas = new byte[weightStride * weightStride];
            var levelSize = mHostPatch.Level.Level.PatchSide * patchSize;
            float weightRange = weightMax - weightMin;
            if(weightRange == 0)
                weightRange = 1;
            for(int y=0; y<weightStride; y++)
            {
                for(int x=0; x<weightStride; x++)
                {
                    if (weightRange == 1)
                        weightDatas[y * weightStride + x] = byte.MaxValue;
                    else
                    {
                        var uvw = new Vector3(
                            ((float)x / weightStride * patchSize + (float)(patchOffset.X - mHostPatch.Level.Level.StartPosition.X)) / levelSize, 
                            ((float)y / weightStride * patchSize + (float)(patchOffset.Z - mHostPatch.Level.Level.StartPosition.Z)) / levelSize, 0);
                        weightDatas[y * weightStride + x] = (byte)((weights.GetPixel<float>(uvw) - weightMin) / weightRange * byte.MaxValue);
                    }
                }
            }

            UGrassType type;
            if(GrassTypes.TryGetValue(grass, out type) == false)
            {
                type = new UGrassType();
                var task = type.Create(mHostPatch, patchOffset, weightDatas, weightStride, grass);
                GrassTypes.Add(grass, type);
            }
            if(type.CreateFinished == false)
            {
                UEngine.Instance.EventPoster.RunOnUntilFinish((out bool isFinish, Thread.Async.TtAsyncTaskStateBase state) =>
                {
                    isFinish = type.CreateFinished;
                    if(isFinish)
                        UpdateGrass(mHostPatch, type);
                    return true;
                }, Thread.Async.EAsyncTarget.Logic);
            }
            else
                UpdateGrass(mHostPatch, type);
        }
        public void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            if(mCurrentPatchLOD != mHostPatch.CurrentLOD)
            {
                foreach (var type in GrassTypes.Values)
                {
                    if (type.CreateFinished == false)
                        continue;
                    type.InstanceMdf.GrassModifier.ResetCurNumber();
                    UpdateGrass(mHostPatch, type);
                }
                mCurrentPatchLOD = mHostPatch.CurrentLOD;
            }

            foreach (var i in GrassTypes.Values)
            {
                if (i.Mesh == null)
                    continue;
                if (i.InstanceMdf.GrassModifier.CurNumber == 0)
                    continue;
                if(rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
                {
                    i.Mesh.UpdateCameraOffset(rp.World);
                }

                rp.VisibleMeshes.Add(i.Mesh);
            }
            CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
        }
    }
}
