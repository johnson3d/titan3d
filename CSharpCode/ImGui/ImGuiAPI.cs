using System;
using System.Collections.Generic;
using System.Text;
using EngineNS;

namespace EngineNS
{
    public unsafe partial struct ImGuiAPI
    {
        public static void GotoColumns(int index)
        {
            index = index % GetColumnsCount();
            var cur = GetColumnIndex();
            while (cur != index)
            {
                NextColumn();
                cur = GetColumnIndex();
            }
        }
        //这个文件是手撸代码，做一些marshal
        public static bool Combo(string label, ref int current_item, List<string> items, int items_count, int popup_max_height_in_items)
        {
            var ppStrings = stackalloc SByte*[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                ppStrings[i] = (SByte*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(items[i]).ToPointer();
            }
            var result = Combo(label, ref current_item, ppStrings, items_count, popup_max_height_in_items);
            for (int i = 0; i < items.Count; i++)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)ppStrings[i]);
            }
            return result;
        }
        public static bool PointInRect(ref Vector2 pt, ref Vector2 min, ref Vector2 max)
        {
            if (pt.X > max.X || pt.X < min.X || pt.Y > max.Y || pt.Y < min.Y)
                return false;
            return true;
        }
        public static bool InputText(string label, ref string text, 
            ImGuiInputTextFlags_ flags = ImGuiInputTextFlags_.ImGuiInputTextFlags_None, 
            FDelegate_ImGuiInputTextCallback callback = null, object user_data = null)
        {
            if (text == null)
                text = "";
            using (var buffer = BigStackBuffer.CreateInstance((text.Length + 1) * 2))
            {
                buffer.SetText(text);
                bool changed = false;
                if (user_data == null)
                {
                    changed = InputText(label, buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, (void*)0);
                }
                else
                {
                    // user_data to gchandle
                    System.Runtime.InteropServices.GCHandle handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(IntPtr.Zero);
                    changed = InputText(label, buffer.GetBuffer(), (uint)buffer.GetSize(), flags, callback, IntPtr.Zero.ToPointer());
                }
                if (changed)
                {
                    text = buffer.AsText();
                    return true;
                }
                return false;
            }   
        }
        public static bool Button(string label)
        {
            var sz = new Vector2();
            return Button(label, in sz);
        }
    }
}

public unsafe partial struct ImDrawList
{
    public ImDrawCmd* CmdBufferData
    {
        get
        {
            int size = 0;
            return GetCmdBuffer(&size);
        }
    }
    public int CmdBufferSize
    {
        get
        {
            int size = 0;
            GetCmdBuffer(&size);
            return size;
        }
    }
    public ImDrawVert* VtxBufferData
    {
        get
        {
            int size = 0;
            return GetVtxBuffer(&size);
        }
    }
    public int VtxBufferSize
    {
        get
        {
            int size = 0;
            GetVtxBuffer(&size);
            return size;
        }
    }
    public UInt16* IdxBufferData
    {
        get
        {
            int size = 0;
            return GetIdxBuffer(&size);
        }
    }
    public int IdxBufferSize
    {
        get
        {
            int size = 0;
            GetIdxBuffer(&size);
            return size;
        }
    }
}