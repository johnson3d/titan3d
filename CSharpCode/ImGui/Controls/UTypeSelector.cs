using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui.Controls
{
    public class UTypeSelector
    {
        [Flags]
        public enum EFilterMode
        {
            IncludeSealed = 1,
            IncludeObjectType = (1 << 1),
            IncludeValueType = (1 << 2),
            ExcludeNoMeta = (1 << 3),
            ExcludeNoMacrossDecl = (1 << 4),
        }
        public UTypeSelector()
        {
            mBaseType = Rtti.UTypeDescGetter<object>.TypeDesc;// Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(typeof(object).FullName);
        }
        public string AssemblyFilter = null;
        public EFilterMode FilterMode = EFilterMode.IncludeObjectType | EFilterMode.IncludeValueType;
        Rtti.UTypeDesc mBaseType;
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
                foreach (var i in Rtti.UTypeDescManager.Instance.Services)
                {
                    foreach (var j in i.Value.Types)
                    {
                        //if ("UImageLoader" == j.Value.SystemType.Name)
                        //{
                        //    int xxx = 0;
                        //}

                        if (!string.IsNullOrEmpty(AssemblyFilter))
                        {
                            if (AssemblyFilter.Contains(j.Value.Assembly.Name) == false)
                                continue;
                        }
                        //if (j.Value.SystemType.IsGenericType)
                        //    continue;

                        if ((j.Value.SystemType.IsValueType &&
                            ((FilterMode & EFilterMode.IncludeValueType) == EFilterMode.IncludeValueType)) ||
                            (!j.Value.SystemType.IsValueType &&
                            ((FilterMode & EFilterMode.IncludeObjectType) == EFilterMode.IncludeObjectType)))
                        {
                            bool incSealed = (FilterMode & EFilterMode.IncludeValueType) == EFilterMode.IncludeValueType;
                            if (incSealed == false && j.Value.SystemType.IsSealed)
                                continue;

                            bool excNoMeta = (FilterMode & EFilterMode.ExcludeNoMeta) == EFilterMode.ExcludeNoMeta;
                            if (excNoMeta)
                            {
                                if (Rtti.UClassMetaManager.Instance.Metas.ContainsKey(j.Key) == false)
                                    continue;
                            }

                            if (mBaseType != null)
                            {
                                if (j.Value.SystemType.IsSubclassOf(mBaseType.SystemType) ||
                                    j.Value.SystemType.GetInterface(mBaseType.SystemType.Name) != null)
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
        Rtti.UTypeDesc[] mTypeList = null;
        public Rtti.UTypeDesc[] TypeList 
        {
            get => mTypeList;
            set
            {
                if (mTypeList == value)
                    return;
                mTypeList = value;
                mShowTypes.Clear();
                foreach(var i in mTypeList)
                {
                    mShowTypes.Add(i);
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

                        if (ImGuiAPI.Selectable(mShowTypes[j].NickName, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
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
                        if (ImGuiAPI.Selectable(mShowTypes[j].NickName, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
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
