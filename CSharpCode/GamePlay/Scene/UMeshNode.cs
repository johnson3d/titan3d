using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("MeshNode", "MeshNode", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(UMeshNode.UMeshNodeData), DefaultNamePrefix = "Mesh")]
    public partial class UMeshNode : USceneActorNode
    {
        public class UMeshNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
            public RName CollideName { get; set; }
            [Rtti.Meta]
            [ReadOnly(true)]
            public string MdfQueueType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh));
            [Rtti.Meta]
            [ReadOnly(true)]
            public string AtomType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMesh.UAtom));

            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Pipeline.Shader.UMdfQueue))]
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
            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Mesh.UMesh.UAtom))]
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
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as UMeshNodeData == null)
            {
                data = new UMeshNodeData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var meshData = data as UMeshNodeData;
            var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
            if (materialMesh != null)
            {
                var mesh = new Graphics.Mesh.UMesh();
                mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
                this.Mesh = mesh;
            }
            return true;
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
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

                List<string> features = new List<string>();
                if (value == false)
                {
                    features.Add("NoShadow");
                }

                var saved = mMesh.MdfQueue.MdfDatas;
                var mdfQueueType = mMesh.MdfQueue.GetPermutation(features);
                mMesh.SetMdfQueueType(mdfQueueType);
                mMesh.MdfQueue.MdfDatas = saved;

                //int ObjectFlags_2Bit = 0;
                //if (value)
                //    ObjectFlags_2Bit |= 1;
                //else
                //    ObjectFlags_2Bit &= (~1);
                //mMesh.PerMeshCBuffer.SetValue(NxRHI.UBuffer.mPerMeshIndexer.ObjectFLags_2Bit, in ObjectFlags_2Bit);
                mMesh.IsAcceptShadow = value;
            }
        }
        public static async System.Threading.Tasks.Task<UMeshNode> AddMeshNode(GamePlay.UWorld world, UNode parent, UNodeData data, Type placementType, Graphics.Mesh.UMesh mesh, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var meshNode = await scene.NewNode(world, typeof(UMeshNode), data, EBoundVolumeType.Box, placementType) as UMeshNode;
            if (mesh.MaterialMesh.Mesh.AssetName != null)
                meshNode.NodeData.Name = mesh.MaterialMesh.Mesh.AssetName.Name;
            else
                meshNode.NodeData.Name = meshNode.SceneId.ToString();
            meshNode.Mesh = mesh;
            meshNode.Parent = parent;
            
            meshNode.Placement.SetTransform(in pos, in scale, in quat);

            return meshNode;
        }
        public static async System.Threading.Tasks.Task<UMeshNode> AddMeshNode(GamePlay.UWorld world, UNode parent, UNodeData data, Type placementType, DVector3 pos, Vector3 scale, Quaternion quat)
        {
            var meshData = data as UMeshNodeData;
            var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
            if (materialMesh == null)
                return null;
            var mesh = new Graphics.Mesh.UMesh();
            
            var ok = mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
            if (ok == false)
                return null;

            var meshNode = await AddMeshNode(world, parent, data, placementType, mesh, pos, scale, quat);
            if (meshData.CollideName != null)
            {
                var collideMesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshData.CollideName);
                if (collideMesh != null)
                {
                    if (collideMesh.MeshDataProvider == null)
                    {
                        await collideMesh.LoadMeshDataProvider();
                    }
                    meshNode.mMeshDataProvider = collideMesh.MeshDataProvider;
                }
            }
            return meshNode;
        }
        public UBoxBV GetBoxBV()
        {
            return BoundVolume as UBoxBV;
        }
        Graphics.Mesh.UMesh mMesh;
        public Graphics.Mesh.UMesh Mesh 
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

                if (mMesh != null)
                {
                    BoundVolume.LocalAABB = mMesh.MaterialMesh.Mesh.mCoreObject.mAABB;
                    mMesh.HostNode = this;
                }
                else
                    BoundVolume.LocalAABB.InitEmptyBox();

                var meshData = NodeData as UMeshNodeData;
                if (meshData != null)
                {
                    meshData.MeshName = mMesh.MaterialMesh.AssetName;
                    meshData.MdfQueueType = mMesh.MdfQueueType;
                    meshData.AtomType = Rtti.UTypeDesc.TypeStr(mMesh.Atoms[0].GetType());
                }
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }
        [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
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
                System.Action action = async () =>
                {
                    var mesh = new Graphics.Mesh.UMesh();

                    var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(value);
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
                };
                action();
            }
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);

            UpdateAbsTransform();
            var meshData = NodeData as UMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                var cookedMesh = Graphics.Mesh.UMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;// UEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                //var colorVar = materials1[0].FindVar("clr4_0");
                //if (colorVar != null)
                //{
                //    colorVar.SetValue(new Vector4(1, 0, 1, 1));
                //}
                var mesh = new Graphics.Mesh.UMesh();
                if (this.IsAcceptShadow)
                {
                    mesh.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_Shadow>>.TypeDesc);
                }
                else
                {
                    mesh.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                }
                Mesh = mesh;
                return;
            }
            else
            {
                System.Action action = async () =>
                {
                    var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
                    if (materialMesh != null)
                    {
                        var mesh = new Graphics.Mesh.UMesh();
                        mesh.Initialize(materialMesh, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));

                        Mesh = mesh;
                        this.IsAcceptShadow = this.IsAcceptShadow;
                    }
                };
                action();
            }
        }
        protected uint CameralOffsetSerialId = 0;
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (mMesh == null)
                return;

            if (rp.World.CameralOffsetSerialId != CameralOffsetSerialId)
            {
                CameralOffsetSerialId = rp.World.CameralOffsetSerialId;
                mMesh.UpdateCameraOffset(rp.World);
            }

            rp.VisibleMeshes.Add(mMesh);
            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
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
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
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
