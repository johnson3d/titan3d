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
                                if (Rtti.TtClassMetaManager.Instance.Metas.ContainsKey(j.Key) == false)
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
        public void AddShowType(Rtti.UTypeDesc type)
        {
            mShowTypes.Add(type);
        }
        string mFilterText = "";
        unsafe int ImGuiInputTextCallback(ImGuiInputTextCallbackData* data)
        {
            //if (CoreSDK.SDK_StrLen(data->Buf) == 0)
            //    return 0;
            if (data->Buf.Length == 0)
                return 0;
            if (ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_Backspace))
            {
                return 0;
            }
            if (ImGuiAPI.IsKeyDown(ImGuiKey.ImGuiKey_Delete))
            {
                return 0;
            }

            return 0;
        }
        bool mSearchBarFocused = false;
        public string CtrlId = "##ComboTypeSelector";
        public delegate bool FOnTypeFilter(Rtti.UTypeDesc type);
        public bool PopupVisible = false;
        public unsafe bool OnDraw(float itemWidth, int showMaxItems, FOnTypeFilter onTypeFilter = null)
        {
            ImGuiAPI.PushID(CtrlId);
            var pos = ImGuiAPI.GetCursorScreenPos();
            var drawList = ImGuiAPI.GetWindowDrawList();
            var text = mSelectedType?.Name;
            if (text == null)
            {
                text = "";
            }
            var frameHeight = ImGuiAPI.GetFrameHeight();
            var textSize = ImGuiAPI.CalcTextSize(text, false, -1);
            drawList.AddText(pos + new Vector2(0, (frameHeight - textSize.Y) * 0.5f), UIProxy.StyleConfig.Instance.TextColor, text, null);
            var sizeDelta = ImGuiAPI.GetFontSize() * 0.4f;
            ImGuiAPI.Arrow(drawList, pos + new Vector2(UIProxy.StyleConfig.Instance.ItemSpacing.X + textSize.X, (frameHeight - sizeDelta) * 0.5f - 2.0f), UIProxy.StyleConfig.Instance.TextColor, ImGuiDir.ImGuiDir_Down, 1.0f);
            if(ImGuiAPI.InvisibleButton(CtrlId + "InvBtn",
                new Vector2(Math.Max(itemWidth, UIProxy.StyleConfig.Instance.ItemSpacing.X + textSize.X + sizeDelta), frameHeight),
                ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft))
            {
                ImGuiAPI.OpenPopup("combobox", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                PopupVisible = true;
            }

            bool bChanged = false;
            var pivot = new Vector2(0, 0);
            var popWinPos = pos + new Vector2(0, frameHeight);
            var size = new Vector2(300, 500);
            ImGuiAPI.SetNextWindowPos(popWinPos, ImGuiCond_.ImGuiCond_None, in pivot);
            ImGuiAPI.SetNextWindowSize(in size, ImGuiCond_.ImGuiCond_None);
            if (ImGuiAPI.BeginPopupModal("combobox", ref PopupVisible, ImGuiWindowFlags_.ImGuiWindowFlags_NoMove | ImGuiWindowFlags_.ImGuiWindowFlags_Popup))
            {
                var sz = new Vector2(0, 0);
                var popDrawList = ImGuiAPI.GetWindowDrawList();
                UIProxy.SearchBarProxy.OnDraw(ref mSearchBarFocused, popDrawList, "search types", ref mFilterText, ImGuiAPI.GetWindowContentRegionWidth());
                ImGuiAPI.BeginChild("typesWin", in sz, ImGuiChildFlags_.ImGuiChildFlags_None, ImGuiWindowFlags_.ImGuiWindowFlags_None);
                {
                    mFilterText = mFilterText.ToLower();
                    ImGuiAPI.Separator();
                    var bSelected = true;
                    for (int i = 0; i < mShowTypes.Count; i++)
                    {
                        var typeName = mShowTypes[i].CSharpTypeName;
                        if (mFilterText.Length > 0)
                        {
                            if(typeName.ToLower().Contains(mFilterText) == false)
                                continue;
                        }

                        if (onTypeFilter != null)
                        {
                            if (onTypeFilter(mShowTypes[i]))
                                continue;
                        }

                        //if (AllowVoidType == false && mShowTypes[j].SystemType == typeof(void))
                        //    continue;
                        if (ImGuiAPI.Selectable(typeName, ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, in sz))
                        {
                            bChanged = (mSelectedType != mShowTypes[i]);
                            mSelectedType = mShowTypes[i];
                            ImGuiAPI.CloseCurrentPopup();
                        }
                        if (ImGuiAPI.IsItemHovered(ImGuiHoveredFlags_.ImGuiHoveredFlags_None))
                        {
                            CtrlUtility.DrawHelper(typeName);
                        }
                    }
                }
                ImGuiAPI.EndChild();

                ImGuiAPI.EndPopup();
            }
            ImGuiAPI.PopID();
            return bChanged;
        }
    }
}
