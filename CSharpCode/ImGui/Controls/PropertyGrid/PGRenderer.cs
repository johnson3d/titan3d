using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS.EGui.Controls.PropertyGrid
{
    partial class PropertyGrid : IPanel
    {
        public bool IsReadOnly = false;
        private bool mVisible = true;
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = value;
            }
        }
        public uint DockId { get; set; } = uint.MaxValue;
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public string PGName
        {
            get;
            set;
        } = "Property Grid";
        byte[] TextBuffer = new byte[1024 * 4];
        List<KeyValuePair<object, System.Reflection.PropertyInfo>> mCallstack = new List<KeyValuePair<object, System.Reflection.PropertyInfo>>();

        public string SearchInfo
        {
            get
            {
                if (mSearchBar != null)
                    return mSearchBar.InfoText;
                return "";
            }
            set
            {
                if (mSearchBar != null)
                    mSearchBar.InfoText = value;
            }
        }

        EGui.UIProxy.SearchBarProxy mSearchBar;
        EGui.UIProxy.ImageButtonProxy mOpenInPropertyMatrix;
        EGui.UIProxy.ImageButtonProxy mConfig;
        public void Initialize()
        {
            mSearchBar = new UIProxy.SearchBarProxy();
            mSearchBar.Initialize();
            mSearchBar.InfoText = "Search Details";
            mOpenInPropertyMatrix = new UIProxy.ImageButtonProxy()
            {
                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                Size = new Vector2(20, 20),
                UVMin = new Vector2(521.0f/1024, 4.0f/1024),
                UVMax = new Vector2(537.0f/1024, 20.0f/1024),
            };
            mConfig = new UIProxy.ImageButtonProxy()
            {
                ImageFile = RName.GetRName("icons/icons.srv", RName.ERNameType.Engine),
                Size = new Vector2(20, 20),
                UVMin = new Vector2(3.0f / 1024, 691.0f / 1024),
                UVMax = new Vector2(19.0f / 1024, 707.0f / 1024),
            };
        }
        
        public void OnDraw()
        {
            OnDraw(false, true, false);
        }
        public void OnDraw(bool bShowReadOnly, bool bNewForm/*=true*/, bool bKeepColums/*=false*/)
        {
            if (Visible == false)
                return;
            mCallstack.Clear();
            mCallstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(null, null));

            if (bNewForm)
            {
                if (ImGuiAPI.Begin($"{PGName}", ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    OnDrawContent(bShowReadOnly, bKeepColums);
                }
                ImGuiAPI.End();
            }
            else
            {
                OnDrawContent(bShowReadOnly, bKeepColums);
            }
        }

        string mFilterText = "";
        private unsafe void OnDrawHeadBar(ref ImDrawList drawList)
        {
            var winWidth = ImGuiAPI.GetWindowWidth();
            ImGuiAPI.BeginGroup();
            if (mSearchBar != null)
            {
                var indentValue = 8.0f;
                mSearchBar.Width = winWidth - indentValue * 2 - mOpenInPropertyMatrix.Size.X - mOpenInPropertyMatrix.Size.Y - 8 * 2;
                ImGuiAPI.Indent(indentValue);
                mSearchBar.OnDraw(ref drawList);
                ImGuiAPI.Unindent(indentValue);
            }
            if (mOpenInPropertyMatrix != null)
            {
                ImGuiAPI.SameLine(0, -1);
                var size = ImGuiAPI.GetItemRectSize();
                var curPos = ImGuiAPI.GetCursorScreenPos();
                Vector2 offset = Vector2.Zero;
                curPos += new Vector2(0, (size.Y - mOpenInPropertyMatrix.Size.Y) * 0.5f);
                ImGuiAPI.SetCursorScreenPos(ref curPos);
                mOpenInPropertyMatrix.OnDraw(ref drawList);
            }
            if (mConfig != null)
            {
                ImGuiAPI.SameLine(0, -1);
                var size = ImGuiAPI.GetItemRectSize();
                var curPos = ImGuiAPI.GetCursorScreenPos();
                Vector2 offset = Vector2.Zero;
                curPos += new Vector2(0, (size.Y - mOpenInPropertyMatrix.Size.Y) * 0.5f);
                ImGuiAPI.SetCursorScreenPos(ref curPos);
                mConfig.OnDraw(ref drawList);
            }
            ImGuiAPI.EndGroup();
        }
        private void OnDrawContent(bool bShowReadOnly, bool bKeepColums = false)
        {
            var drawList = ImGuiAPI.GetWindowDrawList();
            var posMin = ImGuiAPI.GetWindowPos();
            var posMax = posMin + ImGuiAPI.GetWindowSize();
            drawList.AddRectFilled(ref posMin, ref posMax, EGui.UIProxy.StyleConfig.PanelBackground, 0.0f, ImDrawFlags_.ImDrawFlags_None);

            if (ImGuiAPI.IsWindowDocked())
            {
                DockId = ImGuiAPI.GetWindowDockID();
            }
            if (bShowReadOnly)
            {
                //var base_flags = (int)(ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnArrow | 
                //    ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_OpenOnDoubleClick | 
                //    ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_SpanAvailWidth);
                //ImGuiAPI.CheckboxFlags("IsReadOnly", ref base_flags, (int)ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Framed);
                //ImGuiAPI.CheckboxFlags("IsReadOnly1", ref base_flags, (int)ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_FramePadding);
                ImGuiAPI.Checkbox("IsReadOnly", ref IsReadOnly);
            }

            if (bKeepColums == false)
            {
                ImGuiAPI.Text($"{SingleTarget?.ToString()}");
                OnDrawHeadBar(ref drawList);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGNormalFramePadding);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameBorderSize, 1.0f);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FrameRounding, 3.0f);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_ItemSpacing, ref EGui.UIProxy.StyleConfig.PGNormalItemSpacing);
                ImGuiAPI.PushStyleColor(ImGuiCol_.ImGuiCol_HeaderHovered, ref EGui.UIProxy.StyleConfig.PGItemHoveredColor);
                ImGuiAPI.Separator();

                Vector2 size = Vector2.Zero;
                if(ImGuiAPI.BeginChild($"{PGName}_Properties", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    ImGuiAPI.Columns(2, null, true);
                    OnDraw(this.SingleTarget, mCallstack);
                    ImGuiAPI.Columns(1, null, true);
                }
                ImGuiAPI.EndChild();

                //ImGuiAPI.Separator();
                ImGuiAPI.PopStyleVar(4);
                ImGuiAPI.PopStyleColor(1);
            }
            else
            {
                Vector2 size = Vector2.Zero;
                if(ImGuiAPI.BeginChild($"{PGName}_Properties", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    OnDraw(this.SingleTarget, mCallstack);
                }
                ImGuiAPI.EndChild();
            }
        }
        public Rtti.UTypeDesc HideInheritDeclareType = null;
        private unsafe void OnDraw(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            if (target == null)
                return;

            if (this.GetType() == typeof(System.Type))
            {
                return;
            }

            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen;
            var props = target.GetType().GetProperties();
            foreach (var i in props)
            {
                if (HideInheritDeclareType != null)
                {
                    if (i.DeclaringType.IsSubclassOf(HideInheritDeclareType.SystemType) == false)
                        continue;
                }
                PGCustomValueEditorAttribute editorOnDraw = null;
                var attrs = i.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                    if (editorOnDraw.HideInPG)
                        continue;
                }
                bool isReadOnly = (editorOnDraw != null && editorOnDraw.ReadOnly);

                object obj;
                try
                {
                    if (i.PropertyType == typeof(System.Type))
                    {
                        continue;
                    }
                    if (i.DeclaringType.FullName == "System.RuntimeType")
                    {
                        continue;
                    }
                    if (i.Name == "Item")
                    {
                        continue;
                    }
                    obj = i.GetValue(target);
                }
                catch
                {
                    continue;
                }

                System.Type showType = i.PropertyType;
                if (obj == null && showType != typeof(string) && showType != typeof(RName))
                {   
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var sz = new Vector2(0, 0);
                    if (!showType.IsSubclassOf(typeof(System.Array)) && isReadOnly == false)
                    {
                        if (ImGuiAPI.Button("New", ref sz))
                        {
                            var v = System.Activator.CreateInstance(showType);
                            foreach (var j in this.TargetObjects)
                            {
                                SetValue(this, j, callstack, i, target, v);
                            }
                        }
                    }
                    ImGuiAPI.NextColumn();
                    continue;
                }
                else
                {
                    if (obj != null)
                        showType = obj.GetType();
                }

                if (editorOnDraw != null && editorOnDraw.UserDraw)
                {
                    if (editorOnDraw.IsFullRedraw)
                    {
                        try
                        {
                            editorOnDraw.OnDraw(i, target, obj, this, callstack);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                    else
                    {
                        ImGuiAPI.AlignTextToFramePadding();
                        ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                        ImGuiAPI.NextColumn();
                        try
                        {
                            editorOnDraw.OnDraw(i, target, obj, this, callstack);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        ImGuiAPI.NextColumn();
                    }
                }
                else if (showType == typeof(bool))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = (System.Convert.ToBoolean(obj));
                    var saved = v;
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGCheckboxFramePadding);
                    ImGuiAPI.Checkbox(TName.FromString2("##", i.Name).ToString(), ref v);
                    ImGuiAPI.PopStyleVar(1);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(this, j, callstack, i, target, (bool)v);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(int) || 
                        showType == typeof(sbyte) || 
                        showType == typeof(short) || 
                        showType == typeof(long))
                {
                    ImGuiAPI.AlignTextToFramePadding();                    
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = System.Convert.ToInt32(obj);
                    var saved = v;
                    ImGuiAPI.InputInt(TName.FromString2("##", i.Name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsDecimal);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(this, j, callstack, i, target, EngineNS.Support.TConvert.ToObject(showType, v.ToString()));
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(uint) || 
                    showType == typeof(byte) ||
                    showType == typeof(ushort) ||
                    showType == typeof(ulong))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                    var buffer = BigStackBuffer.CreateInstance(256);
                    var oldValue = obj.ToString();
                    buffer.SetText(oldValue);
                    ImGuiAPI.InputText(TName.FromString2("##", i.Name).ToString(), buffer.GetBuffer(), (uint)buffer.GetSize(),
                        ImGuiInputTextFlags_.ImGuiInputTextFlags_CharsHexadecimal, null, (void*)0);
                    var newValue = buffer.AsText();
                    if (newValue != oldValue)
                    {
                        var v = Support.TConvert.ToObject(showType, newValue);
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(this, j, callstack, i, target, v);
                        }
                    }
                    buffer.DestroyMe();
                    ImGuiAPI.PopStyleVar(1);
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(float))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = System.Convert.ToSingle(obj);
                    var saved = v;
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                    ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.PopStyleVar(1);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(this, j, callstack, i, target, v);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(double))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = System.Convert.ToDouble(obj);
                    var saved = v;
                    ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                    ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref (*(float*)&v), 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    ImGuiAPI.PopStyleVar(1);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(this, j, callstack, i, target, v);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(string))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = obj as string;
                    var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
                    fixed (byte* pBuffer = &TextBuffer[0])
                    {
                        CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
                        ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                        ImGuiAPI.InputText(TName.FromString2("##", i.Name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        ImGuiAPI.PopStyleVar(1);
                        if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                        {
                            v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                            foreach (var j in this.TargetObjects)
                            {
                                SetValue(this, j, callstack, i, target, v);
                            }
                        }
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType.IsEnum)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var attrs1 = i.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
                    if (attrs1 != null && attrs1.Length > 0)
                    {
                        if (ImGuiAPI.TreeNode("Flags"))
                        {
                            var members = showType.GetEnumNames();
                            var values = showType.GetEnumValues();
                            var sz = new Vector2(0, 0);
                            uint newFlags = 0;
                            var EnumFlags = System.Convert.ToUInt32(obj);
                            for (int j = 0; j < members.Length; j++)
                            {
                                var m = values.GetValue(j).ToString();
                                var e_v = System.Enum.Parse(i.PropertyType, m);
                                var v = System.Convert.ToUInt32(e_v);
                                var bSelected = (((EnumFlags & v) == 0) ? false : true);
                                ImGuiAPI.Checkbox(members[j], ref bSelected);
                                if (bSelected)
                                {
                                    newFlags |= v;
                                }
                                else
                                {
                                    newFlags &= ~v;
                                }
                            }
                            if (newFlags != EnumFlags)
                            {
                                foreach (var j in this.TargetObjects)
                                {
                                    SetValue(this, j, callstack, i, target, System.Enum.ToObject(i.PropertyType, newFlags));
                                }
                            }
                            ImGuiAPI.TreePop();
                        }
                    }
                    else
                    {
                        if (ImGuiAPI.BeginCombo(TName.FromString2("##", i.Name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
                        {
                            int item_current_idx = -1;
                            var members = showType.GetEnumNames();
                            var values = showType.GetEnumValues();
                            var sz = new Vector2(0, 0);
                            for (int j = 0; j < members.Length; j++)
                            {
                                var bSelected = true;
                                if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
                                {
                                    item_current_idx = j;
                                }
                            }
                            if (item_current_idx >= 0)
                            {
                                var v = (int)values.GetValue(item_current_idx);
                                foreach (var j in this.TargetObjects)
                                {
                                    SetValue(this, j, callstack, i, target, v);
                                }
                            }

                            ImGuiAPI.EndCombo();
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (obj is Array)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.SameLine(0, -1);
                    var showChild = ImGuiAPI.TreeNode(i.Name, "");
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.Text(i.ToString());
                    ImGuiAPI.NextColumn();
                    if (showChild)
                    {
                        var lst = obj as System.Array;
                        OnArray(target, callstack, i, lst);
                        ImGuiAPI.TreePop();
                    }
                }
                else if (obj is System.Collections.IList)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    if (this.IsReadOnly == false)
                    {
                        ImGuiAPI.SameLine(0, -1);
                        var sz = new Vector2(0, 0);
                        ImGuiAPI.PushID(i.Name);
                        ImGuiAPI.SameLine(0, -1);
                        ImGuiAPI.OpenPopupOnItemClick("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                        //var pos = ImGuiAPI.GetItemRectMin();
                        //var size = ImGuiAPI.GetItemRectSize();
                        if (ImGuiAPI.ArrowButton("##OpenAddItemList", ImGuiDir_.ImGuiDir_Down))
                        {
                            ImGuiAPI.OpenPopup("AddItem", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                        }
                        if (ImGuiAPI.BeginPopup("AddItem", ImGuiWindowFlags_.ImGuiWindowFlags_NoMove))
                        {
                            var dict = obj as System.Collections.IList;
                            if (dict.GetType().GenericTypeArguments.Length == 1)
                            {
                                var listElementType = dict.GetType().GenericTypeArguments[0];
                                var typeSlt = new EGui.Controls.TypeSelector();
                                typeSlt.BaseType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(listElementType.FullName);
                                typeSlt.OnDraw(150, 6);
                                if (typeSlt.SelectedType != null)
                                {
                                    foreach (var j in this.TargetObjects)
                                    {
                                        AddList(j, callstack, i, target, dict.Count, System.Activator.CreateInstance(listElementType));
                                    }
                                }
                            }
                            ImGuiAPI.EndPopup();
                        }
                        ImGuiAPI.PopID();
                    }
                    ImGuiAPI.SameLine(0, -1);
                    var showChild = ImGuiAPI.TreeNode(i.Name, "");
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.Text(i.ToString());
                    ImGuiAPI.NextColumn();
                    if (showChild)
                    {
                        var dict = obj as System.Collections.IList;
                        OnList(target, callstack, i, dict);
                        ImGuiAPI.TreePop();
                    }
                }
                else if (obj is System.Collections.IDictionary)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    if (this.IsReadOnly == false)
                    {
                        ImGuiAPI.SameLine(0, -1);
                        var sz = new Vector2(0, 0);
                        ImGuiAPI.PushID(i.Name);
                        if (ImGuiAPI.ArrowButton("##OpenAddItemDict", ImGuiDir_.ImGuiDir_Down))
                        {
                            ImGuiAPI.PopID();
                            ImGuiAPI.OpenPopup("AddDictElement", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                        }
                        else
                        {
                            ImGuiAPI.PopID();
                        }
                    }

                    {
                        Rtti.UTypeDesc keyType = null, valueType = null;
                        var dict = obj as System.Collections.IDictionary;
                        if (dict.GetType().GenericTypeArguments.Length == 2)
                        {
                            keyType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[0].FullName);
                            valueType = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(dict.GetType().GenericTypeArguments[1].FullName);
                        }
                        if (mKVCreator == null)
                        {
                            mKVCreator = new KeyValueCreator();
                        }
                        if (mKVCreator.CtrlName != i.Name)
                        {
                            mKVCreator.CtrlName = i.Name;
                            mKVCreator.KeyTypeSlt.BaseType = keyType;
                            mKVCreator.ValueTypeSlt.BaseType = valueType;
                        }
                        
                        var size = new Vector2(300, 500);
                        ImGuiAPI.SetNextWindowSize(ref size, ImGuiCond_.ImGuiCond_None);
                        mKVCreator.CreateFinished = false;
                        mKVCreator.OnDraw("AddDictElement");
                        if (mKVCreator.CreateFinished)
                        {
                            foreach (var j in this.TargetObjects)
                            {
                                SetDictionaryValue(j, callstack, i, target, mKVCreator.KeyData, mKVCreator.ValueData);
                            }
                        }
                    }

                    ImGuiAPI.SameLine(0, -1);                    
                    var showChild = ImGuiAPI.TreeNode(i.Name, "");
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.Text(i.ToString());
                    ImGuiAPI.NextColumn();
                    if (showChild)
                    {
                        var dict = obj as System.Collections.IDictionary;
                        OnDictionary(target, callstack, i, dict);
                        ImGuiAPI.TreePop();
                    }
                }
                else if ((editorOnDraw = PGTypeEditorManager.Instance.GetEditorType(showType)) != null)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    try
                    {
                        editorOnDraw.OnDraw(i, target, obj, this, callstack);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    ImGuiAPI.NextColumn();
                }
                else// if (showType.IsValueType == false)
                {
                    attrs = i.PropertyType.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                    }
                    if (editorOnDraw != null)
                    {
                        if (editorOnDraw.IsFullRedraw)
                        {
                            try
                            {
                                editorOnDraw.OnDraw(i, target, obj, this, callstack);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                        else
                        {
                            ImGuiAPI.AlignTextToFramePadding();
                            ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                            ImGuiAPI.NextColumn();
                            try
                            {
                                editorOnDraw.OnDraw(i, target, obj, this, callstack);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            ImGuiAPI.NextColumn();
                        }
                    }
                    else
                    {
                        ImGuiAPI.AlignTextToFramePadding();
                        ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                        ImGuiAPI.SameLine(0, -1);
                        var showChild = ImGuiAPI.TreeNode(i.Name, "");
                        ImGuiAPI.NextColumn();
                        if (showType.IsValueType == false)
                        {
                            var sz = new Vector2(0, 0);
                            if (isReadOnly == false && ImGuiAPI.Button("Remove:", ref sz))
                            {
                                foreach (var j in this.TargetObjects)
                                {
                                    SetValue(this, j, callstack, i, target, null);
                                }
                            }
                            ImGuiAPI.SameLine(0, -1);
                        }
                        ImGuiAPI.Text(i.ToString());
                        ImGuiAPI.NextColumn();
                        if (showChild)
                        {
                            callstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(target, i));
                            OnDraw(obj, callstack);

                            ImGuiAPI.TreePop();
                        }
                    }
                }


                var drawList = ImGuiAPI.GetWindowDrawList();
                var cursorPos = ImGuiAPI.GetCursorScreenPos();
                var columnWidth = ImGuiAPI.GetColumnWidth(ImGuiAPI.GetColumnIndex());
                var endPos = cursorPos + new Vector2(columnWidth, 0);
                drawList.AddLine(ref cursorPos, ref endPos, EGui.UIProxy.StyleConfig.SeparatorColor, 1);
                ImGuiAPI.NextColumn();
                cursorPos = ImGuiAPI.GetCursorScreenPos();
                columnWidth = ImGuiAPI.GetColumnWidth(ImGuiAPI.GetColumnIndex());
                endPos = cursorPos + new Vector2(columnWidth, 0);
                drawList.AddLine(ref cursorPos, ref endPos, EGui.UIProxy.StyleConfig.SeparatorColor, 1);
                ImGuiAPI.NextColumn();
            }
            callstack.RemoveAt(callstack.Count - 1);
        }
        KeyValueCreator mKVCreator = null;
        private unsafe void OnArray(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, System.Array lst)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            List<KeyValuePair<int, object>> changedList = new List<KeyValuePair<int, object>>();
            var sz = new Vector2(0, 0);
            for (int i = 0; i < lst.Length; i++)
            {
                var name = i.ToString();
                var obj = lst.GetValue(i);

                ImGuiAPI.AlignTextToFramePadding();                
                ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.NextColumn();
                ImGuiAPI.SetNextItemWidth(-1);

                if (obj == null)
                {
                    ImGuiAPI.Text("null");
                    ImGuiAPI.NextColumn();
                }
                else
                {
                    var vtype = obj.GetType();
                    if (vtype == typeof(bool))
                    {
                        var v = (System.Convert.ToBoolean(obj));
                        var saved = v;
                        ImGuiAPI.Checkbox(TName.FromString2("##Value", name).ToString(), ref v);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(int) || vtype == typeof(uint) ||
                        vtype == typeof(sbyte) || vtype == typeof(byte) ||
                        vtype == typeof(short) || vtype == typeof(ushort) ||
                        vtype == typeof(long) || vtype == typeof(ulong))
                    {
                        var v = System.Convert.ToInt32(obj);
                        var saved = v;
                        ImGuiAPI.InputInt(TName.FromString2("##Value", name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, EngineNS.Support.TConvert.ToObject(vtype, v.ToString())));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(float))
                    {
                        var v = System.Convert.ToSingle(obj);
                        var saved = v;
                        ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                        ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        ImGuiAPI.PopStyleVar(1);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(double))
                    {
                        var v = System.Convert.ToDouble(obj);
                        var saved = v;
                        ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(string))
                    {
                        var v = obj as string;
                        var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
                        fixed (byte* pBuffer = &TextBuffer[0])
                        {
                            CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
                            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                            ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                            ImGuiAPI.PopStyleVar(1);
                            if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                            {
                                v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                                changedList.Add(new KeyValuePair<int, object>(i, v));
                            }
                            System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype.IsEnum)
                    {
                        var attrs1 = prop.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
                        if (attrs1 != null && attrs1.Length > 0)
                        {
                            if (ImGuiAPI.TreeNode("Flags"))
                            {
                                var members = vtype.GetEnumNames();
                                var values = vtype.GetEnumValues();
                                sz = new Vector2(0, 0);
                                uint newFlags = 0;
                                var EnumFlags = System.Convert.ToUInt32(obj);
                                for (int j = 0; j < members.Length; j++)
                                {
                                    var m = values.GetValue(j).ToString();
                                    var e_v = System.Enum.Parse(prop.PropertyType, m);
                                    var v = System.Convert.ToUInt32(e_v);
                                    var bSelected = ((EnumFlags & v) == 0) ? false : true;
                                    ImGuiAPI.Checkbox(members[j], ref bSelected);
                                    if (bSelected)
                                    {
                                        newFlags |= v;
                                    }
                                    else
                                    {
                                        newFlags &= ~v;
                                    }
                                }
                                if (newFlags != EnumFlags)
                                {
                                    changedList.Add(new KeyValuePair<int, object>(i, newFlags));
                                }
                                ImGuiAPI.TreePop();
                            }
                        }
                        else
                        {
                            if (ImGuiAPI.BeginCombo(TName.FromString2("##Enum", name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
                            {
                                int item_current_idx = -1;
                                var members = vtype.GetEnumNames();
                                var values = vtype.GetEnumValues();
                                sz = new Vector2(0, 0);
                                for (int j = 0; j < members.Length; j++)
                                {
                                    var bSelected = true;
                                    if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
                                    {
                                        item_current_idx = j;
                                    }
                                }
                                if (item_current_idx >= 0)
                                {
                                    var v = (int)values.GetValue(item_current_idx);
                                    changedList.Add(new KeyValuePair<int, object>(i, v));
                                }

                                ImGuiAPI.EndCombo();
                            }
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else
                    {
                        if (ImGuiAPI.TreeNode($"{prop.Name}_{name}", obj.ToString()))
                        {
                            ImGuiAPI.NextColumn();
                            var keyPG = new PropertyGrid();
                            keyPG.IsReadOnly = this.IsReadOnly;
                            keyPG.SingleTarget = obj;
                            keyPG.OnDraw(false, false, true);
                            ImGuiAPI.TreePop();
                        }
                        else
                        {
                            ImGuiAPI.NextColumn();
                        }
                    }
                }
            }

            foreach (var i in changedList)
            {
                foreach (var j in this.TargetObjects)
                {
                    SetListValue(j, callstack, prop, target, i.Key, i.Value);
                }
            }
        }
        private unsafe void OnList(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, System.Collections.IList lst)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            List<KeyValuePair<int, object>> changedList = new List<KeyValuePair<int, object>>();
            List<int> removeList = new List<int>();
            List<KeyValuePair<int, object>> addList = new List<KeyValuePair<int, object>>();
            var sz = new Vector2(0, 0);
            for (int i = 0; i < lst.Count; i++)
            {
                var name = i.ToString();
                var obj = lst[i];

                ImGuiAPI.AlignTextToFramePadding();                
                if (IsReadOnly == false)
                {
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("-", ref sz))
                    {
                        removeList.Add(i);
                    }
                    ImGuiAPI.PopID();

                    ImGuiAPI.SameLine(0, -1);
                    ImGuiAPI.PushID(TName.FromString2("##ListAdd_", i.ToString()).ToString());
                    if (ImGuiAPI.Button("+", ref sz))
                    {
                        addList.Add(new KeyValuePair<int, object>(i, obj));
                    }
                    ImGuiAPI.PopID();
                    ImGuiAPI.SameLine(0, -1);
                }                
                ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.NextColumn();
                ImGuiAPI.SetNextItemWidth(-1);

                if(obj==null)
                {
                    ImGuiAPI.Text("null");
                    ImGuiAPI.NextColumn();
                }
                else
                {
                    var vtype = obj.GetType();
                    if (vtype == typeof(bool))
                    {
                        var v = System.Convert.ToBoolean(obj);
                        var saved = v;
                        ImGuiAPI.Checkbox(TName.FromString2("##Value", name).ToString(), ref v);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(int) || vtype == typeof(uint) || 
                        vtype == typeof(sbyte) || vtype == typeof(byte) || 
                        vtype == typeof(short) || vtype == typeof(ushort) ||
                        vtype == typeof(long) || vtype == typeof(ulong))
                    {
                        var v = System.Convert.ToInt32(obj);
                        var saved = v;
                        ImGuiAPI.InputInt(TName.FromString2("##Value", name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, EngineNS.Support.TConvert.ToObject(vtype, v.ToString())));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(float))
                    {
                        var v = System.Convert.ToSingle(obj);
                        var saved = v;
                        ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                        ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        ImGuiAPI.PopStyleVar(1);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(double))
                    {
                        var v = System.Convert.ToDouble(obj);
                        var saved = v;
                        ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                        if (v != saved)
                        {
                            changedList.Add(new KeyValuePair<int, object>(i, v));
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype == typeof(string))
                    {
                        var v = obj as string;
                        var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
                        fixed (byte* pBuffer = &TextBuffer[0])
                        {
                            CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
                            ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                            ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                            ImGuiAPI.PopStyleVar(1);
                            if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                            {
                                v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                                changedList.Add(new KeyValuePair<int, object>(i, v));
                            }
                            System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else if (vtype.IsEnum)
                    {
                        var attrs1 = prop.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
                        if (attrs1 != null && attrs1.Length > 0)
                        {
                            if (ImGuiAPI.TreeNode("Flags"))
                            {
                                var members = vtype.GetEnumNames();
                                var values = vtype.GetEnumValues();
                                sz = new Vector2(0, 0);
                                uint newFlags = 0;
                                var EnumFlags = System.Convert.ToUInt32(obj);
                                for (int j = 0; j < members.Length; j++)
                                {
                                    var m = values.GetValue(j).ToString();
                                    var e_v = System.Enum.Parse(prop.PropertyType, m);
                                    var v = System.Convert.ToUInt32(e_v);
                                    var bSelected = ((EnumFlags & v) == 0) ? false : true;
                                    ImGuiAPI.Checkbox(members[j], ref bSelected);
                                    if (bSelected)
                                    {
                                        newFlags |= v;
                                    }
                                    else
                                    {
                                        newFlags &= ~v;
                                    }
                                }
                                if (newFlags != EnumFlags)
                                {
                                    changedList.Add(new KeyValuePair<int, object>(i, newFlags));
                                }
                                ImGuiAPI.TreePop();
                            }
                        }
                        else
                        {
                            if (ImGuiAPI.BeginCombo(TName.FromString2("##Enum", name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
                            {
                                int item_current_idx = -1;
                                var members = vtype.GetEnumNames();
                                var values = vtype.GetEnumValues();
                                sz = new Vector2(0, 0);
                                for (int j = 0; j < members.Length; j++)
                                {
                                    var bSelected = true;
                                    if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
                                    {
                                        item_current_idx = j;
                                    }
                                }
                                if (item_current_idx >= 0)
                                {
                                    var v = (int)values.GetValue(item_current_idx);
                                    changedList.Add(new KeyValuePair<int, object>(i, v));
                                }

                                ImGuiAPI.EndCombo();
                            }
                        }
                        ImGuiAPI.NextColumn();
                    }
                    else
                    {
                        if (ImGuiAPI.TreeNode($"{prop.Name}_{name}", obj.ToString()))
                        {
                            ImGuiAPI.NextColumn();
                            var keyPG = new PropertyGrid();
                            keyPG.IsReadOnly = this.IsReadOnly;
                            keyPG.SingleTarget = obj;
                            keyPG.OnDraw(false, false, true);
                            ImGuiAPI.TreePop();
                        }
                        else
                        {
                            ImGuiAPI.NextColumn();
                        }
                    }
                }
            }

            foreach (var i in changedList)
            {
                foreach (var j in this.TargetObjects)
                {
                    SetListValue(j, callstack, prop, target, i.Key, i.Value);
                }
            }
            foreach (var i in removeList)
            {
                foreach (var j in this.TargetObjects)
                {
                    RemoveList(j, callstack, prop, target, i);
                }
            }
            foreach (var i in addList)
            {
                foreach (var j in this.TargetObjects)
                {
                    AddList(j, callstack, prop, target, i.Key, i.Value);
                }
            }
        }
        private unsafe void OnDictionary(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, System.Collections.IDictionary dict)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            List<KeyValuePair<object, object>> changedList = new List<KeyValuePair<object, object>>();
            List<object> removeList = new List<object>();
            var sz = new Vector2(0, 0);
            var iter = dict.GetEnumerator();
            while (iter.MoveNext())
            {
                var name = iter.Key.ToString();
                ImGuiAPI.AlignTextToFramePadding();
                if (IsReadOnly == false)
                {
                    ImGuiAPI.PushID(TName.FromString2("##ListDel_", name).ToString());
                    if (ImGuiAPI.Button("-", ref sz))
                    {
                        removeList.Add(iter.Key);
                    }
                    ImGuiAPI.PopID();
                    ImGuiAPI.SameLine(0, -1);
                }
                ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.NextColumn();
                ImGuiAPI.SetNextItemWidth(-1);
                var vtype = iter.Value.GetType();
                var obj = iter.Value;
                if (vtype == typeof(bool))
                {
                    var v = (System.Convert.ToBoolean(obj));
                    var saved = v;
                    ImGuiAPI.Checkbox(TName.FromString2("##Value", name).ToString(), ref v);
                    if (v != saved)
                    {
                        changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (vtype == typeof(int) || vtype == typeof(uint) ||
                        vtype == typeof(sbyte) || vtype == typeof(byte) ||
                        vtype == typeof(short) || vtype == typeof(ushort) ||
                        vtype == typeof(long) || vtype == typeof(ulong))
                {
                    var v = System.Convert.ToInt32(obj);
                    var saved = v;
                    ImGuiAPI.InputInt(TName.FromString2("##Value", name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        changedList.Add(new KeyValuePair<object, object>(iter.Key, EngineNS.Support.TConvert.ToObject(vtype, v.ToString())));
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (vtype == typeof(float))
                {
                    var v = System.Convert.ToSingle(obj);
                    var saved = v;
                    ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (vtype == typeof(double))
                {
                    var v = System.Convert.ToDouble(obj);
                    var saved = v;
                    ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.6f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (vtype == typeof(string))
                {
                    var v = obj as string;
                    var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
                    if (CoreSDK.SDK_StrLen(strPtr.ToPointer()) >= (uint)TextBuffer.Length)
                    {
                        TextBuffer = new byte[(int)CoreSDK.SDK_StrLen(strPtr.ToPointer()) + 512];
                    }
                    fixed (byte* pBuffer = &TextBuffer[0])
                    {
                        CoreSDK.SDK_StrCpy(pBuffer, strPtr.ToPointer(), (uint)TextBuffer.Length);
                        ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref EGui.UIProxy.StyleConfig.PGInputFramePadding);
                        ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        ImGuiAPI.PopStyleVar(1);
                        if (CoreSDK.SDK_StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                        {
                            v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                            changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
                        }
                        System.Runtime.InteropServices.Marshal.FreeHGlobal(strPtr);
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (vtype.IsEnum)
                {
                    var attrs1 = prop.PropertyType.GetCustomAttributes(typeof(System.FlagsAttribute), false);
                    if (attrs1 != null && attrs1.Length > 0)
                    {
                        if (ImGuiAPI.TreeNode("Flags"))
                        {
                            var members = vtype.GetEnumNames();
                            var values = vtype.GetEnumValues();
                            sz = new Vector2(0, 0);
                            uint newFlags = 0;
                            var EnumFlags = System.Convert.ToUInt32(obj);
                            for (int j = 0; j < members.Length; j++)
                            {
                                var m = values.GetValue(j).ToString();
                                var e_v = System.Enum.Parse(prop.PropertyType, m);
                                var v = System.Convert.ToUInt32(e_v);
                                var bSelected = (((EnumFlags & v) == 0) ? false : true);
                                ImGuiAPI.Checkbox(members[j], ref bSelected);
                                if (bSelected)
                                {
                                    newFlags |= v;
                                }
                                else
                                {
                                    newFlags &= ~v;
                                }
                            }
                            if (newFlags != EnumFlags)
                            {
                                changedList.Add(new KeyValuePair<object, object>(iter.Key, newFlags));
                            }
                            ImGuiAPI.TreePop();
                        }
                    }
                    else
                    {
                        if (ImGuiAPI.BeginCombo(TName.FromString2("##Enum_", name).ToString(), obj.ToString(), ImGuiComboFlags_.ImGuiComboFlags_None))
                        {
                            int item_current_idx = -1;
                            var members = vtype.GetEnumNames();
                            var values = vtype.GetEnumValues();
                            sz = new Vector2(0, 0);
                            for (int j = 0; j < members.Length; j++)
                            {
                                var bSelected = true;
                                if (ImGuiAPI.Selectable(members[j], ref bSelected, ImGuiSelectableFlags_.ImGuiSelectableFlags_None, ref sz))
                                {
                                    item_current_idx = j;
                                }
                            }
                            if (item_current_idx >= 0)
                            {
                                var v = (int)values.GetValue(item_current_idx);
                                changedList.Add(new KeyValuePair<object, object>(iter.Key, v));
                            }

                            ImGuiAPI.EndCombo();
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else
                {
                    if (ImGuiAPI.TreeNode($"{prop.Name}_{name}", obj.ToString()))
                    {
                        ImGuiAPI.NextColumn();
                        var keyPG = new PropertyGrid();
                        keyPG.SingleTarget = obj;
                        keyPG.OnDraw(false, false, true);
                        ImGuiAPI.TreePop();
                    }
                    else
                    {
                        ImGuiAPI.NextColumn();
                    }
                }
            }

            foreach(var i in changedList)
            {
                foreach (var j in this.TargetObjects)
                {
                    SetDictionaryValue(j, callstack, prop, target, i.Key, i.Value);
                }
            }
            foreach (var i in removeList)
            {
                foreach (var j in this.TargetObjects)
                {
                    RemoveDictionary(j, callstack, prop, target, i);
                }
            }
        }
        private static void RemoveList(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index)
        {
            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i = 1; i < callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            var dict = prop.GetValue(owner, null) as System.Collections.IList;
            dict.RemoveAt(index);
        }
        private static void AddList(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index, object value)
        {
            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i = 1; i < callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            var dict = prop.GetValue(owner, null) as System.Collections.IList;
            dict.Insert(index, value);
        }
        private static void SetListValue(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, int index, object value)
        {
            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i = 1; i < callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            var dict = prop.GetValue(owner, null) as System.Collections.IList;
            dict[index] = value;
        }
        public static void RemoveDictionary(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object key)
        {
            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i = 1; i < callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            var dict = prop.GetValue(owner, null) as System.Collections.IDictionary;
            dict.Remove(key);
        }
        public static void SetDictionaryValue(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object key, object value)
        {
            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i = 1; i < callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            var dict = prop.GetValue(owner, null) as System.Collections.IDictionary;
            try
            {
                dict[key] = value;
            }
            catch
            {

            }
        }
        public static void SetValue(PropertyGrid pg, object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object value)
        {
            if (pg.IsReadOnly)
                return;

            PGCustomValueEditorAttribute editorOnDraw = null;
            var attrs = prop.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                if (editorOnDraw.HideInPG)
                    return;
            }
            bool isReadOnly = (editorOnDraw != null && editorOnDraw.ReadOnly);
            if (isReadOnly)
                return;

            List<object> ownerChain = new List<object>();
            ownerChain.Add(null);
            ownerChain.Add(root);
            for (int i=1; i<callstack.Count; i++)
            {
                root = callstack[i].Value.GetValue(root);
                if (root == null)
                    return;
                ownerChain.Add(root);
            }
            if (prop.CanWrite && prop.SetMethod.IsPublic)
            {   
                prop.SetValue(owner, value);
            }
            int curStack = callstack.Count - 1;
            while (owner.GetType().IsValueType && curStack>=0)
            {
                value = owner;
                prop = callstack[curStack].Value;
                if (prop == null)
                    break;
                owner = ownerChain[curStack];
                if (prop.CanWrite)
                {
                    prop.SetValue(owner, value);
                }
                curStack--;
            }
        }
    }
}
