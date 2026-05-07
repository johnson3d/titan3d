//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 72, Pack = 8)]
public unsafe partial struct ImGuiInputTextCallbackData : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(8)]
	public ImGuiInputTextFlags_ m_EventFlag;
	[System.Runtime.InteropServices.FieldOffset(12)]
	public ImGuiInputTextFlags_ m_Flags;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public void* m_UserData;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public uint m_ID;
	[System.Runtime.InteropServices.FieldOffset(28)]
	public ImGuiKey m_EventKey;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public Wchar16 m_EventChar;
	[System.Runtime.InteropServices.FieldOffset(34)]
	public bool m_EventActivated;
	[System.Runtime.InteropServices.FieldOffset(35)]
	public bool m_BufDirty;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public sbyte* m_Buf;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public int m_BufTextLen;
	[System.Runtime.InteropServices.FieldOffset(52)]
	public int m_BufSize;
	[System.Runtime.InteropServices.FieldOffset(56)]
	public int m_CursorPos;
	[System.Runtime.InteropServices.FieldOffset(60)]
	public int m_SelectionStart;
	[System.Runtime.InteropServices.FieldOffset(64)]
	public int m_SelectionEnd;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public ImGuiInputTextFlags_ EventFlag
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventFlag(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventFlag(mPointer, value);
			}
		}
	}
	public ImGuiInputTextFlags_ Flags
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Flags(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Flags(mPointer, value);
			}
		}
	}
	public void* UserData
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__UserData(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__UserData(mPointer, value);
			}
		}
	}
	public uint ID
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__ID(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__ID(mPointer, value);
			}
		}
	}
	public ImGuiKey EventKey
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventKey(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventKey(mPointer, value);
			}
		}
	}
	public Wchar16 EventChar
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventChar(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventChar(mPointer, value);
			}
		}
	}
	public bool EventActivated
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventActivated(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventActivated(mPointer, value);
			}
		}
	}
	public bool BufDirty
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufDirty(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufDirty(mPointer, value);
			}
		}
	}
	public string Buf
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Buf(mPointer));
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Buf(mPointer, value);
			}
		}
	}
	public int BufTextLen
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufTextLen(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufTextLen(mPointer, value);
			}
		}
	}
	public int BufSize
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufSize(mPointer, value);
			}
		}
	}
	public int CursorPos
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__CursorPos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__CursorPos(mPointer, value);
			}
		}
	}
	public int SelectionStart
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionStart(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionStart(mPointer, value);
			}
		}
	}
	public int SelectionEnd
	{
		get
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionEnd(mPointer);
			}
		}
		set
		{
			fixed (ImGuiInputTextCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionEnd(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public void DeleteChars(int pos,int bytes_count)
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_DeleteChars_3539386109(mPointer, pos, bytes_count);
		}
	}
	public void InsertChars(int pos,string text,string text_end)
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_InsertChars_3984929356(mPointer, pos, text, text_end);
		}
	}
	public void SelectAll()
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_SelectAll_2960189489(mPointer);
		}
	}
	public void SetSelection(int s,int e)
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_SetSelection_3539386109(mPointer, s, e);
		}
	}
	public void ClearSelection()
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiInputTextCallbackData_Visitor_ClearSelection_2960189489(mPointer);
		}
	}
	public bool HasSelection()
	{
		fixed (ImGuiInputTextCallbackData* mPointer = &this)
		{
			return TitanImGui_ImGuiInputTextCallbackData_Visitor_HasSelection_82051314(mPointer) == 0 ? false : true;
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiInputTextFlags_ TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventFlag(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventFlag(void* self, ImGuiInputTextFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiInputTextFlags_ TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Flags(void* self, ImGuiInputTextFlags_ value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__UserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__UserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__ID(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__ID(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiKey TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventKey(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventKey(void* self, ImGuiKey value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventChar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventChar(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventActivated(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventActivated(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufDirty(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufDirty(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Buf(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Buf(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufTextLen(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufTextLen(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufSize(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__CursorPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__CursorPos(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionStart(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionStart(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionEnd(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionEnd(void* self, int value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_DeleteChars_3539386109(void* Self, int pos,int bytes_count);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_InsertChars_3984929356(void* Self, int pos,[MarshalAs(UnmanagedType.LPUTF8Str)] string text,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_SelectAll_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_SetSelection_3539386109(void* Self, int s,int e);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImGuiInputTextCallbackData_Visitor_ClearSelection_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImGuiInputTextCallbackData_Visitor_HasSelection_82051314(void* Self);
	//Cast
	#endregion
}
