﻿using EngineNS.Bricks.Procedure;
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
        public FTransform Transform;
        public int MaterialIdx;
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
        public Rtti.TtTypeDesc ElementType => Rtti.TtTypeDescGetter<FGrassTransformData>.TypeDesc;
        public Rtti.TtTypeDesc BufferType => Rtti.TtTypeDescGetter<USuperBuffer<FGrassTransformData, FGrassTransformDataOperator>>.TypeDesc;
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

        public unsafe void Add(TtTypeDesc resultType, void* result, TtTypeDesc leftType, void* left, TtTypeDesc rightType, void* right)
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

        public unsafe void Copy(TtTypeDesc tarTyp, void* tar, TtTypeDesc srcType, void* src)
        {
            (*(FGrassTransformData*)tar) = (*(FGrassTransformData*)src);
        }

        public unsafe void Div(TtTypeDesc resultType, void* result, TtTypeDesc leftType, void* left, TtTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.TtTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
                return;

            FGrassTransformData rValue = FGrassTransformData.Identity;
            if (right != (void*)0)
                rValue = *(FGrassTransformData*)right;
            FTransform.Multiply(out (*(FGrassTransformData*)result).Transform, in (*(FGrassTransformData*)left).Transform, in (*(FGrassTransformData*)right).Transform);
        }

        public unsafe void Lerp(TtTypeDesc resultType, void* result, TtTypeDesc leftType, void* left, TtTypeDesc rightType, void* right, float factor)
        {
            if (resultType != Rtti.TtTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
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

        public unsafe void Mul(TtTypeDesc resultType, void* result, TtTypeDesc leftType, void* left, TtTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.TtTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
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

        public unsafe void SetIfGreateThan(TtTypeDesc tarTyp, void* tar, TtTypeDesc srcType, void* src)
        {
        }

        public unsafe void SetIfLessThan(TtTypeDesc tarTyp, void* tar, TtTypeDesc srcType, void* src)
        {
        }

        public unsafe void Sub(TtTypeDesc resultType, void* result, TtTypeDesc leftType, void* left, TtTypeDesc rightType, void* right)
        {
            if (resultType != Rtti.TtTypeDescGetter<FGrassTransformData>.TypeDesc && resultType != leftType && resultType != rightType)
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
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
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
            public TtPatch Patch;
            public UTerrainGrass GrassDesc { get; set; }
            public List<UGrassInstance> ObjInstances { get; } = new List<UGrassInstance>();
            public Graphics.Mesh.TtMaterialMesh MaterialMesh;
            public Graphics.Mesh.TtMesh Mesh;
            public Grass.UMdfGrassStaticMesh InstanceMdf;
            public NxRHI.TtCbView GrassCBuffer;
            public int RandomSeed;
            public int WeightStride;
            public byte[] WeightMap;
            public bool CreateFinished = false;

            //public async System.Threading.Tasks.Task Create(TtPatch patch, DVector3 patchOffset, UTerrainGrass desc)
            //{
            //    Patch = patch;
            //    GrassDesc = desc;
            //    MaterialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
            //    Mesh = new Graphics.Mesh.TtMesh();
            //    if (desc.FollowHeight)
            //        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
            //    else
            //        Mesh.Initialize(MaterialMesh, Rtti.UTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);

            //    Mesh.IsAcceptShadow = !desc.NoShadow;
            //    Mesh.IsCastShadow = false;
            //    InstanceMdf = Mesh.MdfQueue as Grass.UMdfGrassStaticMesh;
            //    InstanceMdf.GrassModifier.GrassType = this;

            //    Mesh.IsDrawHitproxy = false;
            //    var meshTrans = FTransform.CreateTransform(in patchOffset, in Vector3.One, in Quaternion.Identity);
            //    var world = patch.Level.Level.Node.GetWorld();
            //    Mesh.SetWorldTransform(meshTrans, world, false);
            //    Mesh.UpdateCameraOffset(world);

            //    CreateFinished = true;
            //}
            public async System.Threading.Tasks.Task Create(TtPatch patch, DVector3 patchOffset, byte[] weights, int weightStride, UTerrainGrass desc)
            {
                Patch = patch;
                GrassDesc = desc;
                WeightMap = weights;
                WeightStride = weightStride;
                MaterialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
                if (MaterialMesh == null)
                {
                    CreateFinished = true;
                    return;
                }
                Mesh = new Graphics.Mesh.TtMesh();
                if (desc.FollowHeight)
                    Mesh.Initialize(MaterialMesh, Rtti.TtTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Grass.UMdf_Grass_VertexFollowHeight>>.TypeDesc);
                else
                    Mesh.Initialize(MaterialMesh, Rtti.TtTypeDescGetter<Grass.UMdfGrassStaticMeshPermutation<Grass.UMdf_Grass_VertexNotFollowHeight>>.TypeDesc);
                
                Mesh.IsAcceptShadow = !desc.NoShadow;
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
            public void UpdateAABB()
            {//todo update aabb for mesh
                MaterialMesh.AABB.InitEmptyBox();
                foreach (var i in ObjInstances)
                {
                    //MaterialMesh.AABB.Merge(i.Data.Pos)
                    //MaterialMesh.AABB = BoundingBox.Merge(in MaterialMesh.AABB, );
                }
            }
        }
        public Dictionary<UTerrainGrass, UGrassType> GrassTypes = new Dictionary<UTerrainGrass, UGrassType>();
        public uint CameralOffsetSerialId = 0;
        TtPatch mHostPatch;
        int mCurrentPatchLOD;

        public UTerrainGrassManager(TtPatch patch)
        {
            mHostPatch = patch;
            mCurrentPatchLOD = mHostPatch.CurrentLOD;
        }

        //void PushInstance(TtPatch patch, UGrassType type, in DVector3 patchOffset, UTerrainGrass grass, in FTransform trans, int capacity)
        //{
        //    var patchSize = patch.Level.Level.Node.PatchSize;
        //    var inst = new UGrassInstance();
        //    var pos = (trans.Position - patchOffset).ToSingleVector3();
        //    UInt32 xPos = (UInt32)(pos.X * 4096.0f / patchSize);
        //    UInt32 zPos = (UInt32)(pos.Z * 4096.0f / patchSize);
        //    UInt32 rot = (UInt32)(trans.Quat.GetAngleWithAxis(Vector3.Up) * 7.5f / System.Math.PI);
        //    UInt32 scale = (UInt32)((trans.Scale.Y - type.GrassDesc.MinScale) * 15.0f / type.GrassDesc.GetScaleRange());
        //    inst.Data = (xPos << 20) + (zPos << 8) + (rot << 4) + scale;
        //    inst.TerrainHeight = pos.Y;
        //    //inst.GrassPosition = pos;
        //    //inst.GrassScale = trans.Scale.Y;
        //    //inst.GrassQuat = trans.Quat;
        //    type.PushInstance(inst, capacity);
        //}
        //public void AddGrass(in DVector3 patchOffset, UTerrainGrass grass, in FTransform trans, int capacity)
        //{
        //    UGrassType type;
        //    if(GrassTypes.TryGetValue(grass, out type) == false)
        //    {
        //        type = new UGrassType();
        //        var task = type.Create(mHostPatch, patchOffset, grass);
        //        GrassTypes.Add(grass, type);
        //    }
        //    if(type.CreateFinished == false)
        //    {
        //        var tempPatchOffset = patchOffset;
        //        var tempTrans = trans;
        //        TtEngine.Instance.EventPoster.RunOnUntilFinish((Thread.Async.TtAsyncTaskStateBase state) =>
        //        {
        //            var isFinish = type.CreateFinished;
        //            if (isFinish)
        //                PushInstance(mHostPatch, type, tempPatchOffset, grass, tempTrans, capacity);
        //            return isFinish;
        //        }, Thread.Async.EAsyncTarget.Logic);
        //    }
        //    else
        //        PushInstance(mHostPatch, type, patchOffset, grass, trans, capacity);
        //}

        struct GrassInsData
        {
            public UInt32 Data;
            public float TerrainHeight;
        }
        void UpdateGrass(TtPatch patch, UGrassType type)
        {
            if (type.WeightMap == null || type.WeightStride == 0)
                return;

            var randObj = new Support.TtRandom();
            randObj.mCoreObject.SetSeed(type.RandomSeed);
            var terrainNode = patch.Level.GetTerrainNode();
            var patchSize = terrainNode.PatchSize;
            var weightStride = type.WeightStride;
            int maxGrassStride = (int)Math.Ceiling(Math.Abs(type.GrassDesc.Density * patchSize));
            var div = 1.0f / maxGrassStride;
            var patchPosX = patch.IndexX * patchSize;
            var patchPosZ = patch.IndexZ * patchSize;
            Support.TtNativeArray<GrassInsData> tempArray = Support.TtNativeArray<GrassInsData>.CreateInstance();
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
        public void AddGrass(in DVector3 patchOffset, UTerrainGrass grass, Procedure.UBufferComponent weights, float weightMin, float weightMax)
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
                GrassTypes.Add(grass, type);
                var p_offset = patchOffset;
                Action action = async () =>
                {
                    await type.Create(mHostPatch, p_offset, weightDatas, weightStride, grass);
                    UpdateGrass(mHostPatch, type);
                };
                action();
            }
            
            if(type.CreateFinished)
            {
                UpdateGrass(mHostPatch, type);
            }
        }
        public void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
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

                rp.AddVisibleMesh(i.Mesh);
            }
            CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
        }
    }
}
