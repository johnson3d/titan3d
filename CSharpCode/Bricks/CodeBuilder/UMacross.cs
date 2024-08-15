using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.EGui.Controls;
using EngineNS.IO;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class ContextMenuAttribute : Attribute
    {
        // --------------------------------------
        public string[] MenuPaths;  // \分隔
        // 可使用符号
        // @serial@ 序列号,serial和@之前可以任意加字符，serial会替换为序列号
        // --------------------------------------
        public string FilterStrings; // ,分隔
        public string[] KeyStrings;

        public ContextMenuAttribute(string filterStrings, string menuPaths, params string[] keyStrings)
        {
            MenuPaths = menuPaths.Split('\\');
            FilterStrings = filterStrings;
            KeyStrings = keyStrings;
        }
        public bool HasKeyString(string keyString)
        {
            for(int i=0; i<KeyStrings.Length; i++)
            {
                if (KeyStrings[i] == keyString)
                    return true;
            }
            return false;
        }
    }

    public class MacrossContextMenuData
    {
        public delegate bool Delegate_MenuIsVisible(MacrossContextMenuData data);
        public delegate bool Delegate_MenuNodeShow(MacrossContextMenuData data);

        public string ParentMenuName;
        public string FilterText;
        public Vector2 PosMenu;
    }

    public partial class UMacrossAMeta : IO.IAssetMeta
    {
        [Rtti.Meta]
        public string BaseTypeStr { get; set; }
        public override string GetAssetExtType()
        {
            return UMacross.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "Macross";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            return null;
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.TtFileManager.DeleteDirectory(address);
            IO.TtFileManager.DeleteFile(address + IAssetMeta.MetaExt);

            //if(UMacrossEditor.RemoveAssemblyDescCreateInstanceCode(name, type))
            {
                EngineNS.TtEngine.Instance.MacrossManager.GenerateProjects();
                var assemblyFile = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.EngineSource) + TtEngine.Instance.EditorInstance.Config.GameAssembly;
                if (TtEngine.Instance.MacrossModule.CompileCode(assemblyFile))
                {
                    TtEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
                }
            }
        }
        public override bool CanRefAssetType(IAssetMeta ameta)
        {
            // macross可以引用所有类型的资源
            return true;
        }
        //public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        //{
        //    TtEngine.Instance.EditorInstance.MacrossIcon?.OnDraw(cmdlist, in start, in end, 0);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "Macross", null);
        //}
    }

    [Rtti.Meta]
    [UMacross.MacrossCreate]
    [IO.AssetCreateMenu(MenuName = "Script/Macross")]
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.MacrossNode.UMacrossEditor))]
    public partial class UMacross : IO.IAsset
    {
        public const string AssetExt = ".macross";
        public const string MacrossEditorKeyword = "Macross";

        public class MacrossCreateAttribute : IO.CommonCreateAttribute
        {
            UTypeDesc mSelectedType = null;
            public override async Thread.Async.TtTask DoCreate(RName dir, UTypeDesc type, string ext)
            {
                await base.DoCreate(dir, type, ext);
                mSelectedType = null;
            }

            public override unsafe bool OnDraw(UContentBrowser contentBrowser)
            {
                //base.OnDraw(contentBrowser);
                
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"New Macross", ImGuiPopupFlags_.ImGuiPopupFlags_None);

                var visible = true;
                var retValue = false;
                if (ImGuiAPI.BeginPopupModal($"New Macross", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    switch(eErrorType)
                    {
                        case enErrorType.IsExisting:
                            {
                                var clr = new Vector4(1, 0, 0, 1);
                                ImGuiAPI.TextColored(&clr, $"{mName} is existing");
                            }
                            break;
                        case enErrorType.EmptyName:
                            {
                                var clr = new Vector4(1, 0, 0, 1);
                                ImGuiAPI.TextColored(&clr, $"Name is empty");
                            }
                            break;
                    }

                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.Text("Base Type:");
                    ImGuiAPI.SameLine(0, -1);
                    if(EGui.UIProxy.ComboBox.BeginCombo("##TypeSel", (mSelectedType == null)? "None" : mSelectedType.Name))
                    {
                        var comboDrawList = ImGuiAPI.GetWindowDrawList();
                        var searchBar = TtEngine.Instance.UIProxyManager["MacrossTypeSearchBar"] as EGui.UIProxy.SearchBarProxy;
                        if(searchBar == null)
                        {
                            searchBar = new EGui.UIProxy.SearchBarProxy()
                            {
                                InfoText = "Search macross base type",
                                Width = -1,
                            };
                            TtEngine.Instance.UIProxyManager["MacrossTypeSearchBar"] = searchBar;
                        }
                        if(!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
                            ImGuiAPI.SetKeyboardFocusHere(0);
                        searchBar.OnDraw(in comboDrawList, in Support.UAnyPointer.Default);
                        bool bSelected = true;
                        foreach (var service in Rtti.UTypeDescManager.Instance.Services.Values)
                        {
                            foreach (var type in service.Types.Values)
                            {
                                if (type.IsValueType)
                                    continue;
                                if (type.IsSealed)
                                    continue;

                                var atts = type.GetCustomAttributes(typeof(Macross.UMacrossAttribute), false);
                                if (atts == null || atts.Length == 0)
                                    continue;

                                if (!string.IsNullOrEmpty(searchBar.SearchText) && !type.FullName.ToLower().Contains(searchBar.SearchText.ToLower()))
                                    continue;

                                if(ImGuiAPI.Selectable(type.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                                {
                                    mSelectedType = type;
                                }
                                if(ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                {
                                    CtrlUtility.DrawHelper(type.FullName);
                                }
                            }
                        }

                        EGui.UIProxy.ComboBox.EndCombo();
                    }

                    ImGuiAPI.SetNextItemWidth(-1);
                    var nameChanged =ImGuiAPI.InputText("##in_rname", ref mName);
                    eErrorType = enErrorType.None;
                    if (string.IsNullOrEmpty(mName))
                        eErrorType = enErrorType.EmptyName;
                    else if(nameChanged)
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if (mAsset != null)
                            mAsset.AssetName = rn;
                        if (IO.TtFileManager.FileExists(rn.Address))
                            eErrorType = enErrorType.IsExisting;
                    }

                    if(ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if(IO.TtFileManager.FileExists(rn.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
                        {
                            ((UMacross)mAsset).SelectedType = mSelectedType;
                            if (DoImportAsset())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                    }
                    ImGuiAPI.SameLine(0, 20);
                    if(ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.EndPopup();
                }// */

                return retValue;
            }
        }

        public RName AssetName
        {
            get;
            set;
        }

        public IO.IAssetMeta CreateAMeta()
        {
            var result = new UMacrossAMeta();
            result.Icon = new EGui.TtUVAnim();
            return result;
        }

        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public static void UpdateAMetaReferences(MacrossNode.UMacrossEditor graph, UMacrossAMeta ameta)
        {
            foreach (var i in graph.Methods)
            {
                foreach (var j in i.Nodes)
                {
                    var n = j as Bricks.CodeBuilder.MacrossNode.MethodNode;
                    if (n == null)
                        continue;
                    foreach (var pin in n.Inputs)
                    {
                        if (pin.EditValue == null)
                            continue;
                        var rnm = pin.EditValue.Value as RName;
                        if (rnm != null)
                        {
                            ameta.AddReferenceAsset(rnm);
                        }
                    }
                }
            }
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            var macrossMeta = (ameta as UMacrossAMeta);
            if (macrossMeta != null && SelectedType != null)
                macrossMeta.BaseTypeStr = SelectedType.TypeString;

            var graph = new MacrossNode.UMacrossEditor();
            graph.LoadClassGraph(this.AssetName);
            UpdateAMetaReferences(graph, ameta as UMacrossAMeta);
            //foreach (var i in graph.Methods)
            //{
            //    foreach (var j in i.Nodes)
            //    {
            //        var n = j as Bricks.CodeBuilder.MacrossNode.MethodNode;
            //        if (n == null)
            //            continue;
            //        foreach (var pin in n.Inputs)
            //        {
            //            if (pin.EditValue == null)
            //                continue;
            //            var rnm = pin.EditValue.Value as RName;
            //            if (rnm != null)
            //            {
            //                ameta.AddReferenceAsset(rnm);
            //            }
            //        }
            //    }
            //}
        }

        public UTypeDesc SelectedType = null;
        public MacrossNode.UMacrossEditor MacrossEditor = null;
        public void SaveAssetTo(RName name)
        {
            var ameta = GetAMeta() as UMacrossAMeta;
            if (ameta != null)
            {
                ameta.Description = $"MacrossType:{ameta.BaseTypeStr}\n";
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
            IO.TtFileManager.CreateDirectory(name.Address);

            if (MacrossEditor == null)
                MacrossEditor = new MacrossNode.UMacrossEditor();
            MacrossEditor.AssetName = name;
            MacrossEditor.DefClass.ClassName = name.PureName;
            MacrossEditor.DefClass.Namespace = new UNamespaceDeclaration(IO.TtFileManager.GetParentPathName(name.Name).TrimEnd('/').Replace('/', '.'));
            if (SelectedType != null)
                MacrossEditor.DefClass.SupperClassNames.Add(SelectedType.FullName);
            MacrossEditor.SaveClassGraph(name);
            MacrossEditor.GenerateCode();
            MacrossEditor.CompileCode();
        }
    }
}
