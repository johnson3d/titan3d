using EngineNS.Animation.SkeletonAnimation.AnimatablePose;
using EngineNS.Animation.SkeletonAnimation.Runtime.Pose;
using EngineNS.Bricks.Animation.Macross.StateMachine;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design;
using EngineNS.DesignMacross.Editor;
using EngineNS.EGui.Controls;
using EngineNS.GamePlay.Scene;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Text;

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
                var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                return animatablePose;
            }
            return null;
        }
        public static TtLocalSpaceRuntimePose BindRuntimeSkeletonPoseToNode(TtNode node)
        {
            var meshNode = GetParentMeshNode(node);
            if (meshNode != null)
            {
                var animatablePose = meshNode?.Mesh?.MaterialMesh?.SubMeshes[0].Mesh?.PartialSkeleton?.CreatePose() as SkeletonAnimation.AnimatablePose.TtAnimatableSkeletonPose;
                var skinMDfQueue = meshNode.Mesh.MdfQueue as Graphics.Mesh.UMdfSkinMesh;
                var animatedPose = SkeletonAnimation.Runtime.Pose.TtRuntimePoseUtility.CreateLocalSpaceRuntimePose(animatablePose);
                skinMDfQueue.SkinModifier.RuntimePose = animatedPose;
                return animatedPose;
            }
            return null;
        }
    }

    public class PGStateMachineSelectAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
    {
        public string mSelectedStateMachine = "None";
        public Guid mSelectedStateMachineId = Guid.Empty;
        protected override async Task<bool> Initialize_Override()
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
            if (TtEngine.Instance.GfxDevice.SlateApplication is EngineNS.Editor.UMainEditorApplication mainEditor)
            {
                if(mainEditor.AssetEditorManager.CurrentActiveEditor is TtDesignMacrossEditor designMacrossEditor)
                {
                    return designMacrossEditor.DesignMacross.DesignedClassDescription;
                }
            }
            return null;
        }
    }
}
