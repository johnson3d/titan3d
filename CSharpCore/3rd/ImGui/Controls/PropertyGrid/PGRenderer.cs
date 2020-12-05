using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace CSharpCode.Controls.PropertyGrid
{
    partial class PropertyGrid
    {
        private CppBool mVisible = CppBool.FromBoolean(true);
        public bool Visible
        {
            get
            {
                return mVisible;
            }
            set
            {
                mVisible = CppBool.FromBoolean(value);
            }
        }
        public string PGName
        {
            get;
            set;
        } = "Property Grid";
        byte[] TextBuffer = new byte[1024 * 4];
        List<KeyValuePair<object, System.Reflection.PropertyInfo>> mCallstack = new List<KeyValuePair<object, System.Reflection.PropertyInfo>>();
        public void OnDraw(bool bNewForm = true, bool bKeepColums = false)
        {
            if (Visible == false)
                return;
            mCallstack.Clear();
            mCallstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(null, null));

            unsafe
            {
                fixed (CppBool* pVisible = &mVisible)
                {
                    if(bNewForm)
                    {
                        if (ImGuiAPI.Begin($"{PGName}", pVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                        {
                            OnDrawContent(bKeepColums);
                        }
                        ImGuiAPI.End();
                    }
                    else
                    {
                        OnDrawContent(bKeepColums);
                    }
                }   
            }
        }
        private void OnDrawContent(bool bKeepColums = false)
        {
            if (bKeepColums == false)
            {
                ImGuiAPI.Text($"{SingleTarget?.ToString()}");
                var styleVar = new Vector2(2, 2);
                ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref styleVar);
                ImGuiAPI.Columns(2, null, CppBool.FromBoolean(true));
                ImGuiAPI.Separator();

                OnDraw(this.SingleTarget, mCallstack);

                ImGuiAPI.Columns(1, null, CppBool.FromBoolean(true));
                ImGuiAPI.Separator();
                ImGuiAPI.PopStyleVar(1);
            }
            else
            {
                OnDraw(this.SingleTarget, mCallstack);
            }
        }
        class PopModalDictKey
        {
            public object KeyData { get; set; } = null;
            internal object ViewDict = null;
        }
        PopModalDictKey PopModalDictKeyData = new PopModalDictKey();
        private unsafe void OnDraw(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            if (target == null)
                return;
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            var props = target.GetType().GetProperties();
            foreach (var i in props)
            {
                object obj;
                try
                {
                    obj = i.GetValue(target);
                }
                catch
                {
                    continue;
                }
                System.Type showType = i.PropertyType;
                if (obj == null && showType != typeof(string))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var sz = new Vector2(0, 0);
                    if(ImGuiAPI.Button("New", ref sz))
                    {
                        var v = System.Activator.CreateInstance(showType);
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, v);
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

                PGCustomValueEditorAttribute editorOnDraw = null;
                var attrs = i.GetCustomAttributes(typeof(PGCustomValueEditorAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    editorOnDraw = attrs[0] as PGCustomValueEditorAttribute;
                }
                if(editorOnDraw!=null)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    try
                    {
                        editorOnDraw.OnDraw(i, target, obj, this, callstack);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(bool))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = CppBool.FromBoolean(System.Convert.ToBoolean(obj));
                    var saved = v;
                    ImGuiAPI.Checkbox(TName.FromString2("##", i.Name).ToString(), ref v);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, (bool)v);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(int) || showType == typeof(uint) ||
                        showType == typeof(sbyte) || showType == typeof(byte) ||
                        showType == typeof(short) || showType == typeof(ushort) ||
                        showType == typeof(long) || showType == typeof(ulong))
                {
                    ImGuiAPI.AlignTextToFramePadding();                    
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = System.Convert.ToInt32(obj);
                    var saved = v;
                    ImGuiAPI.InputInt(TName.FromString2("##", i.Name).ToString(), ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if(v!=saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, EngineNS.Support.TConvert.ToObject(showType, v.ToString()));
                        }
                    }
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
                    ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref v, 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, v);
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
                    ImGuiAPI.InputFloat(TName.FromString2("##", i.Name).ToString(), ref (*(float*)&v), 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, v);
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
                        CoreSDK.StrCpy(pBuffer, strPtr.ToPointer());
                        ImGuiAPI.InputText(TName.FromString2("##", i.Name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        if (CoreSDK.StrCmp(pBuffer, strPtr.ToPointer()) != 0)
                        {
                            v = System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)pBuffer);
                            foreach (var j in this.TargetObjects)
                            {
                                SetValue(j, callstack, i, target, v);
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
                                var bSelected = CppBool.FromBoolean(((EnumFlags & v) == 0) ? false : true);
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
                                    SetValue(j, callstack, i, target, System.Enum.ToObject(i.PropertyType, newFlags));
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
                                var bSelected = CppBool.FromBoolean(true);
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
                                    SetValue(j, callstack, i, target, v);
                                }
                            }

                            ImGuiAPI.EndCombo();
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (obj is System.Collections.IList)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    ImGuiAPI.SameLine(0, -1);
                    var sz = new Vector2(0, 0);
                    ImGuiAPI.PushID(i.Name);
                    if (ImGuiAPI.Button("+", ref sz))
                    {
                        var dict = obj as System.Collections.IList;
                        if (dict.GetType().GenericTypeArguments.Length == 1)
                        {
                            var listElementType = dict.GetType().GenericTypeArguments[0];
                            foreach (var j in this.TargetObjects)
                            {
                                AddList(j, callstack, i, target, dict.Count, System.Activator.CreateInstance(listElementType));
                            }
                        }
                    }
                    ImGuiAPI.PopID();
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
                    ImGuiAPI.SameLine(0, -1);
                    var sz = new Vector2(0, 0);
                    ImGuiAPI.PushID(i.Name);
                    if (ImGuiAPI.Button("+", ref sz))
                    {
                        ImGuiAPI.PopID();
                        ImGuiAPI.OpenPopup("AddDictElement", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                        PopModalDictKeyData.ViewDict = obj;
                    }
                    else
                    {
                        ImGuiAPI.PopID();
                    }

                    {
                        var center = ImGuiAPI.GetMainViewport()->GetCenter();
                        var pivot = new Vector2(0.5f, 0.5f);
                        ImGuiAPI.SetNextWindowPos(ref center, ImGuiCond_.ImGuiCond_Appearing, ref pivot);
                        if (PopModalDictKeyData.ViewDict == obj && ImGuiAPI.BeginPopupModal("AddDictElement", (CppBool*)0, ImGuiWindowFlags_.ImGuiWindowFlags_AlwaysAutoResize))
                        {
                            System.Type keyType = null,valueType = null;
                            var dict = obj as System.Collections.IDictionary;
                            if (dict.GetType().GenericTypeArguments.Length == 2)
                            {
                                keyType = dict.GetType().GenericTypeArguments[0];
                                valueType = dict.GetType().GenericTypeArguments[1];
                            }
                            if (PopModalDictKeyData.KeyData == null || PopModalDictKeyData.KeyData.GetType() != keyType)
                                PopModalDictKeyData.KeyData = System.Activator.CreateInstance(keyType);
                            var keyPG = new PropertyGrid();
                            keyPG.SingleTarget = PopModalDictKeyData;
                            keyPG.OnDraw(false);
                            ImGuiAPI.Separator();
                            sz = new Vector2(120, 0);
                            if (ImGuiAPI.Button("OK", ref sz))
                            {
                                foreach (var j in this.TargetObjects)
                                {
                                    SetDictionaryValue(j, callstack, i, target, PopModalDictKeyData.KeyData, System.Activator.CreateInstance(valueType));
                                }
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.SetItemDefaultFocus();
                            ImGuiAPI.SameLine(0, -1);
                            if (ImGuiAPI.Button("Cancel", ref sz))
                            {
                                ImGuiAPI.CloseCurrentPopup();
                            }
                            ImGuiAPI.EndPopup();
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
                            if (ImGuiAPI.Button("Remove:", ref sz))
                            {
                                foreach (var j in this.TargetObjects)
                                {
                                    SetValue(j, callstack, i, target, null);
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
            }
            callstack.RemoveAt(callstack.Count - 1);
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
                        var v = CppBool.FromBoolean(System.Convert.ToBoolean(obj));
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
                        ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
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
                        ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
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
                            CoreSDK.StrCpy(pBuffer, strPtr.ToPointer());
                            ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                            if (CoreSDK.StrCmp(pBuffer, strPtr.ToPointer()) != 0)
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
                                    var bSelected = CppBool.FromBoolean(((EnumFlags & v) == 0) ? false : true);
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
                                    var bSelected = CppBool.FromBoolean(true);
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
                            keyPG.SingleTarget = obj;
                            keyPG.OnDraw(false, true);
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
                ImGuiAPI.PushID(TName.FromString2("##ListDel_", name).ToString());
                if (ImGuiAPI.Button("-", ref sz))
                {
                    removeList.Add(iter.Key);
                }
                ImGuiAPI.PopID();
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.TreeNodeEx(name, flags, name);
                ImGuiAPI.NextColumn();
                ImGuiAPI.SetNextItemWidth(-1);
                var vtype = iter.Value.GetType();
                var obj = iter.Value;
                if (vtype == typeof(bool))
                {
                    var v = CppBool.FromBoolean(System.Convert.ToBoolean(obj));
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
                    ImGuiAPI.InputFloat(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
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
                    ImGuiAPI.InputDouble(TName.FromString2("##Value", name).ToString(), ref v, 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
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
                    fixed (byte* pBuffer = &TextBuffer[0])
                    {
                        CoreSDK.StrCpy(pBuffer, strPtr.ToPointer());
                        ImGuiAPI.InputText(TName.FromString2("##Value", name).ToString(), pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        if (CoreSDK.StrCmp(pBuffer, strPtr.ToPointer()) != 0)
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
                                var bSelected = CppBool.FromBoolean(((EnumFlags & v) == 0) ? false : true);
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
                                var bSelected = CppBool.FromBoolean(true);
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
                        keyPG.OnDraw(false, true);
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
        public static void SetValue(object root, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack, System.Reflection.PropertyInfo prop, object owner, object value)
        {
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
            prop.SetValue(owner, value);
            int curStack = callstack.Count - 1;
            while (owner.GetType().IsValueType && curStack>=0)
            {
                value = owner;
                prop = callstack[curStack].Value;
                owner = ownerChain[curStack];
                prop.SetValue(owner, value);
                curStack--;
            }
        }
    }
}
