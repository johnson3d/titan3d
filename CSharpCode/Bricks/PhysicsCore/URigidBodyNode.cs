using EngineNS.IO;
using EngineNS.Rtti;
using Org.BouncyCastle.Asn1.X509.Qualified;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Bricks.PhysicsCore
{
    [Bricks.CodeBuilder.ContextMenu("PxMeshNode", "PxMeshNode", GamePlay.Scene.UNode.EditorKeyword)]
    [GamePlay.Scene.UNode(NodeDataType = typeof(TtRigidBodyNode.TtRigidBodyNodeData), DefaultNamePrefix = "PxMesh")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    //[Rtti.Meta(NameAlias = new string[] { "URigidBodyNode" })]
    public class TtRigidBodyNode : GamePlay.Scene.USceneActorNode
    {
        public class TtRigidBodyNodeData : GamePlay.Scene.UNodeData
        {
            [Rtti.Meta]
            public EPhyActorType PxActorType { get; set; }
            public class TtShapeSerializer : IO.UCustomSerializerAttribute
            {
                public override unsafe void Save(IO.IWriter ar, object host, string propName)
                {
                    System.Diagnostics.Debug.Assert(propName == "PxShapes");
                    var rbData = host as TtRigidBodyNodeData;
                    ar.Write(rbData.PxShapes.Count);
                    foreach (var i in rbData.PxShapes)
                    {
                        var sr = i.GetShapeSerializer();
                        IO.SerializerHelper.WriteObject(ar, sr.GetType(), sr);
                    }
                }
                public override unsafe object Load(IO.IReader ar, object host, string propName)
                {
                    System.Diagnostics.Debug.Assert(propName == "PxShapes");
                    var rbData = host as TtRigidBodyNodeData;
                    rbData.PxShapes.Clear();
                    int count = 0;
                    ar.Read(out count);
                    for (int i = 0; i < count; i++)
                    {
                        var sr = IO.SerializerHelper.ReadObject(ar, typeof(TtPhyShape.TtShapeSerializer), null) as TtPhyShape.TtShapeSerializer;
                        var shape = TtPhyShape.CreateShape(sr);
                        rbData.PxShapes.Add(shape);
                    }
                    return rbData.PxShapes;
                }
            }
            [TtShapeSerializer]
            [Rtti.Meta]
            public object PxShapeSerializer 
            { 
                get => null; 
                set
                {
                }
            }
            public List<TtPhyShape> PxShapes { get; set; } = new List<TtPhyShape>();
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, GamePlay.Scene.UNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (data==null)
            {
                data = new TtRigidBodyNodeData();
            }
            await base.InitializeNode(world, data, bvType, placementType);

            var rbNodeData = data as TtRigidBodyNodeData;
            if (rbNodeData == null)
                return false;

            InitPhysics(rbNodeData);

            return true;
        }
        private void InitPhysics(TtRigidBodyNodeData rbNodeData)
        {
            ref FTransform transform = ref this.Placement.AbsTransform;
            PxActor = UEngine.Instance.PhyModule.PhyContext.CreateActor(rbNodeData.PxActorType, in transform.mPosition, in transform.mQuat);
            PxActor.mCoreObject.SetActorFlag(EPhyActorFlag.PAF_eVISUALIZATION, true);

            UpdatePxShape(rbNodeData.PxShapes);
        }
        public bool UpdatePxShape(List<TtPhyShape> shapes)
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
            var rbNodeData = this.NodeData as TtRigidBodyNodeData;
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
        public TtPhyActor PxActor
        {
            get;
            private set;
        }
        public List<TtPhyShape> Shapes
        {
            get
            {
                var rbNodeData = this.NodeData as TtRigidBodyNodeData;
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
                    var node = info.ObjectInstance as TtRigidBodyNode;
                    if (node != null)
                    {
                        var mtl = UEngine.Instance.PhyModule.PhyContext.PhyMaterialManager.DefaultMaterial;
                        var box = UEngine.Instance.PhyModule.PhyContext.CreateShapeBox(mtl, in HalfExtent);
                        node.Shapes.Add(box);
                        node.UpdateShapeAABB();
                    }
                }
                return false;
            }
        }
        [PGAddBoxShape()]
        [Category("Editor")]
        public bool AddBoxShape
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public class PGAddTriMeshShapeAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            RName.PGRNameAttribute mMeshRNameEditor;
            RName.PGRNameAttribute mMtlRNameEditor;
            RName mTriMeshName;
            RName mTriMaterialName;
            public PGAddTriMeshShapeAttribute()
            {
                FullRedraw = false;
            }
            protected override async System.Threading.Tasks.Task<bool> Initialize_Override()
            {
                mMeshRNameEditor = new RName.PGRNameAttribute();
                await mMeshRNameEditor.Initialize();
                mMeshRNameEditor.ContentBrowser.Name = "TriMesh";

                mMtlRNameEditor = new RName.PGRNameAttribute();
                await mMtlRNameEditor.Initialize();
                mMtlRNameEditor.ContentBrowser.Name = "TriMaterial";

                return await base.Initialize_Override();
            }
            protected override void Cleanup_Override()
            {
                mMeshRNameEditor?.Cleanup();
                mMtlRNameEditor?.Cleanup();
                base.Cleanup_Override();
            }
            public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
            {
                var tmInfo = new EditorInfo();
                tmInfo.Value = mTriMeshName;
                ImGuiAPI.Text("TriMesh");
                mMeshRNameEditor.FilterExts = TtPhyTriMesh.AssetExt;
                object newMesh;
                ImGuiAPI.PushID("TriMesh");
                mMeshRNameEditor.OnDraw(in tmInfo, out newMesh);
                ImGuiAPI.PopID();
                if (newMesh != mTriMeshName)
                {
                    mTriMeshName = newMesh as RName;
                }
                var tmInfo1 = new EditorInfo();
                tmInfo1.Value = mTriMaterialName;
                ImGuiAPI.Text("TriMaterial");
                mMtlRNameEditor.FilterExts = TtPhyMaterial.AssetExt;
                object newMtl;
                ImGuiAPI.PushID("TriMaterial");
                mMtlRNameEditor.OnDraw(in tmInfo1, out newMtl);
                ImGuiAPI.PopID();
                if (newMtl != mTriMaterialName)
                {
                    mTriMaterialName = newMtl as RName;
                }
                ImGuiAPI.Separator();
                if (mTriMeshName != null && mTriMaterialName != null)
                {
                    if (ImGuiAPI.Button("AddTriMeshShape"))
                    {
                        var pc = UEngine.Instance.PhyModule.PhyContext;
                        var mesh = pc.PhyMeshManager.GetMeshSync(mTriMeshName);
                        var mtl = pc.PhyMaterialManager.GetMaterialSync(mTriMaterialName);
                        var rbNode = info.ObjectInstance as TtRigidBodyNode;
                        if (rbNode != null)
                        {
                            var shape = UEngine.Instance.PhyModule.PhyContext.CreateShapeTriMesh(new List<TtPhyMaterial>() { mtl }, mesh, rbNode.Placement.Scale, rbNode.Placement.Quat);
                            rbNode.Shapes.Add(shape);
                            rbNode.UpdateShapeAABB();
                        }
                    }
                }
                newValue = false;
                return false;
            }
        }
        [PGAddTriMeshShape()]
        [Category("Editor")]
        public bool AddTriMeshShape
        {
            get
            {
                return false;
            }
            set
            {

            }
        }
        public override void OnGatherVisibleMeshes(GamePlay.UWorld.UVisParameter rp)
        {
            //if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug) == false)
            //    return;

            if ((rp.CullFilters & GamePlay.UWorld.UVisParameter.EVisCullFilter.PhyxDebug) == 0)
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
                    rp.AddVisibleMesh(i.DebugMesh);
                }
            }
        }
        #endregion
    }
}

namespace EngineNS.GamePlay.Scene
{
    //public partial class UMeshNode
    //{
    //    public class IsRigidBody_EditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
    //    {
    //        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
    //        {
    //            if (ImGuiAPI.Button("BuildRB"))
    //            {
    //                (info.ObjectInstance as UMeshNode).BuildPhyRBNode();
    //            }
    //            newValue = info.Value;
    //            return true;
    //        }
    //    }
    //    [IsRigidBody_EditorAttribute]
    //    [Category("Option")]
    //    public bool IsRigidBody
    //    {
    //        get;
    //        set;
    //    }
    //    public void BuildPhyRBNode()
    //    {
    //        var name = this.NodeName + ".PhyRB";
    //        var rbNode = this.Children.Find((c) =>
    //        {
    //            return c.NodeName == name;
    //        });
    //        if (rbNode == null)
    //        {
    //            var rbData = new Bricks.PhysicsCore.URigidBodyNode.URigidBodyNodeData();
    //            rbData.Name = name;
    //            rbData.PxActorType = EPhyActorType.PAT_Static;
    //            //UEngine.Instance.PhyModule.PhyContext.CreateShapeTriMesh(,,)
    //            //rbData.PxShapes.Add()
    //            rbNode = new Bricks.PhysicsCore.URigidBodyNode();
    //            var task = rbNode.InitializeNode(GetWorld(), rbData, EBoundVolumeType.Box, this.Placement.GetType());
    //            UEngine.Instance.EventPoster.RunOnUntilFinish((state) =>
    //            {
    //                if (task.IsCompleted)
    //                {
    //                    rbNode.Parent = this;
    //                }
    //                return task.IsCompleted;
    //            }, Thread.Async.EAsyncTarget.Logic);
    //        }
    //    }
    //}
}

