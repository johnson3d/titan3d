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
        public int Density { get; set; } = 100;
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
    public class UTerrainGrassManager
    {
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
        public class UGrassType
        {
            public UPatch Patch;
            public UTerrainGrass GrassDesc { get; set; }
            public List<UGrassInstance> ObjInstances { get; } = new List<UGrassInstance>();
            public Graphics.Mesh.UMaterialMesh MaterialMesh;
            public Graphics.Mesh.UMesh Mesh;
            public Grass.UMdfGrassStaticMesh InstanceMdf;
            public NxRHI.UCbView GrassCBuffer;
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
                var world = Patch.Level.Level.Node.GetWorld();
                Mesh.SetWorldTransform(meshTrans, world, false);
                Mesh.UpdateCameraOffset(world);

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
        public void AddGrass(UPatch patch, in DVector3 patchOffset, UTerrainGrass grass, in FTransform trans, int capacity)
        {
            UGrassType type;
            if(GrassTypes.TryGetValue(grass, out type) == false)
            {
                type = new UGrassType();
                var task = type.Create(patch, patchOffset, grass);
                GrassTypes.Add(grass, type);
            }
            if(type.CreateFinished == false)
            {
                var tempPatchOffset = patchOffset;
                var tempTrans = trans;
                UEngine.Instance.EventPoster.RunOnUntilFinish((out bool isFinish) =>
                {
                    isFinish = type.CreateFinished;
                    if (isFinish)
                        PushInstance(patch, type, tempPatchOffset, grass, tempTrans, capacity);
                    return null;
                }, Thread.Async.EAsyncTarget.Logic);
            }
            else
                PushInstance(patch, type, patchOffset, grass, trans, capacity);
        }
        public void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            foreach(var i in GrassTypes.Values)
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
