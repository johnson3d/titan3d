using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.Animation.Macross.StateMachine;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.GamePlay.Scene;
using EngineNS.IO;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute;

namespace EngineNS.Animation
{
    public class TtAnimUtil
    {
        public static TtMeshNode GetParentMeshNode(TtNode node)
        {
            if(node == null) return null;
            if (node is TtMeshNode meshNode)
            {
                return meshNode;
            }
            if (node.Parent == null) return null;

            if(node.Parent is TtMeshNode parentMeshNode)
            {
                return parentMeshNode;
            }
            else
            {
                return GetParentMeshNode(node.Parent);
            }
        }
        public static TtAnimatableSkeletonPose CreateAnimatableSkeletonPoseFromeNode(TtNode node)
        {
            var meshNode = GetParentMeshNode(node);
            if (meshNode != null)
            {
                var animatablePose = meshNode?.RenderMesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                return animatablePose;
            }
            return null;
        }
        public static TtLocalSpaceRuntimePose BindRuntimeSkeletonPoseToNode(TtNode node)
        {
            var meshNode = GetParentMeshNode(node);
            if (meshNode != null && meshNode.HasSkin)
            {
                //var animatablePose = meshNode?.MaterialMesh?.GetSubMeshPrimitives(0)?.PartialSkeleton?.CreateSkeletonPose();
                var animatablePose = meshNode?.MaterialMesh?.GetMainSkeleton()?.CreateSkeletonPose();
                //var skinMDfQueue = meshNode.RenderMesh.MdfQueue as Graphics.Mesh.TtMdfSkinMesh;
                var animatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
                meshNode.RuntimePose = animatedPose;
                return meshNode.RuntimePose;
            }
            return null;
        }
    }
    public class PGBlendSpaceValueBindSelectAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        public string mSelectedValueBind = "None";
        public Guid mSelectedValueBindId = Guid.Empty;
        protected override async Thread.Async.TtTask<bool> Initialize_Override()
        {
            return await base.Initialize_Override();
        }
        ~PGBlendSpaceValueBindSelectAttribute()
        {
            Cleanup();
        }
        protected override void Cleanup_Override()
        {
            base.Cleanup_Override();
        }

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            if (info.Value != null)
            {
                mSelectedValueBindId = (Guid)info.Value;
                if (GetActiveEditorClassDescription() != null)
                {
                    foreach (var variables in GetActiveEditorClassDescription().Variables)
                    {
                        if (variables is TtVariableDescription variable)
                        {
                            if (variable.Id == mSelectedValueBindId)
                            {
                                mSelectedValueBind = variable.VariableName;
                            }
                        }
                    }
                }
            }
            else
            {
                mSelectedValueBindId = Guid.Empty;
                mSelectedValueBind = "None";
            }

            if (EGui.UIProxy.ComboBox.BeginCombo("##SelectVariable", mSelectedValueBind))
            {
                var comboDrawList = ImGuiAPI.GetWindowDrawList();
                var searchBar = TtEngine.Instance.UIProxyManager["VariableDescSearchBar"] as EGui.UIProxy.SearchBarProxy;
                if (searchBar == null)
                {
                    searchBar = new EGui.UIProxy.SearchBarProxy()
                    {
                        InfoText = "Search macross base type",
                        Width = -1,
                    };
                    TtEngine.Instance.UIProxyManager["VariableDescSearchBar"] = searchBar;
                }
                if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                    ImGuiAPI.SetKeyboardFocusHere(0);
                searchBar.OnDraw(in comboDrawList, in Support.TtAnyPointer.Default);
                bool bSelected = true;
                List<(string Name, Guid Id)> VariableIdName = new List<(string, Guid)>();
                VariableIdName.Add(("None", Guid.Empty));
                if (GetActiveEditorClassDescription() != null)
                {
                    foreach (var variables in GetActiveEditorClassDescription().Variables)
                    {
                        if (variables is TtVariableDescription variable)
                        {
                            VariableIdName.Add((variable.VariableName, variable.Id));
                        }
                    }
                }

                foreach (var idName in VariableIdName)
                {
                    if (!string.IsNullOrEmpty(searchBar.SearchText) && !idName.Name.ToLower().Contains(searchBar.SearchText.ToLower()))
                        continue;

                    if (ImGuiAPI.Selectable(idName.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        mSelectedValueBindId = idName.Id;
                        mSelectedValueBind = idName.Name;
                        newValue = mSelectedValueBindId;
                    }
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        CtrlUtility.DrawHelper(idName.Name);
                    }
                }
                EGui.UIProxy.ComboBox.EndCombo();
            }

            return true;
        }

        public TtClassDescription GetActiveEditorClassDescription()
        {
            if (TtEngine.Instance.GfxDevice.SlateApplication is EngineNS.Editor.TtMainEditorApplication mainEditor)
            {
                if (mainEditor.AssetEditorManager.CurrentActiveEditor is TtDesignMacrossEditor designMacrossEditor)
                {
                    return designMacrossEditor.DesignMacross.DesignedClassDescription;
                }
            }
            return null;
        }
    }

    public class PGStateMachineSelectAttribute : EGui.Controls.PropertyGrid.TtPGCustomValueEditorAttribute
    {
        public string mSelectedStateMachine = "None";
        public Guid mSelectedStateMachineId = Guid.Empty;
        protected override async Thread.Async.TtTask<bool> Initialize_Override()
        {


            return await base.Initialize_Override();
        }
        ~PGStateMachineSelectAttribute()
        {
            Cleanup();
        }
        protected override void Cleanup_Override()
        {
            base.Cleanup_Override();
        }

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            newValue = info.Value;
            if (info.Value != null)
            {
                mSelectedStateMachineId = (Guid)info.Value;
                if(GetActiveEditorClassDescription() != null)
                {
                    foreach (var designableVariables in GetActiveEditorClassDescription().DesignableVariables)
                    {
                        if (designableVariables is TtAnimStateMachineClassDescription animStateMachine)
                        {
                            if (animStateMachine.Id == mSelectedStateMachineId)
                            {
                                mSelectedStateMachine = animStateMachine.VariableName;
                            }
                        }
                    }
                }
            }
            else
            {
                mSelectedStateMachineId = Guid.Empty;
                mSelectedStateMachine = "None";
            }

            if (EGui.UIProxy.ComboBox.BeginCombo("##SelectSM", mSelectedStateMachine))
            {
                var comboDrawList = ImGuiAPI.GetWindowDrawList();
                var searchBar = TtEngine.Instance.UIProxyManager["SMDescSearchBar"] as EGui.UIProxy.SearchBarProxy;
                if (searchBar == null)
                {
                    searchBar = new EGui.UIProxy.SearchBarProxy()
                    {
                        InfoText = "Search macross base type",
                        Width = -1,
                    };
                    TtEngine.Instance.UIProxyManager["SMDescSearchBar"] = searchBar;
                }
                if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                    ImGuiAPI.SetKeyboardFocusHere(0);
                searchBar.OnDraw(in comboDrawList, in Support.TtAnyPointer.Default);
                bool bSelected = true;
                List<(string Name, Guid Id)> SMIdName = new List<(string, Guid)>();
                SMIdName.Add(("None", Guid.Empty));
                if (GetActiveEditorClassDescription() != null)
                {
                    foreach (var designableVariables in GetActiveEditorClassDescription().DesignableVariables)
                    {
                        if (designableVariables is TtAnimStateMachineClassDescription animStateMachine)
                        {
                            SMIdName.Add((animStateMachine.VariableName, animStateMachine.Id));
                        }
                    }
                }

                foreach(var idName in SMIdName)
                {
                    if (!string.IsNullOrEmpty(searchBar.SearchText) && !idName.Name.ToLower().Contains(searchBar.SearchText.ToLower()))
                        continue;

                    if (ImGuiAPI.Selectable(idName.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                    {
                        mSelectedStateMachineId = idName.Id;
                        mSelectedStateMachine = idName.Name;
                        newValue = mSelectedStateMachineId;
                    }
                    if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                    {
                        CtrlUtility.DrawHelper(idName.Name);
                    }
                }
                EGui.UIProxy.ComboBox.EndCombo();
            }

            return true;
        }

        public TtClassDescription GetActiveEditorClassDescription()
        {
            if (TtEngine.Instance.GfxDevice.SlateApplication is EngineNS.Editor.TtMainEditorApplication mainEditor)
            {
                if(mainEditor.AssetEditorManager.CurrentActiveEditor is TtDesignMacrossEditor designMacrossEditor)
                {
                    return designMacrossEditor.DesignMacross.DesignedClassDescription;
                }
            }
            return null;
        }
    }
    [Rtti.Meta("")]
    public class LimbIndexInSkeleton: BaseSerializer, System.IEquatable<LimbIndexInSkeleton>
    {
        [Rtti.Meta("")]
        public string Name { get; set; } = "";
        [Rtti.Meta("")]
        public int Index { get; set; } = -1;
        [Rtti.Meta("")]
        public string Skeleton { get; set; } = "";
        public LimbIndexInSkeleton()
        {

        }
        public LimbIndexInSkeleton(string name, int index, string skeleton)
        {
            Name = name;
            Index = index;
            Skeleton = skeleton;
        }
        public static LimbIndexInSkeleton CrteateDefault()
        {
            return new LimbIndexInSkeleton("", -1, "");
        }

        public bool Equals(LimbIndexInSkeleton other)
        {
            return Name == other.Name && Index == other.Index && Skeleton == other.Skeleton;
        }
    }

    public class TtSkeletonBoneIndexPickerEditorAttribute : TtPGCustomValueEditorAttribute
    {
        string mCachedPreviewString = "";
        LimbIndexInSkeleton mSelected;
        List<Animation.Asset.TtSkeletonAsset> mAllSkeletonAssets = null;
        EGui.UIProxy.ComboBox mComboBox = new EGui.UIProxy.ComboBox();

        protected override async Thread.Async.TtTask<bool> Initialize_Override()
        {
            var rNames = new List<RName>();
            TtEngine.Instance.AssetMetaManager.TourAssetMetas<Animation.Asset.TtSkeletonAssetAMeta, List<RName>>(
                (rName, meta, list) => { list.Add(rName); return false; }, rNames);

            mAllSkeletonAssets = new List<Animation.Asset.TtSkeletonAsset>();
            foreach (var rName in rNames)
            {
                var asset = await TtEngine.Instance.AnimationModule.SkeletonAssetManager.GetSkeletonAsset(rName);
                if (asset != null)
                {
                    bool contains = false;
                    foreach (var skeletonAsset in mAllSkeletonAssets)
                    {
                        if (skeletonAsset.AssetName == rName)
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (!contains)
                    {
                        mAllSkeletonAssets.Add(asset);
                    }
                }

            }

            mComboBox.Flags = ImGuiComboFlags_.ImGuiComboFlags_HeightLarge;
            mComboBox.WinFlags = ImGuiWindowFlags_.ImGuiWindowFlags_Popup |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoTitleBar |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoResize |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoMove;
            mComboBox.Width = -1;
            await mComboBox.Initialize();

            return await base.Initialize_Override();
        }

        public override unsafe bool OnDraw(in EditorInfo info, out object newValue)
        {
            var infoValue = info.Value as LimbIndexInSkeleton;
            if(infoValue == null)
            {
                infoValue = LimbIndexInSkeleton.CrteateDefault();
            }
            newValue = infoValue;
            var currentVal = infoValue;
            var currentBoneIdx = currentVal.Index;
            var currentSkelRName = currentVal.Skeleton;

            mCachedPreviewString = BuildPreviewString(currentVal);
            var preview = mCachedPreviewString;
            if (string.IsNullOrEmpty(preview))
                preview = "None";

            mComboBox.Name = "##SkeletonBonePicker_" + info.Name;
            mComboBox.PreviewValue = preview;

            bool comboChanged = false;
            var comboNewVal = currentVal;

            mComboBox.ComboOpenAction = (in Support.TtAnyPointer data) =>
            {
                var comboDrawList = ImGuiAPI.GetWindowDrawList();

                // 收藏夹分段: 骨架编辑器 BoneDetails 里 [+] 进来的骨骼排在最上面,
                // 免得每次都要在"所有骨架 × 所有骨骼"的全量树里翻
                var favorites = Editor.Infrastructure.TtEditorFavoritePaths.Get(
                    Editor.Infrastructure.TtEditorFavoritePaths.ChannelBone);
                if (favorites.Count > 0)
                {
                    ImGuiAPI.TextDisabled("Favorites");
                    for (int fi = 0; fi < favorites.Count; fi++)
                    {
                        var fav = favorites[fi];
                        if (!TryResolveFavoritePath(fav.Path, out var favVal))
                            continue;
                        var favLabel = $"[{favVal.Index}] {favVal.Name} ({favVal.Skeleton})##fav{fi}";
                        bool favSel = (favVal.Skeleton == currentSkelRName && favVal.Index == currentBoneIdx);
                        if (ImGuiAPI.Selectable(favLabel, ref favSel, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                        {
                            comboNewVal = favVal;
                            comboChanged = true;
                        }
                    }
                    ImGuiAPI.Separator();
                }

                var searchBar = TtEngine.Instance.UIProxyManager["SkeletonBonePickerSearchBar"] as EGui.UIProxy.SearchBarProxy;
                if (searchBar == null)
                {
                    searchBar = new EGui.UIProxy.SearchBarProxy()
                    {
                        InfoText = "Search bone...",
                        Width = -1,
                    };
                    TtEngine.Instance.UIProxyManager["SkeletonBonePickerSearchBar"] = searchBar;
                }
                if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                    ImGuiAPI.SetKeyboardFocusHere(0);
                searchBar.OnDraw(in comboDrawList, in Support.TtAnyPointer.Default);

                bool hasSearch = !string.IsNullOrEmpty(searchBar.SearchText);

                if (mAllSkeletonAssets != null)
                {
                    for (int si = 0; si < mAllSkeletonAssets.Count; si++)
                    {
                        var skeletonAsset = mAllSkeletonAssets[si];
                        if (skeletonAsset?.Skeleton == null) continue;

                        var skelRName = skeletonAsset.AssetName;
                        var rawName = skeletonAsset.AssetName?.Name ?? "Unknown";
                        var skelDisplayName = rawName.EndsWith(".skt") ? rawName.Substring(0, rawName.Length - 4) : rawName;

                        if (hasSearch)
                        {
                            // Search: show matching bones as flat Selectable items
                            if (skeletonAsset.Skeleton.Limbs == null) continue;
                            var searchLower = searchBar.SearchText.ToLower();
                            foreach (var limb in skeletonAsset.Skeleton.Limbs)
                            {
                                var boneName = limb.Desc?.Name;
                                if (string.IsNullOrEmpty(boneName)) continue;
                                if (!boneName.ToLower().Contains(searchLower)) continue;

                                var bi = limb.Index.Value;
                                var label = $"[{bi}] {boneName} ({skelDisplayName})";
                                bool sel = (skelDisplayName == currentSkelRName && bi == currentBoneIdx);
                                if (ImGuiAPI.Selectable(label, ref sel, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                                {
                                    comboNewVal = new LimbIndexInSkeleton(boneName, bi, skelDisplayName);
                                    comboChanged = true;
                                }
                            }
                        }
                        else
                        {
                            // Tree display
                            ImGuiTreeNodeFlags_ skFlags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
                            if (skelDisplayName == currentSkelRName)
                                skFlags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_DefaultOpen;
                            bool skOpen = ImGuiAPI.TreeNodeEx(skelDisplayName, skFlags, skelDisplayName);

                            if (skOpen)
                            {
                                foreach (var child in skeletonAsset.Skeleton.Children)
                                {
                                    if (DrawBoneTreeItem(child, skelDisplayName, currentSkelRName, currentBoneIdx, skelDisplayName, out var selVal))
                                    {
                                        comboNewVal = selVal;
                                        comboChanged = true;
                                    }
                                }
                                ImGuiAPI.TreePop();
                            }
                        }
                    }
                }
            };

            ImGuiAPI.SetNextWindowSize(Vector2.Zero, ImGuiCond_.ImGuiCond_Appearing);
            var drawList = ImGuiAPI.GetWindowDrawList();
            mComboBox.OnDraw(in drawList, in Support.TtAnyPointer.Default);

            if (comboChanged)
            {
                mSelected = comboNewVal;
                mCachedPreviewString = BuildPreviewString(comboNewVal);
                newValue = comboNewVal;
                return true;
            }

            return false;
        }

        private unsafe bool DrawBoneTreeItem(Animation.SkeletonAnimation.Skeleton.Limb.ILimb limb, string skeletonRName, string currentSkelRName, int currentBoneIdx, string skeletonDisplayName, out LimbIndexInSkeleton selectedValue)
        {
            selectedValue = LimbIndexInSkeleton.CrteateDefault();
            if (limb == null) return false;

            var boneName = limb.Desc?.Name ?? "Unnamed";
            var boneIndex = limb.Index.Value;
            var label = $"[{boneIndex}] {boneName} ({skeletonDisplayName})";

            bool hasChildren = limb.Children != null && limb.Children.Count > 0;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None;
            if (!hasChildren)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf;
            if (skeletonRName == currentSkelRName && boneIndex == currentBoneIdx)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;

            bool open = ImGuiAPI.TreeNodeEx(label, flags, label);

            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                selectedValue = new LimbIndexInSkeleton(boneName, boneIndex, skeletonRName);
            }

            if (open)
            {
                if (hasChildren)
                {
                    foreach (var child in limb.Children)
                    {
                        if (DrawBoneTreeItem(child, skeletonRName, currentSkelRName, currentBoneIdx, skeletonDisplayName, out var childVal))
                        {
                            selectedValue = childVal;
                        }

                    }
                }
                ImGuiAPI.TreePop();
            }

            return selectedValue.Index >= 0;
        }

        /// <summary>
        /// 把收藏夹里的 "骨架资产名:骨骼名" 解析回 LimbIndexInSkeleton。
        /// 收藏夹只存名字不存 Index, 所以 Index 在这里按当前骨架重新解析, 骨架重导入也不会指错。
        /// Skeleton 字段与现有选择逻辑保持一致: 存去掉 ".skt" 的显示名。
        /// </summary>
        private bool TryResolveFavoritePath(string path, out LimbIndexInSkeleton value)
        {
            value = null;
            if (mAllSkeletonAssets == null)
                return false;
            if (!Editor.Infrastructure.TtEditorFavoritePaths.TryParseBonePath(path, out var skeletonName, out var boneName))
                return false;

            for (int i = 0; i < mAllSkeletonAssets.Count; i++)
            {
                var asset = mAllSkeletonAssets[i];
                if (asset?.Skeleton?.Limbs == null)
                    continue;
                var rawName = asset.AssetName?.Name ?? "";
                var displayName = rawName.EndsWith(".skt") ? rawName.Substring(0, rawName.Length - 4) : rawName;
                // 收藏夹里存的是带 .skt 的完整资产名; 也容忍只给骨骼名或去扩展名的写法
                if (!string.IsNullOrEmpty(skeletonName) && skeletonName != rawName && skeletonName != displayName)
                    continue;

                foreach (var limb in asset.Skeleton.Limbs)
                {
                    if (limb.Desc?.Name != boneName)
                        continue;
                    value = new LimbIndexInSkeleton(boneName, limb.Index.Value, displayName);
                    return true;
                }
            }
            return false;
        }

        private string BuildPreviewString(LimbIndexInSkeleton val)
        {
            if (val.Index < 0) return "None";
            var skelName = val.Skeleton ?? "Unknown";
            if (skelName.EndsWith(".skt")) skelName = skelName.Substring(0, skelName.Length - 4);
            var boneName = string.IsNullOrEmpty(val.Name) ? "Unknown" : val.Name;
            return $"[{val.Index}] {boneName} ({skelName})";
        }
    }
}
