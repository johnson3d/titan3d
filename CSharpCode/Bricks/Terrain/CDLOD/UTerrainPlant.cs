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
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
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
    public class UTerainPlantManager : IDisposable
    {
        public void Dispose()
        {
            foreach(var i in PlantTypes)
            {
                i.Value.Mesh.Dispose();
            }
        }
        public class UPlantInstance : GamePlay.Scene.TtNode //Graphics.Pipeline.IProxiable
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

                var instance = new Graphics.Pipeline.Shader.FVSInstanceData();
                instance.Position = (Placement.Position - PlantType.InstanceOffset).ToSingleVector3();
                instance.Scale = Placement.Scale;
                instance.Quat = Placement.Quat;
                
                //PlantType.InstanceMdf.InstanceModifier.SetInstance(InstanceIndex, in instance);
                DebugHitproxyMesh?.SetWorldTransform(Placement.TransformData, PlantType.Terrain.GetWorld(), false);
            }
            public override void OnHitProxyChanged()
            {
                base.OnHitProxyChanged();
            }
            public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
            {
                if (DebugHitproxyMesh == null)
                {
                    DebugHitproxyMesh = new Graphics.Mesh.TtMesh();
                    DebugHitproxyMesh.Initialize(PlantType.MaterialMesh,
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    DebugHitproxyMesh.SetWorldTransform(Placement.TransformData, PlantType.Terrain.GetWorld(), false);
                    DebugHitproxyMesh.IsAcceptShadow = false;
                }
                meshes.Add(DebugHitproxyMesh);
            }
            public UPlantType PlantType;
            //public uint InstanceIndex = uint.MaxValue;
            //public FTransform Transform = FTransform.Identity;
            public Graphics.Mesh.TtMesh DebugHitproxyMesh;
        }
        public class UPlantType
        {
            public Bricks.Terrain.CDLOD.TtTerrainNode Terrain;
            public UTerrainPlant PlantDesc { get; set; }
            public List<UPlantInstance> ObjInstances { get; } = new List<UPlantInstance>();
            public Graphics.Mesh.TtMaterialMesh MaterialMesh;
            public Graphics.Mesh.TtMesh Mesh;
            public Graphics.Mesh.UMdfInstanceStaticMesh InstanceMdf;
            public DVector3 InstanceOffset;
            public bool CreateFinished = false;
            public async System.Threading.Tasks.Task Create(Bricks.Terrain.CDLOD.TtTerrainNode trn, DVector3 levelOffset, UTerrainPlant desc)
            {
                Terrain = trn;
                PlantDesc = desc;
                MaterialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(desc.MeshName);
                Mesh = new Graphics.Mesh.TtMesh();
                Mesh.Initialize(MaterialMesh, 
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfInstanceStaticMesh>.TypeDesc);

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
                InstanceMdf.InstanceModifier.SetCapacity((uint)capacity, false);
                TtEngine.Instance.GfxDevice.HitproxyManager.MapProxy(obj);
                ObjInstances.Add(obj);
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
            //        await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(j.MeshName);
            //    }
            //}
        }
        public void AddPlant(Bricks.Terrain.CDLOD.TtTerrainNode trn, in DVector3 levelOffset, UTerrainPlant plant, in FTransform trans, int capacity)
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
                TtEngine.Instance.EventPoster.RunOnUntilFinish((Thread.Async.TtAsyncTaskStateBase state) =>
                {
                    if (type.CreateFinished)
                    {
                        var inst = new UPlantInstance();
                        inst.Placement.TransformData = tempTrans;
                        type.PushInstance(inst, capacity);
                    }
                    return type.CreateFinished;
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
                if (i.ObjInstances.Count == 0)
                    continue;
                if (rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
                {
                    i.Mesh.UpdateCameraOffset(rp.World);
                }

                i.InstanceMdf.InstanceModifier.InstanceBuffers.ResetInstance();
                foreach (var j in i.ObjInstances)
                {
                    DBoundingBox result;
                    var src = new DBoundingBox(in i.Mesh.MaterialMesh.AABB);
                    DBoundingBox.Transform(in src, in j.Placement.TransformRef, out result);
                    if (rp.CullCamera.WhichContainTypeFast(rp.World, in result, false) == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                        continue;

                    var pos = (j.Placement.Position - i.InstanceOffset).ToSingleVector3();
                    var instance = new Graphics.Pipeline.Shader.FVSInstanceData();
                    instance.Position = pos;
                    instance.Scale = j.Placement.Scale;
                    instance.Quat = j.Placement.Quat;
                    instance.HitProxyId = j.HitProxy.ProxyId;

                    i.InstanceMdf.InstanceModifier.PushInstance(in instance);
                    //j.InstanceIndex = i.InstanceMdf.InstanceModifier.PushInstance(in instance);

                    
                    rp.MergeAABB(in result);
                }

                rp.AddVisibleMesh(i.Mesh, false);
            }
            CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
        }
        public void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
        {
            foreach (var i in PlantTypes.Values)
            {
                if (i.Mesh == null)
                    continue;
                if (i.InstanceMdf.InstanceModifier.InstanceBuffers.NumOfInstance == 0)
                    continue;
                meshes.Add(i.Mesh);
            }
        }
    }
}
