﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.EGui.Controls
{
    public class ContentBrowser : Editor.IRootForm, EGui.IPanel
    {
        public ContentBrowser()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public int Index = 0;
        bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public async Task<bool> Initialize()
        {
            await Thread.AsyncDummyClass.DummyFunc();

            if (mCreateMenuItems.Count == 0)
                OnTypeChanged();

            Rtti.UTypeDescManager.Instance.OnTypeChanged += OnTypeChanged;

            return true;
        }

        struct stMenuItem
        {
            public Rtti.UTypeDesc TypeDesc;
            public string Label;
            public string Shortcut;
            public string AssetExt;
            public bool Selected;
            public bool Enabled;

            public void Reset()
            {
                TypeDesc = null;
                Label = "Unknow";
                Shortcut = null;
                AssetExt = null;
                Selected = false;
                Enabled = true;
            }
        }
        List<stMenuItem> mCreateMenuItems = new List<stMenuItem>();
        void OnTypeChanged()
        {
            mCreateMenuItems.Clear();

            foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
            {
                foreach(var typeDesc in service.Types.Values)
                {
                    var atts = typeDesc.SystemType.GetCustomAttributes(typeof(IO.AssetCreateMenuAttribute), true);
                    if (atts.Length == 0)
                        continue;

                    var assetExtField = typeDesc.SystemType.GetField("AssetExt");
                    
                    var menuItem = new stMenuItem()
                    {
                        Label = ((IO.AssetCreateMenuAttribute)atts[0]).MenuName,
                        Shortcut = ((IO.AssetCreateMenuAttribute)atts[0]).Shortcut,
                        Selected = false,
                        Enabled = true,
                        TypeDesc = typeDesc,
                        AssetExt = (string)assetExtField.GetValue(null),
                    };
                    mCreateMenuItems.Add(menuItem);
                }
            }
        }

        public RName CurrentDir;
        public void DrawDirectories(RName root, Vector2 size)
        {
            if (ImGuiAPI.BeginChild("LeftWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
                if (root == CurrentDir)
                    flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;
                if (ImGuiAPI.TreeNodeEx(root.RNameType.ToString(), flags))
                {
                    if (ImGuiAPI.IsItemActivated())
                    {
                        CurrentDir = root;
                    }
                    var dirs = IO.FileManager.GetDirectories(root.Address, "*.*", false);
                    foreach (var i in dirs)
                    {
                        var nextDirName = IO.FileManager.GetRelativePath(root.Address, i);
                        DrawTree(root.RNameType, root.Name, nextDirName);
                    }
                    ImGuiAPI.TreePop();
                }
            }
            ImGuiAPI.EndChild();
        }
        private void DrawTree(RName.ERNameType type, string parentDir, string dirName)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
            var nextParent = parentDir + dirName + "/";
            var rn = RName.GetRName(nextParent, type);
            if (rn == CurrentDir)
                flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;
            
            if (ImGuiAPI.TreeNodeEx(dirName, flags))
            {   
                if (ImGuiAPI.IsItemActivated())
                {
                    CurrentDir = rn;
                }
                var path = RName.GetRName(nextParent, type).Address;
                var dirs = IO.FileManager.GetDirectories(path, "*.*", false);
                foreach(var i in dirs)
                {
                    var nextDirName = IO.FileManager.GetRelativePath(path, i);
                    DrawTree(type, nextParent, nextDirName);
                }
                ImGuiAPI.TreePop();
            }
            else
            {
                if (ImGuiAPI.IsItemActivated())
                {
                    CurrentDir = rn;
                }
            }
        }
        public unsafe void DrawFiles(RName dir, Vector2 size)
        {
            var itemSize = new Vector2(80, 100);
            if (ImGuiAPI.BeginChild("RightWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
                var width = ImGuiAPI.GetWindowContentRegionWidth();
                var files = IO.FileManager.GetFiles(dir.Address, "*.ameta", false);
                float curPos = 0;
                foreach(var i in files)
                {
                    var name = IO.FileManager.GetLastestPathName(i.Substring(0, i.Length - 6));
                    var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType));
                    if (ameta == null)
                        continue;
                    DrawItem(ref cmdlist, ameta.Icon, ameta, ref itemSize);
                    curPos += itemSize.X + 2;
                    if (curPos + itemSize.X < width)
                    {
                        ImGuiAPI.SameLine(0, 2);
                    }
                    else
                    {
                        curPos = 0;
                    }
                }

                if (ImGuiAPI.BeginPopupContextWindow("##ContentFilesMenuWindow", ImGuiPopupFlags_.ImGuiPopupFlags_MouseButtonRight))
                {
                    if (ImGuiAPI.BeginMenu("New Asset", true))
                    {
                        //if (ImGuiAPI.MenuItem($"Texture", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(RHI.CShaderResourceView).FullName), IO.UAssetMetaManager.AssetType.CShaderResourceView);
                        //    }
                        //}
                        //if (ImGuiAPI.MenuItem($"UVAnim", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(UVAnim).FullName), IO.UAssetMetaManager.AssetType.UVAnim);
                        //    }
                        //}
                        //if (ImGuiAPI.MenuItem($"Material", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Graphics.Pipeline.Shader.UMaterial).FullName), IO.UAssetMetaManager.AssetType.UMaterial);
                        //    }
                        //}
                        //if (ImGuiAPI.MenuItem($"MaterialInstance", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Graphics.Pipeline.Shader.UMaterialInstance).FullName), IO.UAssetMetaManager.AssetType.UMaterialInstance);
                        //    }
                        //}
                        //if (ImGuiAPI.MenuItem($"Mesh", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Graphics.Mesh.CMeshPrimitives).FullName), IO.UAssetMetaManager.AssetType.CMeshPrimitives);
                        //    }
                        //}
                        //if (ImGuiAPI.MenuItem($"MaterialMesh", null, false, true))
                        //{
                        //    if (CurrentDir != null)
                        //    {
                        //        mAssetImporter = IO.UAssetMetaManager.Instance.ImportAsset(CurrentDir,
                        //            Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(Graphics.Mesh.UMaterialMesh).FullName), IO.UAssetMetaManager.AssetType.UMaterialMesh);
                        //    }
                        //}

                        for(int i=0; i<mCreateMenuItems.Count; ++i)
                        {
                            if(ImGuiAPI.MenuItem(mCreateMenuItems[i].Label, mCreateMenuItems[i].Shortcut, mCreateMenuItems[i].Selected, mCreateMenuItems[i].Enabled))
                            {
                                mAssetImporter = UEngine.Instance.AssetMetaManager.ImportAsset(CurrentDir, mCreateMenuItems[i].TypeDesc, mCreateMenuItems[i].AssetExt);
                            }
                        }

                        //----------------------------------------------------
                        ImGuiAPI.EndMenu();
                    }
                    ImGuiAPI.EndPopup();
                }
            }
            ImGuiAPI.EndChild();
        }
        private void DrawItem(ref ImDrawList cmdlist, UVAnim icon, IO.IAssetMeta ameta, ref Vector2 sz)
        {
            ImGuiAPI.Selectable($"##{ameta.GetAssetName().Name}", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz);
            if (ImGuiAPI.IsItemVisible())
            {
                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                {
                    CtrlUtility.DrawHelper(ameta.GetAssetName().Name, ameta.Description);
                }
                if (ImGuiAPI.IsMouseDoubleClicked(ImGuiMouseButton_.ImGuiMouseButton_Left) && ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                    {
                        var type = Rtti.UTypeDescManager.Instance.GetTypeFromString(ameta.TypeStr);
                        if (type != null)
                        {
                            var attrs = type.GetCustomAttributes(typeof(Editor.UAssetEditorAttribute), false);
                            if (attrs.Length > 0)
                            {
                                var editorAttr = attrs[0] as Editor.UAssetEditorAttribute;
                                var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, editorAttr.EditorType, ameta.GetAssetName(), null);
                            }
                        }
                    }
                }
                ameta.ShowIconTime = UEngine.Instance.CurrentTickCount;
                ameta.OnDraw(ref cmdlist, ref sz, this);
            }
        }
        Vector2 LeftSize;
        Vector2 RightSize;
        public IO.IAssetCreateAttribute mAssetImporter;
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;

//            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin("ContentBrowser" + Index, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
//                if (ImGuiAPI.IsWindowDocked())
//                {
//                    DockId = ImGuiAPI.GetWindowDockID();
//                }
                var cltMin = ImGuiAPI.GetWindowContentRegionMin();
                var cltMax = ImGuiAPI.GetWindowContentRegionMax();

                ImGuiAPI.Columns(2, null, true);

                LeftSize.X = ImGuiAPI.GetColumnWidth(0);
                LeftSize.Y = cltMax.Y - cltMin.Y;

                DrawDirectories(RName.GetRName("", RName.ERNameType.Game), LeftSize);
                ImGuiAPI.NextColumn();

                RightSize.X = ImGuiAPI.GetColumnWidth(1);
                RightSize.Y = cltMax.Y - cltMin.Y;
                if (CurrentDir != null)
                    DrawFiles(CurrentDir, RightSize);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            ImGuiAPI.End();

            if (mAssetImporter != null)
                mAssetImporter.OnDraw(this);
        }
    }
}