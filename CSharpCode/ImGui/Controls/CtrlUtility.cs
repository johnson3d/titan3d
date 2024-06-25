using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class CtrlUtility
    {
        public static void DrawHelper(string desc)
        {
            ImGuiAPI.BeginTooltip();
            ImGuiAPI.PushTextWrapPos(ImGuiAPI.GetFontSize() * 35.0f);
            ImGuiAPI.TextUnformatted(desc);
            ImGuiAPI.PopTextWrapPos();
            ImGuiAPI.EndTooltip();
        }
        //public static void DrawHelper(string desc1 , string desc2)
        //{
        //    ImGuiAPI.BeginTooltip();
        //    ImGuiAPI.PushTextWrapPos(ImGuiAPI.GetFontSize() * 35.0f);
        //    ImGuiAPI.TextUnformatted(desc1);
        //    ImGuiAPI.TextUnformatted(desc2);
        //    ImGuiAPI.PopTextWrapPos();
        //    ImGuiAPI.EndTooltip();
        //}
        public static void DrawHelper(params string[] desc)
        {
            ImGuiAPI.BeginTooltip();
            ImGuiAPI.PushTextWrapPos(ImGuiAPI.GetFontSize() * 35.0f);
            foreach (var i in desc)
            {
                ImGuiAPI.TextUnformatted(i);
            }
            ImGuiAPI.PopTextWrapPos();
            ImGuiAPI.EndTooltip();
        }
        public static void DrawHelper(List<string> desc)
        {
            ImGuiAPI.BeginTooltip();
            ImGuiAPI.PushTextWrapPos(ImGuiAPI.GetFontSize() * 35.0f);
            foreach (var i in desc)
            {
                ImGuiAPI.TextUnformatted(i);
            }
            ImGuiAPI.PopTextWrapPos();
            ImGuiAPI.EndTooltip();
        }
        //public static RName DrawRName(RName name, string ctrlId, string ext, bool ReadOnly, in EGui.UIProxy.ImageProxy snap)
        //{
        //    var drawList = ImGuiAPI.GetWindowDrawList();
        //    var cursorPos = ImGuiAPI.GetCursorScreenPos();
        //    ImGuiAPI.BeginGroup();
        //    snap?.OnDraw(ref drawList, ref Support.UAnyPointer.Default);

        //    ImGuiAPI.EndGroup();

        //    int slt = 0;
        //    //ImGuiAPI.PushID(ctrlId);
        //    var sz = new Vector2(0, 0);
        //    ImGuiAPI.SetNextItemWidth(-1);
        //    if (name == null)
        //        ImGuiAPI.Text("null");
        //    else
        //        ImGuiAPI.Text(name.ToString());
        //    ImGuiAPI.SameLine(0, -1);
        //    Vector2 SelectPos;
        //    if (ReadOnly==false)
        //    {
        //        if (ImGuiAPI.Button("+", ref sz))
        //        {
        //            slt = 1;
        //            SelectPos = ImGuiAPI.GetItemRectMin();
        //        }
        //        ImGuiAPI.SameLine(0, -1);
        //        if (ImGuiAPI.Button("-", ref sz))
        //        {
        //            slt = 2;
        //        }
        //        ImGuiAPI.SameLine(0, -1);
        //        if (ImGuiAPI.Button("*", ref sz))
        //        {
        //            slt = 3;
        //        }
        //    }


        //    UAssetSelector.Instance.PopName = $"AssetSelector_{name?.ToString()}";
        //    switch (slt)
        //    {
        //        case 0:
        //            break;
        //        case 1://+
        //            {
        //                UAssetSelector.Instance.ExtName = ext;
        //                UAssetSelector.Instance.SelectedAsset = null;
        //                UAssetSelector.Instance.OpenPopup();
        //            }
        //            break;
        //        case 2://-
        //            break;
        //        case 3://*
        //            break;
        //    }

        //    bool isPopup = UAssetSelector.Instance.IsOpenPopup();
        //    var pos = ImGuiAPI.GetCursorScreenPos();
        //    UAssetSelector.Instance.OnDraw(ref pos);
        //    //ImGuiAPI.PopID();
        //    if (isPopup)
        //    {
        //        if (UAssetSelector.Instance.SelectedAsset != null && UAssetSelector.Instance.SelectedAsset.GetAssetName() != name)
        //        {
        //            return UAssetSelector.Instance.SelectedAsset.GetAssetName();
        //        }
        //    }

        //    switch (slt)
        //    {
        //        case 2://-
        //            return null;
        //        case 3://*
        //            break;
        //    }
        //    return name;
        //}
    }
}
