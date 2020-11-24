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
        byte[] TextBuffer = new byte[1024 * 4];
        List<KeyValuePair<object, System.Reflection.PropertyInfo>> mCallstack = new List<KeyValuePair<object, System.Reflection.PropertyInfo>>();
        public void OnDraw()
        {
            if (Visible == false)
                return;
            mCallstack.Clear();
            mCallstack.Add(new KeyValuePair<object, System.Reflection.PropertyInfo>(null, null));

            unsafe
            {
                fixed (CppBool* pVisible = &mVisible)
                {
                    if (ImGuiAPI.Begin($"Property:{SingleTarget?.ToString()}", pVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                    {
                        var styleVar = new Vector2(2, 2);
                        ImGuiAPI.PushStyleVar(ImGuiStyleVar_.ImGuiStyleVar_FramePadding, ref styleVar);
                        ImGuiAPI.Columns(2, null, CppBool.FromBoolean(true));
                        ImGuiAPI.Separator();

                        OnDraw(TargetObjects[0], mCallstack);

                        ImGuiAPI.Columns(1, null, CppBool.FromBoolean(true));
                        ImGuiAPI.Separator();
                        ImGuiAPI.PopStyleVar(1);
                    }
                    ImGuiAPI.End();
                }   
            }
        }
        private unsafe void OnDraw(object target, List<KeyValuePair<object, System.Reflection.PropertyInfo>> callstack)
        {
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            var props = target.GetType().GetProperties();
            foreach (var i in props)
            {
                var obj = i.GetValue(target);
                System.Type showType = i.PropertyType;
                if (obj == null && showType != typeof(string))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    ImGuiAPI.Text("null");
                    ImGuiAPI.NextColumn();
                    continue;
                }
                else
                {
                    if (obj != null)
                        showType = obj.GetType();
                }
                if (showType == typeof(int))
                {
                    ImGuiAPI.AlignTextToFramePadding();                    
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = System.Convert.ToInt32(obj);
                    var saved = v;
                    ImGuiAPI.InputIntNoName(i.Name, ref v, 1, 100, ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if(v!=saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, v);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(float))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = new System.Numerics.Vector2();
                    v.X = System.Convert.ToSingle(obj);
                    var saved = v;
                    ImGuiAPI.InputFloatNoName(i.Name, ref (*(float*)&v), 0.1f, 100.0f, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    //ImGuiAPI.InputFloat2(i.Name, ref (*(float*)&v), "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None);
                    if (v != saved)
                    {
                        foreach (var j in this.TargetObjects)
                        {
                            SetValue(j, callstack, i, target, v.X);
                        }
                    }
                    ImGuiAPI.NextColumn();
                }
                else if (showType == typeof(string))
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.NextColumn();
                    ImGuiAPI.SetNextItemWidth(-1);
                    var v = obj as string;
                    var strPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(v);
                    fixed (byte* pBuffer = &TextBuffer[0])
                    {
                        CoreSDK.StrCpy(pBuffer, strPtr.ToPointer());
                        ImGuiAPI.InputTextNoName(i.Name, pBuffer, (uint)TextBuffer.Length, ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
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
                else// if (showType.IsValueType == false)
                {
                    ImGuiAPI.AlignTextToFramePadding();
                    ImGuiAPI.TreeNodeEx(i.Name, flags, i.Name);
                    //ImGuiAPI.Text(i.Name);
                    ImGuiAPI.SameLine(0, -1);
                    var showChild = ImGuiAPI.TreeNode(i.Name, "");
                    ImGuiAPI.NextColumn();
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
            callstack.RemoveAt(callstack.Count - 1);
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
