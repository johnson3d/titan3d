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
            public string MdfQueueType { get; set; } = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh));
            [Rtti.Meta]
            [ReadOnly(true)]
            public string AtomType { get; set; } = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.TtMesh.TtAtom));

            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Pipeline.Shader.TtMdfQueueBase))]
            public Rtti.TtTypeDesc MdfQueue
            {
                get
                {
                    return Rtti.TtTypeDesc.TypeOf(MdfQueueType);
                }
                set
                {
                    MdfQueueType = Rtti.TtTypeDesc.TypeStr(value);
                }
            }
            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Mesh.TtMesh.TtAtom))]
            public Rtti.TtTypeDesc Atom
            {
                get
                {
                    return Rtti.TtTypeDesc.TypeOf(AtomType);
                }
                set
                {
                    AtomType = Rtti.TtTypeDesc.TypeStr(value);
                }
            }
        }
        bool bNeedUpdateMeshMatrix = false;
        int meshCount = 10;
        [Category("Option")]
        public int MeshCount
        {
            get => meshCount;
            set
            {
                meshCount = value;
                bNeedUpdateMeshMatrix = true;
            }
        }
        float meshSpacing = 1.5f;
        [Category("Option")]
        public float MeshSpacing
        {
            get => meshSpacing;
            set
            {
                bNeedUpdateMeshMatrix = true;
                meshSpacing = value;
            }
        }
        async Thread.Async.TtTask<bool> InitMeshMatrix(UMeshNodeData meshData)
        {
            mMeshMatrix.Clear();
            var world = this.GetWorld();
            for (int x = 0; x < meshCount; ++x)
            {
                for (int z = 0; z < meshCount; ++z)
                {
                    var meshPrimitive = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(RName.GetRName("mesh/base/sphere.vms", RName.ERNameType.Engine));
                    var mtlInst = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("material/brdf_base.uminst", RName.ERNameType.Engine));
                    var mtlInstList = new List<Graphics.Pipeline.Shader.TtMaterial>();
                    mtlInstList.Add(mtlInst);
                    {
                        var meshSphere = new Graphics.Mesh.TtMesh();
                        meshSphere.Initialize(meshPrimitive, mtlInstList, Rtti.TtTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.TtTypeDesc.TypeOf(meshData.AtomType));
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
                        trans.Position = trans.Position + new DVector3(x * meshSpacing, 0, z * meshSpacing);
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
            var maxDistance = meshCount * meshSpacing;
            //BoundVolume.LocalAABB.Merge(new Vector3(maxDistance, 1, maxDistance));
            BoundVolume.LocalAABB = new BoundingBox(BoundVolume.LocalAABB.Minimum, new Vector3(maxDistance, 1, maxDistance));

            this.UpdateAbsTransform();
            UpdateAABB();
            Parent?.UpdateAABB();

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

            return true;
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
        {
            meshes.AddRange(mMeshMatrix);
            foreach(var i in Children)
            {
                if(i.HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnHitProxyChanged()
        {
            if (this.HitProxy == null)
            {
                foreach(var m in mMeshMatrix)
                {
                    m.IsDrawHitproxy = false;
                }
                return;
            }

            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                var value = HitProxy.ConvertHitProxyIdToVector4();
                foreach (var m in mMeshMatrix)
                {
                    m.IsDrawHitproxy = true;
                    m.SetHitproxy(in value);
                }

            }
            else
            {
                foreach (var m in mMeshMatrix)
                {
                    m.IsDrawHitproxy = false;
                }
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
                if (mMeshMatrix.Count == 0)
                    return;

                foreach (var m in mMeshMatrix)
                {
                    m.IsAcceptShadow = value;
                }

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

        public override async Thread.Async.TtTask OnNodeLoaded(TtNode parent)
        {
            await base.OnNodeLoaded(parent);

            UpdateAbsTransform();
            var meshData = NodeData as UMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;// TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                return;
            }
            else
            {
                this.IsAcceptShadow = this.IsAcceptShadow;
            }
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            UpdateCameralOffset(rp.World);

            NodeData.CheckDirty(this);

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

            foreach (var m in mMeshMatrix)
            {
                m?.UpdateCameraOffset(world);
            }
        }
        protected override void OnAbsTransformChanged()
        {
            //if (mMesh == null)
            //    return;

            //var world = this.GetWorld();
            //mMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
        }

/* 项目“Engine.Window”的未合并的更改
在此之前:
        static Macross.UMacrossStackFrame mLogicTickFrame = new Macross.UMacrossStackFrame();
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", false);
在此之后:
        static Macross.TtMacrossStackFrame mLogicTickFrame = new Macross.TtMacrossStackFrame();
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", false);
*/

/* 项目“Engine.Window”的未合并的更改
在此之前:
        static Macross.UMacrossStackFrame mLogicTickFrame = new Macross.UMacrossStackFrame();
在此之后:
        static Macross.TtMacrossStackFrame mLogicTickFrame = new Macross.TtMacrossStackFrame();
*/
        static Macross.TtMacrossStackFrame mLogicTickFrame = new Macross.TtMacrossStackFrame();

/* 项目“Engine.Window”的未合并的更改
在此之前:
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", false);
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
在此之后:
        static Macross.TtMacrossBreak mTestBreak = new Macross.TtMacrossBreak("UMeshNode.OnTickLogic", false);
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
*/
        static Macross.TtMacrossBreak mTestBreak = new Macross.TtMacrossBreak("UMeshNode.OnTickLogic", false);
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            if (bNeedUpdateMeshMatrix == true)
            {
                var meshData = NodeData as UMeshNodeData;
                if (meshData != null)
                    _ = InitMeshMatrix(meshData);
                bNeedUpdateMeshMatrix = false;
            }
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
        }
    }
}
