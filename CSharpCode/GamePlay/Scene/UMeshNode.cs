using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("MeshNode", "MeshNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMeshNode.TtMeshNodeData), DefaultNamePrefix = "Mesh")]
    [EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UMeshNode@EngineCore", "EngineNS.GamePlay.Scene.UMeshNode" })]
    public partial class TtMeshNode : TtGpuSceneNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mMesh);
            base.Dispose();
        }
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UMeshNode.UMeshNodeData@EngineCore" })]
        public class TtMeshNodeData : TtNodeData
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
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtMeshNodeData == null)
            {
                data = new TtMeshNodeData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var meshData = data as TtMeshNodeData;
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
                if(i.HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
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

            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
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

                //var saved = mMesh.MdfQueue.MdfDatas;
                //Rtti.TtTypeDesc mdfQueueType;
                //if (value)
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_NoShadow, Graphics.Pipeline.Shader.UMdf_Shadow>();
                //}
                //else
                //{
                //    mdfQueueType = mMesh.MdfQueue.MdfPermutations.ReplacePermutation<Graphics.Pipeline.Shader.UMdf_Shadow, Graphics.Pipeline.Shader.UMdf_NoShadow>();
                //}
                //mMesh.SetMdfQueueType(mdfQueueType);
                //mMesh.MdfQueue.MdfDatas = saved;

                //int ObjectFlags_2Bit = 0;
                //if (value)
                //    ObjectFlags_2Bit |= 1;
                //else
                //    ObjectFlags_2Bit &= (~1);
                //mMesh.PerMeshCBuffer.SetValue(NxRHI.UBuffer.mPerMeshIndexer.ObjectFLags_2Bit, in ObjectFlags_2Bit);
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
            var meshData = data as TtMeshNodeData;
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
                var collideMesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshData.CollideName);
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

                if (mMesh != null)
                {
                    BoundVolume.LocalAABB = mMesh.MaterialMesh.AABB;
                    mMesh.HostNode = this;
                }
                else
                    BoundVolume.LocalAABB.InitEmptyBox();

                var meshData = NodeData as TtMeshNodeData;
                if (meshData != null)
                {
                    meshData.MeshName = mMesh.MaterialMesh.AssetName;
                    meshData.MdfQueueType = mMesh.MdfQueueType;
                    if (mMesh.SubMeshes[0].Atoms.Count == 0)
                    {
                        meshData.AtomType = Rtti.TtTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfStaticMesh));
                    }
                    else
                    {
                        meshData.AtomType = Rtti.TtTypeDesc.TypeStr(mMesh.SubMeshes[0].Atoms[0].GetType());
                    }
                }
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
                var meshData = NodeData as TtMeshNodeData;
                if (meshData == null)
                    return null;
                return meshData.MeshName;
            }
            set
            {
                var meshData = NodeData as TtMeshNodeData;
                if (meshData == null)
                    return;
                meshData.MeshName = value;
                if (meshData.MeshName == null)
                    return;
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
                };
                action();
            }
        }

        [Category("Option")]
        [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Pipeline.Shader.TtMdfQueueBase))]
        public Rtti.TtTypeDesc MdfQueue
        {
            get
            {
                if(NodeData is TtMeshNodeData meshNodeData)
                {
                    return meshNodeData.MdfQueue;

                }
                return Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfStaticMesh));
            }
            set
            {
                if (NodeData is TtMeshNodeData meshNodeData)
                {
                    if(meshNodeData.MdfQueue != null)
                    {
                        meshNodeData.MdfQueue = value;
                        System.Action action = async () =>
                        {
                            var mesh = new Graphics.Mesh.TtMesh();

                            var materialMesh = await TtEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(MeshName);
                            var ok = mesh.Initialize(materialMesh, meshNodeData.MdfQueue, meshNodeData.Atom);
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
            }
        }
        public override async Thread.Async.TtTask OnNodeLoaded(TtNode parent)
        {
            await base.OnNodeLoaded(parent);

            UpdateAbsTransform();
            var meshData = NodeData as TtMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                var cookedMesh = Graphics.Mesh.TtMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;// TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                //var colorVar = materials1[0].FindVar("clr4_0");
                //if (colorVar != null)
                //{
                //    colorVar.SetValue(new Vector4(1, 0, 1, 1));
                //}
                var mesh = new Graphics.Mesh.TtMesh();
                mesh.Initialize(cookedMesh, materials1, Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                mesh.IsAcceptShadow = this.IsAcceptShadow;
                Mesh = mesh;
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

            if (mMesh == null)
                return;

            NodeData.CheckDirty(this);

            rp.AddVisibleMesh(mMesh);
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
            //using (var guard = new Macross.UMacrossStackGuard(mLogicTickFrame))
            //{
            //    mTestBreak.TryBreak();
            //}
                
            return true;
        }

        Graphics.Mesh.TtMeshDataProvider mMeshDataProvider;
        public Graphics.Mesh.TtMeshDataProvider MeshDataProvider
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
