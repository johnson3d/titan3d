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
        public static string GetStyleColorNameString(ImGuiCol_ idx)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)GetStyleColorName(idx));
        }
        public static string GetClipboardTextString()
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)GetClipboardText());
        }
        public static string SaveIniSettingsToString(ref UInt32 out_ini_size)
        {
            return System.Runtime.InteropServices.Marshal.PtrToStringAnsi((IntPtr)SaveIniSettingsToMemory(ref out_ini_size));
        }
        public static ImGuiIO_PtrType IO
        {
            get
            {
                var ptr = ImGuiAPI.GetIO().NativePointer;
                return new ImGuiIO_PtrType(ptr);
            }
        }
        public static ImGuiPlatformIO_PtrType PlatformIO
        {
            get
            {
                var ptr = ImGuiAPI.GetPlatformIO().NativePointer;
                return new ImGuiPlatformIO_PtrType(ptr);
            }
        }
        public static bool PointInRect(ref Vector2 pt, ref Vector2 min, ref Vector2 max)
        {
            if (pt.X > max.X || pt.X < min.X || pt.Y > max.Y || pt.Y < min.Y)
                return false;
            return true;
        }
    }
}

public unsafe partial struct ImGuiIO_PtrType
{
    public ImFontAtlas_PtrType FontsWrapper
    {
        get
        {
            return new ImFontAtlas_PtrType(Fonts.NativePointer);
        }
    }
    public int* KeyMap
    {
        get
        {
            return TSDK_ImGuiIO_Getter_KeyMap_ArrayAddress(mPtr);
        }
    }
    public CppBool* MouseDown
    {
        get
        {
            return TSDK_ImGuiIO_Getter_MouseDown_ArrayAddress(mPtr);
        }
    }
    public CppBool* KeysDown
    {
        get
        {
            return TSDK_ImGuiIO_Getter_KeysDown_ArrayAddress(mPtr);
        }
    }
}

public unsafe partial struct ImGuiPlatformIO_PtrType
{
    public Renderer_CreateWindow RCreateWindow
    {
        set
        {
            ImGuiAPI.Set_Renderer_CreateWindow(mPtr, value);
        }
    }
    public Renderer_DestroyWindow RDestroyWindow
    {
        set
        {
            ImGuiAPI.Set_Renderer_DestroyWindow(mPtr, value);
        }
    }
    public Renderer_SetWindowSize RSetWindowSize
    {
        set
        {
            ImGuiAPI.Set_Renderer_SetWindowSize(mPtr, value);
        }
    }
    public Renderer_RenderWindow RRenderWindow
    {
        set
        {
            ImGuiAPI.Set_Renderer_RenderWindow(mPtr, value);
        }
    }
    public Renderer_SwapBuffers RSwapBuffers
    {
        set
        {
            ImGuiAPI.Set_Renderer_SwapBuffers(mPtr, value);
        }
    }
}

public unsafe partial struct ImDrawList_PtrType
{
    public ImDrawCmd* CmdBufferData
    {
        get
        {
            int size = 0;
            return GetCmdBuffer(ref size);
        }
    }
    public int CmdBufferSize
    {
        get
        {
            int size = 0;
            GetCmdBuffer(ref size);
            return size;
        }
    }
    public ImDrawVert* VtxBufferData
    {
        get
        {
            int size = 0;
            return GetVtxBuffer(ref size);
        }
    }
    public int VtxBufferSize
    {
        get
        {
            int size = 0;
            GetVtxBuffer(ref size);
            return size;
        }
    }
    public UInt16* IdxBufferData
    {
        get
        {
            int size = 0;
            return GetIdxBuffer(ref size);
        }
    }
    public int IdxBufferSize
    {
        get
        {
            int size = 0;
            GetIdxBuffer(ref size);
            return size;
        }
    }
}