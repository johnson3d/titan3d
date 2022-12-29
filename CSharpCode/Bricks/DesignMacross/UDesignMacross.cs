using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.EGui.Controls;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross
{
    public class DesignableAttribute : Attribute
    {

    }
    public class DesignableGraphAttribute : Attribute
    {
        public Type Type;
    }
    public class DesignableGraphNodeAttribute : Attribute
    {
        public Type Type;
    }

    public class DesignMacrossAttribute : Attribute
    {
    }
    [DesignMacross]
    public class UDesignMacrossBase
    {
        public void Initialize() { }
    }
    public partial class UDesignMacrossAMeta : IO.IAssetMeta
    {
        [Rtti.Meta]
        public string BaseTypeStr { get; set; }
        public override string GetAssetExtType()
        {
            return UDesignMacross.AssetExt;
        }
        public override string GetAssetTypeName()
        {
            return "DesignMacross";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
        public override void DeleteAsset(string name, RName.ERNameType type)
        {
            var address = RName.GetAddress(type, name);
            IO.FileManager.DeleteDirectory(address);
            IO.FileManager.DeleteFile(address + IAssetMeta.MetaExt);

            if (UMacrossEditor.RemoveAssemblyDescCreateInstanceCode(name, type))
            {
                EngineNS.UEngine.Instance.MacrossManager.GenerateProjects();
                var assemblyFile = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.EngineSource) + UEngine.Instance.EditorInstance.Config.GameAssembly;
                if (UEngine.Instance.MacrossModule.CompileCode(assemblyFile))
                {
                    UEngine.Instance.MacrossModule.ReloadAssembly(assemblyFile);
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
        //    UEngine.Instance.EditorInstance.MacrossIcon?.OnDraw(cmdlist, in start, in end, 0);
        //    cmdlist.AddText(in start, 0xFFFFFFFF, "Macross", null);
        //}
    }
    [Rtti.Meta]
    [DesignMacrossCreate]
    [IO.AssetCreateMenu(MenuName = "DesignMacross")]
    [Editor.UAssetEditor(EditorType = typeof(UDesignMacrossEditor))]
    public partial class UDesignMacross : IO.IAsset
    {
        public const string AssetExt = ".designmacross";
        public const string MacrossEditorKeyword = "DesignMacross";
        public class DesignMacrossCreateAttribute : IO.CommonCreateAttribute
        {
            UTypeDesc mSelectedType = null;
            public override void DoCreate(RName dir, UTypeDesc type, string ext)
            {
                base.DoCreate(dir, type, ext);
                mSelectedType = null;
            }

            public override unsafe bool OnDraw(UContentBrowser contentBrowser)
            {
                //base.OnDraw(contentBrowser);

                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"New DesignMacross", ImGuiPopupFlags_.ImGuiPopupFlags_None);

                var visible = true;
                var retValue = false;
                if (ImGuiAPI.BeginPopupModal($"New DesignMacross", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    var drawList = ImGuiAPI.GetWindowDrawList();
                    switch (eErrorType)
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
                    if (EGui.UIProxy.ComboBox.BeginCombo("##TypeSel", (mSelectedType == null) ? "None" : mSelectedType.Name))
                    {
                        var comboDrawList = ImGuiAPI.GetWindowDrawList();
                        var searchBar = UEngine.Instance.UIManager["MacrossTypeSearchBar"] as EGui.UIProxy.SearchBarProxy;
                        if (searchBar == null)
                        {
                            searchBar = new EGui.UIProxy.SearchBarProxy()
                            {
                                InfoText = "Search macross base type",
                                Width = -1,
                            };
                            UEngine.Instance.UIManager["MacrossTypeSearchBar"] = searchBar;
                        }
                        if (!ImGuiAPI.IsAnyItemActive() && !ImGuiAPI.IsMouseClicked(0, false))
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

                                var atts = type.GetCustomAttributes(typeof(DesignMacrossAttribute), false);
                                if (atts == null || atts.Length == 0)
                                    continue;

                                if (!string.IsNullOrEmpty(searchBar.SearchText) && !type.FullName.ToLower().Contains(searchBar.SearchText.ToLower()))
                                    continue;

                                if (ImGuiAPI.Selectable(type.Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in Vector2.Zero))
                                {
                                    mSelectedType = type;
                                }
                                if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                                {
                                    CtrlUtility.DrawHelper(type.FullName);
                                }
                            }
                        }

                        EGui.UIProxy.ComboBox.EndCombo();
                    }

                    ImGuiAPI.SetNextItemWidth(-1);
                    var nameChanged = ImGuiAPI.InputText("##in_rname", ref mName);
                    eErrorType = enErrorType.None;
                    if (string.IsNullOrEmpty(mName))
                        eErrorType = enErrorType.EmptyName;
                    else if (nameChanged)
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if (mAsset != null)
                            mAsset.AssetName = rn;
                        if (IO.FileManager.FileExists(rn.Address))
                            eErrorType = enErrorType.IsExisting;
                    }

                    if (ImGuiAPI.Button("Create Asset", in Vector2.Zero))
                    {
                        var rn = RName.GetRName(mDir.Name + mName + ExtName, mDir.RNameType);
                        if (IO.FileManager.FileExists(rn.Address) == false && string.IsNullOrWhiteSpace(mName) == false)
                        {
                            ((UDesignMacross)mAsset).mSelectedType = mSelectedType;
                            if (DoImportAsset())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                    }
                    ImGuiAPI.SameLine(0, 20);
                    if (ImGuiAPI.Button("Cancel", in Vector2.Zero))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.EndPopup();
                }// */

                return retValue;
            }
        }
        public RName AssetName { get; set; }

        public IAssetMeta CreateAMeta()
        {
            var result = new UDesignMacrossAMeta();
            result.Icon = new EGui.UUvAnim();
            return result;
        }

        public IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        UTypeDesc mSelectedType = null;
        public UDesignMacrossEditor DesignMacrossEditor = null;
        public void SaveAssetTo(RName name)
        {
            //var ameta = GetAMeta();
            //if (ameta != null)
            //{
            //    UpdateAMetaReferences(ameta);
            //    ameta.SaveAMeta();
            //}
            IO.FileManager.CreateDirectory(name.Address);

            if (DesignMacrossEditor == null)
                DesignMacrossEditor = new UDesignMacrossEditor();
            DesignMacrossEditor.AssetName = name;
            DesignMacrossEditor.DefClass.ClassName = name.PureName;
            DesignMacrossEditor.DefClass.Namespace = new UNamespaceDeclaration(IO.FileManager.GetParentPathName(name.Name).TrimEnd('/').Replace('/', '.'));
            if (mSelectedType != null)
                DesignMacrossEditor.DefClass.SupperClassNames.Add(mSelectedType.FullName);
            DesignMacrossEditor.SaveClassGraph(name);
        }

        public void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
            var macrossMeta = (ameta as UDesignMacrossAMeta);
            if (macrossMeta != null && mSelectedType != null)
                macrossMeta.BaseTypeStr = mSelectedType.TypeString;

            var graph = new UDesignMacrossEditor();
            graph.LoadClassGraph(this.AssetName);
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
    }
}
