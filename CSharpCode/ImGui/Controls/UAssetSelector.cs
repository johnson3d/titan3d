using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    //[Obsolete]
    //public class UAssetSelector
    //{
    //    public static UAssetSelector Instance = new UAssetSelector();
    //    public IO.IAssetMeta SelectedAsset
    //    {
    //        get => mContentBrowser.SelectedAsset;
    //        set => mContentBrowser.SelectedAsset = value;
    //    }

    //    string mPopName = "AssetSelector";
    //    public string PopName
    //    {
    //        get => mPopName;
    //        set
    //        {
    //            mPopName = value;
    //            mContentBrowser.Name = mPopName + "_CB";
    //        }
    //    }
    //    public string ExtName
    //    {
    //        get => mContentBrowser.ExtNames;
    //        set => mContentBrowser.ExtNames = value;
    //    }
    //    UContentBrowser mContentBrowser;
    //    public UAssetSelector()
    //    {
    //        mContentBrowser = new UContentBrowser()
    //        {
    //            CreateNewAssets = false,
    //            ItemSelectedAction = (asset) =>
    //            {
    //                ImGuiAPI.CloseCurrentPopup();
    //            },
    //        };
    //        _ = mContentBrowser.Initialize();
    //        mContentBrowser.DrawInWindow = false;
    //    }

    //    //string mFilterText = "";
    //    //Vector2 LeftSize;
    //    //Vector2 RightSize;
    //    public void OpenPopup()
    //    {
    //        ImGuiAPI.OpenPopup(PopName, ImGuiPopupFlags_.ImGuiPopupFlags_None);
    //    }
    //    public bool IsOpenPopup()
    //    {
    //        return ImGuiAPI.IsPopupOpen(PopName, ImGuiPopupFlags_.ImGuiPopupFlags_None);
    //    }
    //    public unsafe void OnDraw(ref Vector2 pos)
    //    {
    //        var pivot = Vector2.Zero;
    //        var size = new Vector2(500, 300);
    //        ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_Always, in pivot);
    //        ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_Always);
    //        if (ImGuiAPI.BeginPopup(PopName, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
    //        {
    //            mContentBrowser.OnDraw();
    //    //        var cltMin = ImGuiAPI.GetWindowContentRegionMin();
    //    //        var cltMax = ImGuiAPI.GetWindowContentRegionMax();

    //    //        var buffer = BigStackBuffer.CreateInstance(256);
    //    //        buffer.SetText(mFilterText);
    //    //        ImGuiAPI.InputText("##in", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
    //    //        mFilterText = buffer.AsText();
    //    //        buffer.DestroyMe();

    //    //        var inputSize = ImGuiAPI.GetItemRectSize();

    //    //        ImGuiAPI.Separator();

    //    //        ImGuiAPI.Columns(2, null, true);

    //    //        LeftSize.X = ImGuiAPI.GetColumnWidth(0);
    //    //        LeftSize.Y = cltMax.Y - cltMin.Y - inputSize.Y;

    //    //        DrawDirectories(RName.GetRName("", RName.ERNameType.Game), LeftSize);
    //    //        ImGuiAPI.NextColumn();

    //    //        RightSize.X = ImGuiAPI.GetColumnWidth(1);
    //    //        RightSize.Y = cltMax.Y - cltMin.Y - inputSize.Y;

    //    //        if (CurrentDir != null)
    //    //            DrawFiles(CurrentDir, RightSize);
    //    //        ImGuiAPI.NextColumn();

    //    //        ImGuiAPI.Columns(1, null, false);

    //            ImGuiAPI.EndPopup();
    //        }
    //    }
    //    //public RName CurrentDir;
    //    //public void DrawDirectories(RName root, Vector2 size)
    //    //{
    //    //    //if (ImGuiAPI.BeginChild("LeftWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
    //    //    {
    //    //        ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
    //    //        if (root == CurrentDir)
    //    //            flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;
    //    //        if (ImGuiAPI.TreeNodeEx(root.RNameType.ToString(), flags))
    //    //        {
    //    //            if (ImGuiAPI.IsItemActivated())
    //    //            {
    //    //                CurrentDir = root;
    //    //            }
    //    //            var dirs = IO.FileManager.GetDirectories(root.Address, "*.*", false);
    //    //            foreach (var i in dirs)
    //    //            {
    //    //                var nextDirName = IO.FileManager.GetRelativePath(root.Address, i);
    //    //                DrawTree(root.RNameType, root.Name, nextDirName);
    //    //            }
    //    //            ImGuiAPI.TreePop();
    //    //        }
    //    //        else
    //    //        {
    //    //            if (ImGuiAPI.IsItemActivated())
    //    //            {
    //    //                CurrentDir = root;
    //    //            }
    //    //        }
    //    //    }
    //    //    //ImGuiAPI.EndChild();
    //    //}
    //    //private void DrawTree(RName.ERNameType type, string parentDir, string dirName)
    //    //{
    //    //    ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow;
    //    //    var nextParent = parentDir + dirName + "/";
    //    //    var rn = RName.GetRName(nextParent, type);
    //    //    if (rn == CurrentDir)
    //    //        flags |= ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed;

    //    //    if (ImGuiAPI.TreeNodeEx(dirName, flags))
    //    //    {
    //    //        if (ImGuiAPI.IsItemActivated())
    //    //        {
    //    //            CurrentDir = rn;
    //    //        }
    //    //        var path = RName.GetRName(nextParent, type).Address;
    //    //        var dirs = IO.FileManager.GetDirectories(path, "*.*", false);
    //    //        foreach (var i in dirs)
    //    //        {
    //    //            var nextDirName = IO.FileManager.GetRelativePath(path, i);
    //    //            DrawTree(type, nextParent, nextDirName);
    //    //        }
    //    //        ImGuiAPI.TreePop();
    //    //    }
    //    //}
    //    //public unsafe void DrawFiles(RName dir, Vector2 size)
    //    //{
    //    //    var itemSize = new Vector2(80, 100);
    //    //    //if (ImGuiAPI.BeginChild("RightWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
    //    //    {
    //    //        var cmdlist = new ImDrawList(ImGuiAPI.GetWindowDrawList());
    //    //        var width = ImGuiAPI.GetColumnWidth(1);
    //    //        var files = IO.FileManager.GetFiles(dir.Address, "*.ameta", true);
    //    //        float curPos = 0;
    //    //        foreach (var i in files)
    //    //        {
    //    //            var name = IO.FileManager.GetLastestPathName(i.Substring(0, i.Length - 6));
    //    //            if (!string.IsNullOrEmpty(ExtName))
    //    //            {
    //    //                var ext = IO.FileManager.GetExtName(name);
    //    //                if (ext != ExtName)
    //    //                    continue;
    //    //            }

    //    //            if (!string.IsNullOrEmpty(mFilterText))
    //    //            {
    //    //                if (name.Contains(mFilterText) == false)
    //    //                    continue;
    //    //            }

    //    //            var ameta = UEngine.Instance.AssetMetaManager.GetAssetMeta(RName.GetRName(dir.Name + name, dir.RNameType));
    //    //            if (ameta == null)
    //    //                continue;

    //    //            DrawItem(ref cmdlist, ameta.Icon, ameta, ref itemSize);
    //    //            curPos += itemSize.X + 2;
    //    //            if (curPos + itemSize.X < width)
    //    //            {
    //    //                ImGuiAPI.SameLine(0, 2);
    //    //            }
    //    //            else
    //    //            {
    //    //                curPos = 0;
    //    //            }
    //    //        }
    //    //    }
    //    //    //ImGuiAPI.EndChild();
    //    //}
    //    //private void DrawItem(ref ImDrawList cmdlist, UVAnim icon, IO.IAssetMeta ameta, ref Vector2 sz)
    //    //{
    //    //    ImGuiAPI.Selectable($"##{ameta.GetAssetName().Name}", false, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz);
    //    //    if (ImGuiAPI.IsItemVisible())
    //    //    {
    //    //        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
    //    //        {
    //    //            CtrlUtility.DrawHelper(ameta.GetAssetName().Name, ameta.Description);
    //    //        }
    //    //        ameta.ShowIconTime = UEngine.Instance.CurrentTickCount;
    //    //        ameta.OnDraw(ref cmdlist, ref sz, null);
    //    //        if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
    //    //        {
    //    //            SelectedAsset = ameta;
    //    //            ImGuiAPI.CloseCurrentPopup();
    //    //        }
    //    //    }
    //    //}
    //}
}
