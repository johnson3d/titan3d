using Assimp;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("PBRTestNode", "PBRTestNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMeshNode.TtMeshNodeData), DefaultNamePrefix = "PBRTest")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtPBRTestNode : TtGpuSceneNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mMesh);
            foreach(var mesh1 in mMeshMatrix)
            {
                mesh1.Dispose();
            }
            mMeshMatrix.Clear();
            foreach (var mesh2 in mCurrMeshMatrix)
            {
                mesh2.Dispose();
            }
            mCurrMeshMatrix.Clear();

            base.Dispose();
        }
        public class UMeshNodeData : TtNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
            public RName CollideName { get; set; }
            [Rtti.Meta]
            [ReadOnly(true)]
            public string MdfQueueType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh));
            [Rtti.Meta]
            [ReadOnly(true)]
            public string AtomType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.TtMesh.TtAtom));

            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Pipeline.Shader.TtMdfQueueBase))]
            public Rtti.UTypeDesc MdfQueue
            {
                get
                {
                    return Rtti.UTypeDesc.TypeOf(MdfQueueType);
                }
                set
                {
                    MdfQueueType = Rtti.UTypeDesc.TypeStr(value);
                }
            }
            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Mesh.TtMesh.TtAtom))]
            public Rtti.UTypeDesc Atom
            {
                get
                {
                    return Rtti.UTypeDesc.TypeOf(AtomType);
                }
                set
                {
                    AtomType = Rtti.UTypeDesc.TypeStr(value);
                }
            }
        }
        async Thread.Async.TtTask<bool> InitMeshMatrix(UMeshNodeData meshData)
        {
            mMeshMatrix.Clear();
            var world = this.GetWorld();
            int meshCount = 10;
            for (int x = 0; x < meshCount; ++x)
            {
                for (int z = 0; z < meshCount; ++z)
                {
                    var meshPrimitive = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(RName.GetRName("tutorials/pbr/cube.vms", RName.ERNameType.Game));
                    var mtlInst = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/brdf_base.uminst", RName.ERNameType.Engine));
                    var mtlInstList = new List<Graphics.Pipeline.Shader.TtMaterial>();
                    mtlInstList.Add(mtlInst);
                    {
                        var meshSphere = new Graphics.Mesh.TtMesh();
                        meshSphere.Initialize(meshPrimitive, mtlInstList, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));
                        var mtl = meshSphere.GetMaterial(0, 0);
                        var roughness = mtl.FindVar("Roughness");
                        if (roughness != null)
                        {
                            roughness.SetValue((float)x / (float)(meshCount-1));
                        }
                        var metallic = mtl.FindVar("Metallic");
                        if (metallic != null)
                        {
                            metallic.SetValue((float)z / (float)(meshCount - 1));
                        }
                        var trans = Placement.AbsTransform;
                        trans.Position = trans.Position + new DVector3(x * 2.5, 0, z * 2.5);
                        if (world != null)
                        {
                            meshSphere.SetWorldTransform(in trans, world, false);
                        }
                        else
                        {
                            meshSphere.SetWorldTransform(in trans, null, false);
                        }

                        mMeshMatrix.Add(meshSphere);
                        meshSphere.HostNode = this;
                    }
                }
            }
            // todo: this function invalid
            BoundVolume.LocalAABB.Merge(new Vector3(25, 0, 25));

            BoundVolume.LocalAABB = new BoundingBox(BoundVolume.LocalAABB.Minimum, new Vector3(25, 1, 25));

            return true;
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as UMeshNodeData == null)
            {
                data = new UMeshNodeData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var meshData = data as UMeshNodeData;

            await InitMeshMatrix(meshData);

            var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
            if (materialMesh != null)
            {
                var mesh = new Graphics.Mesh.TtMesh();
                mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
                this.Mesh = mesh;
                //await materialMesh.Mesh.TryLoadClusteredMesh();
            }

            return true;
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
        {
            if (mMesh == null)
                return;
            meshes.Add(mMesh);
            foreach(var i in Children)
            {
                if(i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnHitProxyChanged()
        {
            if (mMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.UHitProxy.EHitproxyType.None)
            {
                mMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mMesh.SetHitproxy(in value);
            }
            else
            {
                mMesh.IsDrawHitproxy = false;
            }
        }
        protected override void OnSetPrefabTemplate()
        {
            
        }
        public override bool IsAcceptShadow
        {
            get
            {
                return base.IsAcceptShadow;
            }
            set
            {
                base.IsAcceptShadow = value;
                if (mMesh == null)
                    return;

                mMesh.IsAcceptShadow = value;
            }
        }
        public static async System.Threading.Tasks.Task<TtMeshNode> AddMeshNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, Type placementType, Graphics.Mesh.TtMesh mesh, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var meshNode = await scene.NewNode(world, typeof(TtMeshNode), data, EBoundVolumeType.Box, placementType) as TtMeshNode;
            if (mesh.MaterialMesh.AssetName != null)
                meshNode.NodeData.Name = mesh.MaterialMesh.AssetName.Name;
            else
                meshNode.NodeData.Name = meshNode.SceneId.ToString();
            meshNode.Mesh = mesh;
            meshNode.Parent = parent;
            
            meshNode.Placement.SetTransform(in pos, in scale, in quat);

            return meshNode;
        }
        public static async System.Threading.Tasks.Task<TtMeshNode> AddMeshNode(GamePlay.TtWorld world, TtNode parent, TtNodeData data, Type placementType, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var meshData = data as UMeshNodeData;
            var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
            if (materialMesh == null)
                return null;
            var mesh = new Graphics.Mesh.TtMesh();
            
            var ok = mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
            if (ok == false)
                return null;

            var meshNode = await AddMeshNode(world, parent, data, placementType, mesh, pos, scale, quat);
            if (meshData.CollideName != null)
            {
                //var collideMesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshData.CollideName);
                //if (collideMesh != null)
                //{
                //    if (collideMesh.MeshDataProvider == null)
                //    {
                //        await collideMesh.LoadMeshDataProvider();
                //    }
                //    meshNode.mMeshDataProvider = collideMesh.MeshDataProvider;
                //}
            }
            return meshNode;
        }
        public UBoxBV GetBoxBV()
        {
            return BoundVolume as UBoxBV;
        }
        List<Graphics.Mesh.TtMesh> mCurrMeshMatrix = new List<Graphics.Mesh.TtMesh>();
        List<Graphics.Mesh.TtMesh> mMeshMatrix = new List<Graphics.Mesh.TtMesh>();
        Graphics.Mesh.TtMesh mMesh;
        [Rtti.Meta]
        public Graphics.Mesh.TtMesh Mesh 
        {
            get 
            {
                return mMesh;
            }
            set
            {
                if (mMesh != null)
                {
                    mMesh.HostNode = null;
                }
                
                mMesh = value;

                var meshData = NodeData as UMeshNodeData;
                if (meshData != null)
                {
                    meshData.MeshName = mMesh.MaterialMesh.AssetName;
                    meshData.MdfQueueType = mMesh.MdfQueueType;
                    meshData.AtomType = Rtti.UTypeDesc.TypeStr(mMesh.SubMeshes[0].Atoms[0].GetType());
                }

                if (mMesh != null)
                {
                    BoundVolume.LocalAABB = mMesh.MaterialMesh.AABB;
                    mMesh.HostNode = this;
                }
                else
                    BoundVolume.LocalAABB.InitEmptyBox();

                this.UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();

                mMesh.HostNode = this;
            }
        }
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        [Category("Option")]
        public RName MeshName 
        {
            get
            {
                var meshData = NodeData as UMeshNodeData;
                if (meshData == null)
                    return null;
                return meshData.MeshName;
            }
            set
            {
                var meshData = NodeData as UMeshNodeData;
                if (meshData == null)
                    return;
                meshData.MeshName = value;
                if (meshData.MeshName == null)
                    return;
                _ = InitMeshMatrix(meshData);

                System.Action action = async () =>
                {
                    var mesh = new Graphics.Mesh.TtMesh();

                    var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(value);
                    var ok = mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
                    if (ok == false)
                        return;
                    Mesh = mesh;
                    var world = this.GetWorld();
                    if (world != null)
                    {
                        Mesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                    }
                    else
                    {
                        Mesh.SetWorldTransform(in Placement.AbsTransform, null, false);
                    }
                    OnHitProxyChanged();

                    //// add mesh matrix
                    //int meshCount = 10;
                    //mMeshMatrix.Clear();
                    //for(int x = 0; x < meshCount; ++x)
                    //{
                    //    for (int z = 0; z < meshCount; ++z)
                    //    {
                    //        var mtlInst = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/brdf_base.uminst", RName.ERNameType.Engine));
                    //        var meshPrimitive = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(RName.GetRName("mesh/base/sphere.vms", RName.ERNameType.Engine));
                    //        var mtlInstList = new List<Graphics.Pipeline.Shader.TtMaterial>();
                    //        mtlInstList.Add(mtlInst);
                    //        //var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
                    //        //if (materialMesh != null)
                    //        {
                    //            var meshSphere = new Graphics.Mesh.TtMesh();
                    //            meshSphere.Initialize(meshPrimitive, mtlInstList, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));
                    //            var mtl = meshSphere.GetMaterial(0, 0);
                    //            var roughness = mtl.FindVar("Roughness");
                    //            if (roughness != null)
                    //            {
                    //                roughness.SetValue((float)x / (float)(meshCount-1));
                    //            }
                    //            var metallic = mtl.FindVar("Metallic");
                    //            if (metallic != null)
                    //            {
                    //                metallic.SetValue((float)z / (float)(meshCount-1));
                    //            }
                    //            var trans = Placement.AbsTransform;
                    //            trans.Position = trans.Position + new DVector3(x * 2.5, 0, z * 2.5);
                    //            //var world = this.GetWorld();
                    //            if (world != null)
                    //            {
                    //                meshSphere.SetWorldTransform(in trans, world, false);
                    //            }
                    //            else
                    //            {
                    //                meshSphere.SetWorldTransform(in trans, null, false);
                    //            }

                    //            mMeshMatrix.Add(meshSphere);
                    //            meshSphere.HostNode = this;

                    //        }
                    //    }
                    //}
                    //BoundVolume.LocalAABB.Merge(new Vector3(25, 0, 25));
                    //BoundVolume.LocalAABB = new BoundingBox(BoundVolume.LocalAABB.Minimum, new Vector3(25, 1, 25));
                };
                action();
            }
        }
        public override void OnNodeLoaded(TtNode parent)
        {
            base.OnNodeLoaded(parent);

            UpdateAbsTransform();
            var meshData = NodeData as UMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;// TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                //var colorVar = materials1[0].FindVar("clr4_0");
                //if (colorVar != null)
                //{
                //    colorVar.SetValue(new Vector4(1, 0, 1, 1));
                //}
                var mesh = new Graphics.Mesh.TtMesh();
                mesh.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                mesh.IsAcceptShadow = this.IsAcceptShadow;
                Mesh = mesh;
                return;
            }
            else
            {
                System.Action action = async () =>
                {
                    var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
                    if (materialMesh != null)
                    {
                        var mesh = new Graphics.Mesh.TtMesh();
                        mesh.Initialize(materialMesh, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));

                        Mesh = mesh;
                        this.IsAcceptShadow = this.IsAcceptShadow;
                    }
                };
                action();
            }
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            UpdateCameralOffset(rp.World);

            if (mMesh == null)
                return;

            NodeData.CheckDirty(this);

            rp.AddVisibleMesh(mMesh);

            foreach (var mesh in mCurrMeshMatrix)
            {
                rp.AddVisibleMesh(mesh);
            }
            if (mMeshMatrix.Count == 100)
            {
                mCurrMeshMatrix.Clear();
                mCurrMeshMatrix.AddRange(mMeshMatrix);
            }

            rp.AddVisibleNode(this);
        }
        protected override void OnCameralOffsetChanged(TtWorld world)
        {
            mMesh?.UpdateCameraOffset(world);
        }
        protected override void OnAbsTransformChanged()
        {
            if (mMesh == null)
                return;

            var world = this.GetWorld();
            mMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
        }
        static Macross.UMacrossStackFrame mLogicTickFrame = new Macross.UMacrossStackFrame();
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", false);
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            //using (var guard = new Macross.UMacrossStackGuard(mLogicTickFrame))
            //{
            //    mTestBreak.TryBreak();
            //}
                
            return true;
        }

        Graphics.Mesh.UMeshDataProvider mMeshDataProvider;
        public Graphics.Mesh.UMeshDataProvider MeshDataProvider
        {
            get => mMeshDataProvider;
            set => mMeshDataProvider = value;
        }
        public unsafe override bool OnLineCheckTriangle(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            if (mMeshDataProvider == null)
                return false;

            var startf = start.ToSingleVector3();
            var endf = end.ToSingleVector3();
            var pStart = &startf;
            var pEnd = &endf;
            //fixed (DVector3* pStart = &start)
            //fixed (DVector3* pEnd = &end)
            fixed (VHitResult* pResult = &result)
            {
                if (Placement.HasScale)
                {
                    Vector3 scale = Placement.Scale;
                    if (-1 != mMeshDataProvider.mCoreObject.IntersectTriangle(&scale, pStart, pEnd, pResult))
                        return true;
                }
                else
                {
                    if (-1 != mMeshDataProvider.mCoreObject.IntersectTriangle((Vector3*)0, pStart, pEnd, pResult))
                        return true;
                }
                return false;
            }
        }

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            if (MeshName != null)
                ameta.AddReferenceAsset(MeshName);
        }
    }
}
