using EngineNS.EGui.Controls;
using EngineNS.GamePlay.Scene;
using EngineNS.IO;
using NPOI.POIFS.Properties;
using SDL;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace EngineNS.Editor.Forms
{
    public class TtWorldOutliner : TtTreeNodeDrawer, IRootForm
    {
        public GamePlay.TtWorld World
        {
            get
            {
                return WorldViewportState.World;
            }
        }
        public EGui.Slate.TtWorldViewportSlate WorldViewportState { get; set; }

        public TtWorldOutliner(EGui.Slate.TtWorldViewportSlate viewport, bool regRoot = true)
        {
            WorldViewportState = viewport;
            if (regRoot)
                TtEngine.RootFormManager.RegRootForm(this);

            UpdateAddNodeMenu();
        }

        public void Dispose()
        {

        }
        //List<EGui.UIProxy.MenuItemProxy> mDirContextMenu;
        public virtual async Thread.Async.TtTask<bool> Initialize()
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            //mDirContextMenu = new List<EGui.UIProxy.MenuItemProxy>()
            //{
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "Goto",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            var camera = WorldViewportState.CameraController.Camera;
            //            var radius = (node.AABB.GetMaxSide()) *  5.0f;
            //            camera.LookAtLH(node.Placement.Position - camera.GetDirection().AsDVector() * radius, node.Placement.Position, Vector3.Up);
            //        },
            //    },
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "DoCommand",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            node.OnCommand("WorldOutliner");
            //        },
            //    },
            //    new EGui.UIProxy.MenuItemProxy()
            //    {
            //        MenuName = "Delete",
            //        Action = (item, data)=>
            //        {
            //            var node = data.Value.ToObject() as GamePlay.Scene.UNode;
            //            node.Parent = null;
            //        },
            //    },
            //};

            return true;
        }
        public string Title { get; set; } = "WorldOutliner";
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiWindowClass DockKeyClass { get; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public virtual unsafe void DrawAsChildWindow(in Vector2 size)
        {
            EGui.UIProxy.StyleConfig.Instance.PushPanelChildStyle(EGui.UIProxy.StyleConfig.Instance.SecondPanelBackground);
            if (ImGuiAPI.BeginChild(Title, in size, ImGuiChildFlags_.ImGuiChildFlags_Borders, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                DrawWorldTreeBody();
            }
            if (OnDrawMenu != null)
                OnDrawMenu();
            ImGuiAPI.EndChild();
            EGui.UIProxy.StyleConfig.Instance.PopPanelChildStyle();

            
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            var result = EGui.UIProxy.DockProxy.BeginMainForm(Title, this, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (result)
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                DrawWorldTreeBody();
            }
            if (OnDrawMenu != null)
                OnDrawMenu();
            EGui.UIProxy.DockProxy.EndMainForm(result);
        }

        unsafe void DrawWorldTreeBody()
        {
            if (World == null)
                return;

            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_Header, EGui.UIProxy.StyleConfig.Instance.TVHeader);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderActive, EGui.UIProxy.StyleConfig.Instance.TVHeaderActive);
            ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, EGui.UIProxy.StyleConfig.Instance.TVHeaderHovered);
            DrawTree(null, World.Root, 0);
            ImGuiAPI.PopStyleColor(3);

            DrawDeselectAllSpacer();
        }

        // 在 Outliner 树渲染完之后, 把树到窗口底部之间的空白区域铺一个 invisible button,
        // 用户在这块"树之外的空白处"单击即视为"清空选择"。
        // 设计动机:
        //   - 视口里的左键空白点击容易误触, 已经禁用清空语义 (TtSceneEditorViewport.OnHitproxySelected
        //     对 proxy == null 早返回); 但完全不给清空入口体验也不完整。
        //   - Esc 键作为通行约定已经接上 (TtSceneEditorInteractiveMode.OnEvent)。
        //   - Outliner 树空白处单击是另一个明确语义入口: 用户已经在这块面板里操作选择了,
        //     就近给一个不歧义的取消操作位置, 不需要把手离开切到视口去按 Esc。
        //   - 不歧义: 这块区域本身没有 tree node, 不会和 OnNodeUI_LClick 冲突。
        // 仅当宿主是 TtSceneEditor (有 DeselectAll 实现) 时生效, 其他子类编辑器
        // (例如 prefab 编辑器) 走自己的逻辑。
        unsafe void DrawDeselectAllSpacer()
        {
            // 把窗口剩余高度全部占满, 给 invisible button 一个最小的兜底高度,
            // 防止树占满整个 child 时 spacer 高度为 0 / 负数被 ImGui 当 invalid 跳过。
            var avail = ImGuiAPI.GetContentRegionAvail();
            const float minSpacerHeight = 24.0f;
            float h = avail.Y > minSpacerHeight ? avail.Y : minSpacerHeight;
            var size = new Vector2(avail.X > 1.0f ? avail.X : 1.0f, h);

            ImGuiAPI.InvisibleButton("##OutlinerDeselectAllSpacer", in size, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft);
            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
            {
                var sceneEditor = WorldViewportState is EGui.Slate.TtWorldViewportSlate vp
                    ? GetHostSceneEditor(vp)
                    : null;
                sceneEditor?.DeselectAll();
            }
        }

        // 取宿主 SceneEditor: 通过 viewport 反推。WorldOutliner 自身只持有 viewport,
        // 不直接持有编辑器引用, 这里做一次反查。其他派生 viewport (如非 SceneEditorViewport)
        // 没有 HostEditor 字段, 返回 null, DeselectAll 自然不会被调用。
        static TtSceneEditor GetHostSceneEditor(EGui.Slate.TtWorldViewportSlate viewport)
        {
            var seVp = viewport as TtSceneEditor.TtSceneEditorViewport;
            return seVp?.HostEditor;
        }
        protected override bool OnDrawNode(INodeUIProvider parent, INodeUIProvider provider, int index, int NumOfChild)
        {
            return provider.DrawNode(parent,this, index, NumOfChild);
            //ImGuiTreeNodeFlags_ flags = 0;
            //if (provider.Selected)
            //    flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Selected;
            //var ret = ImGuiAPI.TreeNodeEx(index.ToString(), flags, "");
            //ImGuiAPI.SameLine(0, -3);
            //ImGuiAPI.Text(provider.NodeName);
            //return ret;
        }
        #region PopMenu
        System.Action OnDrawMenu = null;
        GamePlay.Scene.TtNode mAddToNode;
        bool mNodeMenuShow = false;
        bool mAddNodeMenuFilterFocused = false;
        string mAddNodeMenuFilterStr = "";
        public TtMenuItem mAddNodeMenus = new TtMenuItem();
        static void GetNodeNameAndMenuStr(in string menuString, ref string nodeName, ref string menuName)
        {
            menuName = menuString;
            nodeName = menuName;
        }
        private async System.Threading.Tasks.Task<GamePlay.Scene.TtNode> NewNode(Rtti.TtClassMeta i)
        {
            if (mAddToNode == null)
                return null;
            var ntype = i.ClassType;
            var newNode = await GamePlay.Scene.TtNode.SpawnNode(mAddToNode, ntype.SystemType, null, null, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.TtPlacement));
            string prefix = "Node";
            var attr = GamePlay.Scene.TtNode.GetNodeAttribute(ntype.SystemType);
            if (attr != null)
            {
                prefix = attr.DefaultNamePrefix;
            }
            newNode.NodeData.Name = $"{prefix}_{newNode.SceneId}";
            return newNode;
        }
        public void UpdateAddNodeMenu()
        {
            mAddNodeMenus = new TtMenuItem();
            var typeDesc = Rtti.TtTypeDescGetter<GamePlay.Scene.TtNode>.TypeDesc;
            var meta = Rtti.TtClassMetaManager.Instance.GetMeta(typeDesc);
            var subClasses = meta.SubClasses;
            foreach (var i in subClasses)
            {
                var atts = i.ClassType.SystemType.GetCustomAttributes(typeof(Bricks.CodeBuilder.ContextMenuAttribute), inherit: false);
                if (atts.Length > 0)
                {
                    var parentMenu = mAddNodeMenus;
                    var att = atts[0] as Bricks.CodeBuilder.ContextMenuAttribute;

                    if (!att.HasKeyString(GamePlay.Scene.TtNode.EditorKeyword))
                        continue;

                    for (var menuIdx = 0; menuIdx < att.MenuPaths.Length; menuIdx++)
                    {
                        var menuStr = att.MenuPaths[menuIdx];
                        string nodeName = null;
                        GetNodeNameAndMenuStr(menuStr, ref nodeName, ref menuStr);
                        if (menuIdx < att.MenuPaths.Length - 1)
                            parentMenu = parentMenu.AddMenuItem(menuStr, null, null);
                        else
                        {
                            parentMenu.AddMenuItem(menuStr, att.FilterStrings, null,
                                (item, sender) =>
                                {
                                    var nu = NewNode(i);
                                });
                        }
                    }
                }
            }
        }
        private void DrawMenu(TtMenuItem item, string filter = "")
        {
            if (!item.FilterCheck(filter))
                return;

            if (item.OnMenuDraw != null)
            {
                item.OnMenuDraw(item, this);
                return;
            }

            if (item.SubMenuItems.Count == 0)
            {
                if (!string.IsNullOrEmpty(item.Text))
                {
                    ImGuiAPI.TreeNodeEx(item.Text, ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen);
                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                    {
                        if (item.Action != null)
                        {
                            item.Action(item, null);
                            ImGuiAPI.CloseCurrentPopup();
                        }
                    }
                }
            }
            else
            {
                if (ImGuiAPI.TreeNode(item.Text))
                {
                    for (int menuIdx = 0; menuIdx < item.SubMenuItems.Count; menuIdx++)
                    {
                        DrawMenu(item.SubMenuItems[menuIdx], filter);
                    }
                    ImGuiAPI.TreePop();
                }
            }
        }
        #endregion

        public override void OnNodeUI_RClick(INodeUIProvider provider)
        {
            var node = provider as GamePlay.Scene.TtNode;
            if (node == null)
            {
                mNodeMenuShow = false;
                OnDrawMenu = null;
                return;
            }
            var scene = provider as GamePlay.Scene.TtScene;
            OnDrawMenu = async () =>
            {
                EGui.UIProxy.StyleConfig.Instance.PushPopupStyle();
                if (ImGuiAPI.BeginPopupContextWindow(null, ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                {
                    mAddToNode = node;
                    mNodeMenuShow = true;
                    DrawBaseMenu(node);
                    ImGuiAPI.EndPopup();
                }
                else
                {
                    //mAddToNode = null;
                    if (mNodeMenuShow)
                    {
                        OnDrawMenu = null;
                    }
                    mNodeMenuShow = false;
                }
                EGui.UIProxy.StyleConfig.Instance.PopPopupStyle();
            };
        }

        protected virtual void DrawBaseMenu(GamePlay.Scene.TtNode node)
        {
            if (ImGuiAPI.MenuItem($"Goto", null, false, true))
            {
                var camera = WorldViewportState.CameraController.Camera;
                var radius = (node.RefAABB.GetMaxSide()) * 5.0f;
                camera.LookAtLH(node.Placement.Position - camera.GetDirection().AsDVector() * radius, node.Placement.Position, Vector3.Up);
            }
            if (ImGuiAPI.MenuItem($"DoCommand", null, false, true))
            {
                node.OnCommand("WorldOutliner");
            }
            if (ImGuiAPI.BeginMenu("AddChild", true))
            {
                var drawList = ImGuiAPI.GetWindowDrawList();
                EGui.UIProxy.SearchBarProxy.OnDraw(ref mAddNodeMenuFilterFocused, in drawList, "search item", ref mAddNodeMenuFilterStr, -1);
                for (var childIdx = 0; childIdx < mAddNodeMenus.SubMenuItems.Count; childIdx++)
                    DrawMenu(mAddNodeMenus.SubMenuItems[childIdx], mAddNodeMenuFilterStr.ToLower());

                ImGuiAPI.EndMenu();
            }
            if (ImGuiAPI.MenuItem($"Delete", null, false, true))
            {
                if (World.Root != node)
                {
                    node.DeleteFromScene();
                }
            }
        }
        public List<GamePlay.Scene.TtNode> SelectedNodes = new List<GamePlay.Scene.TtNode>();
        public override void OnNodeUI_LClick(INodeUIProvider provider)
        {
            //var ctrlKeyDown = TtEngine.Instance.InputSystem.IsCtrlKeyDown();
            //if (ctrlKeyDown)
            //{

            //}
            //else
            //{
            //    for(int i=0; i<mSelectNodes.Count; i++)
            //    {
            //        mSelectNodes[i].Selected = false;
            //    }
            //    mSelectNodes.Clear();
            //    mSelectNodes.Add(provider);
            //    provider.Selected = true;
            //    WorldViewportState.OnHitproxySelected((GamePlay.Scene.UNode)provider);
            //}

            //var appliction = TtEngine.Instance.GfxDevice.MainWindow as EngineNS.Editor.UMainEditorApplication;
            //if (appliction == null)
            //    return;
            //appliction.mMainInspector.PropertyGrid.Target = provider;
            //appliction.WorldViewportSlate.OnHitproxySelected((GamePlay.Scene.UNode)provider);

            var node = provider as GamePlay.Scene.TtNode;
            if (node == null)
                return;

            var key = ImGuiAPI.GetImGuiKey(Bricks.Input.Keycode.KEY_LCTRL);
            if (ImGuiAPI.IsKeyDown(key))
            {
                node.Selected = !node.Selected;
                if (node.Selected == false)
                {
                    SelectedNodes.Remove(node);
                }
                else
                {
                    if (SelectedNodes.Contains(node) == false)
                    {
                        SelectedNodes.Add(node);
                    }
                }
            }
            else
            {
                foreach (var i in SelectedNodes)
                {
                    i.Selected = false;
                }
                SelectedNodes.Clear();
                node.Selected = true;
                SelectedNodes.Add(node);
            }

            WorldViewportState.OnHitproxySelectedMulti(true, SelectedNodes.ToArray());
        }
    }
}
