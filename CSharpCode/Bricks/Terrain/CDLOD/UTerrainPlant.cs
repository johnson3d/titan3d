using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Terrain.CDLOD
{
    public class UTerrainPlant : IO.BaseSerializer
    {
        public override string ToString()
        {
            var result = "";
            if (MeshName != null)
                result += MeshName.Name + MeshName.RNameType.ToString();
            result += MinScale;
            result += MaxScale;
            result += MaxBiasAngle;
            result += Density;
            return result;
        }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
        public RName MeshName { get; set; }
        [Rtti.Meta]
        public float MinScale { get; set; } = 0.8f;
        [Rtti.Meta]
        public float MaxScale { get; set; } = 2.0f;
        [Rtti.Meta]
        public float MaxBiasAngle { get; set; }
        [Rtti.Meta]
        public int Density { get; set; }
    }
    public class UTerainPlantManager
    {
        public class UPlantInstance : GamePlay.Scene.UNode //Graphics.Pipeline.IProxiable
        {
            public UPlantInstance()
            {
                mPlacement = new GamePlay.UPlacement();
                mPlacement.HostNode = this;
            }
            GamePlay.UPlacement mPlacement;
            public override GamePlay.UPlacementBase Placement
            {
                get { return mPlacement; }
            }
            protected unsafe override void OnAbsTransformChanged()
            {
                if (PlantType == null)
                    return;
                var pos = (Placement.Position - PlantType.InstanceOffset).ToSingleVector3();
                var scale = Placement.Scale;
                var quat = Placement.Quat;
                PlantType.InstanceMdf.InstanceModifier.SetInstance(InstanceIndex, &pos, &scale, &quat, (UInt32_4*)IntPtr.Zero.ToPointer(), (uint*)IntPtr.Zero.ToPointer());
                DebugHitproxyMesh?.SetWorldTransform(Placement.TransformData, PlantType.Terrain.GetWorld(), false);
            }
            public override void OnHitProxyChanged()
            {
                base.OnHitProxyChanged();
            }
            public override void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
            {
                if (DebugHitproxyMesh == null)
                {
                    DebugHitproxyMesh = new Graphics.Mesh.UMesh();
                    DebugHitproxyMesh.Initialize(PlantType.MaterialMesh,
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                    DebugHitproxyMesh.SetWorldTransform(Placement.TransformData, PlantType.Terrain.GetWorld(), false);
                }
                meshes.Add(DebugHitproxyMesh);
            }
            public UPlantType PlantType;
            public uint InstanceIndex = uint.MaxValue;
            //public FTransform Transform = FTransform.Identity;
            public Graphics.Mesh.UMesh DebugHitproxyMesh;
        }
        public class UPlantType
        {
            public Bricks.Terrain.CDLOD.UTerrainNode Terrain;
            public UTerrainPlant PlantDesc { get; set; }
            public List<UPlantInstance> ObjInstances { get; } = new List<UPlantInstance>();
            public Graphics.Mesh.UMaterialMesh MaterialMesh;
            public Graphics.Mesh.UMesh Mesh;
            public Graphics.Mesh.UMdfInstanceStaticMesh InstanceMdf;
            public DVector3 InstanceOffset;
            public bool CreateFinished = false;
            public async System.Threading.Tasks.Task Create(Bricks.Terrain.CDLOD.UTerrainNode trn, DVector3 levelOffset, UTerrainPlant desc)
            {
                Terrain = trn;
                PlantDesc = desc;
                MaterialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
                Mesh = new Graphics.Mesh.UMesh();
                Mesh.Initialize(MaterialMesh, 
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfInstanceStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);

                Mesh.IsCastShadow = true;
                InstanceMdf = Mesh.MdfQueue as Graphics.Mesh.UMdfInstanceStaticMesh;

                Mesh.IsDrawHitproxy = true;
                InstanceOffset = levelOffset;
                var meshTrans = FTransform.CreateTransform(in InstanceOffset, in Vector3.One, in Quaternion.Identity);
                Mesh.SetWorldTransform(meshTrans, trn.GetWorld(), false);
                //HitProxy.ConvertHitProxyIdToVector4();
                //Mesh.SetHitproxy(in value);

                CreateFinished = true;
            }
            public void PushInstance(UPlantInstance obj, int capacity)
            {
                obj.PlantType = this;
                if (InstanceMdf == null)
                    return;
                InstanceMdf.InstanceModifier.SureBuffers((uint)capacity);
                UEngine.Instance.GfxDevice.HitproxyManager.MapProxy(obj);
                ObjInstances.Add(obj);

                var pos = (obj.Placement.Position - InstanceOffset).ToSingleVector3();
                obj.InstanceIndex = InstanceMdf.InstanceModifier.PushInstance(in pos, obj.Placement.Scale, obj.Placement.Quat, in UInt32_4.Zero, obj.HitProxy.ProxyId);
                //InstanceMdf.InstanceModifier.PushInstance(obj.Transform.mPosition.ToSingleVector3(), in obj.Transform.mScale, in obj.Transform.mQuat, in UInt32_4.Zero, obj.HitProxy.ProxyId);
            }
            public void OnHitProxyChanged()
            {
                if (Mesh == null)
                    return;
            }
        }
        public Dictionary<UTerrainPlant, UPlantType> PlantTypes = new Dictionary<UTerrainPlant, UPlantType>();
        public uint CameralOffsetSerialId = 0;
        public async System.Threading.Tasks.Task Initialize(Procedure.Node.UMaterialIdMapNode matIdMap)
        {
            //await matIdMap.SureMaterialResources();
            //foreach (var i in matIdMap.MaterialIdManager.MaterialIdArray)
            //{
            //    foreach (var j in i.Plants)
            //    {
            //        await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(j.MeshName);
            //    }
            //}
        }
        public void AddPlant(Bricks.Terrain.CDLOD.UTerrainNode trn, in DVector3 levelOffset, UTerrainPlant plant, in FTransform trans, int capacity)
        {
            UPlantType type;
            if (PlantTypes.TryGetValue(plant, out type) == false)
            {
                type = new UPlantType();
                var task = type.Create(trn, levelOffset, plant);
                PlantTypes.Add(plant, type);
            }
            if(type.CreateFinished == false)
            {
                var tempTrans = trans;
                UEngine.Instance.EventPoster.RunOnUntilFinish((out bool isFinish)=>
                {
                    isFinish = type.CreateFinished;
                    if(isFinish)
                    {
                        var inst = new UPlantInstance();
                        inst.Placement.TransformData = tempTrans;
                        type.PushInstance(inst, capacity);
                    }
                    return null;
                }, Thread.Async.EAsyncTarget.Logic);
            }
            else
            {
                var inst = new UPlantInstance();
                inst.Placement.TransformData = trans;
                type.PushInstance(inst, capacity);
            }
        }
        public void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            foreach (var i in PlantTypes.Values)
            {
                if (i.Mesh == null)
                    continue;
                if (i.InstanceMdf.InstanceModifier.CurNumber == 0)
                    continue;
                if (rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
                {
                    i.Mesh.UpdateCameraOffset(rp.World);
                }

                rp.VisibleMeshes.Add(i.Mesh);

                //if (rp.CullType == GamePlay.UWorld.UVisParameter.EVisCull.Shadow)
                //{
                //    csm test
                //}
            }
            CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
        }
        public void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            foreach (var i in PlantTypes.Values)
            {
                if (i.Mesh == null)
                    continue;
                if (i.InstanceMdf.InstanceModifier.CurNumber == 0)
                    continue;
                meshes.Add(i.Mesh);
            }
        }
    }
}
