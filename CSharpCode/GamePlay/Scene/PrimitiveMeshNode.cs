using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Mesh.Modifier;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    /// <summary>
    /// 以 TtMeshPrimitives(.vms) 为几何源、允许逐 SubMesh/Atom 单独配置 TtMaterial 的场景节点。
    /// 与 TtMeshNode(.ums, 一份 TtMaterialMesh 内已绑定材质) 形成对仗：
    ///   - TtMeshNode          —— 引用一个 TtMaterialMesh(.ums)，几何 + 材质打包；
    ///   - TtPrimitiveMeshNode —— 引用一个 TtMeshPrimitives(.vms)，材质在 NodeData 上逐 Atom 配置。
    /// 全部数据存放在 TtPrimitiveMeshNodeData，保证序列化/反序列化加载。
    /// </summary>
    [Bricks.CodeBuilder.ContextMenu("PrimitiveMeshNode", "Graphics\\PrimitiveMeshNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPrimitiveMeshNode.TtPrimitiveMeshNodeData), DefaultNamePrefix = "PrimitiveMesh")]
    [Rtti.Meta]
    public partial class TtPrimitiveMeshNode : TtGpuSceneNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mMesh);
            base.Dispose();
        }

        [Rtti.Meta]
        public class TtPrimitiveMeshNodeData : TtNodeData
        {
            public TtPrimitiveMeshNodeData()
            {
                HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            }

            /// <summary>
            /// 几何资源 (.vms / TtMeshPrimitives)。
            /// </summary>
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.TtMeshPrimitives.AssetExt)]
            public RName MeshName { get; set; }

            /// <summary>
            /// 可选：碰撞用的 .vms (TtMeshPrimitives)。和 TtMeshNodeData.CollideName 行为一致。
            /// </summary>
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.TtMeshPrimitives.AssetExt)]
            public RName CollideName { get; set; }

            /// <summary>
            /// 逐 Atom 的材质资源 (.material / TtMaterial)。
            /// 索引语义：扁平展开 SubMesh -> Atom 顺序；目前 TtMeshPrimitives 默认为单 SubMesh 多 Atom，
            /// 这里也按 "AtomIndex == MaterialIndex" 一一对应即可。
            /// 加载时若数量与 Mesh 的 atom 数不一致，会自动用 DefaultMaterial 补齐 / 截断。
            /// </summary>
            [Rtti.Meta]
            // 长度由 mesh 的 atom 数决定, 禁止用户在 PG 上随手 + / X 改长度。
            [RName.PGRNameList(false, false,
                FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt + "," + Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
            public List<RName> MaterialNames { get; set; } = new List<RName>();

            /// <summary>
            /// 为 null 表示"本节点不指定", 交由 TtRenderMesh.Initialize 自动选型
            /// (见 CodingGuidelines.md §7.7)。
            /// </summary>
            [Rtti.Meta]
            [ReadOnly(true)]
            public string MdfQueueType { get; set; } = null;

            [Rtti.Meta]
            [ReadOnly(true)]
            public string AtomType { get; set; } = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.TtRenderMesh.TtAtom));

            [EGui.Controls.PropertyGrid.TtPGTypeEditor(typeof(Graphics.Pipeline.Shader.TtMdfQueueBase))]
            public Rtti.TtTypeDesc MdfQueue
            {
                // 挡掉空值: TypeOf(null/"") 会打 "Typeof failed:" 警告日志
                get => string.IsNullOrEmpty(MdfQueueType) ? null : Rtti.TtTypeDesc.TypeOf(MdfQueueType);
                set => MdfQueueType = Rtti.TtTypeDesc.TypeStr(value);
            }

            [EGui.Controls.PropertyGrid.TtPGTypeEditor(typeof(Graphics.Mesh.TtRenderMesh.TtAtom))]
            public Rtti.TtTypeDesc Atom
            {
                get => Rtti.TtTypeDesc.TypeOf(AtomType);
                set => AtomType = Rtti.TtTypeDesc.TypeStr(value);
            }
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtPrimitiveMeshNodeData == null)
            {
                data = new TtPrimitiveMeshNodeData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var meshData = data as TtPrimitiveMeshNodeData;
            if (meshData.MeshName != null)
            {
                var mesh = await BuildRenderMeshFromData(meshData);
                if (mesh != null)
                    this.RenderMesh = mesh;
            }

            this.SetStyle(ENodeStyles.ParallelTick);

            return true;
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);

            UpdateAbsTransform();

            var meshData = NodeData as TtPrimitiveMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                // 没指定 vms 时，给一个 wireframe Box 占位，方便编辑器里直观看到节点位置。
                var cookedMesh = Graphics.Mesh.TtMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                materials[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria;
                var mesh = new Graphics.Mesh.TtRenderMesh();
                mesh.Initialize(cookedMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                mesh.IsAcceptShadow = this.IsAcceptShadow;
                RenderMesh = mesh;
                return;
            }

            // 已通过 InitializeNode 构造过 RenderMesh，这里同步一次 IsAcceptShadow 状态即可。
            this.IsAcceptShadow = this.IsAcceptShadow;
        }

        /// <summary>
        /// 根据 NodeData 构建一份 TtRenderMesh：
        ///   1) 加载 vms 几何;
        ///   2) 把 MaterialNames 按 atom 数对齐成材质数组 (不足用 DefaultMaterial 补，多余截断);
        ///   3) 调 TtRenderMesh.Initialize(mesh, mats, mdfQueueType, atomType)。
        /// </summary>
        private async Thread.Async.TtTask<Graphics.Mesh.TtRenderMesh> BuildRenderMeshFromData(TtPrimitiveMeshNodeData meshData)
        {
            var primitives = await meshData.MeshName.GetAsset<Graphics.Mesh.TtMeshPrimitives>();
            if (primitives == null)
                return null;

            // TtMeshPrimitives 默认是单 SubMesh + N 个 Atom，这里按 atom 数对齐材质。
            int atomCount = (int)primitives.NumAtom;
            if (atomCount <= 0)
                atomCount = 1;

            var materials = new Graphics.Pipeline.Shader.TtMaterial[atomCount];
            for (int i = 0; i < atomCount; i++)
            {
                Graphics.Pipeline.Shader.TtMaterial mtl = null;
                if (meshData.MaterialNames != null && i < meshData.MaterialNames.Count && meshData.MaterialNames[i] != null)
                {
                    mtl = await meshData.MaterialNames[i].GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
                }
                if (mtl == null)
                {
                    // 缺失材质用工程默认材质兜底，避免 Initialize 失败 / 渲染时 NRE。
                    var defaultName = TtEngine.Instance?.Config?.DefaultMaterial;
                    if (defaultName != null)
                        mtl = await defaultName.GetAsset<Graphics.Pipeline.Shader.TtMaterial>();
                }
                materials[i] = mtl;
            }

            var renderMesh = new Graphics.Mesh.TtRenderMesh();
            var ok = renderMesh.Initialize(primitives, materials, meshData.MdfQueue, meshData.Atom);
            if (!ok)
                return null;

            // 把"对齐后"的材质回写到 NodeData，保证下次序列化数组长度正确。
            SyncMaterialNamesFromArray(meshData, materials);

            return renderMesh;
        }

        private static void SyncMaterialNamesFromArray(TtPrimitiveMeshNodeData meshData, Graphics.Pipeline.Shader.TtMaterial[] materials)
        {
            if (meshData.MaterialNames == null)
                meshData.MaterialNames = new List<RName>();
            meshData.MaterialNames.Clear();
            for (int i = 0; i < materials.Length; i++)
            {
                meshData.MaterialNames.Add(materials[i]?.AssetName);
            }
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            if (mMesh == null)
                return;
            meshes.Add(mMesh);
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
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
            get => base.IsAcceptShadow;
            set
            {
                base.IsAcceptShadow = value;
                if (mMesh == null)
                    return;
                mMesh.IsAcceptShadow = value;
            }
        }

        public UBoxBV GetBoxBV()
        {
            return BoundVolume as UBoxBV;
        }

        Graphics.Mesh.TtRenderMesh mMesh;
        [Rtti.Meta]
        public override Graphics.Mesh.TtRenderMesh RenderMesh
        {
            get => mMesh;
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
                {
                    BoundVolume.LocalAABB.InitEmptyBox();
                }

                var meshData = NodeData as TtPrimitiveMeshNodeData;
                if (meshData != null && mMesh != null)
                {
                    meshData.MdfQueueType = mMesh.MdfQueueType;
                    if (mMesh.SubMeshes.Count > 0 && mMesh.SubMeshes[0].Atoms.Count > 0)
                    {
                        meshData.AtomType = Rtti.TtTypeDesc.TypeStr(mMesh.SubMeshes[0].Atoms[0].GetType());
                    }
                    else
                    {
                        meshData.AtomType = Rtti.TtTypeDesc.TypeStr(typeof(Graphics.Mesh.TtRenderMesh.TtAtom));
                    }
                }

                this.UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();

                if (mMesh != null)
                    mMesh.HostNode = this;
            }
        }

        /// <summary>
        /// 编辑器侧的几何资源属性。改这个会触发整体重建（mesh + 材质数组按新 atom 数对齐）。
        /// </summary>
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMeshPrimitives.AssetExt)]
        [Category("Assets")]
        public RName MeshName
        {
            get
            {
                var meshData = NodeData as TtPrimitiveMeshNodeData;
                return meshData?.MeshName;
            }
            set
            {
                var meshData = NodeData as TtPrimitiveMeshNodeData;
                if (meshData == null)
                    return;
                if (meshData.MeshName == value)
                    return;
                meshData.MeshName = value;
                if (meshData.MeshName == null)
                    return;

                System.Action action = async () =>
                {
                    var mesh = await BuildRenderMeshFromData(meshData);
                    if (mesh == null)
                        return;
                    RenderMesh = mesh;
                    var world = this.GetWorld();
                    RenderMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                    OnHitProxyChanged();
                };
                action();
            }
        }

        /// <summary>
        /// 编辑器侧的逐 Atom 材质列表。改这个会按新材质重建 RenderMesh。
        /// PG 里使用 PGRNameList 自定义编辑器，每行选资产时按 mtl/uinst 扩展名过滤。
        /// </summary>
        [Category("Assets")]
        [RName.PGRNameList(false, false,
            FilterExts = Graphics.Pipeline.Shader.TtMaterial.AssetExt + "," + Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
        public List<RName> MaterialNames
        {
            get
            {
                var meshData = NodeData as TtPrimitiveMeshNodeData;
                return meshData?.MaterialNames;
            }
            set
            {
                var meshData = NodeData as TtPrimitiveMeshNodeData;
                if (meshData == null)
                    return;
                meshData.MaterialNames = value ?? new List<RName>();
                if (meshData.MeshName == null)
                    return;

                System.Action action = async () =>
                {
                    var mesh = await BuildRenderMeshFromData(meshData);
                    if (mesh == null)
                        return;
                    RenderMesh = mesh;
                    var world = this.GetWorld();
                    RenderMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                    OnHitProxyChanged();
                };
                action();
            }
        }

        [Category("Assets")]
        [EGui.Controls.PropertyGrid.TtPGTypeEditor(typeof(Graphics.Pipeline.Shader.TtMdfQueueBase))]
        public Rtti.TtTypeDesc MdfQueue
        {
            get
            {
                if (NodeData is TtPrimitiveMeshNodeData meshNodeData)
                    return meshNodeData.MdfQueue;
                return Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Mesh.TtMdfStaticMesh));
            }
            set
            {
                if (NodeData is TtPrimitiveMeshNodeData meshNodeData)
                {
                    if (meshNodeData.MdfQueue == value)
                        return;
                    meshNodeData.MdfQueue = value;
                    if (meshNodeData.MeshName == null)
                        return;
                    System.Action action = async () =>
                    {
                        var mesh = await BuildRenderMeshFromData(meshNodeData);
                        if (mesh == null)
                            return;
                        RenderMesh = mesh;
                        var world = this.GetWorld();
                        RenderMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                        OnHitProxyChanged();
                    };
                    action();
                }
            }
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            UpdateCameralOffset(rp.World);

            if (mMesh == null)
                return;

            this.CheckDirty();

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

        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return null;
        }

        public override bool IsNoTick
        {
            get => true;
            set => base.IsNoTick = value;
        }

        // —— 碰撞 / Linecheck，与 TtMeshNode 行为对齐 ——
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
            var meshData = NodeData as TtPrimitiveMeshNodeData;
            if (meshData == null)
                return;
            if (meshData.MeshName != null)
                ameta.AddReferenceAsset(meshData.MeshName);
            if (meshData.CollideName != null)
                ameta.AddReferenceAsset(meshData.CollideName);
            if (meshData.MaterialNames != null)
            {
                foreach (var mtl in meshData.MaterialNames)
                {
                    if (mtl != null)
                        ameta.AddReferenceAsset(mtl);
                }
            }
        }
    }
}
