using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class TypeSelector
    {
        public string AssemblyFilter = null;
        public bool ExcludeSealed = false;
        public bool ExcludeValueType = false;
        public bool SearchFromMetas = false;
        Rtti.UTypeDesc mBaseType;
        public TypeSelector()
        {
            mBaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(object).FullName);
        }
        public Rtti.UTypeDesc BaseType
        {
            get => mBaseType;
            set
            {
                if (mBaseType == value)
                    return;
                mBaseType = value;
                mShowTypes.Clear();
                if (mBaseType != null)
                    mShowTypes.Add(mBaseType);
                if (SearchFromMetas)
                {
                    foreach (var i in Rtti.UClassMetaManager.Instance.Metas.Values)
                    {
                        if (i.MetaAttribute == null)
                            continue;
                        if (!string.IsNullOrEmpty(AssemblyFilter))
                        {
                            if (AssemblyFilter.Contains(i.ClassType.Assembly.Name) == false)
                                continue;
                        }
                        if (i.MetaAttribute.IsMacrossDeclareable == false)
                            continue;
                        if (ExcludeValueType && i.ClassType.SystemType.IsValueType)
                            continue;
                        if (ExcludeSealed && i.ClassType.SystemType.IsSealed)
                            continue;

                        if (mBaseType != null)
                        {
                            if (i.ClassType.SystemType.IsSubclassOf(mBaseType.SystemType))
                            {
                                mShowTypes.Add(i.ClassType);
                            }
                        }
                        else
                        {
                            mShowTypes.Add(i.ClassType);
                        }
                    }
                    foreach (var i in Rtti.UClassMetaManager.Instance.TypeMetas.Values)
                    {
                        if (!string.IsNullOrEmpty(AssemblyFilter))
                        {
                            if (AssemblyFilter.Contains(i.ClassType.Assembly.Name) == false)
                                continue;
                        }
                        if (ExcludeValueType && i.ClassType.SystemType.IsValueType)
                            continue;
                        if (ExcludeSealed && i.ClassType.SystemType.IsSealed)
                            continue;

                        if (mBaseType != null)
                        {
                            if (i.ClassType.SystemType.IsSubclassOf(mBaseType.SystemType))
                            {
                                mShowTypes.Add(i.ClassType);
                            }
                        }
                        else
                        {
                            mShowTypes.Add(i.ClassType);
                        }
                    }
                }
                else
                {
                    foreach (var i in Rtti.UTypeDescManager.Instance.Services)
                    {
                        foreach (var j in i.Value.Types)
                        {
                            if (!string.IsNullOrEmpty(AssemblyFilter))
                            {
                                if (AssemblyFilter.Contains(j.Value.Assembly.Name) == false)
                                    continue;
                            }
                            if (ExcludeValueType && j.Value.SystemType.IsValueType)
                                continue;
                            if (ExcludeSealed && j.Value.SystemType.IsSealed)
                                continue;

                            //Rtti.UClassMetaManager.Instance.GetMeta(in hash);

                            if (mBaseType != null)
                            {
                                if (j.Value.SystemType.IsSubclassOf(mBaseType.SystemType))
                                {
                                    mShowTypes.Add(j.Value);
                                }
                            }
                            else
                            {
                                mShowTypes.Add(j.Value);
                            }
                        }
                    }
                }
            }
        }
        Rtti.UTypeDesc mSelectedType;
        public Rtti.UTypeDesc SelectedType
        {
            get => mSelectedType;
            set => mSelectedType = value;
        }
        private List<Rtti.UTypeDesc> mShowTypes = new List<Rtti.UTypeDesc>();
        public List<Rtti.UTypeDesc> ShowTypes
        {
            get
            {
                return mShowTypes;
            }
        }
        string mFilterText = "";
        unsafe int ImGuiInputTextCallback(ImGuiInputTextCallbackData* data)
        {
            //if (CoreSDK.SDK_StrLen(data->Buf) == 0)
            //    return 0;
            if (data->Buf.Length == 0)
                return 0;
            if (ImGuiAPI.IsKeyDown((int)ImGuiKey_.ImGuiKey_Backspace))
            {
                return 0;
            }
            if (ImGuiAPI.IsKeyDown((int)ImGuiKey_.ImGuiKey_Delete))
            {
                return 0;
            }

            return 0;
        }
        public string CtrlId = "##ComboTypeSelector";
        public delegate bool FOnTypeFilter(Rtti.UTypeDesc type);
        public unsafe bool OnDraw(float itemWidth, int showMaxItems, FOnTypeFilter onTypeFilter = null)
        {
            ImGuiAPI.PushID(CtrlId);
            var textSize = ImGuiAPI.CalcTextSize(mSelectedType?.Name, false, -1);
            var stName = mSelectedType?.Name;
            float arrowBtnWidth = 30 + UIProxy.StyleConfig.Instance.ItemSpacing.X;
            ImGuiAPI.SetNextItemWidth(itemWidth - arrowBtnWidth);
            ImGuiAPI.InputText("##in", ref stName);

            ImGuiAPI.OpenPopupOnItemClick("combobox", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            var pos = ImGuiAPI.GetItemRectMin();
            var size = ImGuiAPI.GetItemRectSize();
            ImGuiAPI.SameLine(0, UIProxy.StyleConfig.Instance.ItemSpacing.X);
            if (ImGuiAPI.ArrowButton("##openCombo", ImGuiDir_.ImGuiDir_Down))
            {
                ImGuiAPI.OpenPopup("combobox", ImGuiPopupFlags_.ImGuiPopupFlags_None);
            }
            //ImGuiAPI.OpenPopupOnItemClick("combobox", ImGuiPopupFlags_.ImGuiPopupFlags_None);

            bool bChanged = false;
            pos.Y += size.Y;
            size.X += ImGuiAPI.GetItemRectSize().X;
            size.Y += 5 + (size.Y * showMaxItems);
            var pivot = new Vector2(0, 0);
            ImGuiAPI.SetNextWindowPos(in pos, ImGuiCond_.ImGuiCond_None, in pivot);
            //ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopup("combobox", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
            {
                ImGuiAPI.InputText("##in", ref mFilterText);
             
                ImGuiAPI.Separator();
                var bSelected = true;
                var sz = new Vector2(0, 0);

                if (mFilterText.Length > 0)
                {
                    for (int j = 0; j < mShowTypes.Count; j++)
                    {
                        if (mShowTypes[j].FullName.Contains(mFilterText) == false)
                            continue;

                        if (ImGuiAPI.Selectable(mShowTypes[j].Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
                        {
                            bChanged = (mSelectedType != mShowTypes[j]);
                            mSelectedType = mShowTypes[j];
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            CtrlUtility.DrawHelper(mShowTypes[j].FullName);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < mShowTypes.Count; j++)
                    {
                        if (onTypeFilter != null)
                        {
                            if(onTypeFilter(mShowTypes[j]))
                            {
                                continue;
                            }
                        }

                        //if (AllowVoidType == false && mShowTypes[j].SystemType == typeof(void))
                        //    continue;
                        if (ImGuiAPI.Selectable(mShowTypes[j].Name, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
                        {
                            bChanged = (mSelectedType != mShowTypes[j]);
                            mSelectedType = mShowTypes[j];
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            CtrlUtility.DrawHelper(mShowTypes[j].FullName);
                        }
                    }
                }
                ImGuiAPI.EndPopup();
            }
            ImGuiAPI.PopID();

            return bChanged;
        }
    }
}
