using EngineNS.Graphics.Mesh;
using EngineNS.Graphics.Mesh.Modifier;
using EngineNS.Graphics.Pipeline;
using EngineNS.Graphics.Pipeline.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    /// <summary>
    /// Custom PropertyGrid editor for displaying MergedNodes as a popup list with Focus/Select buttons.
    /// </summary>
    public class TtPGMergedNodesEditorAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        public TtPGMergedNodesEditorAttribute()
        {
        }

        private static string PopupId = "##MergedNodesPopup";

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;

            var instanceNode = info.FirstObjectInstance as TtInstanceMeshNode;
            if (instanceNode == null)
                return false;

            var mergedNodes = instanceNode.MergedNodes;
            int count = mergedNodes.Count;

            // Draw a button that opens the popup
            string btnLabel = count > 0 ? $"Select ({count})##MergedNodesBtn" : "(empty)##MergedNodesBtn";
            var btnSize = new Vector2(ImGuiAPI.GetContentRegionAvail().X, 0);
            if (count > 0 && ImGuiAPI.Button(btnLabel, in btnSize))
            {
                ImGuiAPI.OpenPopup(PopupId, ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            else if (count == 0)
            {
                ImGuiAPI.Text("(empty)");
            }

            // Popup window with the merged nodes list
            if (ImGuiAPI.BeginPopup(PopupId, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
            {
                float focusBtnWidth = 24.0f;
                float itemWidth = 200.0f;
                for (int i = 0; i < mergedNodes.Count; i++)
                {
                    var node = mergedNodes[i];
                    if (node == null)
                        continue;

                    string nodeName = node.NodeData?.Name ?? "unnamed";

                    // Selectable row
                    bool isSelected = node.Selected;
                    var selectableSize = new Vector2(itemWidth, 0);
                    if (ImGuiAPI.Selectable(nodeName + "##MergedPopupNode" + i, isSelected,
                        ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in selectableSize))
                    {
                        SelectNodeInSceneEditor(instanceNode, node);
                        ImGuiAPI.CloseCurrentPopup();
                    }

                    // Focus button
                    ImGuiAPI.SameLine(0, 4.0f);
                    var focusBtnSize = new Vector2(focusBtnWidth, 0);
                    if (ImGuiAPI.Button("\u2192##FocusMergedPopup" + i, in focusBtnSize))
                    {
                        FocusNodeInSceneEditor(instanceNode, node);
                        ImGuiAPI.CloseCurrentPopup();
                    }
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        ImGuiAPI.BeginTooltip();
                        ImGuiAPI.Text($"Focus: {nodeName}");
                        ImGuiAPI.EndTooltip();
                    }
                }
                ImGuiAPI.EndPopup();
            }

            return false;
        }

        private void SelectNodeInSceneEditor(TtInstanceMeshNode instanceNode, TtMeshNode targetNode)
        {
            var sceneEditor = instanceNode.GetWorld()?.ViewportSlate?.AssetEditor as Editor.Forms.TtSceneEditor;
            if (sceneEditor != null)
            {
                sceneEditor.PreviewViewport.OnHitproxySelectedMulti(true,
                    new IProxiable[] { targetNode });
            }
        }

        private void FocusNodeInSceneEditor(TtInstanceMeshNode instanceNode, TtMeshNode targetNode)
        {
            var sceneEditor = instanceNode.GetWorld()?.ViewportSlate?.AssetEditor as Editor.Forms.TtSceneEditor;
            if (sceneEditor != null)
            {
                sceneEditor.mWorldOutliner?.FocusNodeInViewport(targetNode);
                sceneEditor.PreviewViewport.OnHitproxySelectedMulti(true,
                    new IProxiable[] { targetNode });
            }
        }
    }

    /// <summary>
    /// Custom PropertyGrid editor for selecting a TtInstanceMeshNode from the scene.
    /// Filters by matching MaterialMesh and provides a Focus button.
    /// </summary>
    public class TtPGInstanceNodeSelectorAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            bool valueChanged = false;

            var meshNode = info.FirstObjectInstance as TtMeshNode;
            if (meshNode == null)
                return false;

            var currentTarget = info.Value as TtInstanceMeshNode;
            var scene = meshNode.GetNearestParentScene();

            // Collect filtered TtInstanceMeshNode list
            var candidates = new List<TtInstanceMeshNode>();
            if (scene != null)
            {
                CollectInstanceMeshNodes(scene, meshNode.MaterialMesh, candidates);
            }

            // Draw combo box
            var index = ImGuiAPI.TableGetColumnIndex();
            var width = ImGuiAPI.GetColumnWidth(index);
            float buttonWidth = 24.0f;
            float comboWidth = width - buttonWidth - EGui.UIProxy.StyleConfig.Instance.PGCellPadding.X - 4.0f;

            ImGuiAPI.SetNextItemWidth(comboWidth);
            string currentLabel = currentTarget != null
                ? $"{currentTarget.NodeData?.Name ?? "unnamed"} ({currentTarget.NodeId.ToString().Substring(0, 8)})"
                : "None";

            if (ImGuiAPI.BeginCombo("##" + info.Name, currentLabel, ImGuiComboFlags_.ImGuiComboFlags_None))
            {
                // "None" option
                bool isNone = currentTarget == null;
                if (ImGuiAPI.Selectable("None", isNone, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                {
                    if (currentTarget != null)
                    {
                        newValue = null;
                        valueChanged = true;
                    }
                }

                // Filtered candidates
                for (int i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    bool isSelected = (candidate == currentTarget);
                    string label = $"{candidate.NodeData?.Name ?? "unnamed"} ({candidate.NodeId.ToString().Substring(0, 8)})";
                    if (ImGuiAPI.Selectable(label, isSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        if (candidate != currentTarget)
                        {
                            newValue = candidate;
                            valueChanged = true;
                        }
                    }
                }
                ImGuiAPI.EndCombo();
            }

            // Focus button
            ImGuiAPI.SameLine(0, 4.0f);
            bool disabled = currentTarget == null;
            if (disabled)
            {
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Text, EGui.UIProxy.StyleConfig.Instance.TextDisableColor);
            }
            var btnSize = new Vector2(buttonWidth, 0);
            if (ImGuiAPI.Button("\u2192##FocusInstanceNode" + info.Name, in btnSize) && !disabled)
            {
                var sceneEditor = meshNode.GetWorld()?.ViewportSlate?.AssetEditor as Editor.Forms.TtSceneEditor;
                if (sceneEditor != null)
                {
                    sceneEditor.mWorldOutliner?.FocusNodeInViewport(currentTarget);
                    sceneEditor.PreviewViewport.OnHitproxySelectedMulti(true,
                        new Graphics.Pipeline.IProxiable[] { currentTarget });
                }
            }
            if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
            {
                ImGuiAPI.BeginTooltip();
                ImGuiAPI.Text("Focus to InstanceMeshNode");
                ImGuiAPI.EndTooltip();
            }
            if (disabled)
            {
                ImGuiAPI.PopStyleColor(1);
            }

            return valueChanged;
        }

        private void CollectInstanceMeshNodes(TtNode root, TtMaterialMesh filterMesh, List<TtInstanceMeshNode> results)
        {
            if (root is TtInstanceMeshNode instanceNode)
            {
                if (filterMesh == null || instanceNode.MaterialMesh == filterMesh)
                {
                    results.Add(instanceNode);
                }
            }
            foreach (var child in root.Children)
            {
                CollectInstanceMeshNodes(child, filterMesh, results);
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("InstanceMeshNode", "Graphics\\InstanceMeshNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtInstanceMeshNode.TtInstanceMeshNodeData), DefaultNamePrefix = "InstanceMesh")]
    [Rtti.Meta("")]
    public partial class TtInstanceMeshNode : TtGpuSceneNode
    {
        [Rtti.Meta("")]
        public class TtInstanceMeshNodeData : TtNodeData
        {
            public TtInstanceMeshNodeData()
            {
                HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
            }
            [Rtti.Meta("")]
            [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
            public RName MeshName { get; set; }

            [Rtti.Meta("")]
            public List<FTransform> InstanceTransforms { get; set; } = new List<FTransform>();

            [Rtti.Meta("")]
            public bool UseInstanceBounding { get; set; } = false;

            [Rtti.Meta("")]
            public uint Capacity { get; set; } = 512;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mRenderMesh);
            base.Dispose();
        }

        #region Properties
        TtMaterialMesh mMaterialMesh;
        public TtMaterialMesh MaterialMesh
        {
            get => mMaterialMesh;
            private set => mMaterialMesh = value;
        }

        Graphics.Mesh.TtRenderMesh mRenderMesh;
        public override Graphics.Mesh.TtRenderMesh RenderMesh
        {
            get => mRenderMesh;
            set
            {
                if (mRenderMesh != null)
                {
                    mRenderMesh.HostNode = null;
                }

                mRenderMesh = value;

                if (mRenderMesh != null)
                {
                    BoundVolume.LocalAABB = mRenderMesh.MaterialMesh.AABB;
                    mRenderMesh.HostNode = this;
                }
                else
                {
                    BoundVolume.LocalAABB.InitEmptyBox();
                }

                this.UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }

        /// <summary>
        /// MaterialMesh asset to use for instanced rendering.
        /// Changing this in the Detail panel will reload the mesh and rebuild the render pipeline.
        /// </summary>
        [Category("Option")]
        [RName.PGRName(FilterExts = Graphics.Mesh.TtMaterialMesh.AssetExt)]
        public RName MeshName
        {
            get
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                return meshData?.MeshName;
            }
            set
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                if (meshData == null)
                    return;
                meshData.MeshName = value;
                System.Action action = async () =>
                {
                    if (value == null)
                    {
                        MaterialMesh = null;
                        RenderMesh = null;
                        return;
                    }
                    MaterialMesh = await value.GetAsset<Graphics.Mesh.TtMaterialMesh>();
                    if (MaterialMesh == null)
                        return;
                    var mesh = new Graphics.Mesh.TtRenderMesh();
                    if (mesh.Initialize(MaterialMesh, Rtti.TtTypeDescGetter<TtMdfInstanceStaticMesh>.TypeDesc))
                    {
                        RenderMesh = mesh;
                        var mdfQueue = mesh.MdfQueue as TtMdfInstanceStaticMesh;
                        mdfQueue?.InstanceModifier.SetCapacity(meshData.Capacity, meshData.UseInstanceBounding);
                        var world = this.GetWorld();
                        mesh.SetWorldTransform(in Placement.AbsTransform, world, false);
                        OnHitProxyChanged();
                    }
                    // Reload hitproxy icon with new snapshot
                    mHitproxyMesh = null;
                    await InitHitproxyMesh();
                };
                action();
            }
        }

        /// <summary>
        /// Whether to use per-instance bounding boxes for culling.
        /// </summary>
        [Category("Option")]
        public bool UseInstanceBounding
        {
            get
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                return meshData?.UseInstanceBounding ?? false;
            }
            set
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                if (meshData == null)
                    return;
                meshData.UseInstanceBounding = value;
            }
        }

        /// <summary>
        /// Pre-allocated instance buffer capacity.
        /// </summary>
        [Category("Option")]
        public uint Capacity
        {
            get
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                return meshData?.Capacity ?? 512;
            }
            set
            {
                var meshData = NodeData as TtInstanceMeshNodeData;
                if (meshData == null)
                    return;
                meshData.Capacity = value;
                // Re-apply capacity to InstanceModifier if already initialized
                var mdfQueue = mRenderMesh?.MdfQueue as TtMdfInstanceStaticMesh;
                mdfQueue?.InstanceModifier.SetCapacity(value, meshData.UseInstanceBounding);
            }
        }

        /// <summary>
        /// Total instance count (manual + merged).
        /// </summary>
        [Category("Info")]
        [ReadOnly(true)]
        public int InstanceCount
        {
            get
            {
                var nodeData = NodeData as TtInstanceMeshNodeData;
                int count = nodeData != null ? nodeData.InstanceTransforms.Count : 0;
                count += mMergedNodes.Count;
                return count;
            }
        }

        /// <summary>
        /// Number of manually placed instances (from InstanceTransforms list).
        /// </summary>
        [Category("Info")]
        [ReadOnly(true)]
        public int ManualInstanceCount
        {
            get
            {
                var nodeData = NodeData as TtInstanceMeshNodeData;
                return nodeData != null ? nodeData.InstanceTransforms.Count : 0;
            }
        }

        /// <summary>
        /// Number of TtMeshNodes currently merged into this instance group.
        /// </summary>
        [Category("Info")]
        [ReadOnly(true)]
        public int MergedNodeCount => mMergedNodes.Count;

        /// <summary>
        /// List of merged nodes displayed in the Detail panel with Focus/Select buttons.
        /// </summary>
        [Category("Merged Nodes")]
        [TtPGMergedNodesEditor]
        public IReadOnlyList<TtMeshNode> MergedNodeList => ResolveMergedNodes();
        #endregion

        #region MergedNodes
        private List<WeakReference<TtMeshNode>> mMergedNodes = new List<WeakReference<TtMeshNode>>();
        // Cached resolved list to avoid per-frame allocation
        private List<TtMeshNode> mResolvedMergedNodes = new List<TtMeshNode>();

        /// <summary>
        /// Resolve live merged nodes, removing dead WeakReferences.
        /// Returns a cached list (valid until next call).
        /// </summary>
        private List<TtMeshNode> ResolveMergedNodes()
        {
            mResolvedMergedNodes.Clear();
            for (int i = mMergedNodes.Count - 1; i >= 0; i--)
            {
                if (mMergedNodes[i].TryGetTarget(out var node) && node.Parent != null)
                    mResolvedMergedNodes.Add(node);
                else
                    mMergedNodes.RemoveAt(i);
            }
            return mResolvedMergedNodes;
        }

        public void RegisterMergedNode(TtMeshNode node)
        {
            if (node == null)
                return;
            // Check if already registered
            foreach (var wr in mMergedNodes)
            {
                if (wr.TryGetTarget(out var existing) && existing == node)
                    return;
            }
            node.SetStyle(ENodeStyles.Invisible);
            mMergedNodes.Add(new WeakReference<TtMeshNode>(node));
            UpdateAABB();
        }

        public void UnregisterMergedNode(TtMeshNode node)
        {
            if (node == null)
                return;
            for (int i = mMergedNodes.Count - 1; i >= 0; i--)
            {
                if (mMergedNodes[i].TryGetTarget(out var existing) && existing == node)
                {
                    node.UnsetStyle(ENodeStyles.Invisible);
                    mMergedNodes.RemoveAt(i);
                    break;
                }
            }
            UpdateAABB();
        }

        public IReadOnlyList<TtMeshNode> MergedNodes => ResolveMergedNodes();
        #endregion

        #region Instance Management
        public int AddInstance(in FTransform transform)
        {
            var nodeData = NodeData as TtInstanceMeshNodeData;
            if (nodeData == null)
                return -1;
            nodeData.InstanceTransforms.Add(transform);
            UpdateAABB();
            return nodeData.InstanceTransforms.Count - 1;
        }

        public bool RemoveInstance(int index)
        {
            var nodeData = NodeData as TtInstanceMeshNodeData;
            if (nodeData == null || index < 0 || index >= nodeData.InstanceTransforms.Count)
                return false;
            nodeData.InstanceTransforms.RemoveAt(index);
            UpdateAABB();
            return true;
        }

        public bool UpdateInstance(int index, in FTransform transform)
        {
            var nodeData = NodeData as TtInstanceMeshNodeData;
            if (nodeData == null || index < 0 || index >= nodeData.InstanceTransforms.Count)
                return false;
            nodeData.InstanceTransforms[index] = transform;
            UpdateAABB();
            return true;
        }

        public void ClearInstances()
        {
            var nodeData = NodeData as TtInstanceMeshNodeData;
            nodeData?.InstanceTransforms.Clear();
            UpdateAABB();
        }
        #endregion

        #region Initialization
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data as TtInstanceMeshNodeData == null)
            {
                data = new TtInstanceMeshNodeData();
            }
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            var meshData = data as TtInstanceMeshNodeData;
            if (meshData.MeshName != null)
            {
                MaterialMesh = await meshData.MeshName.GetAsset<Graphics.Mesh.TtMaterialMesh>();
                if (MaterialMesh != null)
                {
                    var mesh = new Graphics.Mesh.TtRenderMesh();
                    if (mesh.Initialize(MaterialMesh, Rtti.TtTypeDescGetter<TtMdfInstanceStaticMesh>.TypeDesc))
                    {
                        this.RenderMesh = mesh;

                        // Initialize InstanceModifier
                        var mdfQueue = mesh.MdfQueue as TtMdfInstanceStaticMesh;
                        if (mdfQueue != null)
                        {
                            mdfQueue.InstanceModifier.SetCapacity(meshData.Capacity, meshData.UseInstanceBounding);
                        }
                    }
                }
            }

            return true;
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            await InitHitproxyMesh();
            UpdateAbsTransform();
        }
        #endregion

        #region Editor Hitproxy Icon
        Graphics.Mesh.TtRenderMesh mHitproxyMesh;

        /// <summary>
        /// Editor-only billboard mesh using the MaterialMesh's snapshot as icon texture.
        /// Only visible when UtilityEditor cull filter is active.
        /// </summary>
        async Thread.Async.TtTask InitHitproxyMesh()
        {
            if (mHitproxyMesh != null)
                return;

            float rectSize = 0.5f;
            var rectProvider = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(
                -rectSize * 0.5f, -rectSize * 0.5f, rectSize, rectSize, 0.0f);
            var rectMesh = rectProvider.ToMesh();

            // Load base unlit material (point_light material is a simple textured billboard)
            var mtl = await RName.GetRName("material/utility/point_light.uminst", RName.ERNameType.Engine)
                .GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>();
            if (mtl == null)
                return;

            // Clone the material instance so we can override its texture with the snapshot
            var iconMtl = mtl.CloneMaterialInstance();

            // Try to load MaterialMesh snapshot as icon texture
            await LoadSnapshotToMaterial(iconMtl);

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = iconMtl;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var ok = mesh.Initialize(rectMesh, materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mesh.IsAcceptShadow = false;
                mHitproxyMesh = mesh;
                mHitproxyMesh.HostNode = this;

                this.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;

                UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }

        async Thread.Async.TtTask LoadSnapshotToMaterial(Graphics.Pipeline.Shader.TtMaterialInstance iconMtl)
        {
            var meshData = NodeData as TtInstanceMeshNodeData;
            if (meshData?.MeshName == null)
                return;

            var assetMeta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(meshData.MeshName);
            if (assetMeta == null)
                return;

            var snapshot = await Editor.TtSnapshot.Load(assetMeta, true);
            if (snapshot?.mTextureRSV != null && iconMtl.UsedSrView.Count > 0)
            {
                // Override the first texture slot with the snapshot SRV
                iconMtl.UsedSrView[0].SrvObject = snapshot.mTextureRSV;
            }
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Threshold: when instance count exceeds this, use parallel frustum culling.
        /// </summary>
        private const int ParallelCullThreshold = 64;
        private byte[] mCullVisibility = null;

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            // Draw editor hitproxy billboard icon (UtilityEditor mode)
            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) != 0)
            {
                if (mHitproxyMesh != null && rp.CullCamera != null)
                {
                    var objPos = Placement.AbsTransform.mPosition;
                    var yawQuat = rp.CullCamera.GetYawFaceToCamera(objPos);
                    float scale = 1.0f;
                    var scaleVec = new Vector3(scale);
                    var hitproxyTransform = FTransform.CreateTransform(in objPos, in scaleVec, in yawQuat);
                    mHitproxyMesh.SetWorldTransform(in hitproxyTransform, rp.World, false);
                    rp.AddVisibleMesh(mHitproxyMesh);
                }
            }

            if (mRenderMesh == null)
                return;

            // Instance shader computes world-space position directly from per-instance data.
            // WorldMatrix in CBuffer must be Identity so the main VS doesn't double-offset.
            mRenderMesh.DirectSetWorldMatrix(in Matrix.Identity);

            var mdfQueue = mRenderMesh.MdfQueue as TtMdfInstanceStaticMesh;
            if (mdfQueue == null)
                return;

            var instanceMod = mdfQueue.InstanceModifier;
            instanceMod.InstanceBuffers.ResetInstance();

            var meshAABB = BoundVolume.mLocalAABB;
            bool doFrustumCull = rp.CullCamera != null && !rp.DontFrustumCull && !meshAABB.IsEmpty();

            // 1. Push manual instances from NodeData
            var nodeData = NodeData as TtInstanceMeshNodeData;
            if (nodeData != null)
            {
                var transforms = nodeData.InstanceTransforms;
                int manualCount = transforms.Count;
                var cameraOffset = rp.World.CameraOffset;

                if (doFrustumCull && manualCount >= ParallelCullThreshold)
                {
                    // Parallel frustum culling pass
                    EnsureCullBuffer(manualCount);
                    TtEngine.Instance.EventPoster.ParallelFor(manualCount, static (i, state) =>
                    {
                        var pThis = state.GetForArgument0<TtInstanceMeshNode>();
                        var visParam = state.GetForArgument1<TtWorld.TtVisParameter>();
                        var data = (pThis.NodeData as TtInstanceMeshNodeData);
                        var t = data.InstanceTransforms[i];
                        var localAABB = pThis.BoundVolume.mLocalAABB;
                        var instAABB = new DBoundingBox(
                            t.mPosition + localAABB.Minimum * t.mScale,
                            t.mPosition + localAABB.Maximum * t.mScale);
                        var ct = visParam.CullCamera.WhichContainTypeFast(visParam.World, in instAABB, true);
                        pThis.mCullVisibility[i] = ct == CONTAIN_TYPE.CONTAIN_TEST_OUTER ? (byte)0 : (byte)1;
                    }, -1, this, rp);

                    // Sequential push of visible instances
                    for (int i = 0; i < manualCount; i++)
                    {
                        if (mCullVisibility[i] == 0)
                            continue;
                        var t = transforms[i];
                        var inst = new FVSInstanceData();
                        inst.Position = (t.mPosition - cameraOffset).ToSingleVector3();
                        inst.Scale = t.mScale;
                        inst.Quat = t.mQuat;
                        instanceMod.PushInstance(in inst);
                    }
                }
                else
                {
                    // Serial path (small count or no frustum cull)
                    foreach (var t in transforms)
                    {
                        if (doFrustumCull)
                        {
                            var instAABB = new DBoundingBox(
                                t.mPosition + meshAABB.Minimum * t.mScale,
                                t.mPosition + meshAABB.Maximum * t.mScale);
                            var ct = rp.CullCamera.WhichContainTypeFast(rp.World, in instAABB, true);
                            if (ct == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                continue;
                        }
                        var inst = new FVSInstanceData();
                        inst.Position = (t.mPosition - cameraOffset).ToSingleVector3();
                        inst.Scale = t.mScale;
                        inst.Quat = t.mQuat;
                        instanceMod.PushInstance(in inst);
                    }
                }
            }

            // 2. Push merged MeshNode instances
            {
                // Resolve live nodes (cleans up dead WeakReferences)
                var liveNodes = ResolveMergedNodes();
                int mergedCount = liveNodes.Count;
                if (doFrustumCull && mergedCount >= ParallelCullThreshold)
                {
                    // Parallel frustum culling pass
                    EnsureCullBuffer(mergedCount);
                    TtEngine.Instance.EventPoster.ParallelFor(mergedCount, static (i, state) =>
                    {
                        var pThis = state.GetForArgument0<TtInstanceMeshNode>();
                        var visParam = state.GetForArgument1<TtWorld.TtVisParameter>();
                        var node = pThis.mResolvedMergedNodes[i];
                        var absT = node.Placement.AbsTransform;
                        var localAABB = pThis.BoundVolume.mLocalAABB;
                        var instAABB = new DBoundingBox(
                            absT.mPosition + localAABB.Minimum * absT.mScale,
                            absT.mPosition + localAABB.Maximum * absT.mScale);
                        var ct = visParam.CullCamera.WhichContainTypeFast(visParam.World, in instAABB, true);
                        pThis.mCullVisibility[i] = ct == CONTAIN_TYPE.CONTAIN_TEST_OUTER ? (byte)0 : (byte)1;
                    }, -1, this, rp);

                    // Sequential push of visible instances
                    for (int i = 0; i < mergedCount; i++)
                    {
                        if (mCullVisibility[i] == 0)
                            continue;
                        var absT = liveNodes[i].Placement.AbsTransform;
                        var inst = new FVSInstanceData();
                        inst.Position = (absT.mPosition - rp.World.CameraOffset).ToSingleVector3();
                        inst.Scale = absT.mScale;
                        inst.Quat = absT.mQuat;
                        instanceMod.PushInstance(in inst);
                    }
                }
                else
                {
                    // Serial path
                    for (int i = 0; i < mergedCount; i++)
                    {
                        var node = liveNodes[i];
                        var absT = node.Placement.AbsTransform;
                        if (doFrustumCull)
                        {
                            var instAABB = new DBoundingBox(
                                absT.mPosition + meshAABB.Minimum * absT.mScale,
                                absT.mPosition + meshAABB.Maximum * absT.mScale);
                            var ct = rp.CullCamera.WhichContainTypeFast(rp.World, in instAABB, true);
                            if (ct == CONTAIN_TYPE.CONTAIN_TEST_OUTER)
                                continue;
                        }
                        var inst = new FVSInstanceData();
                        inst.Position = (absT.mPosition - rp.World.CameraOffset).ToSingleVector3();
                        inst.Scale = absT.mScale;
                        inst.Quat = absT.mQuat;
                        instanceMod.PushInstance(in inst);
                    }
                }
            }

            if (instanceMod.InstanceBuffers.NumOfInstance > 0)
            {
                rp.AddVisibleMesh(mRenderMesh);
                rp.AddVisibleNode(this);
            }
        }

        private void EnsureCullBuffer(int count)
        {
            if (mCullVisibility == null || mCullVisibility.Length < count)
                mCullVisibility = new byte[Math.Max(count, 64)];
        }

        protected override void OnCameralOffsetChanged(TtWorld world)
        {
            // mRenderMesh uses Identity WorldMatrix (set per-frame in OnGatherVisibleMeshes)
            // Only hitproxy mesh needs camera offset update
            mHitproxyMesh?.UpdateCameraOffset(world);
        }

        protected override void OnAbsTransformChanged()
        {
            if (mHitproxyMesh == null)
                return;
            var world = this.GetWorld();
            if (mHitproxyMesh != null)
            {
                // Hitproxy billboard uses fixed scale; facing is driven by OnGatherVisibleMeshes
                var hitproxyTransform = FTransform.CreateTransform(in Placement.AbsTransform.mPosition, in Vector3.One, in Quaternion.Identity);
                mHitproxyMesh.SetWorldTransform(in hitproxyTransform, world, true);
            }
        }

        /// <summary>
        /// Override AABB to encompass all instances (manual + merged nodes).
        /// This ensures the node passes frustum culling when any instance is visible.
        /// </summary>
        public override void UpdateAABB()
        {
            if (NodeData == null || BoundVolume == null || Placement == null || HasStyle(ENodeStyles.DiscardAABB))
                return;

            var meshAABB = BoundVolume.mLocalAABB;
            if (meshAABB.IsEmpty())
            {
                base.UpdateAABB();
                return;
            }

            var worldAABB = new DBoundingBox();
            worldAABB.InitEmptyBox();

            // Manual instances
            var nodeData = NodeData as TtInstanceMeshNodeData;
            if (nodeData != null)
            {
                foreach (var t in nodeData.InstanceTransforms)
                {
                    var instMin = t.mPosition + meshAABB.Minimum * t.mScale;
                    var instMax = t.mPosition + meshAABB.Maximum * t.mScale;
                    worldAABB.Merge(in instMin);
                    worldAABB.Merge(in instMax);
                }
            }

            // Merged nodes
            var resolvedNodes = ResolveMergedNodes();
            for (int i = 0; i < resolvedNodes.Count; i++)
            {
                var node = resolvedNodes[i];
                var absT = node.Placement.AbsTransform;
                var instMin = absT.mPosition + meshAABB.Minimum * absT.mScale;
                var instMax = absT.mPosition + meshAABB.Maximum * absT.mScale;
                worldAABB.Merge(in instMin);
                worldAABB.Merge(in instMax);
            }

            // Also include the InstanceNode's own position (for the hitproxy icon)
            var selfPos = Placement.AbsTransform.mPosition;
            worldAABB.Merge(in selfPos);

            if (worldAABB.IsEmpty())
            {
                RefAABB.InitEmptyBox();
            }
            else
            {
                // Convert world AABB to node-local space (RefAABB is relative to node position)
                RefAABB.Minimum = worldAABB.Minimum - selfPos;
                RefAABB.Maximum = worldAABB.Maximum - selfPos;
            }

            // Merge children AABBs (e.g., hitproxy icon node)
            foreach (var child in Children)
            {
                if (child.HasStyle(ENodeStyles.DiscardAABB) || child.Placement == null)
                    continue;
                RefAABB = DBoundingBox.Merge(RefAABB, child.RefAABB);
            }

            if (RefAABB.IsEmpty())
            {
                RefAABB.Minimum = DVector3.Zero;
                RefAABB.Maximum = DVector3.Zero;
            }

            if (BoundVolume != null)
                DBoundingBox.TransformNoScale(in RefAABB, in Placement.AbsTransform, out RefAbsAABB);

            if (Parent != null)
                Parent.UpdateAABB();

            OnAbsAABBChanged();
        }
        #endregion

        #region Editor
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            if (mHitproxyMesh != null)
                meshes.Add(mHitproxyMesh);
        }

        public override void OnHitProxyChanged()
        {
            if (mHitproxyMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mHitproxyMesh.IsDrawHitproxy = false;
                return;
            }
            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                mHitproxyMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mHitproxyMesh.SetHitproxy(in value);
            }
            else
            {
                mHitproxyMesh.IsDrawHitproxy = false;
            }
        }

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            var meshData = NodeData as TtInstanceMeshNodeData;
            if (meshData?.MeshName != null)
                ameta.AddReferenceAsset(meshData.MeshName);
        }
        #endregion
    }
}
