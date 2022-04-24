using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    public class URigidBodyNode : GamePlay.Scene.ULightWeightNodeBase
    {
        public class URigidBodyNodeData : GamePlay.Scene.UNodeData
        {
            [Rtti.Meta]
            public EPhyActorType PxActorType { get; set; }
            public List<UPhyShape> PxShapes = new List<UPhyShape>();
            public override void OnWriteMember(IWriter ar, ISerializer obj, UMetaVersion metaVersion)
            {
                base.OnWriteMember(ar, obj, metaVersion);

                PxShapes.Clear();
                ar.Write(PxShapes.Count);
                foreach (var i in PxShapes)
                {
                    ar.Write(i.ShapeType);
                    Vector3 p = new Vector3();
                    Quaternion q = new Quaternion();
                    i.mCoreObject.GetLocalPose(ref p, ref q);
                    ar.Write(p);
                    ar.Write(q);
                    ar.Write(i.Materials.Length);
                    foreach (var j in i.Materials)
                    {
                        ar.Write(j.AssetName);
                    }
                    switch (i.ShapeType)
                    {
                        case EPhysShapeType.PST_Plane:
                            break;
                        case EPhysShapeType.PST_Box:
                            {
                                var shape = i as UPhyBoxShape;
                                ar.Write(shape.HalfExtent);
                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                var shape = i as UPhySphereShape;
                                ar.Write(shape.Radius);
                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                var shape = i as UPhyCapsuleShape;
                                ar.Write(shape.Radius);
                                ar.Write(shape.HalfHeight);
                            }
                            break;
                        case EPhysShapeType.PST_Convex:
                            break;
                        case EPhysShapeType.PST_TriangleMesh:
                            {
                                var shape = i as UPhyTriMeshShape;
                                ar.Write(shape.TriMeshSource);
                            }
                            break;
                        case EPhysShapeType.PST_Unknown:
                            break;
                    }
                }
            }
            public override void OnReadMember(IReader ar, ISerializer obj, UMetaVersion metaVersion)
            {
                base.OnReadMember(ar, obj, metaVersion);
                int nbShape = 0;
                ar.Read(out nbShape);
                for (int i = 0; i < nbShape; i++)
                {
                    EPhysShapeType type;
                    ar.Read(out type);
                    Vector3 p = new Vector3();
                    Quaternion q = new Quaternion();
                    ar.Read(out p);
                    ar.Read(out q);
                    int nbMaterial;
                    ar.Read(out nbMaterial);                    
                    UPhyMaterial[] pxMaterials = new UPhyMaterial[nbMaterial];                    
                    for (int j = 0; j < nbMaterial; j++)
                    {
                        RName mtlName;
                        ar.Read(out mtlName);
                        pxMaterials[j] = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.GetMaterialSync(mtlName);
                    }
                    switch (type)
                    {
                        case EPhysShapeType.PST_Plane:
                            break;
                        case EPhysShapeType.PST_Box:
                            {
                                Vector3 extent = new Vector3();
                                ar.Read(out extent);
                                var shape = UEngine.Instance.PhyModue.PhyContext.CreateShapeBox(pxMaterials[0], in extent);
                                PxShapes.Add(shape);
                            }
                            break;
                        case EPhysShapeType.PST_Sphere:
                            {
                                float fRadius = 0;
                                ar.Read(out fRadius);
                                var shape = UEngine.Instance.PhyModue.PhyContext.CreateShapeSphere(pxMaterials[0], fRadius);
                                PxShapes.Add(shape);
                            }
                            break;
                        case EPhysShapeType.PST_Capsule:
                            {
                                float fRadius = 0;
                                float fHalfHeight = 0;
                                ar.Read(out fRadius);
                                ar.Read(out fHalfHeight);
                                var shape = UEngine.Instance.PhyModue.PhyContext.CreateShapeCapsule(pxMaterials[0], fRadius, fHalfHeight);
                                PxShapes.Add(shape);
                            }
                            break;
                        case EPhysShapeType.PST_Convex:
                            break;
                        case EPhysShapeType.PST_TriangleMesh:
                            {
                                RName triMeshSource;
                                ar.Read(out triMeshSource);
                                var triMesh = UEngine.Instance.PhyModue.PhyContext.PhyMeshManager.GetMeshSync(triMeshSource);
                                Vector3 scale = new Vector3();
                                Quaternion quat = new Quaternion();
                                var shape = UEngine.Instance.PhyModue.PhyContext.CreateShapeTriMesh(pxMaterials, triMesh, in scale, in quat);
                                PxShapes.Add(shape);
                            }
                            break;
                        case EPhysShapeType.PST_Unknown:
                            break;
                    }
                }
            }
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, GamePlay.Scene.UNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data==null)
            {
                data = new URigidBodyNodeData();
            }
            await base.InitializeNode(world, data, bvType, placementType);

            var rbNodeData = data as URigidBodyNodeData;
            if (rbNodeData == null)
                return false;

            InitPhysics(rbNodeData);

            return true;
        }
        private void InitPhysics(URigidBodyNodeData rbNodeData)
        {
            ref FTransform transform = ref this.Placement.AbsTransform;
            PxActor = UEngine.Instance.PhyModue.PhyContext.CreateActor(rbNodeData.PxActorType, in transform.mPosition, in transform.mQuat);
            PxActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            UpdatePxShape(rbNodeData.PxShapes);
        }
        public bool UpdatePxShape(List<UPhyShape> shapes)
        {
            foreach (var i in Shapes)
            {
                i.mCoreObject.RemoveFromActor();
            }

            foreach (var i in shapes)
            {
                Vector3 p = new Vector3();
                Quaternion q = new Quaternion();
                i.mCoreObject.GetLocalPose(ref p, ref q);
                i.mCoreObject.AddToActor(PxActor.mCoreObject, in p, in q);
            }
            var rbNodeData = this.NodeData as URigidBodyNodeData;
            if (rbNodeData != null)
            {
                rbNodeData.PxShapes = shapes;
            }

            UpdateShapeAABB();
            return true;
        }
        public void UpdateShapeAABB()
        {
            BoundVolume.LocalAABB.InitEmptyBox();
            foreach (var i in this.Shapes)
            {
                var tmp = i.GetAABB();
                BoundVolume.LocalAABB = BoundingBox.Merge(BoundVolume.LocalAABB, tmp);                
            }
            UpdateAABB();
        }
        protected override void OnParentSceneChanged(GamePlay.Scene.UScene prev, GamePlay.Scene.UScene cur)
        {
            PxActor.AddToScene(cur.PxSceneMB.PxScene);
        }
        protected override void OnAbsTransformChanged()
        {
            if (PxActor != null)
            {
                ref FTransform transform = ref this.Placement.AbsTransform;
                PxActor.SetPose2Physics(in transform.mPosition, transform.mQuat, false);
            }
        }
        public UPhyActor PxActor
        {
            get;
            private set;
        }
        public List<UPhyShape> Shapes
        {
            get
            {
                var rbNodeData = this.NodeData as URigidBodyNodeData;
                if (rbNodeData == null)
                    return null;
                return rbNodeData.PxShapes;
            }
        }
        public int GetShapeNumber()
        {
            if (Shapes == null)
                return 0;
            return Shapes.Count;
        }
        public void SetShapeLocalPose(int idx, in Vector3 p, in Quaternion q)
        {
            if (Shapes == null || idx >= Shapes.Count)
                return;
            Shapes[idx].mCoreObject.SetLocalPose(in p, in q);
        }

        #region Editor Only
        public class PGAddBoxShapeAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public PGAddBoxShapeAttribute()
            {
                //FullRedraw = true;
            }
            Vector3 HalfExtent = new Vector3(1);
            public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var sz = new Vector2(0);
                fixed (Vector3* p = &HalfExtent)
                {
                    ImGuiAPI.InputFloat3("HalfExtent", (float*)p, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                }                
                if (ImGuiAPI.Button("AddBoxShape", in sz))
                {
                    var node = info.ObjectInstance as URigidBodyNode;
                    if (node != null)
                    {
                        var mtl = UEngine.Instance.PhyModue.PhyContext.PhyMaterialManager.DefaultMaterial;
                        var box = UEngine.Instance.PhyModue.PhyContext.CreateShapeBox(mtl, in HalfExtent);
                        node.Shapes.Add(box);
                        node.UpdateShapeAABB();
                    }
                }
                return false;
            }
        }
        [PGAddBoxShape()]
        [System.ComponentModel.Category("Editor")]
        public bool Ed_AddBoxShape
        {
            get
            {
                return true;
            }
            set
            {

            }
        }
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug) == false)
                return;

            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }

            var nodeTransform = this.Placement.AbsTransform;
            foreach (var i in Shapes)
            {
                if (i.DebugMesh != null)
                {
                    FTransform absTransform;
                    FTransform.Multiply(out absTransform, i.Transform, in nodeTransform);
                    i.DebugMesh.SetWorldTransform(in absTransform, rp.World, true);
                    rp.VisibleMeshes.Add(i.DebugMesh);
                }
            }   
        }
        #endregion
    }
}
